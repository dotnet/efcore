using System;
using Microsoft.EntityFrameworkCore.SqlServer.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer
{
    public class TypeMappingTests
    {
        [Fact]
        public void Maps_int_column()
        {
            var mapping = CreateMapper().FindMapping(
                new RelationalTypeMappingInfo(
                    storeTypeName: "int",
                    storeTypeNameBase: "int",
                    unicode: null,
                    size: null,
                    precision: null,
                    scale: null));

            Assert.Null(mapping);
        }

        [Fact]
        public void Maps_hierarchyid_column()
        {
            var mapping = CreateMapper().FindMapping(
                new RelationalTypeMappingInfo(
                    storeTypeName: SqlServerHierarchyIdTypeMappingSourcePlugin.SqlServerTypeName,
                    storeTypeNameBase: SqlServerHierarchyIdTypeMappingSourcePlugin.SqlServerTypeName,
                    unicode: null,
                    size: null,
                    precision: null,
                    scale: null));

            AssertMapping<HierarchyId>(mapping);
        }

        private static void AssertMapping<T>(
            RelationalTypeMapping mapping)
        {
            AssertMapping(typeof(T), mapping);
        }

        private static void AssertMapping(
            Type type,
            RelationalTypeMapping mapping)
        {
            Assert.Same(type, mapping.ClrType);
        }

        private static IRelationalTypeMappingSourcePlugin CreateMapper()
            => new SqlServerHierarchyIdTypeMappingSourcePlugin();
    }
}
