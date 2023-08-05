using EventsManager.Server;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
Startup.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

startup.Configure(app, builder.Environment, app.Services);
