using BuildingBlocks.Host;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Extensions.JwtBearer;
using DotNetEnv;

if (File.Exists(".env"))
    Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

builder.Services.AddProblemDetails(options =>
    options.AddCustomConfiguration(builder.Environment));

builder.Services.AddOpenApi(options =>
{
    options.AddProblemDetailsSchemas();
});

builder.Services
    .AddAuthentication()
    .AddZitadelJwtBearer();

builder.Services.AddAuthorization();

builder.Services.AddUserContext();

builder.Services.RegisterModules(builder.Configuration, [new HostModule()]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseModules(builder.Configuration);

await app.Services.InitModules();
await app.RunAsync();
