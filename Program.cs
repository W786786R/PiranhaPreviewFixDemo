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

// Explicitly registering services
//builder.Services.AddScoped<Piranha.Services.ISiteService, Piranha.Services.SiteService>();
//builder.Services.AddScoped<Piranha.Services.IPageService, Piranha.Services.PageService>();


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

// 1. Initializing Piranha first

app.UsePiranha(options =>
{
    App.Init(options.Api);

    new ContentTypeBuilder(options.Api)
        .AddAssembly(typeof(Program).Assembly)
        .Build()
        .DeleteOrphans();

    EditorConfig.FromFile("editorconfig.json");

    options.UseManager();
    options.UseTinyMCE();
    options.UseIdentity();
});


// 2. Custom preview middleware

app.Use(async (context, next) =>
{
    if (context.Request.Query.ContainsKey("draft"))
    {
        //var siteService = context.RequestServices.GetRequiredService<Piranha.Services.ISiteService>();
        //var sites = await siteService.GetAllAsync();
        var api = context.RequestServices.GetRequiredService<IApi>();
        var sites = await api.Sites.GetAllAsync();


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


// 3. Custom preview endpoint

app.Map("/manager/page/preview", async context =>
{
    var pageId = context.Request.Query["id"];
    var draft = context.Request.Query.ContainsKey("draft");

    //var pageService = context.RequestServices.GetRequiredService<Piranha.Services.IPageService>();
    var api = context.RequestServices.GetRequiredService<IApi>();
    if (Guid.TryParse(pageId, out var id))
    {
        //var page = await pageService.GetByIdAsync(id);
        var page = await api.Pages.GetByIdAsync(id);
        if (page != null)
        {
            var url = draft ? $"{page.Permalink}?draft=true" : page.Permalink;
            context.Response.Redirect(url);
            return;
        }
    }

    context.Response.StatusCode = 404;
});

app.Run();
