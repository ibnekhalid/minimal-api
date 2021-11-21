using Application.Company.Commands;
using Application.Company.Commands.ViewModel;
using Application.Company.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tm.Api.Models;

namespace Tm.Api.Endpoints;

public static partial class ApiEndpoints
{
    public static IEndpointRouteBuilder MapAccountRoutes(this IEndpointRouteBuilder builder)
    {


        builder.MapPost("/accounts/login", async (IConfiguration configuration, SignInManager<Core.Model.User> signInManager, UserManager<Core.Model.User> userManager, LoginModel loginModel) =>
        {

            var loginResult = await signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, isPersistent: false, lockoutOnFailure: false);

            if (!loginResult.Succeeded)
                throw new BadHttpRequestException("Invalid username or password.");

            var user = await userManager.FindByNameAsync(loginModel.Username);

            return GetToken(user, configuration);

        });

        builder.MapPost("/accounts/register", async (IConfiguration configuration, ICompanyCommandService companyCommand,
            ICompanyQueryService companyQuery, SignInManager<Core.Model.User> signInManager, UserManager<Core.Model.User> userManager, RegisterModel registerModel) =>
        {

            var companyId = await companyCommand.Create(new CreateCompanyVm { Name = registerModel.CompanyName });
            var company = await companyQuery.Get(companyId);
            if (company is null) throw new Exception("Unable to create company.");
            var user = new Core.Model.User(company, registerModel.Email, registerModel.Username);

            var identityResult = await userManager.CreateAsync(user, registerModel.Password);

            if (!identityResult.Succeeded)
                throw new BadHttpRequestException("Invalid username or password.");

            await signInManager.SignInAsync(user, isPersistent: false);
            return GetToken(user, configuration);

        });



        builder.MapPost("/accounts/refresh-token", async (IConfiguration configuration, ClaimsPrincipal claims, UserManager<Core.Model.User> userManager) =>
        {
            var user = await userManager.FindByNameAsync(
                           claims.Identity.Name ??
                           claims.Claims.Where(c => c.Properties.ContainsKey("unique_name")).Select(c => c.Value).FirstOrDefault()
                           );
            return GetToken(user, configuration);

        });
        return builder;
    }
    private static string GetToken(Core.Model.User user, IConfiguration configuration)
    {
        var utcNow = DateTime.UtcNow;

        var claims = new Claim[]
        {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, utcNow.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("Tokens:Key")));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            signingCredentials: signingCredentials,
            claims: claims,
            notBefore: utcNow,
            expires: utcNow.AddSeconds(configuration.GetValue<int>("Tokens:Lifetime")),
            audience: configuration.GetValue<string>("Tokens:Audience"),
            issuer: configuration.GetValue<string>("Tokens:Issuer")
            );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
