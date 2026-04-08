using Application.Extensions;
using Application.Queries;
using BankProcessingEngine.Extensions;
using Infrastructure.Extensions;
using MediatR;

namespace BankProcessingEngine;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();

        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.ApplyMigrations();
        
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapPost("/payments", (HttpContext httpContext) =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                .ToArray();
            return forecast;
        });
        
        app.MapGet("/users", async (IMediator mediator, HttpContext _) => await mediator.Send(new GetUsersQuery()));
        
        app.MapGet("/outbox-messages", async (IMediator mediator, HttpContext _) => await mediator.Send(new GetOutboxMessagesQuery()));

        app.Run();
    }
}
