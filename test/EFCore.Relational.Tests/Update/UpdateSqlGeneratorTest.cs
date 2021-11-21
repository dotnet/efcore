// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Update;

public class UpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
{
    protected override IUpdateSqlGenerator CreateSqlGenerator()
        => new FakeSqlGenerator(
            new UpdateSqlGeneratorDependencies(
                new RelationalSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));

    protected override TestHelpers TestHelpers
        => RelationalTestHelpers.Instance;

    protected override string RowsAffected
        => "provider_specific_rowcount()";

    protected override string Identity
        => "provider_specific_identity()";
}
