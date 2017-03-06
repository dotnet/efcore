// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Tests
{
    public abstract partial class ModelBuilderTest
    {
        private class BigMak
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }

            public IEnumerable<Pickle> Pickles { get; set; }

            public Bun Bun { get; set; }
        }

        private class Ingredient
        {
            public static readonly PropertyInfo BurgerIdProperty = typeof(Ingredient).GetProperty("BurgerId");

            public int Id { get; set; }
            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        private class Pickle : Ingredient
        {
        }

        private class Bun : Ingredient
        {
        }

        private class Whoopper
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public int AlternateKey1 { get; set; }
            public int AlternateKey2 { get; set; }

            public IEnumerable<Tomato> Tomatoes { get; set; }

            public ToastedBun ToastedBun { get; set; }

            public Moostard Moostard { get; set; }
        }

        private class Tomato
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class ToastedBun
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class Moostard
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public Whoopper Whoopper { get; set; }
        }

        protected class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");
            public static readonly PropertyInfo AlternateKeyProperty = typeof(Customer).GetProperty("AlternateKey");

            public int Id { get; set; }
            public Guid AlternateKey { get; set; }
            public string Name { get; set; }

            public IEnumerable<Order> Orders { get; set; }

            public CustomerDetails Details { get; set; }
        }

        [NotMapped]
        protected class SpecialCustomer : Customer
        {
            public ICollection<SpecialOrder> SpecialOrders { get; set; }
        }

        protected class OtherCustomer : Customer
        {
        }

        protected class CustomerDetails
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }

            public Customer Customer { get; set; }
        }

        protected class Order
        {
            public static readonly PropertyInfo DetailsProperty = typeof(Order).GetProperty(nameof(Details));

            public int OrderId { get; set; }

            public int? CustomerId { get; set; }
            public Guid AnotherCustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderCombination OrderCombination { get; set; }

            public OrderDetails Details { get; set; }
        }

        [NotMapped]
        protected class SpecialOrder : Order
        {
            public int? SpecialCustomerId { get; set; }
            public SpecialCustomer SpecialCustomer { get; set; }
            public BackOrder BackOrder { get; set; }
            public OrderCombination SpecialOrderCombination { get; set; }
        }

        protected class BackOrder : Order
        {
            public int SpecialOrderId { get; set; }
            public SpecialOrder SpecialOrder { get; set; }
        }

        [NotMapped]
        protected class OrderCombination
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public Order Order { get; set; }
            public int SpecialOrderId { get; set; }
            public SpecialOrder SpecialOrder { get; set; }
        }

        protected class OrderDetails
        {
            public static readonly PropertyInfo OrderIdProperty = typeof(OrderDetails).GetProperty("OrderId");

            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class Quarks : INotifyPropertyChanging, INotifyPropertyChanged
        {
            private int _forUp;
            private string _forDown;

            public int Id { get; set; }

            // ReSharper disable once ConvertToAutoProperty
            public int Up
            {
                get { return _forUp; }
                set { _forUp = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public string Down
            {
                get { return _forDown; }
                set { _forDown = value; }
            }

#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        private class Hob
        {
            public string Id1 { get; set; }
            public string Id2 { get; set; }

            public int NobId1 { get; set; }
            public int NobId2 { get; set; }

            public Nob Nob { get; set; }
            public ICollection<Nob> Nobs { get; set; }
        }

        private class Nob
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public string HobId1 { get; set; }
            public string HobId2 { get; set; }

            public Hob Hob { get; set; }
            public ICollection<Hob> Hobs { get; set; }
        }

        protected class SelfRef
        {
            public int Id { get; set; }
            public SelfRef SelfRef1 { get; set; }
            public SelfRef SelfRef2 { get; set; }
            public int SelfRefId { get; set; }
        }

        private class Book
        {
            public static readonly PropertyInfo BookdDetailsNavigation = typeof(Book).GetProperty("Details");

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

        private class EntityWithoutId
        {
            public string Name { get; set; }
        }

        protected class PrincipalEntity
        {
            public int Id { get; set; }

            public List<DependentEntity> InverseNav { get; set; }
        }

        protected class DependentEntity
        {
            public int Id { get; set; }

            [NotMapped]
            public int PrincipalEntityId { get; set; }

            public PrincipalEntity Nav { get; set; }
        }

        protected class Alpha
        {
            public int? Id { get; set; }
            public int AnotherId { get; set; }

            public Delta NavDelta { get; set; }
            public IList<Epsilon> Epsilons { get; set; }
            public IList<Eta> Etas { get; set; }

            [ForeignKey("Id")]
            public IList<Theta> Thetas { get; set; }

            [ForeignKey("Id")]
            public IList<Kappa> Kappas { get; set; }
        }

        protected class Beta
        {
            public int Id { get; set; }

            public Alpha FirstNav { get; set; }
            public Alpha SecondNav { get; set; }
        }

        protected class Gamma
        {
            public int Id { get; set; }

            public List<Alpha> Alphas { get; set; }
        }

        protected class Delta
        {
            [ForeignKey("Alpha")]
            public int Id { get; set; }

            public Alpha Alpha { get; set; }
        }

        protected class Epsilon
        {
            [ForeignKey("Alpha")]
            public int Id { get; set; }

            public Alpha Alpha { get; set; }
        }

        protected class Eta
        {
            public int Id { get; set; }

            [ForeignKey("Id")]
            public Alpha Alpha { get; set; }
        }

        protected class Zeta
        {
            public int Id { get; set; }

            public int CommonFkProperty { get; set; }

            [ForeignKey("CommonFkProperty")]
            public Alpha AlphaOne { get; set; }

            [ForeignKey("CommonFkProperty")]
            public Alpha AlphaTwo { get; set; }
        }

        protected class Theta
        {
            public int ThetaId { get; set; }

            public Alpha Alpha { get; set; }

            public Theta NavTheta { get; set; }
            public IList<Theta> InverseNavThetas { get; set; }
            public IList<Iota> AllIotas { get; set; }
        }

        protected class Kappa
        {
            public int KappaId { get; set; }
            public int OmegaId { get; set; }

            public Alpha Alpha { get; set; }
            public IList<Omega> Omegas { get; set; }
        }

        protected class Iota
        {
            public int Id { get; set; }

            public Theta FirstTheta { get; set; }
            public Theta SecondTheta { get; set; }
        }

        protected class Omega
        {
            public int Id { get; set; }
            public int KappaId { get; set; }

            public Kappa Kappa { get; set; }
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
            public static readonly PropertyInfo NavigationProperty = typeof(OneToOnePrincipalEntity).GetProperty("NavOneToOneDependentEntity");
            public static readonly PropertyInfo EntityMatchingProperty = typeof(OneToOnePrincipalEntity).GetProperty("OneToOneDependentEntityId");
            public static readonly PropertyInfo NavigationMatchingProperty = typeof(OneToOnePrincipalEntity).GetProperty("NavOneToOneDependentEntityId");

            public int Id { get; set; }

            public int NavOneToOneDependentEntityId { get; set; }
            public int OneToOneDependentEntityId { get; set; }

            [NotMapped]
            public OneToOneDependentEntity NavOneToOneDependentEntity { get; set; }
        }

        protected class OneToOneDependentEntity
        {
            public static readonly PropertyInfo NavigationProperty = typeof(OneToOneDependentEntity).GetProperty("NavOneToOnePrincipalEntity");
            public static readonly PropertyInfo EntityMatchingProperty = typeof(OneToOneDependentEntity).GetProperty("OneToOnePrincipalEntityId");
            public static readonly PropertyInfo NavigationMatchingProperty = typeof(OneToOneDependentEntity).GetProperty("NavOneToOnePrincipalEntityId");

            public int Id { get; set; }

            public int NavOneToOnePrincipalEntityId { get; set; }
            public int OneToOnePrincipalEntityId { get; set; }

            [NotMapped]
            public OneToOnePrincipalEntity NavOneToOnePrincipalEntity { get; set; }
        }

        protected class OneToOnePrincipalEntityWithAnnotation
        {
            public int Id { get; set; }

            public int NavOneToOneDependentEntityWithAnnotationId { get; set; }
            public int OneToOneDependentEntityWithAnnotationId { get; set; }
            public int FkProperty { get; set; }

            [NotMapped]
            [ForeignKey("FkProperty")]
            public OneToOneDependentEntityWithAnnotation NavOneToOneDependentEntityWithAnnotation { get; set; }
        }

        protected class OneToOneDependentEntityWithAnnotation
        {
            public int Id { get; set; }

            public int NavOneToOnePrincipalEntityWithAnnotationId { get; set; }
            public int OneToOnePrincipalEntityWithAnnotationId { get; set; }

            [NotMapped]
            public OneToOnePrincipalEntityWithAnnotation NavOneToOnePrincipalEntityWithAnnotation { get; set; }
        }

        protected class BaseTypeWithKeyAnnotation
        {
            [Key]
            public int MyPrimaryKey { get; set; }

            public int AnotherKey { get; set; }

            public int ForeignKeyProperty { get; set; }

            [ForeignKey("ForeignKeyProperty")]
            public PrincipalTypeWithKeyAnnotation Navigation { get; set; }
        }

        protected class DerivedTypeWithKeyAnnotation : BaseTypeWithKeyAnnotation
        {
        }

        protected class PrincipalTypeWithKeyAnnotation
        {
            public int Id { get; set; }
        }

        protected class CityViewModel
        {
            public int Id { get; set; }

            public virtual ICollection<CitizenViewModel> People { get; set; }

            public virtual ICollection<PoliceViewModel> Police { get; set; }

            public virtual ICollection<DoctorViewModel> Medics { get; set; }
        }

        protected abstract class PersonBaseViewModel
        {
            public int Id { get; set; }

            public int CityVMId { get; set; }

            public virtual CityViewModel CityVM { get; set; }
        }

        protected class CitizenViewModel : PersonBaseViewModel
        {
        }

        protected abstract class ServicePersonViewModel : PersonBaseViewModel
        {
        }

        protected class DoctorViewModel : ServicePersonViewModel
        {
        }

        protected class PoliceViewModel : ServicePersonViewModel
        {
        }
    }
}
