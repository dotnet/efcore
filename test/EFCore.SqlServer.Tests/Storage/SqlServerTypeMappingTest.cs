// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Storage;

public class SqlServerTypeMappingTest : RelationalTypeMappingTest
{
    [ConditionalTheory]
    [InlineData(nameof(ChangeTracker.DetectChanges), false)]
    [InlineData(nameof(PropertyEntry.CurrentValue), false)]
    [InlineData(nameof(PropertyEntry.OriginalValue), false)]
    [InlineData(nameof(ChangeTracker.DetectChanges), true)]
    [InlineData(nameof(PropertyEntry.CurrentValue), true)]
    [InlineData(nameof(PropertyEntry.OriginalValue), true)]
    public void Row_version_is_marked_as_modified_only_if_it_really_changed(string mode, bool changeValue)
    {
        using var context = new OptimisticContext();
        var token = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var newToken = changeValue ? [1, 2, 3, 4, 0, 6, 7, 8] : token;

        var entity = context.Attach(
            new WithRowVersion { Id = 789, Version = token.ToArray() }).Entity;

        var propertyEntry = context.Entry(entity).Property(e => e.Version);

        Assert.Equal(token, propertyEntry.CurrentValue);
        Assert.Equal(token, propertyEntry.OriginalValue);
        Assert.False(propertyEntry.IsModified);
        Assert.Equal(EntityState.Unchanged, context.Entry(entity).State);

        switch (mode)
        {
            case nameof(ChangeTracker.DetectChanges):
                entity.Version = newToken.ToArray();
                context.ChangeTracker.DetectChanges();
                break;
            case nameof(PropertyEntry.CurrentValue):
                propertyEntry.CurrentValue = newToken.ToArray();
                break;
            case nameof(PropertyEntry.OriginalValue):
                propertyEntry.OriginalValue = newToken.ToArray();
                break;
            default:
                throw new NotImplementedException("Unexpected test mode.");
        }

        Assert.Equal(changeValue, propertyEntry.IsModified);
        Assert.Equal(changeValue ? EntityState.Modified : EntityState.Unchanged, context.Entry(entity).State);
    }

    private class WithRowVersion
    {
        public int Id { get; set; }
        public byte[] Version { get; set; }
    }

    private class OptimisticContext : DbContext
    {
        public DbSet<WithRowVersion> _ { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer("Data Source=Branston");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<WithRowVersion>().Property(e => e.Version).IsRowVersion();
    }

    protected override DbCommand CreateTestCommand()
        => new SqlCommand();

    [ConditionalTheory]
    [InlineData(typeof(SqlServerDateTimeOffsetTypeMapping), typeof(DateTimeOffset))]
    [InlineData(typeof(SqlServerDoubleTypeMapping), typeof(double))]
    [InlineData(typeof(SqlServerFloatTypeMapping), typeof(float))]
    [InlineData(typeof(SqlServerTimeSpanTypeMapping), typeof(TimeSpan))]
    public override void Create_and_clone_with_converter(Type mappingType, Type type)
        => base.Create_and_clone_with_converter(mappingType, type);

    [ConditionalFact]
    public void Create_and_clone_SQL_Server_DateTime_mappings_with_converter()
    {
        var mapping = (RelationalTypeMapping)Activator.CreateInstance(
            typeof(SqlServerDateTimeTypeMapping),
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
            null,
            [FakeTypeMapping.CreateParameters(typeof(SqlServerDateTimeTypeMapping)), SqlDbType.SmallDateTime],
            null,
            null);

        var clone = AssertClone(typeof(SqlServerDateTimeTypeMapping), mapping);

        Assert.Equal(SqlDbType.SmallDateTime, ((SqlServerDateTimeTypeMapping)clone).SqlType);
    }

    [ConditionalFact]
    public virtual void Create_and_clone_SQL_Server_sized_mappings_with_converter()
        => ConversionCloneTest(
            typeof(SqlServerByteArrayTypeMapping),
            typeof(byte[]),
            SqlDbType.Image);

    [ConditionalFact]
    public virtual void Create_and_clone_SQL_Server_unicode_sized_mappings_with_converter()
        => UnicodeConversionCloneTest(
            typeof(SqlServerStringTypeMapping),
            typeof(string),
            SqlDbType.Text);

    [ConditionalFact]
    public virtual void Create_and_clone_UDT_mapping_with_converter()
    {
        Func<object, Expression> literalGenerator = Expression.Constant;

        var mapping = new SqlServerUdtTypeMapping(
            typeof(object),
            "storeType",
            literalGenerator,
            StoreTypePostfix.None,
            "udtType",
            CreateConverter(typeof(object)),
            CreateComparer(typeof(object)),
            CreateComparer(typeof(object)),
            DbType.VarNumeric,
            false,
            33,
            true);

        var clone = (SqlServerUdtTypeMapping)mapping.WithStoreTypeAndSize("<clone>", 66);

        Assert.NotSame(mapping, clone);
        Assert.Same(mapping.GetType(), clone.GetType());
        Assert.Equal("storeType", mapping.StoreType);
        Assert.Equal("<clone>", clone.StoreType);
        Assert.Equal("udtType", mapping.UdtTypeName);
        Assert.Equal("udtType", clone.UdtTypeName);
        Assert.Equal(DbType.VarNumeric, clone.DbType);
        Assert.Equal(33, mapping.Size);
        Assert.Equal(66, clone.Size);
        Assert.False(mapping.IsUnicode);
        Assert.False(clone.IsUnicode);
        Assert.NotNull(mapping.Converter);
        Assert.Same(mapping.Converter, clone.Converter);
        Assert.Same(mapping.Comparer, clone.Comparer);
        Assert.Same(mapping.KeyComparer, clone.KeyComparer);
        Assert.Same(typeof(object), clone.ClrType);
        Assert.True(mapping.IsFixedLength);
        Assert.True(clone.IsFixedLength);
        Assert.Same(literalGenerator, clone.LiteralGenerator);

        var newConverter = CreateConverter(typeof(object));
        clone = (SqlServerUdtTypeMapping)mapping.WithComposedConverter(newConverter);

        Assert.NotSame(mapping, clone);
        Assert.Same(mapping.GetType(), clone.GetType());
        Assert.Equal("storeType", mapping.StoreType);
        Assert.Equal("storeType", clone.StoreType);
        Assert.Equal("udtType", mapping.UdtTypeName);
        Assert.Equal("udtType", clone.UdtTypeName);
        Assert.Equal(DbType.VarNumeric, clone.DbType);
        Assert.Equal(33, mapping.Size);
        Assert.Equal(33, clone.Size);
        Assert.False(mapping.IsUnicode);
        Assert.False(clone.IsUnicode);
        Assert.NotSame(mapping.Converter, clone.Converter);
        Assert.Same(mapping.Comparer, clone.Comparer);
        Assert.Same(mapping.KeyComparer, clone.KeyComparer);
        Assert.Same(typeof(object), clone.ClrType);
        Assert.True(mapping.IsFixedLength);
        Assert.True(clone.IsFixedLength);
        Assert.Same(literalGenerator, clone.LiteralGenerator);
    }

    public static RelationalTypeMapping GetMapping(Type type)
        => new SqlServerTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())
            .FindMapping(type);

    public override void ByteArray_literal_generated_correctly()
        => Test_GenerateSqlLiteral_helper(GetMapping(typeof(byte[])), new byte[] { 0xDA, 0x7A }, "0xDA7A");

    public override void Byte_literal_generated_correctly()
    {
        var typeMapping = GetMapping(typeof(byte));

        Test_GenerateSqlLiteral_helper(typeMapping, byte.MinValue, "CAST(0 AS tinyint)");
        Test_GenerateSqlLiteral_helper(typeMapping, byte.MaxValue, "CAST(255 AS tinyint)");
    }

    public override void DateTimeOffset_literal_generated_correctly()
    {
        Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(DateTimeOffset)),
            new DateTimeOffset(2015, 3, 12, 13, 36, 37, new TimeSpan(-7, 0, 0)).AddTicks(1234567),
            "'2015-03-12T13:36:37.1234567-07:00'");

        Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(DateTimeOffset)),
            new DateTimeOffset(2015, 3, 12, 13, 36, 37, new TimeSpan(-7, 0, 0)).AddTicks(1230000),
            "'2015-03-12T13:36:37.1230000-07:00'"); //should this really output trailing zeros?
    }

    [ConditionalFact]
    public override void TimeOnly_literal_generated_correctly()
    {
        var typeMapping = GetMapping(typeof(TimeOnly));

        Test_GenerateSqlLiteral_helper(typeMapping, new TimeOnly(13, 10, 15), "'13:10:15'");
        Test_GenerateSqlLiteral_helper(typeMapping, new TimeOnly(13, 10, 15, 120), "'13:10:15.12'");
        Test_GenerateSqlLiteral_helper(typeMapping, new TimeOnly(13, 10, 15, 120, 20), "'13:10:15.12002'");
    }

    [ConditionalFact]
    public override void DateOnly_literal_generated_correctly()
        => Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(DateOnly)),
            new DateOnly(2015, 3, 12),
            "'2015-03-12'");

    [ConditionalFact]
    public override void Timespan_literal_generated_correctly()
    {
        Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(TimeSpan)),
            new TimeSpan(13, 10, 15),
            "'13:10:15'");

        Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(TimeSpan)),
            new TimeSpan(0, 13, 10, 15, 120),
            "'13:10:15.12'");

        Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(TimeSpan)),
            new TimeSpan(0, 13, 10, 15, 120, 20),
            "'13:10:15.12002'");
    }

    public override void DateTime_literal_generated_correctly()
    {
        Test_GenerateSqlLiteral_helper(
            GetMapping(typeof(DateTime)),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.1234567Z'");

        Test_GenerateSqlLiteral_helper(
            GetMapping("date"),
            new DateTime(2015, 3, 12, 13, 36, 37, 371, DateTimeKind.Utc),
            "'2015-03-12'");

        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime"),
            new DateTime(2015, 3, 12, 13, 36, 37, 371, DateTimeKind.Utc),
            "'2015-03-12T13:36:37.371'");

        Test_GenerateSqlLiteral_helper(
            GetMapping("smalldatetime"),
            new DateTime(2015, 3, 12, 13, 36, 37, 371, DateTimeKind.Utc),
            "'2015-03-12T13:36:37'");

        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.1234567Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(0)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(1)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.1Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(2)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.12Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(3)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.123Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(4)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.1234Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(5)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.12345Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(6)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.123456Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(7)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1234567),
            "'2015-03-12T13:36:37.1234567Z'");
        Test_GenerateSqlLiteral_helper(
            GetMapping("datetime2(7)"),
            new DateTime(2015, 3, 12, 13, 36, 37, DateTimeKind.Utc).AddTicks(1200000),
            "'2015-03-12T13:36:37.1200000Z'"); //should this really output trailing zeros?
    }

    public override void Float_literal_generated_correctly()
    {
        var typeMapping = GetMapping(typeof(float));

        Test_GenerateSqlLiteral_helper(typeMapping, float.NaN, "CAST(NaN AS real)");
        Test_GenerateSqlLiteral_helper(typeMapping, float.PositiveInfinity, "CAST(Infinity AS real)");
        Test_GenerateSqlLiteral_helper(typeMapping, float.NegativeInfinity, "CAST(-Infinity AS real)");
        Test_GenerateSqlLiteral_helper(typeMapping, float.MinValue, "CAST(-3.4028235E+38 AS real)");
        Test_GenerateSqlLiteral_helper(typeMapping, float.MaxValue, "CAST(3.4028235E+38 AS real)");
    }

    public override void Long_literal_generated_correctly()
    {
        var typeMapping = GetMapping(typeof(long));

        Test_GenerateSqlLiteral_helper(typeMapping, long.MinValue, "CAST(-9223372036854775808 AS bigint)");
        Test_GenerateSqlLiteral_helper(typeMapping, long.MaxValue, "CAST(9223372036854775807 AS bigint)");
    }

    public override void Short_literal_generated_correctly()
    {
        var typeMapping = GetMapping(typeof(short));

        Test_GenerateSqlLiteral_helper(typeMapping, short.MinValue, "CAST(-32768 AS smallint)");
        Test_GenerateSqlLiteral_helper(typeMapping, short.MaxValue, "CAST(32767 AS smallint)");
    }

    [ConditionalFact]
    public virtual void SqlVariant_literal_generated_correctly()
    {
        var typeMapping = GetMapping("sql_variant");

        Test_GenerateSqlLiteral_helper(typeMapping, 1, "1");
    }

    public override void String_literal_generated_correctly()
    {
        Test_GenerateSqlLiteral_helper(GetMapping("nvarchar(max)"), "Text", "N'Text'");
        Test_GenerateSqlLiteral_helper(GetMapping("varchar(max)"), "Text", "'Text'");
    }

    [ConditionalFact]
    public virtual void String_Utf8()
    {
        var typeMappingSource = GetTypeMappingSource();

        var utf8StringMapping = typeMappingSource.FindMapping(typeof(string), "varchar(max)", unicode: true)!;
        Assert.Same(typeof(string), utf8StringMapping.ClrType);
        Assert.Equal("varchar(max)", utf8StringMapping.StoreType);
        Assert.True(utf8StringMapping.IsUnicode);
        Test_GenerateSqlLiteral_helper(utf8StringMapping, "Text", "'Text'");

        using var command = CreateTestCommand();
        var parameter = utf8StringMapping.CreateParameter(command, "foo", "hello");
        Assert.Equal(DbType.String, parameter.DbType);
    }

    [ConditionalFact]
    public virtual void Char_Utf8()
    {
        var typeMappingSource = GetTypeMappingSource();

        var utf8StringMapping = typeMappingSource.FindMapping(typeof(char), "varchar(max)", unicode: true)!;
        Assert.Same(typeof(char), utf8StringMapping.ClrType);
        Assert.Equal("varchar(max)", utf8StringMapping.StoreType);
        Assert.True(utf8StringMapping.IsUnicode);
        Test_GenerateSqlLiteral_helper(utf8StringMapping, "T", "'T'");

        using var command = CreateTestCommand();
        var parameter = utf8StringMapping.CreateParameter(command, "foo", "h");
        Assert.Equal(DbType.String, parameter.DbType);
    }

    [ConditionalFact]
    public virtual void DateOnly_code_literal_generated_correctly()
    {
        var typeMapping = new DateOnlyTypeMapping("date", DbType.Date);

        Test_GenerateCodeLiteral_helper(typeMapping, new DateOnly(2020, 3, 5), "new DateOnly(2020, 3, 5)");
    }

    [ConditionalFact]
    public virtual void TimeOnly_code_literal_generated_correctly()
    {
        var typeMapping = new TimeOnlyTypeMapping("time", DbType.Time);

        Test_GenerateCodeLiteral_helper(typeMapping, new TimeOnly(12, 30, 10), "new TimeOnly(12, 30, 10)");
        Test_GenerateCodeLiteral_helper(typeMapping, new TimeOnly(12, 30, 10, 500), "new TimeOnly(12, 30, 10, 500)");

        Test_GenerateCodeLiteral_helper(
            typeMapping,
            new TimeOnly(12, 30, 10).Add(TimeSpan.FromTicks(10)),
            "new TimeOnly(12, 30, 10).Add(TimeSpan.FromTicks(10))");
        Test_GenerateCodeLiteral_helper(
            typeMapping,
            new TimeOnly(12, 30, 10, 500).Add(TimeSpan.FromTicks(10)),
            "new TimeOnly(12, 30, 10, 500).Add(TimeSpan.FromTicks(10))");
    }

    public static RelationalTypeMapping GetMapping(string type)
        => GetTypeMappingSource().FindMapping(type);

    public static SqlServerTypeMappingSource GetTypeMappingSource()
        => new(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

    protected virtual void Test_GenerateCodeLiteral_helper(
        RelationalTypeMapping typeMapping,
        object value,
        string expectedCode)
    {
        var typeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var csharpHelper = new CSharpHelper(typeMappingSource);

        Assert.Equal(expectedCode, csharpHelper.UnknownLiteral(value));
    }

    private class FakeType(string fullName) : Type
    {
        public override object[] GetCustomAttributes(bool inherit)
            => throw new NotImplementedException();

        public override bool IsDefined(Type attributeType, bool inherit)
            => throw new NotImplementedException();

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            => throw new NotImplementedException();

        public override Type GetInterface(string name, bool ignoreCase)
            => throw new NotImplementedException();

        public override Type[] GetInterfaces()
            => throw new NotImplementedException();

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
            => throw new NotImplementedException();

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            => throw new NotImplementedException();

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            => throw new NotImplementedException();

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
            => throw new NotImplementedException();

        public override Type GetElementType()
            => throw new NotImplementedException();

        protected override bool HasElementTypeImpl()
            => throw new NotImplementedException();

        protected override PropertyInfo GetPropertyImpl(
            string name,
            BindingFlags bindingAttr,
            Binder binder,
            Type returnType,
            Type[] types,
            ParameterModifier[] modifiers)
            => throw new NotImplementedException();

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            => throw new NotImplementedException();

        protected override MethodInfo GetMethodImpl(
            string name,
            BindingFlags bindingAttr,
            Binder binder,
            CallingConventions callConvention,
            Type[] types,
            ParameterModifier[] modifiers)
            => throw new NotImplementedException();

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            => throw new NotImplementedException();

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
            => throw new NotImplementedException();

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            => throw new NotImplementedException();

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            => throw new NotImplementedException();

        protected override TypeAttributes GetAttributeFlagsImpl()
            => throw new NotImplementedException();

        protected override bool IsArrayImpl()
            => throw new NotImplementedException();

        protected override bool IsByRefImpl()
            => throw new NotImplementedException();

        protected override bool IsPointerImpl()
            => throw new NotImplementedException();

        protected override bool IsPrimitiveImpl()
            => throw new NotImplementedException();

        protected override bool IsCOMObjectImpl()
            => throw new NotImplementedException();

        public override object InvokeMember(
            string name,
            BindingFlags invokeAttr,
            Binder binder,
            object target,
            object[] args,
            ParameterModifier[] modifiers,
            CultureInfo culture,
            string[] namedParameters)
            => throw new NotImplementedException();

        public override Type UnderlyingSystemType { get; }

        protected override ConstructorInfo GetConstructorImpl(
            BindingFlags bindingAttr,
            Binder binder,
            CallingConventions callConvention,
            Type[] types,
            ParameterModifier[] modifiers)
            => throw new NotImplementedException();

        public override string Name
            => throw new NotImplementedException();

        public override Guid GUID
            => throw new NotImplementedException();

        public override Module Module
            => throw new NotImplementedException();

        public override Assembly Assembly
            => throw new NotImplementedException();

        public override string Namespace
            => throw new NotImplementedException();

        public override string AssemblyQualifiedName
            => throw new NotImplementedException();

        public override Type BaseType
            => throw new NotImplementedException();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => throw new NotImplementedException();

        public override string FullName { get; } = fullName;

        public override int GetHashCode()
            => FullName.GetHashCode();

        public override bool Equals(object o)
            => ReferenceEquals(this, o);
    }

    protected override DbContextOptions ContextOptions { get; }
        = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
            .UseSqlServer("Server=Dummy").Options;
}
