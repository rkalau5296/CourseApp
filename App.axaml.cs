using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CourseApp.Configurations;
using CourseApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace CourseApp;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }
    public IConfiguration? Configuration { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {        
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
             
        var services = new ServiceCollection();
                
        ServicesConfiguration.ConfigureServices(services, Configuration);

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {            
            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<ViewModels.MainViewModel>()
            };

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}