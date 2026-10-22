// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

public abstract class RelationalTypeMappingSourceTestBase
{
    protected IMutableEntityType CreateEntityType<TEntity>()
    {
        var builder = CreateModelBuilder();

        builder.Entity<MyType>().Property(e => e.Id).HasColumnType("money");
        builder.Entity<MyRelatedType1>().Property(e => e.Id).HasMaxLength(200).IsFixedLength();
        builder.Entity<MyRelatedType1>().Property(e => e.Relationship2Id).HasColumnType("dec(6,1)");
        builder.Entity<MyRelatedType2>().Property(e => e.Id).HasMaxLength(100).IsFixedLength();
        builder.Entity<MyRelatedType2>().Property(e => e.Relationship2Id).HasMaxLength(787);
        builder.Entity<MyRelatedType3>().Property(e => e.Id).IsUnicode(false);
        builder.Entity<MyRelatedType3>().Property(e => e.Relationship2Id).HasMaxLength(767);
        builder.Entity<MyRelatedType4>().Property(e => e.Relationship2Id).IsUnicode();
        builder.Entity<MyPrecisionType>().Property(e => e.PrecisionOnly).HasPrecision(16);
        builder.Entity<MyPrecisionType>().Property(e => e.PrecisionAndScale).HasPrecision(18, 7);
        builder.Entity<MyTypeWithIndexAttribute>();
        builder.Entity<MyTypeWithIndexAttributeOnCollection>();

        return builder.Model.FindEntityType(typeof(TEntity));
    }

    protected IModel CreateModel()
        => CreateEntityType<MyType>().Model.FinalizeModel();

    protected RelationalTypeMapping GetTypeMapping(
        Type propertyType,
        bool? nullable = null,
        int? maxLength = null,
        int? precision = null,
        int? scale = null,
        Type providerType = null,
        bool? unicode = null,
        bool? fixedLength = null,
        string storeTypeName = null,
        bool useConfiguration = false)
    {
        if (useConfiguration)
        {
            var model = CreateModelBuilder(
                c =>
                {
                    var scalarBuilder = c.DefaultTypeMapping(propertyType);

                    if (maxLength.HasValue)
                    {
                        scalarBuilder.HasMaxLength(maxLength.Value);
                    }

                    if (precision.HasValue)
                    {
                        if (scale.HasValue)
                        {
                            scalarBuilder.HasPrecision(precision.Value, scale.Value);
                        }
                        else
                        {
                            scalarBuilder.HasPrecision(precision.Value);
                        }
                    }

                    if (providerType != null)
                    {
                        scalarBuilder.HasConversion(providerType);
                    }

                    if (unicode.HasValue)
                    {
                        scalarBuilder.IsUnicode(unicode.Value);
                    }

                    if (fixedLength.HasValue)
                    {
                        scalarBuilder.IsFixedLength(fixedLength.Value);
                    }

                    if (storeTypeName != null)
                    {
                        scalarBuilder.HasColumnType(storeTypeName);
                    }
                }).FinalizeModel();

            return CreateRelationalTypeMappingSource(model).GetMapping(propertyType, model);
        }
        else
        {
            var modelBuilder = CreateModelBuilder();
            var entityType = modelBuilder.Entity<MyType>();
            entityType.Property(e => e.Id);
            var property = entityType.Property(propertyType, "MyProp").Metadata;

            if (nullable.HasValue)
            {
                property.IsNullable = nullable.Value;
            }

            if (maxLength.HasValue)
            {
                property.SetMaxLength(maxLength);
            }

            if (precision.HasValue)
            {
                property.SetPrecision(precision);
            }

            if (scale.HasValue)
            {
                property.SetScale(scale);
            }

            if (providerType != null)
            {
                property.SetProviderClrType(providerType);
            }

            if (unicode.HasValue)
            {
                property.SetIsUnicode(unicode);
            }

            if (fixedLength.HasValue)
            {
                property.SetIsFixedLength(fixedLength);
            }

            if (storeTypeName != null)
            {
                property.SetColumnType(storeTypeName);
            }

            var model = modelBuilder.Model.FinalizeModel();
            return CreateRelationalTypeMappingSource(model).GetMapping(model.FindEntityType(typeof(MyType)).FindProperty(property.Name));
        }
    }

    protected abstract ModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configureConventions = null);
    protected abstract IRelationalTypeMappingSource CreateRelationalTypeMappingSource(IModel model);

    protected class MyType
    {
        public decimal Id { get; set; }
    }

    protected class MyPrecisionType
    {
        public decimal Id { get; set; }
        public decimal PrecisionOnly { get; set; }
        public decimal PrecisionAndScale { get; set; }
    }

    protected class MyRelatedType1
    {
        public string Id { get; set; }

        public decimal Relationship1Id { get; set; }
        public MyType Relationship1 { get; set; }

        public decimal Relationship2Id { get; set; }
        public MyType Relationship2 { get; set; }
    }

    protected class MyRelatedType2
    {
        public byte[] Id { get; set; }

        public string Relationship1Id { get; set; }
        public MyRelatedType1 Relationship1 { get; set; }

        public string Relationship2Id { get; set; }
        public MyRelatedType1 Relationship2 { get; set; }
    }

    protected class MyRelatedType3
    {
        public string Id { get; set; }

        public byte[] Relationship1Id { get; set; }
        public MyRelatedType2 Relationship1 { get; set; }

        public byte[] Relationship2Id { get; set; }
        public MyRelatedType2 Relationship2 { get; set; }
    }

    protected class MyRelatedType4
    {
        public string Id { get; set; }

        public string Relationship1Id { get; set; }
        public MyRelatedType3 Relationship1 { get; set; }

        public string Relationship2Id { get; set; }
        public MyRelatedType3 Relationship2 { get; set; }
    }

    [Index(nameof(Name))]
    protected class MyTypeWithIndexAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Index(nameof(Ints))]
    protected class MyTypeWithIndexAttributeOnCollection
    {
        public int Id { get; set; }
        public IEnumerable<int> Ints { get; set; }
    }
}
