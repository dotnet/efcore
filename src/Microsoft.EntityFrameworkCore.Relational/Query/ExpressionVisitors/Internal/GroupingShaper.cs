// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class GroupingShaper : Shaper, IShaper<IGrouping<ValueBuffer, ValueBuffer>>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public GroupingShaper([NotNull] GroupResultOperator groupResultOperator)
            : base(groupResultOperator)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type Type => typeof(IGrouping<ValueBuffer, ValueBuffer>);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IGrouping<ValueBuffer, ValueBuffer> Shape(QueryContext queryContext, ValueBuffer valueBuffer)
        {
            return new ValueBufferGrouping(valueBuffer, valueBuffer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression CreateValueBufferAccessExpression(
            [NotNull] Expression valueBufferGroupingExpression)
        {
            return Expression.MakeMemberAccess(
                Expression.Convert(
                    valueBufferGroupingExpression,
                    typeof(ValueBufferGrouping)),
                _valueBufferGroupingValueBufferProperty);
        }

        private static readonly PropertyInfo _valueBufferGroupingValueBufferProperty
            = typeof(ValueBufferGrouping).GetTypeInfo().GetDeclaredProperty("ValueBuffer");
        
        private struct ValueBufferGrouping : IGrouping<ValueBuffer, ValueBuffer>
        {
            private ValueBuffer _element;

            public ValueBufferGrouping(ValueBuffer key, ValueBuffer element)
            {
                Key = key;
                _element = element;
            }

            public ValueBuffer Key { get; }

            public ValueBuffer ValueBuffer => Key;

            public IEnumerator<ValueBuffer> GetEnumerator() => Enumerable.Repeat(_element, 1).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
