using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Memo.Core;

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
                var config = new CommandConfig();

                services.AddMvc();

                services.AddSingleton<CommandConfig>();
                services.AddSingleton<Core.Categories.ICategoryConfigStore, Core.Categories.CategoryConfigStore>();
                services.AddSingleton<Core.Notes.INoteBuilder, Core.Notes.NoteBuilder>();
                services.AddSingleton<Core.Notes.INoteSerializer, Core.Notes.NoteSerializer>();
                services.AddSingleton<Core.Notes.INoteQueryFilter, Core.Notes.NoteQueryFilter>();
                services.AddSingleton<Core.Notes.INoteStorage, Core.Notes.NoteStorageFileSystemImpl>();
                services.AddSingleton<Core.Notes.INoteRepository, Core.Notes.NoteRepository>();
                services.AddSingleton<Core.Notes.INoteService, Core.Notes.NoteService>();
                services.AddSingleton<Core.Categories.ICategoryRepository, Core.Categories.CategoryRepository>();
                services.AddSingleton<Core.Categories.ICategoryService, Core.Categories.CategoryService>();
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