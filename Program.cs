using Microsoft.AspNetCore.HttpLogging;
using Scalar.AspNetCore;
using WebApi.Controllers.V2;
using WebApi.Core;
using WebApi.Data;
using WebApi.Data.Repositories;
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
         opts.LoggingFields = HttpLoggingFields.All);
      
      // Add ProblemDetails, see https://tools.ietf.org/html/rfc7807
      builder.Services.AddProblemDetails();
      
      // Add controllers
      builder.Services.AddControllers();
      builder.Services.AddScoped<ControllerHelper>();
      
      // Add versioning
      builder.Services.AddApiVersioning();
      
      builder.Services.AddOpenApi(options => {
         options.AddDocumentTransformer((document, context, cancellationToken) => {
            document.Info = new() {
               Title = "CarShop API",
               Version = "v1",
               Description = "Online marketplace for used cars."
            };
            return Task.CompletedTask;
         });
      });
      
      
      //builder.Services.AddOpenApi("v2");
      
      builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
      builder.Services.AddSingleton<ICarRepository, CarRepository>();
      builder.Services.AddSingleton<IDataContext, DataContext>();
      
      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
         app.MapOpenApi();
         
         app.UseSwaggerUI(opt => {
            opt.SwaggerEndpoint("/openapi/v1.json", "CarShop API v1");
            //opt.SwaggerEndpoint("/openapi/v2.json", "CarShop API v2");
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