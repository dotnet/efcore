// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal.Configuration
{
    public class IndexConfiguration : IFluentApiConfiguration
    {
        public IndexConfiguration(
            [NotNull] string lambdaIdentifier,
            [NotNull] Index index)
        {
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));
            Check.NotNull(index, nameof(index));

            LambdaIdentifier = lambdaIdentifier;
            Index = index;
        }

        public virtual string LambdaIdentifier { get; }
        public virtual Index Index { get; }

        public virtual bool HasAttributeEquivalent { get; }

        public virtual ICollection<string> FluentApiLines
        {
            get
            {
                var lines = new List<string>();
                lines.Add(nameof(EntityTypeBuilder<EntityType>.HasIndex) + "(" + LambdaIdentifier + " => "
                          + new ScaffoldingUtilities().GenerateLambdaToKey(Index.Properties, LambdaIdentifier) + ")");

                if (!string.IsNullOrEmpty(Index.Relational().Name))
                {
                    lines.Add("." + nameof(RelationalIndexBuilderExtensions.HasName) + "("
                              + CSharpUtilities.Instance.DelimitString(Index.Relational().Name) + ")");
                }

                if (Index.IsUnique)
                {
                    lines.Add("." + nameof(IndexBuilder.IsUnique) + "()");
                }

                return lines;
            }
        }
    }
}
