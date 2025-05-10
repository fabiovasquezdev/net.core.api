using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using API.Middleware;
using API.Application;
using System;
using Hangfire;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddHealthChecks()
                .AddMongoDb(x => x.GetRequiredService<IMongoClient>());

builder.Services.AddCors(opt => opt.AddDefaultPolicy(builder =>
                                                        builder.AllowAnyHeader()
                                                               .AllowAnyMethod()
                                                               .AllowAnyOrigin()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHealthChecks("/health");

app.UseMiddleware<JwtMiddleware>();
app.UseHangfireDashboard();
app.MapControllers();
// Enable WebSocket
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromHours(24),
});


app.MapControllers();

app.Run();