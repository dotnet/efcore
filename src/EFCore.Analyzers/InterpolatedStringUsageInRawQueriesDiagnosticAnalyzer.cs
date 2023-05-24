// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.EntityFrameworkCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InterpolatedStringUsageInRawQueriesDiagnosticAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "EF1002";

    private static readonly DiagnosticDescriptor Descriptor
        // HACK: Work around dotnet/roslyn-analyzers#5890 by not using target-typed new
        = new DiagnosticDescriptor(
            Id,
            title: AnalyzerStrings.InterpolatedStringUsageInRawQueriesAnalyzerTitle,
            messageFormat: AnalyzerStrings.InterpolatedStringUsageInRawQueriesMessageFormat,
            category: "Security",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Descriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzerInvocation, OperationKind.Invocation);
    }

    private void AnalyzerInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var targetMethod = invocation.TargetMethod;

        var report = targetMethod.Name switch
        {
            "FromSqlRaw" => AnalyzeFromSqlRawInvocation(invocation),
            "ExecuteSqlRaw" or "ExecuteSqlRawAsync" => AnalyzeExecuteSqlRawInvocation(invocation),
            "SqlQueryRaw" => AnalyzeSqlQueryRawInvocation(invocation),
            _ => false
        };

        if (report)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                GetTargetLocation(invocation.Syntax),
                targetMethod.Name,
                GetReplacementMethodName(targetMethod.Name)));
        }

        static Location GetTargetLocation(SyntaxNode syntax)
        {
            if (syntax is not InvocationExpressionSyntax invocationExpression)
            {
                Debug.Fail("In theory should never happen");
                return syntax.GetLocation();
            }

            var targetNode = invocationExpression.Expression;

            while (targetNode is MemberAccessExpressionSyntax memberAccess)
            {
                targetNode = memberAccess.Name;
            }

            // Generic name case, e.g. `db.Database.SqlQueryRaw<int>(...)`.
            // At this point `targetNode` is `SqlQueryRaw<int>`, but we need location of the actual identifier
            if (targetNode is GenericNameSyntax genericName)
            {
                return genericName.Identifier.GetLocation();
            }

            // We should appear at name expression, representing method name token, e.g.:
            // db.Users.[|FromSqlRaw|](...) or db.Database.[|ExecuteSqlRaw|](...)
            return targetNode.GetLocation();
        }
    }

    internal static string GetReplacementMethodName(string oldName) => oldName switch
    {
        "FromSqlRaw" => "FromSql",
        "ExecuteSqlRaw" => "ExecuteSql",
        "ExecuteSqlRawAsync" => "ExecuteSqlAsync",
        "SqlQueryRaw" => "SqlQuery",
        _ => oldName
    };

    private static bool AnalyzeFromSqlRawInvocation(IInvocationOperation invocation)
    {
        var targetMethod = invocation.TargetMethod;
        Debug.Assert(targetMethod.Name == "FromSqlRaw");

        var compilation = invocation.SemanticModel!.Compilation;
        var correctFromSqlRaw = compilation.FromSqlRawMethod();

        if (correctFromSqlRaw is null)
        {
            Debug.Fail("Unable to find original `FromSqlRaw` method");
            return false;
        }

        if (!targetMethod.ConstructedFrom.Equals(correctFromSqlRaw, SymbolEqualityComparer.Default))
        {
            return false;
        }

        // The second argument, that corresponds to `string sql` parameter, must be an interpolated string
        if (invocation.Arguments[1].Value is not IInterpolatedStringOperation interpolatedString)
        {
            return false;
        }

        // Report warning if interpolated string is not a constant and all its interpolations are not constants
        return AnalyzeInterpolatedString(interpolatedString);
    }

    private static bool AnalyzeExecuteSqlRawInvocation(IInvocationOperation invocation)
    {
        var targetMethod = invocation.TargetMethod;
        Debug.Assert(targetMethod.Name is "ExecuteSqlRaw" or "ExecuteSqlRawAsync");

        var compilation = invocation.SemanticModel!.Compilation;

        if (targetMethod.Name == "ExecuteSqlRaw")
        {
            var correctMethods = compilation.ExecuteSqlRawMethods();

            if (!correctMethods.Any())
            {
                Debug.Fail("Unable to find any `ExecuteSqlRaw` methods");
                return false;
            }

            if (!correctMethods.Contains(targetMethod.ConstructedFrom, SymbolEqualityComparer.Default))
            {
                return false;
            }
        }
        else
        {
            var correctMethods = compilation.ExecuteSqlRawAsyncMethods();

            if (!correctMethods.Any())
            {
                Debug.Fail("Unable to find any `ExecuteSqlRawAsync` methods");
                return false;
            }

            if (!correctMethods.Contains(targetMethod.ConstructedFrom, SymbolEqualityComparer.Default))
            {
                return false;
            }
        }

        // At this point assume that the method is correct since both `ExecuteSqlRaw` and `ExecuteSqlRawAsync` have multiple overloads.
        // Checking for every possible one is too much work for almost no gain.
        // So check whether the second argument, that corresponds to `string sql` parameter, is an interpolated string...
        if (invocation.Arguments[1].Value is not IInterpolatedStringOperation interpolatedString)
        {
            return false;
        }

        // ...and report warning if interpolated string is not a constant and all its interpolations are not constants
        return AnalyzeInterpolatedString(interpolatedString);
    }

    private static bool AnalyzeSqlQueryRawInvocation(IInvocationOperation invocation)
    {
        var targetMethod = invocation.TargetMethod;
        Debug.Assert(targetMethod.Name == "SqlQueryRaw");

        var compilation = invocation.SemanticModel!.Compilation;

        var correctSqlQueryRaw = compilation.SqlQueryRawMethod();

        if (correctSqlQueryRaw is null)
        {
            Debug.Fail("Unable to find original `SqlQueryRaw` method");
            return false;
        }

        if (!targetMethod.ConstructedFrom.Equals(correctSqlQueryRaw, SymbolEqualityComparer.Default))
        {
            return false;
        }

        // The second argument, that corresponds to `string sql` parameter, must be an interpolated string
        if (invocation.Arguments[1].Value is not IInterpolatedStringOperation interpolatedString)
        {
            return false;
        }

        // Report warning if interpolated string is not a constant and all its interpolations are not constants
        return AnalyzeInterpolatedString(interpolatedString);
    }

    private static bool AnalyzeInterpolatedString(IInterpolatedStringOperation interpolatedString)
    {
        if (interpolatedString.ConstantValue.HasValue)
        {
            return false;
        }

        foreach (var part in interpolatedString.Parts)
        {
            if (part is not IInterpolationOperation interpolation)
            {
                continue;
            }

            if (!interpolation.Expression.ConstantValue.HasValue)
            {
                // Found non-constant interpolation. Report it
                return true;
            }
        }

        return false;
    }
}
