# ASP.NET Core Health Check

## Built-In Health Checks
> In ASP.NET Core, the package **Microsoft.AspNetCore.Diagnostics.HealthChecks** is used to add health checks to application. This means that in every project, we have the ability to add health checks out of the box.

```
public void ConfigureServices(IServiceCollection services)
{
  services.AddHealthChecks();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
  app.UseEndpoints(endpoints =>
  {
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/healthz");
  });
}
```
> The package doesn't come with any built-in checks, it only provides the base. In order to add your own checks, there are two ways:

```
services.AddHealthChecks()
  .AddCheck("AlwaysHealthy", () => HealthCheckResult.Healthy())
  .AddCheck<MyCustomCheck>("My Custom Check");

```

* ```AddCheck(string name, Func<HealthCheckResult> check)```, which takes no arguments and returns a status.
* ```AddCheck<IHealthCheck>(string name)```, which takes a class that implements the interface IHealthCheck, where you can put your logic.


## Advanced Health Checks

> Instead of writing every check ourselves, the package **AspNetCore.Diagnostics.HealthChecks** comes to the rescue! It's an ASP.NET Core package that plugs into the existing health checks base and adds many custom checks, including:

* Postgres
* SQL Server
* System (Disk, Memory, Windows Service)
* Network (TCP port)
* Kafka
* …

The full list, which is a lot bigger. [Full List Here](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/netcore-3.0/README.md#health-checks)

Here I did some helath checks.

#### Database Health Checks

> **AspNetCore.HealthChecks** contains checks for the most popular database providers. Today we'll try the SQL Server one by installing the package **AspNetCore.HealthChecks.SqlServer** & PosgreSQL one by installing the package **AspNetCore.HealthChecks.Npgsql**

```
services.AddHealthChecks()
.AddSqlServer(Configuration["ConnectionStrings"]) 
//sql server 
.AddNpgSql(Configuration["ConnectionStrings"]);
//Postgres
```

#### System Health Checks

> **AspNetCore.HealthChecks.System** contains many system related checks, let's look at a few of them.

```
services.AddHealthChecks()
.AddDiskStorageHealthCheck(s => s.AddDrive("C:\\", 1024)) 
//check disk storage 1024 MB (1 GB) free minimum
.AddProcessAllocatedMemoryHealthCheck(512) 
//check 512 MB max allocated memory if exceeds
.AddProcessHealthCheck("System", p => p.Length > 0) 
//check if process is running
.AddWindowsServiceHealthCheck("Audiosrv", s => s.Status == ServiceControllerStatus.Running)
//check if windows service is running
```
#### Network Health Checks

> **AspNetCore.HealthChecks.Network** contains many Network related checks, let’s do tcp health check.

```
services.AddHealthChecks()
.AddTcpHealthCheck(_=> { _.AddHost("127.0.0.1", 9092); }, name:"broker tcp port")
//check if 9092 port is healthy
```

#### Kafka Health Checks

> Lets install **AspNetCore.HealthChecks.Kafka**  to kafka health check.

```
var producerConfig = new Confluent.Kafka.ProducerConfig(new Dictionary<string, string>() { { "bootstrap.servers", "localhost:9092" } });

services.AddHealthChecks()
.AddKafka(producerConfig,topic:"pageviews"); 
//check kafka health
```

## Health Checks UI

> There is also a package that adds a monitoring UI that shows you the status of all the checks you added, as well as their history.

> First, let's install the packages:

> **AspNetCore.HealthChecks.UI** which adds the UI.
**AspNetCore.HealthChecks.UI.Client** which turns our old response (e.g. Healthy) into a more detailed response.
**AspNetCore.HealthChecks.UI.InMemory.Storage** which saves the results in memory for the UI to use.
Then let's register the UI:

```
public void ConfigureServices(IServiceCollection services)
{
  // ...
  services
    .AddHealthChecksUI(s =>
    {
        s.AddHealthCheckEndpoint("S3HealthEndpoint", "https://localhost:44376/healthz");
    })
    .AddInMemoryStorage();
  // ...
}
```
```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
  // ...

  app.UseEndpoints(endpoints =>
  {
      endpoints.MapControllers();
      endpoints.MapHealthChecksUI();
      endpoints.MapHealthChecks("/healthz", new HealthCheckOptions()
      {
          Predicate = _ => true,
          ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
      });
  });

  // ...
}
```

#### Setting Health Checks Interval

> By default this health checks api perform it action in per minutes. But we can customize it by this code.

```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
  // ...

	services.AddHealthChecksUI(s =>
                       {
                           s.AddHealthCheckEndpoint("S3HealthEndpoint", 	"https://localhost:44376/healthz");
                           s.SetEvaluationTimeInSeconds(300);
                       })
                .AddInMemoryStorage();
  // ...
}
```

## Result Screenshots
```https://localhost:44376/healthz```
> Raw Result
![Image of Raw Result](https://github.com/iamonlysaiful/AspNetCoreHealthCheck/blob/main/S3HealthChecks/Screenshots/raw_output.PNG?raw=true)

```https://localhost:44376/healthchecks-ui```
> Health Checks UI
![Image of Health Checks UI](https://github.com/iamonlysaiful/AspNetCoreHealthCheck/blob/main/S3HealthChecks/Screenshots/healthChecks_ui.PNG?raw=true)


