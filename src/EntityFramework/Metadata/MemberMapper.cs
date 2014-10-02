// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class MemberMapper
    {
        private readonly FieldMatcher _fieldMatcher;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected MemberMapper()
        {
        }

        public MemberMapper([NotNull] FieldMatcher fieldMatcher)
        {
            Check.NotNull(fieldMatcher, "fieldMatcher");

            _fieldMatcher = fieldMatcher;
        }

        // TODO: Consider doing this at model building time, but also consider mapping to interfaces
        // Issue #757
        public virtual IEnumerable<Tuple<IProperty, MemberInfo>> MapPropertiesToMembers([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var fieldCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            var propertyMappings = new List<Tuple<IProperty, MemberInfo>>();

            foreach (var property in entityType.Properties.Where(p => !p.IsShadowProperty))
            {
                var propertyName = property.Name;

                MemberInfo memberInfo = null;
                foreach (var propertyInfo in entityType.Type.GetPropertiesInHierarchy(propertyName))
                {
                    // TODO: Handle cases where backing field is declared in a different class than the property
                    // Issue #758
                    Dictionary<string, FieldInfo> fields;
                    if (!fieldCache.TryGetValue(propertyInfo.DeclaringType, out fields))
                    {
                        fields = propertyInfo.DeclaringType.GetRuntimeFields().ToDictionary(f => f.Name);
                        fieldCache[propertyInfo.DeclaringType] = fields;
                    }

                    var fieldName = property["BackingField"];
                    if (fieldName != null)
                    {
                        FieldInfo fieldInfo;
                        if (!fields.TryGetValue(fieldName, out fieldInfo))
                        {
                            throw new InvalidOperationException(Strings.FormatMissingBackingField(entityType.Name, propertyName, fieldName));
                        }
                        if (!fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(property.PropertyType.GetTypeInfo()))
                        {
                            throw new InvalidOperationException(
                                Strings.FormatBadBackingFieldType(fieldName, fieldInfo.FieldType.Name, entityType.Name, propertyName, property.PropertyType.Name));
                        }
                        memberInfo = fieldInfo;
                    }
                    else
                    {
                        memberInfo = _fieldMatcher.TryMatchFieldName(property, propertyInfo, fields);
                    }

                    if (memberInfo != null)
                    {
                        break;
                    }
                }

                if (memberInfo == null)
                {
                    memberInfo = entityType.Type.GetPropertiesInHierarchy(propertyName).FirstOrDefault(p => p.SetMethod != null);
                }

                if (memberInfo == null)
                {
                    throw new InvalidOperationException(Strings.FormatNoFieldOrSetter(entityType.Name, propertyName));
                }

                propertyMappings.Add(Tuple.Create(property, memberInfo));
            }

            return propertyMappings;
        }
    }
}
