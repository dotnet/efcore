// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Value snapshotting and comparison logic for GeoAPI.Geometries.IGeometry instances.
    /// </summary>
    public class GeometryValueComparer<TGeometry> : ValueComparer<TGeometry>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GeometryValueComparer{TGeometry}"/> class.
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

            var checkLeftNull = Expression.ReferenceEqual(left, Expression.Constant(null));
            var checkRightNull = Expression.ReferenceEqual(right, Expression.Constant(null));
            var bothNotNull = Expression.Call(
                left,
                GetGeometryType().GetRuntimeMethod("EqualsTopologically", new[] { typeof(TGeometry) }),
                right);
            
            var body = Expression.Condition(checkLeftNull,
                            Expression.Condition(checkRightNull, Expression.Constant(true), Expression.Constant(false)),
                            Expression.Condition(checkRightNull, Expression.Constant(false), bothNotNull));
            
            return Expression.Lambda<Func<TGeometry, TGeometry, bool>>(body, left, right);
        }

        private static Expression<Func<TGeometry, TGeometry>> GetSnapshotExpression()
        {
            var instance = Expression.Parameter(typeof(TGeometry), "instance");

            var geometryType = GetGeometryType();

            Expression body = Expression.Call(
                instance,
                geometryType.GetRuntimeMethod("Copy", Type.EmptyTypes));

            if (geometryType != typeof(TGeometry))
            {
                body = Expression.Convert(body, typeof(TGeometry));
            }

            var checkNull = Expression.ReferenceEqual(instance, Expression.Constant(null));
            var returnNull = Expression.Convert(Expression.Constant(null), typeof(TGeometry));
            var conditional = Expression.Condition(checkNull, returnNull, body);
            return Expression.Lambda<Func<TGeometry, TGeometry>>(Expression.Convert(conditional, typeof(TGeometry)), instance);
        }

        private static Type GetGeometryType()
            => typeof(TGeometry).FullName != "GeoAPI.Geometries.IGeometry"
                ? typeof(TGeometry).GetInterface("GeoAPI.Geometries.IGeometry")
                : typeof(TGeometry);
    }
}
