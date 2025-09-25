using API.Configurations;
using API.Filter;
using API.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

//// Cho phép config đọc từ Environment Variables
Env.Load("../API/env.env");
builder.Configuration.AddEnvironmentVariables();

// add config manager appsettings
builder.Services.ConfigureServices(builder.Configuration);
ConfigManager.CreateManager(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Cấu hình OData
builder.Services.AddControllers().AddOData(options =>
    options.Select().Filter().OrderBy().Expand().SetMaxTop(100).Count());

// Add session 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian chờ phiên
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Chắc chắn cookie có mặt
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 🔒 Chỉ gửi cookie qua HTTPS
    options.Cookie.SameSite = SameSiteMode.None; // ⚠️ Cho phép chia sẻ session giữa FE & BE khác domain
});

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>  // JWT Bearer
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = ConfigManager.gI().Issuer,
        ValidAudience = ConfigManager.gI().Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigManager.gI().SecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 👇 Đây là cách hoạt động cho cả API và SignalR
            var token = context.Request.Query["access_token"]; // dùng cho SignalR

            // fallback nếu muốn hỗ trợ cả cookie (cho API)
            if (string.IsNullOrEmpty(token))
                token = context.Request.Cookies["JwtToken"];

            if (!string.IsNullOrEmpty(token))
                context.Token = token;
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HttpContext.Response.Cookies.Delete("JwtToken");
            //   context.Response.Redirect("http://localhost:5173/login");  
            //      context.HandleResponse();
            await Task.CompletedTask;
        }
    };
})
//.AddGoogle(googleOptions =>  // Google OAuth
//{
//    googleOptions.ClientId = ConfigManager.gI().GoogleClientIp;
//    googleOptions.ClientSecret = ConfigManager.gI().GoogleClientSecert;
//    googleOptions.CallbackPath = new PathString(ConfigManager.gI().GoogleRedirectUri);
//    googleOptions.SaveTokens = true;
//})
.AddFacebook(facebookOptions =>  // Facebook OAuth
{
    facebookOptions.AppId = ConfigManager.gI().FacebookAppId;
    facebookOptions.AppSecret = ConfigManager.gI().FacebookAppSecret;
    facebookOptions.CallbackPath = new PathString(ConfigManager.gI().FacebookRedirectUri);
    facebookOptions.SaveTokens = true;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://localhost:5173", "http://localhost:5173", "https://g77-sep490-su25-ab4781.gitlab.io")  
                  .AllowCredentials() // Quan trọng để cookie hoạt động
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("Set-Cookie");
        });
});

// Đảm bảo rằng cookie không bị chặn trong ứng dụng hoặc trình duyệt
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;

});

builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});

// Add compression services
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // bật cả khi dùng HTTPS
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
        "application/json"
    });
});

// Tuỳ chỉnh cấp độ nén (Fastest, Optimal)
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.UseInlineDefinitionsForEnums();
    c.SchemaFilter<EnumSchemaFilter>();
    c.SchemaFilter<DateOnlySchemaFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token "
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
app.UseExceptionHandler(errorApp =>          // bắt mọi exception và trả về JSON format
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var message = app.Environment.IsDevelopment()
                ? error.Error.Message
                : "Unable to process the entity. Please try again or contact support.";

            var result = JsonSerializer.Serialize(new
            {
                success = false,
                message
            });
            await context.Response.WriteAsync(result);
        }
    });
});
app.MapHub<ExamHub>("/examHub");
app.MapHub<LogHub>("/logHub");
app.MapHub<NotifyHub>("/notifyHub");

app.UseResponseCompression();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        }
        else context.Response.StatusCode = 400;
    }
    else await next();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

