// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         A base type with a simple API surface for configuring a <see cref="ConventionAnnotatable" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
[DebuggerDisplay("Builder {" + nameof(Metadata) + ",nq}")]
public abstract class AnnotatableBuilder<TMetadata, TModelBuilder> : IConventionAnnotatableBuilder
    where TMetadata : ConventionAnnotatable
    where TModelBuilder : IConventionModelBuilder
{
    /// <summary>
    ///     Creates a new instance of <see cref="AnnotatableBuilder{TMetadata, TModelBuilder}" />
    /// </summary>
    protected AnnotatableBuilder(TMetadata metadata, TModelBuilder modelBuilder)
    {
        Metadata = metadata;
        ModelBuilder = modelBuilder;
    }

    /// <summary>
    ///     Gets the item being configured.
    /// </summary>
    public virtual TMetadata Metadata { get; }

    /// <summary>
    ///     Gets the model builder.
    /// </summary>
    public virtual TModelBuilder ModelBuilder { get; }

    /// <summary>
    ///     Sets the annotation with given key and value on this object using given configuration source.
    ///     Overwrites the existing annotation if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="configurationSource">The configuration source of the annotation to be set.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual AnnotatableBuilder<TMetadata, TModelBuilder>? HasAnnotation(
        string name,
        object? value,
        ConfigurationSource configurationSource)
        => HasAnnotation(name, value, configurationSource, canOverrideSameSource: true);

    private AnnotatableBuilder<TMetadata, TModelBuilder>? HasAnnotation(
        string name,
        object? value,
        ConfigurationSource configurationSource,
        bool canOverrideSameSource)
    {
        var existingAnnotation = Metadata.FindAnnotation(name);
        if (existingAnnotation != null)
        {
            if (Equals(existingAnnotation.Value, value))
            {
                existingAnnotation.UpdateConfigurationSource(configurationSource);
                return this;
            }

            var existingConfigurationSource = existingAnnotation.GetConfigurationSource();
            if (!configurationSource.Overrides(existingConfigurationSource)
                || (configurationSource == existingConfigurationSource
                    && !canOverrideSameSource))
            {
                return null;
            }

            Metadata.SetAnnotation(name, value, configurationSource);

            return this;
        }

        Metadata.AddAnnotation(name, value, configurationSource);

        return this;
    }

    /// <summary>
    ///     Sets the annotation with given key and value on this object using given configuration source.
    ///     Overwrites the existing annotation if an annotation with the specified name already exists.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The key of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="configurationSource">The configuration source of the annotation to be set.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual AnnotatableBuilder<TMetadata, TModelBuilder>? HasNonNullAnnotation(
        string name,
        object? value,
        ConfigurationSource configurationSource)
        => value == null
            ? HasNoAnnotation(name, configurationSource)
            : HasAnnotation(name, value, configurationSource, canOverrideSameSource: true);

    /// <summary>
    ///     Returns a value indicating whether an annotation with the given name and value can be set from this configuration source.
    /// </summary>
    /// <param name="name">The name of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="configurationSource">The configuration source of the annotation to be set.</param>
    /// <returns><see langword="true" /> if the annotation can be set, <see langword="false" /> otherwise.</returns>
    public virtual bool CanSetAnnotation(string name, object? value, ConfigurationSource configurationSource)
    {
        var existingAnnotation = Metadata.FindAnnotation(name);
        return existingAnnotation == null
            || CanSetAnnotationValue(existingAnnotation, value, configurationSource, canOverrideSameSource: true);
    }

    private static bool CanSetAnnotationValue(
        ConventionAnnotation annotation,
        object? value,
        ConfigurationSource configurationSource,
        bool canOverrideSameSource)
    {
        if (Equals(annotation.Value, value))
        {
            return true;
        }

        var existingConfigurationSource = annotation.GetConfigurationSource();
        return configurationSource.Overrides(existingConfigurationSource)
            && (configurationSource != existingConfigurationSource
                || canOverrideSameSource);
    }

    /// <summary>
    ///     Removes any annotation with the given name.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="configurationSource">The configuration source of the annotation to be set.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    [Obsolete("Use HasNoAnnotation instead")]
    public virtual AnnotatableBuilder<TMetadata, TModelBuilder>? RemoveAnnotation(
        string name,
        ConfigurationSource configurationSource)
        => HasNoAnnotation(name, configurationSource);

    /// <summary>
    ///     Removes any annotation with the given name.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="configurationSource">The configuration source of the annotation to be set.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual AnnotatableBuilder<TMetadata, TModelBuilder>? HasNoAnnotation(
        string name,
        ConfigurationSource configurationSource)
    {
        if (!CanRemoveAnnotation(name, configurationSource))
        {
            return null;
        }

        Metadata.RemoveAnnotation(name);
        return this;
    }

    /// <summary>
    ///     Returns a value indicating whether an annotation with the given name can be removed using this configuration source.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="configurationSource">The configuration source of the annotation to be set.</param>
    /// <returns><see langword="true" /> if the annotation can be removed, <see langword="false" /> otherwise.</returns>
    public virtual bool CanRemoveAnnotation(string name, ConfigurationSource configurationSource)
    {
        var existingAnnotation = Metadata.FindAnnotation(name);
        return existingAnnotation == null
            || configurationSource.Overrides(existingAnnotation.GetConfigurationSource());
    }

    /// <summary>
    ///     Copies all the explicitly configured annotations from the given object overwriting any existing ones.
    /// </summary>
    /// <param name="annotatable">The object to copy annotations from.</param>
    public virtual AnnotatableBuilder<TMetadata, TModelBuilder> MergeAnnotationsFrom(TMetadata annotatable)
        => MergeAnnotationsFrom(annotatable, ConfigurationSource.Explicit);

    /// <summary>
    ///     Copies all the configured annotations from the given object overwriting any existing ones.
    /// </summary>
    /// <param name="annotatable">The object to copy annotations from.</param>
    /// <param name="minimalConfigurationSource">The minimum configuration source for an annotation to be copied.</param>
    public virtual AnnotatableBuilder<TMetadata, TModelBuilder> MergeAnnotationsFrom(
        TMetadata annotatable,
        ConfigurationSource minimalConfigurationSource)
    {
        var builder = this;
        foreach (var annotation in annotatable.GetAnnotations())
        {
            var configurationSource = annotation.GetConfigurationSource();
            if (configurationSource.Overrides(minimalConfigurationSource))
            {
                builder = builder.HasAnnotation(
                        annotation.Name,
                        annotation.Value,
                        configurationSource,
                        canOverrideSameSource: true)
                    ?? builder;
            }
        }

        return builder;
    }

    /// <inheritdoc />
    IConventionModelBuilder IConventionAnnotatableBuilder.ModelBuilder
    {
        [DebuggerStepThrough]
        get => ModelBuilder;
    }

    /// <inheritdoc />
    IConventionAnnotatable IConventionAnnotatableBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionAnnotatableBuilder? IConventionAnnotatableBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => HasAnnotation(name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionAnnotatableBuilder? IConventionAnnotatableBuilder.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => HasNonNullAnnotation(name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionAnnotatableBuilder.CanSetAnnotation(string name, object? value, bool fromDataAnnotation)
        => CanSetAnnotation(name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionAnnotatableBuilder? IConventionAnnotatableBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => HasNoAnnotation(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionAnnotatableBuilder.CanRemoveAnnotation(string name, bool fromDataAnnotation)
        => CanRemoveAnnotation(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
