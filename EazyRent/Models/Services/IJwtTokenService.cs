namespace EazyRent.Models.Services
{
    public interface IJwtTokenService
    {
        string GenerateJwtToken(string email, string role, int ownerId);
    }
}
