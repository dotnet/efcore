// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable MemberHidesStaticFromOuterClass
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public abstract class ModelValidatorTestBase
{
    protected virtual void SetBaseType(IMutableEntityType entityType, IMutableEntityType baseEntityType)
        => entityType.BaseType = baseEntityType;

    protected IMutableKey CreateKey(IMutableEntityType entityType, int startingPropertyIndex = -1, int propertyCount = 1)
    {
        if (startingPropertyIndex == -1)
        {
            startingPropertyIndex = entityType.GetProperties().Count() - 1;
        }

        var keyProperties = new IMutableProperty[propertyCount];
        for (var i = 0; i < propertyCount; i++)
        {
            var propertyName = "P" + (startingPropertyIndex + i);
            keyProperties[i] = entityType.FindProperty(propertyName)
                ?? entityType.AddProperty(propertyName, typeof(int?));
            keyProperties[i].IsNullable = false;
        }

        return entityType.AddKey(keyProperties);
    }

    public void AddProperties(IMutableEntityType entityTypeA)
    {
        entityTypeA.AddProperty(nameof(A.P0), typeof(int?));
        entityTypeA.AddProperty(nameof(A.P1), typeof(int?));
        entityTypeA.AddProperty(nameof(A.P2), typeof(int?));
        entityTypeA.AddProperty(nameof(A.P3), typeof(int?));
    }

    public void SetPrimaryKey(IMutableEntityType entityType)
        => entityType.SetPrimaryKey(entityType.AddProperty("Id", typeof(int)));

    protected IMutableForeignKey CreateForeignKey(IMutableKey dependentKey, IMutableKey principalKey)
        => CreateForeignKey(dependentKey.DeclaringEntityType, dependentKey.Properties, principalKey);

    protected IMutableForeignKey CreateForeignKey(
        IMutableEntityType dependEntityType,
        IReadOnlyList<IMutableProperty> dependentProperties,
        IMutableKey principalKey)
    {
        var foreignKey = dependEntityType.AddForeignKey(dependentProperties, principalKey, principalKey.DeclaringEntityType);
        foreignKey.IsUnique = true;

        return foreignKey;
    }

    protected class A
    {
        public int Id { get; set; }

        public int? P0 { get; set; }
        public int? P1 { get; set; }
        public int? P2 { get; set; }
        public int? P3 { get; set; }
    }

    protected class B
    {
        public int Id { get; set; }

        public int? P0 { get; set; }
        public int? P1 { get; set; }
        public int? P2 { get; set; }
        public int? P3 { get; set; }

        public A A { get; set; }

        [NotMapped]
        public A AnotherA { get; set; }

        [NotMapped]
        public ICollection<A> ManyAs { get; set; }
    }

    protected class C : A;

    protected class D : A;

    protected class F : D;

    protected class G
    {
        public int Id { get; set; }

        public int? P0 { get; set; }
        public int? P1 { get; set; }
        public int? P2 { get; set; }
        public int? P3 { get; set; }

        public A A { get; set; }
    }

    protected abstract class Abstract : A;

    // ReSharper disable once UnusedTypeParameter
    protected class Generic<T> : Abstract;

#nullable enable
    protected class BaseEntity
    {
        public int Id { get; set; }
    }

    protected class ChildA : BaseEntity
    {
        public OwnedType OwnedType { get; set; } = null!;
    }

    protected class ChildB : BaseEntity
    {
    }

    protected class ChildC : BaseEntity
    {
    }

    protected class ChildD : BaseEntity
    {
    }

    [Owned]
    protected class OwnedType
    {
        public NestedOwnedType NestedOwnedType { get; set; } = null!;
    }

    [Owned]
    protected class NestedOwnedType
    {
    }

#nullable restore

    protected class SampleEntity
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public ReferencedEntity ReferencedEntity { get; set; }

        [NotMapped]
        public ReferencedEntity AnotherReferencedEntity { get; set; }

        public ICollection<SampleEntity> OtherSamples { get; set; }
    }

    protected class AnotherSampleEntity
    {
        public int Id { get; set; }
        public ReferencedEntity ReferencedEntity { get; set; }
    }

    protected class ReferencedEntity
    {
        public int Id { get; set; }
        public int SampleEntityId { get; set; }
    }

    protected class SampleEntityMinimal
    {
        public int Id { get; set; }
        public ReferencedEntityMinimal ReferencedEntity { get; set; }
    }

    protected class ReferencedEntityMinimal;

    protected class AnotherSampleEntityMinimal
    {
        public int Id { get; set; }
        public ReferencedEntityMinimal ReferencedEntity { get; set; }
    }

    protected class E
    {
        public int Id { get; set; }
        public bool ImBool { get; set; }
        public bool ImNotUsed { get; set; }
        public bool? ImNot { get; set; }
    }

    protected class E2
    {
        private bool? _imBool;

        public int Id { get; set; }

        public bool ImBool
        {
            get => _imBool ?? true;
            set => _imBool = value;
        }
    }

    protected enum X
    {
        A = 1,
        B
    }

    protected class WithEnum
    {
        public int Id { get; set; }
        public X EnumWithDefaultConstraint { get; set; }
        public X EnumNoDefaultConstraint { get; set; }
        public X? NullableEnum { get; set; }
    }

    protected class WithEnum2
    {
        private X? _enumWithDefaultConstraint;

        public int Id { get; set; }

        public X EnumWithDefaultConstraint
        {
            get => _enumWithDefaultConstraint ?? X.B;
            set => _enumWithDefaultConstraint = value;
        }
    }

    protected class EntityWithInvalidProperties
    {
        public int Id { get; set; }

        public bool NotImplemented
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public static int Static { get; set; }

        public int WriteOnly
        {
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public int ReadOnly { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int PrivateGetter { private get; set; }

        public int this[int index]
        {
            get => 0;
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }
    }

    protected class Customer
    {
        private string _name;
        public string OtherName;

        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string PartitionId { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    protected class Order
    {
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty(nameof(Id));

        public int Id { get; set; }
        public string PartitionId { get; set; }
        public Customer Customer { get; set; }

        public OrderDetails OrderDetails { get; set; }

        [NotMapped]
        public virtual ICollection<Product> Products { get; set; }
    }

    [Owned]
    protected class OrderDetails
    {
        public Customer Customer { get; set; }
        public string ShippingAddress { get; set; }
    }

    protected class OrderProduct
    {
        public static readonly PropertyInfo OrderIdProperty = typeof(OrderProduct).GetProperty(nameof(OrderId));
        public static readonly PropertyInfo ProductIdProperty = typeof(OrderProduct).GetProperty(nameof(ProductId));

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }

    protected class Product
    {
        public static readonly PropertyInfo IdProperty = typeof(Product).GetProperty(nameof(Id));

        public int Id { get; set; }

        [NotMapped]
        public virtual ICollection<Order> Orders { get; set; }
    }

    protected class KeylessSeed
    {
        public string Species { get; set; }
    }

    protected class PrincipalOne
    {
        public int Id { get; set; }

        public ICollection<DependentOne> DependentsOnes { get; set; }
    }

    protected class DependentOne
    {
        public static readonly PropertyInfo PrincipalOneIdProperty = typeof(DependentOne).GetProperty(nameof(PrincipalOneId));

        public int Id { get; set; }

        public string PrincipalOneId { get; set; }
        public PrincipalOne PrincipalOne { get; set; }
    }

    protected class PrincipalTwo
    {
        public int Id { get; set; }

        public ICollection<DependentTwo> DependentsTwos { get; set; }
    }

    protected class DependentTwo
    {
        public int Id { get; set; }

        public int? PrincipalTwoId { get; set; }
        public PrincipalTwo PrincipalTwo { get; set; }
    }

    protected class PrincipalThree
    {
        public int Id { get; set; }

        public ICollection<DependentThree> DependentsThreesA { get; set; }
        public ICollection<DependentThree> DependentsThreesB { get; set; }
    }

    protected class DependentThree
    {
        public static readonly PropertyInfo PrincipalThreeIdProperty = typeof(DependentThree).GetProperty(nameof(PrincipalThreeId));

        public int Id { get; set; }

        public int? PrincipalThreeId { get; set; }
        public PrincipalThree PrincipalThreeA { get; set; }
        public PrincipalThree PrincipalThreeB { get; set; }
    }

    protected class PrincipalFour
    {
        public int Id { get; set; }

        public ICollection<DependentFour> DependentsFours { get; set; }
    }

    protected class DependentFour
    {
        public static readonly PropertyInfo PrincipalFourIdProperty = typeof(DependentFour).GetProperty(nameof(PrincipalFourId));
        public static readonly PropertyInfo PrincipalFourId1Property = typeof(DependentFour).GetProperty(nameof(PrincipalFourId1));

        public int Id { get; set; }

        public string PrincipalFourId1 { get; set; }
        public string PrincipalFourId { get; set; }
        public PrincipalFour PrincipalFour { get; set; }
    }

    protected class Blog
    {
        public int BlogId { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<PicturePost> PicturePosts { get; set; }
        public List<BlogOwnedEntity> BlogOwnedEntities { get; set; }
    }

    protected class BlogOwnedEntity
    {
        public int BlogOwnedEntityId { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    protected class Post
    {
        public int PostId { get; set; }
        public int BlogId { get; set; }
        public string Content { get; set; }
        public bool IsDeleted { get; set; }
        public Blog Blog { get; set; }
    }

    protected class PicturePost : Post
    {
        public string PictureUrl { get; set; }
        public List<Picture> Pictures { get; set; }
    }

    protected class Picture
    {
        public int PictureId { get; set; }
        public bool IsDeleted { get; set; }
        public int PicturePostId { get; set; }
        public PicturePost PicturePost { get; set; }
    }

    protected ModelValidatorTestBase()
    {
        LoggerFactory = new ListLoggerFactory(l => l == DbLoggerCategory.Model.Validation.Name || l == DbLoggerCategory.Model.Name);
    }

    protected ListLoggerFactory LoggerFactory { get; }

    protected virtual void VerifyWarning(
        string expectedMessage,
        TestHelpers.TestModelBuilder modelBuilder,
        LogLevel level = LogLevel.Warning)
    {
        Validate(modelBuilder);

        var logEntry = LoggerFactory.Log.Single(l => l.Level == level);
        Assert.Equal(expectedMessage, logEntry.Message);
    }

    protected virtual void VerifyWarnings(
        string[] expectedMessages,
        TestHelpers.TestModelBuilder modelBuilder,
        LogLevel level = LogLevel.Warning)
    {
        Validate(modelBuilder);
        var logEntries = LoggerFactory.Log.Where(l => l.Level == level);
        Assert.Equal(expectedMessages.Length, logEntries.Count());

        var count = 0;
        foreach (var logEntry in logEntries)
        {
            Assert.Equal(expectedMessages[count++], logEntry.Message);
        }
    }

    protected virtual void VerifyError(
        string expectedMessage,
        TestHelpers.TestModelBuilder modelBuilder,
        bool sensitiveDataLoggingEnabled = false)
    {
        var message = Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder, sensitiveDataLoggingEnabled)).Message;
        Assert.Equal(expectedMessage, message);
    }

    protected virtual void VerifyLogDoesNotContain(string expectedMessage, TestHelpers.TestModelBuilder modelBuilder)
    {
        Validate(modelBuilder);

        var logEntries = LoggerFactory.Log.Where(l => l.Message.Contains(expectedMessage));

        Assert.Empty(logEntries);
    }

    protected virtual IModel Validate(TestHelpers.TestModelBuilder modelBuilder, bool sensitiveDataLoggingEnabled = false)
        => modelBuilder.FinalizeModel(designTime: true);

    protected DiagnosticsLogger<DbLoggerCategory.Model.Validation> CreateValidationLogger(bool sensitiveDataLoggingEnabled = false)
    {
        var options = new LoggingOptions();
        options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled).Options);
        return new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
            LoggerFactory,
            options,
            new DiagnosticListener("Fake"),
            TestHelpers.LoggingDefinitions,
            new NullDbContextLogger());
    }

    protected DiagnosticsLogger<DbLoggerCategory.Model> CreateModelLogger(bool sensitiveDataLoggingEnabled = false)
    {
        var options = new LoggingOptions();
        options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled).Options);
        return new DiagnosticsLogger<DbLoggerCategory.Model>(
            LoggerFactory,
            options,
            new DiagnosticListener("Fake"),
            TestHelpers.LoggingDefinitions,
            new NullDbContextLogger());
    }

    protected virtual TestHelpers.TestModelBuilder CreateConventionModelBuilder(
        Action<ModelConfigurationBuilder> configure = null,
        bool sensitiveDataLoggingEnabled = false)
        => TestHelpers.CreateConventionBuilder(
            CreateModelLogger(sensitiveDataLoggingEnabled), CreateValidationLogger(sensitiveDataLoggingEnabled),
            configurationBuilder => configure?.Invoke(configurationBuilder));

    protected virtual TestHelpers.TestModelBuilder CreateConventionlessModelBuilder(
        Action<ModelConfigurationBuilder> configure = null,
        bool sensitiveDataLoggingEnabled = false)
        => TestHelpers.CreateConventionBuilder(
            CreateModelLogger(sensitiveDataLoggingEnabled), CreateValidationLogger(sensitiveDataLoggingEnabled),
            configurationBuilder =>
            {
                configure?.Invoke(configurationBuilder);
                configurationBuilder.RemoveAllConventions();
            });

    protected virtual TestHelpers TestHelpers
        => InMemoryTestHelpers.Instance;

    protected virtual InternalModelBuilder CreateConventionlessInternalModelBuilder()
        => (InternalModelBuilder)CreateConventionlessModelBuilder().GetInfrastructure();
}
