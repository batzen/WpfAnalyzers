namespace WpfAnalyzers.Test.WPF0130UseTemplatePartAttributeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTemplateChildAnalyzer();
        private static readonly CodeFixProvider Fix = new AddTemplatePartAttributeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0130");

        [Test]
        public void CastStringLiteral()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)↓this.GetTemplateChild(""PART_Bar"");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(Border))]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void CastConstant()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)↓this.GetTemplateChild(PartBar);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(Border))]
    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(PartBar);
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
