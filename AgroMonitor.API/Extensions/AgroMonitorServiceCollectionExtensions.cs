using AgroMonitor.Application.Repositories;
using AgroMonitor.Application.Services.Implementations;
using AgroMonitor.Application.Services.Interfaces;
using AgroMonitor.Infrastructure.Persistence;
using AgroMonitor.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Infrastructure;

namespace AgroMonitor.API.Extensions;

/// <summary>
/// Extensões para registrar persistência, repositórios e serviços de aplicação
/// da solução AgroMonitor na injeção de dependências.
/// </summary>
public static class AgroMonitorServiceCollectionExtensions
{
    /// <summary>
    /// Registra o <see cref="AgroMonitorContext"/> apontando para o Oracle.
    /// A connection string vem de ConnectionStrings:AgroMonitorOracle (appsettings
    /// ou variável de ambiente ConnectionStrings__AgroMonitorOracle no deploy).
    /// </summary>
    /// <exception cref="InvalidOperationException">Quando a connection string não for encontrada.</exception>
    public static IServiceCollection AddAgroMonitorDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "AgroMonitorOracle")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' não encontrada. " +
                "Configure em appsettings.json ou na variável de ambiente " +
                $"ConnectionStrings__{connectionStringName}.");

        // Informa ao provider que o banco é Oracle 19c — sem isso ele gera
        // literais booleanos TRUE/FALSE (só existem no Oracle 23), causando
        // ORA-00904 em consultas com .Any()/EXISTS.
        services.AddDbContext<AgroMonitorContext>(options =>
            options.UseOracle(connectionString,
                oracle => oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

        return services;
    }

    /// <summary>
    /// Registra os repositórios como <c>Scoped</c> (um por requisição HTTP).
    /// </summary>
    public static IServiceCollection AddAgroMonitorRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISpeciesRepository, SpeciesRepository>();

        return services;
    }

    /// <summary>
    /// Registra os serviços de aplicação que orquestram os repositórios.
    /// As implementações são adicionadas conforme cada vertical slice é construído.
    /// </summary>
    public static IServiceCollection AddAgroMonitorApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISpeciesService, SpeciesService>();
        services.AddScoped<ISlotService, SlotService>();

        return services;
    }
}
