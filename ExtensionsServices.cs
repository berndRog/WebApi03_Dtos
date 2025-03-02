using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
namespace WebApi;

public static class ExtensionsServices {

   #region AddApiVersioning
   public static IServiceCollection AddApiVersioning(
      this IServiceCollection services
   ) {

      var apiVersionReader = ApiVersionReader.Combine(
         new UrlSegmentApiVersionReader(),
         new HeaderApiVersionReader("x-api-version")
         // new MediaTypeApiVersionReader("x-api-version"),
         // new QueryStringApiVersionReader("api-version")
      );

      services.AddApiVersioning(options => {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = apiVersionReader;
         })
         //.AddMvc()
         .AddApiExplorer(options => {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
         });
      return services;
   }
   #endregion
   
   #region AddOpenApiSettings
   public static IServiceCollection AddOpenApiSettings(
      this IServiceCollection services,
      string version
   ) {

      services.AddOpenApi(version, options => {
         options.AddDocumentTransformer((document, context, cancellationToken) => {
            document.Info = new() {
               Title = "CarShop API",
               Version = version,
               Description = "Online marketplace for used cars."
            };
            return Task.CompletedTask;
         });

         //options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
         // options.AddDocumentTransformer((document, context, cancellationToken) => {
         //    var requirements = new Dictionary<string, OpenApiSecurityScheme> {
         //       ["Bearer"] = new OpenApiSecurityScheme {
         //          Type = SecuritySchemeType.Http,
         //          Scheme = "bearer", // "bearer" refers to the header name here
         //          In = ParameterLocation.Header,
         //          BearerFormat = "Json Web Token"
         //       }
         //    };
         //    document.Components ??= new OpenApiComponents();
         //    document.Components.SecuritySchemes = requirements;
         //    return Task.CompletedTask;
         // });
      });

      return services;
   }
   #endregion
}


internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
   public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
   {
      var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
      if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
      {
         var requirements = new Dictionary<string, OpenApiSecurityScheme>
         {
            ["Bearer"] = new OpenApiSecurityScheme
            {
               Type = SecuritySchemeType.Http,
               Scheme = "bearer", // "bearer" refers to the header name here
               In = ParameterLocation.Header,
               BearerFormat = "Json Web Token"
            }
         };
         document.Components ??= new OpenApiComponents();
         document.Components.SecuritySchemes = requirements;
      }
   }
}


// public static IServiceCollection AddOpenApiSettings(
   //    this IServiceCollection services,
   //    string version
   // ) {
   //    // Add OpenAPI with Bearer token support
   //    services.AddOpenApi(version, settings => {
   //   
   //       settings.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
   //          Type = SecuritySchemeType.Http,
   //          Scheme = "bearer",
   //          BearerFormat = "JWT",
   //          Description = "JWT Authorization header using the Bearer scheme."
   //       });
   //       settings.AddSecurityRequirement(new OpenApiSecurityRequirement {
   //          {
   //             new OpenApiSecurityScheme {
   //                Reference = new OpenApiReference {
   //                   Type = ReferenceType.SecurityScheme,
   //                   Id = "Bearer"
   //                }
   //             },
   //             Array.Empty<string>()
   //          }
   //       });
   //    });
   //    return services;
   // }
   

   
