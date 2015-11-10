// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Configuration
{
    public class KeyFluentApiConfiguration : IFluentApiConfiguration
    {
        public KeyFluentApiConfiguration(
            [NotNull] string lambdaIdentifier,
            [NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));
            Check.NotEmpty(properties, nameof(properties));

            LambdaIdentifier = lambdaIdentifier;
            Properties = new List<Property>(properties);
        }

        public virtual string LambdaIdentifier { get; }
        public virtual IReadOnlyList<Property> Properties { get; }

        public virtual bool HasAttributeEquivalent { get; set; }
        public virtual string For { get; }

        public virtual string FluentApi => string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1} => {2})",
                nameof(EntityTypeBuilder.HasKey),
                LambdaIdentifier,
                new ScaffoldingUtilities().GenerateLambdaToKey(Properties, LambdaIdentifier));
    }
}
