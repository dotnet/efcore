// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Infrastructure
{
    public abstract class ModelValidatorBaseTest
    {
        [Fact]
        public virtual void Detects_shadow_keys()
        {
            var model = new Model();
            var entityType = model.AddEntityType("E");
            var keyProperty = entityType.AddProperty("Id", typeof(int), shadowProperty: true);
            entityType.AddKey(keyProperty);

            VerifyWarning(Strings.ShadowKey("{'Id'}", "E", "{'Id'}"), model);
        }

        protected virtual void Validate(IModel model)
        {
            CreateModelValidatorBase().Validate(model);
        }

        protected abstract void VerifyWarning(string expectedMessage, IModel model);

        protected abstract void VerifyError(string expectedMessage, IModel model);

        protected abstract ModelValidatorBase CreateModelValidatorBase();
    }
}
