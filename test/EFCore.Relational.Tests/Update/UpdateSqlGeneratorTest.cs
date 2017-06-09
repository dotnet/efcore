// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class UpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator()
            => new FakeSqlGenerator(
                new UpdateSqlGeneratorDependencies(
                    new RelationalSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    new FakeRelationalTypeMapper(
                        new RelationalTypeMapperDependencies())));

        protected override TestHelpers TestHelpers => RelationalTestHelpers.Instance;

        protected override string RowsAffected => "provider_specific_rowcount()";

        protected override string Identity => "provider_specific_identity()";
    }
}
