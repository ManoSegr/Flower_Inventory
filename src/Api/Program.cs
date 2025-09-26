using System.Globalization;
using FlowerShop.Application;
using FlowerShop.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

var enUS = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = enUS;
CultureInfo.DefaultThreadCurrentUICulture = enUS;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(enUS);
    options.SupportedCultures = new[] { enUS };
    options.SupportedUICultures = new[] { enUS };
});

builder.Services.AddDbContext<FlowerShopDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IFlowerService, FlowerService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseRequestLocalization(); 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
