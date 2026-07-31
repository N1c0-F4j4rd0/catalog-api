namespace CatalogApi.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(string username, IEnumerable<string> roles);
}
