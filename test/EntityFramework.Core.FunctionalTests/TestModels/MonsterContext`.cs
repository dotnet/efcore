// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels
{
    public class MonsterContext<
        TCustomer, TBarcode, TIncorrectScan, TBarcodeDetail, TComplaint, TResolution, TLogin, TSuspiciousActivity,
        TSmartCard, TRsaToken, TPasswordReset, TPageView, TLastLogin, TMessage, TAnOrder, TOrderNote, TOrderQualityCheck,
        TOrderLine, TProduct, TProductDetail, TProductReview, TProductPhoto, TProductWebFeature, TSupplier, TSupplierLogo,
        TSupplierInfo, TCustomerInfo, TComputer, TComputerDetail, TDriver, TLicense> : MonsterContext
        where TCustomer : class, ICustomer, new()
        where TBarcode : class, IBarcode, new()
        where TIncorrectScan : class, IIncorrectScan, new()
        where TBarcodeDetail : class, IBarcodeDetail, new()
        where TComplaint : class, IComplaint, new()
        where TResolution : class, IResolution, new()
        where TLogin : class, ILogin, new()
        where TSuspiciousActivity : class, ISuspiciousActivity, new()
        where TSmartCard : class, ISmartCard, new()
        where TRsaToken : class, IRsaToken, new()
        where TPasswordReset : class, IPasswordReset, new()
        where TPageView : class, IPageView, new()
        where TLastLogin : class, ILastLogin, new()
        where TMessage : class, IMessage, new()
        where TAnOrder : class, IAnOrder, new()
        where TOrderNote : class, IOrderNote, new()
        where TOrderQualityCheck : class, IOrderQualityCheck, new()
        where TOrderLine : class, IOrderLine, new()
        where TProduct : class, IProduct, new()
        where TProductDetail : class, IProductDetail, new()
        where TProductReview : class, IProductReview, new()
        where TProductPhoto : class, IProductPhoto, new()
        where TProductWebFeature : class, IProductWebFeature, new()
        where TSupplier : class, ISupplier, new()
        where TSupplierLogo : class, ISupplierLogo, new()
        where TSupplierInfo : class, ISupplierInfo, new()
        where TCustomerInfo : class, ICustomerInfo, new()
        where TComputer : class, IComputer, new()
        where TComputerDetail : class, IComputerDetail, new()
        where TDriver : class, IDriver, new()
        where TLicense : class, ILicense, new()
    {
        private readonly Action<ModelBuilder> _onModelCreating;

        public MonsterContext(IServiceProvider serviceProvider, DbContextOptions options, Action<ModelBuilder> onModelCreating)
            : base(serviceProvider, options)
        {
            _onModelCreating = onModelCreating;
        }

        public override IQueryable<ICustomer> Customers
        {
            get { return Set<TCustomer>(); }
        }

        public override IQueryable<IBarcode> Barcodes
        {
            get { return Set<TBarcode>(); }
        }

        public override IQueryable<IIncorrectScan> IncorrectScans
        {
            get { return Set<TIncorrectScan>(); }
        }

        public override IQueryable<IBarcodeDetail> BarcodeDetails
        {
            get { return Set<TBarcodeDetail>(); }
        }

        public override IQueryable<IComplaint> Complaints
        {
            get { return Set<TComplaint>(); }
        }

        public override IQueryable<IResolution> Resolutions
        {
            get { return Set<TResolution>(); }
        }

        public override IQueryable<ILogin> Logins
        {
            get { return Set<TLogin>(); }
        }

        public override IQueryable<ISuspiciousActivity> SuspiciousActivities
        {
            get { return Set<TSuspiciousActivity>(); }
        }

        public override IQueryable<ISmartCard> SmartCards
        {
            get { return Set<TSmartCard>(); }
        }

        public override IQueryable<IRsaToken> RsaTokens
        {
            get { return Set<TRsaToken>(); }
        }

        public override IQueryable<IPasswordReset> PasswordResets
        {
            get { return Set<TPasswordReset>(); }
        }

        public override IQueryable<IPageView> PageViews
        {
            get { return Set<TPageView>(); }
        }

        public override IQueryable<ILastLogin> LastLogins
        {
            get { return Set<TLastLogin>(); }
        }

        public override IQueryable<IMessage> Messages
        {
            get { return Set<TMessage>(); }
        }

        public override IQueryable<IAnOrder> Orders
        {
            get { return Set<TAnOrder>(); }
        }

        public override IQueryable<IOrderNote> OrderNotes
        {
            get { return Set<TOrderNote>(); }
        }

        public override IQueryable<IOrderQualityCheck> OrderQualityChecks
        {
            get { return Set<TOrderQualityCheck>(); }
        }

        public override IQueryable<IOrderLine> OrderLines
        {
            get { return Set<TOrderLine>(); }
        }

        public override IQueryable<IProduct> Products
        {
            get { return Set<TProduct>(); }
        }

        public override IQueryable<IProductDetail> ProductDetails
        {
            get { return Set<TProductDetail>(); }
        }

        public override IQueryable<IProductReview> ProductReviews
        {
            get { return Set<TProductReview>(); }
        }

        public override IQueryable<IProductPhoto> ProductPhotos
        {
            get { return Set<TProductPhoto>(); }
        }

        public override IQueryable<IProductWebFeature> ProductWebFeatures
        {
            get { return Set<TProductWebFeature>(); }
        }

        public override IQueryable<ISupplier> Suppliers
        {
            get { return Set<TSupplier>(); }
        }

        public override IQueryable<ISupplierLogo> SupplierLogos
        {
            get { return Set<TSupplierLogo>(); }
        }

        public override IQueryable<ISupplierInfo> SupplierInformation
        {
            get { return Set<TSupplierInfo>(); }
        }

        public override IQueryable<ICustomerInfo> CustomerInformation
        {
            get { return Set<TCustomerInfo>(); }
        }

        public override IQueryable<IComputer> Computers
        {
            get { return Set<TComputer>(); }
        }

        public override IQueryable<IComputerDetail> ComputerDetails
        {
            get { return Set<TComputerDetail>(); }
        }

        public override IQueryable<IDriver> Drivers
        {
            get { return Set<TDriver>(); }
        }

        public override IQueryable<ILicense> Licenses
        {
            get { return Set<TLicense>(); }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Complex types
            modelBuilder.Ignore<AuditInfo>();
            modelBuilder.Ignore<ConcurrencyInfo>();
            modelBuilder.Ignore<ContactDetails>();
            modelBuilder.Ignore<Dimensions>();

            modelBuilder.Entity<TBarcodeDetail>().Key(e => e.Code);

            modelBuilder.Entity<TSuspiciousActivity>();
            modelBuilder.Entity<TLastLogin>().Key(e => e.Username);
            modelBuilder.Entity<TMessage>().Key(e => new { e.MessageId, e.FromUsername });

            modelBuilder.Entity<TOrderNote>().Key(e => e.NoteId);

            modelBuilder.Entity<TProductDetail>().Key(e => e.ProductId);

            modelBuilder.Entity<TProductWebFeature>().Key(e => e.FeatureId);

            modelBuilder.Entity<TSupplierLogo>().Key(e => e.SupplierId);

            modelBuilder.Entity<TLicense>().Key(e => e.Name);

            modelBuilder.Entity<TAnOrder>(b =>
                {
                    b.Collection(e => (IEnumerable<TOrderLine>)e.OrderLines).InverseReference(e => (TAnOrder)e.Order)
                        .ForeignKey(e => e.OrderId);

                    b.Collection(e => (IEnumerable<TOrderNote>)e.Notes).InverseReference(e => (TAnOrder)e.Order)
                        .PrincipalKey(e => e.AlternateId);
                });

            modelBuilder.Entity<TOrderQualityCheck>(b =>
                {
                    b.Key(e => e.OrderId);

                    b.Reference(e => (TAnOrder)e.Order).InverseReference()
                        .ForeignKey<TOrderQualityCheck>(e => e.OrderId)
                        .PrincipalKey<TAnOrder>(e => e.AlternateId);
                });

            modelBuilder.Entity<TProduct>(b =>
                {
                    b.Collection(e => (IEnumerable<TProductReview>)e.Reviews).InverseReference(e => (TProduct)e.Product);
                    b.Collection(e => (IEnumerable<TBarcode>)e.Barcodes).InverseReference(e => (TProduct)e.Product);
                    b.Collection(e => (IEnumerable<TProductPhoto>)e.Photos).InverseReference();
                    b.Reference(e => (TProductDetail)e.Detail).InverseReference(e => (TProduct)e.Product)
                        .ForeignKey<TProductDetail>(e => e.ProductId);
                });

            modelBuilder.Entity<TOrderLine>(b =>
                {
                    b.Key(e => new { e.OrderId, e.ProductId });

                    b.Reference(e => (TProduct)e.Product).InverseCollection().ForeignKey(e => e.ProductId);
                });

            modelBuilder.Entity<TSupplier>().Reference(e => (TSupplierLogo)e.Logo).InverseReference().ForeignKey<TSupplierLogo>(e => e.SupplierId);

            modelBuilder.Entity<TCustomer>(b =>
                {
                    b.Collection(e => (IEnumerable<TAnOrder>)e.Orders).InverseReference(e => (TCustomer)e.Customer);
                    b.Collection(e => (IEnumerable<TLogin>)e.Logins).InverseReference(e => (TCustomer)e.Customer);
                    b.Reference(e => (TCustomerInfo)e.Info).InverseReference().ForeignKey<TCustomerInfo>(e => e.CustomerInfoId);

                    b.Reference(e => (TCustomer)e.Husband).InverseReference(e => (TCustomer)e.Wife)
                        .ForeignKey<TCustomer>(e => e.HusbandId);
                });

            modelBuilder.Entity<TComplaint>(b =>
                {
                    b.Reference(e => (TCustomer)e.Customer)
                        .InverseCollection()
                        .ForeignKey(e => e.CustomerId);

                    b.Reference(e => (TResolution)e.Resolution).InverseReference(e => (TComplaint)e.Complaint)
                        .PrincipalKey<TComplaint>(e => e.AlternateId);
                });

            modelBuilder.Entity<TProductPhoto>(b =>
                {
                    b.Key(e => new { e.PhotoId, e.ProductId });

                    b.Collection(e => (IEnumerable<TProductWebFeature>)e.Features).InverseReference(e => (TProductPhoto)e.Photo)
                        .ForeignKey(e => new { e.PhotoId, e.ProductId })
                        .PrincipalKey(e => new { e.PhotoId, e.ProductId });
                });

            modelBuilder.Entity<TProductReview>(b =>
                {
                    b.Key(e => new { e.ReviewId, e.ProductId });

                    b.Collection(e => (IEnumerable<TProductWebFeature>)e.Features).InverseReference(e => (TProductReview)e.Review)
                        .ForeignKey(e => new { e.ReviewId, e.ProductId });
                });

            modelBuilder.Entity<TLogin>(b =>
                {
                    var key = b.Key(e => e.Username);

                    b.Collection(e => (IEnumerable<TMessage>)e.SentMessages).InverseReference(e => (TLogin)e.Sender)
                        .ForeignKey(e => e.FromUsername);

                    b.Collection(e => (IEnumerable<TMessage>)e.ReceivedMessages).InverseReference(e => (TLogin)e.Recipient)
                        .ForeignKey(e => e.ToUsername);

                    b.Collection(e => (IEnumerable<TAnOrder>)e.Orders).InverseReference(e => (TLogin)e.Login)
                        .ForeignKey(e => e.Username);

                    var entityType = b.Metadata;
                    var activityEntityType = entityType.Model.GetEntityType(typeof(TSuspiciousActivity));
                    activityEntityType.AddForeignKey(activityEntityType.GetProperty("Username"), key.Metadata);

                    b.Reference(e => (TLastLogin)e.LastLogin).InverseReference(e => (TLogin)e.Login)
                        .ForeignKey<TLastLogin>(e => e.Username);
                });

            modelBuilder.Entity<TPasswordReset>(b =>
                {
                    b.Key(e => new { e.ResetNo, e.Username });

                    b.Reference(e => (TLogin)e.Login).InverseCollection()
                        .ForeignKey(e => e.Username)
                        .PrincipalKey(e => e.AlternateUsername);
                });

            modelBuilder.Entity<TPageView>().Reference(e => (TLogin)e.Login).InverseCollection()
                .ForeignKey(e => e.Username);

            modelBuilder.Entity<TBarcode>(b =>
                {
                    b.Key(e => e.Code);

                    b.Collection(e => (IEnumerable<TIncorrectScan>)e.BadScans).InverseReference(e => (TBarcode)e.ExpectedBarcode)
                        .ForeignKey(e => e.ExpectedCode);

                    b.Reference(e => (TBarcodeDetail)e.Detail).InverseReference()
                        .ForeignKey<TBarcodeDetail>(e => e.Code);
                });

            modelBuilder.Entity<TIncorrectScan>().Reference(e => (TBarcode)e.ActualBarcode).InverseCollection()
                .ForeignKey(e => e.ActualCode);

            modelBuilder.Entity<TSupplierInfo>().Reference(e => (TSupplier)e.Supplier).InverseCollection();

            modelBuilder.Entity<TComputer>().Reference(e => (TComputerDetail)e.ComputerDetail).InverseReference(e => (TComputer)e.Computer)
                .ForeignKey<TComputerDetail>(e => e.ComputerDetailId);

            modelBuilder.Entity<TDriver>(b =>
                {
                    b.Key(e => e.Name);
                    b.Reference(e => (TLicense)e.License).InverseReference(e => (TDriver)e.Driver)
                        .PrincipalKey<TDriver>(e => e.Name);
                });

            modelBuilder.Entity<TSmartCard>(b =>
                {
                    b.Key(e => e.Username);

                    b.Reference(e => (TLogin)e.Login).InverseReference()
                        .ForeignKey<TSmartCard>(e => e.Username);

                    b.Reference(e => (TLastLogin)e.LastLogin).InverseReference()
                        .ForeignKey<TLastLogin>(e => e.SmartcardUsername);
                });

            modelBuilder.Entity<TRsaToken>(b =>
                {
                    b.Key(e => e.Serial);
                    b.Reference(e => (TLogin)e.Login).InverseReference()
                        .ForeignKey<TRsaToken>(e => e.Username);
                });

            // TODO: Many-to-many
            //modelBuilder.Entity<TSupplier>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.SupplierId));

            // TODO: Inheritance
            //modelBuilder.Entity<TBackOrderLine>().ForeignKeys(fk => fk.ForeignKey<TSupplier>(e => e.SupplierId));
            //modelBuilder.Entity<TDiscontinuedProduct>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ReplacementProductId));
            //modelBuilder.Entity<TProductPageView>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ProductId));

            if (_onModelCreating != null)
            {
                _onModelCreating(modelBuilder);
            }
        }

        public override void SeedUsingFKs(bool saveChanges = true)
        {
            var customer0 = Add(new TCustomer { Name = "Eeky Bear" }).Entity;
            var customer1 = Add(new TCustomer { Name = "Sheila Koalie" }).Entity;
            var customer3 = Add(new TCustomer { Name = "Tarquin Tiger" }).Entity;

            var customer2 = Add(new TCustomer { Name = "Sue Pandy", HusbandId = customer0.CustomerId }).Entity;

            var product1 = Add(new TProduct { Description = "Mrs Koalie's Famous Waffles", BaseConcurrency = "Pounds Sterling" }).Entity;
            var product2 = Add(new TProduct { Description = "Chocolate Donuts", BaseConcurrency = "US Dollars" }).Entity;
            var product3 = Add(new TProduct { Description = "Assorted Dog Treats", BaseConcurrency = "Stuffy Money" }).Entity;

            var barcode1 = Add(new TBarcode { Code = new byte[] { 1, 2, 3, 4 }, ProductId = product1.ProductId, Text = "Barcode 1 2 3 4" }).Entity;
            var barcode2 = Add(new TBarcode { Code = new byte[] { 2, 2, 3, 4 }, ProductId = product2.ProductId, Text = "Barcode 2 2 3 4" }).Entity;
            var barcode3 = Add(new TBarcode { Code = new byte[] { 3, 2, 3, 4 }, ProductId = product3.ProductId, Text = "Barcode 3 2 3 4" }).Entity;

            var barcodeDetails1 = Add(new TBarcodeDetail { Code = barcode1.Code, RegisteredTo = "Eeky Bear" }).Entity;
            var barcodeDetails2 = Add(new TBarcodeDetail { Code = barcode2.Code, RegisteredTo = "Trent" }).Entity;

            var incorrectScan1 = Add(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 9, 6),
                        Details = "Treats not Donuts",
                        ActualCode = barcode3.Code,
                        ExpectedCode = barcode2.Code
                    }).Entity;

            var incorrectScan2 = Add(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 15, 31),
                        Details = "Wot no waffles?",
                        ActualCode = barcode2.Code,
                        ExpectedCode = barcode1.Code
                    }).Entity;

            var complaint1 = Add(new TComplaint
                {
                    CustomerId = customer2.CustomerId,
                    AlternateId = 88,
                    Details = "Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 27, 19, 22, 26)
                }).Entity;

            var complaint2 = Add(new TComplaint
                {
                    CustomerId = customer2.CustomerId,
                    AlternateId = 89,
                    Details = "Really! Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 28, 19, 22, 26)
                }).Entity;

            var resolution = Add(new TResolution
                {
                    ResolutionId = complaint2.AlternateId,
                    Details = "Destroyed all coffee in Redmond area."
                }).Entity;

            var login1 = Add(new TLogin { CustomerId = customer1.CustomerId, Username = "MrsKoalie73", AlternateUsername = "Sheila" }).Entity;
            var login2 = Add(new TLogin { CustomerId = customer2.CustomerId, Username = "MrsBossyPants", AlternateUsername = "Sue" }).Entity;
            var login3 = Add(new TLogin { CustomerId = customer3.CustomerId, Username = "TheStripedMenace", AlternateUsername = "Tarquin" }).Entity;

            var suspiciousActivity1 = Add(new TSuspiciousActivity { Activity = "Pig prints on keyboard", Username = login3.Username }).Entity;
            var suspiciousActivity2 = Add(new TSuspiciousActivity { Activity = "Crumbs in the cupboard", Username = login3.Username }).Entity;
            var suspiciousActivity3 = Add(new TSuspiciousActivity { Activity = "Donuts gone missing", Username = login3.Username }).Entity;

            var rsaToken1 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "1234", Username = login1.Username }).Entity;
            var rsaToken2 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "2234", Username = login2.Username }).Entity;

            var smartCard1 = Add(new TSmartCard { Username = login1.Username, CardSerial = rsaToken1.Serial, Issued = rsaToken1.Issued }).Entity;
            var smartCard2 = Add(new TSmartCard { Username = login2.Username, CardSerial = rsaToken2.Serial, Issued = rsaToken2.Issued }).Entity;

            var reset1 = Add(new TPasswordReset
                {
                    EmailedTo = "trent@example.com",
                    ResetNo = 1,
                    TempPassword = "Rent-A-Mole",
                    Username = login3.AlternateUsername
                }).Entity;

            var pageView1 = Add(new TPageView { PageUrl = "somePage1", Username = login1.Username, Viewed = DateTime.Now }).Entity;
            var pageView2 = Add(new TPageView { PageUrl = "somePage2", Username = login1.Username, Viewed = DateTime.Now }).Entity;
            var pageView3 = Add(new TPageView { PageUrl = "somePage3", Username = login1.Username, Viewed = DateTime.Now }).Entity;

            var lastLogin1 = Add(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 10, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 11, 22, 26),
                    Username = login1.Username,
                    SmartcardUsername = smartCard1.Username
                }).Entity;

            var lastLogin2 = Add(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 12, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 13, 22, 26),
                    Username = login2.Username,
                    SmartcardUsername = smartCard2.Username
                }).Entity;

            var message1 = Add(new TMessage
                {
                    Subject = "Tea?",
                    Body = "Fancy a cup of tea?",
                    FromUsername = login1.Username,
                    ToUsername = login2.Username,
                    Sent = DateTime.Now
                }).Entity;

            var message2 = Add(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "Love one!",
                    FromUsername = login2.Username,
                    ToUsername = login1.Username,
                    Sent = DateTime.Now
                }).Entity;

            var message3 = Add(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "I'll put the kettle on.",
                    FromUsername = login1.Username,
                    ToUsername = login2.Username,
                    Sent = DateTime.Now
                }).Entity;

            var order1 = Add(new TAnOrder { CustomerId = customer1.CustomerId, Username = login1.Username, AlternateId = 77 }).Entity;
            var order2 = Add(new TAnOrder { CustomerId = customer2.CustomerId, Username = login2.Username, AlternateId = 78 }).Entity;
            var order3 = Add(new TAnOrder { CustomerId = customer3.CustomerId, Username = login3.Username, AlternateId = 79 }).Entity;

            var orderNote1 = Add(new TOrderNote { Note = "Must have tea!", OrderId = order1.AlternateId }).Entity;
            var orderNote2 = Add(new TOrderNote { Note = "And donuts!", OrderId = order1.AlternateId }).Entity;
            var orderNote3 = Add(new TOrderNote { Note = "But no coffee. :-(", OrderId = order1.AlternateId }).Entity;

            var orderQualityCheck1 = Add(new TOrderQualityCheck { OrderId = order1.AlternateId, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now }).Entity;
            var orderQualityCheck2 = Add(new TOrderQualityCheck { OrderId = order2.AlternateId, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now }).Entity;
            var orderQualityCheck3 = Add(new TOrderQualityCheck { OrderId = order3.AlternateId, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now }).Entity;

            var orderLine1 = Add(new TOrderLine { OrderId = order1.AnOrderId, ProductId = product1.ProductId, Quantity = 7 }).Entity;
            var orderLine2 = Add(new TOrderLine { OrderId = order1.AnOrderId, ProductId = product2.ProductId, Quantity = 1 }).Entity;
            var orderLine3 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product3.ProductId, Quantity = 2 }).Entity;
            var orderLine4 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product2.ProductId, Quantity = 3 }).Entity;
            var orderLine5 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product1.ProductId, Quantity = 4 }).Entity;
            var orderLine6 = Add(new TOrderLine { OrderId = order3.AnOrderId, ProductId = product2.ProductId, Quantity = 5 }).Entity;

            var productDetail1 = Add(new TProductDetail { Details = "A Waffle Cart specialty!", ProductId = product1.ProductId }).Entity;
            var productDetail2 = Add(new TProductDetail { Details = "Eeky Bear's favorite!", ProductId = product2.ProductId }).Entity;

            var productReview1 = Add(new TProductReview { ProductId = product1.ProductId, Review = "Better than Tarqies!" }).Entity;
            var productReview2 = Add(new TProductReview { ProductId = product1.ProductId, Review = "Good with maple syrup." }).Entity;
            var productReview3 = Add(new TProductReview { ProductId = product2.ProductId, Review = "Eeky says yes!" }).Entity;

            var productPhoto1 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 101, 102 } }).Entity;
            var productPhoto2 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 103, 104 } }).Entity;
            var productPhoto3 = Add(new TProductPhoto { ProductId = product3.ProductId, Photo = new byte[] { 105, 106 } }).Entity;

            var productWebFeature1 = Add(new TProductWebFeature
                {
                    Heading = "Waffle Style",
                    PhotoId = productPhoto1.PhotoId,
                    ProductId = product1.ProductId,
                    ReviewId = productReview1.ReviewId
                }).Entity;

            var productWebFeature2 = Add(new TProductWebFeature
                {
                    Heading = "What does the waffle say?",
                    ProductId = product2.ProductId,
                    ReviewId = productReview3.ReviewId
                }).Entity;

            var supplier1 = Add(new TSupplier { Name = "Trading As Trent" }).Entity;
            var supplier2 = Add(new TSupplier { Name = "Ants By Boris" }).Entity;

            var supplierLogo1 = Add(new TSupplierLogo { SupplierId = supplier1.SupplierId, Logo = new byte[] { 201, 202 } }).Entity;

            var supplierInfo1 = Add(new TSupplierInfo { SupplierId = supplier1.SupplierId, Information = "Seems a bit dodgy." }).Entity;
            var supplierInfo2 = Add(new TSupplierInfo { SupplierId = supplier1.SupplierId, Information = "Orange fur?" }).Entity;
            var supplierInfo3 = Add(new TSupplierInfo { SupplierId = supplier2.SupplierId, Information = "Very expensive!" }).Entity;

            var customerInfo1 = Add(new TCustomerInfo { CustomerInfoId = customer1.CustomerId, Information = "Really likes tea." }).Entity;
            var customerInfo2 = Add(new TCustomerInfo { CustomerInfoId = customer2.CustomerId, Information = "Mrs Bossy Pants!" }).Entity;

            var computer1 = Add(new TComputer { Name = "markash420" }).Entity;
            var computer2 = Add(new TComputer { Name = "unicorns420" }).Entity;

            var computerDetail1 = Add(new TComputerDetail
                {
                    ComputerDetailId = computer1.ComputerId,
                    Manufacturer = "Dell",
                    Model = "420",
                    PurchaseDate = new DateTime(2008, 4, 1),
                    Serial = "4201",
                    Specifications = "It's a Dell!"
                }).Entity;

            var computerDetail2 = Add(new TComputerDetail
                {
                    ComputerDetailId = computer2.ComputerId,
                    Manufacturer = "Not A Dell",
                    Model = "Not 420",
                    PurchaseDate = new DateTime(2012, 4, 1),
                    Serial = "4202",
                    Specifications = "It's not a Dell!"
                }).Entity;

            var driver1 = Add(new TDriver { BirthDate = new DateTime(2006, 9, 19), Name = "Eeky Bear" }).Entity;
            var driver2 = Add(new TDriver { BirthDate = new DateTime(2007, 9, 19), Name = "Splash Bear" }).Entity;

            var license1 = Add(new TLicense
                {
                    Name = driver1.Name,
                    LicenseClass = "C",
                    LicenseNumber = "10",
                    Restrictions = "None",
                    State = LicenseState.Active,
                    ExpirationDate = new DateTime(2018, 9, 19)
                }).Entity;

            var license2 = Add(new TLicense
                {
                    Name = driver2.Name,
                    LicenseClass = "A",
                    LicenseNumber = "11",
                    Restrictions = "None",
                    State = LicenseState.Revoked,
                    ExpirationDate = new DateTime(2018, 9, 19)
                }).Entity;

            if (saveChanges)
            {
                SaveChanges();
            }
        }

        public override void SeedUsingNavigations(bool dependentNavs, bool principalNavs, bool saveChanges = true)
        {
            var customer0 = Add(new TCustomer { Name = "Eeky Bear" }).Entity;
            var customer1 = Add(new TCustomer { Name = "Sheila Koalie" }).Entity;
            var customer3 = Add(new TCustomer { Name = "Tarquin Tiger" }).Entity;

            var customer2 = Add(new TCustomer { Name = "Sue Pandy", Husband = dependentNavs ? customer0 : null }).Entity;
            if (principalNavs)
            {
                customer0.Wife = customer2;
            }

            var product1 = Add(new TProduct { Description = "Mrs Koalie's Famous Waffles", BaseConcurrency = "Pounds Sterling" }).Entity;
            var product2 = Add(new TProduct { Description = "Chocolate Donuts", BaseConcurrency = "US Dollars" }).Entity;
            var product3 = Add(new TProduct { Description = "Assorted Dog Treats", BaseConcurrency = "Stuffy Money" }).Entity;

            var barcode1 = Add(new TBarcode { Code = new byte[] { 1, 2, 3, 4 }, Product = dependentNavs ? product1 : null, Text = "Barcode 1 2 3 4" }).Entity;
            var barcode2 = Add(new TBarcode { Code = new byte[] { 2, 2, 3, 4 }, Product = dependentNavs ? product2 : null, Text = "Barcode 2 2 3 4" }).Entity;
            var barcode3 = Add(new TBarcode { Code = new byte[] { 3, 2, 3, 4 }, Product = dependentNavs ? product3 : null, Text = "Barcode 3 2 3 4" }).Entity;
            if (principalNavs)
            {
                product1.InitializeCollections();
                product1.Barcodes.Add(barcode1);
                product2.InitializeCollections();
                product2.Barcodes.Add(barcode2);
                product3.InitializeCollections();
                product3.Barcodes.Add(barcode3);
            }

            var barcodeDetails1 = Add(new TBarcodeDetail { Code = barcode1.Code, RegisteredTo = "Eeky Bear" }).Entity;
            var barcodeDetails2 = Add(new TBarcodeDetail { Code = barcode2.Code, RegisteredTo = "Trent" }).Entity;
            if (principalNavs)
            {
                barcode1.Detail = barcodeDetails1;
                barcode2.Detail = barcodeDetails2;
            }

            var incorrectScan1 = Add(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 9, 6),
                        Details = "Treats not Donuts",
                        ActualBarcode = barcode3,
                        ExpectedBarcode = dependentNavs ? barcode2 : null
                    }).Entity;
            if (principalNavs)
            {
                barcode2.InitializeCollections();
                barcode2.BadScans.Add(incorrectScan1);
            }

            var incorrectScan2 = Add(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 15, 31),
                        Details = "Wot no waffles?",
                        ActualBarcode = barcode2,
                        ExpectedBarcode = dependentNavs ? barcode1 : null
                    }).Entity;
            if (principalNavs)
            {
                barcode1.InitializeCollections();
                barcode1.BadScans.Add(incorrectScan2);
            }

            var complaint1 = Add(new TComplaint
                {
                    Customer = customer2,
                    AlternateId = 88,
                    Details = "Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 27, 19, 22, 26)
                }).Entity;

            var complaint2 = Add(new TComplaint
                {
                    Customer = customer2,
                    AlternateId = 89,
                    Details = "Really! Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 28, 19, 22, 26)
                }).Entity;

            var resolution = Add(new TResolution { Complaint = dependentNavs ? complaint2 : null, Details = "Destroyed all coffee in Redmond area." }).Entity;
            if (principalNavs)
            {
                complaint2.Resolution = resolution;
            }

            var login1 = Add(new TLogin { Customer = dependentNavs ? customer1 : null, Username = "MrsKoalie73", AlternateUsername = "Sheila" }).Entity;
            var login2 = Add(new TLogin { Customer = dependentNavs ? customer2 : null, Username = "MrsBossyPants", AlternateUsername = "Sue" }).Entity;
            var login3 = Add(new TLogin { Customer = dependentNavs ? customer3 : null, Username = "TheStripedMenace", AlternateUsername = "Tarquin" }).Entity;
            if (principalNavs)
            {
                customer1.InitializeCollections();
                customer1.Logins.Add(login1);
                customer2.InitializeCollections();
                customer2.Logins.Add(login2);
                customer3.InitializeCollections();
                customer3.Logins.Add(login3);
            }

            var suspiciousActivity1 = Add(new TSuspiciousActivity { Activity = "Pig prints on keyboard", Username = login3.Username }).Entity;
            var suspiciousActivity2 = Add(new TSuspiciousActivity { Activity = "Crumbs in the cupboard", Username = login3.Username }).Entity;
            var suspiciousActivity3 = Add(new TSuspiciousActivity { Activity = "Donuts gone missing", Username = login3.Username }).Entity;

            var rsaToken1 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "1234", Login = login1 }).Entity;
            var rsaToken2 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "2234", Login = login2 }).Entity;

            var smartCard1 = Add(new TSmartCard { Login = login1, CardSerial = rsaToken1.Serial, Issued = rsaToken1.Issued }).Entity;
            var smartCard2 = Add(new TSmartCard { Login = login2, CardSerial = rsaToken2.Serial, Issued = rsaToken2.Issued }).Entity;

            var reset1 = Add(new TPasswordReset
                {
                    EmailedTo = "trent@example.com",
                    ResetNo = 1,
                    TempPassword = "Rent-A-Mole",
                    Login = login3
                }).Entity;

            var pageView1 = Add(new TPageView { PageUrl = "somePage1", Login = login1, Viewed = DateTime.Now }).Entity;
            var pageView2 = Add(new TPageView { PageUrl = "somePage2", Login = login1, Viewed = DateTime.Now }).Entity;
            var pageView3 = Add(new TPageView { PageUrl = "somePage3", Login = login1, Viewed = DateTime.Now }).Entity;

            var lastLogin1 = Add(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 10, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 11, 22, 26),
                    Login = login1,
                    SmartcardUsername = smartCard1.Username
                }).Entity;
            if (principalNavs)
            {
                login1.LastLogin = lastLogin1;
                smartCard1.LastLogin = lastLogin1;
            }

            var lastLogin2 = Add(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 12, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 13, 22, 26),
                    Login = login2,
                    SmartcardUsername = smartCard2.Username
                }).Entity;
            if (principalNavs)
            {
                login2.LastLogin = lastLogin2;
                smartCard2.LastLogin = lastLogin2;
            }

            var message1 = Add(new TMessage
                {
                    Subject = "Tea?",
                    Body = "Fancy a cup of tea?",
                    Sender = login1,
                    Recipient = dependentNavs ? login2 : null,
                    Sent = DateTime.Now
                }).Entity;
            if (principalNavs)
            {
                login1.InitializeCollections();
                login1.SentMessages.Add(message1);
                login2.InitializeCollections();
                login2.ReceivedMessages.Add(message1);
            }

            var message2 = Add(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "Love one!",
                    Sender = login2,
                    Recipient = dependentNavs ? login1 : null,
                    Sent = DateTime.Now
                }).Entity;
            if (principalNavs)
            {
                login2.SentMessages.Add(message2);
                login1.ReceivedMessages.Add(message2);
            }

            var message3 = Add(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "I'll put the kettle on.",
                    Sender = login1,
                    Recipient = dependentNavs ? login2 : null,
                    Sent = DateTime.Now
                }).Entity;
            if (principalNavs)
            {
                login1.SentMessages.Add(message3);
                login2.ReceivedMessages.Add(message3);
            }

            var order1 = Add(new TAnOrder { Customer = dependentNavs ? customer1 : null, Login = dependentNavs ? login1 : null, AlternateId = 77 }).Entity;
            var order2 = Add(new TAnOrder { Customer = dependentNavs ? customer2 : null, Login = dependentNavs ? login2 : null, AlternateId = 78 }).Entity;
            var order3 = Add(new TAnOrder { Customer = dependentNavs ? customer3 : null, Login = dependentNavs ? login3 : null, AlternateId = 79 }).Entity;
            if (principalNavs)
            {
                customer1.Orders.Add(order1);
                customer2.Orders.Add(order2);
                customer3.Orders.Add(order3);
                login1.Orders.Add(order1);
                login2.Orders.Add(order2);
                login3.InitializeCollections();
                login3.Orders.Add(order3);
            }

            var orderNote1 = Add(new TOrderNote { Note = "Must have tea!", Order = dependentNavs ? order1 : null }).Entity;
            var orderNote2 = Add(new TOrderNote { Note = "And donuts!", Order = dependentNavs ? order1 : null }).Entity;
            var orderNote3 = Add(new TOrderNote { Note = "But no coffee. :-(", Order = dependentNavs ? order1 : null }).Entity;
            if (principalNavs)
            {
                order1.InitializeCollections();
                order1.Notes.Add(orderNote1);
                order1.Notes.Add(orderNote2);
                order1.Notes.Add(orderNote3);
            }

            var orderQualityCheck1 = Add(new TOrderQualityCheck { Order = order1, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now }).Entity;
            var orderQualityCheck2 = Add(new TOrderQualityCheck { Order = order2, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now }).Entity;
            var orderQualityCheck3 = Add(new TOrderQualityCheck { Order = order3, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now }).Entity;

            var orderLine1 = Add(new TOrderLine { Order = order1, Product = product1, Quantity = 7 }).Entity;
            var orderLine2 = Add(new TOrderLine { Order = order1, Product = product2, Quantity = 1 }).Entity;
            var orderLine3 = Add(new TOrderLine { Order = order2, Product = product3, Quantity = 2 }).Entity;
            var orderLine4 = Add(new TOrderLine { Order = order2, Product = product2, Quantity = 3 }).Entity;
            var orderLine5 = Add(new TOrderLine { Order = order2, Product = product1, Quantity = 4 }).Entity;
            var orderLine6 = Add(new TOrderLine { Order = order3, Product = product2, Quantity = 5 }).Entity;
            if (principalNavs)
            {
                order1.OrderLines.Add(orderLine1);
                order1.OrderLines.Add(orderLine2);
                order2.InitializeCollections();
                order2.OrderLines.Add(orderLine3);
                order2.OrderLines.Add(orderLine4);
                order2.OrderLines.Add(orderLine5);
                order3.InitializeCollections();
                order3.OrderLines.Add(orderLine6);
            }

            var productDetail1 = Add(new TProductDetail { Details = "A Waffle Cart specialty!", Product = product1 }).Entity;
            var productDetail2 = Add(new TProductDetail { Details = "Eeky Bear's favorite!", Product = product2 }).Entity;
            if (principalNavs)
            {
                product1.Detail = productDetail1;
                product2.Detail = productDetail2;
            }

            var productReview1 = Add(new TProductReview { Product = dependentNavs ? product1 : null, Review = "Better than Tarqies!" }).Entity;
            var productReview2 = Add(new TProductReview { Product = dependentNavs ? product1 : null, Review = "Good with maple syrup." }).Entity;
            var productReview3 = Add(new TProductReview { Product = dependentNavs ? product2 : null, Review = "Eeky says yes!" }).Entity;
            if (principalNavs)
            {
                product1.Reviews.Add(productReview1);
                product1.Reviews.Add(productReview2);
                product2.Reviews.Add(productReview3);
            }

            var productPhoto1 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 101, 102 } }).Entity;
            var productPhoto2 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 103, 104 } }).Entity;
            var productPhoto3 = Add(new TProductPhoto { ProductId = product3.ProductId, Photo = new byte[] { 105, 106 } }).Entity;
            if (principalNavs)
            {
                product1.Photos.Add(productPhoto1);
                product1.Photos.Add(productPhoto2);
                product3.Photos.Add(productPhoto3);
            }

            var productWebFeature1 = Add(new TProductWebFeature
                {
                    Heading = "Waffle Style",
                    Photo = dependentNavs ? productPhoto1 : null,
                    ProductId = product1.ProductId,
                    Review = dependentNavs ? productReview1 : null
                }).Entity;
            if (principalNavs)
            {
                productPhoto1.InitializeCollections();
                productPhoto1.Features.Add(productWebFeature1);
                productReview1.InitializeCollections();
                productReview1.Features.Add(productWebFeature1);
            }

            var productWebFeature2 = Add(new TProductWebFeature
                {
                    Heading = "What does the waffle say?",
                    ProductId = product2.ProductId,
                    Review = dependentNavs ? productReview3 : null
                }).Entity;
            if (principalNavs)
            {
                productReview3.InitializeCollections();
                productReview3.Features.Add(productWebFeature2);
            }

            var supplier1 = Add(new TSupplier { Name = "Trading As Trent" }).Entity;
            var supplier2 = Add(new TSupplier { Name = "Ants By Boris" }).Entity;

            var supplierLogo1 = Add(new TSupplierLogo { SupplierId = !principalNavs ? supplier1.SupplierId : 0, Logo = new byte[] { 201, 202 } }).Entity;
            if (principalNavs)
            {
                supplier1.Logo = supplierLogo1;
            }

            var supplierInfo1 = Add(new TSupplierInfo { Supplier = supplier1, Information = "Seems a bit dodgy." }).Entity;
            var supplierInfo2 = Add(new TSupplierInfo { Supplier = supplier1, Information = "Orange fur?" }).Entity;
            var supplierInfo3 = Add(new TSupplierInfo { Supplier = supplier2, Information = "Very expensive!" }).Entity;

            var customerInfo1 = Add(new TCustomerInfo { CustomerInfoId = customer1.CustomerId, Information = "Really likes tea." }).Entity;
            var customerInfo2 = Add(new TCustomerInfo { CustomerInfoId = customer2.CustomerId, Information = "Mrs Bossy Pants!" }).Entity;
            if (principalNavs)
            {
                customer1.Info = customerInfo1;
                customer2.Info = customerInfo2;
            }

            var computer1 = Add(new TComputer { Name = "markash420" }).Entity;
            var computer2 = Add(new TComputer { Name = "unicorns420" }).Entity;

            var computerDetail1 = Add(new TComputerDetail
                {
                    Computer = computer1,
                    Manufacturer = "Dell",
                    Model = "420",
                    PurchaseDate = new DateTime(2008, 4, 1),
                    Serial = "4201",
                    Specifications = "It's a Dell!"
                }).Entity;
            if (principalNavs)
            {
                computer1.ComputerDetail = computerDetail1;
            }

            var computerDetail2 = Add(new TComputerDetail
                {
                    Computer = computer2,
                    Manufacturer = "Not A Dell",
                    Model = "Not 420",
                    PurchaseDate = new DateTime(2012, 4, 1),
                    Serial = "4202",
                    Specifications = "It's not a Dell!"
                }).Entity;
            if (principalNavs)
            {
                computer2.ComputerDetail = computerDetail2;
            }

            var driver1 = Add(new TDriver { BirthDate = new DateTime(2006, 9, 19), Name = "Eeky Bear" }).Entity;
            var driver2 = Add(new TDriver { BirthDate = new DateTime(2007, 9, 19), Name = "Splash Bear" }).Entity;

            var license1 = Add(new TLicense
                {
                    Driver = driver1,
                    LicenseClass = "C",
                    LicenseNumber = "10",
                    Restrictions = "None",
                    State = LicenseState.Active,
                    ExpirationDate = new DateTime(2018, 9, 19)
                }).Entity;
            if (principalNavs)
            {
                driver1.License = license1;
            }

            var license2 = Add(new TLicense
                {
                    Driver = driver2,
                    LicenseClass = "A",
                    LicenseNumber = "11",
                    Restrictions = "None",
                    State = LicenseState.Revoked,
                    ExpirationDate = new DateTime(2018, 9, 19)
                }).Entity;
            if (principalNavs)
            {
                driver2.License = license2;
            }

            if (saveChanges)
            {
                SaveChanges();
            }
        }

        public override void SeedUsingNavigationsWithDeferredAdd(bool saveChanges = true)
        {
            var toAdd = new List<object>[4];

            for (var i = 0; i < toAdd.Length; i++)
            {
                toAdd[i] = new List<object>();
            }

            var customer0 = toAdd[0].AddEx(new TCustomer { Name = "Eeky Bear" });
            var customer1 = toAdd[0].AddEx(new TCustomer { Name = "Sheila Koalie" });
            var customer3 = toAdd[0].AddEx(new TCustomer { Name = "Tarquin Tiger" });
            var customer2 = toAdd[0].AddEx(new TCustomer { Name = "Sue Pandy", Husband = customer0 });

            var product1 = toAdd[0].AddEx(new TProduct { Description = "Mrs Koalie's Famous Waffles", BaseConcurrency = "Pounds Sterling" });
            var product2 = toAdd[0].AddEx(new TProduct { Description = "Chocolate Donuts", BaseConcurrency = "US Dollars" });
            var product3 = toAdd[0].AddEx(new TProduct { Description = "Assorted Dog Treats", BaseConcurrency = "Stuffy Money" });

            var barcode1 = toAdd[1].AddEx(new TBarcode { Code = new byte[] { 1, 2, 3, 4 }, Text = "Barcode 1 2 3 4" });
            var barcode2 = toAdd[1].AddEx(new TBarcode { Code = new byte[] { 2, 2, 3, 4 }, Text = "Barcode 2 2 3 4" });
            var barcode3 = toAdd[1].AddEx(new TBarcode { Code = new byte[] { 3, 2, 3, 4 }, Text = "Barcode 3 2 3 4" });

            product1.InitializeCollections();
            product1.Barcodes.Add(barcode1);
            product2.InitializeCollections();
            product2.Barcodes.Add(barcode2);
            product3.InitializeCollections();
            product3.Barcodes.Add(barcode3);

            var barcodeDetails1 = toAdd[1].AddEx(new TBarcodeDetail { RegisteredTo = "Eeky Bear" });
            var barcodeDetails2 = toAdd[1].AddEx(new TBarcodeDetail { RegisteredTo = "Trent" });

            barcode1.Detail = barcodeDetails1;
            barcode2.Detail = barcodeDetails2;

            var incorrectScan1 = toAdd[1].AddEx(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 9, 6),
                        Details = "Treats not Donuts",
                        ActualBarcode = barcode3
                    });
            barcode2.InitializeCollections();
            barcode2.BadScans.Add(incorrectScan1);

            var incorrectScan2 = toAdd[1].AddEx(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 15, 31),
                        Details = "Wot no waffles?",
                        ActualBarcode = barcode2
                    });
            barcode1.InitializeCollections();
            barcode1.BadScans.Add(incorrectScan2);

            var complaint1 = toAdd[1].AddEx(new TComplaint
                {
                    Customer = customer2,
                    AlternateId = 88,
                    Details = "Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 27, 19, 22, 26)
                });

            var complaint2 = toAdd[1].AddEx(new TComplaint
                {
                    Customer = customer2,
                    AlternateId = 89,
                    Details = "Really! Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 28, 19, 22, 26)
                });

            var resolution = toAdd[2].AddEx(new TResolution { Details = "Destroyed all coffee in Redmond area." });
            complaint2.Resolution = resolution;

            var login1 = toAdd[1].AddEx(new TLogin { Username = "MrsKoalie73", AlternateUsername = "Sheila" });
            var login2 = toAdd[1].AddEx(new TLogin { Username = "MrsBossyPants", AlternateUsername = "Sue" });
            var login3 = toAdd[1].AddEx(new TLogin { Username = "TheStripedMenace", AlternateUsername = "Tarquin" });

            customer1.InitializeCollections();
            customer1.Logins.Add(login1);
            customer2.InitializeCollections();
            customer2.Logins.Add(login2);
            customer3.InitializeCollections();
            customer3.Logins.Add(login3);

            var suspiciousActivity1 = toAdd[2].AddEx(new TSuspiciousActivity { Activity = "Pig prints on keyboard", Username = login3.Username });
            var suspiciousActivity2 = toAdd[2].AddEx(new TSuspiciousActivity { Activity = "Crumbs in the cupboard", Username = login3.Username });
            var suspiciousActivity3 = toAdd[2].AddEx(new TSuspiciousActivity { Activity = "Donuts gone missing", Username = login3.Username });

            var rsaToken1 = toAdd[2].AddEx(new TRsaToken { Issued = DateTime.Now, Serial = "1234", Login = login1 });
            var rsaToken2 = toAdd[2].AddEx(new TRsaToken { Issued = DateTime.Now, Serial = "2234", Login = login2 });

            var smartCard1 = toAdd[2].AddEx(new TSmartCard { Login = login1, CardSerial = rsaToken1.Serial, Issued = rsaToken1.Issued });
            var smartCard2 = toAdd[2].AddEx(new TSmartCard { Login = login2, CardSerial = rsaToken2.Serial, Issued = rsaToken2.Issued });

            var reset1 = toAdd[2].AddEx(new TPasswordReset
                {
                    EmailedTo = "trent@example.com",
                    ResetNo = 1,
                    TempPassword = "Rent-A-Mole",
                    Login = login3
                });

            var pageView1 = toAdd[1].AddEx(new TPageView { PageUrl = "somePage1", Login = login1, Viewed = DateTime.Now });
            var pageView2 = toAdd[1].AddEx(new TPageView { PageUrl = "somePage2", Login = login1, Viewed = DateTime.Now });
            var pageView3 = toAdd[1].AddEx(new TPageView { PageUrl = "somePage3", Login = login1, Viewed = DateTime.Now });

            var lastLogin1 = toAdd[2].AddEx(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 10, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 11, 22, 26)
                });

            login1.LastLogin = lastLogin1;
            smartCard1.LastLogin = lastLogin1;

            var lastLogin2 = toAdd[2].AddEx(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 12, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 13, 22, 26)
                });

            login2.LastLogin = lastLogin2;
            smartCard2.LastLogin = lastLogin2;

            var message1 = toAdd[2].AddEx(new TMessage
                {
                    Subject = "Tea?",
                    Body = "Fancy a cup of tea?",
                    Sent = DateTime.Now
                });

            login1.InitializeCollections();
            login1.SentMessages.Add(message1);
            login2.InitializeCollections();
            login2.ReceivedMessages.Add(message1);

            var message2 = toAdd[2].AddEx(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "Love one!",
                    Sent = DateTime.Now
                });

            login2.SentMessages.Add(message2);
            login1.ReceivedMessages.Add(message2);

            var message3 = toAdd[2].AddEx(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "I'll put the kettle on.",
                    Sent = DateTime.Now
                });

            login1.SentMessages.Add(message3);
            login2.ReceivedMessages.Add(message3);

            var order1 = toAdd[2].AddEx(new TAnOrder { Customer = customer1, Login = login1, AlternateId = 77 });
            var order2 = toAdd[2].AddEx(new TAnOrder { Customer = customer2, Login = login2, AlternateId = 78 });
            var order3 = toAdd[2].AddEx(new TAnOrder { Customer = customer3, Login = login3, AlternateId = 79 });

            customer1.Orders.Add(order1);
            customer2.Orders.Add(order2);
            customer3.Orders.Add(order3);

            login1.Orders.Add(order1);
            login2.Orders.Add(order2);
            login3.InitializeCollections();
            login3.Orders.Add(order3);

            var orderNote1 = toAdd[2].AddEx(new TOrderNote { Note = "Must have tea!" });
            var orderNote2 = toAdd[2].AddEx(new TOrderNote { Note = "And donuts!" });
            var orderNote3 = toAdd[2].AddEx(new TOrderNote { Note = "But no coffee. :-(" });

            order1.InitializeCollections();
            order1.Notes.Add(orderNote1);
            order1.Notes.Add(orderNote2);
            order1.Notes.Add(orderNote3);

            var orderQualityCheck1 = toAdd[2].AddEx(new TOrderQualityCheck { Order = order1, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now });
            var orderQualityCheck2 = toAdd[2].AddEx(new TOrderQualityCheck { Order = order2, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now });
            var orderQualityCheck3 = toAdd[2].AddEx(new TOrderQualityCheck { Order = order3, CheckedBy = "Eeky Bear", CheckedDateTime = DateTime.Now });

            var orderLine1 = toAdd[3].AddEx(new TOrderLine { Product = product1, Quantity = 7 });
            var orderLine2 = toAdd[3].AddEx(new TOrderLine { Product = product2, Quantity = 1 });
            var orderLine3 = toAdd[3].AddEx(new TOrderLine { Product = product3, Quantity = 2 });
            var orderLine4 = toAdd[3].AddEx(new TOrderLine { Product = product2, Quantity = 3 });
            var orderLine5 = toAdd[3].AddEx(new TOrderLine { Product = product1, Quantity = 4 });
            var orderLine6 = toAdd[3].AddEx(new TOrderLine { Product = product2, Quantity = 5 });

            order1.OrderLines.Add(orderLine1);
            order1.OrderLines.Add(orderLine2);
            order2.InitializeCollections();
            order2.OrderLines.Add(orderLine3);
            order2.OrderLines.Add(orderLine4);
            order2.OrderLines.Add(orderLine5);
            order3.InitializeCollections();
            order3.OrderLines.Add(orderLine6);

            var productDetail1 = toAdd[0].AddEx(new TProductDetail { Details = "A Waffle Cart specialty!" });
            var productDetail2 = toAdd[0].AddEx(new TProductDetail { Details = "Eeky Bear's favorite!" });

            product1.Detail = productDetail1;
            product2.Detail = productDetail2;

            var productReview1 = toAdd[0].AddEx(new TProductReview { Review = "Better than Tarqies!" });
            var productReview2 = toAdd[0].AddEx(new TProductReview { Review = "Good with maple syrup." });
            var productReview3 = toAdd[0].AddEx(new TProductReview { Review = "Eeky says yes!" });

            product1.Reviews.Add(productReview1);
            product1.Reviews.Add(productReview2);
            product2.Reviews.Add(productReview3);

            var productPhoto1 = toAdd[0].AddEx(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 101, 102 } });
            var productPhoto2 = toAdd[0].AddEx(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 103, 104 } });
            var productPhoto3 = toAdd[0].AddEx(new TProductPhoto { ProductId = product3.ProductId, Photo = new byte[] { 105, 106 } });

            product1.Photos.Add(productPhoto1);
            product1.Photos.Add(productPhoto2);
            product3.Photos.Add(productPhoto3);

            var productWebFeature1 = toAdd[0].AddEx(new TProductWebFeature
                {
                    Heading = "Waffle Style",
                    ProductId = product1.ProductId
                });

            productPhoto1.InitializeCollections();
            productPhoto1.Features.Add(productWebFeature1);
            productReview1.InitializeCollections();
            productReview1.Features.Add(productWebFeature1);

            var productWebFeature2 = toAdd[0].AddEx(new TProductWebFeature
                {
                    Heading = "What does the waffle say?",
                    ProductId = product2.ProductId
                });

            productReview3.InitializeCollections();
            productReview3.Features.Add(productWebFeature2);

            var supplier1 = toAdd[0].AddEx(new TSupplier { Name = "Trading As Trent" });
            var supplier2 = toAdd[0].AddEx(new TSupplier { Name = "Ants By Boris" });

            var supplierLogo1 = toAdd[0].AddEx(new TSupplierLogo { Logo = new byte[] { 201, 202 } });

            supplier1.Logo = supplierLogo1;

            var supplierInfo1 = toAdd[0].AddEx(new TSupplierInfo { Supplier = supplier1, Information = "Seems a bit dodgy." });
            var supplierInfo2 = toAdd[0].AddEx(new TSupplierInfo { Supplier = supplier1, Information = "Orange fur?" });
            var supplierInfo3 = toAdd[0].AddEx(new TSupplierInfo { Supplier = supplier2, Information = "Very expensive!" });

            var customerInfo1 = toAdd[0].AddEx(new TCustomerInfo { Information = "Really likes tea." });
            var customerInfo2 = toAdd[0].AddEx(new TCustomerInfo { Information = "Mrs Bossy Pants!" });

            customer1.Info = customerInfo1;
            customer2.Info = customerInfo2;

            var computer1 = toAdd[0].AddEx(new TComputer { Name = "markash420" });
            var computer2 = toAdd[0].AddEx(new TComputer { Name = "unicorns420" });

            var computerDetail1 = toAdd[0].AddEx(new TComputerDetail
                {
                    Manufacturer = "Dell",
                    Model = "420",
                    PurchaseDate = new DateTime(2008, 4, 1),
                    Serial = "4201",
                    Specifications = "It's a Dell!"
                });

            computer1.ComputerDetail = computerDetail1;

            var computerDetail2 = toAdd[0].AddEx(new TComputerDetail
                {
                    Manufacturer = "Not A Dell",
                    Model = "Not 420",
                    PurchaseDate = new DateTime(2012, 4, 1),
                    Serial = "4202",
                    Specifications = "It's not a Dell!"
                });

            computer2.ComputerDetail = computerDetail2;

            var driver1 = toAdd[0].AddEx(new TDriver { BirthDate = new DateTime(2006, 9, 19), Name = "Eeky Bear" });
            var driver2 = toAdd[0].AddEx(new TDriver { BirthDate = new DateTime(2007, 9, 19), Name = "Splash Bear" });

            var license1 = toAdd[1].AddEx(new TLicense
                {
                    LicenseClass = "C",
                    LicenseNumber = "10",
                    Restrictions = "None",
                    State = LicenseState.Active,
                    ExpirationDate = new DateTime(2018, 9, 19)
                });

            driver1.License = license1;

            var license2 = toAdd[1].AddEx(new TLicense
                {
                    LicenseClass = "A",
                    LicenseNumber = "11",
                    Restrictions = "None",
                    State = LicenseState.Revoked,
                    ExpirationDate = new DateTime(2018, 9, 19)
                });
            driver2.License = license2;

            foreach (var entity in toAdd.SelectMany(l => l))
            {
                Add(entity);
            }

            if (saveChanges)
            {
                SaveChanges();
            }
        }
    }

    internal static class Adder
    {
        public static TValue AddEx<TValue>(this List<object> list, TValue value)
        {
            list.Add(value);
            return value;
        }
    }
}
