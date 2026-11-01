using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CourseApp.Configurations;
using CourseApp.Persistence;
using CourseApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
             
        var services = new ServiceCollection();
                
        ServicesConfiguration.ConfigureServices(services, Configuration);

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = Services.GetRequiredService<ViewModels.MainViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = viewModel };
            _ = InitializeAsync(viewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeAsync(ViewModels.MainViewModel viewModel)
    {
        try
        {
            var factory = Services!.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using (var db = await factory.CreateDbContextAsync())
            {
                await DbSeeder.SeedAsync(db);
            }
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Błąd bazy danych: {ex.Message}";
            return;
        }

        await viewModel.LoadCoursesAsync();
    }
}