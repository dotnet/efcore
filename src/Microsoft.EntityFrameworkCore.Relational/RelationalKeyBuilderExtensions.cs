// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="KeyBuilder" />.
    /// </summary>
    public static class RelationalKeyBuilderExtensions
    {
        /// <summary>
        ///     Configures the name of the key constraint in the database when targeting a relational database.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="name"> The name of the key. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static KeyBuilder HasName([NotNull] this KeyBuilder keyBuilder, [CanBeNull] string name)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            keyBuilder.GetInfrastructure<InternalKeyBuilder>().Relational(ConfigurationSource.Explicit).HasName(name);

            return keyBuilder;
        }
    }
}
