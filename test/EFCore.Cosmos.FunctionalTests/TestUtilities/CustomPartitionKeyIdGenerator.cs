// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class CustomPartitionKeyIdGenerator<T> : ValueGenerator<T>
    {
        public override bool GeneratesTemporaryValues
            => false;

        public override T Next(EntityEntry entry)
        {
            return (T)NextValue(entry);
        }

        protected override object NextValue(EntityEntry entry)
        {
            var builder = new StringBuilder();
            var entityType = entry.Metadata;

            var primaryKey = entityType.FindPrimaryKey();
            var discriminator = entityType.GetDiscriminatorValue();
            if (discriminator != null
                && !primaryKey.Properties.Contains(entityType.GetDiscriminatorProperty()))
            {
                AppendString(builder, discriminator);
                builder.Append("-");
            }

            var partitionKey = entityType.GetPartitionKeyPropertyName();
            foreach (var property in primaryKey.Properties)
            {
                if (property.Name == partitionKey
                    || property.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName)
                {
                    continue;
                }

                var value = entry.Property(property.Name).CurrentValue;

                var converter = property.GetTypeMapping().Converter;
                if (converter != null)
                {
                    value = converter.ConvertToProvider(value);
                }

                if (value is int x)
                {
                    // We don't allow the Id to be zero for our custom generator.
                    if (x == 0)
                    {
                        return default;
                    }
                }

                AppendString(builder, value);

                builder.Append("-");
            }

            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        private static void AppendString(StringBuilder builder, object propertyValue)
        {
            switch (propertyValue)
            {
                case string stringValue:
                    builder.Append(stringValue.Replace("-", "/-"));
                    return;
                case IEnumerable enumerable:
                    foreach (var item in enumerable)
                    {
                        builder.Append(item.ToString().Replace("-", "/-"));
                        builder.Append("|");
                    }

                    return;
                default:
                    builder.Append(propertyValue == null ? "null" : propertyValue.ToString().Replace("-", "/-"));
                    return;
            }
        }
    }
}
