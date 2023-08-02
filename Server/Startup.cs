using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Queries.Users.GetMyUser;
using EventsManager.Server.Models;
using EventsManager.Server.Services;
using EventsManager.Server.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EventsManager.Server;

public class Startup {
    
    public IConfiguration configRoot {
        get;
    }
    
    public Startup(IConfiguration configuration) {
        configRoot = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
        // Add services to the container.
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 3;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 0;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        var issuer = configuration.GetSection("IdentityServer")["IssuerUri"];
        services.AddIdentityServer(options =>
            { 
                options.IssuerUri = issuer;
            })
            .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(opt => 
            {
                opt.IdentityResources["openid"].UserClaims.Add("role");
                opt.ApiResources.Single().UserClaims.Add("role");
            });

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");
        
        var emailSection = configuration.GetSection("EmaiLService");
        services.Configure<EmailOptions>(emailSection);
        services.AddScoped<IEmailService, EmailService>();
        
        var googleAuthSection = configuration.GetSection("GoogleAuth");
        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = googleAuthSection["Settings:ClientId"];
                options.ClientSecret = googleAuthSection["Settings:ClientSecret"];
                options.Scope.Add("email");
            })
            .AddIdentityServerJwt();

        services.AddControllersWithViews();
        services.AddRazorPages();
        
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("padelpointdb", tags: new []{ FullTag, LiteTag });
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetMyUserQueryHandler).Assembly));
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<GetMyUserQueryHandler>();
        });
        
        var blobStorageSection = configuration.GetSection("BlobStorage");
        services.Configure<BlobStorageSettings>(blobStorageSection);
    }
    
    public void Configure(WebApplication app, IWebHostEnvironment env, IServiceProvider services) {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();    
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        app.UseHealthChecks("/hc", new HealthCheckOptions
        {
            ResponseWriter = WriteResponseAsync,
            Predicate = reg => reg.Tags.Contains(FullTag)
        });

        app.UseHealthChecks("/hc-lite", new HealthCheckOptions
        {
            ResponseWriter = WriteResponseAsync,
            Predicate = reg => reg.Tags.Contains(LiteTag)
        });
        
        app.UseDeveloperExceptionPage();
        
        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseIdentityServer();
        app.UseAuthorization();
        
        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
        
        app.UseHealthChecks("/hc", new HealthCheckOptions
        {
            ResponseWriter = WriteResponseAsync,
            Predicate = reg => reg.Tags.Contains(FullTag)
        });

        app.UseHealthChecks("/hc-lite", new HealthCheckOptions
        {
            ResponseWriter = WriteResponseAsync,
            Predicate = reg => reg.Tags.Contains(LiteTag)
        });
        
        app.Run();
    }
    
    private const string FullTag = "full";
    private const string LiteTag = "lite";

    private static Task WriteResponseAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json";
        var text = Serialize(report);
        return httpContext.Response.WriteAsync(text);
    }

    private static string Serialize(HealthReport report)
    {
        var options = new JsonWriterOptions()
        {
            Indented = true
        };
        using var utf8Json = new MemoryStream();
        using (var writer = new Utf8JsonWriter((Stream) utf8Json, options))
        {
            writer.WriteStartObject();
            writer.WriteString("status", report.Status.ToString());
            writer.WriteStartObject("results");
            report.Entries.ToList<KeyValuePair<string, HealthReportEntry>>().ForEach((Action<KeyValuePair<string, HealthReportEntry>>) (pair =>
            {
                writer.WriteStartObject(pair.Key);
                writer.WriteString(pair.Key + "_status", pair.Value.Status.ToString());
                writer.WriteString("description", pair.Value.Description);
                writer.WriteStartObject("data");
                pair.Value.Data.ToList<KeyValuePair<string, object>>().ForEach((Action<KeyValuePair<string, object>>) (dataItem => writer.WriteString(dataItem.Key, dataItem.Value.ToString())));
                writer.WriteEndObject();
                writer.WriteEndObject();
            }));
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(utf8Json.ToArray());
    }
}
