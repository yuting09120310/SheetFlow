using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using SheetFlow.Infrastructure;
using SheetFlow.Repositories;
using SheetFlow.Services;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<DapperDbContext>(sp =>
    new DapperDbContext(builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFormTemplateRepository, FormTemplateRepository>();
builder.Services.AddScoped<IFormRequestRepository, FormRequestRepository>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
builder.Services.AddScoped<ExcelTemplateParser>();
builder.Services.AddScoped<IFormRequestService, FormRequestService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<INotificationService, EmailNotificationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
