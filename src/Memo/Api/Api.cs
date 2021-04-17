using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Memo
{
    public class Api
    {
        public static IWebHost CreateWebHost()
        {
            var config = new CommandConfig();

            return new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(config.HomeDirectory.FullName)
                .UseStartup<Startup>()
                .Build();
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddMvc();
                services.AddScoped<IMemoManager, MemoManager>();
            }

            public void Configure(IApplicationBuilder application, IWebHostEnvironment environment)
            {
                if (environment.IsDevelopment())
                {
                    application.UseDeveloperExceptionPage();
                }

                application.UseRouting();

                application.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
        }
    }
}