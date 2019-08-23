// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Value snapshotting and comparison logic for NetTopologySuite.Geometries.Geometry instances.
    /// </summary>
    public class GeometryValueComparer<TGeometry> : ValueComparer<TGeometry>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GeometryValueComparer{TGeometry}" /> class.
        /// </summary>
        public GeometryValueComparer()
            : base(
                GetEqualsExpression(),
                CreateDefaultHashCodeExpression(favorStructuralComparisons: false),
                GetSnapshotExpression())
        {
        }

        private static Expression<Func<TGeometry, TGeometry, bool>> GetEqualsExpression()
        {
            var left = Expression.Parameter(typeof(TGeometry), "left");
            var right = Expression.Parameter(typeof(TGeometry), "right");

            return Expression.Lambda<Func<TGeometry, TGeometry, bool>>(
                Expression.Call(
                    left,
                    typeof(TGeometry).GetRuntimeMethod("EqualsTopologically", new[] { typeof(TGeometry) }),
                    right),
                left,
                right);
        }

        private static Expression<Func<TGeometry, TGeometry>> GetSnapshotExpression()
        {
            var instance = Expression.Parameter(typeof(TGeometry), "instance");

            Expression body = Expression.Call(
                instance,
                typeof(TGeometry).GetRuntimeMethod("Copy", Type.EmptyTypes));

            if (typeof(TGeometry).FullName != "NetTopologySuite.Geometries.Geometry")
            {
                body = Expression.Convert(body, typeof(TGeometry));
            }

            return Expression.Lambda<Func<TGeometry, TGeometry>>(body, instance);
        }
    }
}
