// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Metadata about the shape of entities, the relationships between them, and how they map to
///     the database. A model is typically created by overriding the
///     <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived
///     <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IModel : IReadOnlyModel, IAnnotatable
{
    /// <summary>
    ///     Gets the entity with the given name. Returns <see langword="null" /> if no entity type with the given name is found
    ///     or the given CLR type is being used by shared type entity type
    ///     or the entity type has a defining navigation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the entity type to find.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    new IEntityType? FindEntityType(string name);

    /// <summary>
    ///     Gets the entity type for the given name, defining navigation name
    ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the entity type to find.</param>
    /// <param name="definingNavigationName">The defining navigation of the entity type to find.</param>
    /// <param name="definingEntityType">The defining entity type of the entity type to find.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    IEntityType? FindEntityType(
        string name,
        string definingNavigationName,
        IEntityType definingEntityType);

    /// <summary>
    ///     Gets the entity that maps the given entity class, where the class may be a proxy derived from the
    ///     actual entity type. Returns <see langword="null" /> if no entity type with the given CLR type is found
    ///     or the given CLR type is being used by shared type entity type
    ///     or the entity type has a defining navigation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The type to find the corresponding entity type for.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    IEntityType? FindRuntimeEntityType(Type? type)
    {
        Check.NotNull(type, nameof(type));

        while (type != null)
        {
            var entityType = FindEntityType(type);
            if (entityType != null)
            {
                return entityType;
            }

            type = type.BaseType;
        }

        return null;
    }

    /// <summary>
    ///     Gets all entity types defined in the model.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <returns>All entity types defined in the model.</returns>
    new IEnumerable<IEntityType> GetEntityTypes();

    /// <summary>
    ///     The runtime service dependencies.
    /// </summary>
    [DisallowNull]
    RuntimeModelDependencies? ModelDependencies
    {
        get => (RuntimeModelDependencies?)FindRuntimeAnnotationValue(CoreAnnotationNames.ModelDependencies);
        set => SetRuntimeAnnotation(CoreAnnotationNames.ModelDependencies, Check.NotNull(value, nameof(value)));
    }

    /// <summary>
    ///     Gets the runtime service dependencies.
    /// </summary>
    RuntimeModelDependencies GetModelDependencies()
    {
        var dependencies = ModelDependencies;
        if (dependencies == null)
        {
            throw new InvalidOperationException(CoreStrings.ModelNotFinalized(nameof(GetModelDependencies)));
        }

        return dependencies;
    }

    /// <summary>
    ///     Gets the entity that maps the given entity class. Returns <see langword="null" /> if no entity type with
    ///     the given CLR type is found or the given CLR type is being used by shared type entity type
    ///     or the entity type has a defining navigation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The type to find the corresponding entity type for.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    new IEntityType? FindEntityType([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type);

    /// <summary>
    ///     Gets the entity type for the given name, defining navigation name
    ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The type of the entity type to find.</param>
    /// <param name="definingNavigationName">The defining navigation of the entity type to find.</param>
    /// <param name="definingEntityType">The defining entity type of the entity type to find.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    IEntityType? FindEntityType(
        Type type,
        string definingNavigationName,
        IEntityType definingEntityType)
        => (IEntityType?)((IReadOnlyModel)this).FindEntityType(type, definingNavigationName, definingEntityType);

    /// <summary>
    ///     Gets the entity types matching the given type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The type of the entity type to find.</param>
    /// <returns>The entity types found.</returns>
    [DebuggerStepThrough]
    new IEnumerable<IEntityType> FindEntityTypes(Type type);

    /// <summary>
    ///     Returns the entity types corresponding to the least derived types from the given.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The base type.</param>
    /// <param name="condition">An optional condition for filtering entity types.</param>
    /// <returns>List of entity types corresponding to the least derived types from the given.</returns>
    new IEnumerable<IEntityType> FindLeastDerivedEntityTypes(
        Type type,
        Func<IReadOnlyEntityType, bool>? condition = null)
        => ((IReadOnlyModel)this).FindLeastDerivedEntityTypes(type, condition)
            .Cast<IEntityType>();

    /// <summary>
    ///     Gets a value indicating whether the given <see cref="MethodInfo" /> represents an indexer access.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="methodInfo">The <see cref="MethodInfo" /> to check.</param>
    bool IsIndexerMethod(MethodInfo methodInfo);

    /// <summary>
    ///     Gets all the pre-convention configurations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <returns>The pre-convention configurations.</returns>
    IEnumerable<ITypeMappingConfiguration> GetTypeMappingConfigurations();

    /// <summary>
    ///     Finds the pre-convention configuration for a given scalar <see cref="Type" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="scalarType">The CLR type.</param>
    /// <returns>The pre-convention configuration or <see langword="null" /> if none is found.</returns>
    ITypeMappingConfiguration? FindTypeMappingConfiguration(Type scalarType);
}
