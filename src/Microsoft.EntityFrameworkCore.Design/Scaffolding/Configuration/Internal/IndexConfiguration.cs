// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Configuration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IndexConfiguration : IFluentApiConfiguration
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IndexConfiguration(
            [NotNull] string lambdaIdentifier,
            [NotNull] Index index)
        {
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));
            Check.NotNull(index, nameof(index));

            LambdaIdentifier = lambdaIdentifier;
            Index = index;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string LambdaIdentifier { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index Index { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasAttributeEquivalent => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ICollection<string> FluentApiLines
        {
            get
            {
                var lines = new List<string>
                {
                    nameof(EntityTypeBuilder<EntityType>.HasIndex) + "(" + LambdaIdentifier + " => "
                    + new ScaffoldingUtilities().GenerateLambdaToKey(Index.Properties, LambdaIdentifier) + ")"
                };

                if (!string.IsNullOrEmpty(Index.Relational().Name))
                {
                    lines.Add($".{nameof(RelationalIndexBuilderExtensions.HasName)}({CSharpUtilities.Instance.DelimitString(Index.Relational().Name)})");
                }

                if (Index.IsUnique)
                {
                    lines.Add($".{nameof(IndexBuilder.IsUnique)}()");
                }

                if (Index.Relational().Filter != null)
                {
                    lines.Add($".{nameof(RelationalIndexBuilderExtensions.HasFilter)}(@\"{Index.Relational().Filter.Replace("\"", "\"\"")}\")");
                }

                return lines;
            }
        }
    }
}
