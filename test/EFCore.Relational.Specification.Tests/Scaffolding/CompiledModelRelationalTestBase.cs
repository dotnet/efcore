// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Scaffolding;

public abstract class CompiledModelRelationalTestBase : CompiledModelTestBase
{
    [ConditionalFact]
    public virtual void BigModel_with_JSON_columns()
        => Test(
            modelBuilder => BuildBigModel(modelBuilder, jsonColumns: true),
            model => AssertBigModel(model, jsonColumns: true),
            // Blocked by dotnet/runtime/issues/89439
            //c =>
            //{
            //    c.Set<PrincipalDerived<DependentBase<byte?>>>().Add(
            //        new PrincipalDerived<DependentBase<byte?>>
            //        {
            //            Id = 1,
            //            AlternateId = new Guid(),
            //            Dependent = new DependentDerived<byte?>(1, "one"),
            //            Owned = new OwnedType(c)
            //        });

            //    c.SaveChanges();

            //    var dependent = c.Set<PrincipalDerived<DependentBase<byte?>>>().Include(p => p.Dependent).Single().Dependent!;
            //    Assert.Equal("one", ((DependentDerived<byte?>)dependent).GetData());
            //},
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true });

    protected override void BuildBigModel(ModelBuilder modelBuilder, bool jsonColumns)
    {
        base.BuildBigModel(modelBuilder, jsonColumns);

        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                if (!jsonColumns)
                {
                    eb.Property(e => e.Id)
                        .Metadata.SetColumnName("DerivedId", StoreObjectIdentifier.Table("PrincipalDerived"));
                }

                eb.HasKey(e => new { e.Id, e.AlternateId })
                    .HasName("PK");

                eb.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
                        ob.UsePropertyAccessMode(PropertyAccessMode.Field);

                        if (jsonColumns)
                        {
                            ob.ToJson();
                        }
                        else
                        {
                            ob.ToTable(
                                "PrincipalBase", "mySchema",
                                t => t.Property("PrincipalBaseId"));

                            ob.SplitToTable("Details", s => s.Property(e => e.Details));
                        }
                    });

                if (!jsonColumns)
                {
                    eb.ToTable("PrincipalBase", "mySchema");
                }
            });

        modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
            eb =>
            {
                eb.OwnsMany(
                    typeof(OwnedType).FullName!, "ManyOwned", ob =>
                    {
                        if (jsonColumns)
                        {
                            ob.ToJson();
                        }
                        else
                        {
                            ob.ToTable("ManyOwned");
                        }
                    });

                eb.HasMany(e => e.Principals).WithMany(e => (ICollection<PrincipalDerived<DependentBase<byte?>>>)e.Deriveds)
                    .UsingEntity(
                        jb =>
                        {
                            jb.ToTable(
                                tb =>
                                {
                                    tb.HasComment("Join table");
                                    tb.ExcludeFromMigrations();
                                });
                            jb.Property<byte[]>("rowid")
                                .IsRowVersion()
                                .HasComment("RowVersion")
                                .HasColumnOrder(1);
                        });

                if (!jsonColumns)
                {
                    eb.ToTable("PrincipalDerived");
                }
            });

        modelBuilder.Entity<DependentDerived<byte?>>(
            eb =>
            {
                eb.Property<string>("Data")
                    .IsFixedLength();
            });
    }

    protected override void AssertBigModel(IModel model, bool jsonColumns)
    {
        base.AssertBigModel(model, jsonColumns);

        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(model.GetCollation).Message);

        var manyTypesType = model.FindEntityType(typeof(ManyTypes))!;
        Assert.Equal("ManyTypes", manyTypesType.GetTableName());
        Assert.Null(manyTypesType.GetSchema());

        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;
        Assert.Equal("PrincipalBase", principalBase.GetTableName());
        var principalId = principalBase.FindProperty(nameof(PrincipalBase.Id))!;
        var discriminatorProperty = principalBase.FindDiscriminatorProperty();
        if (jsonColumns)
        {
            Assert.Null(principalBase.GetSchema());
            Assert.Equal("Id", principalId.GetColumnName(StoreObjectIdentifier.Table("PrincipalBase")));
            Assert.Null(principalId.GetColumnName(StoreObjectIdentifier.Table("PrincipalDerived")));

            Assert.Equal("Discriminator", discriminatorProperty!.Name);
            Assert.Equal(typeof(string), discriminatorProperty.ClrType);
        }
        else
        {
            Assert.Equal("mySchema", principalBase.GetSchema());
            Assert.Equal("Id", principalId.GetColumnName(StoreObjectIdentifier.Table("PrincipalBase", "mySchema")));
            Assert.Equal("DerivedId", principalId.GetColumnName(StoreObjectIdentifier.Table("PrincipalDerived")));

            Assert.Null(discriminatorProperty);
        }

        Assert.Equal("Id", principalId.GetColumnName());

        var compositeIndex = principalBase.GetIndexes().Single();
        Assert.Equal("IX_PrincipalBase_AlternateId_Id", compositeIndex.GetDatabaseName());

        var principalAlternateKey = principalBase.GetKeys().First();
        Assert.Equal("AK_PrincipalBase_Id", principalAlternateKey.GetName());

        var principalKey = principalBase.GetKeys().Last();
        Assert.Equal(
            new[] { RelationalAnnotationNames.Name },
            principalKey.GetAnnotations().Select(a => a.Name));
        Assert.Equal("PK", principalKey.GetName());

        Assert.Equal(new[] { principalAlternateKey, principalKey }, principalId.GetContainingKeys());

        Assert.Equal("PrincipalBase", principalBase.GetTableName());
        if (jsonColumns)
        {
            Assert.Null(principalBase.GetSchema());
        }
        else
        {
            Assert.Equal("mySchema", principalBase.GetSchema());
        }

        var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>))!;
        var dependentNavigation = principalDerived.GetDeclaredNavigations().First();
        var dependentForeignKey = dependentNavigation.ForeignKey;

        var referenceOwnedNavigation = principalBase.GetNavigations().Single();
        var referenceOwnedType = referenceOwnedNavigation.TargetEntityType;
        var principalTable = StoreObjectIdentifier.Create(referenceOwnedType, StoreObjectType.Table)!.Value;

        var ownedId = referenceOwnedType.FindProperty("PrincipalBaseId")!;
        Assert.True(ownedId.IsPrimaryKey());

        var detailsProperty = referenceOwnedType.FindProperty(nameof(OwnedType.Details))!;
        Assert.Null(detailsProperty[RelationalAnnotationNames.Collation]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(detailsProperty.GetCollation).Message);

        if (jsonColumns)
        {
            Assert.Empty(referenceOwnedType.GetMappingFragments());
        }
        else
        {
            var ownedFragment = referenceOwnedType.GetMappingFragments().Single();
            Assert.Equal(nameof(OwnedType.Details), detailsProperty.GetColumnName(ownedFragment.StoreObject));
        }

        Assert.Null(detailsProperty.GetColumnName(principalTable));

        var referenceOwnership = referenceOwnedNavigation.ForeignKey;
        var ownedCollectionNavigation = principalDerived.GetDeclaredNavigations().Last();
        var collectionOwnership = ownedCollectionNavigation.ForeignKey;

        var tptForeignKey = principalDerived.GetForeignKeys().SingleOrDefault();
        if (jsonColumns)
        {
            Assert.Null(tptForeignKey);
        }
        else
        {
            Assert.False(tptForeignKey!.IsOwnership);
            Assert.True(tptForeignKey.IsRequired);
            Assert.False(tptForeignKey.IsRequiredDependent);
            Assert.True(tptForeignKey.IsUnique);
            Assert.Null(tptForeignKey.DependentToPrincipal);
            Assert.Null(tptForeignKey.PrincipalToDependent);
            Assert.Equal(DeleteBehavior.Cascade, tptForeignKey.DeleteBehavior);
            Assert.Equal(principalKey.Properties, tptForeignKey.Properties);
            Assert.Same(principalKey, tptForeignKey.PrincipalKey);
        }

        var derivedSkipNavigation = principalDerived.GetDeclaredSkipNavigations().Single();
        Assert.Same(derivedSkipNavigation, derivedSkipNavigation.ForeignKey.GetReferencingSkipNavigations().Single());
        Assert.Same(
            derivedSkipNavigation.Inverse, derivedSkipNavigation.Inverse.ForeignKey.GetReferencingSkipNavigations().Single());

        Assert.Equal(new[] { derivedSkipNavigation.Inverse, derivedSkipNavigation }, principalDerived.GetSkipNavigations());

        var joinType = derivedSkipNavigation.JoinEntityType;
        Assert.Null(joinType[RelationalAnnotationNames.Comment]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(joinType.GetComment).Message);
        Assert.Null(joinType.GetQueryFilter());
        Assert.Null(joinType[RelationalAnnotationNames.IsTableExcludedFromMigrations]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => joinType.IsTableExcludedFromMigrations()).Message);

        var rowid = joinType.GetProperties().Single(p => !p.IsForeignKey());
        Assert.Equal("rowid", rowid.GetColumnName());
        Assert.Null(rowid[RelationalAnnotationNames.Comment]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(rowid.GetComment).Message);
        Assert.Null(rowid[RelationalAnnotationNames.ColumnOrder]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => rowid.GetColumnOrder()).Message);

        var dependentBase = dependentNavigation.TargetEntityType;
        var dependentDerived = dependentBase.GetDerivedTypes().Single();
        var dependentData = dependentDerived.GetDeclaredProperties().First();
        Assert.Equal("Data", dependentData.GetColumnName());
        Assert.True(dependentData.IsFixedLength());

        var dependentBaseForeignKey = dependentBase.GetForeignKeys().Single(fk => fk != dependentForeignKey);

        var joinTable = joinType.GetTableMappings().Single().Table;
        Assert.Null(joinTable[RelationalAnnotationNames.Comment]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => joinTable.Comment).Message);

        var dependentMoney = dependentDerived.GetDeclaredProperties().Last();
        Assert.Equal("Money", dependentMoney.GetColumnName());
        Assert.Null(dependentMoney.IsFixedLength());

        if (jsonColumns)
        {
            Assert.Equal(
                new[]
                {
                    derivedSkipNavigation.ForeignKey,
                    referenceOwnership,
                    collectionOwnership,
                    dependentForeignKey,
                    derivedSkipNavigation.Inverse.ForeignKey
                },
                principalKey.GetReferencingForeignKeys());

            Assert.Equal(
                new[] { dependentBaseForeignKey, referenceOwnership, derivedSkipNavigation.Inverse.ForeignKey },
                principalBase.GetReferencingForeignKeys());
        }
        else
        {
            Assert.Equal(
                new[]
                {
                    derivedSkipNavigation.ForeignKey,
                    tptForeignKey,
                    referenceOwnership,
                    collectionOwnership,
                    dependentForeignKey,
                    derivedSkipNavigation.Inverse.ForeignKey
                },
                principalKey.GetReferencingForeignKeys());

            Assert.Equal(
                new[] { dependentBaseForeignKey, tptForeignKey, referenceOwnership, derivedSkipNavigation.Inverse.ForeignKey },
                principalBase.GetReferencingForeignKeys());
        }
    }

    protected override void BuildComplexTypesModel(ModelBuilder modelBuilder)
    {
        base.BuildComplexTypesModel(modelBuilder);

        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.ComplexProperty(
                    e => e.Owned, eb =>
                    {
                        eb.Property(c => c.Details)
                            .HasColumnName("Deets")
                            .HasColumnOrder(1)
                            .HasColumnType("varchar")
                            .HasComment("Dt");
                    });

                eb.ToTable("PrincipalBase");
                eb.ToView("PrincipalBaseView");
                eb.ToSqlQuery("select * from PrincipalBase");
                eb.ToFunction("PrincipalBaseTvf");

                eb.InsertUsingStoredProcedure(
                    s => s
                        .HasParameter("PrincipalBaseId")
                        .HasParameter("Enum1")
                        .HasParameter("Enum2")
                        .HasParameter("FlagsEnum1")
                        .HasParameter("FlagsEnum2")
                        .HasParameter("ValueTypeList")
                        .HasParameter("ValueTypeIList")
                        .HasParameter("ValueTypeArray")
                        .HasParameter("ValueTypeEnumerable")
                        .HasParameter("RefTypeList")
                        .HasParameter("RefTypeIList")
                        .HasParameter("RefTypeArray")
                        .HasParameter("RefTypeEnumerable")
                        .HasParameter("Discriminator")
                        .HasParameter(p => p.Id, p => p.IsOutput()));
                eb.UpdateUsingStoredProcedure(
                    s => s
                        .HasParameter("PrincipalBaseId")
                        .HasParameter("Enum1")
                        .HasParameter("Enum2")
                        .HasParameter("FlagsEnum1")
                        .HasParameter("FlagsEnum2")
                        .HasParameter("ValueTypeList")
                        .HasParameter("ValueTypeIList")
                        .HasParameter("ValueTypeArray")
                        .HasParameter("ValueTypeEnumerable")
                        .HasParameter("RefTypeList")
                        .HasParameter("RefTypeIList")
                        .HasParameter("RefTypeArray")
                        .HasParameter("RefTypeEnumerable")
                        .HasOriginalValueParameter(p => p.Id));
                eb.DeleteUsingStoredProcedure(
                    s => s.HasRowsAffectedParameter()
                        .HasOriginalValueParameter(p => p.Id));
            });

        modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
            eb =>
            {
                //eb.ComplexCollection(typeof(OwnedType).Name, "ManyOwned");
                eb.ToTable("PrincipalBase");
                eb.ToFunction((string?)null);
            });
    }

    protected override void AssertComplexTypes(IModel model)
    {
        base.AssertComplexTypes(model);

        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;
        var complexProperty = principalBase.GetComplexProperties().Single();
        var complexType = complexProperty.ComplexType;
        Assert.Equal(
            new[]
            {
                RelationalAnnotationNames.FunctionName,
                RelationalAnnotationNames.Schema,
                RelationalAnnotationNames.SqlQuery,
                RelationalAnnotationNames.TableName,
                RelationalAnnotationNames.ViewName,
                RelationalAnnotationNames.ViewSchema,
                "go"
            },
            complexType.GetAnnotations().Select(a => a.Name));

        var detailsProperty = complexType.FindProperty(nameof(OwnedType.Details))!;
        Assert.Equal("Deets", detailsProperty.GetColumnName());
        Assert.Equal("varchar(64)", detailsProperty.GetColumnType());
        Assert.Null(detailsProperty.IsFixedLength());
        Assert.Null(detailsProperty.GetDefaultValueSql());
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(detailsProperty.GetCollation).Message);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(detailsProperty.GetComment).Message);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => detailsProperty.GetColumnOrder()).Message);

        var principalTable = StoreObjectIdentifier.Create(complexType, StoreObjectType.Table)!.Value;

        Assert.Equal("Deets", detailsProperty.GetColumnName(principalTable));

        var dbFunction = model.FindDbFunction("PrincipalBaseTvf")!;
        Assert.False(dbFunction.IsNullable);
        Assert.False(dbFunction.IsScalar);
        Assert.False(dbFunction.IsBuiltIn);
        Assert.False(dbFunction.IsAggregate);
        Assert.Null(dbFunction.Translation);
        Assert.Null(dbFunction.TypeMapping);
        Assert.Equal(typeof(IQueryable<PrincipalBase>), dbFunction.ReturnType);
        Assert.Null(dbFunction.MethodInfo);
        Assert.Empty(dbFunction.GetAnnotations());
        Assert.Empty(dbFunction.GetRuntimeAnnotations());
        Assert.Equal("PrincipalBaseTvf", dbFunction.StoreFunction.Name);
        Assert.False(dbFunction.StoreFunction.IsShared);
        Assert.NotNull(dbFunction.ToString());
        Assert.Empty(dbFunction.Parameters);

        var principalBaseFunctionMapping = principalBase.GetFunctionMappings().Single(m => m.IsDefaultFunctionMapping);
        Assert.True(principalBaseFunctionMapping.IncludesDerivedTypes);
        Assert.Null(principalBaseFunctionMapping.IsSharedTablePrincipal);
        Assert.Null(principalBaseFunctionMapping.IsSplitEntityTypePrincipal);
        Assert.Same(dbFunction, principalBaseFunctionMapping.DbFunction);
    }

    [ConditionalFact]
    public virtual void Tpc_Sprocs()
        => Test(
            BuildTpcSprocsModel,
            AssertTpcSprocs,
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true });

    protected virtual void BuildTpcSprocsModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("TPC");

        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.Ignore(e => e.Owned);

                eb.UseTpcMappingStrategy();

                eb.ToTable("PrincipalBase");
                eb.ToView("PrincipalBaseView", tb => tb.Property(e => e.Id).HasAnnotation("foo", "bar2"));

                eb.Property(p => p.Id).ValueGeneratedNever();
                eb.Property(p => p.Enum1).HasDefaultValue(AnEnum.A).HasSentinel(AnEnum.A);
                eb.InsertUsingStoredProcedure(
                    s => s
                        .HasParameter(p => p.Id)
                        .HasParameter("PrincipalBaseId")
                        .HasParameter("PrincipalDerivedId")
                        .HasParameter("Enum2")
                        .HasParameter("FlagsEnum1")
                        .HasParameter("FlagsEnum2")
                        .HasParameter("ValueTypeList")
                        .HasParameter("ValueTypeIList")
                        .HasParameter("ValueTypeArray")
                        .HasParameter("ValueTypeEnumerable")
                        .HasParameter("RefTypeList")
                        .HasParameter("RefTypeIList")
                        .HasParameter("RefTypeArray")
                        .HasParameter("RefTypeEnumerable")
                        .HasParameter(p => p.Enum1, pb => pb.HasName("BaseEnum").IsOutput().HasAnnotation("foo", "bar"))
                        .HasAnnotation("foo", "bar1"));
                eb.UpdateUsingStoredProcedure(
                    s => s
                        .HasParameter("PrincipalBaseId")
                        .HasParameter("PrincipalDerivedId")
                        .HasParameter("Enum1")
                        .HasParameter("Enum2")
                        .HasParameter("FlagsEnum1")
                        .HasParameter("FlagsEnum2")
                        .HasParameter("ValueTypeList")
                        .HasParameter("ValueTypeIList")
                        .HasParameter("ValueTypeArray")
                        .HasParameter("ValueTypeEnumerable")
                        .HasParameter("RefTypeList")
                        .HasParameter("RefTypeIList")
                        .HasParameter("RefTypeArray")
                        .HasParameter("RefTypeEnumerable")
                        .HasOriginalValueParameter(p => p.Id));
                eb.DeleteUsingStoredProcedure(
                    s =>
                    {
                        s.HasOriginalValueParameter(p => p.Id);
                        if (UseSprocReturnValue)
                        {
                            s.HasRowsAffectedReturnValue();
                        }
                        else
                        {
                            s.HasRowsAffectedParameter(p => p.HasName("RowsAffected"));
                        }
                    });

                eb.HasIndex(["PrincipalBaseId"], "PrincipalIndex")
                    .IsUnique()
                    .HasDatabaseName("PIX")
                    .HasFilter("AlternateId <> NULL");
            });

        modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
            eb =>
            {
                eb.HasOne(e => e.Dependent).WithOne(e => e.Principal)
                    .HasForeignKey<DependentBase<byte?>>()
                    .OnDelete(DeleteBehavior.ClientCascade);

                eb.Navigation(e => e.Dependent).IsRequired();

                eb.ToTable("PrincipalDerived");
                eb.ToView("PrincipalDerivedView");

                eb.Property(p => p.Id).ValueGeneratedNever();
                eb.Property(p => p.Enum1).HasDefaultValue(AnEnum.A).HasSentinel(AnEnum.A);
                eb.InsertUsingStoredProcedure(
                    "Derived_Insert", s => s
                        .HasParameter(p => p.Id)
                        .HasParameter("PrincipalBaseId")
                        .HasParameter("PrincipalDerivedId")
                        .HasParameter("Enum2")
                        .HasParameter("FlagsEnum1")
                        .HasParameter("FlagsEnum2")
                        .HasParameter("ValueTypeList")
                        .HasParameter("ValueTypeIList")
                        .HasParameter("ValueTypeArray")
                        .HasParameter("ValueTypeEnumerable")
                        .HasParameter("RefTypeList")
                        .HasParameter("RefTypeIList")
                        .HasParameter("RefTypeArray")
                        .HasParameter("RefTypeEnumerable")
                        .HasResultColumn(p => p.Enum1, pb => pb.HasName("DerivedEnum").HasAnnotation("foo", "bar3")));
                eb.UpdateUsingStoredProcedure(
                    "Derived_Update", "Derived", s => s
                        .HasParameter("PrincipalBaseId")
                        .HasParameter("PrincipalDerivedId")
                        .HasParameter("Enum1")
                        .HasParameter("Enum2")
                        .HasParameter("FlagsEnum1")
                        .HasParameter("FlagsEnum2")
                        .HasParameter("ValueTypeList")
                        .HasParameter("ValueTypeIList")
                        .HasParameter("ValueTypeArray")
                        .HasParameter("ValueTypeEnumerable")
                        .HasParameter("RefTypeList")
                        .HasParameter("RefTypeIList")
                        .HasParameter("RefTypeArray")
                        .HasParameter("RefTypeEnumerable")
                        .HasOriginalValueParameter(p => p.Id));
                eb.DeleteUsingStoredProcedure(
                    "Derived_Delete", s => s
                        .HasOriginalValueParameter(p => p.Id));
            });

        modelBuilder.Entity<DependentBase<byte?>>(
            eb =>
            {
                eb.Property<byte?>("Id");
            });
    }

    protected virtual bool UseSprocReturnValue
        => false;

    protected virtual void AssertTpcSprocs(IModel model)
    {
        Assert.Equal("TPC", model.GetDefaultSchema());

        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;
        var id = principalBase.FindProperty(nameof(PrincipalBase.Id))!;

        Assert.Equal("Id", id.GetColumnName());
        Assert.Equal("PrincipalBase", principalBase.GetTableName());
        Assert.Equal("TPC", principalBase.GetSchema());
        Assert.Equal("Id", id.GetColumnName(StoreObjectIdentifier.Create(principalBase, StoreObjectType.Table)!.Value));
        Assert.Null(id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.Table)!.Value));

        Assert.Equal("PrincipalBaseView", principalBase.GetViewName());
        Assert.Equal("TPC", principalBase.GetViewSchema());
        Assert.Equal("Id", id.GetColumnName(StoreObjectIdentifier.Create(principalBase, StoreObjectType.View)!.Value));
        Assert.Equal(
            "bar2",
            id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.View)!.Value)!["foo"]);

        var enum1 = principalBase.FindProperty(nameof(PrincipalBase.Enum1))!;
        Assert.Equal(AnEnum.A, enum1.GetDefaultValue());

        var principalBaseId = principalBase.FindProperty("PrincipalBaseId")!;

        var alternateIndex = principalBase.GetIndexes().Last();
        Assert.Same(principalBaseId, alternateIndex.Properties.Single());
        Assert.True(alternateIndex.IsUnique);
        Assert.Equal("PrincipalIndex", alternateIndex.Name);
        Assert.Equal("PIX", alternateIndex.GetDatabaseName());
        Assert.Equal("AlternateId <> NULL", alternateIndex.GetFilter());

        Assert.Equal(new[] { alternateIndex }, principalBaseId.GetContainingIndexes());

        var insertSproc = principalBase.GetInsertStoredProcedure()!;
        Assert.Equal("PrincipalBase_Insert", insertSproc.Name);
        Assert.Equal("TPC", insertSproc.Schema);
        Assert.Equal(
            new[]
            {
                "Id",
                "PrincipalBaseId",
                "PrincipalDerivedId",
                "Enum2",
                "FlagsEnum1",
                "FlagsEnum2",
                "ValueTypeList",
                "ValueTypeIList",
                "ValueTypeArray",
                "ValueTypeEnumerable",
                "RefTypeList",
                "RefTypeIList",
                "RefTypeArray",
                "RefTypeEnumerable",
                "Enum1"
            },
            insertSproc.Parameters.Select(p => p.PropertyName));
        Assert.Empty(insertSproc.ResultColumns);
        Assert.False(insertSproc.IsRowsAffectedReturned);
        Assert.Equal("bar1", insertSproc["foo"]);
        Assert.Same(principalBase, insertSproc.EntityType);
        Assert.Equal("BaseEnum", insertSproc.Parameters.Last().Name);
        Assert.Equal("bar", insertSproc.Parameters.Last()["foo"]);
        Assert.Null(enum1.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.InsertStoredProcedure)!.Value));
        Assert.Equal(
            "Enum1",
            enum1.GetColumnName(StoreObjectIdentifier.Create(principalBase, StoreObjectType.InsertStoredProcedure)!.Value));

        var updateSproc = principalBase.GetUpdateStoredProcedure()!;
        Assert.Equal("PrincipalBase_Update", updateSproc.Name);
        Assert.Equal("TPC", updateSproc.Schema);
        Assert.Equal(
            new[]
            {
                "PrincipalBaseId",
                "PrincipalDerivedId",
                "Enum1",
                "Enum2",
                "FlagsEnum1",
                "FlagsEnum2",
                "ValueTypeList",
                "ValueTypeIList",
                "ValueTypeArray",
                "ValueTypeEnumerable",
                "RefTypeList",
                "RefTypeIList",
                "RefTypeArray",
                "RefTypeEnumerable",
                "Id"
            },
            updateSproc.Parameters.Select(p => p.PropertyName));
        Assert.Empty(updateSproc.ResultColumns);
        Assert.False(updateSproc.IsRowsAffectedReturned);
        Assert.Empty(updateSproc.GetAnnotations());
        Assert.Same(principalBase, updateSproc.EntityType);
        Assert.Equal("Id_Original", updateSproc.Parameters.Last().Name);
        Assert.Null(enum1.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.UpdateStoredProcedure)!.Value));

        var deleteSproc = principalBase.GetDeleteStoredProcedure()!;
        Assert.Equal("PrincipalBase_Delete", deleteSproc.Name);
        Assert.Equal("TPC", deleteSproc.Schema);
        if (UseSprocReturnValue)
        {
            Assert.Equal(["Id_Original"], deleteSproc.Parameters.Select(p => p.Name));
        }
        else
        {
            Assert.Equal(["Id_Original", "RowsAffected"], deleteSproc.Parameters.Select(p => p.Name));
        }

        Assert.Empty(deleteSproc.ResultColumns);
        Assert.Equal(UseSprocReturnValue, deleteSproc.IsRowsAffectedReturned);
        Assert.Same(principalBase, deleteSproc.EntityType);
        Assert.Null(id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.DeleteStoredProcedure)!.Value));

        Assert.Equal("PrincipalBase", principalBase.GetDiscriminatorValue());
        Assert.Null(principalBase.FindDiscriminatorProperty());
        Assert.Equal("TPC", principalBase.GetMappingStrategy());

        var selfRefNavigation = principalBase.GetDeclaredNavigations().Last();
        Assert.Equal("Deriveds", selfRefNavigation.Name);
        Assert.True(selfRefNavigation.IsCollection);
        Assert.False(selfRefNavigation.IsOnDependent);
        Assert.Equal(principalBase, selfRefNavigation.TargetEntityType);
        Assert.Null(selfRefNavigation.Inverse);

        var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>))!;
        Assert.Equal(principalBase, principalDerived.BaseType);

        Assert.Equal("PrincipalDerived", principalDerived.GetTableName());
        Assert.Equal("TPC", principalDerived.GetSchema());
        Assert.Equal("PrincipalDerivedView", principalDerived.GetViewName());
        Assert.Equal("TPC", principalBase.GetViewSchema());

        insertSproc = principalDerived.GetInsertStoredProcedure()!;
        Assert.Equal("Derived_Insert", insertSproc.Name);
        Assert.Equal("TPC", insertSproc.Schema);
        Assert.Equal(
            new[]
            {
                "Id",
                "PrincipalBaseId",
                "PrincipalDerivedId",
                "Enum2",
                "FlagsEnum1",
                "FlagsEnum2",
                "ValueTypeList",
                "ValueTypeIList",
                "ValueTypeArray",
                "ValueTypeEnumerable",
                "RefTypeList",
                "RefTypeIList",
                "RefTypeArray",
                "RefTypeEnumerable"
            },
            insertSproc.Parameters.Select(p => p.PropertyName));
        Assert.Equal(new[] { "Enum1" }, insertSproc.ResultColumns.Select(p => p.PropertyName));
        Assert.Null(insertSproc["foo"]);
        Assert.Same(principalDerived, insertSproc.EntityType);
        Assert.Equal("DerivedEnum", insertSproc.ResultColumns.Last().Name);
        Assert.Equal(
            "DerivedEnum",
            enum1.GetColumnName(StoreObjectIdentifier.Create(principalDerived, StoreObjectType.InsertStoredProcedure)!.Value));
        Assert.Equal("bar3", insertSproc.ResultColumns.Last()["foo"]);
        Assert.Null(
            enum1.FindOverrides(
                StoreObjectIdentifier.Create(principalDerived, StoreObjectType.InsertStoredProcedure)!.Value)!["foo"]);

        updateSproc = principalDerived.GetUpdateStoredProcedure()!;
        Assert.Equal("Derived_Update", updateSproc.Name);
        Assert.Equal("Derived", updateSproc.Schema);
        Assert.Equal(
            new[]
            {
                "PrincipalBaseId",
                "PrincipalDerivedId",
                "Enum1",
                "Enum2",
                "FlagsEnum1",
                "FlagsEnum2",
                "ValueTypeList",
                "ValueTypeIList",
                "ValueTypeArray",
                "ValueTypeEnumerable",
                "RefTypeList",
                "RefTypeIList",
                "RefTypeArray",
                "RefTypeEnumerable",
                "Id"
            },
            updateSproc.Parameters.Select(p => p.PropertyName));
        Assert.Empty(updateSproc.ResultColumns);
        Assert.Empty(updateSproc.GetAnnotations());
        Assert.Same(principalDerived, updateSproc.EntityType);
        Assert.Equal("Id_Original", updateSproc.Parameters.Last().Name);
        Assert.Null(
            id.FindOverrides(StoreObjectIdentifier.Create(principalDerived, StoreObjectType.UpdateStoredProcedure)!.Value));

        deleteSproc = principalDerived.GetDeleteStoredProcedure()!;
        Assert.Equal("Derived_Delete", deleteSproc.Name);
        Assert.Equal("TPC", deleteSproc.Schema);
        Assert.Equal(new[] { "Id" }, deleteSproc.Parameters.Select(p => p.PropertyName));
        Assert.Empty(deleteSproc.ResultColumns);
        Assert.Same(principalDerived, deleteSproc.EntityType);
        Assert.Equal("Id_Original", deleteSproc.Parameters.Last().Name);
        Assert.Null(
            id.FindOverrides(StoreObjectIdentifier.Create(principalDerived, StoreObjectType.DeleteStoredProcedure)!.Value));

        Assert.Equal("PrincipalDerived<DependentBase<byte?>>", principalDerived.GetDiscriminatorValue());
        Assert.Null(principalDerived.FindDiscriminatorProperty());
        Assert.Equal("TPC", principalDerived.GetMappingStrategy());

        Assert.Equal(2, principalDerived.GetDeclaredNavigations().Count());
        var derivedNavigation = principalDerived.GetDeclaredNavigations().Last();
        Assert.Equal("Principals", derivedNavigation.Name);
        Assert.True(derivedNavigation.IsCollection);
        Assert.False(derivedNavigation.IsOnDependent);
        Assert.Equal(principalBase, derivedNavigation.TargetEntityType);
        Assert.Null(derivedNavigation.Inverse);

        var dependentNavigation = principalDerived.GetDeclaredNavigations().First();
        Assert.Equal("Dependent", dependentNavigation.Name);
        Assert.Equal("Dependent", dependentNavigation.PropertyInfo!.Name);
        Assert.Equal("<Dependent>k__BackingField", dependentNavigation.FieldInfo!.Name);
        Assert.False(dependentNavigation.IsCollection);
        Assert.False(dependentNavigation.IsEagerLoaded);
        Assert.True(dependentNavigation.LazyLoadingEnabled);
        Assert.False(dependentNavigation.IsOnDependent);
        Assert.Equal(principalDerived, dependentNavigation.DeclaringEntityType);
        Assert.Equal("Principal", dependentNavigation.Inverse!.Name);

        var dependentForeignKey = dependentNavigation.ForeignKey;
        Assert.False(dependentForeignKey.IsOwnership);
        Assert.False(dependentForeignKey.IsRequired);
        Assert.True(dependentForeignKey.IsRequiredDependent);
        Assert.True(dependentForeignKey.IsUnique);
        Assert.Same(principalDerived, dependentForeignKey.PrincipalEntityType);
        Assert.Same(dependentNavigation.Inverse, dependentForeignKey.DependentToPrincipal);
        Assert.Same(dependentNavigation, dependentForeignKey.PrincipalToDependent);
        Assert.Equal(DeleteBehavior.ClientCascade, dependentForeignKey.DeleteBehavior);
        Assert.Equal(new[] { "PrincipalId" }, dependentForeignKey.Properties.Select(p => p.Name));

        var dependentBase = dependentNavigation.TargetEntityType;

        Assert.True(dependentBase.GetIsDiscriminatorMappingComplete());
        Assert.Null(dependentBase.FindDiscriminatorProperty());

        Assert.Same(dependentForeignKey, dependentBase.GetForeignKeys().Single());

        Assert.Equal(
            new[] { dependentBase, principalBase, principalDerived },
            model.GetEntityTypes());
    }

    [ConditionalFact]
    public virtual void Sequences()
        => Test(
            modelBuilder =>
                modelBuilder.HasSequence<long>("Long")
                    .HasMin(-2)
                    .HasMax(2)
                    .IsCyclic()
                    .IncrementsBy(2)
                    .StartsAt(-4),
            model =>
            {
                Assert.Single(model.GetSequences());

                var longSequence = model.FindSequence("Long")!;
                Assert.Same(model, longSequence.Model);
                Assert.Equal(typeof(long), longSequence.Type);
                Assert.True(longSequence.IsCyclic);
                Assert.Equal(-4, longSequence.StartValue);
                Assert.Equal(-2, longSequence.MinValue);
                Assert.Equal(2, longSequence.MaxValue);
                Assert.Equal(2, longSequence.IncrementBy);
                Assert.NotNull(longSequence.ToString());
            });

    [ConditionalFact]
    public virtual void CheckConstraints()
        => Test(
            modelBuilder => modelBuilder.Entity<Data>(
                eb =>
                {
                    eb.Property<int>("Id");
                    eb.HasKey("Id");

                    eb.ToTable(tb => tb.HasCheckConstraint("idConstraint", "Id <> 0"));
                    eb.ToTable(tb => tb.HasCheckConstraint("anotherConstraint", "Id <> -1"));
                }),
            model =>
            {
                var dataEntity = model.GetEntityTypes().Single();

                Assert.Equal(
                    CoreStrings.RuntimeModelMissingData,
                    Assert.Throws<InvalidOperationException>(() => dataEntity.GetCheckConstraints()).Message);
            });

    [ConditionalFact]
    public virtual void Triggers()
        => Test(
            modelBuilder => modelBuilder.Entity<Data>(
                eb =>
                {
                    eb.Property<int>("Id");
                    eb.HasKey("Id");

                    eb.ToTable(
                        tb =>
                        {
                            tb.HasTrigger("Trigger1");
                            tb.HasTrigger("Trigger2");
                        });
                }),
            model =>
            {
                var dataEntity = model.GetEntityTypes().Single();

                Assert.Equal(2, dataEntity.GetDeclaredTriggers().Count());
            });

    [ConditionalFact]
    public virtual Task DbFunctions()
        => Test<DbFunctionContext>(
            assertModel: model =>
            {
                Assert.Equal(5, model.GetDbFunctions().Count());

                var getCount = model.FindDbFunction(
                    typeof(DbFunctionContext)
                        .GetMethod("GetCount", BindingFlags.NonPublic | BindingFlags.Instance)!)!;
                Assert.Equal("CustomerOrderCount", getCount.Name);
                Assert.Same(model, getCount.Model);
                Assert.Same(model, ((IReadOnlyDbFunction)getCount).Model);
                Assert.Equal(typeof(DbFunctionContext).FullName + ".GetCount(System.Guid?,string)", getCount.ModelName);
                Assert.Equal("dbf", getCount.Schema);
                Assert.False(getCount.IsNullable);
                Assert.True(getCount.IsScalar);
                Assert.False(getCount.IsBuiltIn);
                Assert.False(getCount.IsAggregate);
                Assert.Null(getCount.Translation);
                Assert.NotNull(getCount.TypeMapping?.StoreType);
                Assert.Equal(typeof(int), getCount.ReturnType);
                Assert.Equal("GetCount", getCount.MethodInfo!.Name);
                Assert.Empty(getCount.GetAnnotations());
                Assert.Empty(getCount.GetRuntimeAnnotations());
                Assert.Equal("CustomerOrderCount", getCount.StoreFunction.Name);
                Assert.False(getCount.StoreFunction.IsShared);
                Assert.NotNull(getCount.ToString());
                Assert.Equal(getCount.Parameters, ((IReadOnlyDbFunction)getCount).Parameters);
                Assert.Equal(2, getCount.Parameters.Count);

                var getCountParameter1 = getCount.Parameters[0];
                Assert.Same(getCount, getCountParameter1.Function);
                Assert.Same(getCount, ((IReadOnlyDbFunctionParameter)getCountParameter1).Function);
                Assert.Equal("id", getCountParameter1.Name);
                Assert.NotNull(getCountParameter1.StoreType);
                Assert.Equal(getCountParameter1.StoreType, ((IReadOnlyDbFunctionParameter)getCountParameter1).StoreType);
                Assert.True(getCountParameter1.PropagatesNullability);
                Assert.Equal(typeof(Guid?), getCountParameter1.ClrType);
                Assert.Equal(getCountParameter1.StoreType, getCountParameter1.TypeMapping!.StoreType);
                Assert.Single(getCountParameter1.GetAnnotations());
                Assert.Equal(new[] { 1L }, getCountParameter1["MyAnnotation"]);
                Assert.Equal("id", getCountParameter1.StoreFunctionParameter.Name);
                Assert.Equal(getCountParameter1.StoreType, getCountParameter1.StoreFunctionParameter.StoreType);
                Assert.NotNull(getCountParameter1.ToString());

                var getCountParameter2 = getCount.Parameters[1];
                Assert.Same(getCount, getCountParameter2.Function);
                Assert.Equal("condition", getCountParameter2.Name);
                Assert.NotNull(getCountParameter2.StoreType);
                Assert.False(getCountParameter2.PropagatesNullability);
                Assert.Equal(typeof(string), getCountParameter2.ClrType);
                Assert.Equal(getCountParameter2.StoreType, getCountParameter2.TypeMapping!.StoreType);
                Assert.Equal("condition", getCountParameter2.StoreFunctionParameter.Name);
                Assert.Equal(getCountParameter2.StoreType, getCountParameter2.StoreFunctionParameter.StoreType);
                Assert.NotNull(getCountParameter2.ToString());

                var isDate = model.FindDbFunction(typeof(DbFunctionContext).GetMethod("IsDateStatic")!)!;
                Assert.Equal("IsDate", isDate.Name);
                Assert.Null(isDate.Schema);
                Assert.Equal(typeof(DbFunctionContext).FullName + ".IsDateStatic(string)", isDate.ModelName);
                Assert.True(isDate.IsNullable);
                Assert.True(isDate.IsScalar);
                Assert.True(isDate.IsBuiltIn);
                Assert.False(isDate.IsAggregate);
                Assert.Null(isDate.Translation);
                Assert.Equal(typeof(bool), isDate.ReturnType);
                Assert.Equal("IsDateStatic", isDate.MethodInfo!.Name);
                Assert.Single(isDate.GetAnnotations());
                Assert.Equal(new Guid(), isDate["MyGuid"]);
                Assert.Empty(isDate.GetRuntimeAnnotations());
                Assert.NotNull(isDate.StoreFunction.ReturnType);
                Assert.Empty(isDate.StoreFunction.EntityTypeMappings);
                Assert.Single(isDate.Parameters);

                var isDateParameter = isDate.Parameters[0];
                Assert.Same(isDate, isDateParameter.Function);
                Assert.Equal("date", isDateParameter.Name);
                Assert.NotNull(isDateParameter.StoreType);
                Assert.False(isDateParameter.PropagatesNullability);
                Assert.Equal(typeof(string), isDateParameter.ClrType);
                Assert.Equal(isDateParameter.StoreType, isDateParameter.TypeMapping!.StoreType);
                Assert.Equal("date", isDateParameter.StoreFunctionParameter.Name);
                Assert.Equal(isDateParameter.StoreType, isDateParameter.StoreFunctionParameter.StoreType);

                var getData = model.FindDbFunction(
                    typeof(DbFunctionContext)
                        .GetMethod("GetData", [typeof(int)])!)!;
                Assert.Equal("GetData", getData.Name);
                //Assert.Equal("dbo", getData.Schema);
                Assert.Equal(typeof(DbFunctionContext).FullName + ".GetData(int)", getData.ModelName);
                Assert.False(getData.IsNullable);
                Assert.False(getData.IsScalar);
                Assert.False(getData.IsBuiltIn);
                Assert.False(getData.IsAggregate);
                Assert.Null(getData.Translation);
                Assert.Equal(typeof(IQueryable<Data>), getData.ReturnType);
                Assert.Equal("GetData", getData.MethodInfo!.Name);
                Assert.Empty(getData.GetAnnotations());
                Assert.Empty(getData.GetRuntimeAnnotations());
                Assert.Null(getData.TypeMapping?.StoreType);
                Assert.Null(getData.StoreFunction.ReturnType);
                Assert.Equal(typeof(Data), getData.StoreFunction.EntityTypeMappings.Single().TypeBase.ClrType);
                Assert.Single(getData.Parameters);

                var getDataParameter = getData.Parameters[0];
                Assert.Same(getData, getDataParameter.Function);
                Assert.Equal("id", getDataParameter.Name);
                Assert.NotNull(getDataParameter.StoreType);
                Assert.False(getDataParameter.PropagatesNullability);
                Assert.Equal(typeof(int), getDataParameter.ClrType);
                Assert.Equal(getDataParameter.StoreType, getDataParameter.TypeMapping!.StoreType);
                Assert.Equal("id", getDataParameter.StoreFunctionParameter.Name);
                Assert.Equal(getDataParameter.StoreType, getDataParameter.StoreFunctionParameter.StoreType);

                var getDataParameterless = model.FindDbFunction(
                    typeof(DbFunctionContext).GetMethod("GetData", new Type[0])!)!;
                Assert.Equal("GetAllData", getDataParameterless.Name);
                //Assert.Equal("dbo", getDataParameterless.Schema);
                Assert.Equal(typeof(DbFunctionContext).FullName + ".GetData()", getDataParameterless.ModelName);
                Assert.False(getDataParameterless.IsNullable);
                Assert.False(getDataParameterless.IsScalar);
                Assert.False(getDataParameterless.IsBuiltIn);
                Assert.False(getDataParameterless.IsAggregate);
                Assert.Null(getDataParameterless.Translation);
                Assert.Equal(typeof(IQueryable<Data>), getDataParameterless.ReturnType);
                Assert.Equal("GetData", getDataParameterless.MethodInfo!.Name);
                Assert.Empty(getDataParameterless.GetAnnotations());
                Assert.Empty(getDataParameterless.GetRuntimeAnnotations());
                Assert.False(getDataParameterless.StoreFunction.IsBuiltIn);
                Assert.Equal(typeof(Data), getDataParameterless.StoreFunction.EntityTypeMappings.Single().TypeBase.ClrType);
                Assert.Equal(0, getDataParameterless.Parameters.Count);

                Assert.Equal(2, model.GetEntityTypes().Count());
                var dataEntity = model.FindEntityType(typeof(Data))!;
                Assert.Null(dataEntity.FindPrimaryKey());
                var dataEntityFunctionMapping = dataEntity.GetFunctionMappings().Single(m => m.IsDefaultFunctionMapping);
                Assert.Null(dataEntityFunctionMapping.IncludesDerivedTypes);
                Assert.Null(dataEntityFunctionMapping.IsSharedTablePrincipal);
                Assert.Null(dataEntityFunctionMapping.IsSplitEntityTypePrincipal);
                Assert.Same(getDataParameterless, dataEntityFunctionMapping.DbFunction);

                var getDataStoreFunction = dataEntityFunctionMapping.StoreFunction;
                Assert.Same(getDataParameterless, getDataStoreFunction.DbFunctions.Single());
                Assert.False(getDataStoreFunction.IsOptional(dataEntity));

                var dataEntityOtherFunctionMapping = dataEntity.GetFunctionMappings().Single(m => !m.IsDefaultFunctionMapping);
                Assert.Null(dataEntityOtherFunctionMapping.IncludesDerivedTypes);
                Assert.Null(dataEntityOtherFunctionMapping.IsSharedTablePrincipal);
                Assert.Null(dataEntityOtherFunctionMapping.IsSplitEntityTypePrincipal);
                Assert.Same(getData, dataEntityOtherFunctionMapping.DbFunction);

                var getDataOtherStoreFunction = dataEntityOtherFunctionMapping.StoreFunction;
                Assert.Same(getData, getDataOtherStoreFunction.DbFunctions.Single());
                Assert.False(getDataOtherStoreFunction.IsOptional(dataEntity));

                var getBlobs = model.FindDbFunction("GetBlobs()")!;
                //Assert.Equal("dbo", getBlobs.Schema);
                Assert.False(getBlobs.IsNullable);
                Assert.False(getBlobs.IsScalar);
                Assert.False(getBlobs.IsBuiltIn);
                Assert.False(getBlobs.IsAggregate);
                Assert.Null(getBlobs.Translation);
                Assert.Null(getBlobs.TypeMapping);
                Assert.Equal(typeof(IQueryable<object>), getBlobs.ReturnType);
                Assert.Null(getBlobs.MethodInfo);
                Assert.Empty(getBlobs.GetAnnotations());
                Assert.Empty(getBlobs.GetRuntimeAnnotations());
                Assert.Equal("GetBlobs", getBlobs.StoreFunction.Name);
                Assert.False(getBlobs.StoreFunction.IsShared);
                Assert.NotNull(getBlobs.ToString());
                Assert.Empty(getBlobs.Parameters);

                var objectEntity = model.FindEntityType(typeof(object))!;
                Assert.Null(objectEntity.FindPrimaryKey());
                var objectEntityFunctionMapping = objectEntity.GetFunctionMappings().Single(m => m.IsDefaultFunctionMapping);
                Assert.Null(objectEntityFunctionMapping.IncludesDerivedTypes);
                Assert.Null(objectEntityFunctionMapping.IsSharedTablePrincipal);
                Assert.Null(objectEntityFunctionMapping.IsSplitEntityTypePrincipal);
                Assert.Same(getBlobs, objectEntityFunctionMapping.DbFunction);
            });

    public class DbFunctionContext(DbContextOptions<DbFunctionContext> options) : DbContext(options)
    {
        public static bool IsDateStatic(string date)
            => throw new NotImplementedException();

        private int GetCount(Guid? id, string condition)
            => throw new NotImplementedException();

        public IQueryable<Data> GetData(int id)
            => FromExpression(() => GetData(id));

        public IQueryable<Data> GetData()
            => FromExpression(() => GetData());

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            => configurationBuilder.DefaultTypeMapping<string>().HasMaxLength(256).IsFixedLength();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDbFunction(
                    typeof(DbFunctionContext).GetMethod(nameof(GetCount), BindingFlags.NonPublic | BindingFlags.Instance)!)
                .HasName("CustomerOrderCount").HasSchema("dbf").IsNullable(false)
                .HasParameter("id").PropagatesNullability().Metadata.SetAnnotation("MyAnnotation", new[] { 1L });

            modelBuilder.HasDbFunction(typeof(DbFunctionContext).GetMethod(nameof(IsDateStatic))!).HasName("IsDate").IsBuiltIn()
                .Metadata.SetAnnotation("MyGuid", new Guid());

            modelBuilder.HasDbFunction(typeof(DbFunctionContext).GetMethod(nameof(GetData), [typeof(int)])!);
            modelBuilder.HasDbFunction(typeof(DbFunctionContext).GetMethod(nameof(GetData), new Type[0])!);

            modelBuilder.Entity<Data>().ToFunction(typeof(DbFunctionContext).FullName + ".GetData()", f => f.HasName("GetAllData"))
                .HasNoKey();

            modelBuilder.Entity<object>().ToFunction("GetBlobs()", f => f.HasName("GetBlobs")).HasNoKey();
        }
    }

    [ConditionalFact]
    public virtual Task Custom_function_type_mapping()
        => Test<FunctionTypeMappingContext>(
            assertModel: model =>
            {
                var function = model.GetDbFunctions().Single();

                var typeMapping = function.TypeMapping;
                Assert.IsType<StringTypeMapping>(typeMapping);
                Assert.Equal("varchar", typeMapping.StoreType);
            });

    public class FunctionTypeMappingContext(DbContextOptions<FunctionTypeMappingContext> options) : DbContext(options)
    {
        public static string GetSqlFragmentStatic(string param)
            => throw new NotImplementedException();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDbFunction(typeof(FunctionTypeMappingContext).GetMethod(nameof(GetSqlFragmentStatic))!)
                .Metadata.TypeMapping = new StringTypeMapping("varchar", DbType.AnsiString);
        }
    }

    [ConditionalFact]
    public virtual Task Custom_function_parameter_type_mapping()
        => Test<FunctionParameterTypeMappingContext>(
            assertModel: model =>
            {
                var function = model.GetDbFunctions().Single();
                var parameter = function.Parameters.Single();

                var typeMapping = parameter.TypeMapping;
                Assert.IsType<StringTypeMapping>(typeMapping);
                Assert.Equal("varchar", typeMapping.StoreType);
            });

    public class FunctionParameterTypeMappingContext(DbContextOptions<FunctionParameterTypeMappingContext> options) : DbContext(options)
    {
        public static string GetSqlFragmentStatic(string param)
            => throw new NotImplementedException();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDbFunction(typeof(FunctionParameterTypeMappingContext).GetMethod(nameof(GetSqlFragmentStatic))!)
                .HasParameter("param").Metadata.TypeMapping = new StringTypeMapping("varchar", DbType.AnsiString);
        }
    }

    [ConditionalFact]
    public virtual Task Throws_for_custom_function_translation()
        => Test<FunctionTranslationContext>(
            expectedExceptionMessage: RelationalStrings.CompiledModelFunctionTranslation("GetSqlFragmentStatic"));

    public class FunctionTranslationContext(DbContextOptions<FunctionTranslationContext> options) : DbContext(options)
    {
        public static string GetSqlFragmentStatic()
            => throw new NotImplementedException();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDbFunction(typeof(FunctionTranslationContext).GetMethod(nameof(GetSqlFragmentStatic))!)
                .HasTranslation(args => new SqlFragmentExpression("NULL"));
        }
    }

    [ConditionalFact]
    public virtual void Dynamic_schema()
        => Test(
            modelBuilder => modelBuilder.Entity<Data>(
                eb =>
                {
                    eb.Property<int>("Id");
                    eb.HasKey("Id");
                }),
            model =>
            {
                Assert.Equal("custom", model.GetDefaultSchema());

                var dataEntity = model.GetEntityTypes().Single();
                Assert.Equal("custom", dataEntity.GetSchema());
            },
            additionalSourceFiles:
            [
                new()
                {
                    Path = "DbContextModelCustomizer.cs",
                    Code = """
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace;

public partial class DbContextModel
{
    private string DefaultSchema { get; init; } = "custom";

    partial void Customize()
    {
        RemoveAnnotation("Relational:DefaultSchema");
        AddAnnotation("Relational:DefaultSchema", DefaultSchema);
        RemoveRuntimeAnnotation("Relational:RelationalModel");

        foreach (RuntimeEntityType entityType in ((IModel)this).GetEntityTypes())
        {
            Customize(entityType);

            foreach (var key in entityType.GetDeclaredKeys())
            {
                key.RemoveRuntimeAnnotation(RelationalAnnotationNames.UniqueConstraintMappings);
            }

            foreach (var index in entityType.GetDeclaredIndexes())
            {
                index.RemoveRuntimeAnnotation(RelationalAnnotationNames.TableIndexMappings);
            }

            foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
            {
                foreignKey.RemoveRuntimeAnnotation(RelationalAnnotationNames.ForeignKeyMappings);
            }

            var tableName = entityType.FindAnnotation("Relational:TableName")?.Value as string;
            if (string.IsNullOrEmpty(tableName))
                continue;

            entityType.SetAnnotation("Relational:Schema", DefaultSchema);
        }
    }

    private static void Customize(RuntimeTypeBase entityType)
    {
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.DefaultMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.TableMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.ViewMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.SqlQueryMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.FunctionMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.InsertStoredProcedureMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.DeleteStoredProcedureMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.UpdateStoredProcedureMappings);

        foreach (var property in entityType.GetDeclaredProperties())
        {
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.DefaultColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.TableColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.ViewColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.SqlQueryColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.FunctionColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.InsertStoredProcedureParameterMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.DeleteStoredProcedureParameterMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.UpdateStoredProcedureParameterMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.InsertStoredProcedureResultColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.UpdateStoredProcedureResultColumnMappings);
        }

        foreach (var complexProperty in entityType.GetDeclaredComplexProperties())
        {
            Customize(complexProperty.ComplexType);
        }
    }
}
"""
                }
            ]);

    public class SpatialTypes : AbstractBase;

    protected override BuildSource AddReferences(BuildSource build, [CallerFilePath] string filePath = "")
    {
        base.AddReferences(build, filePath);
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Specification.Tests"));
        return build;
    }

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.ForeignKeyTpcPrincipalWarning));
}
