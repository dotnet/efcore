// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property which can be used to navigate a relationship.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="INavigation" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionNavigation : IReadOnlyNavigation, IConventionNavigationBase
{
    /// <summary>
    ///     Gets the builder that can be used to configure this navigation.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the property has been removed from the model.</exception>
    new IConventionNavigationBuilder Builder { get; }

    /// <summary>
    ///     Gets the type that this navigation property belongs to.
    /// </summary>
    new IConventionEntityType DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => (IConventionEntityType)((IReadOnlyNavigationBase)this).DeclaringEntityType;
    }

    /// <summary>
    ///     Gets the entity type that this navigation property will hold an instance(s) of.
    /// </summary>
    new IConventionEntityType TargetEntityType
    {
        [DebuggerStepThrough]
        get => (IConventionEntityType)((IReadOnlyNavigationBase)this).TargetEntityType;
    }

    /// <summary>
    ///     Returns the configuration source for this navigation property.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource IConventionPropertyBase.GetConfigurationSource()
        => (ConfigurationSource)(IsOnDependent
            ? ForeignKey.GetDependentToPrincipalConfigurationSource()
            : ForeignKey.GetPrincipalToDependentConfigurationSource())!;

    /// <summary>
    ///     Gets the foreign key that defines the relationship this navigation property will navigate.
    /// </summary>
    new IConventionForeignKey ForeignKey
    {
        [DebuggerStepThrough]
        get => (IConventionForeignKey)((IReadOnlyNavigation)this).ForeignKey;
    }

    /// <summary>
    ///     Gets the inverse navigation.
    /// </summary>
    new IConventionNavigation? Inverse
    {
        [DebuggerStepThrough]
        get => (IConventionNavigation?)((IReadOnlyNavigation)this).Inverse;
    }

    /// <summary>
    ///     Sets the inverse navigation.
    /// </summary>
    /// <param name="inverseName">
    ///     The name of the inverse navigation property. Passing <see langword="null" /> will result in there being
    ///     no inverse navigation property defined.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new inverse navigation.</returns>
    IConventionNavigation? SetInverse(string? inverseName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the inverse navigation.
    /// </summary>
    /// <param name="inverse">
    ///     The name of the inverse navigation property. Passing <see langword="null" /> will result in there being
    ///     no inverse navigation property defined.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new inverse navigation.</returns>
    IConventionNavigation? SetInverse(MemberInfo? inverse, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="Inverse" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="Inverse" />.</returns>
    ConfigurationSource? GetInverseConfigurationSource();
}
