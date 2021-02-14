// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    internal static class IOperationExtensions
    {
        private static readonly ImmutableArray<OperationKind> s_LambdaAndLocalFunctionKinds =
            ImmutableArray.Create(OperationKind.AnonymousFunction, OperationKind.LocalFunction);

        public static bool IsWithinExpressionTree(this IOperation operation, INamedTypeSymbol linqExpressionTreeType)
            => linqExpressionTreeType != null
                && operation.GetAncestor(s_LambdaAndLocalFunctionKinds)?.Parent?.Type?.OriginalDefinition is { } lambdaType
                && linqExpressionTreeType.Equals(lambdaType, SymbolEqualityComparer.Default);

        /// <summary>
        /// Gets the first ancestor of this operation with:
        ///  1. Any OperationKind from the specified <paramref name="ancestorKinds"/>.
        ///  2. If <paramref name="predicate"/> is non-null, it succeeds for the ancestor.
        /// Returns null if there is no such ancestor.
        /// </summary>
        public static IOperation? GetAncestor(this IOperation root, ImmutableArray<OperationKind> ancestorKinds, Func<IOperation, bool>? predicate = null)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            var ancestor = root;
            do
            {
                ancestor = ancestor.Parent;
            } while (ancestor != null && !ancestorKinds.Contains(ancestor.Kind));

            if (ancestor != null)
            {
                if (predicate != null && !predicate(ancestor))
                {
                    return GetAncestor(ancestor, ancestorKinds, predicate);
                }
                return ancestor;
            }
            else
            {
                return default;
            }
        }


    }
}
