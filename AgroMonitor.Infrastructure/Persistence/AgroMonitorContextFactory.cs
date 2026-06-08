using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgroMonitor.Infrastructure.Persistence;

/// <summary>
/// Fábrica usada pelas ferramentas do EF Core (dotnet ef) em tempo de design,
/// para criar o contexto sem subir a API. A connection string vem da variável
/// de ambiente ConnectionStrings__AgroMonitorOracle, com um valor padrão de
/// desenvolvimento (a geração da migration não abre conexão com o banco).
/// </summary>
public sealed class AgroMonitorContextFactory : IDesignTimeDbContextFactory<AgroMonitorContext>
{
    public AgroMonitorContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__AgroMonitorOracle")
            ?? "User Id=REPLACE_USER;Password=REPLACE_PASSWORD;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))(CONNECT_DATA=(SID=ORCL)))";

        var options = new DbContextOptionsBuilder<AgroMonitorContext>()
            .UseOracle(connectionString)
            .Options;

        return new AgroMonitorContext(options);
    }
}
