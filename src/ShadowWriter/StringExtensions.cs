using System.Collections.Generic;
using System.Linq;

namespace ShadowWriter;

public static class StringExtensions
{
    public static string ToPascalCase(this string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        if (name.Length == 1) return name;
        var buffer = name.ToLowerInvariant();
        var firstChar = char.ToUpperInvariant(buffer[0]);

        return string.Concat(new[] { firstChar }.Concat(name.ToLowerInvariant().Skip(1)));
    }

    public static string ToValidPropertyName(this string name)
    {
        return string.Concat(Filter(name));

        static IEnumerable<char> Filter(string value)
        {
            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    yield return c;
                }
            }
        }
    }
}