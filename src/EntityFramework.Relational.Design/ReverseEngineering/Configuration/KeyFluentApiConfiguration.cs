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
        private readonly IReadOnlyList<Property> _properties;
        private readonly string _lambdaIdentifier;

        public KeyFluentApiConfiguration(
            [NotNull] string lambdaIdentifier,
            [NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));
            Check.NotEmpty(properties, nameof(properties));

            _lambdaIdentifier = lambdaIdentifier;
            _properties = new List<Property>(properties);
        }

        public virtual bool IsPrimaryKey { get; set; }
        public virtual bool HasAttributeEquivalent { get; set; }
        public virtual string For { get; }

        public virtual string FluentApi => string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1} => {2})",
                IsPrimaryKey ? nameof(EntityTypeBuilder.HasKey) : nameof(EntityTypeBuilder.HasAlternateKey),
                _lambdaIdentifier,
                new ModelUtilities().GenerateLambdaToKey(_properties, _lambdaIdentifier));
    }
}
