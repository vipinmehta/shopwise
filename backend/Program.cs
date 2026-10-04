using System.Text;
using ECommerce.Api.Middleware;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Auth;
using ECommerce.Infrastructure.Integrations;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Seed;
using ECommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SeedSettings>(builder.Configuration.GetSection("Seed"));

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();

builder.Services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
builder.Services.AddSingleton<IOtpSender, ConsoleOtpSender>();
builder.Services.AddSingleton<IGoogleTokenValidator, DevGoogleTokenValidator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]!))
        };
    });
builder.Services.AddAuthorization();

if (!builder.Environment.IsDevelopment())
{
    var secret = builder.Configuration["Jwt:Secret"];
    if (string.IsNullOrWhiteSpace(secret) || secret.StartsWith("dev-only") || secret.Length < 32)
    {
        throw new InvalidOperationException("Set a strong Jwt:Secret (environment variable Jwt__Secret, 32+ characters) outside Development.");
    }
    if (builder.Configuration["Seed:AdminPassword"] == "Admin@123")
    {
        throw new InvalidOperationException("Set Seed__AdminPassword to a non-default value outside Development.");
    }
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    var seedOptions = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SeedSettings>>();
    await DataSeeder.SeedAsync(db, seedOptions);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
