// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents an entity type in a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyEntityType : IReadOnlyTypeBase
{
    /// <summary>
    ///     Gets the base type of this entity type. Returns <see langword="null" /> if this is not a
    ///     derived type in an inheritance hierarchy.
    /// </summary>
    IReadOnlyEntityType? BaseType { get; }

    /// <summary>
    ///     Gets the data stored in the model for the given entity type.
    /// </summary>
    /// <param name="providerValues">
    ///     If <see langword="true" /> then provider values are returned for properties with value converters.
    /// </param>
    /// <returns>The data.</returns>
    IEnumerable<IDictionary<string, object?>> GetSeedData(bool providerValues = false);

    /// <summary>
    ///     Gets the LINQ expression filter automatically applied to queries for this entity type.
    /// </summary>
    /// <returns>The LINQ expression filter.</returns>
    LambdaExpression? GetQueryFilter();

    /// <summary>
    ///     Returns the property that will be used for storing a discriminator value.
    /// </summary>
    /// <returns>The property that will be used for storing a discriminator value.</returns>
    IReadOnlyProperty? FindDiscriminatorProperty()
    {
        var propertyName = GetDiscriminatorPropertyName();
        return propertyName == null ? null : FindProperty(propertyName);
    }

    /// <summary>
    ///     Returns the name of the property that will be used for storing a discriminator value.
    /// </summary>
    /// <returns>The name of the property that will be used for storing a discriminator value.</returns>
    string? GetDiscriminatorPropertyName();

    /// <summary>
    ///     Returns the value indicating whether the discriminator mapping is complete for this entity type.
    /// </summary>
    bool GetIsDiscriminatorMappingComplete()
        => (bool?)this[CoreAnnotationNames.DiscriminatorMappingComplete]
            ?? true;

    /// <summary>
    ///     Returns the discriminator value for this entity type.
    /// </summary>
    /// <returns>The discriminator value for this entity type.</returns>
    object? GetDiscriminatorValue()
    {
        var annotation = FindAnnotation(CoreAnnotationNames.DiscriminatorValue);
        return annotation != null
            ? annotation.Value
            : !ClrType.IsInstantiable()
            || (BaseType == null && GetDirectlyDerivedTypes().Count() == 0)
                ? null
                : (object?)GetDefaultDiscriminatorValue();
    }

    /// <summary>
    ///     Returns the default discriminator value that would be used for this entity type.
    /// </summary>
    /// <returns>The default discriminator value for this entity type.</returns>
    string GetDefaultDiscriminatorValue()
        => !HasSharedClrType ? ClrType.ShortDisplayName() : ShortName();

    /// <summary>
    ///     Gets all types in the model from which this entity type derives, starting with the root.
    /// </summary>
    /// <returns>
    ///     The base types.
    /// </returns>
    IEnumerable<IReadOnlyEntityType> GetAllBaseTypes()
        => GetAllBaseTypesAscending().Reverse();

    /// <summary>
    ///     Gets all types in the model from which this entity type derives, starting with the closest one.
    /// </summary>
    /// <returns>
    ///     The base types.
    /// </returns>
    IEnumerable<IReadOnlyEntityType> GetAllBaseTypesAscending()
        => GetAllBaseTypesInclusiveAscending().Skip(1);

    /// <summary>
    ///     Returns all base types of this entity type, including the type itself, top to bottom.
    /// </summary>
    /// <returns>Base types.</returns>
    IEnumerable<IReadOnlyEntityType> GetAllBaseTypesInclusive()
        => GetAllBaseTypesInclusiveAscending().Reverse();

    /// <summary>
    ///     Returns all base types of this entity type, including the type itself, bottom to top.
    /// </summary>
    /// <returns>Base types.</returns>
    IEnumerable<IReadOnlyEntityType> GetAllBaseTypesInclusiveAscending()
    {
        var tmp = this;
        while (tmp != null)
        {
            yield return tmp;
            tmp = tmp.BaseType;
        }
    }

    /// <summary>
    ///     Gets all types in the model that derive from this entity type.
    /// </summary>
    /// <returns>The derived types.</returns>
    IEnumerable<IReadOnlyEntityType> GetDerivedTypes();

    /// <summary>
    ///     Returns all derived types of this entity type, including the type itself.
    /// </summary>
    /// <returns>Derived types.</returns>
    IEnumerable<IReadOnlyEntityType> GetDerivedTypesInclusive()
        => new[] { this }.Concat(GetDerivedTypes());

    /// <summary>
    ///     Gets all types in the model that directly derive from this entity type.
    /// </summary>
    /// <returns>The derived types.</returns>
    IEnumerable<IReadOnlyEntityType> GetDirectlyDerivedTypes();

    /// <summary>
    ///     Returns all the derived types of this entity type, including the type itself,
    ///     which are not <see langword="abstract" />.
    /// </summary>
    /// <returns>Non-abstract, derived types.</returns>
    IEnumerable<IReadOnlyEntityType> GetConcreteDerivedTypesInclusive()
        => GetDerivedTypesInclusive().Where(et => !et.IsAbstract());

    /// <summary>
    ///     Gets the root base type for a given entity type.
    /// </summary>
    /// <returns>
    ///     The root base type. If the given entity type is not a derived type, then the same entity type is returned.
    /// </returns>
    IReadOnlyEntityType GetRootType()
        => BaseType?.GetRootType() ?? this;

    /// <summary>
    ///     Determines if this type derives from (or is the same as) a given type.
    /// </summary>
    /// <param name="derivedType">The type to check whether it derives from this type.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="derivedType" /> derives from (or is the same as) this type,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IReadOnlyTypeBase.IsAssignableFrom(IReadOnlyTypeBase derivedType)
        => derivedType is IReadOnlyEntityType derivedEntityType && IsAssignableFrom(derivedEntityType);

    /// <summary>
    ///     Determines if this entity type derives from (or is the same as) a given entity type.
    /// </summary>
    /// <param name="derivedType">The entity type to check whether it derives from this entity type.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="derivedType" /> derives from (or is the same as) this entity type,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IsAssignableFrom(IReadOnlyEntityType derivedType)
    {
        Check.NotNull(derivedType, nameof(derivedType));

        if (derivedType == this)
        {
            return true;
        }

        if (!GetDirectlyDerivedTypes().Any())
        {
            return false;
        }

        var baseType = derivedType.BaseType;
        while (baseType != null)
        {
            if (baseType == this)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    ///     Returns the closest entity type that is a parent of both given entity types. If one of the given entities is
    ///     a parent of the other, that parent is returned. Returns <see langword="null" /> if the two entity types aren't
    ///     in the same hierarchy.
    /// </summary>
    /// <param name="otherEntityType">Another entity type.</param>
    /// <returns>
    ///     The closest common parent of this entity type and <paramref name="otherEntityType" />,
    ///     or <see langword="null" /> if they have not common parent.
    /// </returns>
    IReadOnlyEntityType? FindClosestCommonParent(IReadOnlyEntityType otherEntityType)
    {
        Check.NotNull(otherEntityType, nameof(otherEntityType));

        var leastDerived = LeastDerivedType(otherEntityType);
        if (leastDerived != null)
        {
            return leastDerived;
        }

        return GetAllBaseTypesInclusiveAscending()
            .FirstOrDefault(i => otherEntityType.GetAllBaseTypesInclusiveAscending().Any(j => j == i));
    }

    /// <summary>
    ///     Gets the least derived type between the specified two.
    /// </summary>
    /// <param name="otherEntityType">The other entity type to compare with.</param>
    /// <returns>
    ///     The least derived type between the specified two.
    ///     If the given entity types are not related, then <see langword="null" /> is returned.
    /// </returns>
    IReadOnlyEntityType? LeastDerivedType(IReadOnlyEntityType otherEntityType)
        => IsAssignableFrom(Check.NotNull(otherEntityType, nameof(otherEntityType)))
            ? this
            : otherEntityType.IsAssignableFrom(this)
                ? otherEntityType
                : null;

    /// <summary>
    ///     Gets primary key for this entity type. Returns <see langword="null" /> if no primary key is defined.
    /// </summary>
    /// <returns>The primary key, or <see langword="null" /> if none is defined.</returns>
    IReadOnlyKey? FindPrimaryKey();

    /// <summary>
    ///     Gets the primary or alternate key that is defined on the given properties.
    ///     Returns <see langword="null" /> if no key is defined for the given properties.
    /// </summary>
    /// <param name="properties">The properties that make up the key.</param>
    /// <returns>The key, or <see langword="null" /> if none is defined.</returns>
    IReadOnlyKey? FindKey(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets the primary or alternate key that is defined on the given property. Returns <see langword="null" /> if no key is defined
    ///     for the given property.
    /// </summary>
    /// <param name="property">The property that the key is defined on.</param>
    /// <returns>The key, or null if none is defined.</returns>
    IReadOnlyKey? FindKey(IReadOnlyProperty property)
        => FindKey([property]);

    /// <summary>
    ///     Gets the primary and alternate keys for this entity type.
    /// </summary>
    /// <returns>The primary and alternate keys.</returns>
    IEnumerable<IReadOnlyKey> GetKeys();

    /// <summary>
    ///     Gets all keys declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return keys declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same key more than once.
    ///     Use <see cref="GetKeys" /> to also return keys declared on base types.
    /// </remarks>
    /// <returns>Declared keys.</returns>
    IEnumerable<IReadOnlyKey> GetDeclaredKeys();

    /// <summary>
    ///     Gets the foreign key for the given properties that points to a given primary or alternate key.
    ///     Returns <see langword="null" /> if no foreign key is found.
    /// </summary>
    /// <param name="properties">The properties that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <returns>The foreign key, or <see langword="null" /> if none is defined.</returns>
    IReadOnlyForeignKey? FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType);

    /// <summary>
    ///     Gets the foreign keys defined on the given property. Only foreign keys that are defined on exactly the specified
    ///     property are returned. Composite foreign keys that include the specified property are not returned.
    /// </summary>
    /// <param name="property">The property to find the foreign keys on.</param>
    /// <returns>The foreign keys.</returns>
    IEnumerable<IReadOnlyForeignKey> FindForeignKeys(IReadOnlyProperty property)
        => FindForeignKeys(new[] { property });

    /// <summary>
    ///     Gets the foreign keys defined on the given properties. Only foreign keys that are defined on exactly the specified
    ///     set of properties are returned.
    /// </summary>
    /// <param name="properties">The properties to find the foreign keys on.</param>
    /// <returns>The foreign keys.</returns>
    IEnumerable<IReadOnlyForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets the foreign key for the given properties that points to a given primary or alternate key. Returns <see langword="null" />
    ///     if no foreign key is found.
    /// </summary>
    /// <param name="property">The property that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <returns>The foreign key, or <see langword="null" /> if none is defined.</returns>
    IReadOnlyForeignKey? FindForeignKey(
        IReadOnlyProperty property,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(new[] { property }, principalKey, principalEntityType);

    /// <summary>
    ///     Gets the foreign keys declared on this entity type using the given properties.
    /// </summary>
    /// <param name="properties">The properties to find the foreign keys on.</param>
    /// <returns>Declared foreign keys.</returns>
    IEnumerable<IReadOnlyForeignKey> FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets all foreign keys declared on this entity type..
    /// </summary>
    /// <remarks>
    ///     This method does not return foreign keys declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
    ///     Use <see cref="GetForeignKeys" /> to also return foreign keys declared on base types.
    /// </remarks>
    /// <returns>Declared foreign keys.</returns>
    IEnumerable<IReadOnlyForeignKey> GetDeclaredForeignKeys();

    /// <summary>
    ///     Gets all foreign keys declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return foreign keys declared on the given entity type itself.
    ///     Use <see cref="GetForeignKeys" /> to return foreign keys declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived foreign keys.</returns>
    IEnumerable<IReadOnlyForeignKey> GetDerivedForeignKeys();

    /// <summary>
    ///     Gets the foreign keys defined on this entity type.
    /// </summary>
    /// <returns>The foreign keys defined on this entity type.</returns>
    IEnumerable<IReadOnlyForeignKey> GetForeignKeys();

    /// <summary>
    ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
    ///     or a base type is the principal).
    /// </summary>
    /// <returns>The foreign keys that reference the given entity type or a base type.</returns>
    IEnumerable<IReadOnlyForeignKey> GetReferencingForeignKeys();

    /// <summary>
    ///     Gets all foreign keys that target a given entity type (i.e. foreign keys where the given entity type
    ///     is the principal).
    /// </summary>
    /// <returns>The foreign keys that reference the given entity type.</returns>
    IEnumerable<IReadOnlyForeignKey> GetDeclaredReferencingForeignKeys();

    /// <summary>
    ///     Returns the relationship to the owner if this is an owned type or <see langword="null" /> otherwise.
    /// </summary>
    /// <returns>The relationship to the owner if this is an owned type or <see langword="null" /> otherwise.</returns>
    IReadOnlyForeignKey? FindOwnership()
    {
        foreach (var foreignKey in GetForeignKeys())
        {
            if (foreignKey.IsOwnership)
            {
                return foreignKey;
            }
        }

        return null;
    }

    /// <summary>
    ///     Gets a value indicating whether this entity type is owned by another entity type.
    /// </summary>
    /// <returns><see langword="true" /> if this entity type is owned by another entity type.</returns>
    [DebuggerStepThrough]
    bool IsOwned()
        => GetForeignKeys().Any(fk => fk.IsOwnership);

    /// <summary>
    ///     Gets a value indicating whether given entity type is in ownership path for this entity type.
    /// </summary>
    /// <param name="targetType">Entity type to search for in ownership path.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="targetType" /> is in ownership path of this entity type,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IsInOwnershipPath(IReadOnlyEntityType targetType)
    {
        var owner = this;
        while (true)
        {
            var ownOwnership = owner.FindOwnership();
            if (ownOwnership == null)
            {
                return false;
            }

            owner = ownOwnership.PrincipalEntityType;
            if (owner.IsAssignableFrom(targetType))
            {
                return true;
            }
        }
    }

    /// <summary>
    ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="memberInfo">The navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyNavigation? FindNavigation(MemberInfo memberInfo)
        => FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

    /// <summary>
    ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyNavigation? FindNavigation(string name)
        => FindDeclaredNavigation(Check.NotEmpty(name, nameof(name))) ?? BaseType?.FindNavigation(name);

    /// <summary>
    ///     Gets a navigation property on the given entity type. Does not return navigation properties defined on a base type.
    ///     Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyNavigation? FindDeclaredNavigation(string name);

    /// <summary>
    ///     Gets all navigation properties declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return navigation properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same navigation property more than once.
    ///     Use <see cref="GetNavigations" /> to also return navigation properties declared on base types.
    /// </remarks>
    /// <returns>Declared navigation properties.</returns>
    IEnumerable<IReadOnlyNavigation> GetDeclaredNavigations();

    /// <summary>
    ///     Gets all navigation properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return navigation properties declared on the given entity type itself.
    ///     Use <see cref="GetNavigations" /> to return navigation properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived navigation properties.</returns>
    IEnumerable<IReadOnlyNavigation> GetDerivedNavigations();

    /// <summary>
    ///     Gets all navigation properties on the given entity type.
    /// </summary>
    /// <returns>All navigation properties on the given entity type.</returns>
    IEnumerable<IReadOnlyNavigation> GetNavigations();

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="memberInfo">The navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    IReadOnlySkipNavigation? FindSkipNavigation(MemberInfo memberInfo)
        => FindSkipNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no skip navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    IReadOnlySkipNavigation? FindSkipNavigation(string name);

    /// <summary>
    ///     Gets a skip navigation property on this entity type.
    /// </summary>
    /// <remarks>
    ///     Does not return skip navigation properties defined on a base type.
    ///     Returns <see langword="null" /> if no skip navigation property is found.
    /// </remarks>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    IReadOnlySkipNavigation? FindDeclaredSkipNavigation(string name)
    {
        var navigation = FindSkipNavigation(name);
        return navigation?.DeclaringEntityType == this ? navigation : null;
    }

    /// <summary>
    ///     Gets all skip navigation properties declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return skip navigation properties declared declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
    ///     Use <see cref="GetSkipNavigations" /> to also return skip navigation properties declared on base types.
    /// </remarks>
    /// <returns>Declared skip navigations.</returns>
    IEnumerable<IReadOnlySkipNavigation> GetDeclaredSkipNavigations();

    /// <summary>
    ///     Gets all skip navigation properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return skip navigation properties declared on the given entity type itself.
    ///     Use <see cref="GetSkipNavigations" /> to return skip navigation properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived skip navigation properties.</returns>
    IEnumerable<IReadOnlySkipNavigation> GetDerivedSkipNavigations();

    /// <summary>
    ///     Gets the skip navigation properties on this entity type.
    /// </summary>
    /// <returns>All skip navigation properties on this entity type.</returns>
    IEnumerable<IReadOnlySkipNavigation> GetSkipNavigations();

    /// <summary>
    ///     Gets the unnamed index defined on the given properties. Returns <see langword="null" /> if no such index is defined.
    /// </summary>
    /// <remarks>
    ///     Named indexes will not be returned even if the list of properties matches.
    /// </remarks>
    /// <param name="properties">The properties to find the index on.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    IReadOnlyIndex? FindIndex(IReadOnlyList<IReadOnlyProperty> properties);

    /// <summary>
    ///     Gets the index with the given name. Returns <see langword="null" /> if no such index exists.
    /// </summary>
    /// <param name="name">The name of the index to find.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    IReadOnlyIndex? FindIndex(string name);

    /// <summary>
    ///     Gets the unnamed index defined on the given property. Returns <see langword="null" /> if no such index is defined.
    /// </summary>
    /// <remarks>
    ///     Named indexes will not be returned even if the list of properties matches.
    /// </remarks>
    /// <param name="property">The property to find the index on.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    IReadOnlyIndex? FindIndex(IReadOnlyProperty property)
        => FindIndex(new[] { property });

    /// <summary>
    ///     Gets all indexes declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return indexes declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same index more than once.
    ///     Use <see cref="GetForeignKeys" /> to also return indexes declared on base types.
    /// </remarks>
    /// <returns>Declared indexes.</returns>
    IEnumerable<IReadOnlyIndex> GetDeclaredIndexes();

    /// <summary>
    ///     Gets all indexes declared on the types derived from this entity type.
    /// </summary>
    /// <returns>Derived indexes.</returns>
    IEnumerable<IReadOnlyIndex> GetDerivedIndexes();

    /// <summary>
    ///     Gets the indexes defined on this entity type.
    /// </summary>
    /// <returns>The indexes defined on this entity type.</returns>
    IEnumerable<IReadOnlyIndex> GetIndexes();

    /// <summary>
    ///     Gets the service property with a given name.
    ///     Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds service properties and does not find scalar or navigation properties.
    /// </remarks>
    /// <param name="name">The name of the service property.</param>
    /// <returns>The service property, or <see langword="null" /> if none is found.</returns>
    IReadOnlyServiceProperty? FindServiceProperty(string name);

    /// <summary>
    ///     Gets all service properties declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same property more than once.
    ///     Use <see cref="GetServiceProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared service properties.</returns>
    IEnumerable<IReadOnlyServiceProperty> GetDeclaredServiceProperties();

    /// <summary>
    ///     Gets all service properties declared on the types derived from this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return service properties declared on the given entity type itself.
    ///     Use <see cref="GetServiceProperties" /> to return service properties declared on this
    ///     and base entity typed types.
    /// </remarks>
    /// <returns>Derived service properties.</returns>
    IEnumerable<IReadOnlyServiceProperty> GetDerivedServiceProperties();

    /// <summary>
    ///     Checks whether or not this entity type has any <see cref="IServiceProperty" /> defined.
    /// </summary>
    /// <returns><see langword="true" /> if there are any service properties defined on this entity type or base types.</returns>
    bool HasServiceProperties();

    /// <summary>
    ///     Gets all the <see cref="IReadOnlyServiceProperty" /> defined on this entity type.
    /// </summary>
    /// <remarks>
    ///     This API only returns service properties and does not return scalar or navigation properties.
    /// </remarks>
    /// <returns>The service properties defined on this entity type.</returns>
    IEnumerable<IReadOnlyServiceProperty> GetServiceProperties();

    /// <summary>
    ///     Finds a trigger with the given name.
    /// </summary>
    /// <param name="name">The trigger name.</param>
    /// <returns>The trigger or <see langword="null" /> if no trigger with the given name was found.</returns>
    IReadOnlyTrigger? FindDeclaredTrigger(string name);

    /// <summary>
    ///     Returns the declared triggers on the entity type.
    /// </summary>
    IEnumerable<IReadOnlyTrigger> GetDeclaredTriggers();

    /// <summary>
    ///     Gets the <see cref="PropertyAccessMode" /> being used for navigations of this entity type.
    /// </summary>
    /// <remarks>
    ///     Note that individual navigations can override this access mode. The value returned here will
    ///     be used for any navigation for which no override has been specified.
    /// </remarks>
    /// <returns>The access mode being used.</returns>
    PropertyAccessMode GetNavigationAccessMode();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    Func<MaterializationContext, object> GetOrCreateMaterializer(IEntityMaterializerSource source);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    Func<MaterializationContext, object> GetOrCreateEmptyMaterializer(IEntityMaterializerSource source);

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        try
        {
            builder
                .Append(indentString)
                .Append("EntityType: ")
                .Append(DisplayName());

            if (BaseType != null)
            {
                builder.Append(" Base: ").Append(BaseType.DisplayName());
            }

            if (HasSharedClrType)
            {
                builder.Append(" CLR Type: ").Append(ClrType.ShortDisplayName());
            }

            if (IsAbstract())
            {
                builder.Append(" Abstract");
            }

            if (FindPrimaryKey() == null)
            {
                builder.Append(" Keyless");
            }

            if (IsOwned())
            {
                builder.Append(" Owned");
            }

            if (this is EntityType
                && GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
            {
                builder.Append(" ChangeTrackingStrategy.").Append(GetChangeTrackingStrategy());
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                var properties = GetDeclaredProperties().ToList();
                if (properties.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Properties: ");
                    foreach (var property in properties)
                    {
                        builder.AppendLine().Append(property.ToDebugString(options, indent + 4));
                    }
                }

                var navigations = GetDeclaredNavigations().ToList();
                if (navigations.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Navigations: ");
                    foreach (var navigation in navigations)
                    {
                        builder.AppendLine().Append(navigation.ToDebugString(options, indent + 4));
                    }
                }

                var skipNavigations = GetDeclaredSkipNavigations().ToList();
                if (skipNavigations.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Skip navigations: ");
                    foreach (var skipNavigation in skipNavigations)
                    {
                        builder.AppendLine().Append(skipNavigation.ToDebugString(options, indent + 4));
                    }
                }

                var complexProperties = GetDeclaredComplexProperties().ToList();
                if (complexProperties.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Complex properties: ");
                    foreach (var complexProperty in complexProperties)
                    {
                        builder.AppendLine().Append(complexProperty.ToDebugString(options, indent + 4));
                    }
                }

                var serviceProperties = GetDeclaredServiceProperties().ToList();
                if (serviceProperties.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Service properties: ");
                    foreach (var serviceProperty in serviceProperties)
                    {
                        builder.AppendLine().Append(serviceProperty.ToDebugString(options, indent + 4));
                    }
                }

                var keys = GetDeclaredKeys().ToList();
                if (keys.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Keys: ");
                    foreach (var key in keys)
                    {
                        builder.AppendLine().Append(key.ToDebugString(options, indent + 4));
                    }
                }

                var fks = GetDeclaredForeignKeys().ToList();
                if (fks.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Foreign keys: ");
                    foreach (var fk in fks)
                    {
                        builder.AppendLine().Append(fk.ToDebugString(options, indent + 4));
                    }
                }

                var indexes = GetDeclaredIndexes().ToList();
                if (indexes.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Indexes: ");
                    foreach (var index in indexes)
                    {
                        builder.AppendLine().Append(index.ToDebugString(options, indent + 4));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(AnnotationsToDebugString(indent: indent + 2));
                }
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}
