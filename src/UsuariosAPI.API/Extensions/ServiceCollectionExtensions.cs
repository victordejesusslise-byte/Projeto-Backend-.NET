using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using UsuariosAPI.API.Documentation;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.DTOs.Responses;
using UsuariosAPI.Application.Interfaces;
using UsuariosAPI.Application.Services;
using UsuariosAPI.Application.Validators;
using UsuariosAPI.Domain.Interfaces.Repositories;
using UsuariosAPI.Infrastructure.Data.Context;
using UsuariosAPI.Infrastructure.Repositories;

namespace UsuariosAPI.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IMvcBuilder AddApiControllers(this IServiceCollection services)
    {
        var mvcBuilder = services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                var erros = actionContext.ModelState
                    .SelectMany(item => item.Value?.Errors
                        .Select(error => MapearErroDeModelo(item.Key, error))
                        ?? Enumerable.Empty<string>())
                    .Distinct()
                    .ToArray();

                if (erros.Length == 0)
                    erros = new[] { "A requisição contém dados inválidos." };

                var response = new ApiResponse<object>(
                    false,
                    "Não foi possível processar a requisição.",
                    null,
                    erros,
                    actionContext.HttpContext.TraceIdentifier);

                return new BadRequestObjectResult(response);
            };
        });

        return mvcBuilder;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IUsuarioService, UsuarioService>();

        // Validators
        services.AddScoped<IValidator<CriarUsuarioRequest>, CriarUsuarioValidator>();
        services.AddScoped<IValidator<AtualizarUsuarioRequest>, AtualizarUsuarioValidator>();
        services.AddScoped<IValidator<ListarUsuariosRequest>, ListarUsuariosValidator>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "String de conexão 'DefaultConnection' não encontrada. Configure o arquivo .env ou appsettings.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.EnableRetryOnFailure(3)));

        // Repositories
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Usuários API",
                Version = "v1",
                Description = "API RESTful para gerenciamento de usuários — CRUD completo com filtros e paginação.",
                Contact = new OpenApiContact
                {
                    Name = "Equipe de Desenvolvimento",
                    Email = "dev@empresa.com"
                }
            });

            // Incluir comentários XML para documentação
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);

            c.OperationFilter<SwaggerParameterExamplesOperationFilter>();
        });

        return services;
    }

    private static string MapearErroDeModelo(string campo, ModelError erro)
    {
        if (erro.ErrorMessage.Contains("request body", StringComparison.OrdinalIgnoreCase) ||
            erro.ErrorMessage.Contains("corpo da requisição", StringComparison.OrdinalIgnoreCase))
        {
            return "O corpo da requisição é obrigatório e deve conter JSON válido.";
        }

        if (erro.Exception is JsonException ||
            erro.Exception?.InnerException is JsonException ||
            erro.ErrorMessage.Contains("JSON", StringComparison.OrdinalIgnoreCase))
        {
            return "O corpo da requisição contém JSON inválido ou um valor com tipo incorreto.";
        }

        if (string.IsNullOrWhiteSpace(campo))
            return "O corpo da requisição é obrigatório e deve conter JSON válido.";

        var nomeCampo = campo.TrimStart('$', '.');

        if (erro.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase) ||
            erro.ErrorMessage.Contains("obrigatório", StringComparison.OrdinalIgnoreCase))
        {
            return $"O campo '{nomeCampo}' é obrigatório.";
        }

        return $"O valor informado para o campo '{nomeCampo}' é inválido.";
    }
}
