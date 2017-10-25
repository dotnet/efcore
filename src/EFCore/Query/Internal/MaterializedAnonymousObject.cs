// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public struct MaterializedAnonymousObject
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsGetValueExpression(
            [NotNull] MethodCallExpression methodCallExpression,
            out QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            querySourceReferenceExpression = null;

            if (methodCallExpression.Object?.Type == typeof(MaterializedAnonymousObject)
                && methodCallExpression.Method.Equals(GetValueMethodInfo)
                && methodCallExpression.Object is QuerySourceReferenceExpression qsre)
            {
                querySourceReferenceExpression = qsre;

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly ConstructorInfo AnonymousObjectCtor
            = typeof(MaterializedAnonymousObject).GetTypeInfo()
                .DeclaredConstructors
                .Single(c => c.GetParameters().Length == 1);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo GetValueMethodInfo
            = typeof(MaterializedAnonymousObject).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetValue));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool operator ==(MaterializedAnonymousObject x, MaterializedAnonymousObject y) => x.Equals(y);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool operator !=(MaterializedAnonymousObject x, MaterializedAnonymousObject y) => !x.Equals(y);

        private readonly object[] _values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [UsedImplicitly]
        public MaterializedAnonymousObject([NotNull] object[] values) => _values = values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsDefault() => _values == null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is MaterializedAnonymousObject anonymousObject
                   && _values.SequenceEqual(anonymousObject._values);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return _values.Aggregate(
                    0,
                    (current, argument)
                        => current + ((current * 397) ^ (argument?.GetHashCode() ?? 0)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public object GetValue(int index) => _values[index];
    }
}
