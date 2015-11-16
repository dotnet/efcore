// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Configuration
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
        public virtual string For { get; }

        public virtual string FluentApi
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(nameof(EntityTypeBuilder<EntityType>.HasIndex) + "(");
                sb.Append(LambdaIdentifier);
                sb.Append(" => ");
                sb.Append(new ScaffoldingUtilities().GenerateLambdaToKey(Index.Properties, LambdaIdentifier));
                sb.Append(")");

                if (!string.IsNullOrEmpty(Index.Relational().Name))
                {
                    sb.Append("." + nameof(RelationalIndexBuilderExtensions.HasName) + "(");
                    sb.Append(CSharpUtilities.Instance.DelimitString(Index.Relational().Name));
                    sb.Append(")");
                }

                if (Index.IsUnique)
                {
                    sb.Append("." + nameof(IndexBuilder.IsUnique) + "()");
                }

                return sb.ToString();
            }
        }
    }
}
