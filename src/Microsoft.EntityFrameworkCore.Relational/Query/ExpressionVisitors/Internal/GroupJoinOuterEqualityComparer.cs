// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    /// 
    public class GroupJoinOuterEqualityComparer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static object Create(
            [NotNull] Type outerType, 
            [NotNull] Delegate accessor, 
            [NotNull] int[] indices)
        {
            Check.NotNull(outerType, nameof(outerType));
            Check.NotNull(accessor, nameof(accessor));
            Check.NotNull(indices, nameof(indices));

            return _createMethodInfo
                .MakeGenericMethod(outerType)
                .Invoke(null, new object[] { accessor, indices });
        }

        private static readonly MethodInfo _createMethodInfo
            = typeof(GroupJoinOuterEqualityComparer).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateMethod));

        [UsedImplicitly]
        private static TypedGroupJoinOuterEqualityComparer<TOuter> CreateMethod<TOuter>(
            Func<TOuter, ValueBuffer> accessor,
            int[] indices)
            => new TypedGroupJoinOuterEqualityComparer<TOuter>(accessor, indices);

        private class TypedGroupJoinOuterEqualityComparer<TOuter> : IEqualityComparer<TOuter>
        {
            private readonly Func<TOuter, ValueBuffer> _accessor;
            private readonly int[] _indices;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public TypedGroupJoinOuterEqualityComparer(Func<TOuter, ValueBuffer> accessor, int[] indices)
            {
                _accessor = accessor;
                _indices = indices;
            }

            public bool Equals(TOuter xOuter, TOuter yOuter)
            {
                var xBuffer = _accessor(xOuter);
                var yBuffer = _accessor(yOuter);

                for (var i = 0; i < _indices.Length; i++)
                {
                    var index = _indices[i];
                    var xValue = xBuffer[index];
                    var yValue = yBuffer[index];

                    if (xValue == null || yValue == null || !xValue.Equals(yValue))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(TOuter obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
