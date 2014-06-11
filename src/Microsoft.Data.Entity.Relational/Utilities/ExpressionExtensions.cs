// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

// ReSharper disable once CheckNamespace

namespace System.Linq
{
    public static class ExpressionExtensions
    {
        public static bool IsLogicalOperation([NotNull] this Expression expression)
        {
            Check.NotNull(expression, "expression");

            return expression.NodeType == ExpressionType.AndAlso
                   || expression.NodeType == ExpressionType.OrElse;
        }
    }
}
