using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadAngle.ViewModels;
using RoadAngle.Views;
using System.IO;
using System.Reflection;

namespace RoadAngle
{
    /// <summary>
    ///     Provides a host for the application's services and manages their lifetimes
    /// </summary>
    public static class Host
    {
        private static IHost _host;

        /// <summary>
        ///     Starts the host and configures the application's services
        /// </summary>
        public static void Start()
        {
            var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings
            {
                ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location),
                DisableDefaults = true
            });

            builder.Services.AddTransient<RoadAngleViewModel>();
            builder.Services.AddTransient<RoadAngleView>();

            _host = builder.Build();
            _host.Start();
        }

        /// <summary>
        ///     Stops the host
        /// </summary>
        public static void Stop()
        {
            _host.StopAsync();
        }

        /// <summary>
        ///     Gets a service of the specified type
        /// </summary>
        /// <typeparam name="T">The type of service object to get</typeparam>
        /// <returns>A service object of type T or null if there is no such service</returns>
        public static T GetService<T>() where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }
    }
}