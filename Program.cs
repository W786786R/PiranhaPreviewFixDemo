using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AttributeBuilder;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;

var builder = WebApplication.CreateBuilder(args);
// Add antiforgery service
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "XSRF-TOKEN"; // what Piranha preview JS expects
});

builder.AddPiranha(options =>
{
    // Hot reload of .cshtml during development
    options.AddRazorRuntimeCompilation = true;

    options.UseCms();
    options.UseManager();

    options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
    options.UseImageSharp();
    options.UseTinyMCE();
    options.UseMemoryCache();

    var connectionString = builder.Configuration.GetConnectionString("piranha");
    options.UseEF<SQLiteDb>(db => db.UseSqlite(connectionString));
    options.UseIdentityWithSeed<IdentitySQLiteDb>(db => db.UseSqlite(connectionString));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Middleware for handling `?draft=true` and multi-site preview paths
app.Use(async (context, next) =>
{
    if (context.Request.Query.ContainsKey("draft"))
    {
        var siteService = context.RequestServices.GetRequiredService<Piranha.Services.ISiteService>();
        var sites = await siteService.GetAllAsync();

        var path = context.Request.Path.Value?.Trim('/');
        foreach (var site in sites)
        {
            if (!string.IsNullOrEmpty(site.InternalId) &&
                path?.StartsWith(site.InternalId) == true)
            {
                context.Request.Path = $"/{site.InternalId}/{path}";
                break;
            }
        }
    }

    await next();
});

// Custom Preview Endpoint


/*app.Map("/manager/page/preview", async context =>
{
    var pageId = context.Request.Query["id"];
    var draft = context.Request.Query.ContainsKey("draft");

    var pageService = context.RequestServices.GetRequiredService<Piranha.Services.IPageService>();
    if (Guid.TryParse(pageId, out var id))
    {
        var page = await pageService.GetByIdAsync(id);
        if (page != null)
        {
            var url = draft ? $"{page.Permalink}?draft=true" : page.Permalink;
            context.Response.Redirect(url);
            return;
        }
    }

    context.Response.StatusCode = 404;
});*/

/*app.Map("/manager/page/preview/{id:guid}", async context =>
{
    var idStr = context.Request.RouteValues["id"]?.ToString();
    var draft = context.Request.Query.ContainsKey("draft");

    var pageService = context.RequestServices.GetRequiredService<Piranha.Services.IPageService>();
    if (Guid.TryParse(idStr, out var id))
    {
        var page = await pageService.GetByIdAsync(id);
        if (page != null)
        {
            var url = draft ? $"{page.Permalink}?draft=true" : page.Permalink;
            context.Response.Redirect(url);
            return;
        }
    }

    context.Response.StatusCode = 404;
});*/


// 3. Custom preview endpoint

app.Map("/manager/page/preview", async context =>
{
    var pageId = context.Request.Query["id"];
    var draft = context.Request.Query.ContainsKey("draft");

    var pageService = context.RequestServices.GetRequiredService<Piranha.Services.IPageService>();
    
    if (Guid.TryParse(pageId, out var id))
    {
        var page = await pageService.GetByIdAsync(id);
        
        if (page != null)
        {
            var url = draft ? $"{page.Permalink}?draft=true" : page.Permalink;
            context.Response.Redirect(url);
            return;
        }
    }

    context.Response.StatusCode = 404;
});







app.UsePiranha(options =>
{
    // Initialize Piranha
    App.Init(options.Api);

    // Build content types
    new ContentTypeBuilder(options.Api)
        .AddAssembly(typeof(Program).Assembly)
        .Build()
        .DeleteOrphans();

    // Configure Tiny MCE
    EditorConfig.FromFile("editorconfig.json");

    options.UseManager();
    options.UseTinyMCE();
    options.UseIdentity();
});

app.Run();