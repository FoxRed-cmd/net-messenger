using ProfileService.Configs;
using ProfileService.Data;
using ProfileService.Repositories;
using ProfileService.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddDbContext<ProfileDbContext>();

builder.Services.AddScoped(typeof(ICrudRepository<,>), typeof(CrudRepository<,>));
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddSingleton<IProfileService, ProfileService.Services.ProfileService>();

builder.Services.AddHostedService<RabbitMqConsumerService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
