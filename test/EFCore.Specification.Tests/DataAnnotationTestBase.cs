// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class DataAnnotationTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : DataAnnotationTestBase<TFixture>.DataAnnotationFixtureBase, new()
    {
        protected DataAnnotationTestBase(TFixture fixture)
        {
            Fixture = fixture;
            fixture.ListLoggerFactory.Clear();
        }

        protected TFixture Fixture { get; }

        protected DbContext CreateContext()
            => Fixture.CreateContext();

        protected virtual void ExecuteWithStrategyInTransaction(Action<DbContext> testOperation)
            => TestHelpers.ExecuteWithStrategyInTransaction(CreateContext, UseTransaction, testOperation);

        protected virtual void ExecuteWithStrategyInTransaction(Action<DbContext> testOperation1, Action<DbContext> testOperation2)
            => TestHelpers.ExecuteWithStrategyInTransaction(CreateContext, UseTransaction, testOperation1, testOperation2);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        public virtual ModelBuilder CreateModelBuilder()
        {
            var context = CreateContext();
            return new ModelBuilder(
                context.GetService<IConventionSetBuilder>().CreateConventionSet(),
                context.GetService<ModelDependencies>());
        }

        protected virtual IModel Validate(ModelBuilder modelBuilder)
        {
            var context = CreateContext();
            var modelRuntimeInitializer = context.GetService<IModelRuntimeInitializer>();
            var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Model.Validation>>();
            return modelRuntimeInitializer.Initialize((IModel)modelBuilder.Model, designTime: true, logger);
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            private string PersonFirstName { get; set; }
        }

        [ConditionalFact]
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
            private readonly string _personFirstName;
#pragma warning restore 169
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void NotMapped_on_base_class_property_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Unit1>();
            modelBuilder.Entity<BaseEntity1>().Property(e => e.BaseClassProperty);

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity1)).FindProperty("BaseClassProperty"));
            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(BaseEntity1)).FindProperty("BaseClassProperty"));
            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(Unit1)).FindProperty("BaseClassProperty"));
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

        [ConditionalFact]
        public virtual void NotMapped_on_base_class_property_and_overridden_property_ignores_them()
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

        [ConditionalFact]
        public virtual void NotMapped_on_base_class_property_discovered_through_navigation_ignores_it()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Unit3>();

            Validate(modelBuilder);

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(AbstractBaseEntity3)).FindProperty("AbstractBaseClassProperty"));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(BaseEntity3)));
            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Unit3)).FindProperty("AbstractBaseClassProperty"));
        }

        [ConditionalFact]
        public virtual void NotMapped_on_overridden_property_is_ignored()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Unit3>();
            modelBuilder.Entity<BaseEntity3>();

            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(Unit3)).FindProperty("VirtualBaseClassProperty"));

            Validate(modelBuilder);
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void NotMapped_on_unmapped_base_class_property_and_overridden_property_ignores_it()
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void StringLength_with_value_takes_precedence_over_MaxLength()
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Key_from_base_type_is_recognized()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SRelated>();
            modelBuilder.Entity<OKeyBase>();

            Validate(modelBuilder);

            Assert.True(GetProperty<OKeyBase>(modelBuilder, "OrderLineNo").IsPrimaryKey());
            Assert.True(GetProperty<DODerived>(modelBuilder, "OrderLineNo").IsPrimaryKey());
        }

        [ConditionalFact]
        public virtual void Key_from_base_type_is_recognized_if_base_discovered_first()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OKeyBase>();
            modelBuilder.Entity<SRelated>();

            Validate(modelBuilder);

            Assert.True(GetProperty<OKeyBase>(modelBuilder, "OrderLineNo").IsPrimaryKey());
            Assert.True(GetProperty<DODerived>(modelBuilder, "OrderLineNo").IsPrimaryKey());
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual ModelBuilder Key_property_is_not_used_for_FK_when_set_by_annotation()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Parent>();
            modelBuilder.Entity<Child>();
            var toy = modelBuilder.Entity<Toy>();

            Assert.DoesNotContain(
                toy.Metadata.GetForeignKeys(), fk => fk.IsUnique == false && fk.Properties.Any(p => p.Name == nameof(Toy.IdRow)));

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

        [ConditionalFact]
        public virtual ModelBuilder Key_specified_on_multiple_properties_can_be_overridden()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CompositeKeyAttribute>().HasKey(
                c => new { c.IdRow, c.Name });

            Validate(modelBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(CompositeKeyAttribute));
            Assert.Equal(2, entityType.GetKeys().Single().Properties.Count);
            Assert.Equal(2, entityType.GetProperties().Count());

            return modelBuilder;
        }

        [ConditionalFact]
        public virtual void Keyless_and_key_attributes_which_conflict_cause_warning()
        {
            var modelBuilder = CreateModelBuilder();
            var entity = modelBuilder.Entity<KeylessAndKeyAttributes>();

            Assert.True(entity.Metadata.IsKeyless);
            Assert.Null(entity.Metadata.FindPrimaryKey());

            var logEntry = Fixture.ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Warning, logEntry.Level);
            Assert.Equal(
                CoreResources.LogConflictingKeylessAndKeyAttributes(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage("NotAKey", nameof(KeylessAndKeyAttributes)),
                logEntry.Message);
        }

        [ConditionalFact]
        public virtual void Keyless_fluent_api_and_key_attribute_do_not_cause_warning()
        {
            var modelBuilder = CreateModelBuilder();
            var entity = modelBuilder.Entity<KeylessFluentApiAndKeyAttribute>();
            entity.HasNoKey();

            Assert.True(entity.Metadata.IsKeyless);
            Assert.Null(entity.Metadata.FindPrimaryKey());

            Assert.Empty(Fixture.ListLoggerFactory.Log);
        }

        [ConditionalFact]
        public virtual void Key_fluent_api_and_keyless_attribute_do_not_cause_warning()
        {
            var modelBuilder = CreateModelBuilder();
            var entity = modelBuilder.Entity<KeyFluentApiAndKeylessAttribute>();
            entity.HasKey("MyKey");

            Assert.False(entity.Metadata.IsKeyless);
            Assert.NotNull(entity.Metadata.FindPrimaryKey());

            Assert.Empty(Fixture.ListLoggerFactory.Log);
        }

        private class CompositeKeyAttribute
        {
            [Key]
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int IdRow { get; set; }

            [Key]
            public string Name { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        [ConditionalFact]
        public virtual ModelBuilder DatabaseGeneratedOption_configures_the_property_correctly()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<GeneratedEntity>().HasAlternateKey(
                e => new { e.Identity, e.Version });

            var entity = modelBuilder.Model.FindEntityType(typeof(GeneratedEntity));

            var id = entity.FindProperty(nameof(GeneratedEntity.Id));
            Assert.Equal(ValueGenerated.Never, id.ValueGenerated);
            Assert.False(id.RequiresValueGenerator());

            var identity = entity.FindProperty(nameof(GeneratedEntity.Identity));
            Assert.Equal(ValueGenerated.OnAdd, identity.ValueGenerated);
            Assert.True(identity.RequiresValueGenerator());

            var version = entity.FindProperty(nameof(GeneratedEntity.Version));
            Assert.Equal(ValueGenerated.OnAddOrUpdate, version.ValueGenerated);
            Assert.True(version.RequiresValueGenerator());

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

        [ConditionalFact]
        public virtual ModelBuilder DatabaseGeneratedOption_Identity_does_not_throw_on_noninteger_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<GeneratedEntityNonInteger>().HasAlternateKey(
                e => new
                {
                    e.String,
                    e.DateTime,
                    e.Guid
                });

            var entity = modelBuilder.Model.FindEntityType(typeof(GeneratedEntityNonInteger));

            var stringProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.String));
            Assert.Equal(ValueGenerated.OnAdd, stringProperty.ValueGenerated);
            Assert.True(stringProperty.RequiresValueGenerator());

            var dateTimeProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.DateTime));
            Assert.Equal(ValueGenerated.OnAdd, dateTimeProperty.ValueGenerated);
            Assert.True(dateTimeProperty.RequiresValueGenerator());

            var guidProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.Guid));
            Assert.Equal(ValueGenerated.OnAdd, guidProperty.ValueGenerated);
            Assert.True(guidProperty.RequiresValueGenerator());

            Validate(modelBuilder);

            return modelBuilder;
        }

        public class GeneratedEntityNonInteger
        {
            public int Id { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public string String { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public DateTime DateTime { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public Guid Guid { get; set; }
        }

        [ConditionalFact]
        public virtual ModelBuilder Timestamp_takes_precedence_over_MaxLength()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<TimestampAndMaxlen>().Ignore(x => x.NonMaxTimestamp);

            Validate(modelBuilder);

            Assert.Null(GetProperty<TimestampAndMaxlen>(modelBuilder, "MaxTimestamp").GetMaxLength());

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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Required_and_ForeignKey_to_Required_and_ForeignKey()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login4>();
            modelBuilder.Entity<Profile4>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login4>(modelBuilder, nameof(Login4.Login4Id)).IsForeignKey());
            Assert.True(GetProperty<Profile4>(modelBuilder, nameof(Profile4.Profile4Id)).IsForeignKey());
        }

        [ConditionalFact]
        public virtual void Required_and_ForeignKey_to_Required_and_ForeignKey_can_be_overridden()
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
            public int Id { get; set; }
            public int Login4Id { get; set; }
            public string UserName { get; set; }

            [Required]
            [ForeignKey("Login4Id")]
            public virtual Profile4 Profile { get; set; }
        }

        protected class Profile4
        {
            public int Id { get; set; }
            public int Profile4Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            [Required]
            [ForeignKey("Profile4Id")]
            public virtual Login4 User { get; set; }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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
            public int Id { get; set; }
            public int? Login9Id { get; set; }
            public string UserName { get; set; }

            [ForeignKey("Login9Id")]
            public virtual Profile9 Profile { get; set; }
        }

        protected class Profile9
        {
            public int Id { get; set; }
            public int? Profile9Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            [ForeignKey("Profile9Id")]
            public virtual Login9 User { get; set; }
        }

        [ConditionalFact]
        public virtual void ForeignKey_to_ForeignKey_same_name()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login10>();

            Validate(modelBuilder);

            Assert.True(GetProperty<Login10>(modelBuilder, "FkId").IsForeignKey());
            Assert.True(GetProperty<Profile10>(modelBuilder, "FkId").IsForeignKey());
        }

        public class Login10
        {
            public int Id { get; set; }

            public int FkId { get; set; }

            [ForeignKey("FkId")]
            public virtual Profile10 Login { get; set; }
        }

        public class Profile10
        {
            public int Id { get; set; }

            public int FkId { get; set; }

            [ForeignKey("FkId")]
            public Login10 User { get; set; }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void ForeignKeyAttribute_configures_relationships_when_inverse_on_derived()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Answer>();
            modelBuilder.Entity<MultipleAnswers>()
                .HasMany(m => m.Answers)
                .WithOne(p => (MultipleAnswers)p.Answer)
                .HasForeignKey(p => p.AnswerId);
            modelBuilder.Entity<MultipleAnswersRepeating>()
                .HasMany(m => m.Answers)
                .WithOne(p => (MultipleAnswersRepeating)p.Answer)
                .HasForeignKey(p => p.AnswerId);

            modelBuilder.Entity<PartialAnswerBase>();
            modelBuilder.Entity<PartialAnswer>();
            modelBuilder.Entity<PartialAnswerRepeating>();

            var model = modelBuilder.FinalizeModel();

            var fk1 = model.FindEntityType(typeof(PartialAnswer)).GetForeignKeys().Single();
            Assert.Equal(nameof(PartialAnswer.Answer), fk1.DependentToPrincipal.Name);
            Assert.Equal(nameof(MultipleAnswers.Answers), fk1.PrincipalToDependent.Name);
            Assert.Equal(nameof(PartialAnswer.AnswerId), fk1.Properties.Single().Name);

            var fk2 = model.FindEntityType(typeof(PartialAnswerRepeating)).GetForeignKeys().Single();
            Assert.Equal(nameof(PartialAnswerRepeating.Answer), fk2.DependentToPrincipal.Name);
            Assert.Equal(nameof(MultipleAnswersRepeating.Answers), fk2.PrincipalToDependent.Name);
            Assert.Equal(nameof(PartialAnswerRepeating.AnswerId), fk2.Properties.Single().Name);
        }

        private abstract class Answer
        {
            public int Id { get; set; }
        }

        private class PartialAnswerBase
        {
            public int Id { get; set; }
            public int AnswerId { get; set; }

            [ForeignKey("AnswerId")]
            public virtual Answer Answer { get; set; }
        }

        private class PartialAnswer : PartialAnswerBase
        {
        }

        private class PartialAnswerRepeating : PartialAnswerBase
        {
        }

        private class MultipleAnswers : Answer
        {
            public virtual ICollection<PartialAnswer> Answers { get; set; }
        }

        private class MultipleAnswersRepeating : Answer
        {
            public virtual ICollection<PartialAnswerRepeating> Answers { get; set; }
        }

        [ConditionalFact]
        public virtual void ForeignKeyAttribute_configures_two_self_referencing_relationships()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Comment>();

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(Comment));
            var fk1 = entityType.GetForeignKeys().Single(fk => fk.Properties.Single().Name == nameof(Comment.ParentCommentID));
            Assert.Equal(nameof(Comment.ParentComment), fk1.DependentToPrincipal.Name);
            Assert.Null(fk1.PrincipalToDependent);
            var index1 = entityType.FindIndex(fk1.Properties);
            Assert.False(index1.IsUnique);

            var fk2 = entityType.GetForeignKeys().Single(fk => fk.Properties.Single().Name == nameof(Comment.ReplyCommentID));
            Assert.Equal(nameof(Comment.ReplyComment), fk2.DependentToPrincipal.Name);
            Assert.Null(fk2.PrincipalToDependent);
            var index2 = entityType.FindIndex(fk2.Properties);
            Assert.False(index2.IsUnique);

            Assert.Equal(2, entityType.GetForeignKeys().Count());
            Assert.Equal(2, entityType.GetIndexes().Count());
        }

        private class Comment
        {
            [Key]
            public long CommentID { get; set; }

            public long? ReplyCommentID { get; set; }

            public long? ParentCommentID { get; set; }

            [ForeignKey("ParentCommentID")]
            public virtual Comment ParentComment { get; set; }

            [ForeignKey("ReplyCommentID")]
            public virtual Comment ReplyComment { get; set; }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var clientRow = context.Set<One>().First(r => r.UniqueNo == 1);
                    clientRow.RowVersion = new Guid("00000000-0000-0000-0002-000000000001");
                    clientRow.RequiredColumn = "ChangedData";

                    using var innerContext = CreateContext();
                    UseTransaction(innerContext.Database, context.Database.CurrentTransaction);
                    var storeRow = innerContext.Set<One>().First(r => r.UniqueNo == 1);
                    storeRow.RowVersion = new Guid("00000000-0000-0000-0003-000000000001");
                    storeRow.RequiredColumn = "ModifiedData";

                    innerContext.SaveChanges();

                    Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
                });
        }

        [ConditionalFact]
        public virtual void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<One>().Add(
                        new One
                        {
                            RequiredColumn = "Third",
                            RowVersion = new Guid("00000000-0000-0000-0000-000000000003"),
                            Details = new Details { Name = "Third Name" },
                            AdditionalDetails = new Details { Name = "Third Additional Name" }
                        });

                    context.SaveChanges();
                });
        }

        [ConditionalFact]
        public virtual void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<One>().Add(
                        new One
                        {
                            RequiredColumn = "ValidString",
                            RowVersion = new Guid("00000000-0000-0000-0000-000000000001"),
                            MaxLengthProperty = "Short",
                            Details = new Details { Name = "Third Name" },
                            AdditionalDetails = new Details { Name = "Third Additional Name" }
                        });

                    context.SaveChanges();
                });

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<One>().Add(
                        new One
                        {
                            RequiredColumn = "ValidString",
                            RowVersion = new Guid("00000000-0000-0000-0000-000000000002"),
                            MaxLengthProperty = "VeryVeryVeryVeryVeryVeryLongString",
                            Details = new Details { Name = "Third Name" },
                            AdditionalDetails = new Details { Name = "Third Additional Name" }
                        });

                    Assert.Equal(
                        "An error occurred while saving the entity changes. See the inner exception for details.",
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public virtual void NotMappedAttribute_ignores_entityType()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Two>();

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(C)));
        }

        [ConditionalFact]
        public virtual void NotMappedAttribute_ignores_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Book>();

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(UselessBookDetails)));
        }

        [ConditionalFact]
        public virtual void NotMappedAttribute_ignores_property()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<One>();

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(One)).FindProperty("IgnoredProperty"));
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void NotMappedAttribute_removes_ambiguity_in_relationship_building()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Book>();

            Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
            Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
            Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
        }

        [ConditionalFact]
        public virtual void NotMappedAttribute_removes_ambiguity_in_relationship_building_with_base()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BookDetailsBase>();
            modelBuilder.Entity<Book>();

            Assert.Same(model.FindEntityType(typeof(BookDetailsBase)), model.FindEntityType(typeof(BookDetails)).BaseType);
            Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name).ToList());
            Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name).ToList());
            Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name).ToList());

            modelBuilder.Entity<BookDetails>().HasBaseType((Type)null);

            Assert.Same(
                model.FindEntityType(typeof(BookDetails)),
                model.FindEntityType(typeof(Book)).GetNavigations().Single(n => n.Name == "Details").ForeignKey.DeclaringEntityType);
            Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name).ToList());
            Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name).ToList());
            Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name).ToList());
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_removes_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Entity<Book>();

            Assert.Equal(
                nameof(Book.Label),
                model.FindEntityType(typeof(BookLabel)).FindNavigation(nameof(BookLabel.Book)).Inverse?.Name);

            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).Inverse);
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_removes_ambiguity_with_base_type()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<SpecialBookLabel>();

            Assert.Same(model.FindEntityType(typeof(BookLabel)), model.FindEntityType(typeof(SpecialBookLabel)).BaseType);

            Assert.Equal(
                nameof(Book.Label), model.FindEntityType(typeof(SpecialBookLabel))
                    .FindNavigation(nameof(SpecialBookLabel.Book)).Inverse?.Name);
            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).Inverse);

            modelBuilder.Entity<SpecialBookLabel>().HasBaseType((Type)null);

            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)));
            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)));
            Assert.Null(model.FindEntityType(typeof(BookLabel)).FindNavigation(nameof(SpecialBookLabel.Book)));
            Assert.Null(model.FindEntityType(typeof(SpecialBookLabel)).FindNavigation(nameof(SpecialBookLabel.Book)));
            Assert.Null(model.FindEntityType(typeof(AnotherBookLabel)).FindNavigation(nameof(SpecialBookLabel.Book)));
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_removes_ambiguity_with_base_type_ignored()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Entity<SpecialBookLabel>();
            modelBuilder.Ignore<BookLabel>();

            Assert.Null(model.FindEntityType(typeof(BookLabel)));
            Assert.Equal(
                nameof(Book.Label), model.FindEntityType(typeof(SpecialBookLabel))
                    .FindNavigation(nameof(SpecialBookLabel.Book)).Inverse?.Name);
            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)));
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_from_ignored_base_causes_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<BookDetails>();
            modelBuilder.Ignore<Details>();
            modelBuilder.Entity<SpecialBookLabel>();
            modelBuilder.Ignore<BookLabel>();

            Assert.Null(model.FindEntityType(typeof(AnotherBookLabel)).FindNavigation(nameof(AnotherBookLabel.Book)));
            Assert.Null(model.FindEntityType(typeof(SpecialBookLabel)).FindNavigation(nameof(SpecialBookLabel.Book)));
            Assert.Empty(model.FindEntityType(typeof(Book)).GetNavigations());
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_from_ignored_base_can_be_ignored_to_remove_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<BookLabel>();
            modelBuilder.Entity<AnotherBookLabel>().Ignore(e => e.Book);
            modelBuilder.Entity<SpecialBookLabel>();

            Assert.Null(model.FindEntityType(typeof(BookLabel)));
            Assert.Equal(
                nameof(Book.Label), model.FindEntityType(typeof(SpecialBookLabel))
                    .FindNavigation(nameof(SpecialBookLabel.Book)).Inverse?.Name);
            Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)));
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_removes_ambiguity_from_the_ambiguous_end()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<BookLabel>();
            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Entity<Book>().Ignore(e => e.AlternateLabel);
            modelBuilder.Entity<ExtraSpecialBookLabel>();

            Assert.Null(model.FindEntityType(typeof(BookLabel)));
            Assert.Equal(
                nameof(Book.Label), model.FindEntityType(typeof(ExtraSpecialBookLabel))
                    .FindNavigation(nameof(ExtraSpecialBookLabel.Book)).Inverse?.Name);
            Assert.Null(
                model.FindEntityType(typeof(ExtraSpecialBookLabel))
                    .FindNavigation(nameof(ExtraSpecialBookLabel.ExtraSpecialBook)).Inverse);
        }

        protected class Book
        {
            public static readonly PropertyInfo BookdDetailsNavigation = typeof(Book).GetTypeInfo().GetDeclaredProperty("Details");

            public int Id { get; set; }

            public BookLabel Label { get; set; }

            public BookLabel AlternateLabel { get; set; }

            public BookDetails Details { get; set; }

            public Details AdditionalDetails { get; set; }

            [NotMapped]
            public virtual UselessBookDetails UselessBookDetails { get; set; }
        }

        protected abstract class BookDetailsBase
        {
            public int Id { get; set; }

            public int? AnotherBookId { get; set; }

            [Required]
            public Book AnotherBook { get; set; }
        }

        protected class BookDetails : BookDetailsBase
        {
            public int? AdditionalBookDetailsId { get; set; }

            [NotMapped]
            public Book Book { get; set; }
        }

        protected class AdditionalBookDetails : BookDetailsBase
        {
            [Required]
            public virtual BookDetails BookDetails { get; set; }
        }

        protected class UselessBookDetails : BookDetailsBase
        {
            [NotMapped]
            public virtual Book Book { get; set; }
        }

        protected class BookLabel
        {
            public int Id { get; set; }

            [InverseProperty("Label")]
            public Book Book { get; set; }

            public int BookId { get; set; }

            public SpecialBookLabel SpecialBookLabel { get; set; }

            public AnotherBookLabel AnotherBookLabel { get; set; }
        }

        protected class SpecialBookLabel : BookLabel
        {
            public BookLabel BookLabel { get; set; }
        }

        protected class ExtraSpecialBookLabel : SpecialBookLabel
        {
            public Book ExtraSpecialBook { get; set; }
        }

        protected class AnotherBookLabel : BookLabel
        {
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_removes_ambiguity_when_combined_with_other_attributes()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Relation>();

            var accountNavigation = model.FindEntityType(typeof(Relation)).FindNavigation(nameof(Relation.AccountManager));
            Assert.Equal(nameof(User.AccountManagerRelations), accountNavigation?.Inverse?.Name);
            Assert.Equal(nameof(Relation.AccountId), accountNavigation?.ForeignKey.Properties.First().Name);

            var salesNavigation = model.FindEntityType(typeof(Relation)).FindNavigation(nameof(Relation.SalesManager));
            Assert.Equal(nameof(User.SalesManagerRelations), salesNavigation?.Inverse?.Name);
            Assert.Equal(nameof(Relation.SalesId), salesNavigation?.ForeignKey.Properties.First().Name);

            Validate(modelBuilder);
        }

        public class User
        {
            [Key]
            public int UserUId { get; set; }

            [InverseProperty(nameof(Relation.AccountManager))]
            public virtual ICollection<Relation> AccountManagerRelations { get; set; }

            public virtual ICollection<Relation> SalesManagerRelations { get; set; }
        }

        public class Relation
        {
            public string Id { get; set; }

            public int AccountId { get; set; }

            [ForeignKey(nameof(AccountId))]
            public virtual User AccountManager { get; set; }

            public int SalesId { get; set; }

            [ForeignKey(nameof(SalesId))]
            public virtual User SalesManager { get; set; }
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_removes_ambiguity_with_base_type_bidirectional()
        {
            var modelBuilder = CreateModelBuilder();
            var qEntity = modelBuilder.Entity<Q>().Metadata;

            Assert.Equal(nameof(P.QRef), qEntity.FindNavigation(nameof(Q.PRef)).Inverse.Name);
            Assert.Equal(nameof(E.QRefDerived), qEntity.FindNavigation(nameof(Q.ERef)).Inverse.Name);
        }

        public class Q
        {
            public int Id { get; set; }

            [InverseProperty(nameof(E.QRefDerived))]
            public virtual E ERef { get; set; }

            [InverseProperty(nameof(P.QRef))]
            public virtual P PRef { get; set; }
        }

        public class P
        {
            public int Id { get; set; }

            [InverseProperty(nameof(Q.PRef))]
            public virtual Q QRef { get; set; }
        }

        public class E : P
        {
            [InverseProperty(nameof(Q.ERef))]
            public virtual Q QRefDerived { get; set; }
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_is_noop_in_unambiguous_models()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Blog7698>();

            Validate(modelBuilder);

            Assert.Equal(
                nameof(Post7698.BlogNav),
                model.FindEntityType(typeof(Blog7698)).FindNavigation(nameof(Blog7698.PostNav)).Inverse.Name);
            Assert.Equal(
                nameof(SpecialPost7698.BlogInverseNav),
                model.FindEntityType(typeof(Blog7698)).FindNavigation(nameof(Blog7698.ASpecialPostNav)).Inverse.Name);
        }

        protected class Blog7698
        {
            public int Id { get; set; }

            [InverseProperty(nameof(Post7698.BlogNav))]
            public List<Post7698> PostNav { get; set; }

            [InverseProperty(nameof(SpecialPost7698.BlogInverseNav))]
            public List<SpecialPost7698> ASpecialPostNav { get; set; }
        }

        protected class Post7698
        {
            public int Id { get; set; }

            [InverseProperty(nameof(Blog7698.PostNav))]
            public Blog7698 BlogNav { get; set; }
        }

        protected class SpecialPost7698 : Post7698
        {
            [InverseProperty(nameof(Blog7698.ASpecialPostNav))]
            public Blog7698 BlogInverseNav { get; set; }
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_pointing_to_same_nav_on_base_causes_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<MultipleAnswersInverse>();
            modelBuilder.Entity<MultipleAnswersRepeatingInverse>();

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.MultipleInversePropertiesSameTargetWarning,
                    CoreResources.LogMultipleInversePropertiesSameTarget(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(
                            $"{nameof(MultipleAnswersRepeatingInverse)}.{nameof(MultipleAnswersRepeatingInverse.Answers)},"
                            + $" {nameof(MultipleAnswersInverse)}.{nameof(MultipleAnswersInverse.Answers)}",
                            nameof(PartialAnswerInverse.Answer)),
                    "CoreEventId.MultipleInversePropertiesSameTargetWarning"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        private class PartialAnswerInverse
        {
            public int Id { get; set; }
            public int AnswerId { get; set; }
            public virtual AnswerBaseInverse Answer { get; set; }
        }

        private class PartialAnswerRepeatingInverse : PartialAnswerInverse
        {
        }

        private abstract class AnswerBaseInverse
        {
            public int Id { get; set; }
        }

        private class MultipleAnswersInverse : AnswerBaseInverse
        {
            [InverseProperty("Answer")]
            public virtual ICollection<PartialAnswerInverse> Answers { get; set; }
        }

        private class MultipleAnswersRepeatingInverse : AnswerBaseInverse
        {
            [InverseProperty("Answer")]
            public virtual IEnumerable<PartialAnswerRepeatingInverse> Answers { get; set; }
        }

        [ConditionalFact]
        public virtual void InversePropertyAttribute_pointing_to_same_skip_nav_on_base_causes_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<AmbiguousInversePropertyLeft>();
            modelBuilder.Entity<AmbiguousInversePropertyLeftDerived>();

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.MultipleInversePropertiesSameTargetWarning,
                    CoreResources.LogMultipleInversePropertiesSameTarget(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(
                            $"{nameof(AmbiguousInversePropertyRightDerived)}.{nameof(AmbiguousInversePropertyRightDerived.DerivedLefts)},"
                            + $" {nameof(AmbiguousInversePropertyRight)}.{nameof(AmbiguousInversePropertyRight.BaseLefts)}",
                            nameof(AmbiguousInversePropertyLeft.BaseRights)),
                    "CoreEventId.MultipleInversePropertiesSameTargetWarning"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        protected class AmbiguousInversePropertyLeft
        {
            public int Id { get; set; }
            public List<AmbiguousInversePropertyRight> BaseRights { get; set; }
        }

        protected class AmbiguousInversePropertyLeftDerived : AmbiguousInversePropertyLeft
        {
            public List<AmbiguousInversePropertyRightDerived> DerivedRights { get; set; }
        }

        protected class AmbiguousInversePropertyRight
        {
            public int Id { get; set; }

            [InverseProperty("BaseRights")]
            public List<AmbiguousInversePropertyLeft> BaseLefts { get; set; }
        }

        protected class AmbiguousInversePropertyRightDerived : AmbiguousInversePropertyRight
        {
            [InverseProperty("BaseRights")]
            public List<AmbiguousInversePropertyLeftDerived> DerivedLefts { get; set; }
        }

        [ConditionalFact]
        public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_property_on_both_side()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<Author>();
            modelBuilder.Ignore<AuthorDetails>();
            modelBuilder.Entity<Post>().Property("PostDetailsId");

            Assert.Null(model.FindEntityType(typeof(Post)).FindNavigation("PostDetails").ForeignKey.PrincipalToDependent);
            Assert.Equal(
                "PostDetailsId", model.FindEntityType(typeof(Post)).FindNavigation("PostDetails").ForeignKey.Properties.First().Name);

            Assert.Null(model.FindEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
            Assert.Equal("PostId", model.FindEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.Properties.First().Name);

            var logEntry = Fixture.ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Warning, logEntry.Level);
            Assert.Equal(
                CoreResources.LogForeignKeyAttributesOnBothProperties(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Post.PostDetails), nameof(Post),
                    nameof(PostDetails.Post), nameof(PostDetails),
                    nameof(PostDetails.PostId), nameof(Post.PostDetailsId)),
                logEntry.Message);
        }

        [ConditionalFact]
        public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_navigations_on_both_sides_and_values_do_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<PostDetails>();
            modelBuilder.Ignore<AuthorDetails>();
            modelBuilder.Entity<Post>().Property("PostDetailsId");

            Assert.Null(model.FindEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.PrincipalToDependent);
            Assert.Equal("AuthorId", model.FindEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.Properties.First().Name);

            Assert.Null(model.FindEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
            Assert.Equal("PostId", model.FindEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.Properties.First().Name);

            var logEntry = Fixture.ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Warning, logEntry.Level);
            Assert.Equal(
                CoreResources.LogForeignKeyAttributesOnBothNavigations(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Post), nameof(Post.Author), nameof(Author), nameof(Author.Post)), logEntry.Message);
        }

        [ConditionalFact]
        public virtual void
            ForeignKeyAttribute_creates_two_relationships_if_applied_on_navigation_and_property_on_different_sides_and_values_do_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<PostDetails>();
            modelBuilder.Ignore<Post>();
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
            Assert.Equal(new[] { "Id", "AuthorDetailsIdByAttribute" }, author.GetProperties().Select(p => p.Name));

            var logEntry = Fixture.ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Warning, logEntry.Level);
            Assert.Equal(
                CoreResources.LogConflictingForeignKeyAttributesOnNavigationAndProperty(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(
                        nameof(Author), nameof(Author.AuthorDetails), nameof(AuthorDetails), nameof(AuthorDetails.AuthorId)),
                logEntry.Message);
        }

        protected class Post
        {
            public int Id { get; set; }

            [ForeignKey("PostDetails")]
            public int PostDetailsId;

            public PostDetails PostDetails { get; set; }

            [ForeignKey("AuthorId")]
            public Author Author { get; set; }
        }

        [ComplexType]
        protected class PostDetails
        {
            public int Id { get; set; }

            [ForeignKey("Post")]
            public int PostId { get; set; }

            public Post Post { get; set; }
        }

        protected class Author
        {
            public int Id { get; set; }

            [ForeignKey("PostId")]
            public Post Post { get; set; }

            [ForeignKey("AuthorDetailsIdByAttribute")]
            public AuthorDetails AuthorDetails { get; set; }
        }

        protected class AuthorDetails
        {
            public int Id { get; set; }

            [ForeignKey("Author")]
            public int AuthorId { get; set; }

            public Author Author { get; set; }
        }

        [ConditionalFact]
        public virtual void
            ForeignKeyAttribute_throws_if_applied_on_property_on_both_side_but_navigations_are_connected_by_inverse_property()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.InvalidRelationshipUsingDataAnnotations(nameof(A.B), nameof(A), nameof(B.A), nameof(B)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<A>()).Message);
        }

        [ConditionalFact]
        public virtual void
            ForeignKeyAttribute_throws_if_applied_on_both_navigations_connected_by_inverse_property_but_values_do_not_match()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.InvalidRelationshipUsingDataAnnotations("C", nameof(D), "D", nameof(C)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<D>()).Message);
        }

        [ConditionalFact]
        public virtual void ForeignKeyAttribute_throws_if_applied_on_two_relationships_targetting_the_same_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<B>();

            Assert.Equal(
                CoreStrings.ConflictingForeignKeyAttributes("{'AId'}", nameof(ConflictingFKAttributes), nameof(A)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<ConflictingFKAttributes>()).Message);
        }

        protected class A
        {
            public int Id { get; set; }

            [ForeignKey("B")]
            public int BId { get; set; }

            public B B { get; set; }
        }

        protected class B
        {
            public int Id { get; set; }

            [ForeignKey("A")]
            public int AId { get; set; }

            [InverseProperty("B")]
            public A A { get; set; }
        }

        protected class C
        {
            public int Id { get; set; }

            [ForeignKey("DId")]
            [InverseProperty("C")]
            public D D { get; set; }
        }

        protected class D
        {
            public int Id { get; set; }

            [ForeignKey("CId")]
            public C C { get; set; }
        }

        protected class ConflictingFKAttributes
        {
            public int Id { get; set; }

            [ForeignKey("A")]
            public int AId { get; set; }

            public A A { get; set; }

            [ForeignKey("AId")]
            public A As { get; set; }
        }

        [ConditionalFact]
        public virtual void Attribute_set_shadow_FK_name_is_preserved_with_HasPrincipalKey()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<User13694>(
                m =>
                {
                    m.Property("_email");

                    m.HasMany<Profile13694>("_profiles")
                        .WithOne("User")
                        .HasPrincipalKey("_email");
                });

            modelBuilder.Entity<Profile13694>().Property<string>("Email");

            var model = modelBuilder.FinalizeModel();

            var fk = model.FindEntityType(typeof(Profile13694)).GetForeignKeys().Single();
            Assert.Equal("_profiles", fk.PrincipalToDependent.Name);
            Assert.Equal("User", fk.DependentToPrincipal.Name);
            Assert.Equal("Email", fk.Properties[0].Name);
            Assert.Equal(typeof(string), fk.Properties[0].ClrType);
            Assert.Equal("_email", fk.PrincipalKey.Properties[0].Name);
        }

        protected class User13694
        {
            public Guid Id { get; set; }
            private readonly string _email = string.Empty;
            private readonly List<Profile13694> _profiles = new();
        }

        protected class Profile13694
        {
            public Guid Id { get; set; }

            [ForeignKey("Email")]
            public User13694 User { get; set; }
        }

        [ConditionalFact]
        public virtual void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<BookDetails>().Add(
                        new BookDetails { AnotherBookId = 1 });

                    context.SaveChanges();
                });

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<BookDetails>().Add(new BookDetails());

                    Assert.Equal(
                        "An error occurred while saving the entity changes. See the inner exception for details.",
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public virtual void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<One>().Add(
                        new One
                        {
                            RequiredColumn = "ValidString",
                            RowVersion = new Guid("00000000-0000-0000-0000-000000000001"),
                            Details = new Details { Name = "One" },
                            AdditionalDetails = new Details { Name = "Two" }
                        });

                    context.SaveChanges();
                });

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<One>().Add(
                        new One
                        {
                            RequiredColumn = null,
                            RowVersion = new Guid("00000000-0000-0000-0000-000000000002"),
                            Details = new Details { Name = "One" },
                            AdditionalDetails = new Details { Name = "Two" }
                        });

                    Assert.Equal(
                        "An error occurred while saving the entity changes. See the inner exception for details.",
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public virtual void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<Two>().Add(
                        new Two { Data = "ValidString" });

                    context.SaveChanges();
                });

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Set<Two>().Add(
                        new Two { Data = "ValidButLongString" });

                    Assert.Equal(
                        "An error occurred while saving the entity changes. See the inner exception for details.",
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public virtual void TimestampAttribute_throws_if_value_in_database_changed()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var clientRow = context.Set<Two>().First(r => r.Id == 1);
                    clientRow.Data = "ChangedData";

                    using var innerContext = CreateContext();
                    UseTransaction(innerContext.Database, context.Database.CurrentTransaction);
                    var storeRow = innerContext.Set<Two>().First(r => r.Id == 1);
                    storeRow.Data = "ModifiedData";

                    innerContext.SaveChanges();

                    Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
                });
        }

        [ConditionalFact]
        public virtual void UnicodeAttribute_sets_unicode_for_properties_and_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<UnicodeAnnotationClass>(b =>
            {
                b.Property(e => e.PersonMiddleName);
                b.Property(e => e.PersonAddress);
            });

            Validate(modelBuilder);

            Assert.True(GetProperty<UnicodeAnnotationClass>(modelBuilder, "PersonFirstName").IsUnicode());
            Assert.False(GetProperty<UnicodeAnnotationClass>(modelBuilder, "PersonLastName").IsUnicode());

            Assert.True(GetProperty<UnicodeAnnotationClass>(modelBuilder, "PersonMiddleName").IsUnicode());
            Assert.False(GetProperty<UnicodeAnnotationClass>(modelBuilder, "PersonAddress").IsUnicode());
        }

        protected class UnicodeAnnotationClass
        {
            public int Id { get; set; }

            [Unicode]
            public string PersonFirstName { get; set; }

            [Unicode(false)]
            public string PersonLastName { get; set; }

            [Unicode]
            public string PersonMiddleName;

            [Unicode(false)]
            public string PersonAddress;
        }

        [ConditionalFact]
        public virtual void PrecisionAttribute_sets_precision_for_properties_and_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<PrecisionAnnotationClass>(b =>
            {
                b.Property(e => e.DecimalField);
                b.Property(e => e.DateTimeField);
                b.Property(e => e.DateTimeOffsetField);
            });

            Validate(modelBuilder);

            Assert.Equal(10, GetProperty<PrecisionAnnotationClass>(modelBuilder, "DecimalProperty").GetPrecision());
            Assert.Equal(2, GetProperty<PrecisionAnnotationClass>(modelBuilder, "DecimalProperty").GetScale());
            Assert.Equal(5, GetProperty<PrecisionAnnotationClass>(modelBuilder, "DateTimeProperty").GetPrecision());
            Assert.Equal(5, GetProperty<PrecisionAnnotationClass>(modelBuilder, "DateTimeOffsetProperty").GetPrecision());

            Assert.Equal(10, GetProperty<PrecisionAnnotationClass>(modelBuilder, "DecimalField").GetPrecision());
            Assert.Equal(2, GetProperty<PrecisionAnnotationClass>(modelBuilder, "DecimalField").GetScale());
            Assert.Equal(5, GetProperty<PrecisionAnnotationClass>(modelBuilder, "DateTimeField").GetPrecision());
            Assert.Equal(5, GetProperty<PrecisionAnnotationClass>(modelBuilder, "DateTimeOffsetField").GetPrecision());
        }

        protected class PrecisionAnnotationClass
        {
            public int Id { get; set; }

            [Precision(10, 2)]
            public decimal DecimalProperty { get; set; }

            [Precision(5)]
            public DateTime DateTimeProperty { get; set; }

            [Precision(5)]
            public DateTimeOffset DateTimeOffsetProperty { get; set; }

            [Precision(10, 2)]
            public string DecimalField;

            [Precision(5)]
            public DateTime DateTimeField;

            [Precision(5)]
            public DateTimeOffset DateTimeOffsetField;
        }

        [ConditionalFact]
        public virtual void OwnedEntityTypeAttribute_configures_one_reference_as_owned()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Order>();

            Validate(modelBuilder);

            Assert.True(model.FindEntityType(typeof(Order)).FindNavigation(nameof(Order.ShippingAddress)).ForeignKey.IsOwnership);
        }

        [Owned]
        public class StreetAddress
        {
            public string Street { get; set; }
            public string City { get; set; }
            public int ZipCode { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public StreetAddress ShippingAddress { get; set; }
        }

        [ConditionalFact]
        public virtual void OwnedEntityTypeAttribute_configures_all_references_as_owned()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Label).WithOne(l => l.Book)
                .HasForeignKey<BookLabel>(l => l.BookId);
            modelBuilder.Entity<One>();
            modelBuilder.Ignore<SpecialBookLabel>();

            Validate(modelBuilder);

            Assert.True(model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AdditionalDetails)).ForeignKey.IsOwnership);
            var one = model.FindEntityType(typeof(One));
            var ownership1 = one.FindNavigation(nameof(One.Details)).ForeignKey;
            Assert.True(ownership1.IsOwnership);
            Assert.NotNull(ownership1.DeclaringEntityType.FindProperty(nameof(Details.Name)));
            var ownership2 = one.FindNavigation(nameof(One.AdditionalDetails)).ForeignKey;
            Assert.True(ownership2.IsOwnership);
            Assert.NotNull(ownership2.DeclaringEntityType.FindProperty(nameof(Details.Name)));
        }

        public abstract class DataAnnotationFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "DataAnnotations";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<One>();
                modelBuilder.Entity<Two>();
                modelBuilder.Ignore<BookLabel>();
                modelBuilder.Entity<BookDetails>();
                modelBuilder.Entity<Book>().Property(d => d.Id).ValueGeneratedNever();
                modelBuilder.Entity<KeylessAndKeyAttributes>();
                modelBuilder.Entity<KeylessFluentApiAndKeyAttribute>();
                modelBuilder.Entity<KeyFluentApiAndKeylessAttribute>();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(CoreEventId.ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning)
                        .Log(CoreEventId.ForeignKeyAttributesOnBothNavigationsWarning)
                        .Log(CoreEventId.ForeignKeyAttributesOnBothPropertiesWarning)
                        .Log(CoreEventId.ConflictingKeylessAndKeyAttributesWarning));

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Model.Name;

            protected override void Seed(PoolableDbContext context)
            {
                context.Set<One>().Add(
                    new One
                    {
                        RequiredColumn = "First",
                        RowVersion = new Guid("00000001-0000-0000-0000-000000000001"),
                        Details = new Details { Name = "First Name" },
                        AdditionalDetails = new Details { Name = "First Additional Name" }
                    });
                context.Set<One>().Add(
                    new One
                    {
                        RequiredColumn = "Second",
                        RowVersion = new Guid("00000001-0000-0000-0000-000000000001"),
                        Details = new Details { Name = "Second Name" },
                        AdditionalDetails = new Details { Name = "Second Additional Name" }
                    });

                context.Set<Two>().Add(
                    new Two { Data = "First" });
                context.Set<Two>().Add(
                    new Two { Data = "Second" });

                context.Set<Book>().Add(
                    new Book { Id = 1, AdditionalDetails = new Details { Name = "Book Name" } });

                context.SaveChanges();
            }
        }

        protected abstract class OneBase
        {
            public virtual int UniqueNo { get; set; }

            public virtual Guid RowVersion { get; set; }

            public virtual string IgnoredProperty { get; set; }

            public virtual string RequiredColumn { get; set; }

            public virtual string MaxLengthProperty { get; set; }
        }

        [Table("Sample")]
        protected class One : OneBase
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column("Unique_No")]
            public override int UniqueNo { get; set; }

            [ConcurrencyCheck]
            public override Guid RowVersion { get; set; }

            [NotMapped]
            public override string IgnoredProperty { get; set; }

            [Required]
            [Column("Name")]
            public override string RequiredColumn { get; set; }

            [MaxLength(10)]
            public override string MaxLengthProperty { get; set; }

            public Details Details { get; set; }

            public Details AdditionalDetails { get; set; }
        }

        protected class Two
        {
            [Key]
            public int Id { get; set; }

            [StringLength(16)]
            public string Data { get; set; }

            [Timestamp]
            public byte[] Timestamp { get; set; }
        }

        [Owned]
        protected class Details
        {
            public int Value { get; set; }
            public string Name { get; set; }
        }

        [Keyless]
        protected class KeylessAndKeyAttributes
        {
            [Key]
            public int NotAKey { get; set; }
        }

        protected class KeylessFluentApiAndKeyAttribute
        {
            [Key]
            public int NotAKey { get; set; }
        }

        [Keyless]
        protected class KeyFluentApiAndKeylessAttribute
        {
            public int MyKey { get; set; }
        }
    }
}
