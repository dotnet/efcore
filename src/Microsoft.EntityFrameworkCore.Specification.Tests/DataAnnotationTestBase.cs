// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class DataAnnotationTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : DataAnnotationFixtureBase<TTestStore>, new()
    {
        protected DataAnnotationContext CreateContext() => Fixture.CreateContext(TestStore);

        protected virtual void ExecuteWithStrategyInTransaction(Action<DataAnnotationContext> testOperation)
            => DbContextHelpers.ExecuteWithStrategyInTransaction(CreateContext, UseTransaction, testOperation);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected DataAnnotationTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public virtual void Dispose() => TestStore.Dispose();

        public virtual ModelBuilder CreateModelBuilder()
        {
            var context = CreateContext();
            var conventionSetBuilder = context.GetService<IDatabaseProviderServices>().ConventionSetBuilder;
            var conventionSet = context.GetService<ICoreConventionSetBuilder>().CreateConventionSet();
            conventionSet = conventionSetBuilder == null
                ? conventionSet
                : conventionSetBuilder.AddConventions(conventionSet);
            return new ModelBuilder(conventionSet);
        }

        protected virtual void Validate(ModelBuilder modelBuilder)
        {
            modelBuilder.GetInfrastructure().Validate();
            var context = CreateContext();
            context.GetService<CoreModelValidator>().Validate(modelBuilder.Model);
            context.GetService<IModelValidator>().Validate(modelBuilder.Model);
        }

        protected class Person
        {
            public int Id { get; set; }

            [StringLength(5)]
            public string Name { get; set; }
        }

        protected class Employee : Person
        {
        }

        [Fact]
        public virtual void Explicit_configuration_on_derived_type_overrides_annotation_on_unmapped_base_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<Employee>()
                .Property(p => p.Name)
                .HasMaxLength(10);

            Validate(modelBuilder);

            Assert.Equal(10, GetProperty<Employee>(modelBuilder, "Name").GetMaxLength());
        }

        [Fact]
        public virtual void Explicit_configuration_on_derived_type_overrides_annotation_on_mapped_base_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<Person>();

            modelBuilder
                .Entity<Employee>()
                .Property(p => p.Name)
                .HasMaxLength(10);

            Validate(modelBuilder);

            Assert.Equal(10, GetProperty<Employee>(modelBuilder, "Name").GetMaxLength());
        }

        [Fact]
        public virtual void Explicit_configuration_on_derived_type_or_base_type_is_last_one_wins()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<Person>()
                .Property(p => p.Name)
                .HasMaxLength(5);

            modelBuilder
                .Entity<Employee>()
                .Property(p => p.Name)
                .HasMaxLength(10);

            Assert.Equal(10, GetProperty<Person>(modelBuilder, "Name").GetMaxLength());
            Assert.Equal(10, GetProperty<Employee>(modelBuilder, "Name").GetMaxLength());

            Validate(modelBuilder);

            modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<Employee>()
                .Property(p => p.Name)
                .HasMaxLength(10);

            modelBuilder
                .Entity<Person>()
                .Property(p => p.Name)
                .HasMaxLength(5);

            Validate(modelBuilder);

            Assert.Equal(5, GetProperty<Person>(modelBuilder, "Name").GetMaxLength());
            Assert.Equal(5, GetProperty<Employee>(modelBuilder, "Name").GetMaxLength());
        }

        protected static IMutableProperty GetProperty<TEntity>(ModelBuilder modelBuilder, string name)
            => modelBuilder.Model.FindEntityType(typeof(TEntity)).FindProperty(name);

        [Fact]
        public virtual void Duplicate_column_order_is_ignored()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Entity_10558>();

            Validate(modelBuilder);
        }

        protected class Entity_10558
        {
            [Key]
            [Column(Order = 1)]
            public int Key1 { get; set; }

            [Column(Order = 1)]
            public int Key2 { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public virtual ModelBuilder Non_public_annotations_are_enabled()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<PrivateMemberAnnotationClass>().Property(
                PrivateMemberAnnotationClass.PersonFirstNameExpr);

            Validate(modelBuilder);

            Assert.True(GetProperty<PrivateMemberAnnotationClass>(modelBuilder, "PersonFirstName").IsPrimaryKey());

            return modelBuilder;
        }

        protected class PrivateMemberAnnotationClass
        {
            public static readonly Expression<Func<PrivateMemberAnnotationClass, string>> PersonFirstNameExpr =
                p => p.PersonFirstName;

            public static Expression<Func<PrivateMemberAnnotationClass, object>> PersonFirstNameObjectExpr =
                p => p.PersonFirstName;

            [Key]
            [Column("dsdsd", Order = 1, TypeName = "nvarchar(128)")]
            private string PersonFirstName { get; set; }
        }

        [Fact]
        public virtual ModelBuilder Field_annotations_are_enabled()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<FieldAnnotationClass>().Property<string>("_personFirstName");

            Validate(modelBuilder);

            Assert.True(GetProperty<FieldAnnotationClass>(modelBuilder, "_personFirstName").IsPrimaryKey());

            return modelBuilder;
        }

        protected class FieldAnnotationClass
        {
#pragma warning disable 169
            [Key]
            [Column("dsdsd", Order = 1, TypeName = "nvarchar(128)")]
            private string _personFirstName;
#pragma warning restore 169
        }

        [Fact]
        public virtual void NotMapped_should_propagate_down_inheritance_hierarchy()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<NotMappedDerived>();

            Validate(modelBuilder);

            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(NotMappedDerived)));
        }

        [NotMapped]
        protected class NotMappedBase
        {
            public int Id { get; set; }
        }

        protected class NotMappedDerived : NotMappedBase
        {
        }

        [Fact]
        public virtual void NotMapped_on_base_class_property_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Unit1>();
            modelBuilder.Entity<BaseEntity1>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity1)).FindProperty("BaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity1)).FindProperty("BaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit1)).FindProperty("BaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity1)).FindProperty("VirtualBaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity1)).FindProperty("VirtualBaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit1)).FindProperty("VirtualBaseClassProperty"));
        }

        protected abstract class AbstractBaseEntity1
        {
            public long Id { get; set; }
            public abstract string AbstractBaseClassProperty { get; set; }
        }

        protected class BaseEntity1 : AbstractBaseEntity1
        {
            [NotMapped]
            public string BaseClassProperty { get; set; }

            [NotMapped]
            public virtual string VirtualBaseClassProperty { get; set; }

            public override string AbstractBaseClassProperty { get; set; }
        }

        protected class Unit1 : BaseEntity1
        {
            public override string VirtualBaseClassProperty { get; set; }
            public virtual AbstractBaseEntity1 Related { get; set; }
        }

        protected class DifferentUnit1 : BaseEntity1
        {
            public new string VirtualBaseClassProperty { get; set; }
        }

        [Fact]
        public virtual void NotMapped_on_base_class_property_and_overriden_property_ignores_them()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Unit2>();
            modelBuilder.Entity<BaseEntity2>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity2)).FindProperty("VirtualBaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity2)).FindProperty("VirtualBaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit2)).FindProperty("VirtualBaseClassProperty"));
        }

        protected abstract class AbstractBaseEntity2
        {
            public long Id { get; set; }
            public abstract string AbstractBaseClassProperty { get; set; }
        }

        protected class BaseEntity2 : AbstractBaseEntity2
        {
            public string BaseClassProperty { get; set; }

            [NotMapped]
            public virtual string VirtualBaseClassProperty { get; set; }

            public override string AbstractBaseClassProperty { get; set; }
        }

        protected class Unit2 : BaseEntity2
        {
            [NotMapped]
            public override string VirtualBaseClassProperty { get; set; }

            public virtual AbstractBaseEntity2 Related { get; set; }
        }

        protected class DifferentUnit2 : BaseEntity2
        {
            public new string VirtualBaseClassProperty { get; set; }
        }

        [Fact]
        public virtual void NotMapped_on_base_class_property_discovered_through_navigation_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Unit3>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity3)).FindProperty("AbstractBaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity3)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit3)).FindProperty("AbstractBaseClassProperty"));
        }

        [Fact]
        public virtual void NotMapped_on_overriden_mapped_base_class_property_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Unit3>();
            modelBuilder.Entity<BaseEntity3>();

            Assert.Equal(CoreStrings.InheritedPropertyCannotBeIgnored(
                nameof(Unit3.VirtualBaseClassProperty), typeof(Unit3).ShortDisplayName(), typeof(BaseEntity3).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () => Validate(modelBuilder)).Message);
        }

        [Fact]
        public virtual void NotMapped_on_unmapped_derived_property_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<AbstractBaseEntity3>();
            modelBuilder.Ignore<BaseEntity3>();
            modelBuilder.Entity<Unit3>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity3)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity3)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit3)).FindProperty("VirtualBaseClassProperty"));
        }

        protected abstract class AbstractBaseEntity3
        {
            public long Id { get; set; }

            [NotMapped]
            public abstract string AbstractBaseClassProperty { get; set; }
        }

        protected class BaseEntity3 : AbstractBaseEntity3
        {
            public string BaseClassProperty { get; set; }
            public virtual string VirtualBaseClassProperty { get; set; }
            public override string AbstractBaseClassProperty { get; set; }
        }

        protected class Unit3 : BaseEntity3
        {
            [NotMapped]
            public override string VirtualBaseClassProperty { get; set; }

            public virtual AbstractBaseEntity3 Related { get; set; }
        }

        protected class DifferentUnit3 : BaseEntity3
        {
            public new string VirtualBaseClassProperty { get; set; }
        }

        [Fact]
        public virtual void NotMapped_on_abstract_base_class_property_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<AbstractBaseEntity3>();
            modelBuilder.Entity<BaseEntity3>().Ignore(e => e.VirtualBaseClassProperty);
            modelBuilder.Entity<Unit3>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity3)).FindProperty("AbstractBaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity3)).FindProperty("AbstractBaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit3)).FindProperty("AbstractBaseClassProperty"));
        }

        [Fact]
        public virtual void NotMapped_on_unmapped_base_class_property_and_overriden_property_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<AbstractBaseEntity2>();
            modelBuilder.Ignore<BaseEntity2>();
            modelBuilder.Entity<Unit2>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity2)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity2)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit2)).FindProperty("VirtualBaseClassProperty"));
        }

        [Fact]
        public virtual void NotMapped_on_unmapped_base_class_property_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<AbstractBaseEntity1>();
            modelBuilder.Ignore<BaseEntity1>();
            modelBuilder.Entity<Unit1>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity1)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity1)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit1)).FindProperty("VirtualBaseClassProperty"));
        }

        [Fact]
        public virtual void NotMapped_on_new_property_with_same_name_as_in_unmapped_base_class_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<DifferentUnit5>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity5)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity5)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit5)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(DifferentUnit5)).FindProperty("VirtualBaseClassProperty"));
        }

        protected abstract class AbstractBaseEntity5
        {
            public long Id { get; set; }
            public abstract string AbstractBaseClassProperty { get; set; }
        }

        protected class BaseEntity5 : AbstractBaseEntity5
        {
            public string BaseClassProperty { get; set; }
            public virtual string VirtualBaseClassProperty { get; set; }
            public override string AbstractBaseClassProperty { get; set; }
        }

        protected class Unit5 : BaseEntity5
        {
            public override string VirtualBaseClassProperty { get; set; }
            public virtual AbstractBaseEntity5 Related { get; set; }
        }

        protected class DifferentUnit5 : BaseEntity5
        {
            [NotMapped]
            public new string VirtualBaseClassProperty { get; set; }
        }

        [Fact]
        public virtual void StringLength_with_value_takes_presedence_over_MaxLength()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<MaxLengthAnnotationClass>();

            Validate(modelBuilder);

            Assert.Equal(500, GetProperty<MaxLengthAnnotationClass>(modelBuilder, "PersonFirstName").GetMaxLength());
            Assert.Equal(500, GetProperty<MaxLengthAnnotationClass>(modelBuilder, "PersonLastName").GetMaxLength());
        }

        protected class MaxLengthAnnotationClass
        {
            public int Id { get; set; }

            [StringLength(500)]
            [MaxLength]
            public string PersonFirstName { get; set; }

            [MaxLength]
            [StringLength(500)]
            public string PersonLastName { get; set; }
        }

        [Fact]
        public virtual void MaxLength_with_length_takes_precedence_over_StringLength()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<MaxLengthWithLengthAnnotationClass>();

            Validate(modelBuilder);

            Assert.Equal(500, GetProperty<MaxLengthWithLengthAnnotationClass>(modelBuilder, "PersonFirstName").GetMaxLength());
            Assert.Equal(500, GetProperty<MaxLengthWithLengthAnnotationClass>(modelBuilder, "PersonLastName").GetMaxLength());
        }

        protected class MaxLengthWithLengthAnnotationClass
        {
            public int Id { get; set; }

            [StringLength(500)]
            [MaxLength(30)]
            public string PersonFirstName { get; set; }

            [MaxLength(30)]
            [StringLength(500)]
            public string PersonLastName { get; set; }
        }

        [Fact]
        public virtual ModelBuilder Default_length_for_key_string_column()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login1>();
            modelBuilder.Ignore<Profile1>();

            Validate(modelBuilder);

            return modelBuilder;
        }

        protected class Login1
        {
            public int Login1Id { get; set; }

            [Key]
            public string UserName { get; set; }

            public virtual Profile1 Profile { get; set; }
        }

        protected class Profile1
        {
            public int Profile1Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public virtual Login1 User { get; set; }
        }

        [Fact]
        public virtual ModelBuilder Key_and_column_work_together()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ColumnKeyAnnotationClass1>();

            Validate(modelBuilder);

            Assert.True(GetProperty<ColumnKeyAnnotationClass1>(modelBuilder, "PersonFirstName").IsPrimaryKey());

            return modelBuilder;
        }

        protected class ColumnKeyAnnotationClass1
        {
            [Key]
            [Column("dsdsd", Order = 1, TypeName = "nvarchar(128)")]
            public string PersonFirstName { get; set; }
        }

        [Fact]
        public virtual ModelBuilder Key_and_MaxLength_64_produce_nvarchar_64()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ColumnKeyAnnotationClass2>();

            Validate(modelBuilder);

            Assert.True(GetProperty<ColumnKeyAnnotationClass2>(modelBuilder, "PersonFirstName").IsPrimaryKey());
            Assert.Equal(64, GetProperty<ColumnKeyAnnotationClass2>(modelBuilder, "PersonFirstName").GetMaxLength());

            return modelBuilder;
        }

        protected class ColumnKeyAnnotationClass2
        {
            [Key]
            [MaxLength(64)]
            public string PersonFirstName { get; set; }
        }

        [Fact]
        public virtual void Key_from_base_type_is_recognized()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SRelated>();
            modelBuilder.Entity<OKeyBase>();

            Validate(modelBuilder);

            Assert.True(GetProperty<OKeyBase>(modelBuilder, "OrderLineNo").IsPrimaryKey());
            Assert.True(GetProperty<DODerived>(modelBuilder, "OrderLineNo").IsPrimaryKey());
        }

        [Fact]
        public virtual void Key_from_base_type_is_recognized_if_base_discovered_first()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OKeyBase>();
            modelBuilder.Entity<SRelated>();

            Validate(modelBuilder);

            Assert.True(GetProperty<OKeyBase>(modelBuilder, "OrderLineNo").IsPrimaryKey());
            Assert.True(GetProperty<DODerived>(modelBuilder, "OrderLineNo").IsPrimaryKey());
        }

        [Fact]
        public virtual void Key_from_base_type_is_recognized_if_discovered_through_relationship()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SRelated>();

            Validate(modelBuilder);
            Assert.True(GetProperty<OKeyBase>(modelBuilder, nameof(OKeyBase.OrderLineNo)).IsPrimaryKey());
            Assert.True(GetProperty<DODerived>(modelBuilder, nameof(DODerived.OrderLineNo)).IsPrimaryKey());
        }

        protected class SRelated
        {
            public int SRelatedId { get; set; }

            public ICollection<OKeyBase> OKeyBases { get; set; }
            public ICollection<DODerived> DADeriveds { get; set; }
        }

        protected class OKeyBase
        {
            [Key]
            public int OrderLineNo { get; set; }

            public int Quantity { get; set; }
        }

        protected class DODerived : OKeyBase
        {
            public SRelated DARelated { get; set; }
            public string Special { get; set; }
        }

        [Fact]
        public virtual void Key_on_nav_prop_is_ignored()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<KeyOnNavProp>();

            Validate(modelBuilder);

            Assert.True(GetProperty<KeyOnNavProp>(modelBuilder, "Id").IsPrimaryKey());
        }

        protected class DASimple
        {
            public int Id { get; set; }
        }

        protected class KeyOnNavProp
        {
            public int Id { get; set; }

            [Key]
            public ICollection<DASimple> Simples { get; set; }

            [Key]
            public DASimple SpecialSimple { get; set; }
        }

        [Fact]
        public virtual ModelBuilder Key_property_is_not_used_for_FK_when_set_by_annotation()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Parent>();
            modelBuilder.Entity<Child>();
            var toy = modelBuilder.Entity<Toy>();

            Assert.False(toy.Metadata.GetForeignKeys().Any(fk => fk.IsUnique == false && fk.Properties.Any(p => p.Name == nameof(Toy.IdRow))));

            Validate(modelBuilder);

            return modelBuilder;
        }

        public class Parent
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int IdRow { get; set; }

            public string Name { get; set; }

            public ICollection<Child> Children { get; set; }
        }

        public class Child
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int IdRow { get; set; }

            public string Name { get; set; }
            public ICollection<Toy> Toys { get; set; }
        }

        public class Toy
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int IdRow { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public virtual ModelBuilder DatabaseGeneratedOption_configures_the_property_correctly()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<GeneratedEntity>();

            var entity = modelBuilder.Model.FindEntityType(typeof(GeneratedEntity));

            var id = entity.FindProperty(nameof(GeneratedEntity.Id));
            Assert.Equal(ValueGenerated.Never, id.ValueGenerated);
            Assert.False(id.RequiresValueGenerator);

            var identity = entity.FindProperty(nameof(GeneratedEntity.Identity));
            Assert.Equal(ValueGenerated.OnAdd, identity.ValueGenerated);

            var version = entity.FindProperty(nameof(GeneratedEntity.Version));
            Assert.Equal(ValueGenerated.OnAddOrUpdate, version.ValueGenerated);
            Assert.False(version.RequiresValueGenerator);

            Validate(modelBuilder);

            return modelBuilder;
        }

        public class GeneratedEntity
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Identity { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public Guid Version { get; set; }
        }

        [Fact]
        public virtual ModelBuilder Timestamp_takes_precedence_over_MaxLength()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<TimestampAndMaxlen>().Ignore(x => x.NonMaxTimestamp);

            Validate(modelBuilder);

            Assert.Null(GetProperty<TimestampAndMaxlen>(modelBuilder, "MaxTimestamp").GetMaxLength());

            return modelBuilder;
        }

        [Fact]
        public virtual ModelBuilder Timestamp_takes_precedence_over_MaxLength_with_value()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<TimestampAndMaxlen>().Ignore(x => x.MaxTimestamp);

            Validate(modelBuilder);

            Assert.Equal(100, GetProperty<TimestampAndMaxlen>(modelBuilder, "NonMaxTimestamp").GetMaxLength());

            return modelBuilder;
        }

        protected class TimestampAndMaxlen
        {
            public int Id { get; set; }

            [MaxLength]
            [Timestamp]
            public byte[] MaxTimestamp { get; set; }

            [MaxLength(100)]
            [Timestamp]
            public byte[] NonMaxTimestamp { get; set; }
        }

        [Fact]
        public virtual void Annotation_in_derived_class_when_base_class_processed_after_derived_class()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<StyledProduct>();
            modelBuilder.Entity<Product>();

            Validate(modelBuilder);

            Assert.Equal(150, GetProperty<StyledProduct>(modelBuilder, "Style").GetMaxLength());
        }

        protected class Product
        {
            public virtual int ProductID { get; set; }
        }

        protected class StyledProduct : Product
        {
            [StringLength(150)]
            public virtual string Style { get; set; }
        }

        [Fact]
        public virtual void Required_and_ForeignKey_to_Required()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login2>();
            modelBuilder.Entity<Profile2>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login2>(modelBuilder, "Login2Id").IsForeignKey());
        }

        protected class Login2
        {
            public int Login2Id { get; set; }
            public string UserName { get; set; }

            [Required]
            [ForeignKey("Login2Id")]
            public virtual Profile2 Profile { get; set; }
        }

        protected class Profile2
        {
            public int Profile2Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            [Required]
            public virtual Login2 User { get; set; }
        }

        [Fact]
        // Regression test for Dev11 Bug 94993
        public virtual void Required_to_Required_and_ForeignKey()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login3>();
            modelBuilder.Entity<Profile3>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Profile3>(modelBuilder, "Profile3Id").IsForeignKey());
        }

        protected class Login3
        {
            public int Login3Id { get; set; }
            public string UserName { get; set; }

            [Required]
            public virtual Profile3 Profile { get; set; }
        }

        protected class Profile3
        {
            public int Profile3Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            [Required]
            [ForeignKey("Profile3Id")]
            public virtual Login3 User { get; set; }
        }

        [Fact]
        public virtual void Required_and_ForeignKey_to_Required_and_ForeignKey()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login4>();
            modelBuilder.Entity<Profile4>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login4>(modelBuilder, nameof(Login4.Login4Id)).IsForeignKey());
            Assert.True(GetProperty<Profile4>(modelBuilder, nameof(Profile4.Profile4Id)).IsForeignKey());
        }

        [Fact]
        public virtual void Required_and_ForeignKey_to_Required_and_ForeignKey_can_be_overriden()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login4>()
                .HasOne(l => l.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Login4>(l => l.Login4Id);

            Validate(modelBuilder);

            Assert.True(GetProperty<Login4>(modelBuilder, nameof(Login4.Login4Id)).IsForeignKey());
            Assert.False(GetProperty<Profile4>(modelBuilder, nameof(Profile4.Profile4Id)).IsForeignKey());
        }

        protected class Login4
        {
            public int Login4Id { get; set; }
            public string UserName { get; set; }

            [Required]
            [ForeignKey("Login4Id")]
            public virtual Profile4 Profile { get; set; }
        }

        protected class Profile4
        {
            public int Profile4Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            [Required]
            [ForeignKey("Profile4Id")]
            public virtual Login4 User { get; set; }
        }

        [Fact]
        public virtual void ForeignKey_to_nothing()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login5>();
            modelBuilder.Entity<Profile5>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login5>(modelBuilder, "Login5Id").IsForeignKey());
        }

        protected class Login5
        {
            public int Login5Id { get; set; }
            public string UserName { get; set; }

            [ForeignKey("Login5Id")]
            public virtual Profile5 Profile { get; set; }
        }

        protected class Profile5
        {
            public int Profile5Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            public virtual Login5 User { get; set; }
        }

        [Fact]
        public virtual void Required_and_ForeignKey_to_nothing()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login6>();
            modelBuilder.Entity<Profile6>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login6>(modelBuilder, "Login6Id").IsForeignKey());
        }

        protected class Login6
        {
            public int Login6Id { get; set; }
            public string UserName { get; set; }

            [Required]
            [ForeignKey("Login6Id")]
            public virtual Profile6 Profile { get; set; }
        }

        protected class Profile6
        {
            public int Profile6Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            public virtual Login6 User { get; set; }
        }

        [Fact]
        public virtual void Nothing_to_ForeignKey()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login7>();
            modelBuilder.Entity<Profile7>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Profile7>(modelBuilder, "Profile7Id").IsForeignKey());
        }

        protected class Login7
        {
            public int Login7Id { get; set; }
            public string UserName { get; set; }

            public virtual Profile7 Profile { get; set; }
        }

        protected class Profile7
        {
            public int Profile7Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            [ForeignKey("Profile7Id")]
            public virtual Login7 User { get; set; }
        }

        [Fact]
        public virtual void Nothing_to_Required_and_ForeignKey()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login8>();
            modelBuilder.Entity<Profile8>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Profile8>(modelBuilder, "Profile8Id").IsForeignKey());
        }

        protected class Login8
        {
            public int Login8Id { get; set; }
            public string UserName { get; set; }

            public virtual Profile8 Profile { get; set; }
        }

        protected class Profile8
        {
            public int Profile8Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            [Required]
            [ForeignKey("Profile8Id")]
            public virtual Login8 User { get; set; }
        }

        [Fact]
        public virtual void ForeignKey_to_ForeignKey()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login9>();
            modelBuilder.Entity<Profile9>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login9>(modelBuilder, "Login9Id").IsForeignKey());
            Assert.True(GetProperty<Profile9>(modelBuilder, "Profile9Id").IsForeignKey());
        }

        protected class Login9
        {
            public int Login9Id { get; set; }
            public string UserName { get; set; }

            [ForeignKey("Login9Id")]
            public virtual Profile9 Profile { get; set; }
        }

        protected class Profile9
        {
            public int Profile9Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            [ForeignKey("Profile9Id")]
            public virtual Login9 User { get; set; }
        }

        [Fact]
        public virtual void ForeignKey_to_ForeignKey_same_name()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login10>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login10>(modelBuilder, "Id").IsForeignKey());
            Assert.True(GetProperty<Profile10>(modelBuilder, "Id").IsForeignKey());
        }

        public class Login10
        {
            public int Id { get; set; }

            [ForeignKey("Id")]
            public virtual Profile10 Login { get; set; }
        }

        public class Profile10
        {
            public int Id { get; set; }

            [ForeignKey("Id")]
            public Login10 User { get; set; }
        }

        [Fact]
        public virtual void ForeignKey_to_ForeignKey_same_name_one_shadow()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login11>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login11>(modelBuilder, nameof(Profile11.Profile11Id)).IsForeignKey());
            Assert.True(GetProperty<Profile11>(modelBuilder, nameof(Profile11.Profile11Id)).IsForeignKey());
        }

        protected class Login11
        {
            public int Login11Id { get; set; }

            [ForeignKey(nameof(Profile11.Profile11Id))]
            public virtual Profile11 Profile { get; set; }
        }

        protected class Profile11
        {
            public int Profile11Id { get; set; }

            [ForeignKey(nameof(Profile11Id))]
            public virtual Login11 User { get; set; }
        }

        [Fact]
        public virtual void Shared_ForeignKey_to_different_principals()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login12>();

            Validate(modelBuilder);

            var login = modelBuilder.Model.FindEntityType(typeof(Login12));
            var fk1 = login.FindNavigation(nameof(Login12.Profile)).ForeignKey;
            var fk2 = login.FindNavigation(nameof(Login12.ProfileDetails)).ForeignKey;

            Assert.NotSame(fk1, fk2);
            Assert.Equal(nameof(Login12.ProfileId), fk1.Properties.Single().Name);
            Assert.Equal(nameof(Login12.ProfileId), fk2.Properties.Single().Name);
        }

        public class Login12
        {
            public int Id { get; set; }
            public int ProfileId { get; set; }

            [ForeignKey("ProfileId")]
            public virtual Profile12 Profile { get; set; }

            [ForeignKey("ProfileId")]
            public virtual ProfileDetails12 ProfileDetails { get; set; }
        }

        public class Profile12
        {
            public int Id { get; set; }
        }

        public class ProfileDetails12
        {
            public int Id { get; set; }
        }

        [Fact]
        public virtual ModelBuilder TableNameAttribute_affects_table_name_in_TPH()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<TNAttrBase>();
            modelBuilder.Entity<TNAttrDerived>();

            Validate(modelBuilder);

            return modelBuilder;
        }

        [Table("A")]
        protected class TNAttrBase
        {
            public int Id { get; set; }
            public string BaseData { get; set; }
        }

        protected class TNAttrDerived : TNAttrBase
        {
            public string DerivedData { get; set; }
        }

        [Fact]
        public virtual void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    var clientRow = context.Ones.First(r => r.UniqueNo == 1);
                    clientRow.RowVersion = new Guid("00000000-0000-0000-0002-000000000001");
                    clientRow.RequiredColumn = "ChangedData";

                    using (var innerContext = CreateContext())
                    {
                        UseTransaction(innerContext.Database, context.Database.CurrentTransaction);
                        var storeRow = innerContext.Ones.First(r => r.UniqueNo == 1);
                        storeRow.RowVersion = new Guid("00000000-0000-0000-0003-000000000001");
                        storeRow.RequiredColumn = "ModifiedData";

                        innerContext.SaveChanges();

                        Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
                    }
                });
        }

        [Fact]
        public virtual void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    context.Ones.Add(new One { RequiredColumn = "Third", RowVersion = new Guid("00000000-0000-0000-0000-000000000003") });

                    context.SaveChanges();
                });
        }

        [Fact]
        public virtual void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    context.Ones.Add(new One { RequiredColumn = "ValidString", RowVersion = new Guid("00000000-0000-0000-0000-000000000001"), MaxLengthProperty = "Short" });

                    context.SaveChanges();
                });

            ExecuteWithStrategyInTransaction(context =>
                {
                    context.Ones.Add(new One { RequiredColumn = "ValidString", RowVersion = new Guid("00000000-0000-0000-0000-000000000002"), MaxLengthProperty = "VeryVeryVeryVeryVeryVeryLongString" });

                    Assert.Equal("An error occurred while updating the entries. See the inner exception for details.",
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
                });
        }

        [Fact]
        public virtual void NotMappedAttribute_ignores_entityType()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Two>();

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Tests.C)));
        }

        [Fact]
        public virtual void NotMappedAttribute_ignores_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Tests.Book>();

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(UselessBookDetails)));
        }

        [Fact]
        public virtual void NotMappedAttribute_ignores_property()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<One>();

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(One)).FindProperty("IgnoredProperty"));
        }

        [Fact]
        public virtual void NotMappedAttribute_ignores_explicit_interface_implementation_property()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<EntityAnnotationBase>();

            Assert.Empty(modelBuilder.Model.FindEntityType(typeof(EntityAnnotationBase)).GetProperties());
        }

        protected interface IEntityBase
        {
            int Target { get; set; }
        }

        protected class EntityAnnotationBase : IEntityBase
        {
            [NotMapped]
            int IEntityBase.Target { get; set; }
        }

        [Fact]
        public virtual void NotMappedAttribute_removes_ambiguity_in_relationship_building()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Book>();

            Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
            Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
            Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
        }

        [Fact]
        public virtual void NotMappedAttribute_removes_ambiguity_in_relationship_building_with_base()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BookDetailsBase>();
            modelBuilder.Entity<Book>();

            Assert.Same(model.FindEntityType(typeof(BookDetailsBase)), model.FindEntityType(typeof(BookDetails)).BaseType);
            Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
            Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetailsBase)).GetNavigations().Select(nav => nav.Name));
            Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));

            modelBuilder.Entity<BookDetails>().HasBaseType((Type)null);

            Assert.Same(model.FindEntityType(typeof(BookDetails)),
                model.FindEntityType(typeof(Book)).GetNavigations().Single(n => n.Name == "Details").ForeignKey.DeclaringEntityType);
            Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
            Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetailsBase)).GetNavigations().Select(nav => nav.Name));
            Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
        }

        [Fact]
        public virtual void InversePropertyAttribute_removes_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Entity<Book>();

            Assert.Equal(nameof(Book.Label),
                model.FindEntityType(typeof(BookLabel)).FindNavigation(nameof(BookLabel.Book)).FindInverse()?.Name);

            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).FindInverse());
        }

        [Fact]
        public virtual void InversePropertyAttribute_removes_ambiguity_with_base_type()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<SpecialBookLabel>();

            Assert.Same(model.FindEntityType(typeof(BookLabel)), model.FindEntityType(typeof(SpecialBookLabel)).BaseType);

            Assert.Equal(nameof(Book.Label), model.FindEntityType(typeof(SpecialBookLabel))
                .FindNavigation(nameof(SpecialBookLabel.Book)).FindInverse()?.Name);
            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).FindInverse());

            modelBuilder.Entity<SpecialBookLabel>().HasBaseType((Type)null);

            Assert.Null(model.FindEntityType(typeof(SpecialBookLabel)).FindNavigation(nameof(SpecialBookLabel.Book))?.FindInverse());
            Assert.Null(model.FindEntityType(typeof(BookLabel)).FindNavigation(nameof(SpecialBookLabel.Book)));
            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)));
        }

        [Fact]
        public virtual void InversePropertyAttribute_removes_ambiguity_with_base_type_ignored()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Entity<SpecialBookLabel>();
            modelBuilder.Ignore<BookLabel>();

            Assert.Null(model.FindEntityType(typeof(BookLabel)));
            Assert.Equal(nameof(Book.Label), model.FindEntityType(typeof(SpecialBookLabel))
                .FindNavigation(nameof(SpecialBookLabel.Book)).FindInverse()?.Name);
            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)));
        }

        [Fact]
        public virtual void InversePropertyAttribute_from_ignored_base_causes_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<BookDetails>();
            modelBuilder.Entity<SpecialBookLabel>();
            modelBuilder.Ignore<BookLabel>();

            Assert.Null(model.FindEntityType(typeof(BookLabel)));
            Assert.Null(model.FindEntityType(typeof(AnotherBookLabel)).FindNavigation(nameof(AnotherBookLabel.Book)).FindInverse());
            Assert.Null(model.FindEntityType(typeof(SpecialBookLabel)).FindNavigation(nameof(SpecialBookLabel.Book)).FindInverse());
            Assert.Equal(0, model.FindEntityType(typeof(Book)).GetNavigations().Count());
        }

        [Fact]
        public virtual void InversePropertyAttribute_from_ignored_base_can_be_ignored_to_remove_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<BookLabel>();
            modelBuilder.Entity<AnotherBookLabel>().Ignore(e => e.Book);
            modelBuilder.Entity<SpecialBookLabel>();

            Assert.Null(model.FindEntityType(typeof(BookLabel)));
            Assert.Equal(nameof(Book.Label), model.FindEntityType(typeof(SpecialBookLabel))
                .FindNavigation(nameof(SpecialBookLabel.Book)).FindInverse()?.Name);
            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)));
        }

        private class Book
        {
            public static readonly PropertyInfo BookdDetailsNavigation = typeof(Book).GetTypeInfo().GetDeclaredProperty("Details");

            public int Id { get; set; }

            public BookLabel Label { get; set; }

            public BookLabel AlternateLabel { get; set; }

            public BookDetails Details { get; set; }
        }

        private abstract class BookDetailsBase
        {
            public int Id { get; set; }

            public int AnotherBookId { get; set; }

            public Book AnotherBook { get; set; }
        }

        private class BookDetails : BookDetailsBase
        {
            [NotMapped]
            public Book Book { get; set; }
        }

        private class BookLabel
        {
            public int Id { get; set; }

            [InverseProperty("Label")]
            public Book Book { get; set; }

            public int BookId { get; set; }

            public SpecialBookLabel SpecialBookLabel { get; set; }

            public AnotherBookLabel AnotherBookLabel { get; set; }
        }

        private class SpecialBookLabel : BookLabel
        {
            public BookLabel BookLabel { get; set; }
        }

        private class ExtraSpecialBookLabel : SpecialBookLabel
        {
        }

        private class AnotherBookLabel : BookLabel
        {
        }

        [Fact]
        public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_property_on_both_side()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Post>();

            Assert.Null(model.FindEntityType(typeof(Post)).FindNavigation("PostDetails").ForeignKey.PrincipalToDependent);
            Assert.Equal("PostDetailsId", model.FindEntityType(typeof(Post)).FindNavigation("PostDetails").ForeignKey.Properties.First().Name);

            Assert.Null(model.FindEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
            Assert.Equal("PostId", model.FindEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.Properties.First().Name);
        }

        [Fact]
        public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_navigations_on_both_side_and_values_do_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Post>();

            Assert.Null(model.FindEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.PrincipalToDependent);
            Assert.Equal("AuthorId", model.FindEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.Properties.First().Name);

            Assert.Null(model.FindEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
            Assert.Equal("PostId", model.FindEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.Properties.First().Name);
        }

        [Fact]
        public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_navigation_and_property_on_different_side_and_values_do_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Author>();

            var authorDetails = model.FindEntityType(typeof(AuthorDetails));
            var firstFk = authorDetails.FindNavigation(nameof(AuthorDetails.Author)).ForeignKey;
            Assert.Equal(typeof(AuthorDetails), firstFk.DeclaringEntityType.ClrType);
            Assert.Equal("AuthorId", firstFk.Properties.First().Name);

            var author = model.FindEntityType(typeof(Author));
            var secondFk = author.FindNavigation(nameof(Author.AuthorDetails)).ForeignKey;
            Assert.Equal(typeof(Author), secondFk.DeclaringEntityType.ClrType);
            Assert.Equal("AuthorDetailsIdByAttribute", secondFk.Properties.First().Name);

            Assert.Equal(new[] { "Id", "AuthorId" }, authorDetails.GetProperties().Select(p => p.Name));
            Assert.Equal(new[] { "Id", "AuthorDetailsIdByAttribute", "PostId" }, author.GetProperties().Select(p => p.Name));
        }

        private class Post
        {
            public int Id { get; set; }

            [ForeignKey("PostDetails")]
            public int PostDetailsId { get; set; }

            public PostDetails PostDetails { get; set; }

            [ForeignKey("AuthorId")]
            public Author Author { get; set; }
        }

        private class PostDetails
        {
            public int Id { get; set; }

            [ForeignKey("Post")]
            public int PostId { get; set; }

            public Post Post { get; set; }
        }

        private class Author
        {
            public int Id { get; set; }

            [ForeignKey("PostId")]
            public Post Post { get; set; }

            [ForeignKey("AuthorDetailsIdByAttribute")]
            public AuthorDetails AuthorDetails { get; set; }
        }

        private class AuthorDetails
        {
            public int Id { get; set; }

            [ForeignKey("Author")]
            public int AuthorId { get; set; }

            public Author Author { get; set; }
        }

        [Fact]
        public virtual void ForeignKeyAttribute_throws_if_applied_on_property_on_both_side_but_navigations_are_connected_by_inverse_property()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(CoreStrings.InvalidRelationshipUsingDataAnnotations("B", nameof(A), "A", nameof(B)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<A>()).Message);
        }

        [Fact]
        public virtual void ForeignKeyAttribute_throws_if_applied_on_both_navigations_connected_by_inverse_property_but_values_do_not_match()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(CoreStrings.InvalidRelationshipUsingDataAnnotations("C", nameof(D), "D", nameof(C)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<D>()).Message);
        }

        private class A
        {
            public int Id { get; set; }

            [ForeignKey("B")]
            public int BId { get; set; }

            public B B { get; set; }
        }

        private class B
        {
            public int Id { get; set; }

            [ForeignKey("A")]
            public int AId { get; set; }

            [InverseProperty("B")]
            public A A { get; set; }
        }

        private class C
        {
            public int Id { get; set; }

            [ForeignKey("DId")]
            [InverseProperty("C")]
            public D D { get; set; }
        }

        private class D
        {
            public int Id { get; set; }

            [ForeignKey("CId")]
            public C C { get; set; }
        }

        [Fact]
        public virtual void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    context.BookDetails.Add(new BookDetail { BookId = "Book1" });

                    context.SaveChanges();
                });

            ExecuteWithStrategyInTransaction(context =>
                {
                    context.BookDetails.Add(new BookDetail());

                    Assert.Equal("An error occurred while updating the entries. See the inner exception for details.",
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
                });
        }

        [Fact]
        public virtual void RequiredAttribute_does_nothing_when_specified_on_nav_to_dependent_per_convention()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<AdditionalBookDetail>();

            var relationship = modelBuilder.Model.FindEntityType(typeof(AdditionalBookDetail))
                .FindNavigation(nameof(AdditionalBookDetail.BookDetail)).ForeignKey;
            Assert.Equal(typeof(AdditionalBookDetail), relationship.PrincipalEntityType.ClrType);
            Assert.False(relationship.IsRequired);
        }

        [Fact]
        public virtual void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    context.Ones.Add(new One { RequiredColumn = "ValidString", RowVersion = new Guid("00000000-0000-0000-0000-000000000001") });

                    context.SaveChanges();
                });

            ExecuteWithStrategyInTransaction(context =>
                {
                    context.Ones.Add(new One { RequiredColumn = null, RowVersion = new Guid("00000000-0000-0000-0000-000000000002") });

                    Assert.Equal("An error occurred while updating the entries. See the inner exception for details.",
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
                });
        }

        [Fact]
        public virtual void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    context.Twos.Add(new Two { Data = "ValidString" });

                    context.SaveChanges();
                });

            ExecuteWithStrategyInTransaction(context =>
                {
                    context.Twos.Add(new Two { Data = "ValidButLongString" });

                    Assert.Equal("An error occurred while updating the entries. See the inner exception for details.",
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
                });
        }

        [Fact]
        public virtual void TimestampAttribute_throws_if_value_in_database_changed()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    var clientRow = context.Twos.First(r => r.Id == 1);
                    clientRow.Data = "ChangedData";

                    using (var innerContext = CreateContext())
                    {
                        UseTransaction(innerContext.Database, context.Database.CurrentTransaction);
                        var storeRow = innerContext.Twos.First(r => r.Id == 1);
                        storeRow.Data = "ModifiedData";

                        innerContext.SaveChanges();

                        Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
                    }
                });
        }
    }
}
