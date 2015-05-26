// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityOptionsBuilder : IOptionsBuilderExtender
    {
        private EntityOptions _options;

        public EntityOptionsBuilder()
            : this(new EntityOptions<DbContext>())
        {
        }

        public EntityOptionsBuilder([NotNull] EntityOptions options)
        {
            Check.NotNull(options, nameof(options));

            _options = options;
        }

        public virtual EntityOptions Options => _options;

        public virtual bool IsConfigured => _options.Extensions.Any();

        public virtual EntityOptionsBuilder UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            ((IOptionsBuilderExtender)this).AddOrUpdateExtension(new CoreOptionsExtension { Model = model });

            return this;
        }

        void IOptionsBuilderExtender.AddOrUpdateExtension<TExtension>(TExtension extension)
        {
            Check.NotNull(extension, nameof(extension));

            _options = _options.WithExtension(extension);
        }
    }
}
