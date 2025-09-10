// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.EntityFrameworkCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StringsUsageInRawQueriesDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor InterpolatedStringDescriptor
        = new(
            EFDiagnostics.InterpolatedStringUsageInRawQueries,
            title: AnalyzerStrings.InterpolatedStringUsageInRawQueriesAnalyzerTitle,
            messageFormat: AnalyzerStrings.InterpolatedStringUsageInRawQueriesMessageFormat,
            category: "Security",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor StringConcatenationDescriptor
        = new(
            EFDiagnostics.StringConcatenationUsageInRawQueries,
            title: AnalyzerStrings.StringConcatenationUsageInRawQueriesAnalyzerTitle,
            messageFormat: AnalyzerStrings.StringConcatenationUsageInRawQueriesMessageFormat,
            category: "Security",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [InterpolatedStringDescriptor, StringConcatenationDescriptor];

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

        var diagnosticDescriptor = targetMethod.Name switch
        {
            "FromSqlRaw" => AnalyzeFromSqlRawInvocation(invocation),
            "ExecuteSqlRaw" or "ExecuteSqlRawAsync" => AnalyzeExecuteSqlRawInvocation(invocation),
            "SqlQueryRaw" => AnalyzeSqlQueryRawInvocation(invocation),
            _ => null
        };

        if (diagnosticDescriptor is not null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    diagnosticDescriptor,
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

    internal static string GetReplacementMethodName(string oldName)
        => oldName switch
        {
            "FromSqlRaw" => "FromSql",
            "ExecuteSqlRaw" => "ExecuteSql",
            "ExecuteSqlRawAsync" => "ExecuteSqlAsync",
            "SqlQueryRaw" => "SqlQuery",
            _ => oldName
        };

    private static DiagnosticDescriptor? AnalyzeFromSqlRawInvocation(IInvocationOperation invocation)
    {
        var targetMethod = invocation.TargetMethod;
        Debug.Assert(targetMethod.Name == "FromSqlRaw");

        var compilation = invocation.SemanticModel!.Compilation;
        var correctFromSqlRaw = FromSqlRawMethod(compilation);

        Debug.Assert(correctFromSqlRaw is not null, "Unable to find original `FromSqlRaw` method");

        // Verify that the method is the one we analyze and its second argument, which corresponds to `string sql`, is...
        return correctFromSqlRaw is not null && targetMethod.ConstructedFrom.Equals(correctFromSqlRaw, SymbolEqualityComparer.Default)
            ? AnalyzeInvocation(invocation)
            : null;
    }

    private static DiagnosticDescriptor? AnalyzeExecuteSqlRawInvocation(IInvocationOperation invocation)
    {
        var targetMethod = invocation.TargetMethod;
        Debug.Assert(targetMethod.Name is "ExecuteSqlRaw" or "ExecuteSqlRawAsync");

        var compilation = invocation.SemanticModel!.Compilation;

        if (targetMethod.Name == "ExecuteSqlRaw")
        {
            var correctMethods = ExecuteSqlRawMethods(compilation);

            Debug.Assert(correctMethods.Any(), "Unable to find any `ExecuteSqlRaw` methods");

            if (!correctMethods.Contains(targetMethod.ConstructedFrom, SymbolEqualityComparer.Default))
            {
                return null;
            }
        }
        else
        {
            var correctMethods = ExecuteSqlRawAsyncMethods(compilation);

            Debug.Assert(correctMethods.Any(), "Unable to find any `ExecuteSqlRawAsync` methods");

            if (!correctMethods.Contains(targetMethod.ConstructedFrom, SymbolEqualityComparer.Default))
            {
                return null;
            }
        }

        // At this point assume that the method is correct since both `ExecuteSqlRaw` and `ExecuteSqlRawAsync` have multiple overloads.
        // Checking for every possible one is too much work for almost no gain.
        // So check whether the second argument, that corresponds to `string sql` parameter, is an interpolated string...
        return AnalyzeInvocation(invocation);
    }

    private static DiagnosticDescriptor? AnalyzeSqlQueryRawInvocation(IInvocationOperation invocation)
    {
        var targetMethod = invocation.TargetMethod;
        Debug.Assert(targetMethod.Name == "SqlQueryRaw");

        var compilation = invocation.SemanticModel!.Compilation;

        var correctSqlQueryRaw = SqlQueryRawMethod(compilation);

        Debug.Assert(correctSqlQueryRaw is not null, "Unable to find original `SqlQueryRaw` method");

        // Verify that the method is the one we analyze and its second argument, which corresponds to `string sql`, is...
        return correctSqlQueryRaw is not null && targetMethod.ConstructedFrom.Equals(correctSqlQueryRaw, SymbolEqualityComparer.Default)
            ? AnalyzeInvocation(invocation)
            : null;
    }

    private static DiagnosticDescriptor? AnalyzeInvocation(IInvocationOperation invocation)
    {
        // ...an interpolated string
        if (invocation.Arguments[1].Value is IInterpolatedStringOperation interpolatedString
            && AnalyzeInterpolatedString(interpolatedString))
        {
            return InterpolatedStringDescriptor;
        }

        // ...a string concatenation
        if (TryGetStringConcatenation(invocation.Arguments[1].Value, out var concatenation)
            && AnalyzeConcatenation(concatenation))
        {
            return StringConcatenationDescriptor;
        }

        return null;
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

    private static bool TryGetStringConcatenation(IOperation operation, [NotNullWhen(true)] out IBinaryOperation? concatenation)
    {
        if (operation is IBinaryOperation
            {
                OperatorKind: BinaryOperatorKind.Add,
                Type.SpecialType: SpecialType.System_String,
            } binaryOperation)
        {
            concatenation = binaryOperation;
            return true;
        }

        concatenation = default;
        return false;
    }

    private static bool AnalyzeConcatenation(IBinaryOperation operation)
    {
        var left = operation.LeftOperand;
        var right = operation.RightOperand;

        if ((left is IBinaryOperation leftBinary && AnalyzeConcatenation(leftBinary))
            || (right is IBinaryOperation rightBinary && AnalyzeConcatenation(rightBinary)))
        {
            return true;
        }

        return !left.ConstantValue.HasValue || !right.ConstantValue.HasValue;
    }

    private static IMethodSymbol? FromSqlRawMethod(Compilation compilation)
    {
        var type = compilation.RelationalQueryableExtensionsType();
        return (IMethodSymbol?)type?.GetMembers("FromSqlRaw").FirstOrDefault(s => s is IMethodSymbol);
    }

    private static IEnumerable<IMethodSymbol> ExecuteSqlRawMethods(Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return type?.GetMembers("ExecuteSqlRaw").Where(s => s is IMethodSymbol).Cast<IMethodSymbol>() ?? [];
    }

    private static IEnumerable<IMethodSymbol> ExecuteSqlRawAsyncMethods(Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return type?.GetMembers("ExecuteSqlRawAsync").Where(s => s is IMethodSymbol).Cast<IMethodSymbol>() ?? [];
    }

    private static IMethodSymbol? SqlQueryRawMethod(Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return (IMethodSymbol?)type?.GetMembers("SqlQueryRaw").FirstOrDefault(s => s is IMethodSymbol);
    }
}
