using Castle.DynamicProxy;
using ECommerceBackend.AOP;
using ECommerceBackend.BackgroundJobs;
using ECommerceBackend.Data;
using ECommerceBackend.Middlewares;
using ECommerceBackend.Services;
using ECommerceBackend.Services.Implementations;
using ECommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);



// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
builder.Services.AddHostedService<OrderBackgroundWorker>();
builder.Services.AddScoped<ISalesReportService, SalesReportService>();
builder.Services.AddHostedService<DailySalesBatchWorker>();
//builder.Services.AddHostedService<DailySalesBatchMThreadsWorker>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddSingleton<ProxyGenerator>();
builder.Services.AddSingleton<CacheAspect>();
builder.Services.AddTransient<IProductService>(provider =>
{
    var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
    var aspect = provider.GetRequiredService<CacheAspect>();

    var realService = new ProductService(
        provider.GetRequiredService<AppDbContext>(),
        provider // this is IServiceProvider
    );

    return proxyGenerator.CreateInterfaceProxyWithTarget<IProductService>(realService, aspect);
});




var app = builder.Build();

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     db.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
// }


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();
// app.UseMiddleware<RequestLoggingMiddleware>(); //! this is for logging... 
app.UseMiddleware<PerformanceMiddleware>(); //! this is for performance (time).

app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();


