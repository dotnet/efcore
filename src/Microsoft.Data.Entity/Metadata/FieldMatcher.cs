// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
            Check.NotNull(property, "property");
            Check.NotNull(propertyInfo, "propertyInfo");
            Check.NotNull(dclaredFields, "dclaredFields");

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
