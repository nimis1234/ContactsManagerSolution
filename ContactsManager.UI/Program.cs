using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services;
using Rotativa.AspNetCore;
using IRepository;
using Repository;
using Microsoft.Extensions.Configuration;
using Serilog;
using CURDExample.Filters;
using CURDExample.StartUpExtensions;
using CURDExample.Middleware;
using ContactManager.Core.Domain.IdentityEntity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// adding services: services code are added to configuration services extension  
// this how custom services are added
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);// name if extension class ie 


//
var path = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName;
// Define log directory
var logPath = Path.Combine(path, "Logs");

// Ensure directory exists
if (!Directory.Exists(logPath))
{
Directory.CreateDirectory(logPath);
}
//Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {

loggerConfiguration
.ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
.ReadFrom.Services(services); //read out current app's services and make them available to serilog
});

builder.Services.AddHttpContextAccessor();


//INSTALL-package Rotativa aspnetcore
//install rotativa  exe and put ito wwwroot folder


//Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

// get appsettings.json
var rotativaRootPath = builder.Configuration.GetSection("RotativaPath").GetSection("RootPath").Value;
var WkhtmltopdfRelativePath = builder.Configuration.GetSection("RotativaPath").GetSection("WkhtmltopdfRelativePath").Value;

RotativaConfiguration.Setup(rotativaRootPath, WkhtmltopdfRelativePath);
// build app and run
var app = builder.Build();

//middeleware addings

// Ensure logs are flushed on shutdown
app.Lifetime.ApplicationStopping.Register(Log.CloseAndFlush);
// Enable HTTP request logging
app.UseSerilogRequestLogging();



app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable Authentication & Authorization Middleware
app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();



if (builder.Environment.IsDevelopment())
{
//app.UseDeveloperExceptionPage();

app.UseExceptionHandler(
     new ExceptionHandlerOptions()
     {
         AllowStatusCode404Response = true,
         ExceptionHandlingPath = "/Home/Error"
     }
 );

app.UseExceptionalHandleMiddleware();
}
else

{
app.UseExceptionHandler(
     new ExceptionHandlerOptions()
{
AllowStatusCode404Response = true,
ExceptionHandlingPath = "/Home/Error"
}
 );

app.UseExceptionalHandleMiddleware();
// this is   exceptional Hnadle middleware created to catch all exceptions globally
//create a exceptional handle middleware in middleware folder, added it as a extension method
}
Serilog.Debugging.SelfLog.Enable(Console.Error);

// enable http



app.UseHttpLogging();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=AdminIndex}"); // if Area exist then 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");  
app.Run();
public partial class Program { } // Make the auto-generated Program accessible programmatically this for integration testing
