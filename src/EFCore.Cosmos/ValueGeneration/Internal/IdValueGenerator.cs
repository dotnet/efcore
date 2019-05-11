// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal
{
    public class IdValueGenerator : ValueGenerator
    {
        public override bool GeneratesTemporaryValues => false;

        protected override object NextValue([NotNull] EntityEntry entry)
        {
            var builder = new StringBuilder();

            var pk = entry.Metadata.FindPrimaryKey();
            var discriminator = entry.Metadata.GetDiscriminatorValue();
            if (discriminator != null
                && !pk.Properties.Contains(entry.Metadata.GetDiscriminatorProperty()))
            {
                AppendString(builder,discriminator);
                builder.Append("|");
            }

            foreach (var property in pk.Properties)
            {
                if (property.Name == "__partitionKey")
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
