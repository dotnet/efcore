// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure.Internal
{
    public class InMemoryOptionsExtension : IDbContextOptionsExtension
    {
        private bool _ignoreTransactions;

        public InMemoryOptionsExtension()
        {
        }

        public InMemoryOptionsExtension([NotNull] InMemoryOptionsExtension copyFrom)
        {
            _ignoreTransactions = copyFrom._ignoreTransactions;
        }

        public virtual bool IgnoreTransactions
        {
            get { return _ignoreTransactions; }
            set { _ignoreTransactions = value; }
        }

        public virtual void ApplyServices(EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.AddInMemoryDatabase();
        }
    }
}
