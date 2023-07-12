// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.EntityFrameworkCore;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InterpolatedStringUsageInRawQueriesCodeFixProvider))]
[Shared]
public sealed class InterpolatedStringUsageInRawQueriesCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(InterpolatedStringUsageInRawQueriesDiagnosticAnalyzer.Id);

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        // We report only 1 diagnostic per span, so this is ok
        var diagnostic = context.Diagnostics.First();

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root!.FindNode(diagnostic.Location.SourceSpan) is not SimpleNameSyntax simpleName)
        {
            Debug.Fail("Analyzer reported diagnostic not on a SimpleNameSyntax. This should never happen");
            return;
        }

        var invocationSyntax = simpleName.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (invocationSyntax is null)
        {
            return;
        }

        var foundInterpolation = false;

        // Not all reported by analyzer cases are fixable. If there is a mix of interpolated arguments and normal ones, e.g. `FromSqlRaw($"SELECT * FROM [Users] WHERE [Id] = {id}", id)`,
        // then replacing `FromSqlRaw` to `FromSqlInterpolated` creates compiler error since there is no overload for this.
        // We find such cases by walking through syntaxes of each argument and searching for first interpolated string. If there are arguments after it, we consider such case unfixable.
        foreach (var argument in invocationSyntax.ArgumentList.Arguments)
        {
            if (argument.Expression is InterpolatedStringExpressionSyntax)
            {
                foundInterpolation = true;
                continue;
            }

            if (!foundInterpolation)
            {
                continue;
            }

            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                AnalyzerStrings.InterpolatedStringUsageInRawQueriesCodeActionTitle,
                _ => Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(simpleName, GetReplacementName(simpleName)))),
                nameof(InterpolatedStringUsageInRawQueriesCodeFixProvider)),
            diagnostic);
    }

    private static SimpleNameSyntax GetReplacementName(SimpleNameSyntax oldName)
    {
        var oldNameToken = oldName.Identifier;
        var oldMethodName = oldNameToken.ValueText;

        var replacementMethodName = InterpolatedStringUsageInRawQueriesDiagnosticAnalyzer.GetReplacementMethodName(oldMethodName);
        Debug.Assert(replacementMethodName != oldMethodName, "At this point we must find correct replacement name");

        var replacementToken = SyntaxFactory.Identifier(replacementMethodName).WithTriviaFrom(oldNameToken);
        return oldName.WithIdentifier(replacementToken);
    }
}
