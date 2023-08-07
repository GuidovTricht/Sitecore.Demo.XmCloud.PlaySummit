using Microsoft.AspNetCore.Localization;
using PlayWebsite.Configuration;
using Sitecore.AspNet.ExperienceEditor;
using Sitecore.AspNet.RenderingEngine.Extensions;
using Sitecore.AspNet.RenderingEngine.Localization;
using Sitecore.AspNet.Tracking;
using Sitecore.LayoutService.Client.Extensions;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var handler = builder.Configuration.GetSection(DefaultHandlerOptions.Key).Get<DefaultHandlerOptions>();
var experienceEditor = builder.Configuration.GetSection(ExperienceEditorConfiguration.Key).Get<ExperienceEditorConfiguration>();
var jssEditingSecret = builder.Configuration.GetValue<string>(ExperienceEditorConfiguration.JssEditingSecretKey);

builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });

//builder.Services
//    .AddSitecoreLayoutService()
//    .AddHttpHandler(handler.Name, handler.Uri)
//    .WithRequestOptions(request =>
//    {
//        foreach (var entry in handler.RequestDefaults)
//            request.Add(entry.Key, entry.Value);
//    })
//    .AsDefaultHandler();

builder.Services
    .AddSitecoreLayoutService()
    .AddGraphQlHandler(handler.Name, handler.SiteName, handler.ApiKey, handler.Uri)
    .AsDefaultHandler();

builder.Services.AddSitecoreRenderingEngine(options =>
    {
        options
            .AddModelBoundView<object>("Header")
            .AddModelBoundView<object>("MainNavigation")
            .AddDefaultPartialView("_ComponentNotFound");
    })
    // In Experience Editor, relative links to resources of Rendering Host may render incorrectly,
    // Rendering Host therefore replaces such links with absolute ones, when sending the rendered layout back to Experience Editor.
    // By default, when generating absolute links, the current request from Experience Editor is used to get the Rendering Host URL.
    // You can change this behavior by setting your custom URL in ExperienceEditorOptions.
    // .WithExperienceEditor(options =>
    // {
    //     options.ApplicationUrl = new Uri("https://[your custom URL]");
    // })
    // More details see in ExperienceEditorOptions documentation.
    .WithExperienceEditor(options =>
    {
        options.Endpoint = experienceEditor.Endpoint;
        options.JssEditingSecret = jssEditingSecret;
        //This is an example to show how we can target custom routes for the Experience Editor by adding custom mapping handlers.
        options.MapToRequest((sitecoreResponse, scPath, httpRequest) =>
            httpRequest.Path = scPath + sitecoreResponse?.Sitecore?.Route?.DatabaseName);
    })
    .WithTracking();

builder.Services.AddSitecoreVisitorIdentification(options =>
    {
        // Usually SitecoreInstanceHostName is same as Layout service but can be any Sitecore CD/CM instance which shares same AspNet session with Layout Service.
        // This Sitecore instance will be used for Visitor identification.
        var uriSetting = builder.Configuration.GetSection("Analytics:SitecoreInstanceUri").Get<Uri>();
        options.SitecoreInstanceUri = uriSetting ?? new Uri("https://SitecoreInstanceHostName");
    });

// This configuration necessary for proper resolving of IP address and Scheme of original request in case reverse proxies sends XForwarded sets of headers. See Tracking documentation for details.
// Uncomment if you expect to resolve x-forwarded headers.
//builder.Services.Configure<ForwardedHeadersOptions>(options =>
//{
//    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSitecoreExperienceEditor();
app.UseRouting();
app.UseHttpsRedirection();
app.UseStaticFiles();

// Make sure to resolve IP address before Rendering engine functionality. It will allow xDb to record real client IP address.
// Uncomment if you expect to resolve x-forwarded headers.
//app.UseForwardedHeaders();
app.UseSitecoreVisitorIdentification();

//Adds localization functionality
//Calling UseSitecoreRequestLocalization() on the localization  allows culture to be resolved from both the sc_lang query string and the culture token from route data.
app.UseRequestLocalization(options =>
{
    var supportedCultures = new List<CultureInfo> { new CultureInfo("en"), new CultureInfo("da"), new CultureInfo("da-DK") };
    options.DefaultRequestCulture = new RequestCulture(culture: "da", uiCulture: "da");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.UseSitecoreRequestLocalization();
});
app.UseSitecoreRenderingEngine();

app.MapSitecoreLocalizedRoute("Localized", "Index", "Sitecore");
app.MapFallbackToController("Index", "Sitecore");

app.Run();
