// 'using' — imports the DI extension methods namespace so AddPaymentServices is callable
using Microsoft.Extensions.DependencyInjection;
// 'using' — imports the Payment.API namespace where DependencyInjection.AddPaymentServices lives
using Payment.API;
// 'using' — imports JWT Bearer authentication constants and extension methods
using Microsoft.AspNetCore.Authentication.JwtBearer;
// 'using' — imports ILoggerFactory and ILogger abstractions for structured logging
using Microsoft.Extensions.Logging;
// 'using' — imports TokenValidationParameters and SymmetricSecurityKey for JWT validation configuration
using Microsoft.IdentityModel.Tokens;
// 'using' — imports OpenApiInfo, OpenApiSecurityScheme, etc. for Swagger/OpenAPI documentation
using Microsoft.OpenApi.Models;
// 'using' — imports ClaimTypes constants (NameIdentifier, Role, Name) used to map JWT claims
using System.Security.Claims;
// 'using' — imports Encoding.UTF8 for converting the JWT signing key string to bytes
using System.Text;
// 'using' — imports DbContext and MigrateAsync for applying EF Core migrations at startup
using Microsoft.EntityFrameworkCore;

// WebApplication.CreateBuilder — factory method that sets up the host, configuration, logging, and DI container
var builder = WebApplication.CreateBuilder(args);

// AddPaymentServices — custom extension method (DependencyInjection.cs) that registers DB, repos, services, and consumers
builder.Services.AddPaymentServices(builder.Configuration);
// AddControllers — registers MVC controller services; discovers all [ApiController] classes
builder.Services.AddControllers()
    // AddJsonOptions — configures the System.Text.Json serialiser used for request/response bodies
    .AddJsonOptions(o =>
    {
        // CamelCase — serialises property names as camelCase (e.g. "orderId") to match JavaScript conventions
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // JsonStringEnumConverter — serialises enum values as their string names instead of integers
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // IstDateTimeJsonConverter — custom converter that serialises DateTime values in IST timezone
        o.JsonSerializerOptions.Converters.Add(new FoodFleet.Shared.Messaging.IstDateTimeJsonConverter());
        // IstNullableDateTimeJsonConverter — same but for nullable DateTime? properties
        o.JsonSerializerOptions.Converters.Add(new FoodFleet.Shared.Messaging.IstNullableDateTimeJsonConverter());
    });
// AddEndpointsApiExplorer — registers the metadata provider that Swagger uses to discover minimal-API endpoints
builder.Services.AddEndpointsApiExplorer();
// AddSwaggerGen — registers the Swagger/OpenAPI document generator
builder.Services.AddSwaggerGen(options =>
{
    // SwaggerDoc — defines the OpenAPI document version and metadata shown in the Swagger UI
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment API",
        Version = "v1",
        Description = "Food Fleet Payment Processing Service"
    });

    // AddSecurityDefinition — declares the "Bearer" JWT security scheme in the OpenAPI spec
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        // SecuritySchemeType.Http — tells Swagger this is an HTTP authentication scheme (not API key or OAuth)
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        // ParameterLocation.Header — the token is sent in the HTTP Authorization header
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token."
    });

    // AddSecurityRequirement — applies the Bearer scheme globally to all operations in the Swagger UI
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                // Reference — links this requirement to the "Bearer" scheme defined above
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            // Array.Empty<string>() — no OAuth scopes required; empty array is the correct value for JWT
            Array.Empty<string>()
        }
    });
});

// AddAuthentication — registers the authentication middleware and sets the default scheme to JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    // AddJwtBearer — configures the JWT Bearer handler with validation parameters
    .AddJwtBearer(options =>
    {
        // RequireHttpsMetadata = false — allows HTTP in development; set true in production
        options.RequireHttpsMetadata = false;
        // SaveToken = true — stores the raw JWT in the authentication properties for later retrieval
        options.SaveToken = true;
        // MapInboundClaims = false — prevents Microsoft's middleware from renaming standard JWT claims
        options.MapInboundClaims = false;
        // TokenValidationParameters — the rules the handler uses to accept or reject a JWT
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ValidateIssuer — checks the 'iss' claim matches the expected issuer
            ValidateIssuer = true,
            // ValidateAudience — checks the 'aud' claim matches the expected audience
            ValidateAudience = true,
            // ValidateLifetime — rejects tokens whose 'exp' (expiry) has passed
            ValidateLifetime = true,
            // ValidateIssuerSigningKey — verifies the token's signature using the shared secret
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // SymmetricSecurityKey — a shared-secret key; both issuer and validator use the same bytes
            // Encoding.UTF8.GetBytes — converts the string secret to a byte array for the key
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            // NameClaimType — maps the JWT claim used as the user's display name
            NameClaimType = ClaimTypes.Name,
            // RoleClaimType — maps the JWT claim used for role-based authorisation
            RoleClaimType = ClaimTypes.Role,
            // ClockSkew — allows a 5-minute tolerance for clock drift between servers
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        // JwtBearerEvents — hooks into the JWT pipeline for custom logic
        options.Events = new JwtBearerEvents
        {
            // OnMessageReceived — fires before the token is validated; used to extract the token from non-standard locations
            OnMessageReceived = context =>
            {
                // 'var' — implicitly typed string
                var authorization = context.Request.Headers.Authorization.ToString();
                // string.IsNullOrWhiteSpace — returns true if the string is null, empty, or only whitespace
                if (string.IsNullOrWhiteSpace(context.Token)
                    && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    // Range operator [..] — slices the string from "Bearer ".Length to the end, then trims whitespace
                    context.Token = authorization["Bearer ".Length..].Trim();
                }

                // Task.CompletedTask — a pre-completed Task; used when an async delegate has no async work to do
                return Task.CompletedTask;
            },
            // OnAuthenticationFailed — fires when JWT validation fails; used here to log the failure details
            OnAuthenticationFailed = context =>
            {
                // GetRequiredService<T> — resolves a service from the DI container; throws if not registered
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");

                // LogWarning — logs at Warning level; appropriate for security events that are not fatal
                logger.LogWarning(
                    context.Exception,
                    "JWT authentication failed for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                return Task.CompletedTask;
            }
        };
    });

// AddAuthorization — registers the authorisation middleware that enforces [Authorize] attributes
builder.Services.AddAuthorization();

// Build — finalises the service registrations and creates the WebApplication (the HTTP pipeline host)
var app = builder.Build();

// Auto-apply pending migrations on startup
// 'using' — creates a DI scope; scoped services (like DbContext) are resolved and disposed within this block
using (var scope = app.Services.CreateScope())
{
    // GetRequiredService<T> — resolves PaymentDbContext from the scoped DI container
    var db = scope.ServiceProvider
        .GetRequiredService<Payment.API.Infrastructure.Persistence.PaymentDbContext>();
    // MigrateAsync — applies any pending EF Core migrations to the database at startup
    // 'await' — waits for the async migration to complete before the app starts serving requests
    await db.Database.MigrateAsync();
}

// IsDevelopment — checks the ASPNETCORE_ENVIRONMENT variable; true when running locally
if (app.Environment.IsDevelopment())
{
    // UseSwagger — adds the middleware that serves the OpenAPI JSON document
    app.UseSwagger();
    // UseSwaggerUI — adds the middleware that serves the interactive Swagger HTML UI
    app.UseSwaggerUI();
}

// UseHttpsRedirection — redirects HTTP requests to HTTPS; skipped in development for convenience
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
// UseAuthentication — adds the authentication middleware; must come before UseAuthorization
app.UseAuthentication();
// UseAuthorization — adds the authorisation middleware; enforces [Authorize] attributes on controllers
app.UseAuthorization();
// MapControllers — registers all [ApiController] routes discovered by AddControllers()
app.MapControllers();
// Run — starts the Kestrel web server and blocks until the application shuts down
app.Run();
