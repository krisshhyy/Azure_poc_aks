using System.Diagnostics.CodeAnalysis;

namespace Api4;

public static class UserTransformer
{
    public static object Transform(JsonPlaceholderUser u) => new
    {
        id         = u.Id,
        name       = u.Name,
        email      = u.Email.ToLower(),
        phone      = StripPhoneExtension(u.Phone),
        city       = u.Address.City,
        company    = u.Company.Name,
        profileUrl = BuildProfileUrl(u.Id)
    };

    public static object TransformDetail(JsonPlaceholderUser u) => new
    {
        id         = u.Id,
        name       = u.Name,
        email      = u.Email.ToLower(),
        phone      = StripPhoneExtension(u.Phone),
        city       = u.Address.City,
        zipCode    = u.Address.Zipcode,
        company    = u.Company.Name,
        website    = u.Website,
        profileUrl = BuildProfileUrl(u.Id)
    };

    public static string StripPhoneExtension(string phone) =>
        phone.Split(' ')[0];

    public static string BuildProfileUrl(int userId) =>
        $"https://myapp.com/users/{userId}";
}

[ExcludeFromCodeCoverage] public record JsonPlaceholderUser(int Id, string Name, string Username, string Email, string Phone, string Website, Address Address, Company Company);
[ExcludeFromCodeCoverage] public record Address(string Street, string Suite, string City, string Zipcode);
[ExcludeFromCodeCoverage] public record Company(string Name, string CatchPhrase, string Bs);
