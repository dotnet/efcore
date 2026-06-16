// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     <para>
///         Classifies CLR members of a type during model building, determining whether a member is a candidate
///         navigation, scalar property, complex property, or service property.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
///     <para>
///         Warning: do not implement this interface directly. Instead, derive from <see cref="MemberClassifier" />.
///     </para>
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
/// </remarks>
public interface IMemberClassifier
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    IReadOnlyDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)> GetNavigationCandidates(
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
            if (!IsCandidateNavigationProperty(propertyInfo, entityType.Model, useAttributes, out var targetType, out var shouldBeOwned, out _)
                || targetType == null)
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    IReadOnlyCollection<Type> GetInverseCandidateTypes(
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

    /// <summary>
    ///     Returns a value indicating whether the given member is a candidate navigation.
    /// </summary>
    /// <param name="memberInfo">The member to classify.</param>
    /// <param name="model">The model.</param>
    /// <param name="useAttributes">Whether attributes found on the member should be considered.</param>
    /// <param name="elementType">
    ///     When this method returns, the navigation target type (the element type for collection navigations),
    ///     or <see langword="null" /> if the member is not a candidate navigation.
    /// </param>
    /// <param name="shouldBeOwned">When this method returns, indicates whether the target should be owned, if known.</param>
    /// <param name="explicitlyConfigured">When this method returns, indicates whether the target type was explicitly configured.</param>
    /// <returns><see langword="true" /> if the member is a candidate navigation; otherwise <see langword="false" />.</returns>
    bool IsCandidateNavigationProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out Type? elementType,
        out bool? shouldBeOwned,
        out bool explicitlyConfigured);

    /// <summary>
    ///     Returns a value indicating whether the given member is a candidate primitive (scalar) property.
    /// </summary>
    /// <param name="memberInfo">The member to classify.</param>
    /// <param name="model">The model.</param>
    /// <param name="useAttributes">Whether attributes found on the member should be considered.</param>
    /// <param name="typeMapping">When this method returns, the type mapping for the member, if one was found.</param>
    /// <param name="explicitlyConfigured">When this method returns, indicates whether the type was explicitly configured.</param>
    /// <returns><see langword="true" /> if the member is a candidate primitive property; otherwise <see langword="false" />.</returns>
    bool IsCandidatePrimitiveProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out CoreTypeMapping? typeMapping,
        out bool explicitlyConfigured);

    /// <summary>
    ///     Returns a value indicating whether the given member is a candidate complex property.
    /// </summary>
    /// <param name="memberInfo">The member to classify.</param>
    /// <param name="model">The model.</param>
    /// <param name="useAttributes">Whether attributes found on the member should be considered.</param>
    /// <param name="elementType">When this method returns, the element type if the member is a complex collection.</param>
    /// <param name="explicitlyConfigured">When this method returns, indicates whether the type was explicitly configured.</param>
    /// <returns><see langword="true" /> if the member is a candidate complex property; otherwise <see langword="false" />.</returns>
    bool IsCandidateComplexProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out Type? elementType,
        out bool explicitlyConfigured);

    /// <summary>
    ///     Returns a value indicating whether the given member is a candidate service property.
    /// </summary>
    /// <param name="memberInfo">The member to classify.</param>
    /// <param name="model">The model.</param>
    /// <param name="useAttributes">Whether attributes found on the member should be considered.</param>
    /// <param name="bindingFactory">When this method returns, the parameter binding factory for the service property, if one was found.</param>
    /// <param name="explicitlyConfigured">When this method returns, indicates whether the type was explicitly configured.</param>
    /// <returns><see langword="true" /> if the member is a candidate service property; otherwise <see langword="false" />.</returns>
    bool IsCandidateServiceProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out IParameterBindingFactory? bindingFactory,
        out bool explicitlyConfigured);
}
