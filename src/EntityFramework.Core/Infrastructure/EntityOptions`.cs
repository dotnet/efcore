// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class EntityOptions<TContext> : EntityOptions
        where TContext : DbContext
    {
        public EntityOptions()
            : base(new Dictionary<Type, IEntityOptionsExtension>())
        {
        }

        public EntityOptions(
            [NotNull] IReadOnlyDictionary<Type, IEntityOptionsExtension> extensions)
            : base(extensions)
        {
        }

        public override EntityOptions WithExtension<TExtension>(TExtension extension)
        {
            Check.NotNull(extension, nameof(extension));

            var extensions = Extensions.ToDictionary(p => p.GetType(), p => p);
            extensions[typeof(TExtension)] = extension;

            return new EntityOptions<TContext>(extensions);
        }
    }
}
