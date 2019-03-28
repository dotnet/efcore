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
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BackingFieldConvention : IPropertyAddedConvention, INavigationAddedConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BackingFieldConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

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
                var fieldInfo = TryMatchFieldName(
                    propertyBase.DeclaringType.Model, type, propertyBase.ClrType, propertyBase.Name);
                if (fieldInfo != null
                    && (propertyBase.PropertyInfo != null || propertyBase.Name == fieldInfo.GetSimpleMemberName()))
                {
                    return fieldInfo;
                }

                type = type.GetTypeInfo().BaseType;
            }

            return null;
        }

        private static FieldInfo TryMatchFieldName(IConventionModel model, Type entityClrType, Type propertyType, string propertyName)
        {
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

            var typeInfo = propertyType.GetTypeInfo();

            var match = TryMatch(sortedFields, "<", propertyName, ">k__BackingField", null, null, entityClrType, propertyName);
            if (match == null)
            {
                match = TryMatch(sortedFields, propertyName, "", "", typeInfo, null, entityClrType, propertyName);

                var camelPrefix = char.ToLowerInvariant(propertyName[0]).ToString();
                var camelizedSuffix = propertyName.Substring(1);

                match = TryMatch(sortedFields, camelPrefix, camelizedSuffix, "", typeInfo, match, entityClrType, propertyName);
                match = TryMatch(sortedFields, "_", camelPrefix, camelizedSuffix, typeInfo, match, entityClrType, propertyName);
                match = TryMatch(sortedFields, "_", "", propertyName, typeInfo, match, entityClrType, propertyName);
                match = TryMatch(sortedFields, "m_", camelPrefix, camelizedSuffix, typeInfo, match, entityClrType, propertyName);
                match = TryMatch(sortedFields, "m_", "", propertyName, typeInfo, match, entityClrType, propertyName);
            }

            return match;
        }

        private static FieldInfo TryMatch(
            KeyValuePair<string, FieldInfo>[] array,
            string prefix,
            string middle,
            string suffix,
            TypeInfo typeInfo,
            FieldInfo existingMatch,
            Type entityClrType,
            string propertyName)
        {
            var index = PrefixBinarySearch(array, prefix, 0, array.Length - 1);
            if (index == -1)
            {
                return existingMatch;
            }

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
                            throw new InvalidOperationException(
                                CoreStrings.ConflictingBackingFields(
                                    propertyName, entityClrType.ShortDisplayName(), existingMatch.Name, newMatch.Name));
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
    }
}
