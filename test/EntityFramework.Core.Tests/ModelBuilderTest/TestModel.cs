// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
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
            public readonly static PropertyInfo BurgerIdProperty = typeof(Ingredient).GetProperty("BurgerId");

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

        private class Customer
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

        private class SpecialCustomer : Customer
        {
            public ICollection<SpecialOrder> SpecialOrders { get; set; }
        }

        private class CustomerDetails
        {
            public int Id { get; set; }

            public Customer Customer { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int? CustomerId { get; set; }
            public Guid AnotherCustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class SpecialOrder : Order
        {
        }

        private class BackOrder : Order
        {
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class Quarks : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public int Id { get; set; }

            public int Up { get; set; }
            public string Down { get; set; }
            private int Charm { get; set; }
            private string Strange { get; set; }
            private int Top { get; set; }
            private string Bottom { get; set; }

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

        private class SelfRef
        {
            public int Id { get; set; }
            public SelfRef SelfRef1 { get; set; }
            public SelfRef SelfRef2 { get; set; }
            public int SelfRefId { get; set; }
        }

        private class Book
        {
            public int Id { get; set; }

            public BookLabel Label { get; set; }

            public BookLabel AlternateLabel { get; set; }

            public BookDetails Details { get; set; }
        }

        private class BookDetails
        {
            public int Id { get; set; }

            [NotMapped]
            public Book Book { get; set; }

            public Book AnotherBook { get; set; }
        }

        private class BookLabel
        {
            public int Id { get; set; }
            public int BookId { get; set; }

            [InverseProperty("Label")]
            public Book Book { get; set; }
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

            [ForeignKey("AuthorDetailsId")]
            public AuthorDetails AuthorDetails { get; set; }
        }

        private class AuthorDetails
        {
            public int Id { get; set; }

            [ForeignKey("Author")]
            public int AuthorId { get; set; }

            public Author Author { get; set; }
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

    }
}
