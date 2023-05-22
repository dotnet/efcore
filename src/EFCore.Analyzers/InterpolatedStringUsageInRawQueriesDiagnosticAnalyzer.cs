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
                targetMethod.Name));
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

    private static bool AnalyzeFromSqlRawInvocation(IInvocationOperation invocation)
    {
        var targetMethod = invocation.TargetMethod;
        Debug.Assert(targetMethod.Name == "FromSqlRaw");

        // Correct `FromSqlRaw` is an extension method, therefore it must be static
        if (!targetMethod.IsStatic)
        {
            return false;
        }

        var compilation = invocation.SemanticModel!.Compilation;

        // Correct `FromSqlRaw` method must return IQueryable<T> type
        if (!targetMethod.ReturnType.OriginalDefinition.Equals(compilation.IQueryableOfTType(), SymbolEqualityComparer.Default))
        {
            return false;
        }

        var parameters = targetMethod.Parameters;

        // Correct `FromSqlRaw` method must have 3 parameters
        if (parameters.Length != 3)
        {
            return false;
        }

        var firstParameter = parameters[0];
        var secondParameter = parameters[1];
        var thirdParameter = parameters[2];

        // Correct first parameter is a DbSet<T> type
        if (!firstParameter.Type.OriginalDefinition.Equals(compilation.DbSetType(), SymbolEqualityComparer.Default))
        {
            return false;
        }

        // Correct second parameter type is a string
        if (secondParameter.Type.SpecialType != SpecialType.System_String)
        {
            return false;
        }

        // Correct third parameter is `params object[]`
        if (!IsParamsObjectArray(thirdParameter))
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

        // Both methods is an extension ones, therefore target method must be static
        if (!targetMethod.IsStatic)
        {
            return false;
        }

        var compilation = invocation.SemanticModel!.Compilation;

        // Both `ExecuteSqlRaw` and `ExecuteSqlRawAsync` must return named types
        if (targetMethod.ReturnType is not INamedTypeSymbol returnType)
        {
            return false;
        }

        if (targetMethod.Name == "ExecuteSqlRawAsync")
        {
            // Correct `ExecuteSqlRawAsync` must return Task<int> type. We check for Task and unwrap it to check for int later
            if (!returnType.OriginalDefinition.Equals(compilation.TaskOfTType(), SymbolEqualityComparer.Default) ||
                returnType.TypeArguments[0] is not INamedTypeSymbol unWrappedType)
            {
                return false;
            }

            returnType = unWrappedType;
        }

        // Now check for `int`. For `ExecuteSqlRaw` this is the actual check and for `ExecuteSqlRawAsync` this is a check of unwrapped type
        if (returnType.SpecialType != SpecialType.System_Int32)
        {
            return false;
        }

        var parameters = targetMethod.Parameters;

        // Both methods must have 3 or more parameters
        if (parameters.Length < 3)
        {
            return false;
        }

        var firstParameter = parameters[0];
        var secondParameter = parameters[1];

        // Correct first parameter is a DatabaseFacade type
        if (!firstParameter.Type.OriginalDefinition.Equals(compilation.DatabaseFacadeType(), SymbolEqualityComparer.Default))
        {
            return false;
        }

        // Correct second parameter type is a string
        if (secondParameter.Type.SpecialType != SpecialType.System_String)
        {
            return false;
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

        // Correct `SqlQueryRaw` is an extension method, therefore it must be static
        if (!targetMethod.IsStatic)
        {
            return false;
        }

        var compilation = invocation.SemanticModel!.Compilation;

        // Correct `SqlQueryRaw` method must return IQueryable<T> type
        if (!targetMethod.ReturnType.OriginalDefinition.Equals(compilation.IQueryableOfTType(), SymbolEqualityComparer.Default))
        {
            return false;
        }

        var parameters = targetMethod.Parameters;

        // Correct `SqlQueryRaw` method must have 3 parameters
        if (parameters.Length != 3)
        {
            return false;
        }

        var firstParameter = parameters[0];
        var secondParameter = parameters[1];
        var thirdParameter = parameters[2];

        // Correct first parameter is a DatabaseFacade type
        if (!firstParameter.Type.OriginalDefinition.Equals(compilation.DatabaseFacadeType(), SymbolEqualityComparer.Default))
        {
            return false;
        }

        // Correct second parameter type is a string
        if (secondParameter.Type.SpecialType != SpecialType.System_String)
        {
            return false;
        }

        // Correct third parameter is `params object[]`
        if (!IsParamsObjectArray(thirdParameter))
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

    private static bool IsParamsObjectArray(IParameterSymbol parameter)
        => parameter.IsParams && parameter.Type is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Object, Rank: 1 };

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
