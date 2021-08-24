// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class IdValueGenerator : ValueGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool GeneratesTemporaryValues
            => false;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool GeneratesStableValues
            => true;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override object NextValue(EntityEntry entry)
        {
            var builder = new StringBuilder();
            var entityType = entry.Metadata;

            var primaryKey = entityType.FindPrimaryKey()!;
            var discriminator = entityType.GetDiscriminatorValue();
            if (discriminator != null
                && !primaryKey.Properties.Contains(entityType.FindDiscriminatorProperty()))
            {
                AppendString(builder, discriminator);
                builder.Append('|');
            }

            var partitionKey = entityType.GetPartitionKeyPropertyName();
            foreach (var property in primaryKey.Properties)
            {
                if (property.Name == partitionKey
                    && primaryKey.Properties.Count > 1)
                {
                    continue;
                }

                var value = entry.Property(property.Name).CurrentValue;

                var converter = property.GetTypeMapping().Converter;
                if (converter != null)
                {
                    value = converter.ConvertToProvider(value);
                }

                AppendString(builder, value);

                builder.Append('|');
            }

            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        private void AppendString(StringBuilder builder, object? propertyValue)
        {
            switch (propertyValue)
            {
                case string stringValue:
                    builder.Append(stringValue.Replace("|", "^|"));
                    return;
                case IEnumerable enumerable:
                    foreach (var item in enumerable)
                    {
                        builder.Append(item.ToString()!.Replace("|", "^|"));
                        builder.Append('|');
                    }

                    return;
                default:
                    builder.Append(propertyValue == null ? "null" : propertyValue.ToString()!.Replace("|", "^|"));
                    return;
            }
        }
    }
}
