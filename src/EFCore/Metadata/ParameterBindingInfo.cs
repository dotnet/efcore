// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Carries information about a parameter binding.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public readonly struct ParameterBindingInfo
{
    /// <summary>
    ///     Creates a new <see cref="ParameterBindingInfo" /> to define a parameter binding.
    /// </summary>
    /// <param name="entityType">The entity type for this binding.</param>
    /// <param name="materializationContextExpression">The expression tree from which the parameter value will come.</param>
    public ParameterBindingInfo(
        IEntityType entityType,
        Expression materializationContextExpression)
    {
        Check.NotNull(entityType, nameof(entityType));
        Check.NotNull(entityType, nameof(materializationContextExpression));

        EntityType = entityType;
        EntityInstanceName = "instance";
        MaterializationContextExpression = materializationContextExpression;
    }

    /// <summary>
    ///     Creates a new <see cref="ParameterBindingInfo" /> to define a parameter binding.
    /// </summary>
    /// <param name="materializerSourceParameters">Parameters for the materialization that is happening.</param>
    /// <param name="materializationContextExpression">The expression tree from which the parameter value will come.</param>
    public ParameterBindingInfo(
        EntityMaterializerSourceParameters materializerSourceParameters,
        Expression materializationContextExpression)
    {
        EntityType = materializerSourceParameters.EntityType;
        QueryTrackingBehavior = materializerSourceParameters.QueryTrackingBehavior;
        EntityInstanceName = materializerSourceParameters.EntityInstanceName;
        MaterializationContextExpression = materializationContextExpression;
    }

    /// <summary>
    ///     The entity type for this binding.
    /// </summary>
    public IEntityType EntityType { get; }

    /// <summary>
    ///     The name of the instance being materialized.
    /// </summary>
    public string EntityInstanceName { get; }

    /// <summary>
    ///     The query tracking behavior, or <see langword="null" /> if this materialization is not from a query.
    /// </summary>
    public QueryTrackingBehavior? QueryTrackingBehavior { get; }

    /// <summary>
    ///     The expression tree from which the parameter value will come.
    /// </summary>
    public Expression MaterializationContextExpression { get; }

    /// <summary>
    ///     Expressions holding initialized instances for service properties.
    /// </summary>
    public List<ParameterExpression> ServiceInstances { get; } = new();

    /// <summary>
    ///     Gets the index into the <see cref="ValueBuffer" /> where the property value can be found.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The index where its value can be found.</returns>
    public int GetValueBufferIndex(IPropertyBase property)
        => property.GetIndex();
}
