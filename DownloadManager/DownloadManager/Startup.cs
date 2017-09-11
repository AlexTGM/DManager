using System;
using System.Linq;
using SystemInterface.IO;
using SystemInterface.Timers;
using SystemWrapper.IO;
using SystemWrapper.Timers;
using DownloadManager.Factories;
using DownloadManager.Factories.Impl;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using DownloadManager.Tools;
using DownloadManager.Tools.Impl;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DownloadManager
{
    public class ApplicationOptions
    {
        public int DefaultThreadsPerDownload { get; set; } = 1;
        public long DefaultThreasholdPerSecond { get; set; } = 0;
        public long BytesPerSecond { get; set; }
        public int ThreadsPerDownload { get; set; }
    }

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
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/event-stream" });
            });

            services.AddServerSentEvents();
            services.AddServerSentEvents<IServerSentEventsService, ServerSentEventsService>();

            services.Configure<ApplicationOptions>(Configuration);

            services.AddSingleton<IFile, FileWrap>();
            services.AddSingleton<IFileMerger, FileMerger>();
            services.AddSingleton<ITimerFactory, TimerFactory>();
            services.AddSingleton<INameGenerator, NameGenerator>();
            services.AddSingleton<IUrlHelperTools, UrlHelperTools>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IBinaryReaderFactory, BinaryReaderFactory>();
            services.AddSingleton<IBinaryWriterFactory, BinaryWriterFactory>();
            services.AddSingleton<IHttpWebRequestFactory, HttpWebRequestFactory>();
            services.AddSingleton<IDownloadingTasksFactory, DownloadingTasksFactory>();
            services.AddSingleton<IFileInformationProvider, FileInformationProvider>();

            services.AddScoped<IFileDownloader, FileDownloader>();
            services.AddScoped<IDownloadSpeedMeter, DownloadSpeedMeter>();
            services.AddScoped<IDownloadSpeedLimiter, DownloadSpeedLimiter>();
            services.AddScoped<IFileDownloaderManager, FileDownloaderManager>();
            services.AddScoped<IDownloadManager, Services.Impl.DownloadManager>();
            
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseResponseCompression();

            app.MapServerSentEvents("/api/sse-notifications", serviceProvider.GetService<ServerSentEventsService>());

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
