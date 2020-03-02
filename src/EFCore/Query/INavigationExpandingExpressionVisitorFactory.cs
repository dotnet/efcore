// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public interface INavigationExpandingExpressionVisitorFactory
    {
        NavigationExpandingExpressionVisitor Create(
            [NotNull] QueryTranslationPreprocessor queryTranslationPreprocessor,
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter);
    }
}
