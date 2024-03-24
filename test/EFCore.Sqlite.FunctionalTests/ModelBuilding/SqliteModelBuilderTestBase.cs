// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqliteModelBuilderTestBase : RelationalModelBuilderTest
{
    public abstract class SqliteNonRelationship(SqliteModelBuilderFixture fixture) : RelationalNonRelationshipTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteComplexType(SqliteModelBuilderFixture fixture) : RelationalComplexTypeTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteInheritance(SqliteModelBuilderFixture fixture) : RelationalInheritanceTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteOneToMany(SqliteModelBuilderFixture fixture) : RelationalOneToManyTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteManyToOne(SqliteModelBuilderFixture fixture) : RelationalManyToOneTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteOneToOne(SqliteModelBuilderFixture fixture) : RelationalOneToOneTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteManyToMany(SqliteModelBuilderFixture fixture) : RelationalManyToManyTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteOwnedTypes(SqliteModelBuilderFixture fixture) : RelationalOwnedTypesTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>
    {
        public override void Can_use_sproc_mapping_with_owned_reference()
            => Assert.Equal(SqliteStrings.StoredProceduresNotSupported("Book.Label#BookLabel"),
                Assert.Throws<InvalidOperationException>(base.Can_use_sproc_mapping_with_owned_reference).Message);
    }

    public class SqliteModelBuilderFixture : RelationalModelBuilderFixture
    {
        public override TestHelpers TestHelpers => SqliteTestHelpers.Instance;
    }
}
