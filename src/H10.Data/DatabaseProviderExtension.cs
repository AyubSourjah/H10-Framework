using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace H10.Data;

public static class DatabaseProviderExtension
{
    public static void AddPeoplesHrDbService(this IServiceCollection services)
    {
        services.TryAddScoped<DatabaseProvider>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var context = sp.GetRequiredService<IHttpContextAccessor>();
            var dbFactory = sp.GetRequiredService<DbProviderFactory>();
            
            return new DatabaseProvider(config, context, dbFactory);
        });
    }
    
    public static void AddJuraaDbService(this IServiceCollection services, string domain)
    {
        services.TryAddScoped<DatabaseProvider>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var dbFactory = sp.GetRequiredService<DbProviderFactory>();
            
            return new DatabaseProvider(config, domain, dbFactory);
        });
    }
}