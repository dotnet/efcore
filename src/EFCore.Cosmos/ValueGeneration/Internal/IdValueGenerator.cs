// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
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
        public override bool GeneratesTemporaryValues => false;

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

            var pk = entityType.FindPrimaryKey();
            var discriminator = entityType.GetDiscriminatorValue();
            if (discriminator != null
                && !pk.Properties.Contains(entityType.GetDiscriminatorProperty()))
            {
                AppendString(builder, discriminator);
                builder.Append("|");
            }

            var partitionKey = entityType.GetPartitionKeyPropertyName() ?? CosmosClientWrapper.DefaultPartitionKey;
            foreach (var property in pk.Properties)
            {
                if (property.Name == partitionKey)
                {
                    continue;
                }

                AppendString(builder, entry.Property(property.Name).CurrentValue);
                builder.Append("|");
            }

            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        private void AppendString(StringBuilder builder, object propertyValue)
        {
            switch (propertyValue)
            {
                case string stringValue:
                    builder.Append(stringValue.Replace("|", "/|"));
                    return;
                case IEnumerable enumerable:
                    foreach (var item in enumerable)
                    {
                        builder.Append(item.ToString().Replace("|", "/|"));
                        builder.Append("|");
                    }

                    return;
                default:
                    builder.Append(propertyValue.ToString().Replace("|", "/|"));
                    return;
            }
        }
    }
}
