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
        protected override DiagnosticAnalyzer CreateDiagnosticAnalyzer()
            => new InternalUsageDiagnosticAnalyzer();

        [ConditionalFact]
        public Task Invocation_on_type_in_internal_namespace()
            => Test(
                "var x = typeof(object).GetMethod(nameof(object.ToString), Type.EmptyTypes).DisplayName();",
                "Microsoft.EntityFrameworkCore.Internal.MethodInfoExtensions",
                "DisplayName");

        [ConditionalFact]
        public Task Instantiation_on_type_in_internal_namespace()
            => Test(
                "new CoreSingletonOptions();",
                "Microsoft.EntityFrameworkCore.Internal.CoreSingletonOptions",
                "CoreSingletonOptions");

        [ConditionalFact]
        public async Task Base_type()
        {
            var source = @"
class MyClass : Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter {
    MyClass() : base(null, null) {}
}";

            var diagnostics = await GetDiagnosticsFullSourceAsync(source);

            Assert.Collection(
                diagnostics,
                diagnostic =>
                {
                    Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
                    Assert.Equal(
                        string.Format(
                            InternalUsageDiagnosticAnalyzer.MessageFormat,
                            "Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter"),
                        diagnostic.GetMessage());

                    var span = diagnostic.Location.SourceSpan;
                    Assert.Equal(
                        "Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter",
                        source[span.Start..span.End]);
                },
                diagnostic =>
                {
                    Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
                    Assert.Equal(
                        string.Format(
                            InternalUsageDiagnosticAnalyzer.MessageFormat,
                            "Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter"),
                        diagnostic.GetMessage());

                    var span = diagnostic.Location.SourceSpan;
                    Assert.Equal(": base(null, null)", source[span.Start..span.End]);
                });
        }

        [ConditionalFact]
        public Task Implemented_interface()
            => TestFullSource(
                @"
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

class MyClass : IDbSetSource {
    public object Create(DbContext context, Type type) => null;
    public object Create(DbContext context, string name, Type type) => null;
}",
                "Microsoft.EntityFrameworkCore.Internal.IDbSetSource",
                "MyClass");

        [ConditionalFact]
        public Task Access_property_with_internal_attribute()
            => Test(
                "var x = Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.RelationalServices.Count;",
                "Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.RelationalServices",
                "RelationalServices");

        [ConditionalFact]
        public Task Instantiation_with_ctor_with_internal_attribute()
            => Test(
                "new Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies(null, null);",
                "Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies",
                "Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies");

        [ConditionalFact]
        public Task Local_variable_declaration()
            => Test(
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager state = null;",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

        [ConditionalFact]
        public Task Generic_type_parameter_in_method_call()
            => Test(
                @"
void SomeGenericMethod<T>() {}

SomeGenericMethod<Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager>();",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager",
                "SomeGenericMethod<Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager>()");

        [ConditionalFact]
        public Task Typeof()
            => Test(
                "var t = typeof(Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager);",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

        [ConditionalFact]
        public Task Field_declaration()
            => TestFullSource(
                @"
class MyClass {
    private readonly Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager StateManager;
}",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

        [ConditionalFact]
        public Task Property_declaration()
            => TestFullSource(
                @"
class MyClass {
    private Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager StateManager { get; set; }
}",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

        [ConditionalFact]
        public Task Method_declaration_return_type()
            => TestFullSource(
                @"
class MyClass {
    private Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager Foo() => null;
}",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

        [ConditionalFact]
        public Task Method_declaration_parameter()
            => TestFullSource(
                @"
class MyClass {
    private void Foo(Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager stateManager) {}
}",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager",
                "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

        [ConditionalFact]
        public async Task No_warning_on_non_internal()
            => await AssertNoDiagnostics(
                @"
var a = new Microsoft.EntityFrameworkCore.Infrastructure.Annotatable();
var x = a.GetAnnotations();
");

        [ConditionalFact]
        public async Task No_warning_in_same_assembly()
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

        private async Task Test(
            string source,
            string expectedInternalApi,
            string expectedDiagnosticSpan)
        {
            var (diagnostics, fullSource) = await GetDiagnosticsAsync(source);
            var diagnostic = Assert.Single(diagnostics);

            Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                string.Format(InternalUsageDiagnosticAnalyzer.MessageFormat, expectedInternalApi),
                diagnostic.GetMessage());

            var span = diagnostic.Location.SourceSpan;
            Assert.Equal(expectedDiagnosticSpan, fullSource[span.Start..span.End]);
        }

        private async Task TestFullSource(
            string fullSource,
            string expectedInternalApi,
            string expectedDiagnosticSpan)
        {
            var diagnostics = await GetDiagnosticsFullSourceAsync(fullSource);
            var diagnostic = Assert.Single(diagnostics);

            Assert.Equal(InternalUsageDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                string.Format(InternalUsageDiagnosticAnalyzer.MessageFormat, expectedInternalApi),
                diagnostic.GetMessage());

            var span = diagnostic.Location.SourceSpan;
            Assert.Equal(expectedDiagnosticSpan, fullSource[span.Start..span.End]);
        }

        protected override Task<(Diagnostic[], string)> GetDiagnosticsAsync(string source, params string[] extraUsings)
            => base.GetDiagnosticsAsync(source, extraUsings.Concat(new[] { "Microsoft.EntityFrameworkCore.Internal" }).ToArray());
    }
}
