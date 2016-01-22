// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class InMemoryDbContextOptionsBuilder
    {
        public InMemoryDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            OptionsBuilder = optionsBuilder;
        }

        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

        public virtual InMemoryDbContextOptionsBuilder IgnoreTransactions()
            => SetOption(e => e.IgnoreTransactions = true);

        protected virtual InMemoryDbContextOptionsBuilder SetOption([NotNull] Action<InMemoryOptionsExtension> setAction)
        {
            Check.NotNull(setAction, nameof(setAction));

            var extension = new InMemoryOptionsExtension(OptionsBuilder.Options.GetExtension<InMemoryOptionsExtension>());

            setAction(extension);

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }
    }
}
