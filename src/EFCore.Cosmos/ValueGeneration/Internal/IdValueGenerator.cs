// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

            var discriminator = entry.Metadata.Cosmos().DiscriminatorValue;
            if (discriminator != null)
            {
                builder.Append(discriminator.ToString());
                builder.Append("|");
            }

            foreach (var property in entry.Metadata.FindPrimaryKey().Properties)
            {
                var sanitizedValue = entry.Property(property.Name).CurrentValue.ToString().Replace("|", "/|");
                builder.Append(sanitizedValue);
                builder.Append("|");
            }

            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }
    }
}
