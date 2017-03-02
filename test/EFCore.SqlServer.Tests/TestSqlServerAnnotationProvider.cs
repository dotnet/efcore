// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
{
    public class TestSqlServerAnnotationProvider : TestAnnotationProvider
    {
        public override IRelationalModelAnnotations For(IModel model) => new SqlServerModelAnnotations(model);
        public override IRelationalPropertyAnnotations For(IProperty property) => new SqlServerPropertyAnnotations(property);
        public override IRelationalEntityTypeAnnotations For(IEntityType entityType) => new SqlServerEntityTypeAnnotations(entityType);
        public override IRelationalForeignKeyAnnotations For(IForeignKey foreignKey) => new RelationalForeignKeyAnnotations(foreignKey, SqlServerFullAnnotationNames.Instance);
        public override IRelationalIndexAnnotations For(IIndex index) => new SqlServerIndexAnnotations(index);
        public override IRelationalKeyAnnotations For(IKey key) => new SqlServerKeyAnnotations(key);
    }
}
