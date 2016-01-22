// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public class TestModelSource : ModelSource
    {
        private readonly Action<ModelBuilder> _onModelCreating;

        public TestModelSource(Action<ModelBuilder> onModelCreating, IDbSetFinder setFinder, ICoreConventionSetBuilder coreConventionSetBuilder, IModelCustomizer modelCustomizer, IModelCacheKeyFactory modelCacheKeyFactory)
            : base(setFinder, coreConventionSetBuilder, modelCustomizer, modelCacheKeyFactory)
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

            model.Validate();
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
