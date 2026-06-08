using System.Reflection;
using AgroMonitor.API.Exceptions;
using AgroMonitor.API.Extensions;
using Microsoft.OpenApi.Models;

namespace AgroMonitor.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAgroMonitorDbContext(builder.Configuration);
        builder.Services.AddAgroMonitorRepositories();
        builder.Services.AddAgroMonitorApplicationServices();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AgroMonitor API - Cadastro",
                Version = "v1",
                Description =
                    "API REST do dominio de Cadastro (espécies e vagas de plantio) do AgroMonitor. " +
                    "Dona da escrita de TB_CAD_SPECIES e TB_CAD_SLOT no schema Oracle compartilhado."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "AgroMonitor API v1");
                options.RoutePrefix = "";
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
