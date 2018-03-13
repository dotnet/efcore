// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore
{
    internal static class Extensions
    {
        [CanBeNull]
        public static ISymbol GetSymbol(this SyntaxNodeAnalysisContext analysisContext, SyntaxNode syntaxNode)
            => analysisContext.SemanticModel
                .GetSymbolInfo(syntaxNode, analysisContext.CancellationToken)
                .Symbol;
    }
}
