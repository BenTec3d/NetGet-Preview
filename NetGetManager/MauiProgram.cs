using Microsoft.AspNetCore.Components.WebView.Maui;
using MudBlazor.Services;
using NetGetShared;
using DataLibrary;

namespace NetGetManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.RegisterBlazorMauiWebView().UseMauiApp<App>();

        builder.Services.AddBlazorWebView();
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<DataAccessSQLite>();
        builder.Services.AddSingleton<DataAccessMySQL>();
        builder.Services.AddSingleton<PropertiesService>();
        builder.Services.AddSingleton<RefreshService>();
        builder.Services.AddSingleton<CrudService>();
        builder.Services.AddSingleton<DownloadExtractService>();
        builder.Services.AddSingleton<IconColorService>();


        return builder.Build();
    }
}
