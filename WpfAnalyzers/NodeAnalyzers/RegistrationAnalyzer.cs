namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RegistrationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Descriptor,
            WPF0008RegisterShouldUseNameOf.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax registerCall
                && context.ContainingSymbol.IsStatic)
            {
                if (DependencyProperty.TryGetRegisterCall(registerCall, context.SemanticModel, context.CancellationToken, out var method)
                    || DependencyProperty.TryGetRegisterReadOnlyCall(registerCall, context.SemanticModel, context.CancellationToken, out method))
                {
                    ValidateWPF0007(context, method, registerCall);
                    ValidateWPF0008(context, method, registerCall);
                }
                else if (DependencyProperty.TryGetRegisterAttachedCall(registerCall, context.SemanticModel, context.CancellationToken, out method)
                    || DependencyProperty.TryGetRegisterAttachedReadOnlyCall(registerCall, context.SemanticModel, context.CancellationToken, out method))
                {
                    ValidateWPF0007(context, method, registerCall);
                }
            }
        }

        private static void ValidateWPF0007(SyntaxNodeAnalysisContext context, IMethodSymbol method, InvocationExpressionSyntax invocationExpressionSyntax)
        {
            if (method.TryFindParameter(KnownSymbol.ValidateValueCallback, out var parameter)
                && invocationExpressionSyntax.TryFindArgument(parameter, out var validateValueCallback)
                && Callback.TryGetName(validateValueCallback, KnownSymbol.ValidateValueCallback, context.SemanticModel, context.CancellationToken, out var callBackIdentifier, out _)
                && DependencyProperty.TryGetRegisteredName(invocationExpressionSyntax, context.SemanticModel, context.CancellationToken, out var registeredName)
                && !callBackIdentifier.Identifier.ValueText.IsParts("Validate", registeredName))
            {
                context.ReportDiagnostic(Diagnostic.Create(WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Descriptor, callBackIdentifier.GetLocation(), ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"Validate{registeredName}"), callBackIdentifier, $"Validate{registeredName}"));
            }
        }

        private static void ValidateWPF0008(SyntaxNodeAnalysisContext context, IMethodSymbol method, InvocationExpressionSyntax invocationExpressionSyntax)
        {
            if (method.TryFindParameter("name", out var nameParameter)
                && invocationExpressionSyntax.TryFindArgument(nameParameter, out var nameArgument)
                && nameArgument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
            {
                var suggestedName = GetSuggestedName();

                context.ReportDiagnostic(Diagnostic.Create(WPF0008RegisterShouldUseNameOf.Descriptor, nameArgument.GetLocation(), ImmutableDictionary<string, string>.Empty.Add("SuggestedName", $"{suggestedName}"), suggestedName, literalExpressionSyntax.Token.ValueText));
            }

            string GetSuggestedName()
            {
                var suggestedName = literalExpressionSyntax.Token.ValueText;

                if (BackingFieldOrProperty.TryCreate(context.ContainingSymbol, out var fieldOrProperty))
                {
                    suggestedName = TrimSuffix(fieldOrProperty.Name);
                }

                return suggestedName;

                string TrimSuffix(string fieldOrPropertyName)
                {
                    const string propertykeySuffix = "PropertyKey";
                    const string propertySuffix = "Property";

                    if (fieldOrPropertyName.EndsWith(propertykeySuffix))
                    {
                        return fieldOrPropertyName.Substring(0, fieldOrPropertyName.Length - propertykeySuffix.Length);
                    }

                    if (fieldOrPropertyName.EndsWith(propertySuffix))
                    {
                        return fieldOrPropertyName.Substring(0, fieldOrPropertyName.Length - propertySuffix.Length);
                    }

                    return fieldOrPropertyName;
                }
            }
        }
    }
}
