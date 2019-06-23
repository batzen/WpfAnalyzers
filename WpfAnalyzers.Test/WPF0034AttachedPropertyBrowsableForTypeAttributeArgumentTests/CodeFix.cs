namespace WpfAnalyzers.Test.WPF0034AttachedPropertyBrowsableForTypeAttributeArgumentTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new AttachedPropertyBrowsableForTypeArgumentFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0034AttachedPropertyBrowsableForTypeAttributeArgument.Descriptor);

        [Test]
        public void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetValue(UIElement element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        [AttachedPropertyBrowsableForType(↓typeof(DependencyObject))]
        public static int GetValue(UIElement element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Use [AttachedPropertyBrowsableForType(typeof(UIElement)]"), testCode);
        }

        [Test]
        public void WhenWrongType()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetValue(UIElement element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        [AttachedPropertyBrowsableForType(↓typeof(DependencyObject))]
        public static int GetValue(UIElement element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetValue(UIElement element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int GetValue(UIElement element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
