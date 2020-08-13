// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     <para>
    ///         A convention that finds backing fields for properties based on their names:
    ///         * &lt;[property name]&gt;k__BackingField
    ///         * _[camel-cased property name]
    ///         * _[property name]
    ///         * m_[camel-cased property name]
    ///         * m_[property name]
    ///     </para>
    ///     <para>
    ///         The field type must be of a type that's assignable to or from the property type.
    ///         If more than one matching field is found an exception is thrown.
    ///     </para>
    /// </summary>
    public class BackingFieldConvention :
        IPropertyAddedConvention,
        INavigationAddedConvention,
        IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="BackingFieldConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public BackingFieldConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a property is added to the entity type.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            IConventionContext<IConventionPropertyBuilder> context)
        {
            var field = GetFieldToSet(propertyBuilder.Metadata);
            if (field != null)
            {
                propertyBuilder.HasField(field);
            }
        }

        /// <summary>
        ///     Called after a navigation is added to the entity type.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessNavigationAdded(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionNavigation navigation,
            IConventionContext<IConventionNavigation> context)
        {
            var field = GetFieldToSet(navigation);
            if (field != null)
            {
                relationshipBuilder.HasField(field, navigation.IsDependentToPrincipal());
            }
        }

        private FieldInfo GetFieldToSet(IConventionPropertyBase propertyBase)
        {
            if (propertyBase == null
                || !ConfigurationSource.Convention.Overrides(propertyBase.GetFieldInfoConfigurationSource()))
            {
                return null;
            }

            var type = propertyBase.DeclaringType.ClrType;
            while (type != null)
            {
                var fieldInfo = TryMatchFieldName(propertyBase, type);
                if (fieldInfo != null
                    && (propertyBase.PropertyInfo != null || propertyBase.Name == fieldInfo.GetSimpleMemberName()))
                {
                    return fieldInfo;
                }

                type = type.GetTypeInfo().BaseType;
            }

            return null;
        }

        private static FieldInfo TryMatchFieldName(IConventionPropertyBase propertyBase, Type entityClrType)
        {
            var model = propertyBase.DeclaringType.Model;
            var propertyName = propertyBase.Name;

            IReadOnlyDictionary<string, FieldInfo> fields;
            var entityType = model.FindEntityType(entityClrType);
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
                var camelizedSuffix = propertyName.Substring(1);

                match = TryMatch(sortedFields, camelPrefix, camelizedSuffix, "", propertyBase, match, entityClrType, propertyName);
                match = TryMatch(sortedFields, "_", camelPrefix, camelizedSuffix, propertyBase, match, entityClrType, propertyName);
                match = TryMatch(sortedFields, "_", "", propertyName, propertyBase, match, entityClrType, propertyName);
                match = TryMatch(sortedFields, "m_", camelPrefix, camelizedSuffix, propertyBase, match, entityClrType, propertyName);
                match = TryMatch(sortedFields, "m_", "", propertyName, propertyBase, match, entityClrType, propertyName);
            }

            return match;
        }

        private static FieldInfo TryMatch(
            KeyValuePair<string, FieldInfo>[] array,
            string prefix,
            string middle,
            string suffix,
            IConventionPropertyBase propertyBase,
            FieldInfo existingMatch,
            Type entityClrType,
            string propertyName)
        {
            var index = PrefixBinarySearch(array, prefix, 0, array.Length - 1);
            if (index == -1)
            {
                return existingMatch;
            }

            var typeInfo = propertyBase?.ClrType.GetTypeInfo();
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
                        : (IsConvertible(typeInfo, currentValue.Value)
                            ? currentValue.Value
                            : null);

                    if (newMatch != null)
                    {
                        if (existingMatch != null
                            && newMatch != existingMatch)
                        {
                            propertyBase.SetOrRemoveAnnotation(
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

        private static bool IsConvertible(TypeInfo typeInfo, FieldInfo fieldInfo)
        {
            var fieldTypeInfo = fieldInfo.FieldType.GetTypeInfo();

            return typeInfo.IsAssignableFrom(fieldTypeInfo)
                || fieldTypeInfo.IsAssignableFrom(typeInfo);
        }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(
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
                            throw new InvalidOperationException((string)ambiguousField.Value);
                        }

                        property.RemoveAnnotation(CoreAnnotationNames.AmbiguousField);
                    }
                }
            }
        }
    }
}
