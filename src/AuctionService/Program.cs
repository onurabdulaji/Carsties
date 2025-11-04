using System.Reflection;
using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var auctionAssembly = Assembly.GetExecutingAssembly();

// Add services to the container.

builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(config =>
{
    config.LicenseKey = "";
}, auctionAssembly);

builder.Services.AddMassTransit(q =>
{
    q.AddEntityFrameworkOutbox<AuctionDbContext>(options =>
    {
        options.QueryDelay = TimeSpan.FromSeconds(10);
        options.UsePostgres();
        options.UseBusOutbox();
    });

    q.AddConsumersFromNamespaceContaining<AuctionCreatedFaultsConsumer>();

    q.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    q.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();
