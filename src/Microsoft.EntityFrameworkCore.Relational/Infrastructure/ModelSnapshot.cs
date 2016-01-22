// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public abstract class ModelSnapshot
    {
        private readonly LazyRef<IModel> _model;

        protected ModelSnapshot()
        {
            _model = new LazyRef<IModel>(
                () =>
                    {
                        var modelBuilder = new ModelBuilder(new ConventionSet());
                        BuildModel(modelBuilder);

                        return modelBuilder.Model;
                    });
        }

        public virtual IModel Model => _model.Value;

        protected abstract void BuildModel([NotNull] ModelBuilder modelBuilder);
    }
}
