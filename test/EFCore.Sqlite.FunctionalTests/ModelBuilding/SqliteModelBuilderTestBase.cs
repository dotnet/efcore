// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqliteModelBuilderTestBase : RelationalModelBuilderTest
{
    public abstract class SqliteNonRelationship : RelationalNonRelationshipTestBase, IClassFixture<SqliteModelBuilderFixture>
    {
        public SqliteNonRelationship(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class SqliteComplexType : RelationalComplexTypeTestBase, IClassFixture<SqliteModelBuilderFixture>
    {
        public SqliteComplexType(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class SqliteInheritance : RelationalInheritanceTestBase, IClassFixture<SqliteModelBuilderFixture>
    {
        public SqliteInheritance(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class SqliteOneToMany : RelationalOneToManyTestBase, IClassFixture<SqliteModelBuilderFixture>
    {
        public SqliteOneToMany(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class SqliteManyToOne : RelationalManyToOneTestBase, IClassFixture<SqliteModelBuilderFixture>
    {
        public SqliteManyToOne(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class SqliteOneToOne : RelationalOneToOneTestBase, IClassFixture<SqliteModelBuilderFixture>
    {
        public SqliteOneToOne(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class SqliteManyToMany : RelationalManyToManyTestBase, IClassFixture<SqliteModelBuilderFixture>
    {
        public SqliteManyToMany(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class SqliteOwnedTypes : RelationalOwnedTypesTestBase, IClassFixture<SqliteModelBuilderFixture>
    {
        public override void Can_use_sproc_mapping_with_owned_reference()
            => Assert.Equal(SqliteStrings.StoredProceduresNotSupported("Book.Label#BookLabel"), 
                Assert.Throws<InvalidOperationException>(base.Can_use_sproc_mapping_with_owned_reference).Message);

        public SqliteOwnedTypes(SqliteModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public class SqliteModelBuilderFixture : RelationalModelBuilderFixture
    {
        public override TestHelpers TestHelpers => SqliteTestHelpers.Instance;
    }
}
