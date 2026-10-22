using PTO_web_application.Services;
using Supabase;
using PTO_web_application.Extensions;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════
// 1. Add MVC services
// ═══════════════════════════════════════════════════
builder.Services.AddControllersWithViews();

// ═══════════════════════════════════════════════════
// 2. Register Supabase client (server-side singleton)
// ═══════════════════════════════════════════════════
var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Supabase:Url is missing in appsettings.json");

var supabaseKey = builder.Configuration["Supabase:Key"]
    ?? throw new InvalidOperationException("Supabase:Key is missing in appsettings.json");

builder.Services.AddSingleton<Supabase.Client>(_ =>
    new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = false
    })
);

// ═══════════════════════════════════════════════════
// 3. Register your app services
// ═══════════════════════════════════════════════════
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<SiteService>();
builder.Services.AddScoped<DocumentService>();

// ═══════════════════════════════════════════════════
// 4. Session support (for login cookies)
// ═══════════════════════════════════════════════════
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ═══════════════════════════════════════════════════
// 5. Middleware pipeline
// ═══════════════════════════════════════════════════
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();           // ← IMPORTANT
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();