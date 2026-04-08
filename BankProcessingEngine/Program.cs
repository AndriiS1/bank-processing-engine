using Application.Extensions;
using Application.Queries;
using Application.Requests;
using BankProcessingEngine.Extensions;
using Infrastructure.Extensions;
using MediatR;

namespace BankProcessingEngine;

public static class Program
{
    private record CreatePaymentRequest(Guid UserId, long Amount);
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddCors();
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
        app.UseCors(policyBuilder => 
        {
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
        
        app.MapPost("/payments", async (CreatePaymentRequest request, IMediator mediator) =>
        {
            var command = new CreatePaymentCommand(request.UserId, request.Amount);
            var result = await mediator.Send(command);
            return result ? Results.Ok() : Results.BadRequest("Payment failed");
        });
        
        app.MapPost("/payments/publish", async (IMediator mediator) =>
        {
             await mediator.Send(new PublishPaymentsCommand());
        });
        
        app.MapGet("/users", async (IMediator mediator) => await mediator.Send(new GetUsersQuery()));
        
        app.MapGet("/outbox-messages", async (IMediator mediator) => await mediator.Send(new GetOutboxMessagesQuery()));

        app.Run();
    }
}
