// Program.cs — Yaseer (Welcome push on first page open, once per browser)

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
// using Microsoft.AspNetCore.Builder.Extensions; // not needed, can be removed
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Yaseer.Data;
using Yaseer.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- Identity & DbContext ----------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

// ---------------------- MVC & RazorPages --------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();

// ---------------------- Firebase Admin setup ----------------------
var credsPath = Path.Combine(builder.Environment.ContentRootPath, "Keys", "service-account.json");
if (!File.Exists(credsPath))
    throw new FileNotFoundException("Firebase service account key not found.", credsPath);

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile(credsPath)
});

builder.Services.AddSingleton(FirebaseMessaging.DefaultInstance);

// ---------------------- Build App ----------------------
var app = builder.Build();

// ---------------------- Middleware ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ---------------------- MVC Routes ----------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ---------------------- Minimal API for Firebase ----------------------

// POST /api/token — welcome notification (once per browser)
app.MapPost("/api/token", async (
    [FromBody] TokenDto body,
    FirebaseMessaging messaging,
    IMemoryCache cache,
    ILoggerFactory lf) =>
{
    var logger = lf.CreateLogger("ApiToken");
    var token = body?.Token?.Trim();

    if (string.IsNullOrWhiteSpace(token))
        return Results.BadRequest(new { ok = false, error = "Missing FCM token" });

    // Keep last token for debug endpoint
    cache.Set("lastToken", token, TimeSpan.FromMinutes(15));

    // Server-side dedupe (inline: no static helper in top-level Program.cs)
    var sent = cache.GetOrCreate("sentTokens", entry =>
    {
        entry.SlidingExpiration = TimeSpan.FromHours(24);
        return new HashSet<string>(StringComparer.Ordinal);
    });

    if (sent.Contains(token))
        return Results.Ok(new { ok = true, alreadySent = true });

    var msg = new Message
    {
        Token = token,
        Notification = new Notification
        {
            Title = "Welcome to Yaseer",
            Body = "Thanks for enabling notifications!"
        }
    };

    try
    {
        var id = await messaging.SendAsync(msg);
        sent.Add(token);
        logger.LogInformation("Welcome sent (first time). Id={Id}", id);
        return Results.Ok(new { ok = true, id, alreadySent = false });
    }
    catch (FirebaseMessagingException ex)
    {
        logger.LogWarning("Firebase error: {Msg}", ex.Message);
        return Results.BadRequest(new
        {
            ok = false,
            error = ex.Message,
            errorCode = ex.ErrorCode.ToString(),
            msgCode = ex.MessagingErrorCode?.ToString(),
            tokenLen = token.Length
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Welcome push failed (unexpected).");
        return Results.Json(new { ok = false, error = ex.Message }, statusCode: 500);
    }
});

// POST /api/debug/send-welcome — manual resend
app.MapPost("/api/debug/send-welcome", async (
    [FromBody] TokenDto? body,
    FirebaseMessaging messaging,
    IMemoryCache cache,
    ILoggerFactory lf) =>
{
    var logger = lf.CreateLogger("DebugSendWelcome");
    var token = body?.Token?.Trim();

    if (string.IsNullOrWhiteSpace(token))
        cache.TryGetValue("lastToken", out token);

    if (string.IsNullOrWhiteSpace(token))
        return Results.BadRequest(new { ok = false, error = "No token available. Open the site and allow notifications first." });

    var msg = new Message
    {
        Token = token,
        Notification = new Notification
        {
            Title = "Welcome to Yaseer (Test)",
            Body = "Triggered from /api/debug/send-welcome"
        }
    };

    try
    {
        var id = await messaging.SendAsync(msg);
        logger.LogInformation("Debug Welcome sent. Id={Id}", id);
        return Results.Ok(new { ok = true, id, tokenLen = token.Length });
    }
    catch (FirebaseMessagingException ex)
    {
        logger.LogWarning("Firebase error: {Msg}", ex.Message);
        return Results.BadRequest(new
        {
            ok = false,
            error = ex.Message,
            errorCode = ex.ErrorCode.ToString(),
            msgCode = ex.MessagingErrorCode?.ToString(),
            tokenLen = token.Length
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Debug welcome failed (unexpected).");
        return Results.Json(new { ok = false, error = ex.Message }, statusCode: 500);
    }
});

app.Run();

// ---------------------- Types (allowed after top-level statements) -----------
public record TokenDto(string? Token);