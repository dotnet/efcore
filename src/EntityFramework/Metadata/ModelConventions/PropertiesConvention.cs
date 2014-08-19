// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class PropertiesConvention : IModelConvention
    {
        private static readonly Type[] _propertyTypes = new[]
            {
                typeof(bool),
                typeof(byte),
                typeof(byte[]),
                typeof(char),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(decimal),
                typeof(double),
                typeof(float),
                typeof(Guid),
                typeof(int),
                typeof(long),
                typeof(sbyte),
                typeof(short),
                typeof(string),
                typeof(TimeSpan),
                typeof(uint),
                typeof(ulong),
                typeof(ushort)
            };

        public virtual void Apply([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Honor [NotMapped]
            if (entityType.HasClrType)
            {
                var primitiveProperties = entityType.Type.GetRuntimeProperties().Where(IsPrimitiveProperty);
                foreach (var propertyInfo in primitiveProperties)
                {
                    entityType.AddProperty(propertyInfo);
                }
            }
        }

        protected virtual bool IsValidProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return !propertyInfo.IsStatic()
                   && propertyInfo.GetIndexParameters().Length == 0
                   && propertyInfo.CanRead
                   && propertyInfo.CanWrite;
        }

        protected virtual bool IsPrimitiveProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            if (!IsValidProperty(propertyInfo))
            {
                return false;
            }

            var propertyType = propertyInfo.PropertyType.UnwrapNullableType();

            return _propertyTypes.Contains(propertyType)
                   || propertyType.GetTypeInfo().IsEnum;
        }
    }
}
