using FunBooksAndVideos.Api.Extensions;
using FunBooksAndVideos.Api.Middleware;
using FunBooksAndVideos.Application.Dto;
using FunBooksAndVideos.Application.Processing;
using FunBooksAndVideos.Application.Processing.Rules;
using FunBooksAndVideos.Infrastructure;
using FunBooksAndVideos.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IPurchaseOrderProcessor, PurchaseOrderProcessor>();
builder.Services.AddScoped<IPurchaseOrderRule, MembershipActivationRule>();
builder.Services.AddScoped<IPurchaseOrderRule, PhysicalProductShippingRule>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FunBooksDbContext>();
    await DatabaseInitializer.InitializeAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/purchase-orders", async (CreatePurchaseOrderRequestDto request, IPurchaseOrderProcessor processor, CancellationToken cancellationToken) =>
{
    var result = await processor.ProcessAsync(request, cancellationToken);
    return Results.Ok(result);
})
.WithName("CreatePurchaseOrder");

app.Run();
