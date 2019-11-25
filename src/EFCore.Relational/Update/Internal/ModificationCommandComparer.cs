// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class ModificationCommandComparer : IComparer<ModificationCommand>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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

            var xState = x.EntityState;
            result = (int)xState - (int)y.EntityState;
            if (0 != result)
            {
                return result;
            }

            if (xState != EntityState.Added
                && x.Entries.Count > 0
                && y.Entries.Count > 0)
            {
                var xEntry = x.Entries[0];
                var yEntry = y.Entries[0];

                var xEntityType = xEntry.EntityType;
                var yEntityType = yEntry.EntityType;
                var xKey = xEntry.EntityType.FindPrimaryKey();
                var yKey = xKey;
                if (xEntityType != yEntityType)
                {
                    result = StringComparer.Ordinal.Compare(xEntityType.Name, yEntityType.Name);
                    if (0 != result)
                    {
                        return result;
                    }

                    result = StringComparer.Ordinal.Compare(xEntityType.DefiningNavigationName, yEntityType.DefiningNavigationName);
                    if (0 != result)
                    {
                        return result;
                    }

                    yKey = yEntry.EntityType.FindPrimaryKey();
                }

                for (var i = 0; i < xKey.Properties.Count; i++)
                {
                    var xKeyProperty = xKey.Properties[i];
                    var compare = GetComparer(xKeyProperty.ClrType);

                    result = compare(xEntry.GetCurrentValue(xKeyProperty), yEntry.GetCurrentValue(yKey.Properties[i]));
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual Func<object, object, int> GetComparer([NotNull] Type type)
            => _comparers.GetOrAdd(
                type, t =>
                {
                    var xParameter = Expression.Parameter(typeof(object), name: "x");
                    var yParameter = Expression.Parameter(typeof(object), name: "y");
                    return Expression.Lambda<Func<object, object, int>>(
                            Expression.Call(
                                null,
                                (typeof(IStructuralComparable).IsAssignableFrom(type)
                                    ? _structuralCompareMethod
                                    : _compareMethod)
                                .MakeGenericMethod(t),
                                Expression.Convert(xParameter, t),
                                Expression.Convert(yParameter, t)),
                            xParameter,
                            yParameter)
                        .Compile();
                });

        private static readonly MethodInfo _compareMethod
            = typeof(ModificationCommandComparer).GetTypeInfo().GetDeclaredMethod(nameof(CompareValue));

        private static int CompareValue<T>(T x, T y) => Comparer<T>.Default.Compare(x, y);

        private static readonly MethodInfo _structuralCompareMethod
            = typeof(ModificationCommandComparer).GetTypeInfo().GetDeclaredMethod(nameof(CompareStructureValue));

        private static int CompareStructureValue<T>(T x, T y)
        {
            if (x is Array array1
                && y is Array array2
                && array1.Length != array2.Length)
            {
                return array1.Length - array2.Length;
            }

            return StructuralComparisons.StructuralComparer.Compare(x, y);
        }
    }
}
