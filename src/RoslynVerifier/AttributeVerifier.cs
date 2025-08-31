using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier;

public sealed class AttributeVerifier
{
    private readonly IReadOnlyCollection<AttributeSyntax> attributes;

    public AttributeVerifier(IReadOnlyCollection<AttributeSyntax> attributes)
    {
        this.attributes = attributes;
    }

    public void WithArguments(params object[] expectedArgs)
    {
        var filtered = this.attributes.Where(a => a.ArgumentList?.Arguments.Count == expectedArgs.Length);

        foreach (var attributeSyntax in filtered)
        {
            var zipped =
                attributeSyntax.ArgumentList!.Arguments.Zip(expectedArgs, (arg, expected) => (arg, expected));

            if (AllArgsEqual(zipped))
            {
                return;
            }
        }

        throw new VerifierException($"Expected attribute to have arguments '{string.Join(", ", expectedArgs)}'");

        bool AllArgsEqual(IEnumerable<(AttributeArgumentSyntax arg, object expected)> zipped)
        {
            foreach (var (arg, expected) in zipped)
            {
                var x = arg.ToFullString().Trim('"');
                var y = expected.ToString();

                switch (expected)
                {
                    case bool b:
                    {
                        if (!bool.TryParse(x, out var result)) return false;

                        if (result != b) return false;
                        break;
                    }
                    default:
                        if (!x.Equals(y)) return false;
                        break;
                }
            }

            return true;
        }
    }
}