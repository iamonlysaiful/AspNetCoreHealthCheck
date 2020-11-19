using System.Collections.Generic;
using System.ServiceProcess;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace S3HealthChecks
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var producerConfig = new Confluent.Kafka.ProducerConfig(new Dictionary<string, string>() { { "bootstrap.servers", "localhost:9092" } });

            services.AddHealthChecks()
              .AddSqlServer(Configuration["ConnectionStrings:DatabaseSQL"]) //sql server 
              .AddNpgSql(Configuration["ConnectionStrings:Database"])// Postgres
              .AddDiskStorageHealthCheck(s => s.AddDrive("C:\\", 1024)) //check disk storage 1024 MB (1 GB) free minimum
              .AddProcessAllocatedMemoryHealthCheck(512) //check 512 MB max allocated memory if exceeds
              .AddProcessHealthCheck("System", p => p.Length > 0) //check if process is running
              .AddWindowsServiceHealthCheck("Audiosrv", s => s.Status == ServiceControllerStatus.Running)//check if windows service is running
              .AddTcpHealthCheck(_=> { _.AddHost("127.0.0.1", 9092); },name:"broker tcp port")//check if 9092 tcp port is healthy
              .AddKafka(producerConfig,topic:"pageviews");//check kafka health 
            
            services.AddHealthChecksUI(s =>
                       {
                           s.AddHealthCheckEndpoint("S3HealthEndpoint", "https://localhost:44376/healthz");
                           s.SetEvaluationTimeInSeconds(300);
                       })
                .AddInMemoryStorage();
            
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapHealthChecks("/healthz");
                endpoints.MapHealthChecksUI();

                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });;
            });
        }
    }
}
