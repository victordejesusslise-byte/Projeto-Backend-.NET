using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UsuariosAPI.API.Documentation;

public class SwaggerParameterExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Description = "ID numérico do usuário. Exemplo: 1.";
                parameter.Example = new OpenApiInteger(1);
            }

            if (parameter.Name.Equals("pagina", StringComparison.OrdinalIgnoreCase))
                parameter.Example = new OpenApiInteger(1);

            if (parameter.Name.Equals("tamanhoPagina", StringComparison.OrdinalIgnoreCase))
                parameter.Example = new OpenApiInteger(10);
        }
    }
}
