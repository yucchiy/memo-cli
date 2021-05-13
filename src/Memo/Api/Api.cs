using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Markdig;
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

                // TODO: using DI Container
                var configStore = new Core.Categories.CategoryConfigStore(config.MemoConfig.Categories);
                var noteBuilder = new Core.Notes.NoteBuilder(configStore);
                var noteRepository = new Core.Notes.NoteRepository(
                    new Core.Notes.NoteStorageFileSystemImpl(
                        new Core.Notes.NoteSerializer(
                            noteBuilder,
                            configStore,
                            (new Markdig.MarkdownPipelineBuilder())
                                .UseYamlFrontMatter().Build(),
                            new Core.Notes.NoteSerializer.Options(config.HomeDirectory, '/')
                        ),
                        new Core.Notes.NoteStorageFileSystemImpl.Options(config.HomeDirectory)
                    ),
                    new Core.Notes.NoteQueryFilter()
                );
                var noteService = new Core.Notes.NoteService(
                    noteRepository,
                    noteBuilder
                );

                var categoryService = new Core.Categories.CategoryService(
                    new Core.Categories.CategoryRepository(noteRepository)
                );

                services.AddMvc();
                services.AddScoped<IMemoManager, MemoManager>();
                services.AddSingleton<Core.Categories.ICategoryService>(categoryService);
                services.AddSingleton<Core.Notes.INoteService>(noteService);
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