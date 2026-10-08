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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToAsyncEnumerableOnQueryableCodeFixProvider)), Shared]
public sealed class ToAsyncEnumerableOnQueryableCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => [EFDiagnostics.ToAsyncEnumerableOnQueryable];

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        // The analyzer reports a single diagnostic per location.
        var diagnostic = context.Diagnostics.First();

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        if (root.FindNode(diagnostic.Location.SourceSpan) is not SimpleNameSyntax simpleName)
        {
            Debug.Fail("Analyzer reported diagnostic not on a SimpleNameSyntax. This should never happen");
            return;
        }

        // Skip the static-call form `AsyncEnumerable.ToAsyncEnumerable<T>(q)`. Fixing it would require a
        // structural rewrite to a different containing type (EntityFrameworkQueryableExtensions) or to
        // the instance form, which is beyond a pure name swap. The analyzer still warns; the user can
        // refactor manually.
        if (simpleName.Parent is MemberAccessExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.ValueText: "AsyncEnumerable" }
            })
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                AnalyzerStrings.ToAsyncEnumerableOnQueryableCodeActionTitle,
                _ => Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(simpleName, GetReplacementName(simpleName)))),
                nameof(ToAsyncEnumerableOnQueryableCodeFixProvider)),
            diagnostic);
    }

    private static SimpleNameSyntax GetReplacementName(SimpleNameSyntax oldName)
    {
        // Preserve trivia (comments/whitespace) and any generic type arguments — only the identifier changes.
        var newToken = SyntaxFactory.Identifier("AsAsyncEnumerable").WithTriviaFrom(oldName.Identifier);
        return oldName.WithIdentifier(newToken);
    }
}
