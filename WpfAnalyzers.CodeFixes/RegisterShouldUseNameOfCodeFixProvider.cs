namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegisterShouldUseNameOfCodeFixProvider))]
    [Shared]
    internal class RegisterShouldUseNameOfCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0008RegisterShouldUseNameOf.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var nameArgument = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                  .FirstAncestorOrSelf<ArgumentSyntax>();
                if (nameArgument == null ||
                    nameArgument.IsMissing)
                {
                    continue;
                }

                if (nameArgument.Expression is LiteralExpressionSyntax literal)
                {
                    context.RegisterCodeFix(
                                            "Use nameof",
                                            (editor, cancellationToken) => ApplyFix(editor, nameArgument, diagnostic.Properties["SuggestedName"], cancellationToken),
                                            this.GetType(),
                                            diagnostic);
                }
            }
        }

        private static void ApplyFix(DocumentEditor editor, ArgumentSyntax argument, string name, CancellationToken cancellationToken)
        {
            editor.ReplaceNode(argument.Expression, (x, _) => SyntaxNodeExtensions.WithTriviaFrom(SyntaxFactory.ParseExpression($"nameof({name})"), x));
        }
    }
}
