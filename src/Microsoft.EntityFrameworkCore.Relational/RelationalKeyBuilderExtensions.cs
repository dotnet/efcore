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
    public static class RelationalKeyBuilderExtensions
    {
        public static KeyBuilder HasName([NotNull] this KeyBuilder keyBuilder, [CanBeNull] string name)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            keyBuilder.GetInfrastructure<InternalKeyBuilder>().Relational(ConfigurationSource.Explicit).HasName(name);

            return keyBuilder;
        }
    }
}
