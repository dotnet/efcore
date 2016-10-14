// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public class TestModelSource : ModelSource
    {
        private readonly Action<ModelBuilder> _onModelCreating;

        public TestModelSource(
            Action<ModelBuilder> onModelCreating,
            IDbSetFinder setFinder,
            ICoreConventionSetBuilder coreConventionSetBuilder,
            IModelCustomizer modelCustomizer,
            IModelCacheKeyFactory modelCacheKeyFactory,
            CoreModelValidator coreModelValidator)
            : base(setFinder, coreConventionSetBuilder, modelCustomizer, modelCacheKeyFactory, coreModelValidator)
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
            CoreModelValidator.Validate(model);
            validator.Validate(model);

            return model;
        }
    }
}
