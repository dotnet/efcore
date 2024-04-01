// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public abstract partial class ModelBuilderTest
{
    protected class BigMak
    {
        public int Id { get; set; }
        public int AlternateKey { get; set; }

        public IEnumerable<Pickle>? Pickles { get; set; }

        public Bun? Bun { get; set; }
    }

    protected class Ingredient
    {
        public static readonly PropertyInfo BurgerIdProperty = typeof(Ingredient).GetProperty("BurgerId")!;

        public int Id { get; set; }
        public int? BurgerId { get; set; }
        public BigMak? BigMak { get; set; }
    }

    protected class Pickle : Ingredient;

    protected class Bun : Ingredient;

    protected class SesameBun : Bun;

    protected class Whoopper
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
        public int AlternateKey1 { get; set; }
        public int AlternateKey2 { get; set; }

        public IEnumerable<Tomato>? Tomatoes { get; set; }

        public ToastedBun? ToastedBun { get; set; }

        public Mustard? Mustard { get; set; }
    }

    protected class Tomato
    {
        public int Id { get; set; }

        public int BurgerId1 { get; set; }
        public int BurgerId2 { get; set; }
        public Whoopper? Whoopper { get; set; }
    }

    protected class ToastedBun
    {
        public int Id { get; set; }

        public int BurgerId1 { get; set; }
        public int BurgerId2 { get; set; }
        public Whoopper? Whoopper { get; set; }
    }

    protected class Mustard
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }

        public Whoopper? Whoopper { get; set; }
    }

    protected class Customer
    {
        public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id")!;
        public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name")!;
        public static readonly PropertyInfo AlternateKeyProperty = typeof(Customer).GetProperty("AlternateKey")!;

        public int Id { get; set; }
        public Guid AlternateKey { get; set; }
        public string? Name { get; set; }

        public IEnumerable<Order>? Orders { get; set; }

        public List<string>? Notes { get; set; }

        [NotMapped]
        public ICollection<SpecialOrder>? SomeOrders { get; set; }

        public CustomerDetails? Details { get; set; }
    }

    [NotMapped]
    protected class SpecialCustomer : Customer
    {
        public ICollection<SpecialOrder>? SpecialOrders { get; set; }
    }

    protected class OtherCustomer : Customer;

    protected class DetailsBase
    {
        public int Id { get; set; }
    }

    protected class CustomerDetails : DetailsBase, INotifyPropertyChanged
    {
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged == null)
            {
                throw new NotImplementedException();
            }
        }
    }

    protected class Order : INotifyPropertyChanged
    {
        public static readonly PropertyInfo DetailsProperty = typeof(Order).GetProperty(nameof(Details))!;

        public int OrderId { get; set; }

        public int? CustomerId { get; set; }
        public Guid AnotherCustomerId { get; set; }
        public Customer? Customer { get; set; }
        public OrderCombination? OrderCombination { get; set; }
        public OrderDetails? Details { get; set; }
        public ICollection<Product>? Products { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged == null)
            {
                throw new NotImplementedException();
            }
        }
    }

    private class OrderProduct
    {
        public static readonly PropertyInfo OrderIdProperty = typeof(OrderProduct).GetProperty(nameof(OrderId))!;
        public static readonly PropertyInfo ProductIdProperty = typeof(OrderProduct).GetProperty(nameof(ProductId))!;

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public virtual required Order Order { get; set; } = null!;
        public virtual required Product Product { get; set; } = null!;
    }

    protected class UniProduct
    {
        public int Id { get; set; }
    }

    protected class UniCategory
    {
        public int Id { get; set; }
        public virtual ICollection<UniProduct>? Products { get; set; }
    }

    protected class NoProduct
    {
        public int Id { get; set; }
    }

    protected class NoCategory
    {
        public int Id { get; set; }
    }

    [NotMapped]
    protected class Product
    {
        public int Id { get; set; }

        [NotMapped]
        public Order? Order { get; set; }

        [NotMapped]
        public virtual ICollection<Order>? Orders { get; set; }

        public virtual ICollection<Category>? Categories { get; set; }
    }

    protected class ProductCategory
    {
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Category? Category { get; set; }
    }

    protected class CategoryBase
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    protected class Category : CategoryBase
    {
        public virtual ICollection<ProductCategory>? ProductCategories { get; set; }
        public virtual ICollection<Product>? Products { get; set; }
    }

    [Owned]
    protected class StreetAddress
    {
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
    }

    [NotMapped]
    protected class SpecialOrder : Order
    {
        public int SpecialOrderId { get; set; }
        public int? SpecialCustomerId { get; set; }
        public SpecialCustomer? SpecialCustomer { get; set; }
        public BackOrder? BackOrder { get; set; }
        public OrderCombination? SpecialOrderCombination { get; set; }
        public required StreetAddress ShippingAddress { get; set; }
    }

    protected class BackOrder : Order
    {
        public int SpecialOrderId { get; set; }
        public required SpecialOrder SpecialOrder { get; set; }
    }

    [NotMapped]
    protected class OrderCombination
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public required Order Order { get; set; }
        public int SpecialOrderId { get; set; }
        public required SpecialOrder SpecialOrder { get; set; }
        public required DetailsBase Details { get; set; }
    }

    protected class OrderDetails : DetailsBase
    {
        public static readonly PropertyInfo OrderIdProperty = typeof(OrderDetails).GetProperty("OrderId")!;

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }

    // INotify interfaces not really implemented; just marking the classes to test metadata construction
    protected class Quarks : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private int _forUp;
        private string? _forDown;
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169 // Remove unused private fields
        private int? _forWierd;
#pragma warning restore CS0169 // Remove unused private fields
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier

        public int Id { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public int Up
        {
            get => _forUp;
            set => _forUp = value;
        }

        // ReSharper disable once ConvertToAutoProperty
        public string? Down
        {
            get => _forDown;
            set => _forDown = value;
        }

#pragma warning disable 67
        public event PropertyChangingEventHandler? PropertyChanging;
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }

    // INotify interfaces not really implemented; just marking the classes to test metadata construction
    protected class CollectionQuarks : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private ObservableCollection<int> _forUp = null!;
        private ObservableCollection<string>? _forDown;
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169 // Remove unused private fields
        private ObservableCollection<string>? _forWierd;
#pragma warning restore CS0169 // Remove unused private fields
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier

        public int Id { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public ObservableCollection<int> Up
        {
            get => _forUp;
            set => _forUp = value;
        }

        // ReSharper disable once ConvertToAutoProperty
        public ObservableCollection<string>? Down
        {
            get => _forDown;
            set => _forDown = value;
        }

#pragma warning disable 67
        public event PropertyChangingEventHandler? PropertyChanging;
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }

    protected class DerivedCollectionQuarks : CollectionQuarks;

    protected class Hob
    {
        public string? Id1 { get; set; }
        public string? Id2 { get; set; }

        public int NobId1 { get; set; }
        public int NobId2 { get; set; }

        public Nob? Nob { get; set; }
        public ICollection<Nob>? Nobs { get; set; }
    }

    protected class Nob
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }

        public string? HobId1 { get; set; }
        public string? HobId2 { get; set; }

        public Hob? Hob { get; set; }
        public ICollection<Hob>? Hobs { get; set; }
    }

    protected class SelfRef
    {
        public int Id { get; set; }

        // TODO: Make both non-nullable when #25830 is fixed
        public SelfRef? SelfRef1 { get; set; }
        public SelfRef? SelfRef2 { get; set; }
        public int SelfRefId { get; set; }
    }

    protected class SelfRefManyToOne
    {
        public int Id { get; set; }
        public int SelfRef1Id { get; set; }
        public SelfRefManyToOne SelfRef1 { get; set; } = null!;
        public ICollection<SelfRefManyToOne> SelfRef2 { get; set; } = null!;

        [NotMapped]
        public ManyToManyRelated Related { get; set; } = null!;

        [NotMapped]
        public ICollection<ManyToManyRelated> Relateds { get; set; } = null!;
    }

    protected class ManyToManyRelated
    {
        public int Id { get; set; }
        public ICollection<SelfRefManyToOne>? DirectlyRelatedSelfRefs { get; set; }
        public ICollection<SelfRefManyToOne>? RelatedSelfRefs { get; set; }
    }

    protected class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        public required User CreatedBy { get; set; }

        public User? UpdatedBy { get; set; }

        public Guid CreatedById { get; set; }

        public Guid? UpdatedById { get; set; }
    }

    protected class SelfRefManyToOneDerived : SelfRefManyToOne;

    protected class Book
    {
        public static readonly PropertyInfo BookDetailsNavigation = typeof(Book).GetProperty("Details")!;

        public int Id { get; set; }

        public required BookLabel Label { get; set; }

        public required BookLabel AlternateLabel { get; set; }

        public required BookDetails Details { get; set; }
    }

    protected abstract class BookDetailsBase : DetailsBase
    {
        public int AnotherBookId { get; set; }

        public required Book AnotherBook { get; set; }
    }

    protected class BookDetails : BookDetailsBase
    {
        [NotMapped]
        public required Book Book { get; set; }
    }

    protected class BookLabel
    {
        public int Id { get; set; }

        [InverseProperty("Label")]
        public required Book Book { get; set; }

        public int BookId { get; set; }

        public SpecialBookLabel? SpecialBookLabel { get; set; }

        public AnotherBookLabel? AnotherBookLabel { get; set; }
    }

    protected class SpecialBookLabel : BookLabel
    {
        public BookLabel? BookLabel { get; set; }
    }

    protected class ExtraSpecialBookLabel : SpecialBookLabel;

    protected class AnotherBookLabel : BookLabel;

    private class EntityWithoutId
    {
        public string? Name { get; set; }
    }

    protected class PrincipalEntity
    {
        public int Id { get; set; }

        public List<DependentEntity>? InverseNav { get; set; }
    }

    protected class DependentEntity
    {
        public int Id { get; set; }

        [NotMapped]
        public int? PrincipalEntityId { get; set; }

        public required PrincipalEntity Nav { get; set; }
    }

    protected class Alpha
    {
        public int? Id { get; set; }
        public int AnotherId { get; set; }

        public Delta? NavDelta { get; set; }
        public IList<Epsilon>? Epsilons { get; set; }
        public IList<Eta>? Etas { get; set; }

        [ForeignKey("Id")]
        public IList<Theta>? Thetas { get; set; }

        [ForeignKey("Id")]
        public IList<Kappa>? Kappas { get; set; }
    }

    protected class Beta
    {
        private string? _name;
        public int Id { get; set; }

        public string? Name
        {
            get => "Beta: " + _name;
            set => _name = value;
        }

        public Alpha? FirstNav { get; set; }
        public Alpha? SecondNav { get; set; }
    }

    protected class Gamma
    {
        public int Id { get; set; }
        private int PrivateProperty { get; set; }
        private List<string> PrivateCollection { get; set; } = null!;

        public List<Alpha>? Alphas { get; set; }
    }

    protected class Delta
    {
        [ForeignKey("Alpha")]
        public int Id { get; set; }

        public Alpha? Alpha { get; set; }
    }

    protected class Epsilon
    {
        [ForeignKey("Alpha")]
        public int Id { get; set; }

        public Alpha? Alpha { get; set; }
    }

    protected class Eta
    {
        public int Id { get; set; }

        [ForeignKey("Id")]
        public Alpha? Alpha { get; set; }
    }

    protected class Zeta
    {
        public int Id { get; set; }

        public int CommonFkProperty { get; set; }

        [ForeignKey("CommonFkProperty")]
        public required Alpha AlphaOne { get; set; }

        [ForeignKey("CommonFkProperty")]
        public required Alpha AlphaTwo { get; set; }
    }

    [NotMapped]
    protected class Theta
    {
        public int ThetaId { get; set; }

        public Alpha? Alpha { get; set; }

        public Theta? NavTheta { get; set; }
        public IList<Theta>? InverseNavThetas { get; set; }
        public IList<Iota>? AllIotas { get; set; }
    }

    protected class Kappa
    {
        public int KappaId { get; set; }
        public int OmegaId { get; set; }

        public Alpha? Alpha { get; set; }
        public IList<Omega>? Omegas { get; set; }
    }

    protected class Iota
    {
        public int Id { get; set; }

        public Theta? FirstTheta { get; set; }
        public Theta? SecondTheta { get; set; }
    }

    protected class Omega
    {
        public int Id { get; set; }
        public int KappaId { get; set; }

        public Kappa? Kappa { get; set; }
    }

    protected class DynamicProperty
    {
        public int Id { get; set; }

        public ExpandoObject? ExpandoObject { get; set; }
    }

    protected interface IEntityBase
    {
        int Target { get; set; }
    }

    protected class EntityBase : IEntityBase
    {
        int IEntityBase.Target { get; set; }
    }

    protected class OneToOnePrincipalEntity
    {
        public static readonly PropertyInfo NavigationProperty =
            typeof(OneToOnePrincipalEntity).GetProperty("NavOneToOneDependentEntity")!;

        public static readonly PropertyInfo EntityMatchingProperty =
            typeof(OneToOnePrincipalEntity).GetProperty("OneToOneDependentEntityId")!;

        public static readonly PropertyInfo NavigationMatchingProperty =
            typeof(OneToOnePrincipalEntity).GetProperty("NavOneToOneDependentEntityId")!;

        public int Id { get; set; }

        public int NavOneToOneDependentEntityId { get; set; }
        public int OneToOneDependentEntityId { get; set; }

        [NotMapped]
        public required OneToOneDependentEntity NavOneToOneDependentEntity { get; set; }
    }

    protected class OneToOneDependentEntity
    {
        public static readonly PropertyInfo NavigationProperty =
            typeof(OneToOneDependentEntity).GetProperty("NavOneToOnePrincipalEntity")!;

        public static readonly PropertyInfo EntityMatchingProperty =
            typeof(OneToOneDependentEntity).GetProperty("OneToOnePrincipalEntityId")!;

        public static readonly PropertyInfo NavigationMatchingProperty =
            typeof(OneToOneDependentEntity).GetProperty("NavOneToOnePrincipalEntityId")!;

        public int Id { get; set; }

        public int NavOneToOnePrincipalEntityId { get; set; }
        public int OneToOnePrincipalEntityId { get; set; }

        [NotMapped]
        public required OneToOnePrincipalEntity NavOneToOnePrincipalEntity { get; set; }
    }

    protected class OneToOnePrincipalEntityWithAnnotation
    {
        public int Id { get; set; }

        public int NavOneToOneDependentEntityWithAnnotationId { get; set; }
        public int OneToOneDependentEntityWithAnnotationId { get; set; }
        public int FkProperty { get; set; }

        [NotMapped]
        [ForeignKey("FkProperty")]
        public required OneToOneDependentEntityWithAnnotation NavOneToOneDependentEntityWithAnnotation { get; set; }
    }

    protected class OneToOneDependentEntityWithAnnotation
    {
        public int Id { get; set; }

        public int NavOneToOnePrincipalEntityWithAnnotationId { get; set; }
        public int OneToOnePrincipalEntityWithAnnotationId { get; set; }

        [NotMapped]
        public required OneToOnePrincipalEntityWithAnnotation NavOneToOnePrincipalEntityWithAnnotation { get; set; }
    }

    protected class BaseTypeWithKeyAnnotation
    {
        [Key]
        public int MyPrimaryKey { get; set; }

        public int AnotherKey { get; set; }

        public int ForeignKeyProperty { get; set; }

        [ForeignKey("ForeignKeyProperty")]
        public required PrincipalTypeWithKeyAnnotation Navigation { get; set; }
    }

    protected class DerivedTypeWithKeyAnnotation : BaseTypeWithKeyAnnotation;

    protected class PrincipalTypeWithKeyAnnotation
    {
        public int Id { get; set; }

        [NotMapped]
        public BaseTypeWithKeyAnnotation? Navigation { get; set; }
    }

    protected class CityViewModel
    {
        public int Id { get; set; }

        public virtual required ICollection<CitizenViewModel> People { get; set; }

        public virtual required ICollection<PoliceViewModel> Police { get; set; }

        public virtual required ICollection<DoctorViewModel> Medics { get; set; }

        public Dictionary<string, string>? CustomValues { get; set; } = new();
    }

    protected abstract class PersonBaseViewModel
    {
        public int Id { get; set; }

        public int CityVMId { get; set; }

        public virtual required CityViewModel CityVM { get; set; }
    }

    protected class CitizenViewModel : PersonBaseViewModel;

    protected abstract class ServicePersonViewModel : PersonBaseViewModel;

    protected class DoctorViewModel : ServicePersonViewModel;

    protected class PoliceViewModel : ServicePersonViewModel;

    protected class StringIdBase
    {
        public string Id { get; set; } = "";
    }

    protected class StringIdDerived : StringIdBase;

    protected class Friendship
    {
        public int Id { get; set; }

        public int ActionUserId { get; set; }
        public required ApplicationUser ActionUser { get; set; }

        public int ApplicationUserId { get; set; }
        public required ApplicationUser ApplicationUser { get; set; }
    }

    protected class ApplicationUser
    {
        public int Id { get; set; }
        public required IList<Friendship> Friendships { get; set; }
    }

    public class EntityWithFields
    {
        public long Id;
        public int CompanyId;
        public int TenantId;
        public long[] CollectionId = null!;
        public int[] CollectionCompanyId = null!;
        public int[] CollectionTenantId = null!;
        public KeylessEntityWithFields KeylessEntity = new();
    }

    public class KeylessEntityWithFields
    {
        public string? FirstName;
        public string? LastName;
    }

    protected class QueryResult
    {
        public required CustomId Id { get; set; }
        public int ValueFk { get; set; }
        public required Value Value { get; set; }
    }

    [Owned]
    protected class Value
    {
        public int Id { get; set; }
        public CustomId? CategoryId { get; set; }
        public ValueCategory? Category { get; set; }
    }

    protected class CustomId
    {
        public int Id { get; set; }
    }

    protected class ValueCategory
    {
        public required CustomId Id { get; set; }
    }

    protected class KeylessEntity
    {
        public int CustomerId { get; set; }
        public required Customer Customer { get; set; }
    }

    protected class Parent
    {
        public int Id { get; set; }
        public required List<CompositeChild> Children { get; set; }
    }

    protected class CompositeChild
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public required Parent Parent { get; set; }
    }

    protected class BillingOwner
    {
        public int Id { get; set; }
        public BillingDetail? Bill1 { get; set; }
        public BillingDetail? Bill2 { get; set; }
    }

    protected class BillingDetail
    {
        public string? Country { get; set; }
    }

    protected class Country
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    protected class DependentShadowFk
    {
        public Guid DependentShadowFkId { get; set; }

        [ForeignKey("PrincipalShadowFkId")]
        public required PrincipalShadowFk Principal { get; set; }
    }

    protected class PrincipalShadowFk
    {
        public Guid PrincipalShadowFkId { get; set; }
        public List<DependentShadowFk>? Dependents { get; set; }
    }

    protected class BaseOwner
    {
        public int Id { get; set; }
        public required OwnedTypeInheritance1 Owned1 { get; set; }
        public required OwnedTypeInheritance2 Owned2 { get; set; }
    }

    protected class DerivedOwner : BaseOwner;

    [Owned]
    protected class OwnedTypeInheritance1
    {
        public string? Value { get; set; }
    }

    [Owned]
    protected class OwnedTypeInheritance2
    {
        public string? Value { get; set; }
    }

    protected interface IReplaceable
    {
        int Property { get; set; }
    }

    protected class ComplexPropertiesBase
    {
        public int Id { get; set; }
    }

    protected class ComplexProperties : ComplexPropertiesBase
    {
        public required Customer Customer { get; set; }
        public required DoubleProperty DoubleProperty { get; set; }
        public required IndexedClass IndexedClass { get; set; }
        public required Quarks Quarks { get; set; }
        public CollectionQuarks CollectionQuarks { get; set; } = null!;

        [NotMapped]
        public required DynamicProperty DynamicProperty { get; set; }

        [NotMapped]
        public required EntityWithFields EntityWithFields { get; set; }

        [NotMapped]
        public required WrappedStringEntity WrappedStringEntity { get; set; }
    }

    protected class ValueComplexProperties
    {
        public int Id { get; set; }
        public ProductLabel Label { get; set; }
        public ProductLabel OldLabel { get; set; }
        public (string, int) Tuple { get; set; }
    }

    protected struct ProductLabel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Customer Customer { get; set; }

        [NotMapped]
        public ValueComplexProperties Parent { get; set; }
    }

    protected interface IWrapped<T>
    {
        T? Value { get; init; }
    }

    protected abstract class WrappedStringBase : IWrapped<string>
    {
        public abstract string? Value { get; init; }
    }

    protected class WrappedString : WrappedStringBase
    {
        public override string? Value { get; init; }
    }

    protected class WrappedStringEntity
    {
        public int Id { get; set; }
        public WrappedString WrappedString { get; set; } = new();
    }

    protected class DoubleProperty : IReplaceable
    {
        public int Id { get; set; }
        public int Property { get; set; }

        int IReplaceable.Property
        {
            get => Property;
            set => Property = value;
        }
    }

    [ComplexType]
    protected class IndexedClass
    {
        private int _required;
        private string? _optional;

        public int Id { get; set; }

        public object? this[string name]
        {
            get
            {
                if (string.Equals(name, "Required", StringComparison.Ordinal))
                {
                    return _required;
                }

                if (string.Equals(name, "Optional", StringComparison.Ordinal))
                {
                    return _optional;
                }

                throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(IndexedClass)}.");
            }

            set
            {
                if (string.Equals(name, "Required", StringComparison.Ordinal))
                {
                    _required = (int)value!;
                }
                else if (string.Equals(name, "Optional", StringComparison.Ordinal))
                {
                    _optional = (string?)value;
                }
                else
                {
                    throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(IndexedClass)}.");
                }
            }
        }

        public NestedComplexType Nested { get; set; } = null!;
    }

    [ComplexType]
    protected class NestedComplexType
    {
        public int Foo { get; set; }
        public DoubleNestedComplexType DoubleNested { get; set; } = null!;
    }

    [ComplexType]
    protected class DoubleNestedComplexType
    {
        public int Foo { get; set; }
    }

    protected class IndexedClassByDictionary
    {
        private readonly Dictionary<string, object?> _indexerData = new();

        public int Id { get; set; }

        public object? this[string name]
        {
            get => _indexerData[name];
            set => _indexerData[name] = value;
        }
    }

    protected class OneToManyNavPrincipal
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public List<NavDependent>? Dependents { get; set; }
    }

    protected class OneToOneNavPrincipal
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public NavDependent? Dependent { get; set; }
    }

    protected class ManyToManyNavPrincipal
    {
        private readonly List<NavDependent> _randomField;

        public ManyToManyNavPrincipal()
        {
            _randomField = [];
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        [BackingField("_randomField")]
        public required List<NavDependent> Dependents { get; set; }
    }

    protected class NavDependent
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public OneToManyNavPrincipal? OneToManyPrincipal { get; set; }
        public OneToOneNavPrincipal? OneToOnePrincipal { get; set; }
        public List<ManyToManyNavPrincipal>? ManyToManyPrincipals { get; set; }
    }

    protected class NavDependentManyToManyNavPrincipalWithNavigationIds
    {
        public int DependentsId { get; set; }
        public int ManyToManyPrincipalsId { get; set; }
    }

    protected class NavDependentManyToManyNavPrincipalWithTypeIds
    {
        public int NavDependentId { get; set; }
        public int ManyToManyNavPrincipalId { get; set; }
    }

    protected class OneToManyNavPrincipalOwner
    {
        public int Id { get; set; }
        public string? Description { get; set; }

        public List<OwnedOneToManyNavDependent>? OwnedDependents { get; set; }
    }

    protected class OneToOneNavPrincipalOwner
    {
        public int Id { get; set; }
        public string? Description { get; set; }

        public OwnedNavDependent? OwnedDependent { get; set; }
    }

    protected class OwnedNavDependent
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public OneToOneNavPrincipalOwner? OneToOneOwner { get; set; }
    }

    protected class OwnedOneToManyNavDependent
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public OneToManyNavPrincipalOwner? OneToManyOwner { get; set; }
    }

    protected class OwnerOfOwnees
    {
        public string? Id { get; private set; }

        public Ownee2? AnOwnee2 { get; private set; }
        public Ownee1? Ownee1 { get; private set; }
    }

    protected class Ownee1
    {
        public Ownee3? NewOwnee3 { get; private set; }
    }

    protected class Ownee2
    {
        public Ownee3? Ownee3 { get; private set; }
    }

    protected class Ownee3
    {
        public string? Name { get; private set; }
    }

    protected class OneToManyPrincipalWithField
    {
        public int Id;
        public Guid AlternateKey;
        public string? Name;

        public IEnumerable<DependentWithField>? Dependents;
    }

    protected class OneToOnePrincipalWithField
    {
        public int Id;
        public string? Name;

        public DependentWithField? Dependent;
    }

    protected class ManyToManyPrincipalWithField
    {
        public int Id;
        public string? Name;

        public List<DependentWithField>? Dependents;
    }

    protected class ManyToManyJoinWithFields
    {
        public int ManyToManyPrincipalWithFieldId;
        public int DependentWithFieldId;

        public required ManyToManyPrincipalWithField ManyToManyPrincipalWithField { get; set; }
        public required DependentWithField DependentWithField { get; set; }
    }

    protected class DependentWithField
    {
        public int DependentWithFieldId;

        public int? OneToManyPrincipalId;
        public Guid AnotherOneToManyPrincipalId;
        public OneToManyPrincipalWithField? OneToManyPrincipal { get; set; }
        public int OneToOnePrincipalId;
        public required OneToOnePrincipalWithField OneToOnePrincipal { get; set; }
        public required List<ManyToManyPrincipalWithField> ManyToManyPrincipals { get; set; }
    }

    protected class OneToManyOwnerWithField
    {
        public int Id;
        public Guid AlternateKey;
        public string? Description;

        public List<OneToManyOwnedWithField>? OwnedDependents { get; set; }
    }

    protected class OneToManyOwnedWithField
    {
        public string? FirstName;
        public string? LastName;

        public int OneToManyOwnerId;
        public OneToManyOwnerWithField? OneToManyOwner { get; set; }
    }

    protected class OneToOneOwnerWithField
    {
        public int Id;
        public Guid AlternateKey;
        public string? Description;

        public OneToOneOwnedWithField? OwnedDependent { get; set; }
    }

    protected class OneToOneOwnedWithField
    {
        public string? FirstName;
        public string? LastName;

        public int OneToOneOwnerId;
        public OneToOneOwnerWithField? OneToOneOwner { get; set; }
    }

    protected class ImplicitManyToManyA
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public List<ImplicitManyToManyB>? Bs { get; set; }
    }

    protected class ImplicitManyToManyB
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public List<ImplicitManyToManyA>? As { get; set; }
    }

    protected class ReferenceNavigationToSharedType
    {
        public int Id { get; set; }
        public Dictionary<string, object>? Navigation { get; set; }
    }

    protected class CollectionNavigationToSharedType
    {
        public int Id { get; set; }
        public List<Dictionary<string, object>>? Navigation { get; set; }
    }

    protected class AmbiguousManyToManyImplicitLeft
    {
        public int Id { get; set; }
        public List<AmbiguousManyToManyImplicitRight> Navigation1 { get; } = [];
        public List<AmbiguousManyToManyImplicitRight> Navigation2 { get; } = [];
    }

    protected class AmbiguousManyToManyImplicitRight
    {
        public int Id { get; set; }
        public List<AmbiguousManyToManyImplicitLeft> Navigation1 { get; } = [];
        public List<AmbiguousManyToManyImplicitLeft> Navigation2 { get; } = [];
    }

    protected class AmbiguousInversePropertyLeft
    {
        public int Id { get; set; }
        public List<AmbiguousInversePropertyRight>? BaseRights { get; set; }
    }

    protected class AmbiguousInversePropertyLeftDerived : AmbiguousInversePropertyLeft
    {
        public List<AmbiguousInversePropertyRightDerived>? DerivedRights { get; set; }
    }

    protected class AmbiguousInversePropertyRight
    {
        public int Id { get; set; }

        [InverseProperty("BaseRights")]
        public List<AmbiguousInversePropertyLeft>? BaseLefts { get; set; }
    }

    protected class AmbiguousInversePropertyRightDerived : AmbiguousInversePropertyRight
    {
        [InverseProperty("BaseRights")]
        public List<AmbiguousInversePropertyLeftDerived>? DerivedLefts { get; set; }
    }

    protected class OwnerOfSharedType
    {
        public int Id { get; set; }
        public Dictionary<string, object>? Reference { get; set; }
        public List<Dictionary<string, object>>? Collection { get; set; }
        public NestedOwnerOfSharedType? OwnedNavigation { get; set; }
    }

    protected class NestedOwnerOfSharedType
    {
        public int Id { get; set; }
        public Dictionary<string, object>? Reference { get; set; }
        public List<Dictionary<string, object>>? Collection { get; set; }
    }

    protected class Dr
    {
        public int Id { get; set; }

        public Dre? Dre { get; set; }

        public ICollection<DreJr>? Jrs { get; set; }
    }

    protected class Dre;

    protected class DreJr : Dre;

    protected class Store
    {
        public int StoreId { get; set; }
    }

    protected class KeylessCollectionNavigation
    {
        public List<Store>? Stores { get; set; }

        [NotMapped]
        public KeylessReferenceNavigation? Reference { get; set; }
    }

    protected class KeylessReferenceNavigation
    {
        public List<KeylessCollectionNavigation>? Collection { get; set; }
    }

    protected class Discount
    {
        public int? StoreId { get; set; }
        public Store? Store { get; set; }
    }

    protected class MainOtter
    {
        public Guid Id { get; set; }
        public decimal Number { get; set; }
        public required OwnedOtter OwnedEntity { get; set; }
    }

    protected class OtherOtter
    {
        public Guid Id { get; set; }
        public decimal Number { get; set; }
        public List<OwnedOtter> OwnedEntities { get; } = [];
    }

    protected class OwnedOtter
    {
        public decimal Number { get; set; }
    }

    protected class OneDee
    {
        public int Id { get; set; }

        public int[]? One { get; set; }
    }

    protected class TwoDee
    {
        public int Id { get; set; }

        public int[,]? Two { get; set; }
    }

    protected class ThreeDee
    {
        public int Id { get; set; }

        public int[,,]? Three { get; set; }
    }
}
