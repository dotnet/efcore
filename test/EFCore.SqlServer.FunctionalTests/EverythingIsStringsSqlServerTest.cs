// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotAzureSql)]
public class EverythingIsStringsSqlServerTest(EverythingIsStringsSqlServerTest.EverythingIsStringsSqlServerFixture fixture)
    : BuiltInDataTypesTestBase<
        EverythingIsStringsSqlServerTest.EverythingIsStringsSqlServerFixture>(fixture)
{
    public override Task Can_read_back_mapped_enum_from_collection_first_or_default()
        // The query needs to generate TOP(1)
        => Task.CompletedTask;

    public override Task Can_read_back_bool_mapped_as_int_through_navigation()
        // Column is mapped as int rather than string
        => Task.CompletedTask;

    public override Task Can_compare_enum_to_constant()
        // Column is mapped as int rather than string
        => Task.CompletedTask;

    public override Task Can_compare_enum_to_parameter()
        // Column is mapped as int rather than string
        => Task.CompletedTask;

    public class EverythingIsStringsSqlServerFixture : BuiltInDataTypesFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => true;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        public override bool PreservesDateTimeKind
            => false;

        protected override string StoreName
            => "EverythingIsStrings";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerStringsTestStoreFactory.Instance;

        public override bool SupportsBinaryKeys
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base
                .AddOptions(builder)
                .ConfigureWarnings(c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<MaxLengthDataTypes>().Property(e => e.ByteArray5).HasMaxLength(8);

            modelBuilder.Ignore<Animal>();
            modelBuilder.Ignore<AnimalIdentification>();
            modelBuilder.Ignore<AnimalDetails>();
        }
    }

    public class SqlServerStringsTestStoreFactory : SqlServerTestStoreFactory
    {
        public static new SqlServerStringsTestStoreFactory Instance { get; } = new();

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => base.AddProviderServices(
                serviceCollection.AddSingleton<IRelationalTypeMappingSource, SqlServerStringsTypeMappingSource>());
    }

    public class SqlServerStringsTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly SqlServerStringTypeMapping _fixedLengthUnicodeString = new(unicode: true, fixedLength: true);
        private readonly SqlServerStringTypeMapping _variableLengthUnicodeString = new(unicode: true);
        private readonly SqlServerStringTypeMapping _fixedLengthAnsiString = new(fixedLength: true);
        private readonly SqlServerStringTypeMapping _variableLengthAnsiString = new();
        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        public SqlServerStringsTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
            => _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "char varying", _variableLengthAnsiString },
                    { "char", _fixedLengthAnsiString },
                    { "character varying", _variableLengthAnsiString },
                    { "character", _fixedLengthAnsiString },
                    { "national char varying", _variableLengthUnicodeString },
                    { "national character varying", _variableLengthUnicodeString },
                    { "national character", _fixedLengthUnicodeString },
                    { "nchar", _fixedLengthUnicodeString },
                    { "ntext", _variableLengthUnicodeString },
                    { "nvarchar", _variableLengthUnicodeString },
                    { "text", _variableLengthAnsiString },
                    { "varchar", _variableLengthAnsiString }
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

            if (clrType == typeof(string))
            {
                var isAnsi = mappingInfo.IsUnicode == false;
                var isFixedLength = mappingInfo.IsFixedLength == true;
                var baseName = isAnsi ? "varchar" : "nvarchar";
                var maxSize = isAnsi ? 8000 : 4000;

                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? isAnsi ? 900 : 450 : null);
                if (size > maxSize)
                {
                    size = isFixedLength ? maxSize : null;
                }

                return new SqlServerStringTypeMapping(
                    baseName + "(" + (size == null ? "max" : size.ToString()) + ")",
                    !isAnsi,
                    size,
                    isFixedLength,
                    storeTypePostfix: size == null ? StoreTypePostfix.None : null);
            }

            return null;
        }
    }
}
