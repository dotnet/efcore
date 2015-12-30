// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Data.Entity.Internal
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeGenericMethodCodeFixProvider))]
    [Shared]
    public class MakeGenericMethodCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(MakeGenericMethodAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            context.RegisterCodeFix(
              CodeAction.Create(
                  title: AnalyzerStrings.AddAttribute,
                  createChangedDocument: c => AddAttribute(context.Document, root, methodDeclaration),
                  equivalenceKey: AnalyzerStrings.AddAttribute),
              diagnostic);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: AnalyzerStrings.IgnoreInspection,
                    createChangedDocument: c => AddIgnoreComment(context.Document, root, methodDeclaration),
                    equivalenceKey: AnalyzerStrings.IgnoreInspection),
                diagnostic);
        }

        private const string AttrNamespace = "Microsoft.Data.Entity.Internal";

        private async Task<Document> AddAttribute(Document document, SyntaxNode root, MethodDeclarationSyntax methodDeclaration)
        {
            var newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("GenericMethodFactory"), SyntaxFactory.ParseAttributeArgumentList("(MethodName = nameof(), TypeArguments = new [] { }, TargetType = typeof())"));
            var newAttributeLists = methodDeclaration.AttributeLists.Add(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { newAttribute })));
            var newMethodDecl = methodDeclaration.WithAttributeLists(newAttributeLists);
            var newRoot = root.ReplaceNode(methodDeclaration, newMethodDecl);

            var formatSpans = new List<TextSpan> { new TextSpan(newMethodDecl.SpanStart, methodDeclaration.Identifier.SpanStart - newMethodDecl.SpanStart) };

            var compilationUnit = newRoot.FirstAncestorOrSelf<CompilationUnitSyntax>();

            if (compilationUnit.Usings.All(u => u.Name.ToString() != AttrNamespace))
            {
                var newUsingsList = compilationUnit.Usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(AttrNamespace)));
                newRoot = newRoot.ReplaceNode(compilationUnit, compilationUnit.WithUsings(newUsingsList));
                formatSpans.Add(newUsingsList.FullSpan);
            }

            return await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot), formatSpans);
        }

        private async Task<Document> AddIgnoreComment(Document document, SyntaxNode root, MethodDeclarationSyntax methodDecl)
        {
            var newComment = SyntaxFactory.Comment("// " + AnalyzerStrings.DisableInspectionComment(nameof(MethodInfo.MakeGenericMethod)) + "\r\n");

            var triviaList = methodDecl.GetLeadingTrivia();
            var indentation = triviaList.Last(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
            var newTriviaList = triviaList.AddRange(new[] { newComment, indentation});
            var newMethodDecl = methodDecl.WithLeadingTrivia(newTriviaList);
            return document.WithSyntaxRoot(root.ReplaceNode(methodDecl, newMethodDecl));
        }
    }
}
