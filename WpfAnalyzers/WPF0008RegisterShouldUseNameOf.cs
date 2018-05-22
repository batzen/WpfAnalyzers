namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0008RegisterShouldUseNameOf
    {
        public const string DiagnosticId = "WPF0008";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name of dependency property should be set using nameof.",
            messageFormat: "Use nameof({0}) instead of '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Name of dependency property should be set using ´nameof´. ´nameof´ is refactoring safe and should be preferred over strings.\r\n\r\nIf you register a dependency property using a string this rule will suggest using the name of the dependency properties backing field without the suffix \"Property\" or \"PropertyKey\".",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
