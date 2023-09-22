// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

using VerifyCS = CSharpAnalyzerVerifier<InternalUsageDiagnosticAnalyzer>;

public class InternalUsageDiagnosticAnalyzerTests
{
    [ConditionalFact]
    public Task Invocation_on_type_in_internal_namespace()
        => VerifySingleInternalUsageAsync(
            """
using System;
using Microsoft.EntityFrameworkCore.Internal;

class C
{
    void M()
    {
        var x = typeof(object).GetMethod(nameof(object.ToString), Type.EmptyTypes).{|#0:DisplayName|}();
    }
}
""", "Microsoft.EntityFrameworkCore.Internal.MethodInfoExtensions");

    [ConditionalFact]
    public Task Instantiation_on_type_in_internal_namespace()
        => VerifySingleInternalUsageAsync(
            """
class C
{
    void M()
    {
        new {|#0:Microsoft.EntityFrameworkCore.Infrastructure.Internal.CoreSingletonOptions|}();
    }
}
""", "Microsoft.EntityFrameworkCore.Infrastructure.Internal.CoreSingletonOptions");

    [ConditionalFact]
    public async Task Base_type()
    {
        var source = """
class MyClass : {|#0:Microsoft.EntityFrameworkCore.Storage.Internal.RawRelationalParameter|}
{
    MyClass() {|#1:: base(null, null)|} {}
}
""";

        await VerifyCS.VerifyAnalyzerAsync(
            source,
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
    public Task Implemented_interface()
        => VerifySingleInternalUsageAsync(
            """
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
            
class {|#0:MyClass|} : IDbSetSource
{
    public object Create(DbContext context, Type type) => null;
    public object Create(DbContext context, string name, Type type) => null;
}
""", "Microsoft.EntityFrameworkCore.Internal.IDbSetSource");

    [ConditionalFact]
    public Task Access_property_with_internal_attribute()
        => VerifySingleInternalUsageAsync(
            """
class C
{
    void M()
    {
        var x = Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.{|#0:RelationalServices|}.Count;
    }
}
""", "Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.RelationalServices");

    [ConditionalFact]
    public Task Instantiation_with_ctor_with_internal_attribute()
        => VerifySingleInternalUsageAsync(
            """
class C
{
    void M()
    {
        new {|#0:Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies|}(null, null);
    }
}
""", "Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorDependencies");

    [ConditionalFact]
    public Task Local_variable_declaration()
        => VerifySingleInternalUsageAsync(
            """
class C
{
    void M()
    {
        {|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} state = null;
    }
}
""", "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

    [ConditionalFact]
    public Task Generic_type_parameter_in_method_call()
        => VerifySingleInternalUsageAsync(
            """
class C
{
    void M()
    {
        void SomeGenericMethod<T>() {}
            
        {|#0:SomeGenericMethod<Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager>()|};
    }
}
""", "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

    [ConditionalFact]
    public Task Typeof()
        => VerifySingleInternalUsageAsync(
            """
class C
{
    void M()
    {
        var t = typeof({|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|});
    }
}
""", "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

    [ConditionalFact]
    public Task Field_declaration()
        => VerifySingleInternalUsageAsync(
            """
class MyClass
{
    private readonly {|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} _stateManager;
}
""", "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

    [ConditionalFact]
    public Task Property_declaration()
        => VerifySingleInternalUsageAsync(
            """
class MyClass
{
    private {|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} StateManager { get; set; }
}
""", "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

    [ConditionalFact]
    public Task Method_declaration_return_type()
        => VerifySingleInternalUsageAsync(
            """
class MyClass
{
    private {|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} Foo() => null;
}
""", "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

    [ConditionalFact]
    public Task Method_declaration_parameter()
        => VerifySingleInternalUsageAsync(
            """
class MyClass
{
    private void Foo({|#0:Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager|} stateManager) {}
}
""", "Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IStateManager");

    [ConditionalFact]
    public Task No_warning_on_non_internal()
        => VerifyCS.VerifyAnalyzerAsync(
            """
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
    public Task No_warning_in_same_assembly()
        => VerifyCS.VerifyAnalyzerAsync(
            """
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

    private Task VerifySingleInternalUsageAsync(string source, string internalApi)
        => VerifyCS.VerifyAnalyzerAsync(
            source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(AnalyzerStrings.InternalUsageMessageFormat)
                .WithArguments(internalApi));
}
