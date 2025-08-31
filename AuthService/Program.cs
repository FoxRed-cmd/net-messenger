using AuthService.Configs;
using AuthService.Data;
using AuthService.Middlewares;
using AuthService.Repositories;
using AuthService.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddSingleton<IRabbitMqProducerService, RabbitMqProducerService>();
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();

builder.Services.AddHostedService(sp => (RabbitMqProducerService)sp.GetRequiredService<IRabbitMqProducerService>());

var app = builder.Build();

app.UseErrorHandlingMiddleware();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
