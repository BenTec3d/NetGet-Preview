using Microsoft.AspNetCore.Components.WebView.Maui;

namespace NetGetClient
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.RegisterBlazorMauiWebView().UseMauiApp<App>();

            builder.Services.AddBlazorWebView();

            return builder.Build();
        }
    }
}