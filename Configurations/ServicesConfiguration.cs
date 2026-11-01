using CourseApp.Data;
using CourseApp.Persistence;
using CourseApp.Repository;
using CourseApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseApp.Configurations;

public static class ServicesConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<MainViewModel>();
        services.AddScoped<ICourseRepository, CourseRepository>();

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }
}