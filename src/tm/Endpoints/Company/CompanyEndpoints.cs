using Application.Company.Commands;
using Application.Company.Commands.ViewModel;
using Application.Company.Queries;
using Common.MinimalValidator;

namespace Tm.Api.Endpoints;

internal static class CompanyEndpoints
{
    internal const string COMPNAY_GROUP = "Company";
    internal static IEndpointRouteBuilder MapCompanyRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/companies", async (ICompanyCommandService command, ICompanyQueryService query, IMinimalValidator minimalValidator, CreateCompanyVm vm) =>
        {
            var validationResult = minimalValidator.Validate(vm);
            if (!validationResult.IsValid) validationResult.ThorwInvalidArgumentsException();

            return await command.Create(vm);
        }).RequireAuthorization();

        builder.MapGet("/companies", async (ICompanyQueryService query) =>
        {
            return await query.Get();
        })
        .RequireAuthorization();


        return builder;
    }
}
