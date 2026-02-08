using EBCI_FrontEnd.Services;
using EBCI_FrontEnd.Views.Main;
using EBCI_Library.Services;

namespace EBCI_FrontEnd {
    public static class Program {
        private static void Main(string[] args) {
            //todo parametrize
            LogService.Configure(@"C:\Users\karol\Desktop\EBCI\EBCI_Frontend");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddScoped<ISignalService, SignalService>();
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