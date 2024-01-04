// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>

[Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
public class LiftableConstantExpressionHelpers
{
    private static readonly MethodInfo ModelFindEntiyTypeMethod =
        typeof(IModel).GetRuntimeMethod(nameof(IModel.FindEntityType), [typeof(string)])!;

    private static readonly MethodInfo RuntimeModelFindAdHocEntiyTypeMethod =
        typeof(RuntimeModel).GetRuntimeMethod(nameof(RuntimeModel.FindAdHocEntityType), [typeof(Type)])!;

    private static readonly MethodInfo TypeBaseFindComplexPropertyMethod =
        typeof(ITypeBase).GetRuntimeMethod(nameof(ITypeBase.FindComplexProperty), [typeof(string)])!;

    private static readonly MethodInfo TypeBaseFindPropertyMethod =
        typeof(ITypeBase).GetRuntimeMethod(nameof(ITypeBase.FindProperty), [typeof(string)])!;

    private static readonly MethodInfo TypeBaseFindServicePropertyMethod =
        typeof(IEntityType).GetRuntimeMethod(nameof(IEntityType.FindServiceProperty), [typeof(string)])!;

    private static readonly MethodInfo EntityTypeFindNavigationMethod =
        typeof(IEntityType).GetRuntimeMethod(nameof(IEntityType.FindNavigation), [typeof(string)])!;

    private static readonly MethodInfo EntityTypeFindSkipNavigationMethod =
        typeof(IEntityType).GetRuntimeMethod(nameof(IEntityType.FindSkipNavigation), [typeof(string)])!;

    private static readonly MethodInfo NavigationBaseClrCollectionAccessorMethod =
        typeof(INavigationBase).GetRuntimeMethod(nameof(INavigationBase.GetCollectionAccessor), [])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsLiteral(object? value)
    {
        if (value == StructuralComparisons.StructuralEqualityComparer)
        {
            return true;
        }

        return value switch
        {
            int or long or uint or ulong or short or sbyte or ushort or byte or double or float or decimal or string or char or bool => true,
            null or Type or Enum or CultureInfo or Encoding or IPAddress => true,
            TimeSpan or DateTime or DateTimeOffset or DateOnly or TimeOnly or Guid => true,
            ITuple tuple
                when tuple.GetType() is { IsGenericType: true } tupleType
                     && tupleType.Name.StartsWith("ValueTuple`", StringComparison.Ordinal)
                     && tupleType.Namespace == "System"
                => IsTupleLiteral(tuple),

            Array array => IsCollectionOfLiterals(array),
            IList list => IsCollectionOfLiterals(list),

            _ => false
        };

        bool IsTupleLiteral(ITuple tuple)
        {
            for (var i = 0; i < tuple.Length; i++)
            {
                if (!IsLiteral(tuple[i]))
                {
                    return false;
                }
            }

            return true;
        }

        bool IsCollectionOfLiterals(IEnumerable enumerable)
        {
            foreach (var enumerableElement in enumerable)
            {
                if (!IsLiteral(enumerableElement))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression BuildMemberAccessForEntityOrComplexType(ITypeBase targetType, ParameterExpression liftableConstantContextParameter)
    {
        var (rootEntityType, complexTypes) = FindPathForEntityOrComplexType(targetType);

        Expression result;

        if (rootEntityType.IsAdHoc())
        {
            result = Expression.Call(
                Expression.Convert(
                    Expression.Property(
                        Expression.Property(
                            liftableConstantContextParameter,
                            nameof(MaterializerLiftableConstantContext.Dependencies)),
                        nameof(ShapedQueryCompilingExpressionVisitorDependencies.Model)),
                    typeof(RuntimeModel)),
                RuntimeModelFindAdHocEntiyTypeMethod,
                Expression.Constant(rootEntityType.ClrType));

        }
        else
        {
            result = Expression.Call(
                Expression.Property(
                    Expression.Property(
                        liftableConstantContextParameter,
                        nameof(MaterializerLiftableConstantContext.Dependencies)),
                    nameof(ShapedQueryCompilingExpressionVisitorDependencies.Model)),
                ModelFindEntiyTypeMethod,
                Expression.Constant(rootEntityType.Name));
        }

        foreach (var complexType in complexTypes)
        {
            var complexPropertyName = complexType.ComplexProperty.Name;
            result = Expression.Property(
                Expression.Call(result, TypeBaseFindComplexPropertyMethod, Expression.Constant(complexPropertyName)),
                nameof(IComplexProperty.ComplexType));
        }

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression<Func<MaterializerLiftableConstantContext, object>> BuildMemberAccessLambdaForEntityOrComplexType(ITypeBase type)
    {
        var prm = Expression.Parameter(typeof(MaterializerLiftableConstantContext));
        var body = BuildMemberAccessForEntityOrComplexType(type, prm);

        return Expression.Lambda<Func<MaterializerLiftableConstantContext, object>>(body, prm);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression BuildMemberAccessForProperty(IPropertyBase property, ParameterExpression liftableConstantContextParameter)
    {
        var declaringType = property.DeclaringType;
        var declaringTypeMemberAccessExpression = BuildMemberAccessForEntityOrComplexType(declaringType, liftableConstantContextParameter);

        return Expression.Call(
            declaringTypeMemberAccessExpression,
            property is IServiceProperty ? TypeBaseFindServicePropertyMethod : TypeBaseFindPropertyMethod,
            Expression.Constant(property.Name));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression<Func<MaterializerLiftableConstantContext, object>> BuildMemberAccessLambdaForProperty(IPropertyBase property)
    {
        var prm = Expression.Parameter(typeof(MaterializerLiftableConstantContext));
        var body = BuildMemberAccessForProperty(property, prm);

        return Expression.Lambda<Func<MaterializerLiftableConstantContext, object>>(body, prm);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression BuildNavigationAccess(INavigationBase navigation, ParameterExpression liftableConstantContextParameter)
    {
        var declaringType = navigation.DeclaringType;
        var declaringTypeExpression = BuildMemberAccessForEntityOrComplexType(declaringType, liftableConstantContextParameter);

        var result = Expression.Call(
            declaringTypeExpression,
            navigation is ISkipNavigation ? EntityTypeFindSkipNavigationMethod : EntityTypeFindNavigationMethod,
            Expression.Constant(navigation.Name));

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression<Func<MaterializerLiftableConstantContext, object>> BuildNavigationAccessLambda(INavigationBase navigation)
    {
        var prm = Expression.Parameter(typeof(MaterializerLiftableConstantContext));
        var body = BuildNavigationAccess(navigation, prm);

        return Expression.Lambda<Func<MaterializerLiftableConstantContext, object>>(body, prm);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression BuildClrCollectionAccessor(INavigationBase navigation, ParameterExpression liftableConstantContextParameter)
    {
        var navigationAccessExpression = BuildNavigationAccess(navigation, liftableConstantContextParameter);
        var result = Expression.Call(navigationAccessExpression, NavigationBaseClrCollectionAccessorMethod);

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression<Func<MaterializerLiftableConstantContext, object>> BuildClrCollectionAccessorLambda(INavigationBase navigation)
    {
        var prm = Expression.Parameter(typeof(MaterializerLiftableConstantContext));
        var body = BuildClrCollectionAccessor(navigation, prm);

        return Expression.Lambda<Func<MaterializerLiftableConstantContext, object>>(body, prm);
    }

    private static (IEntityType RootEntity, List<IComplexType> ComplexTypes) FindPathForEntityOrComplexType(ITypeBase targetType)
    {
        if (targetType is IEntityType targetEntity)
        {
            return (targetEntity, []);
        }

        var targetComplexType = (IComplexType)targetType;
        var declaringType = targetComplexType.ComplexProperty.DeclaringType;
        if (declaringType is IEntityType declaringEntityType)
        {
            return (declaringEntityType, [targetComplexType]);
        }

        var complexTypes = new List<IComplexType>();
        while (declaringType is IComplexType complexType)
        {
            complexTypes.Insert(0, complexType);
            declaringType = complexType.ComplexProperty.DeclaringType;
        }

        complexTypes.Add(targetComplexType);

        return ((IEntityType)declaringType, complexTypes);
    }
}
