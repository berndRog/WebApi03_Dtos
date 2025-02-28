using Asp.Versioning;
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
   
}
