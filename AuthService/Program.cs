using AuthService.Configs;
using AuthService.Data;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb"));
});

builder.Services.Configure<RabbitMqConfig>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRabbitMqProducerService, RabbitMqProducerService>();
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.UseHttpsRedirection();


app.Run();
