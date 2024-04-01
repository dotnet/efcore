// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that finds backing fields for properties based on their names:
///     * &lt;[property name]&gt;k__BackingField
///     * _[camel-cased property name]
///     * _[property name]
///     * m_[camel-cased property name]
///     * m_[property name]
///     * [property name]_
/// </summary>
/// <remarks>
///     <para>
///         The field type must be of a type that's assignable to or from the property type.
///         If more than one matching field is found an exception is thrown.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public class BackingFieldConvention :
    IPropertyAddedConvention,
    INavigationAddedConvention,
    ISkipNavigationAddedConvention,
    IComplexPropertyAddedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="BackingFieldConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public BackingFieldConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
        => DiscoverField(propertyBuilder);

    /// <inheritdoc />
    public virtual void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        IConventionContext<IConventionNavigationBuilder> context)
        => DiscoverField(navigationBuilder);

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationAdded(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionContext<IConventionSkipNavigationBuilder> context)
        => DiscoverField(skipNavigationBuilder);

    /// <inheritdoc />
    public virtual void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
        => DiscoverField(propertyBuilder);

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var ambiguousField = property.FindAnnotation(CoreAnnotationNames.AmbiguousField);
                if (ambiguousField != null)
                {
                    if (property.GetFieldName() == null)
                    {
                        throw new InvalidOperationException((string?)ambiguousField.Value);
                    }

                    property.Builder.HasNoAnnotation(CoreAnnotationNames.AmbiguousField);
                }
            }
        }
    }

    private static void DiscoverField<TBuilder>(IConventionPropertyBaseBuilder<TBuilder> conventionPropertyBaseBuilder)
        where TBuilder : IConventionPropertyBaseBuilder<TBuilder>
    {
        if (ConfigurationSource.Convention.Overrides(conventionPropertyBaseBuilder.Metadata.GetFieldInfoConfigurationSource()))
        {
            var field = GetFieldToSet(conventionPropertyBaseBuilder.Metadata);
            if (field != null)
            {
                conventionPropertyBaseBuilder.HasField(field);
            }
        }
    }

    private static FieldInfo? GetFieldToSet(IConventionPropertyBase? propertyBase)
    {
        if (propertyBase == null
            || !ConfigurationSource.Convention.Overrides(propertyBase.GetFieldInfoConfigurationSource())
            || propertyBase.IsIndexerProperty()
            || propertyBase.IsShadowProperty())
        {
            return null;
        }

        var typeBase = propertyBase.DeclaringType;
        var type = typeBase.ClrType;
        var baseTypes = (typeBase as IConventionEntityType)?.GetAllBaseTypes().ToArray();
        while (type != null)
        {
            var fieldInfo = TryMatchFieldName(propertyBase, typeBase, type);
            if (fieldInfo != null
                && (propertyBase.PropertyInfo != null || propertyBase.Name == fieldInfo.GetSimpleMemberName()))
            {
                return fieldInfo;
            }

            type = type.BaseType;
            typeBase = baseTypes?.FirstOrDefault(et => et.ClrType == type);
        }

        return null;
    }

    private static FieldInfo? TryMatchFieldName(
        IConventionPropertyBase propertyBase,
        IConventionTypeBase? entityType,
        Type entityClrType)
    {
        var propertyName = propertyBase.Name;

        IReadOnlyDictionary<string, FieldInfo> fields;
        if (entityType == null)
        {
            var newFields = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
            foreach (var field in entityClrType.GetRuntimeFields())
            {
                if (!field.IsStatic
                    && !newFields.ContainsKey(field.Name))
                {
                    newFields[field.Name] = field;
                }
            }

            fields = newFields;
        }
        else
        {
            fields = entityType.GetRuntimeFields();
        }

        var sortedFields = fields.OrderBy(p => p.Key, StringComparer.Ordinal).ToArray();

        var match = TryMatch(sortedFields, "<", propertyName, ">k__BackingField", null, null, entityClrType, propertyName);
        if (match == null)
        {
            match = TryMatch(sortedFields, propertyName, "", "", propertyBase, null, entityClrType, propertyName);

            var camelPrefix = char.ToLowerInvariant(propertyName[0]).ToString();
            var camelizedSuffix = propertyName[1..];

            match = TryMatch(sortedFields, camelPrefix, camelizedSuffix, "", propertyBase, match, entityClrType, propertyName);
            match = TryMatch(sortedFields, "_", camelPrefix, camelizedSuffix, propertyBase, match, entityClrType, propertyName);
            match = TryMatch(sortedFields, "_", "", propertyName, propertyBase, match, entityClrType, propertyName);
            match = TryMatch(sortedFields, "m_", camelPrefix, camelizedSuffix, propertyBase, match, entityClrType, propertyName);
            match = TryMatch(sortedFields, "m_", "", propertyName, propertyBase, match, entityClrType, propertyName);
            match = TryMatch(sortedFields, "", camelPrefix + camelizedSuffix, "_", propertyBase, match, entityClrType, propertyName);
        }

        return match;
    }

    private static FieldInfo? TryMatch(
        KeyValuePair<string, FieldInfo>[] array,
        string prefix,
        string middle,
        string suffix,
        IConventionPropertyBase? propertyBase,
        FieldInfo? existingMatch,
        Type entityClrType,
        string propertyName)
    {
        var index = PrefixBinarySearch(array, prefix, 0, array.Length - 1);
        if (index == -1)
        {
            return existingMatch;
        }

        var typeInfo = propertyBase?.ClrType;
        var length = prefix.Length + middle.Length + suffix.Length;
        var currentValue = array[index];
        while (true)
        {
            if (currentValue.Key.Length == length
                && currentValue.Key.EndsWith(suffix, StringComparison.Ordinal)
                && currentValue.Key.IndexOf(middle, prefix.Length, StringComparison.Ordinal) == prefix.Length)
            {
                var newMatch = typeInfo == null
                    ? currentValue.Value
                    : (typeInfo.IsCompatibleWith(currentValue.Value.FieldType)
                        ? currentValue.Value
                        : null);

                if (newMatch != null)
                {
                    if (existingMatch != null
                        && newMatch != existingMatch)
                    {
                        propertyBase!.SetOrRemoveAnnotation(
                            CoreAnnotationNames.AmbiguousField,
                            CoreStrings.ConflictingBackingFields(
                                propertyName, entityClrType.ShortDisplayName(), existingMatch.Name, newMatch.Name));
                        return null;
                    }

                    return newMatch;
                }

                return existingMatch;
            }

            if (++index == array.Length)
            {
                return existingMatch;
            }

            currentValue = array[index];
            if (!currentValue.Key.StartsWith(prefix, StringComparison.Ordinal))
            {
                return existingMatch;
            }
        }
    }

    private static int PrefixBinarySearch<T>(KeyValuePair<string, T>[] array, string prefix, int left, int right)
    {
        var found = -1;
        while (true)
        {
            if (right < left)
            {
                return found;
            }

            var middle = (left + right) >> 1;
            var value = array[middle].Key;

            if (value.StartsWith(prefix, StringComparison.Ordinal))
            {
                found = middle;
            }
            else if (StringComparer.Ordinal.Compare(value, prefix) < 0)
            {
                left = middle + 1;
                continue;
            }

            right = middle - 1;
        }
    }
}
