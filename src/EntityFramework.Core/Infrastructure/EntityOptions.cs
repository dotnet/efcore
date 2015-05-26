// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class EntityOptions : IEntityOptions
    {
        protected EntityOptions(
            [NotNull] IReadOnlyDictionary<Type, IEntityOptionsExtension> extensions)
        {
            Check.NotNull(extensions, nameof(extensions));

            _extensions = extensions;
        }

        public virtual IEnumerable<IEntityOptionsExtension> Extensions => _extensions.Values;

        public virtual TExtension FindExtension<TExtension>()
            where TExtension : class, IEntityOptionsExtension
        {
            IEntityOptionsExtension extension;
            return _extensions.TryGetValue(typeof(TExtension), out extension) ? (TExtension)extension : null;
        }

        public virtual TExtension GetExtension<TExtension>()
            where TExtension : class, IEntityOptionsExtension
        {
            var extension = FindExtension<TExtension>();
            if (extension == null)
            {
                throw new InvalidOperationException(Strings.OptionsExtensionNotFound(typeof(TExtension).Name));
            }
            return extension;
        }

        public abstract EntityOptions WithExtension<TExtension>([NotNull] TExtension extension)
            where TExtension : class, IEntityOptionsExtension;

        private readonly IReadOnlyDictionary<Type, IEntityOptionsExtension> _extensions;
    }
}
