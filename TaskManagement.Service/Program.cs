using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManagement.Data;
using TaskManagement.Service;

var builder = Host.CreateApplicationBuilder(args);

// Configure as Windows Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "TaskManagementService";
});

// Add DbContext
builder.Services.AddDbContext<TaskManagementDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure service settings
builder.Services.Configure<ServiceConfiguration>(builder.Configuration.GetSection("ServiceConfiguration"));

// Add hosted service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
