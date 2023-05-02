// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Tests.Utilities;

namespace Microsoft.EntityFrameworkCore.Tests.Analyzers;

using VerifyCS = CSharpAnalyzerVerifier<InternalUsageDiagnosticAnalyzer>;

public class InternalUsageDiagnosticAnalyzerTests
{
    [ConditionalFact]
    public async Task Invocation_on_type_in_internal_namespace()
    {
        var source = """
            using System;
            using Microsoft.EntityFrameworkCore.Internal;

            class C
            {
                void M()
                {
                    var x = typeof(object).GetMethod(nameof(object.ToString), Type.EmptyTypes).{|#0:DisplayName|}();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.Internal.MethodInfoExtensions"));
    }

    [ConditionalFact]
    public async Task Instantiation_on_type_in_internal_namespace()
    {
        var source = """
            class C
            {
                void M()
                {
                    new {|#0:Microsoft.EntityFrameworkCore.Infrastructure.Internal.CoreSingletonOptions|}();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.Infrastructure.Internal.CoreSingletonOptions"));
    }

    [ConditionalFact]
    public async Task Base_type()
    {
        var source = """
            class MyClass : {|#0:Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter|}
            {
                MyClass() {|#1:: base(null, null)|} {}
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter"),
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(1)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter"));
    }

    [ConditionalFact]
    public async Task Implemented_interface()
    {
        var source = """
            using System;
            using Microsoft.EntityFrameworkCore;
            using Microsoft.EntityFrameworkCore.Internal;
            
            class {|#0:MyClass|} : IDbSetSource
            {
                public object Create(DbContext context, Type type) => null;
                public object Create(DbContext context, string name, Type type) => null;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.Internal.IDbSetSource"));
    }

    [ConditionalFact]
    public async Task Access_property_with_internal_attribute()
    {
        var source = """
            class C
            {
                void M()
                {
                    var x = Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.{|#0:RelationalServices|}.Count;
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.RelationalServices"));
    }

    [ConditionalFact]
    public async Task Instantiation_with_ctor_with_internal_attribute()
    {
        var source = """
            class C
            {
                void M()
                {
                    new {|#0:Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies|}(null, null);
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies"));
    }

    [ConditionalFact]
    public async Task Local_variable_declaration()
    {
        var source = """
            class C
            {
                void M()
                {
                    {|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} state = null;
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager"));
    }

    [ConditionalFact]
    public async Task Generic_type_parameter_in_method_call()
    {
        var source = """
            class C
            {
                void M()
                {
                    void SomeGenericMethod<T>() {}
            
                    {|#0:SomeGenericMethod<Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager>()|};
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager"));
    }

    [ConditionalFact]
    public async Task Typeof()
    {
        var source = """
            class C
            {
                void M()
                {
                    var t = typeof({|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|});
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager"));
    }

    [ConditionalFact]
    public async Task Field_declaration()
    {
        var source = """
            class MyClass
            {
                private readonly {|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} _stateManager;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager"));
    }

    [ConditionalFact]
    public async Task Property_declaration()
    {
        var source = """
            class MyClass
            {
                private {|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} StateManager { get; set; }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager"));
    }

    [ConditionalFact]
    public async Task Method_declaration_return_type()
    {
        var source = """
            class MyClass
            {
                private {|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} Foo() => null;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager"));
    }

    [ConditionalFact]
    public async Task Method_declaration_parameter()
    {
        var source = """
            class MyClass
            {
                private void Foo({|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} stateManager) {}
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments("Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager"));
    }

    [ConditionalFact]
    public async Task No_warning_on_non_internal()
        => await VerifyCS.VerifyAnalyzerAsync("""
            class C
            {
                void M()
                {
                    var a = new Microsoft.EntityFrameworkCore.Infrastructure.Annotatable();
                    var x = a.GetAnnotations();
                }
            }
            """);

    [ConditionalFact]
    public async Task No_warning_in_same_assembly()
        => await VerifyCS.VerifyAnalyzerAsync("""
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
            }
            """);
}
