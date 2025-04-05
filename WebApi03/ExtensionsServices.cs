using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApi.Core;
using WebApi.Data;
using WebApi.Data.Repositories_refactored;
namespace WebApi;

public static class ExtensionsServices {
   
   #region AddData
   public static IServiceCollection AddData(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      
      services.AddScoped<IPeopleRepository, PeopleRepository>();
      services.AddScoped<ICarsRepository, CarsRepository>();
      
      // Add DbContext (Database) to DI-Container
      var (useDatabase, dataSource) = DataContext.EvalDatabaseConfiguration(configuration);
      
      switch (useDatabase) {
         case "Sqlite": 
         case "SqliteInMemory":
            services.AddDbContext<IDataContext, DataContext>(options => 
               options.UseSqlite(dataSource)
            );
            break;
         default:
            throw new Exception("appsettings.json UseDatabase not available");
      }
      return services;
   }
   #endregion
   

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

      services.AddOpenApi(version, opt => {
         opt.AddDocumentTransformer((document, _, _) => {
            document.Info = new() {
               Title = "CarShop API",
               Version = version,
               Description = "Online marketplace for used cars."
            };
            return Task.CompletedTask;
         });

         opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
         opt.AddDocumentTransformer((document, context, cancellationToken) => {
            var requirements = new Dictionary<string, OpenApiSecurityScheme> {
               ["Bearer"] = new OpenApiSecurityScheme {
                  Type = SecuritySchemeType.Http,
                  Scheme = "bearer", // "bearer" refers to the header name here
                  In = ParameterLocation.Header,
                  BearerFormat = "Json Web Token"
               }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
            return Task.CompletedTask;
         });
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