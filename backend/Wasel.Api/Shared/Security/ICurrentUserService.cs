namespace Wasel.Api.Shared.Security;

public interface ICurrentUserService
{
    string? KeycloakId { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
