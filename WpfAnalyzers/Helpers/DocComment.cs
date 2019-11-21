﻿namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DocComment
    {
        internal static (Location, string)? VerifyCref(XmlEmptyElementSyntax e, string nameMember)
        {
            if (e.IsCref(out var attribute))
            {
                if (attribute.Cref is NameMemberCrefSyntax { Name: IdentifierNameSyntax name })
                {
                    if (name.Identifier.ValueText == nameMember)
                    {
                        return null;
                    }

                    return (name.GetLocation(), nameMember);
                }
            }

            return (e.GetLocation(), $"<see cref=\"{nameMember}\"/>");
        }

        internal static bool IsCref(this XmlEmptyElementSyntax e, [NotNullWhen(true)] out XmlCrefAttributeSyntax? attribute)
        {
            if (e.Name?.LocalName.ValueText == "see")
            {
                foreach (var candidate in e.Attributes)
                {
                    if (candidate is XmlCrefAttributeSyntax cref)
                    {
                        attribute = cref;
                        return true;
                    }
                }
            }

            attribute = null;
            return false;
        }

        internal static bool IsMatch(this XmlTextSyntax xmlText, string text)
        {
            return xmlText.TextTokens.TrySingle(x => x.IsKind(SyntaxKind.XmlTextLiteralToken) && !string.IsNullOrWhiteSpace(x.ValueText), out var token) &&
                   Matches(token.ValueText);

            bool Matches(string valueText)
            {
                var index = valueText.IndexOf(text);
                if (index < 0)
                {
                    return false;
                }

                for (var i = 0; i < index; i++)
                {
                    if (!char.IsWhiteSpace(valueText[i]) &&
                        valueText[i] != '\\')
                    {
                        return false;
                    }
                }

                for (var i = index + text.Length + 1; i < valueText.Length; i++)
                {
                    if (!char.IsWhiteSpace(valueText[i]) &&
                        valueText[i] != '\\')
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal static bool TryMatch(this XmlElementSyntax e, [NotNullWhen(true)] out XmlTextSyntax? prefix, [NotNullWhen(true)] out XmlEmptyElementSyntax? cref, [NotNullWhen(true)] out XmlTextSyntax? suffix)
        {
            prefix = default;
            cref = default;
            suffix = default;
            return e is { Content: { Count: 3 } content } &&
                   Element(0, out prefix) &&
                   Element(1, out cref) &&
                   Element(2, out suffix);

            bool Element<T>(int index, out T? result)
                 where T : class
            {
                return (result = content[index] as T) is { };
            }
        }

        internal static bool TryMatch(this XmlElementSyntax e, [NotNullWhen(true)] out XmlTextSyntax? prefix, [NotNullWhen(true)] out XmlEmptyElementSyntax? cref, [NotNullWhen(true)] out XmlEmptyElementSyntax? paramref, [NotNullWhen(true)] out XmlTextSyntax? suffix)
        {
            prefix = default;
            cref = default;
            paramref = default;
            suffix = default;
            return e is { Content: { Count: 3 } content } &&
                   Element(0, out prefix) &&
                   Element(1, out cref) &&
                   Element(2, out paramref) &&
                   Element(3, out suffix);

            bool Element<T>(int index, out T? result)
                 where T : class
            {
                return (result = content[index] as T) is { };
            }
        }
    }
}