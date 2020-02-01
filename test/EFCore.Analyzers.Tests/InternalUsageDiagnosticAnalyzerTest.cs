// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using EFCore.Analyzers.Test.TestUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InternalUsageDiagnosticAnalyzerTest : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer CreateDiagnosticAnalyzer() => new InternalUsageDiagnosticAnalyzer();

        [ConditionalFact]
        public async Task No_warning_on_ef_non_internal()
            => await AssertNoDiagnostics(
                @"
var a = new Microsoft.EntityFrameworkCore.Infrastructure.Annotatable();
var x = a.GetAnnotations();
");

        #region Namespace

        [ConditionalFact]
        public async Task Warning_on_ef_internal_namespace_invocation()
        {
            var (diagnostics, source) = await GetDiagnosticsAsync(
                @"var x = typeof(object).GetMethod(nameof(object.ToString), Type.EmptyTypes).DisplayName();");
            var diagnostic = diagnostics.Single();

            Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                string.Format(InternalUsageDiagnosticAnalyzer.MessageFormat, "Microsoft.EntityFrameworkCore.Internal.MethodInfoExtensions"),
                diagnostic.GetMessage());

            var span = diagnostic.Location.SourceSpan;
            Assert.Equal("DisplayName", source[span.Start..span.End]);
        }

        [ConditionalFact]
        public async Task Warning_on_ef_internal_namespace_instantiation()
        {
            var (diagnostics, source) = await GetDiagnosticsAsync(@"new CoreSingletonOptions();");
            var diagnostic = diagnostics.Single();

            Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                string.Format(InternalUsageDiagnosticAnalyzer.MessageFormat, "Microsoft.EntityFrameworkCore.Internal.CoreSingletonOptions"),
                diagnostic.GetMessage());

            var span = diagnostic.Location.SourceSpan;
            Assert.Equal("CoreSingletonOptions", source[span.Start..span.End]);
        }

        [ConditionalFact]
        public async Task Warning_on_ef_internal_namespace_subclass()
        {
            var source = @"
class MyClass : Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter {
    MyClass() : base (null, null) {}
}";

            var diagnostics = await GetDiagnosticsFullSourceAsync(source);
            var diagnostic = diagnostics.Single();

            Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                string.Format(
                    InternalUsageDiagnosticAnalyzer.MessageFormat, "Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter"),
                diagnostic.GetMessage());

            var span = diagnostic.Location.SourceSpan;
            Assert.Equal(
                "Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter",
                source[span.Start..span.End]);
        }

        [ConditionalFact]
        public async Task No_warning_on_ef_internal_namespace_in_same_assembly()
        {
            var diagnostics = await GetDiagnosticsFullSourceAsync(
                @"
namespace My.EntityFrameworkCore.Internal
{
    class MyClass
    {
        static internal void Foo() {}
    }
}

namespace Bar
{
    class Program
    {
        public void Main(string[] args) {
            My.EntityFrameworkCore.Internal.MyClass.Foo();
        }
    }
}");

            Assert.Empty(diagnostics);
        }

        #endregion Namespace

        #region Attribute

        [ConditionalFact]
        public async Task Warning_on_ef_internal_attribute_property_access()
        {
            var (diagnostics, source) = await GetDiagnosticsAsync(
                @"var x = Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.RelationalServices.Count;");
            //throw new Exception("FOO: " + string.Join(", ", diagnostics.Select(d => d.ToString())));
            var diagnostic = diagnostics.Single();

            Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                string.Format(
                    InternalUsageDiagnosticAnalyzer.MessageFormat,
                    "Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.RelationalServices"),
                diagnostic.GetMessage());

            var span = diagnostic.Location.SourceSpan;
            Assert.Equal("RelationalServices", source[span.Start..span.End]);
        }

        [ConditionalFact]
        public async Task Warning_on_ef_internal_name_instantiation()
        {
            var (diagnostics, source) =
                await GetDiagnosticsAsync(@"new Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies(null, null);");
            var diagnostic = diagnostics.Single();

            Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                string.Format(
                    InternalUsageDiagnosticAnalyzer.MessageFormat, "Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies"),
                diagnostic.GetMessage());

            var span = diagnostic.Location.SourceSpan;
            Assert.Equal(
                "new Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies(null, null)", source[span.Start..span.End]);
        }

        #endregion Attribute

        protected override Task<(Diagnostic[], string)> GetDiagnosticsAsync(string source, params string[] extraUsings)
            => base.GetDiagnosticsAsync(source, extraUsings.Concat(new[] { "Microsoft.EntityFrameworkCore.Internal" }).ToArray());
    }
}
