// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Defines a source for generating <see cref="Expression" /> trees that read values from
///         a <see cref="ValueBuffer" /> or creates structural type instances.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
///     </para>
/// </remarks>
public interface IStructuralTypeMaterializerSource
{
    /// <summary>
    ///     <para>
    ///         Creates an <see cref="Expression" /> tree representing creating an entity instance.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="parameters">Parameters for the entity being materialized.</param>
    /// <param name="materializationExpression">The materialization expression to build on.</param>
    /// <returns>An expression to read the value.</returns>
    Expression CreateMaterializeExpression(StructuralTypeMaterializerSourceParameters parameters, Expression materializationExpression);

    /// <summary>
    ///     <para>
    ///         Returns a delegate that creates an instance of the given entity type.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type being materialized.</param>
    /// <returns>A delegate to create instances.</returns>
    Func<MaterializationContext, object> GetMaterializer(IEntityType entityType);

    /// <summary>
    ///     <para>
    ///         Returns a delegate that creates an instance of the given complex type.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="complexType">The entity type being materialized.</param>
    /// <returns>A delegate to create instances.</returns>
    Func<MaterializationContext, object> GetMaterializer(IComplexType complexType);

    /// <summary>
    ///     <para>
    ///         Returns a delegate that creates an empty instance of the given entity type.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type being materialized.</param>
    /// <returns>A delegate to create instances.</returns>
    Func<MaterializationContext, object> GetEmptyMaterializer(IEntityType entityType);

    /// <summary>
    ///     <para>
    ///         Returns a delegate that creates an empty instance of the given complex type.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="complexType">The entity type being materialized.</param>
    /// <returns>A delegate to create instances.</returns>
    Func<MaterializationContext, object> GetEmptyMaterializer(IComplexType complexType);
}

/// <summary>
///     This interface has been obsoleted, use <see cref="IStructuralTypeMaterializerSource" /> instead.
/// </summary>
[Obsolete("This interface has been obsoleted, use IStructuralTypeMaterializerSource instead.", error: true)]
public interface IEntityMaterializerSource;
