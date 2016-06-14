// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
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
    public class KeyFluentApiConfiguration : IFluentApiConfiguration
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public KeyFluentApiConfiguration(
            [NotNull] string lambdaIdentifier,
            [NotNull] Key key)
        {
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));
            Check.NotNull(key, nameof(key));

            LambdaIdentifier = lambdaIdentifier;
            Key = key;
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
        public virtual Key Key { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasAttributeEquivalent { get; set; }

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
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}({1} => {2})",
                        nameof(EntityTypeBuilder.HasKey),
                        LambdaIdentifier,
                        new ScaffoldingUtilities().GenerateLambdaToKey(Key.Properties, LambdaIdentifier))
                };

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
