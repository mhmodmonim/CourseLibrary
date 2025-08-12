using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;


namespace CourseLibrary.API;

internal static class StartupHelperExtensions
{
    // Add services to the container
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(configure =>
        {
            configure.ReturnHttpNotAcceptable = true;
            configure.CacheProfiles.Add("240SecCacheProfile",
                new()
                {
                    Duration = 240, // 4 minutes
                    //Location = ResponseCacheLocation.Any,
                    //NoStore = false
                });
        })
        .AddNewtonsoftJson(options => // This extension method is in Microsoft.AspNetCore.Mvc.NewtonsoftJson
        {
            options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
        })
        .AddXmlDataContractSerializerFormatters()
        .ConfigureApiBehaviorOptions(builder =>
        {
            // Disable the default behavior of returning 400 Bad Request
            // for validation errors, so we can return 422 Unprocessable Entity
            builder.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = context.HttpContext.RequestServices
                    .GetRequiredService<ProblemDetailsFactory>()
                    .CreateValidationProblemDetails(context.HttpContext, context.ModelState);



                problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                problemDetails.Title = "One or more validation errors occurred.";
                problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                problemDetails.Detail = "See the errors field for details.";
                problemDetails.Instance = context.HttpContext.Request.Path;

                return new UnprocessableEntityObjectResult(problemDetails)
                {
                    ContentTypes =  { "application/problem+json"}
                }; 
            };
        });

        builder.Services.AddScoped<ICourseLibraryRepository,
            CourseLibraryRepository>();

        builder.Services.AddTransient<IPropertyMappingService, PropertyMappingService>();
        builder.Services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();
        builder.Services.AddDbContext<CourseLibraryContext>(options =>
        {
            options.UseSqlite(@"Data Source=library.db");
        });

        builder.Services.AddAutoMapper(
            AppDomain.CurrentDomain.GetAssemblies());


        builder.Services.AddResponseCaching();

        return builder.Build();
    }

    // Configure the request/response pipelien
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        app.UseResponseCaching();

        app.UseAuthorization();

        app.MapControllers(); 
         
        return app; 
    }

    public static async Task ResetDatabaseAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetService<CourseLibraryContext>();
                if (context != null)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        } 
    }
}