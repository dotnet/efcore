// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Value snapshotting and comparison logic for NetTopologySuite.Geometries.Geometry instances.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-comparers">EF Core value comparers</see> for more information and examples.
/// </remarks>
public class GeometryValueComparer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TGeometry>
    : ValueComparer<TGeometry>
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

    private static Expression<Func<TGeometry?, TGeometry?, bool>> GetEqualsExpression()
    {
        var left = Expression.Parameter(typeof(TGeometry), "left");
        var right = Expression.Parameter(typeof(TGeometry), "right");

        var x = Expression.Variable(typeof(TGeometry), "x");
        var y = Expression.Variable(typeof(TGeometry), "y");
        var xNull = Expression.Variable(typeof(bool), "xNull");
        var yNull = Expression.Variable(typeof(bool), "yNull");
        var nullExpression = Expression.Constant(null, typeof(TGeometry));

        return Expression.Lambda<Func<TGeometry?, TGeometry?, bool>>(
            Expression.Block(
                typeof(bool),
                new[] { x, y, xNull, yNull },
                Expression.Assign(x, left),
                Expression.Assign(y, right),
                Expression.Assign(xNull, Expression.ReferenceEqual(x, nullExpression)),
                Expression.Assign(yNull, Expression.ReferenceEqual(y, nullExpression)),
                Expression.OrElse(
                    Expression.AndAlso(xNull, yNull),
                    Expression.AndAlso(
                        Expression.IsFalse(xNull),
                        Expression.AndAlso(
                            Expression.IsFalse(yNull),
                            Expression.Call(
                                x,
                                typeof(TGeometry).GetRuntimeMethod("EqualsExact", [typeof(TGeometry)])!,
                                y))))),
            left,
            right);
    }

    private static Expression<Func<TGeometry, TGeometry>> GetSnapshotExpression()
    {
        var instance = Expression.Parameter(typeof(TGeometry), "instance");

        Expression body = Expression.Call(
            instance,
            typeof(TGeometry).GetRuntimeMethod("Copy", Type.EmptyTypes)!);

        if (typeof(TGeometry).FullName != "NetTopologySuite.Geometries.Geometry")
        {
            body = Expression.Convert(body, typeof(TGeometry));
        }

        return Expression.Lambda<Func<TGeometry, TGeometry>>(body, instance);
    }
}
