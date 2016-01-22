// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class FieldMatcher : IFieldMatcher
    {
        public virtual FieldInfo TryMatchFieldName(
            IProperty property, PropertyInfo propertyInfo, Dictionary<string, FieldInfo> declaredFields)
        {
            var propertyName = propertyInfo.Name;
            var propertyType = propertyInfo.PropertyType.GetTypeInfo();

            var camelized = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

            FieldInfo fieldInfo;
            return (declaredFields.TryGetValue(camelized, out fieldInfo)
                    && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                   || (declaredFields.TryGetValue("_" + camelized, out fieldInfo)
                       && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                   || (declaredFields.TryGetValue("_" + propertyName, out fieldInfo)
                       && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                   || (declaredFields.TryGetValue("m_" + camelized, out fieldInfo)
                       && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                   || (declaredFields.TryGetValue("m_" + propertyName, out fieldInfo)
                       && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                ? fieldInfo
                : null;
        }
    }
}
