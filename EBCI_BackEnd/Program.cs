using EBCI_Library.Services;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(EBCI_BackEnd.Startup))]
namespace EBCI_BackEnd {
    public static class Startup {
        public static void Configuration(IAppBuilder app) {
            var settings = Properties.Settings.Default;
            LogService.Configure(settings.LogsPath);

            app.MapSignalR();
        }
    }
}