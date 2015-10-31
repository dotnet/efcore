// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class TestModelSource : ModelSource
    {
        private readonly Action<ModelBuilder> _onModelCreating;

        public TestModelSource(
            Action<ModelBuilder> onModelCreating,
            IDbSetFinder setFinder,
            ICoreConventionSetBuilder coreConventionSetBuilder)
            : base(setFinder, coreConventionSetBuilder)
        {
            _onModelCreating = onModelCreating;
        }

        protected override IModel CreateModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
        {
            var conventionSet = CreateConventionSet(conventionSetBuilder);

            var modelBuilder = new ModelBuilder(conventionSet);
            var model = (Model)modelBuilder.Model;
            model.SetProductVersion(ProductInfo.GetVersion());

            FindSets(modelBuilder, context);

            _onModelCreating(modelBuilder);

            modelBuilder.Validate();

            validator.Validate(model);

            return model;
        }

        private class ThrowingModelValidator : ModelValidator
        {
            protected override void ShowWarning(string message)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
