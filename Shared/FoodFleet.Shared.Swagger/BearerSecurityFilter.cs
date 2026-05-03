using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoodFleet.Shared.Swagger;

/// <summary>
/// Swashbuckle operation filter that adds a Bearer JWT security requirement to every Swagger operation.
/// Ensures all API endpoints in the Swagger UI show the lock icon and require a token.
/// </summary>
public class BearerSecurityFilter : IOperationFilter
{
    /// <summary>
    /// Applies the Bearer security requirement to the given Swagger operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to modify.</param>
    /// <param name="context">The filter context providing metadata about the operation.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
