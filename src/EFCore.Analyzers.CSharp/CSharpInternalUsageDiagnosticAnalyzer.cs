// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpInternalUsageDiagnosticAnalyzer : AbstractInternalUsageDiagnosticAnalyzer
    {
        protected override SyntaxNodeOrToken GetLocationForDiagnostic(SyntaxNode syntax)
        {
            return syntax switch
            {
                VariableDeclarationSyntax s => s.Type,
                ClassDeclarationSyntax s => s.Identifier,
                _ => syntax,
            };
        }

        protected override SyntaxNodeOrToken GetLocationForBaseTypeDiagnostic(SyntaxNode syntax)
        {
            return syntax switch
            {
                ClassDeclarationSyntax s when s.BaseList?.Types.Count > 0
                    => s.BaseList.Types[0],
                _ => syntax,
            };
        }

        protected override SyntaxNodeOrToken GetLocationForTypeDiagnostic(SyntaxNode syntax)
        {
            return syntax switch
            {
                MethodDeclarationSyntax s => s.ReturnType,
                ParameterSyntax s when s.Type != null => s.Type,
                _ => syntax,
            };
        }

        protected override SyntaxNodeOrToken NarrowDownSyntax(SyntaxNode syntax)
        {
            return syntax switch
            {
                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccessSyntax } => memberAccessSyntax.Name,
                MemberAccessExpressionSyntax s => s.Name,
                ObjectCreationExpressionSyntax s => s.Type,
                PropertyDeclarationSyntax s => s.Type,
                VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } => declaration.Type,
                TypeOfExpressionSyntax s => s.Type,
                _ => syntax,
            };
        }
    }
}
