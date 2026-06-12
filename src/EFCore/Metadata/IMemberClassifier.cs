// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    ///     Returns the CLR properties of the given entity type that are candidate navigations, together with the
    ///     target type each navigation points to and whether it should be owned.
    /// </summary>
    /// <param name="entityType">The entity type to find candidate navigations on.</param>
    /// <param name="useAttributes">Whether attributes found on the members should be considered.</param>
    /// <returns>The candidate navigation properties, keyed by the member they were found on.</returns>
    IReadOnlyDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)> GetNavigationCandidates(
        IConventionEntityType entityType,
        bool useAttributes);

    /// <summary>
    ///     Returns the target type if the given member is a candidate navigation, or <see langword="null" /> otherwise.
    /// </summary>
    /// <param name="memberInfo">The member to classify.</param>
    /// <param name="model">The model.</param>
    /// <param name="useAttributes">Whether attributes found on the member should be considered.</param>
    /// <param name="shouldBeOwned">When this method returns, indicates whether the target should be owned, if known.</param>
    /// <returns>The navigation target type, or <see langword="null" /> if the member is not a candidate navigation.</returns>
    Type? FindCandidateNavigationPropertyType(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out bool? shouldBeOwned);

    /// <summary>
    ///     Returns a value indicating whether the given member is a candidate primitive (scalar) property.
    /// </summary>
    /// <param name="memberInfo">The member to classify.</param>
    /// <param name="model">The model.</param>
    /// <param name="useAttributes">Whether attributes found on the member should be considered.</param>
    /// <param name="typeMapping">When this method returns, the type mapping for the member, if one was found.</param>
    /// <returns><see langword="true" /> if the member is a candidate primitive property; otherwise <see langword="false" />.</returns>
    bool IsCandidatePrimitiveProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out CoreTypeMapping? typeMapping);

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
    ///     Returns the CLR types that declare a navigation candidate pointing back to the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type to find inverse candidate types for.</param>
    /// <param name="useAttributes">Whether attributes found on the members should be considered.</param>
    /// <returns>The inverse candidate types.</returns>
    IReadOnlyCollection<Type> GetInverseCandidateTypes(IConventionEntityType entityType, bool useAttributes);

    /// <summary>
    ///     Returns the parameter binding factory for the given member if it is a candidate service property,
    ///     or <see langword="null" /> otherwise.
    /// </summary>
    /// <param name="memberInfo">The member to classify.</param>
    /// <param name="model">The model.</param>
    /// <param name="useAttributes">Whether attributes found on the member should be considered.</param>
    /// <returns>The binding factory, or <see langword="null" /> if the member is not a candidate service property.</returns>
    IParameterBindingFactory? FindServicePropertyCandidateBindingFactory(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes);
}
