// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class VisualBasicInternalUsageDiagnosticAnalyzer : AbstractInternalUsageDiagnosticAnalyzer
    {
        protected override SyntaxNodeOrToken GetLocationForDiagnostic(SyntaxNode syntax)
        {
            return syntax switch
            {
                _ => syntax,
            };
        }

        protected override SyntaxNodeOrToken GetLocationForBaseTypeDiagnostic(SyntaxNode syntax)
        {
            return syntax switch
            {
                _ => syntax,
            };
        }

        protected override SyntaxNodeOrToken GetLocationForTypeDiagnostic(SyntaxNode syntax)
        {
            return syntax switch
            {
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
                TypeOfExpressionSyntax s => s.Type,
                _ => syntax,
            };
        }
    }
}
