// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MemberClassifier : IMemberClassifier
{
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IParameterBindingFactories _parameterBindingFactories;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MemberClassifier(
        ITypeMappingSource typeMappingSource,
        IParameterBindingFactories parameterBindingFactories)
    {
        _typeMappingSource = typeMappingSource;
        _parameterBindingFactories = parameterBindingFactories;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ImmutableSortedDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)> GetNavigationCandidates(
        IConventionEntityType entityType)
    {
        if (entityType.FindAnnotation(CoreAnnotationNames.NavigationCandidates)?.Value
            is ImmutableSortedDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)> navigationCandidates)
        {
            return navigationCandidates;
        }

        var dictionaryBuilder = ImmutableSortedDictionary.CreateBuilder<PropertyInfo, (Type Type, bool? shouldBeOwned)>(
            MemberInfoNameComparer.Instance);

        var model = entityType.Model;
        if (model.FindAnnotation(CoreAnnotationNames.InverseNavigationCandidates)?.Value
            is not Dictionary<Type, SortedSet<Type>> inverseCandidatesLookup)
        {
            inverseCandidatesLookup = new Dictionary<Type, SortedSet<Type>>();
            model.SetAnnotation(CoreAnnotationNames.InverseNavigationCandidates, inverseCandidatesLookup);
        }

        foreach (var propertyInfo in entityType.GetRuntimeProperties().Values)
        {
            var targetType = FindCandidateNavigationPropertyType(propertyInfo, entityType.Model, out var shouldBeOwned);
            if (targetType == null)
            {
                continue;
            }

            dictionaryBuilder[propertyInfo] = (targetType, shouldBeOwned);

            if (!inverseCandidatesLookup.TryGetValue(targetType, out var inverseCandidates))
            {
                inverseCandidates = new SortedSet<Type>(TypeFullNameComparer.Instance);
                inverseCandidatesLookup[targetType] = inverseCandidates;
            }

            inverseCandidates.Add(entityType.ClrType);
        }

        navigationCandidates = dictionaryBuilder.ToImmutable();

        if (!((Annotatable)entityType).IsReadOnly
            && entityType.IsInModel)
        {
            entityType.Builder.HasAnnotation(CoreAnnotationNames.NavigationCandidates, navigationCandidates);
        }

        return navigationCandidates;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyCollection<Type> GetInverseCandidateTypes(IConventionEntityType entityType)
    {
        if (entityType.Model.FindAnnotation(CoreAnnotationNames.InverseNavigationCandidates)?.Value
                is not Dictionary<Type, SortedSet<Type>> inverseCandidatesLookup
            || !inverseCandidatesLookup.TryGetValue(entityType.ClrType, out var inverseCandidates))
        {
            return Type.EmptyTypes;
        }

        return inverseCandidates;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? FindCandidateNavigationPropertyType(
        MemberInfo memberInfo,
        IConventionModel model,
        out bool? shouldBeOwned)
    {
        shouldBeOwned = null;
        var propertyInfo = memberInfo as PropertyInfo;
        var targetType = memberInfo.GetMemberType();
        var targetSequenceType = targetType.TryGetSequenceType();
        return targetSequenceType != null
            && (propertyInfo == null
                || propertyInfo.IsCandidateProperty(needsWrite: false))
            && IsCandidateNavigationPropertyType(targetSequenceType, memberInfo, (Model)model, out shouldBeOwned)
                ? targetSequenceType
                : (propertyInfo == null
                    || propertyInfo.IsCandidateProperty(needsWrite: true))
                && IsCandidateNavigationPropertyType(targetType, memberInfo, (Model)model, out shouldBeOwned)
                    ? targetType
                    : null;
    }

    private bool IsCandidateNavigationPropertyType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type targetType,
        MemberInfo memberInfo,
        Model model,
        out bool? shouldBeOwned)
    {
        shouldBeOwned = null;
        var configuration = model.Configuration;
        var configurationType = configuration?.GetConfigurationType(targetType);
        var isConfiguredAsEntityType = configurationType.IsEntityType();
        if (isConfiguredAsEntityType == false
            || !targetType.IsValidEntityType())
        {
            return false;
        }

        if (configurationType != null)
        {
            shouldBeOwned = configurationType == TypeConfigurationType.OwnedEntityType;
        }

        return isConfiguredAsEntityType == true
            || (targetType != typeof(object)
                && _parameterBindingFactories.FindFactory(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()) == null
                && _typeMappingSource.FindMapping(targetType, model) == null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsCandidatePrimitiveProperty(MemberInfo memberInfo, IConventionModel model)
    {
        if (!memberInfo.IsCandidateProperty())
        {
            return false;
        }

        var configurationType = ((Model)model).Configuration?.GetConfigurationType(memberInfo.GetMemberType());
        return configurationType == TypeConfigurationType.Property
            || (configurationType == null && _typeMappingSource.FindMapping(memberInfo) != null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsCandidateComplexProperty(MemberInfo memberInfo, IConventionModel model, out Type? elementType)
    {
        elementType = null;
        if (!memberInfo.IsCandidateProperty())
        {
            return false;
        }

        var targetType = memberInfo.GetMemberType();
        if (targetType.TryGetSequenceType() is Type sequenceType
            && IsCandidateComplexType(sequenceType, model))
        {
            elementType = sequenceType;
            return true;
        }

        return IsCandidateComplexType(targetType, model);
    }

    private static bool IsCandidateComplexType(Type targetType, IConventionModel model)
    {
        if (targetType.IsGenericType
            && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            return false;
        }

        var configurationType = ((Model)model).Configuration?.GetConfigurationType(targetType);
        return configurationType == TypeConfigurationType.ComplexType
            || configurationType == null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IParameterBindingFactory? FindServicePropertyCandidateBindingFactory(
        MemberInfo memberInfo,
        IConventionModel model)
    {
        if (!memberInfo.IsCandidateProperty(publicOnly: false))
        {
            return null;
        }

        var type = memberInfo.GetMemberType();
        var configurationType = ((Model)model).Configuration?.GetConfigurationType(type);
        if (configurationType != TypeConfigurationType.ServiceProperty)
        {
            if (configurationType != null)
            {
                return null;
            }

            if (memberInfo.IsCandidateProperty()
                && _typeMappingSource.FindMapping(memberInfo.GetMemberType(), (IModel)model) != null)
            {
                return null;
            }
        }

        return _parameterBindingFactories.FindFactory(type, memberInfo.GetSimpleMemberName());
    }
}
