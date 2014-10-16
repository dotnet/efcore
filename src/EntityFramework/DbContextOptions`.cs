// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity
{
    public class DbContextOptions<T> : DbContextOptions
    {
        public new virtual DbContextOptions<T> UseModel([NotNull] IModel model)
        {
            return (DbContextOptions<T>)base.UseModel(model);
        }

        protected internal virtual void ReadRawOptions([NotNull] IConfiguration configuration)
        {
            ReadRawOptions(configuration, typeof(T));
        }
    }
}
