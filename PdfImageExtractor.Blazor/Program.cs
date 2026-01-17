using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PdfImageExtractor.Blazor.Data;
using PdfImageExtractor.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddHubOptions(options =>
{
    options.MaximumReceiveMessageSize = 200 * 1024 * 1024; // 200MB
});
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<PdfService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
