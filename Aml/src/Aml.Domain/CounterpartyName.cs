using System.Text.RegularExpressions;

namespace Aml.Domain
{
    public static partial class CounterpartyName
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();

        public static string Normalize(string? raw)
        {
            
            return CleanWhitespace(raw).ToUpperInvariant();
            

        }

        public static string CleanWhitespace(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            return WhitespaceRegex().Replace(raw, " ").Trim();


        }
    }
}
