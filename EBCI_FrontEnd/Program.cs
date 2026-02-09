using EBCI_FrontEnd.Services;
using EBCI_FrontEnd.Views.Main;
using EBCI_Library.Services;

namespace EBCI_FrontEnd {
    public static class Program {
        private static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
            builder.Services.AddScoped<ISignalService, SignalService>();

            var configurationService = new ConfigurationService(builder.Configuration);
            LogService.Configure(configurationService.GetLogsPath());

            var app = builder.Build();
            if (!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();
            app.UseAntiforgery();
            app.MapStaticAssets();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            app.Run();
        }
    }
}