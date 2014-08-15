// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.MonsterModel
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
        private readonly Action<BasicModelBuilder> _onModelCreating;

        public MonsterContext(IServiceProvider serviceProvider, DbContextOptions options, Action<BasicModelBuilder> onModelCreating)
            : base(serviceProvider, options)
        {
            _onModelCreating = onModelCreating;
        }

        // TODO: Temporary means to disable use of candidate keys on SQL Server. See GitHub #537
        public bool SupportsCandidateKeys { get; set; }

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
            modelBuilder.Entity<TBarcodeDetail>().Key(e => e.Code);
            modelBuilder.Entity<TResolution>();
            modelBuilder.Entity<TSuspiciousActivity>();
            modelBuilder.Entity<TLastLogin>().Key(e => e.Username);
            modelBuilder.Entity<TMessage>().Key(e => new { e.MessageId, e.FromUsername });
            modelBuilder.Entity<TOrderNote>().Key(e => e.NoteId);
            modelBuilder.Entity<TProductDetail>().Key(e => e.ProductId);
            modelBuilder.Entity<TProductWebFeature>().Key(e => e.ProductId);
            modelBuilder.Entity<TSupplierLogo>().Key(e => e.SupplierId);
            modelBuilder.Entity<TCustomerInfo>();
            modelBuilder.Entity<TComputerDetail>();
            modelBuilder.Entity<TLicense>().Key(e => e.Name);

            modelBuilder.Entity<TAnOrder>(b =>
                {
                    b.OneToMany(e => (IEnumerable<TOrderLine>)e.OrderLines, e => (TAnOrder)e.Order);

                    if (SupportsCandidateKeys)
                    {
                        b.OneToMany(e => (IEnumerable<TOrderNote>)e.Notes, e => (TAnOrder)e.Order)
                            .ReferencedKey(e => e.AlternateId);
                    }
                    else
                    {
                        b.OneToMany(e => (IEnumerable<TOrderNote>)e.Notes, e => (TAnOrder)e.Order)
                            .ReferencedKey(e => e.AnOrderId);
                    }
                });

            modelBuilder.Entity<TOrderQualityCheck>(b =>
                {
                    b.Key(e => e.OrderId);

                    if (SupportsCandidateKeys)
                    {
                        b.OneToOne(e => (TAnOrder)e.Order)
                            .ForeignKey<TOrderQualityCheck>(e => e.OrderId)
                            .ReferencedKey<TAnOrder>(e => e.AlternateId);
                    }
                    else
                    {
                        b.OneToOne(e => (TAnOrder)e.Order)
                            .ForeignKey<TOrderQualityCheck>(e => e.OrderId)
                            .ReferencedKey<TAnOrder>(e => e.AnOrderId);
                    }
                });

            modelBuilder.Entity<TProduct>(b =>
                {
                    b.OneToMany(e => (IEnumerable<TProductReview>)e.Reviews, e => (TProduct)e.Product);
                    b.OneToMany(e => (IEnumerable<TBarcode>)e.Barcodes, e => (TProduct)e.Product);
                    b.OneToMany(e => (IEnumerable<TProductPhoto>)e.Photos);
                    b.OneToOne(e => (TProductDetail)e.Detail, e => (TProduct)e.Product);
                    b.OneToOne<TProductWebFeature>();
                });

            modelBuilder.Entity<TOrderLine>(b =>
                {
                    b.Key(e => new { e.OrderId, e.ProductId });
                    b.ManyToOne(e => (TProduct)e.Product);
                });

            modelBuilder.Entity<TSupplier>().OneToOne(e => (TSupplierLogo)e.Logo);

            modelBuilder.Entity<TCustomer>(b =>
                {
                    b.OneToMany(e => (IEnumerable<TAnOrder>)e.Orders, e => (TCustomer)e.Customer);
                    b.OneToMany(e => (IEnumerable<TLogin>)e.Logins, e => (TCustomer)e.Customer);
                    b.OneToOne(e => (TCustomerInfo)e.Info);

                    b.OneToOne(e => (TCustomer)e.Wife, e => (TCustomer)e.Husband)
                        .ForeignKey<TCustomer>(e => e.HusbandId);
                });

            modelBuilder.Entity<TComplaint>(b =>
                {
                    b.ManyToOne(e => (TCustomer)e.Customer)
                        .ForeignKey(e => e.CustomerId);

                    if (SupportsCandidateKeys)
                    {
                        b.OneToOne(e => (TResolution)e.Resolution, e => (TComplaint)e.Complaint)
                            .ReferencedKey<TComplaint>(e => e.AlternateId);
                    }
                    else
                    {
                        b.OneToOne(e => (TResolution)e.Resolution, e => (TComplaint)e.Complaint)
                            .ReferencedKey<TComplaint>(e => e.ComplaintId);
                    }
                });

            modelBuilder.Entity<TProductPhoto>(b =>
                {
                    b.Key(e => new { e.ProductId, e.PhotoId });

                    b.OneToMany(e => (IEnumerable<TProductWebFeature>)e.Features, e => (TProductPhoto)e.Photo)
                        .ForeignKey(e => new { e.ProductId, e.PhotoId })
                        .ReferencedKey(e => new { e.ProductId, e.PhotoId });
                });

            modelBuilder.Entity<TProductReview>(b =>
                {
                    b.Key(e => new { e.ProductId, e.ReviewId });

                    b.OneToMany(e => (IEnumerable<TProductWebFeature>)e.Features, e => (TProductReview)e.Review)
                        .ForeignKey(e => new { e.ProductId, e.ReviewId });
                });

            modelBuilder.Entity<TLogin>(b =>
                {
                    b.Key(e => e.Username);

                    b.OneToMany(e => (IEnumerable<TMessage>)e.SentMessages, e => (TLogin)e.Sender)
                        .ForeignKey(e => e.FromUsername);

                    b.OneToMany(e => (IEnumerable<TMessage>)e.ReceivedMessages, e => (TLogin)e.Recipient)
                        .ForeignKey(e => e.ToUsername);

                    b.OneToMany(e => (IEnumerable<TAnOrder>)e.Orders, e => (TLogin)e.Login)
                        .ForeignKey(e => e.Username);

                    b.OneToMany<TSuspiciousActivity>()
                        .ForeignKey(e => e.Username);

                    b.OneToOne(e => (TLastLogin)e.LastLogin, e => (TLogin)e.Login);
                });

            modelBuilder.Entity<TPasswordReset>(b =>
                {
                    b.Key(e => new { e.ResetNo, e.Username });

                    if (SupportsCandidateKeys)
                    {
                        b.ManyToOne(e => (TLogin)e.Login)
                            .ForeignKey(e => e.Username)
                            .ReferencedKey(e => e.AlternateUsername);
                    }
                    else
                    {
                        b.ManyToOne(e => (TLogin)e.Login)
                            .ForeignKey(e => e.Username)
                            .ReferencedKey(e => e.Username);
                    }
                });

            modelBuilder.Entity<TPageView>()
                .ManyToOne(e => (TLogin)e.Login)
                .ForeignKey(e => e.Username);

            modelBuilder.Entity<TBarcode>(b =>
                {
                    b.Key(e => e.Code);

                    b.OneToMany(e => (IEnumerable<TIncorrectScan>)e.BadScans, e => (TBarcode)e.ExpectedBarcode)
                        .ForeignKey(e => e.ExpectedCode);

                    b.OneToOne(e => (TBarcodeDetail)e.Detail);
                });

            modelBuilder.Entity<TIncorrectScan>()
                .ManyToOne(e => (TBarcode)e.ActualBarcode)
                .ForeignKey(e => e.ActualCode);

            modelBuilder.Entity<TSupplierInfo>().ManyToOne(e => (TSupplier)e.Supplier);

            modelBuilder.Entity<TComputer>().OneToOne(e => (TComputerDetail)e.ComputerDetail, e => (TComputer)e.Computer);

            modelBuilder.Entity<TDriver>(b =>
                {
                    b.Key(e => e.Name);
                    b.OneToOne(e => (TLicense)e.License, e => (TDriver)e.Driver);
                });

            modelBuilder.Entity<TSmartCard>(b =>
                {
                    b.Key(e => e.Username);

                    b.OneToOne(e => (TLogin)e.Login)
                        .ForeignKey<TSmartCard>(e => e.Username);

                    b.OneToOne(e => (TLastLogin)e.LastLogin)
                        .ForeignKey<TLastLogin>(e => e.SmartcardUsername);
                });

            modelBuilder.Entity<TRsaToken>(b =>
                {
                    b.Key(e => e.Serial);
                    b.OneToOne(e => (TLogin)e.Login)
                        .ForeignKey<TRsaToken>(e => e.Username);
                });

            // TODO: Many-to-many
            //modelBuilder.Entity<TSupplier>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.SupplierId));

            // TODO: Inheritance
            //modelBuilder.Entity<TBackOrderLine>().ForeignKeys(fk => fk.ForeignKey<TSupplier>(e => e.SupplierId));
            //modelBuilder.Entity<TDiscontinuedProduct>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ReplacementProductId));
            //modelBuilder.Entity<TProductPageView>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ProductId));

            var model = modelBuilder.Model;

            // TODO: Key should get by-convention value generation even if key is not discovered by convention
            var noteId = model.GetEntityType(typeof(TOrderNote)).GetProperty("NoteId");
            noteId.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            noteId.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

            var featureId = model.GetEntityType(typeof(TProductWebFeature)).GetProperty("FeatureId");
            featureId.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            featureId.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

            // TODO: Should key get by-convention value generation even if part of composite key?
            var reviewId = model.GetEntityType(typeof(TProductReview)).GetProperty("ReviewId");
            reviewId.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            reviewId.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

            var photoId = model.GetEntityType(typeof(TProductPhoto)).GetProperty("PhotoId");
            photoId.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            photoId.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

            // TODO: Key should not get by-convention value generation if it is dependent of identifying relationship
            var detailId = model.GetEntityType(typeof(TComputerDetail)).GetProperty("ComputerDetailId");
            detailId.ValueGenerationOnAdd = ValueGenerationOnAdd.None;
            detailId.ValueGenerationOnSave = ValueGenerationOnSave.None;

            var resolutionId = model.GetEntityType(typeof(TResolution)).GetProperty("ResolutionId");
            resolutionId.ValueGenerationOnAdd = ValueGenerationOnAdd.None;
            resolutionId.ValueGenerationOnSave = ValueGenerationOnSave.None;

            var customerId = model.GetEntityType(typeof(TCustomerInfo)).GetProperty("CustomerInfoId");
            customerId.ValueGenerationOnAdd = ValueGenerationOnAdd.None;
            customerId.ValueGenerationOnSave = ValueGenerationOnSave.None;

            if (_onModelCreating != null)
            {
                _onModelCreating(modelBuilder);
            }
        }

        public override void SeedUsingFKs(bool saveChanges = true)
        {
            var customer0 = Add(new TCustomer { Name = "Eeky Bear" });
            var customer1 = Add(new TCustomer { Name = "Sheila Koalie" });
            var customer3 = Add(new TCustomer { Name = "Tarquin Tiger" });

            var customer2 = Add(new TCustomer { Name = "Sue Pandy", HusbandId = customer0.CustomerId });

            var product1 = Add(new TProduct { Description = "Mrs Koalie's Famous Waffles", BaseConcurrency = "Pounds Sterling" });
            var product2 = Add(new TProduct { Description = "Chocolate Donuts", BaseConcurrency = "US Dollars" });
            var product3 = Add(new TProduct { Description = "Assorted Dog Treats", BaseConcurrency = "Stuffy Money" });

            var barcode1 = Add(new TBarcode { Code = new byte[] { 1, 2, 3, 4 }, ProductId = product1.ProductId, Text = "Barcode 1 2 3 4" });
            var barcode2 = Add(new TBarcode { Code = new byte[] { 2, 2, 3, 4 }, ProductId = product2.ProductId, Text = "Barcode 2 2 3 4" });
            var barcode3 = Add(new TBarcode { Code = new byte[] { 3, 2, 3, 4 }, ProductId = product3.ProductId, Text = "Barcode 3 2 3 4" });

            var barcodeDetails1 = Add(new TBarcodeDetail { Code = barcode1.Code, RegisteredTo = "Eeky Bear" });
            var barcodeDetails2 = Add(new TBarcodeDetail { Code = barcode2.Code, RegisteredTo = "Trent" });

            var incorrectScan1 = Add(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 9, 6),
                        Details = "Treats not Donuts",
                        ActualCode = barcode3.Code,
                        ExpectedCode = barcode2.Code
                    });

            var incorrectScan2 = Add(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 15, 31),
                        Details = "Wot no waffles?",
                        ActualCode = barcode2.Code,
                        ExpectedCode = barcode1.Code
                    });

            var complaint1 = Add(new TComplaint
                {
                    CustomerId = customer2.CustomerId,
                    AlternateId = 88,
                    Details = "Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 27, 19, 22, 26)
                });

            var complaint2 = Add(new TComplaint
                {
                    CustomerId = customer2.CustomerId,
                    AlternateId = 89,
                    Details = "Really! Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 28, 19, 22, 26)
                });

            var resolution = Add(new TResolution
                {
                    ResolutionId = SupportsCandidateKeys ? complaint2.AlternateId : complaint2.ComplaintId, 
                    Details = "Destroyed all coffee in Redmond area."
                });

            var login1 = Add(new TLogin { CustomerId = customer1.CustomerId, Username = "MrsKoalie73", AlternateUsername = "Sheila" });
            var login2 = Add(new TLogin { CustomerId = customer2.CustomerId, Username = "MrsBossyPants", AlternateUsername = "Sue" });
            var login3 = Add(new TLogin { CustomerId = customer3.CustomerId, Username = "TheStripedMenace", AlternateUsername = "Tarquin" });

            var suspiciousActivity1 = Add(new TSuspiciousActivity { Activity = "Pig prints on keyboard", Username = login3.Username });
            var suspiciousActivity2 = Add(new TSuspiciousActivity { Activity = "Crumbs in the cupboard", Username = login3.Username });
            var suspiciousActivity3 = Add(new TSuspiciousActivity { Activity = "Donuts gone missing", Username = login3.Username });

            var rsaToken1 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "1234", Username = login1.Username });
            var rsaToken2 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "2234", Username = login2.Username });

            var smartCard1 = Add(new TSmartCard { Username = login1.Username, CardSerial = rsaToken1.Serial, Issued = rsaToken1.Issued });
            var smartCard2 = Add(new TSmartCard { Username = login2.Username, CardSerial = rsaToken2.Serial, Issued = rsaToken2.Issued });

            var reset1 = Add(new TPasswordReset
                {
                    EmailedTo = "trent@example.com",
                    ResetNo = 1,
                    TempPassword = "Rent-A-Mole",
                    Username = SupportsCandidateKeys ? login3.AlternateUsername : login3.Username
                });

            var pageView1 = Add(new TPageView { PageUrl = "somePage1", Username = login1.Username, Viewed = DateTime.Now });
            var pageView2 = Add(new TPageView { PageUrl = "somePage2", Username = login1.Username, Viewed = DateTime.Now });
            var pageView3 = Add(new TPageView { PageUrl = "somePage3", Username = login1.Username, Viewed = DateTime.Now });

            var lastLogin1 = Add(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 10, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 11, 22, 26),
                    Username = login1.Username,
                    SmartcardUsername = smartCard1.Username
                });

            var lastLogin2 = Add(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 12, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 13, 22, 26),
                    Username = login2.Username,
                    SmartcardUsername = smartCard2.Username
                });

            var message1 = Add(new TMessage
                {
                    Subject = "Tea?",
                    Body = "Fancy a cup of tea?",
                    FromUsername = login1.Username,
                    ToUsername = login2.Username,
                    Sent = DateTime.Now,
                });

            var message2 = Add(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "Love one!",
                    FromUsername = login2.Username,
                    ToUsername = login1.Username,
                    Sent = DateTime.Now,
                });

            var message3 = Add(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "I'll put the kettle on.",
                    FromUsername = login1.Username,
                    ToUsername = login2.Username,
                    Sent = DateTime.Now,
                });

            var order1 = Add(new TAnOrder { CustomerId = customer1.CustomerId, Username = login1.Username, AlternateId = 77 });
            var order2 = Add(new TAnOrder { CustomerId = customer2.CustomerId, Username = login2.Username, AlternateId = 78 });
            var order3 = Add(new TAnOrder { CustomerId = customer3.CustomerId, Username = login3.Username, AlternateId = 79 });

            var orderNote1 = Add(new TOrderNote { Note = "Must have tea!", OrderId = SupportsCandidateKeys ? order1.AlternateId : order1.AnOrderId });
            var orderNote2 = Add(new TOrderNote { Note = "And donuts!", OrderId = SupportsCandidateKeys ? order1.AlternateId : order1.AnOrderId });
            var orderNote3 = Add(new TOrderNote { Note = "But no coffee. :-(", OrderId = SupportsCandidateKeys ? order1.AlternateId : order1.AnOrderId });

            var orderQualityCheck1 = Add(new TOrderQualityCheck { OrderId = SupportsCandidateKeys ? order1.AlternateId : order1.AnOrderId, CheckedBy = "Eeky Bear" });
            var orderQualityCheck2 = Add(new TOrderQualityCheck { OrderId = SupportsCandidateKeys ? order2.AlternateId : order2.AnOrderId, CheckedBy = "Eeky Bear" });
            var orderQualityCheck3 = Add(new TOrderQualityCheck { OrderId = SupportsCandidateKeys ? order3.AlternateId : order3.AnOrderId, CheckedBy = "Eeky Bear" });

            var orderLine1 = Add(new TOrderLine { OrderId = order1.AnOrderId, ProductId = product1.ProductId, Quantity = 7 });
            var orderLine2 = Add(new TOrderLine { OrderId = order1.AnOrderId, ProductId = product2.ProductId, Quantity = 1 });
            var orderLine3 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product3.ProductId, Quantity = 2 });
            var orderLine4 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product2.ProductId, Quantity = 3 });
            var orderLine5 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product1.ProductId, Quantity = 4 });
            var orderLine6 = Add(new TOrderLine { OrderId = order3.AnOrderId, ProductId = product2.ProductId, Quantity = 5 });

            var productDetail1 = Add(new TProductDetail { Details = "A Waffle Cart specialty!", ProductId = product1.ProductId });
            var productDetail2 = Add(new TProductDetail { Details = "Eeky Bear's favorite!", ProductId = product2.ProductId });

            var productReview1 = Add(new TProductReview { ProductId = product1.ProductId, Review = "Better than Tarqies!" });
            var productReview2 = Add(new TProductReview { ProductId = product1.ProductId, Review = "Good with maple syrup." });
            var productReview3 = Add(new TProductReview { ProductId = product2.ProductId, Review = "Eeky says yes!" });

            var productPhoto1 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 101, 102 } });
            var productPhoto2 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 103, 104 } });
            var productPhoto3 = Add(new TProductPhoto { ProductId = product3.ProductId, Photo = new byte[] { 105, 106 } });

            var productWebFeature1 = Add(new TProductWebFeature
                {
                    Heading = "Waffle Style",
                    PhotoId = productPhoto1.PhotoId,
                    ProductId = product1.ProductId,
                    ReviewId = productReview1.ReviewId
                });

            var productWebFeature2 = Add(new TProductWebFeature
                {
                    Heading = "What does the waffle say?",
                    ProductId = product2.ProductId,
                    ReviewId = productReview3.ReviewId
                });

            var supplier1 = Add(new TSupplier { Name = "Trading As Trent" });
            var supplier2 = Add(new TSupplier { Name = "Ants By Boris" });

            var supplierLogo1 = Add(new TSupplierLogo { SupplierId = supplier1.SupplierId, Logo = new byte[] { 201, 202 } });

            var supplierInfo1 = Add(new TSupplierInfo { SupplierId = supplier1.SupplierId, Information = "Seems a bit dodgy." });
            var supplierInfo2 = Add(new TSupplierInfo { SupplierId = supplier1.SupplierId, Information = "Orange fur?" });
            var supplierInfo3 = Add(new TSupplierInfo { SupplierId = supplier2.SupplierId, Information = "Very expensive!" });

            var customerInfo1 = Add(new TCustomerInfo { CustomerInfoId = customer1.CustomerId, Information = "Really likes tea." });
            var customerInfo2 = Add(new TCustomerInfo { CustomerInfoId = customer2.CustomerId, Information = "Mrs Bossy Pants!" });

            var computer1 = Add(new TComputer { Name = "markash420" });
            var computer2 = Add(new TComputer { Name = "unicorns420" });

            var computerDetail1 = Add(new TComputerDetail
                {
                    ComputerDetailId = computer1.ComputerId,
                    Manufacturer = "Dell",
                    Model = "420",
                    PurchaseDate = new DateTime(2008, 4, 1),
                    Serial = "4201",
                    Specifications = "It's a Dell!"
                });

            var computerDetail2 = Add(new TComputerDetail
                {
                    ComputerDetailId = computer2.ComputerId,
                    Manufacturer = "Not A Dell",
                    Model = "Not 420",
                    PurchaseDate = new DateTime(2012, 4, 1),
                    Serial = "4202",
                    Specifications = "It's not a Dell!"
                });

            var driver1 = Add(new TDriver { BirthDate = new DateTime(2006, 9, 19), Name = "Eeky Bear" });
            var driver2 = Add(new TDriver { BirthDate = new DateTime(2007, 9, 19), Name = "Splash Bear" });

            var license1 = Add(new TLicense
                {
                    Name = driver1.Name,
                    LicenseClass = "C",
                    LicenseNumber = "10",
                    Restrictions = "None",
                    State = LicenseState.Active,
                    ExpirationDate = new DateTime(2018, 9, 19)
                });

            var license2 = Add(new TLicense
                {
                    Name = driver2.Name,
                    LicenseClass = "A",
                    LicenseNumber = "11",
                    Restrictions = "None",
                    State = LicenseState.Revoked,
                    ExpirationDate = new DateTime(2018, 9, 19)
                });

            if (saveChanges)
            {
                SaveChanges();
            }
        }

        public override void SeedUsingNavigations(bool dependentNavs, bool principalNavs, bool saveChanges = true)
        {
            var customer0 = Add(new TCustomer { Name = "Eeky Bear" });
            var customer1 = Add(new TCustomer { Name = "Sheila Koalie" });
            var customer3 = Add(new TCustomer { Name = "Tarquin Tiger" });

            var customer2 = Add(new TCustomer { Name = "Sue Pandy", Husband = dependentNavs ? customer0 : null });
            if (principalNavs)
            {
                customer0.Wife = customer2;
            }

            var product1 = Add(new TProduct { Description = "Mrs Koalie's Famous Waffles", BaseConcurrency = "Pounds Sterling" });
            var product2 = Add(new TProduct { Description = "Chocolate Donuts", BaseConcurrency = "US Dollars" });
            var product3 = Add(new TProduct { Description = "Assorted Dog Treats", BaseConcurrency = "Stuffy Money" });

            var barcode1 = Add(new TBarcode { Code = new byte[] { 1, 2, 3, 4 }, Product = dependentNavs ? product1 : null, Text = "Barcode 1 2 3 4" });
            var barcode2 = Add(new TBarcode { Code = new byte[] { 2, 2, 3, 4 }, Product = dependentNavs ? product2 : null, Text = "Barcode 2 2 3 4" });
            var barcode3 = Add(new TBarcode { Code = new byte[] { 3, 2, 3, 4 }, Product = dependentNavs ? product3 : null, Text = "Barcode 3 2 3 4" });
            if (principalNavs)
            {
                product1.Barcodes.Add(barcode1);
                product2.Barcodes.Add(barcode2);
                product3.Barcodes.Add(barcode3);
            }

            var barcodeDetails1 = Add(new TBarcodeDetail { Code = barcode1.Code, RegisteredTo = "Eeky Bear" });
            var barcodeDetails2 = Add(new TBarcodeDetail { Code = barcode2.Code, RegisteredTo = "Trent" });
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
                    });
            if (principalNavs)
            {
                barcode2.BadScans.Add(incorrectScan1);
            }

            var incorrectScan2 = Add(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 15, 31),
                        Details = "Wot no waffles?",
                        ActualBarcode = barcode2,
                        ExpectedBarcode = dependentNavs ? barcode1 : null
                    });
            if (principalNavs)
            {
                barcode1.BadScans.Add(incorrectScan2);
            }

            var complaint1 = Add(new TComplaint
                {
                    Customer = customer2,
                    AlternateId = 88,
                    Details = "Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 27, 19, 22, 26)
                });

            var complaint2 = Add(new TComplaint
                {
                    Customer = customer2,
                    AlternateId = 89,
                    Details = "Really! Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 28, 19, 22, 26)
                });

            var resolution = Add(new TResolution { Complaint = dependentNavs ? complaint2 : null, Details = "Destroyed all coffee in Redmond area." });
            if (principalNavs)
            {
                complaint2.Resolution = resolution;
            }

            var login1 = Add(new TLogin { Customer = dependentNavs ? customer1 : null, Username = "MrsKoalie73", AlternateUsername = "Sheila" });
            var login2 = Add(new TLogin { Customer = dependentNavs ? customer2 : null, Username = "MrsBossyPants", AlternateUsername = "Sue" });
            var login3 = Add(new TLogin { Customer = dependentNavs ? customer3 : null, Username = "TheStripedMenace", AlternateUsername = "Tarquin" });
            if (principalNavs)
            {
                customer1.Logins.Add(login1);
                customer2.Logins.Add(login2);
                customer3.Logins.Add(login3);
            }

            var suspiciousActivity1 = Add(new TSuspiciousActivity { Activity = "Pig prints on keyboard", Username = login3.Username });
            var suspiciousActivity2 = Add(new TSuspiciousActivity { Activity = "Crumbs in the cupboard", Username = login3.Username });
            var suspiciousActivity3 = Add(new TSuspiciousActivity { Activity = "Donuts gone missing", Username = login3.Username });

            var rsaToken1 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "1234", Login = login1 });
            var rsaToken2 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "2234", Login = login2 });

            var smartCard1 = Add(new TSmartCard { Login = login1, CardSerial = rsaToken1.Serial, Issued = rsaToken1.Issued });
            var smartCard2 = Add(new TSmartCard { Login = login2, CardSerial = rsaToken2.Serial, Issued = rsaToken2.Issued });

            var reset1 = Add(new TPasswordReset
                {
                    EmailedTo = "trent@example.com",
                    ResetNo = 1,
                    TempPassword = "Rent-A-Mole",
                    Login = login3
                });

            var pageView1 = Add(new TPageView { PageUrl = "somePage1", Login = login1, Viewed = DateTime.Now });
            var pageView2 = Add(new TPageView { PageUrl = "somePage2", Login = login1, Viewed = DateTime.Now });
            var pageView3 = Add(new TPageView { PageUrl = "somePage3", Login = login1, Viewed = DateTime.Now });

            var lastLogin1 = Add(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 10, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 11, 22, 26),
                    Login = login1,
                    SmartcardUsername = smartCard1.Username
                });
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
                });
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
                    Sent = DateTime.Now,
                });
            if (principalNavs)
            {
                login1.SentMessages.Add(message1);
                login2.ReceivedMessages.Add(message1);
            }

            var message2 = Add(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "Love one!",
                    Sender = login2,
                    Recipient = dependentNavs ? login1 : null,
                    Sent = DateTime.Now,
                });
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
                    Sent = DateTime.Now,
                });
            if (principalNavs)
            {
                login1.SentMessages.Add(message3);
                login2.ReceivedMessages.Add(message3);
            }

            var order1 = Add(new TAnOrder { Customer = dependentNavs ? customer1 : null, Login = dependentNavs ? login1 : null, AlternateId = 77 });
            var order2 = Add(new TAnOrder { Customer = dependentNavs ? customer2 : null, Login = dependentNavs ? login2 : null, AlternateId = 78 });
            var order3 = Add(new TAnOrder { Customer = dependentNavs ? customer3 : null, Login = dependentNavs ? login3 : null, AlternateId = 79 });
            if (principalNavs)
            {
                customer1.Orders.Add(order1);
                customer2.Orders.Add(order2);
                customer3.Orders.Add(order3);
                login1.Orders.Add(order1);
                login2.Orders.Add(order2);
                login3.Orders.Add(order3);
            }

            var orderNote1 = Add(new TOrderNote { Note = "Must have tea!", Order = dependentNavs ? order1 : null });
            var orderNote2 = Add(new TOrderNote { Note = "And donuts!", Order = dependentNavs ? order1 : null });
            var orderNote3 = Add(new TOrderNote { Note = "But no coffee. :-(", Order = dependentNavs ? order1 : null });
            if (principalNavs)
            {
                order1.Notes.Add(orderNote1);
                order1.Notes.Add(orderNote2);
                order1.Notes.Add(orderNote3);
            }

            var orderQualityCheck1 = Add(new TOrderQualityCheck { Order = order1, CheckedBy = "Eeky Bear" });
            var orderQualityCheck2 = Add(new TOrderQualityCheck { Order = order2, CheckedBy = "Eeky Bear" });
            var orderQualityCheck3 = Add(new TOrderQualityCheck { Order = order3, CheckedBy = "Eeky Bear" });

            var orderLine1 = Add(new TOrderLine { Order = order1, Product = product1, Quantity = 7 });
            var orderLine2 = Add(new TOrderLine { Order = order1, Product = product2, Quantity = 1 });
            var orderLine3 = Add(new TOrderLine { Order = order2, Product = product3, Quantity = 2 });
            var orderLine4 = Add(new TOrderLine { Order = order2, Product = product2, Quantity = 3 });
            var orderLine5 = Add(new TOrderLine { Order = order2, Product = product1, Quantity = 4 });
            var orderLine6 = Add(new TOrderLine { Order = order3, Product = product2, Quantity = 5 });
            if (principalNavs)
            {
                order1.OrderLines.Add(orderLine1);
                order1.OrderLines.Add(orderLine2);
                order2.OrderLines.Add(orderLine3);
                order2.OrderLines.Add(orderLine4);
                order2.OrderLines.Add(orderLine5);
                order3.OrderLines.Add(orderLine6);
            }

            var productDetail1 = Add(new TProductDetail { Details = "A Waffle Cart specialty!", Product = product1 });
            var productDetail2 = Add(new TProductDetail { Details = "Eeky Bear's favorite!", Product = product2 });
            if (principalNavs)
            {
                product1.Detail = productDetail1;
                product2.Detail = productDetail2;
            }

            var productReview1 = Add(new TProductReview { Product = dependentNavs ? product1 : null, Review = "Better than Tarqies!" });
            var productReview2 = Add(new TProductReview { Product = dependentNavs ? product1 : null, Review = "Good with maple syrup." });
            var productReview3 = Add(new TProductReview { Product = dependentNavs ? product2 : null, Review = "Eeky says yes!" });
            if (principalNavs)
            {
                product1.Reviews.Add(productReview1);
                product1.Reviews.Add(productReview2);
                product2.Reviews.Add(productReview3);
            }

            var productPhoto1 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 101, 102 } });
            var productPhoto2 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 103, 104 } });
            var productPhoto3 = Add(new TProductPhoto { ProductId = product3.ProductId, Photo = new byte[] { 105, 106 } });
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
                });
            if (principalNavs)
            {
                productPhoto1.Features.Add(productWebFeature1);
                productReview1.Features.Add(productWebFeature1);
            }

            var productWebFeature2 = Add(new TProductWebFeature
                {
                    Heading = "What does the waffle say?",
                    ProductId = product2.ProductId,
                    Review = dependentNavs ? productReview3 : null
                });
            if (principalNavs)
            {
                productReview3.Features.Add(productWebFeature2);
            }

            var supplier1 = Add(new TSupplier { Name = "Trading As Trent" });
            var supplier2 = Add(new TSupplier { Name = "Ants By Boris" });

            var supplierLogo1 = Add(new TSupplierLogo { SupplierId = !principalNavs ? supplier1.SupplierId : 0, Logo = new byte[] { 201, 202 } });
            if (principalNavs)
            {
                supplier1.Logo = supplierLogo1;
            }

            var supplierInfo1 = Add(new TSupplierInfo { Supplier = supplier1, Information = "Seems a bit dodgy." });
            var supplierInfo2 = Add(new TSupplierInfo { Supplier = supplier1, Information = "Orange fur?" });
            var supplierInfo3 = Add(new TSupplierInfo { Supplier = supplier2, Information = "Very expensive!" });

            var customerInfo1 = Add(new TCustomerInfo { CustomerInfoId = customer1.CustomerId, Information = "Really likes tea." });
            var customerInfo2 = Add(new TCustomerInfo { CustomerInfoId = customer2.CustomerId, Information = "Mrs Bossy Pants!" });
            if (principalNavs)
            {
                customer1.Info = customerInfo1;
                customer2.Info = customerInfo2;
            }

            var computer1 = Add(new TComputer { Name = "markash420" });
            var computer2 = Add(new TComputer { Name = "unicorns420" });

            var computerDetail1 = Add(new TComputerDetail
                {
                    Computer = computer1,
                    Manufacturer = "Dell",
                    Model = "420",
                    PurchaseDate = new DateTime(2008, 4, 1),
                    Serial = "4201",
                    Specifications = "It's a Dell!"
                });
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
                });
            if (principalNavs)
            {
                computer2.ComputerDetail = computerDetail2;
            }

            var driver1 = Add(new TDriver { BirthDate = new DateTime(2006, 9, 19), Name = "Eeky Bear" });
            var driver2 = Add(new TDriver { BirthDate = new DateTime(2007, 9, 19), Name = "Splash Bear" });

            var license1 = Add(new TLicense
                {
                    Driver = driver1,
                    LicenseClass = "C",
                    LicenseNumber = "10",
                    Restrictions = "None",
                    State = LicenseState.Active,
                    ExpirationDate = new DateTime(2018, 9, 19)
                });
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
                });
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

            product1.Barcodes.Add(barcode1);
            product2.Barcodes.Add(barcode2);
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
            barcode2.BadScans.Add(incorrectScan1);

            var incorrectScan2 = toAdd[1].AddEx(
                new TIncorrectScan
                    {
                        ScanDate = new DateTime(2014, 5, 28, 19, 15, 31),
                        Details = "Wot no waffles?",
                        ActualBarcode = barcode2
                    });
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

            customer1.Logins.Add(login1);
            customer2.Logins.Add(login2);
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
                    LoggedOut = new DateTime(2014, 5, 27, 11, 22, 26),
                });

            login1.LastLogin = lastLogin1;
            smartCard1.LastLogin = lastLogin1;

            var lastLogin2 = toAdd[2].AddEx(new TLastLogin
                {
                    LoggedIn = new DateTime(2014, 5, 27, 12, 22, 26),
                    LoggedOut = new DateTime(2014, 5, 27, 13, 22, 26),
                });

            login2.LastLogin = lastLogin2;
            smartCard2.LastLogin = lastLogin2;

            var message1 = toAdd[2].AddEx(new TMessage
                {
                    Subject = "Tea?",
                    Body = "Fancy a cup of tea?",
                    Sent = DateTime.Now,
                });

            login1.SentMessages.Add(message1);
            login2.ReceivedMessages.Add(message1);

            var message2 = toAdd[2].AddEx(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "Love one!",
                    Sent = DateTime.Now,
                });

            login2.SentMessages.Add(message2);
            login1.ReceivedMessages.Add(message2);

            var message3 = toAdd[2].AddEx(new TMessage
                {
                    Subject = "Re: Tea?",
                    Body = "I'll put the kettle on.",
                    Sent = DateTime.Now,
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
            login3.Orders.Add(order3);

            var orderNote1 = toAdd[2].AddEx(new TOrderNote { Note = "Must have tea!" });
            var orderNote2 = toAdd[2].AddEx(new TOrderNote { Note = "And donuts!" });
            var orderNote3 = toAdd[2].AddEx(new TOrderNote { Note = "But no coffee. :-(" });

            order1.Notes.Add(orderNote1);
            order1.Notes.Add(orderNote2);
            order1.Notes.Add(orderNote3);

            var orderQualityCheck1 = toAdd[2].AddEx(new TOrderQualityCheck { Order = order1, CheckedBy = "Eeky Bear" });
            var orderQualityCheck2 = toAdd[2].AddEx(new TOrderQualityCheck { Order = order2, CheckedBy = "Eeky Bear" });
            var orderQualityCheck3 = toAdd[2].AddEx(new TOrderQualityCheck { Order = order3, CheckedBy = "Eeky Bear" });

            var orderLine1 = toAdd[3].AddEx(new TOrderLine { Product = product1, Quantity = 7 });
            var orderLine2 = toAdd[3].AddEx(new TOrderLine { Product = product2, Quantity = 1 });
            var orderLine3 = toAdd[3].AddEx(new TOrderLine { Product = product3, Quantity = 2 });
            var orderLine4 = toAdd[3].AddEx(new TOrderLine { Product = product2, Quantity = 3 });
            var orderLine5 = toAdd[3].AddEx(new TOrderLine { Product = product1, Quantity = 4 });
            var orderLine6 = toAdd[3].AddEx(new TOrderLine { Product = product2, Quantity = 5 });

            order1.OrderLines.Add(orderLine1);
            order1.OrderLines.Add(orderLine2);
            order2.OrderLines.Add(orderLine3);
            order2.OrderLines.Add(orderLine4);
            order2.OrderLines.Add(orderLine5);
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
                    ProductId = product1.ProductId,
                });

            productPhoto1.Features.Add(productWebFeature1);
            productReview1.Features.Add(productWebFeature1);

            var productWebFeature2 = toAdd[0].AddEx(new TProductWebFeature
                {
                    Heading = "What does the waffle say?",
                    ProductId = product2.ProductId,
                });

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
