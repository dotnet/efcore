// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ModificationCommandComparer : IComparer<ModificationCommand>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int Compare(ModificationCommand x, ModificationCommand y)
        {
            var result = 0;
            if (ReferenceEquals(x, y))
            {
                return result;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            result = StringComparer.Ordinal.Compare(x.Schema, y.Schema);
            if (0 != result)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.TableName, y.TableName);
            if (0 != result)
            {
                return result;
            }

            result = (int)x.EntityState - (int)y.EntityState;
            if (0 != result)
            {
                return result;
            }

            if (x.EntityState != EntityState.Added
                && x.Entries.Count > 0
                && y.Entries.Count > 0)
            {
                var xEntry = x.Entries[0];
                var yEntry = y.Entries[0];

                var key = xEntry.EntityType.FindPrimaryKey();

                for (var i = 0; i < key.Properties.Count; i++)
                {
                    var keyProperty = key.Properties[i];
                    var compare = GetComparer(keyProperty.ClrType);

                    result = compare(xEntry.GetCurrentValue(keyProperty), yEntry.GetCurrentValue(keyProperty));
                    if (0 != result)
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        private readonly ConcurrentDictionary<Type, Func<object, object, int>> _comparers =
            new ConcurrentDictionary<Type, Func<object, object, int>>();

        protected virtual Func<object, object, int> GetComparer([NotNull] Type type)
            => _comparers.GetOrAdd(type, t =>
                {
                    var xParameter = Expression.Parameter(typeof(object), name: "x");
                    var yParameter = Expression.Parameter(typeof(object), name: "y");
                    return Expression.Lambda<Func<object, object, int>>(
                            Expression.Call(null, _compareMethod.MakeGenericMethod(t),
                                Expression.Convert(xParameter, t),
                                Expression.Convert(yParameter, t)),
                            xParameter,
                            yParameter)
                        .Compile();
                });

        private static readonly MethodInfo _compareMethod
            = typeof(ModificationCommandComparer).GetTypeInfo().GetDeclaredMethod(nameof(CompareValue));

        private static int CompareValue<T>(T x, T y) => Comparer<T>.Default.Compare(x, y);
    }
}
