// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Classifies CLR members of a type during model building, determining whether a member is a candidate
///     navigation, scalar property, complex property, or service property.
/// </summary>
/// <remarks>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
/// </remarks>
public class MemberClassifier : IMemberClassifier
{
    /// <summary>
    ///     Creates a new instance of <see cref="MemberClassifier" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public MemberClassifier(MemberClassifierDependencies dependencies)
        => Dependencies = dependencies;

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual MemberClassifierDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual IReadOnlyDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)> GetNavigationCandidates(
        IConventionEntityType entityType,
        bool useAttributes)
    {
        var candidatesAnnotationName = useAttributes
            ? CoreAnnotationNames.NavigationCandidates
            : CoreAnnotationNames.NavigationCandidatesNoAttribute;
        var inverseAnnotationName = useAttributes
            ? CoreAnnotationNames.InverseNavigations
            : CoreAnnotationNames.InverseNavigationsNoAttribute;
        if (entityType.FindAnnotation(candidatesAnnotationName)?.Value
            is Utilities.OrderedDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)> navigationCandidates)
        {
            return navigationCandidates;
        }

        navigationCandidates = new Utilities.OrderedDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)>();

        var model = entityType.Model;
        if (model.FindAnnotation(inverseAnnotationName)?.Value
            is not Dictionary<Type, SortedSet<Type>> inverseCandidatesLookup)
        {
            inverseCandidatesLookup = new Dictionary<Type, SortedSet<Type>>();
            model.SetAnnotation(inverseAnnotationName, inverseCandidatesLookup);
        }

        foreach (var propertyInfo in entityType.GetRuntimeProperties().Values)
        {
            var targetType = FindCandidateNavigationPropertyType(propertyInfo, entityType.Model, useAttributes, out var shouldBeOwned);
            if (targetType == null)
            {
                continue;
            }

            navigationCandidates.Insert(propertyInfo, (targetType, shouldBeOwned), MemberInfoNameComparer.Instance);

            if (!inverseCandidatesLookup.TryGetValue(targetType, out var inverseCandidates))
            {
                inverseCandidates = new SortedSet<Type>(TypeFullNameComparer.Instance);
                inverseCandidatesLookup[targetType] = inverseCandidates;
            }

            inverseCandidates.Add(entityType.ClrType);
        }

        if (!((Annotatable)entityType).IsReadOnly
            && entityType.IsInModel)
        {
            entityType.Builder.HasAnnotation(candidatesAnnotationName, navigationCandidates);
        }

        return navigationCandidates;
    }

    /// <inheritdoc />
    public virtual IReadOnlyCollection<Type> GetInverseCandidateTypes(
        IConventionEntityType entityType,
        bool useAttributes)
    {
        var annotationName = useAttributes
            ? CoreAnnotationNames.InverseNavigations
            : CoreAnnotationNames.InverseNavigationsNoAttribute;
        if (entityType.Model.FindAnnotation(annotationName)?.Value
                is not Dictionary<Type, SortedSet<Type>> inverseCandidatesLookup
            || !inverseCandidatesLookup.TryGetValue(entityType.ClrType, out var inverseCandidates))
        {
            return Type.EmptyTypes;
        }

        return inverseCandidates;
    }

    /// <inheritdoc />
    public virtual Type? FindCandidateNavigationPropertyType(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out bool? shouldBeOwned)
    {
        shouldBeOwned = null;
        var propertyInfo = memberInfo as PropertyInfo;
        var targetType = memberInfo.GetMemberType();
        var targetSequenceType = targetType.TryGetSequenceType();
        return targetSequenceType != null
            && (propertyInfo == null
                || propertyInfo.IsCandidateProperty(needsWrite: false))
            && IsCandidateNavigationPropertyType(targetSequenceType, memberInfo, (Model)model, useAttributes, out shouldBeOwned)
                ? targetSequenceType
                : (propertyInfo == null
                    || propertyInfo.IsCandidateProperty(needsWrite: true))
                && IsCandidateNavigationPropertyType(targetType, memberInfo, (Model)model, useAttributes, out shouldBeOwned)
                    ? targetType
                    : null;
    }

    private bool IsCandidateNavigationPropertyType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type targetType,
        MemberInfo memberInfo,
        Model model,
        bool useAttributes,
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

        var memberType = memberInfo.GetMemberType();
        return isConfiguredAsEntityType == true
            || targetType != typeof(object)
            && (memberType != targetType
                || (Dependencies.ParameterBindingFactories.FindFactory(memberType, memberInfo.GetSimpleMemberName()) == null
                    && Dependencies.TypeMappingSource.FindMapping(memberInfo, model, useAttributes) == null));
    }

    /// <inheritdoc />
    public virtual bool IsCandidatePrimitiveProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out CoreTypeMapping? typeMapping)
    {
        typeMapping = null;
        if (!memberInfo.IsCandidateProperty())
        {
            return false;
        }

        var configurationType = ((Model)model).Configuration?.GetConfigurationType(memberInfo.GetMemberType());
        return configurationType == TypeConfigurationType.Property
            || (configurationType == null
                && (typeMapping = Dependencies.TypeMappingSource.FindMapping(memberInfo, (IModel)model, useAttributes)) != null);
    }

    /// <inheritdoc />
    public virtual bool IsCandidateComplexProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out Type? elementType,
        out bool explicitlyConfigured)
    {
        explicitlyConfigured = false;
        elementType = null;
        if (!memberInfo.IsCandidateProperty())
        {
            return false;
        }

        var targetType = memberInfo.GetMemberType();
        if (targetType.TryGetElementType(typeof(IList<>)) is { } sequenceType
            && IsCandidateComplexType(sequenceType.UnwrapNullableType(), model, out explicitlyConfigured))
        {
            elementType = sequenceType;
            return true;
        }

        return IsCandidateComplexType(targetType.UnwrapNullableType(), model, out explicitlyConfigured);
    }

    private static bool IsCandidateComplexType(Type targetType, IConventionModel model, out bool explicitlyConfigured)
    {
        if (!targetType.IsValidComplexType()
            || (targetType.IsGenericType
                && targetType.GetGenericTypeDefinition() is var genericTypeDefinition
                && (genericTypeDefinition == typeof(Dictionary<,>)
                    || genericTypeDefinition == typeof(List<>)
                    || genericTypeDefinition == typeof(HashSet<>)
                    || genericTypeDefinition == typeof(Collection<>)
                    || genericTypeDefinition == typeof(ObservableCollection<>))))
        {
            explicitlyConfigured = false;
            return false;
        }

        var configurationType = ((Model)model).Configuration?.GetConfigurationType(targetType);
        explicitlyConfigured = configurationType != null;
        return configurationType == TypeConfigurationType.ComplexType
            || configurationType == null;
    }

    /// <inheritdoc />
    public virtual IParameterBindingFactory? FindServicePropertyCandidateBindingFactory(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes)
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
                && Dependencies.TypeMappingSource.FindMapping(memberInfo, (IModel)model, useAttributes) != null)
            {
                return null;
            }
        }

        return Dependencies.ParameterBindingFactories.FindFactory(type, memberInfo.GetSimpleMemberName());
    }
}
