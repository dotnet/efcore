// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MemberMapper : IMemberMapper
    {
        private readonly IFieldMatcher _fieldMatcher;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MemberMapper([NotNull] IFieldMatcher fieldMatcher)
        {
            _fieldMatcher = fieldMatcher;
        }

        // TODO: Consider doing this at model building time, but also consider mapping to interfaces
        // Issue #757
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Tuple<IProperty, MemberInfo>> MapPropertiesToMembers(IEntityType entityType)
        {
            var fieldCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            var propertyMappings = new List<Tuple<IProperty, MemberInfo>>();

            foreach (var property in entityType.GetProperties().Where(p => !p.IsShadowProperty))
            {
                var memberInfo = (MemberInfo)FindBackingField(property, fieldCache);

                if (memberInfo == null)
                {
                    memberInfo = entityType.ClrType.GetPropertiesInHierarchy(property.Name).FirstOrDefault(p => p.SetMethod != null);

                    if (memberInfo == null)
                    {
                        throw new InvalidOperationException(CoreStrings.NoFieldOrSetter(entityType.DisplayName(), property.Name));
                    }
                }


                propertyMappings.Add(Tuple.Create(property, memberInfo));
            }

            return propertyMappings;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual FieldInfo FindBackingField(IPropertyBase property) 
            => FindBackingField(property, new Dictionary<Type, Dictionary<string, FieldInfo>>());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private FieldInfo FindBackingField(
            IPropertyBase property,
            IDictionary<Type, Dictionary<string, FieldInfo>> fieldCache)
        {
            var typesInHierarchy = property.DeclaringEntityType.ClrType.GetTypesInHierarchy().ToList();

            var fieldName = property["BackingField"] as string;
            if (fieldName != null)
            {
                foreach (var type in typesInHierarchy)
                {
                    var fields = GetFields(type, fieldCache);
                    FieldInfo fieldInfo;
                    if (fields.TryGetValue(fieldName, out fieldInfo))
                    {
                        if (!fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(property.GetClrType().GetTypeInfo()))
                        {
                            throw new InvalidOperationException(
                                CoreStrings.BadBackingFieldType(
                                    fieldName,
                                    fieldInfo.FieldType.ShortDisplayName(),
                                    property.DeclaringEntityType.DisplayName(),
                                    property.Name,
                                    property.GetClrType().ShortDisplayName()));
                        }

                        return fieldInfo;
                    }
                }

                throw new InvalidOperationException(
                    CoreStrings.MissingBackingField(property.DeclaringEntityType.DisplayName(), property.Name, fieldName));
            }

            foreach (var type in typesInHierarchy)
            {
                var fields = GetFields(type, fieldCache);
                var fieldInfo = _fieldMatcher.TryMatchFieldName(property, property.GetPropertyInfo(), fields);
                if (fieldInfo != null)
                {
                    return fieldInfo;
                }
            }

            return null;
        }

        private static Dictionary<string, FieldInfo> GetFields(
            Type type, 
            IDictionary<Type, Dictionary<string, FieldInfo>> fieldCache)
        {
            Dictionary<string, FieldInfo> fields;
            if (!fieldCache.TryGetValue(type, out fields))
            {
                fields = type.GetRuntimeFields().ToDictionary(f => f.Name);
                fieldCache[type] = fields;
            }
            return fields;
        }
    }
}
