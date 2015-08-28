// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class KeyFluentApiConfiguration : IFluentApiConfiguration
    {
        public KeyFluentApiConfiguration(
            [NotNull] string lambdaIdentifier, 
            [NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));
            Check.NotEmpty(properties, nameof(properties));

            FluentApi = string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1} => {2})",
                nameof(EntityTypeBuilder.Key),
                lambdaIdentifier,
                new ModelUtilities().GenerateLambdaToKey(properties, lambdaIdentifier));
        }

        public virtual bool HasAttributeEquivalent { get; set; } = false;
        public virtual string For { get; } = null;

        public virtual string FluentApi { get;[param: NotNull] private set; }
    }
}
