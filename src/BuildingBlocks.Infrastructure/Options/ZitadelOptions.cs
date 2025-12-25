using BuildingBlocks.Abstractions.Abstractions;

namespace BuildingBlocks.Infrastructure.Options
{
    public class ZitadelOptions : IOptions
    {
        public static string SectionName => "Auth:Zitadel";

        public required string Authority { get; init; }
        public required string Audience { get; init; }

    }
}
