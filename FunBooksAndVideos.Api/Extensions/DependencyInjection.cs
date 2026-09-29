using FunBooksAndVideos.Core.Abstractions;
using FunBooksAndVideos.Infrastructure.Persistence;
using FunBooksAndVideos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=funbooks.db";

        services.AddDbContext<FunBooksDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IShippingSlipService, ShippingSlipService>();

        return services;
    }
}
