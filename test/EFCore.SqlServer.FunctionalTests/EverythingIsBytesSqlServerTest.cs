// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotAzureSql)]
public class EverythingIsBytesSqlServerTest(EverythingIsBytesSqlServerTest.EverythingIsBytesSqlServerFixture fixture)
    : BuiltInDataTypesTestBase<EverythingIsBytesSqlServerTest.EverythingIsBytesSqlServerFixture>(fixture)
{
    public override Task Can_read_back_mapped_enum_from_collection_first_or_default()
        // The query needs to generate TOP(1)
        => Task.CompletedTask;

    public override Task Can_read_back_bool_mapped_as_int_through_navigation()
        // Column is mapped as int rather than byte[]
        => Task.CompletedTask;

    public override Task Object_to_string_conversion()
        // Return values are string which byte[] cannot read
        => Task.CompletedTask;

    public override Task Can_compare_enum_to_constant()
        // Column is mapped as int rather than byte[]
        => Task.CompletedTask;

    public override Task Can_compare_enum_to_parameter()
        // Column is mapped as int rather than byte[]
        => Task.CompletedTask;

    public class EverythingIsBytesSqlServerFixture : BuiltInDataTypesFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => true;

        public override bool SupportsUnicodeToAnsiConversion
            => false;

        public override bool SupportsLargeStringComparisons
            => true;

        protected override string StoreName
            => "EverythingIsBytes";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerBytesTestStoreFactory.Instance;

        public override bool SupportsBinaryKeys
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => false;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base
                .AddOptions(builder)
                .ConfigureWarnings(c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Ignore<Animal>();
            modelBuilder.Ignore<AnimalIdentification>();
            modelBuilder.Ignore<AnimalDetails>();
        }
    }

    public class SqlServerBytesTestStoreFactory : SqlServerTestStoreFactory
    {
        public static new SqlServerBytesTestStoreFactory Instance { get; } = new();

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => base.AddProviderServices(
                serviceCollection.AddSingleton<IRelationalTypeMappingSource, SqlServerBytesTypeMappingSource>());
    }

    public class SqlServerBytesTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly SqlServerByteArrayTypeMapping _rowversion = new("rowversion", size: 8);
        private readonly SqlServerByteArrayTypeMapping _variableLengthBinary = new();
        private readonly SqlServerByteArrayTypeMapping _fixedLengthBinary = new(fixedLength: true);
        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        public SqlServerBytesTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
            => _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "binary varying", _variableLengthBinary },
                    { "binary", _fixedLengthBinary },
                    { "image", _variableLengthBinary },
                    { "rowversion", _rowversion },
                    { "varbinary", _variableLengthBinary }
                };

        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => FindRawMapping(mappingInfo)?.WithTypeMappingInfo(mappingInfo);

        private RelationalTypeMapping FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping)
                    || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
                {
                    return clrType == null
                        || mapping.ClrType == clrType
                            ? mapping
                            : null;
                }
            }

            if (clrType == typeof(byte[]))
            {
                if (mappingInfo.IsRowVersion == true)
                {
                    return _rowversion;
                }

                var isFixedLength = mappingInfo.IsFixedLength == true;

                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 900 : null);
                if (size > 8000)
                {
                    size = isFixedLength ? 8000 : null;
                }

                return new SqlServerByteArrayTypeMapping(
                    "varbinary(" + (size == null ? "max" : size.ToString()) + ")",
                    size,
                    isFixedLength,
                    storeTypePostfix: size == null ? StoreTypePostfix.None : null);
            }

            return null;
        }
    }
}
