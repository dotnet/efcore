// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Configuration
{
    public class KeyFluentApiConfiguration : IFluentApiConfiguration
    {
        public KeyFluentApiConfiguration(
            [NotNull] string lambdaIdentifier,
            [NotNull] Key key)
        {
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));
            Check.NotNull(key, nameof(key));

            LambdaIdentifier = lambdaIdentifier;
            Key = key;
        }

        public virtual string LambdaIdentifier { get; }
        public virtual Key Key { get; }

        public virtual bool HasAttributeEquivalent { get; set; }

        public virtual ICollection<string> FluentApiLines
        {
            get
            {
                var lines = new List<string>();
                lines.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}({1} => {2})",
                    nameof(EntityTypeBuilder.HasKey),
                    LambdaIdentifier,
                    new ScaffoldingUtilities().GenerateLambdaToKey(Key.Properties, LambdaIdentifier)));

                if (Key.Relational().Name != null)
                {
                    lines.Add("." + nameof(RelationalKeyBuilderExtensions.HasName)
                        + "(" + CSharpUtilities.Instance.DelimitString(Key.Relational().Name) + ")");
                }

                return lines;
            }
        }
    }
}
