using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Entities;
using Microsoft.VisualStudio.TestPlatform.TestHost;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureServices(services => {
            var descripter = services.SingleOrDefault(temp => temp.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descripter != null)
            {
                services.Remove(descripter);
            }






            services.AddDbContext<ApplicationDbContext>(options =>
            {
               
                options.UseInMemoryDatabase("DatbaseForTesting");
                
                
            });
        });
    }
}


