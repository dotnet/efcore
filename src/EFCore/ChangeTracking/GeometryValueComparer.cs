// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Value snapshotting and comparison logic for GeoAPI.Geometries.IGeometry instances.
    /// </summary>
    public class GeometryValueComparer : ValueComparer
    {
        private Delegate _equals;
        private Delegate _hashCode;
        private Delegate _snapshot;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GeometryValueComparer"/> class.
        /// </summary>
        /// <param name="type"> A type that implements GeoAPI.Geometries.IGeometry. </param>
        public GeometryValueComparer(Type type)
            : base(
                  GetEqualsExpression(type),
                  GetHashCodeExpression(type),
                  GetSnapshotExpression(type))
        {
            Type = type;
        }

        /// <summary>
        ///     The type.
        /// </summary>
        public override Type Type { get; }

        /// <inheritdoc/>
        public override LambdaExpression EqualsExpression
            => base.EqualsExpression;

        /// <summary>
        ///     Compares the two instances to determine if they are equal.
        /// </summary>
        /// <param name="left"> The first instance. </param>
        /// <param name="right"> The second instance. </param>
        /// <returns> <c>True</c> if they are equal; <c>false</c> otherwise. </returns>
        public override bool Equals(object left, object right)
            => (bool)NonCapturingLazyInitializer.EnsureInitialized(
                    ref _equals,
                    this,
                    c => c.EqualsExpression.Compile())
                .DynamicInvoke(left, right);

        /// <summary>
        ///     Returns the hash code for the given instance.
        /// </summary>
        /// <param name="instance"> The instance. </param>
        /// <returns> The hash code. </returns>
        public override int GetHashCode(object instance)
            => (int)NonCapturingLazyInitializer.EnsureInitialized(
                    ref _hashCode,
                    this,
                    c => c.HashCodeExpression.Compile())
                .DynamicInvoke(instance);

        /// <summary>
        ///     <para>
        ///         Creates a snapshot of the given instance.
        ///     </para>
        ///     <para>
        ///         Snapshotting is the process of creating a copy of the value into a snapshot so it can
        ///         later be compared to determine if it has changed. For some types, such as collections,
        ///         this needs to be a deep copy of the collection rather than just a shallow copy of the
        ///         reference.
        ///     </para>
        /// </summary>
        /// <param name="instance"> The instance. </param>
        /// <returns> The snapshot. </returns>
        public override object Snapshot(object instance)
            => NonCapturingLazyInitializer.EnsureInitialized(
                    ref _snapshot,
                    this,
                    c => c.SnapshotExpression.Compile())
                .DynamicInvoke(instance);

        private static LambdaExpression GetEqualsExpression(Type type)
        {
            var geometry = type.FullName != "GeoAPI.Geometries.IGeometry"
                ? type.GetInterface("GeoAPI.Geometries.IGeometry")
                : type;

            var left = Expression.Parameter(type, "left");
            var right = Expression.Parameter(type, "right");

            return Expression.Lambda(
                Expression.Call(
                    left,
                    geometry.GetRuntimeMethod("EqualsTopologically", new[] { type }),
                    right),
                left,
                right);
        }

        private static LambdaExpression GetHashCodeExpression(Type type)
        {
            var instance = Expression.Parameter(type, "instance");

            return Expression.Lambda(
                Expression.Call(
                    instance,
                    typeof(object).GetRuntimeMethod("GetHashCode", Type.EmptyTypes)),
                instance);
        }

        private static LambdaExpression GetSnapshotExpression(Type type)
        {
            var geometry = type.FullName != "GeoAPI.Geometries.IGeometry"
                ? type.GetInterface("GeoAPI.Geometries.IGeometry")
                : type;

            var instance = Expression.Parameter(type, "instance");

            Expression body = Expression.Call(
                instance,
                geometry.GetRuntimeMethod("Copy", Type.EmptyTypes));
            if (!type.IsAssignableFrom(geometry))
            {
                body = Expression.Convert(body, type);
            }

            return Expression.Lambda(body, instance);
        }
    }
}
