﻿namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0004ClrMethodShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0004";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR method for a DependencyProperty should match registered name.",
            messageFormat: "Method '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "CLR methods for accessing a DependencyProperty must have names matching the name the DependencyProperty is registered with.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var methodDeclaration = context.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null ||
                methodDeclaration.IsMissing)
            {
                return;
            }

            var method = context.ContainingSymbol as IMethodSymbol;
            if (method == null)
            {
                return;
            }

            if (ClrMethod.IsAttachedSetMethod(method, context.SemanticModel, context.CancellationToken, out IFieldSymbol setField))
            {
                CheckName(context, setField, method, methodDeclaration, "Set");
                return;
            }

            if (ClrMethod.IsAttachedGetMethod(method, context.SemanticModel, context.CancellationToken, out IFieldSymbol getField))
            {
                CheckName(context, getField, method, methodDeclaration, "Get");
            }
        }

        private static void CheckName(
            SyntaxNodeAnalysisContext context,
            IFieldSymbol dependencyProperty,
            IMethodSymbol method,
            MethodDeclarationSyntax methodDeclaration,
            string prefix)
        {
            if (DependencyProperty.TryGetRegisteredName(
    dependencyProperty,
    context.SemanticModel,
    context.CancellationToken,
    out string registeredName))
            {
                if (!method.Name.IsParts(prefix, registeredName))
                {
                    var identifier = methodDeclaration.Identifier;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor,
                            identifier.GetLocation(),
                            method.Name,
                            prefix + registeredName));
                }
            }
        }
    }
}