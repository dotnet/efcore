// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class FieldMatcher
    {
        public virtual FieldInfo TryMatchFieldName(
            [NotNull] IProperty property, [NotNull] PropertyInfo propertyInfo, [NotNull] Dictionary<string, FieldInfo> dclaredFields)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(propertyInfo, nameof(propertyInfo));
            Check.NotNull(dclaredFields, nameof(dclaredFields));

            var propertyName = propertyInfo.Name;
            var propertyType = propertyInfo.PropertyType.GetTypeInfo();

            var camelized = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

            FieldInfo fieldInfo;
            return (dclaredFields.TryGetValue(camelized, out fieldInfo)
                    && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                   || (dclaredFields.TryGetValue("_" + camelized, out fieldInfo)
                       && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                   || (dclaredFields.TryGetValue("_" + propertyName, out fieldInfo)
                       && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                   || (dclaredFields.TryGetValue("m_" + camelized, out fieldInfo)
                       && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                   || (dclaredFields.TryGetValue("m_" + propertyName, out fieldInfo)
                       && fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(propertyType))
                ? fieldInfo
                : null;
        }
    }
}
