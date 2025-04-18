using Microsoft.AspNetCore.HttpLogging;
using Scalar.AspNetCore;
using WebApi.Core;
using WebApi.Data;
namespace WebApi;

public class Program {
   public static void Main(string[] args) {
      var builder = WebApplication.CreateBuilder(args);
      
      // Configure logging
      builder.Logging.ClearProviders();
      builder.Logging.AddConsole();
      builder.Logging.AddDebug();
      
      // Configure DI-Container -----------------------------------------
      // add http logging 
      builder.Services.AddHttpLogging(opts =>
         opts.LoggingFields = HttpLoggingFields.None);
      
      // Add controllers
      builder.Services.AddControllers();
      
      // Add repositories and data context
      builder.Services.AddData(builder.Configuration);
      
      // Add ProblemDetails, see https://tools.ietf.org/html/rfc7807
      builder.Services.AddProblemDetails(options => {
         options.CustomizeProblemDetails = context => {
            // Add exception details only in development
            context.ProblemDetails.Extensions["devMode"] = builder.Environment.IsDevelopment();
            if (builder.Environment.IsDevelopment()) {
               // Include stack trace or other debug info in development
               if (context.Exception != null)
                  context.ProblemDetails.Extensions["stackTrace"] = context.Exception.StackTrace;
            }
         };
      });
      
      // Add versioning
      builder.Services.AddApiVersioning();
      
      // Add OpenApi
      builder.Services.AddOpenApiSettings("v1");
      builder.Services.AddOpenApiSettings("v2");
      
      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
         app.MapOpenApi();
         
         app.UseSwaggerUI(opt => {
            opt.SwaggerEndpoint("/openapi/v1.json", "CarShop API v1");
            opt.SwaggerEndpoint("/openapi/v2.json", "CarShop API v2");
         });
         
         
         app.MapScalarApiReference( options => {
            options
               .WithTitle("CarShop API")
               .WithSidebar(true)
               .WithDarkMode(false);
            
         });
      }

      //app.UseAuthorization();
      app.MapControllers();

      app.Run();
   }
}