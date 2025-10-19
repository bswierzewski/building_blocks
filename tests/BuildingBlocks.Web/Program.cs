using BuildingBlocks.Modules.Users.Infrastructure;
using BuildingBlocks.Modules.Users.Infrastructure.Module;
using BuildingBlocks.Modules.Users.Infrastructure.Extensions;
using BuildingBlocks.Modules.Users.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Add HttpContextAccessor (required by UserService)
builder.Services.AddHttpContextAccessor();

// Configure Clerk authentication options
builder.Services.AddClerkOptions(builder.Configuration);

// Add authentication with Clerk JWT Bearer
builder.Services
    .AddAuthentication()
    .AddClerkJwtBearer();

// Add authorization services
builder.Services.AddAuthorization();

// Register Users module (includes Application and Infrastructure layers)
builder.Services.AddUsers(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Users module endpoints
app.MapUsersEndpoints();

app.Run();
