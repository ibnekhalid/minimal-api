using Application.Company.Commands;
using Application.Company.Commands.ViewModel;
using Application.Company.Queries;

namespace Tm.Api.Endpoints;

public static partial class ApiEndpoints
{
    public static IEndpointRouteBuilder MapCompanyRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/companies", async ( ICompanyCommandService command,  ICompanyQueryService query,  CreateCompanyVm vm) =>
         {
             return await command.Create(vm);
         }).RequireAuthorization();

        builder.MapGet("/companies", async (ICompanyQueryService query) =>
        {
            return await query.Get();
        }).RequireAuthorization();

      
        return builder;
    }   
}
