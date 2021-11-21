using Common.Environment;
using Common.MinimalValidator;
using Core.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Persistent;
using System.Text;
using Tm.Api.Endpoints;
using Tm.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
#region Services
// Add services to the container.
var Types = AssemblyTypesBuilder.GetAllExecutingContextTypes();

builder.Environment.SetDefault();
var isDebug = builder.Environment.IsDev();

var conStr = builder.Configuration.GetConnectionString("SqlServer");

builder.Services.AddAppSettings(builder.Configuration)
    .RegisterCommandQueryDbContext(conStr)
    .RegisterCommandsAndQueryServices(Types)
    .RegisterRepositories(Types);
builder.Services.AddIdentity<Core.Model.User, Role>()
    .AddEntityFrameworkStores<BaseContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // User settings
    options.User.RequireUniqueEmail = true;

});

#region Add CORS  
builder.Services.AddCors(options => options.AddPolicy("Cors", builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
}));
#endregion

#region Add Authentication  
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Tokens:Key"]));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;

    config.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = signingKey,
        ValidateAudience = true,
        RequireExpirationTime = true,
        //LifetimeValidator = new LifetimeValidator(),
        ValidAudience = builder.Configuration["Tokens:Audience"],
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Tokens:Issuer"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();
#endregion
#region Swagger Open API Doc
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OrderActionsBy(x => x.GroupName);
    options.SwaggerDoc("v1", new() { Title = "TM API", Version = "v1" });
});
#endregion

builder.Services.AddScoped<IMinimalValidator, MinimalValidator>();
#endregion

var app = builder.Build();

#region Configurations
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.UseAuthentication();

#region Endpoints
app.MapCompanyRoutes();
app.MapAccountRoutes();
#endregion

#endregion

app.Run();
