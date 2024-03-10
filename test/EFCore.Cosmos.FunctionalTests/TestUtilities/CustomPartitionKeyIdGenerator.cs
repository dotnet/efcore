// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public class CustomPartitionKeyIdGenerator<T> : ValueGenerator<T>
{
    public override bool GeneratesTemporaryValues
        => false;

    public override T Next(EntityEntry entry)
        => (T)NextValue(entry);

    protected override object NextValue(EntityEntry entry)
    {
        var builder = new StringBuilder();
        var entityType = entry.Metadata;

        var primaryKey = entityType.FindPrimaryKey();
        var discriminator = entityType.GetDiscriminatorValue();
        if (discriminator != null
            && !primaryKey.Properties.Contains(entityType.FindDiscriminatorProperty()))
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

            var value = entry.Property(property).CurrentValue;

            var converter = property.GetTypeMapping().Converter;
            if (converter != null)
            {
                value = converter.ConvertToProvider(value);
            }

            // We don't allow the Id to be zero for our custom generator.
            if (value is 0)
            {
                return default;
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
