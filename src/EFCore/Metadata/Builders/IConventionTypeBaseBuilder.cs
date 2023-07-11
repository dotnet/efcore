// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionTypeBase" /> from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionTypeBaseBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     Gets the type-like object being configured.
    /// </summary>
    new IConventionTypeBase Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionTypeBaseBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionTypeBaseBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionTypeBaseBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionTypeBaseBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionTypeBaseBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionTypeBaseBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure the property with the given name.
    ///     If no matching property exists, then a new property will be added.
    /// </summary>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="setTypeConfigurationSource">Indicates whether the type configuration source should be set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionPropertyBuilder? Property(
        Type propertyType,
        string propertyName,
        bool setTypeConfigurationSource = true,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure the property with the given member info.
    ///     If no matching property exists, then a new property will be added.
    /// </summary>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionPropertyBuilder? Property(MemberInfo memberInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given property can be added to this type.
    /// </summary>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be added.</returns>
    bool CanHaveProperty(
        Type? propertyType,
        string propertyName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given property can be added to this type.
    /// </summary>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be added.</returns>
    bool CanHaveProperty(MemberInfo memberInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure the indexer property with the given name.
    ///     If no matching property exists, then a new property will be added.
    /// </summary>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionPropertyBuilder? IndexerProperty(
        Type propertyType,
        string propertyName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given indexer property can be added to this type.
    /// </summary>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be added.</returns>
    bool CanHaveIndexerProperty(
        Type propertyType,
        string propertyName,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Creates a property with a name that's different from any existing properties.
    /// </summary>
    /// <param name="basePropertyName">The desired property name.</param>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="required">A value indicating whether the property is required.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionPropertyBuilder? CreateUniqueProperty(Type propertyType, string basePropertyName, bool required);

    /// <summary>
    ///     Returns the existing properties with the given names or creates them if matching CLR members are found.
    /// </summary>
    /// <param name="propertyNames">The names of the properties.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A list of properties if they exist on the type, <see langword="null" /> otherwise.</returns>
    IReadOnlyList<IConventionProperty>? GetOrCreateProperties(
        IReadOnlyList<string>? propertyNames,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the existing properties matching the given members or creates them.
    /// </summary>
    /// <param name="memberInfos">The type members.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A list of properties if they exist on the type, <see langword="null" /> otherwise.</returns>
    IReadOnlyList<IConventionProperty>? GetOrCreateProperties(
        IEnumerable<MemberInfo>? memberInfos,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes properties in the given list if they are not part of any metadata object.
    /// </summary>
    /// <param name="properties">The properties to remove.</param>
    IConventionTypeBaseBuilder RemoveUnusedImplicitProperties(IReadOnlyList<IConventionProperty> properties);

    /// <summary>
    ///     Removes a property from this type.
    /// </summary>
    /// <param name="property">The property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the property was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionTypeBaseBuilder? HasNoProperty(IConventionProperty property, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the property can be removed from this type.
    /// </summary>
    /// <param name="property">The property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be removed from this type.</returns>
    bool CanRemoveProperty(IConventionProperty property, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure the complex property with the given name.
    ///     If no matching property exists, then a new property will be added.
    /// </summary>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexType">The target complex type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionComplexPropertyBuilder? ComplexProperty(
        Type propertyType,
        string propertyName,
        Type? complexType = null,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure the complex property with the given member info.
    ///     If no matching property exists, then a new property will be added.
    /// </summary>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property.</param>
    /// <param name="complexType">The target complex type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionComplexPropertyBuilder? ComplexProperty(
        MemberInfo memberInfo,
        Type? complexType = null,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given complex property can be added to this type.
    /// </summary>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexType">The target complex type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be added.</returns>
    bool CanHaveComplexProperty(
        Type? propertyType,
        string propertyName,
        Type? complexType = null,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given complex property can be added to this type.
    /// </summary>
    /// <param name="memberInfo">The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property.</param>
    /// <param name="complexType">The target complex type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be added.</returns>
    bool CanHaveComplexProperty(
        MemberInfo memberInfo,
        Type? complexType = null,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure the complex indexer property with the given name.
    ///     If no matching property exists, then a new property will be added.
    /// </summary>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexType">The target complex type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the property if it exists on the type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionComplexPropertyBuilder? ComplexIndexerProperty(
        Type propertyType,
        string propertyName,
        Type? complexType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given complex indexer property can be added to this type.
    /// </summary>
    /// <param name="propertyType">The type of value the property will hold.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexType">The target complex type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be added.</returns>
    bool CanHaveComplexIndexerProperty(
        Type propertyType,
        string propertyName,
        Type? complexType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a complex property from this type.
    /// </summary>
    /// <param name="complexProperty">The complex property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the complex property was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionTypeBaseBuilder? HasNoComplexProperty(IConventionComplexProperty complexProperty, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the complex property can be removed from this type.
    /// </summary>
    /// <param name="complexProperty">The complex property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the complex property can be removed from this type.</returns>
    bool CanRemoveComplexProperty(IConventionComplexProperty complexProperty, bool fromDataAnnotation = false);

    /// <summary>
    ///     Indicates whether the given member name is ignored for the given configuration source.
    /// </summary>
    /// <param name="memberName">The name of the member that might be ignored.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     <see langword="false" /> if the complex type contains a member with the given name,
    ///     the given member name hasn't been ignored or it was ignored using a lower configuration source;
    ///     <see langword="true" /> otherwise.
    /// </returns>
    bool IsIgnored(string memberName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Excludes the given property from the complex type and prevents conventions from adding a matching property
    ///     or navigation to the type.
    /// </summary>
    /// <param name="memberName">The name of the member to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance so that additional configuration calls can be chained
    ///     if the given member was ignored, <see langword="null" /> otherwise.
    /// </returns>
    IConventionTypeBaseBuilder? Ignore(string memberName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given member name can be ignored from the given configuration source.
    /// </summary>
    /// <param name="memberName">The member name to be removed from the complex type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given member name can be ignored.</returns>
    /// <returns>
    ///     <see langword="false" /> if the complex type contains a member with the given name
    ///     that was configured using a higher configuration source;
    ///     <see langword="true" /> otherwise.
    /// </returns>
    bool CanIgnore(string memberName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this type.
    ///     This strategy indicates how the context detects changes to properties for an instance of the type.
    /// </summary>
    /// <param name="changeTrackingStrategy">
    ///     The change tracking strategy to be used.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the <see cref="ChangeTrackingStrategy" /> was set,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionTypeBaseBuilder? HasChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given change tracking strategy can be set from the current configuration source.
    /// </summary>
    /// <param name="changeTrackingStrategy">
    ///     The change tracking strategy to be used.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given change tracking strategy can be set.</returns>
    bool CanSetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for all properties of this type.
    /// </summary>
    /// <param name="propertyAccessMode">
    ///     The <see cref="PropertyAccessMode" /> to use for properties of this type.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    /// <returns>
    ///     The same builder instance if the <see cref="PropertyAccessMode" /> was set,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionTypeBaseBuilder? UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given <see cref="PropertyAccessMode" /> can be set from the current configuration source.
    /// </summary>
    /// <param name="propertyAccessMode">
    ///     The <see cref="PropertyAccessMode" /> to use for properties of this model.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given <see cref="PropertyAccessMode" /> can be set.</returns>
    bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);
}
