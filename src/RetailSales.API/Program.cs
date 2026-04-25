using System.Text.Json.Serialization;
using RetailSales.API.Workers;
using RetailSales.API.Middleware;
using RetailSales.Infrastructure;
using RetailSales.Infrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using RetailSales.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSaleRequestValidator>();

// Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (DB, Repos, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Workers
builder.Services.AddHostedService<PaymentRetryWorker>();
builder.Services.AddHostedService<InvoicePrintWorker>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure Database is Created & Seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    await DbInitializer.SeedAsync(context);
}

app.Run();
