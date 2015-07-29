// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class ModelDifferTestBase
    {
        protected void Execute(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<IReadOnlyList<MigrationOperation>> assertAction)
        {
            var sourceModelBuilder = CreateModelBuilder();
            buildSourceAction(sourceModelBuilder);

            var targetModelBuilder = CreateModelBuilder();
            buildTargetAction(targetModelBuilder);

            var modelDiffer = CreateModelDiffer();

            var operations = modelDiffer.GetDifferences(sourceModelBuilder.Model, targetModelBuilder.Model);

            assertAction(operations);
        }

        protected virtual ModelBuilder CreateModelBuilder() => TestHelpers.Instance.CreateConventionBuilder();
        protected virtual ModelDiffer CreateModelDiffer()
            => new ModelDiffer(new TestMetadataExtensionProvider(), new MigrationAnnotationProvider());
    }
}
