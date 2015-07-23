// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Tests.Infrastructure;
using Microsoft.Data.Entity.Tests.TestUtilities;
using Xunit;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

namespace Microsoft.Data.Entity.Tests
{
    public class RelationalModelValidatorTest : LoggingModelValidatorTest
    {
        [Fact]
        public virtual void Detects_duplicate_table_names()
        {
            var model = new Entity.Metadata.Model();
            var entityA = model.AddEntityType(typeof(A));
            var entityB = model.AddEntityType(typeof(B));
            entityA.Relational().TableName = "Table";
            entityB.Relational().TableName = "Table";

            VerifyError(Strings.DuplicateTableName("Table", null, entityB.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_duplicate_table_names_with_schema()
        {
            var model = new Entity.Metadata.Model();
            var entityA = model.AddEntityType(typeof(A));
            var entityB = model.AddEntityType(typeof(B));
            entityA.Relational().TableName = "Table";
            entityA.Relational().Schema = "Schema";
            entityB.Relational().TableName = "Table";
            entityB.Relational().Schema = "Schema";

            VerifyError(Strings.DuplicateTableName("Table", "Schema", entityB.DisplayName()), model);
        }

        [Fact]
        public virtual void Does_not_detects_duplicate_table_names_in_different_schema()
        {
            var model = new Entity.Metadata.Model();
            var entityA = model.AddEntityType(typeof(A));
            var entityB = model.AddEntityType(typeof(B));
            entityA.Relational().TableName = "Table";
            entityA.Relational().Schema = "SchemaA";
            entityB.Relational().TableName = "Table";
            entityB.Relational().Schema = "SchemaB";

            CreateModelValidator().Validate(model);
        }

        [Fact]
        public virtual void Does_not_detects_duplicate_table_names_for_inherited_entities()
        {
            var model = new Entity.Metadata.Model();
            var entityA = model.AddEntityType(typeof(A));
            var entityC = model.AddEntityType(typeof(C));
            entityC.BaseType = entityA;

            CreateModelValidator().Validate(model);
        }

        public class C : A
        {
        }

        protected override ModelValidator CreateModelValidator()
            => new RelationalModelValidator(
                new ListLoggerFactory(Log, l => l == typeof(ModelValidator).FullName),
                new TestMetadataExtensionProvider());
    }
}
