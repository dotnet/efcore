// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        public MonsterContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TCustomer>();
            builder.Entity<TBarcode>().Key(e => e.Code);
            builder.Entity<TIncorrectScan>();
            builder.Entity<TBarcodeDetail>().Key(e => e.Code);
            builder.Entity<TComplaint>();
            builder.Entity<TResolution>();
            builder.Entity<TLogin>().Key(e => e.Username);
            builder.Entity<TSuspiciousActivity>();
            builder.Entity<TSmartCard>().Key(e => e.Username);
            builder.Entity<TRsaToken>().Key(e => e.Serial);
            builder.Entity<TPasswordReset>().Key(e => new { e.ResetNo, e.Username });
            builder.Entity<TPageView>();
            builder.Entity<TLastLogin>().Key(e => e.Username);
            builder.Entity<TMessage>().Key(e => new { e.MessageId, e.FromUsername });
            builder.Entity<TAnOrder>();
            builder.Entity<TOrderNote>().Key(e => e.NoteId);
            builder.Entity<TProduct>();
            builder.Entity<TOrderQualityCheck>().Key(e => e.OrderId);
            builder.Entity<TOrderLine>().Key(e => new { e.OrderId, e.ProductId });
            builder.Entity<TProductDetail>().Key(e => e.ProductId);
            builder.Entity<TProductReview>().Key(e => new { e.ProductId, e.ReviewId });
            builder.Entity<TProductPhoto>().Key(e => new { e.ProductId, e.PhotoId });
            builder.Entity<TProductWebFeature>().Key(e => e.ProductId);
            builder.Entity<TSupplier>();
            builder.Entity<TSupplierLogo>().Key(e => e.SupplierId);
            builder.Entity<TSupplierInfo>();
            builder.Entity<TCustomerInfo>();
            builder.Entity<TComputer>();
            builder.Entity<TComputerDetail>();
            builder.Entity<TDriver>().Key(e => e.Name);
            builder.Entity<TLicense>().Key(e => e.Name);

            builder.Entity<TComplaint>().ForeignKeys(fk => fk.ForeignKey<TCustomer>(e => e.CustomerId));
            builder.Entity<TMessage>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.FromUsername));
            builder.Entity<TMessage>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.ToUsername));
            builder.Entity<TCustomerInfo>().ForeignKeys(fk => fk.ForeignKey<TCustomer>(e => e.CustomerInfoId, isUnique: true));
            builder.Entity<TSupplierInfo>().ForeignKeys(fk => fk.ForeignKey<TSupplier>(e => e.SupplierId));
            builder.Entity<TAnOrder>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.Username));
            builder.Entity<TOrderNote>().ForeignKeys(fk => fk.ForeignKey<TAnOrder>(e => e.OrderId));
            builder.Entity<TOrderQualityCheck>().ForeignKeys(fk => fk.ForeignKey<TAnOrder>(e => e.OrderId, isUnique: true));
            builder.Entity<TSupplierLogo>().ForeignKeys(fk => fk.ForeignKey<TSupplier>(e => e.SupplierId, isUnique: true));
            builder.Entity<TAnOrder>().ForeignKeys(fk => fk.ForeignKey<TCustomer>(e => e.CustomerId));
            builder.Entity<TLogin>().ForeignKeys(fk => fk.ForeignKey<TCustomer>(e => e.CustomerId));
            builder.Entity<TLastLogin>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.Username, isUnique: true));
            builder.Entity<TLastLogin>().ForeignKeys(fk => fk.ForeignKey<TSmartCard>(e => e.SmartcardUsername, isUnique: true));
            builder.Entity<TOrderLine>().ForeignKeys(fk => fk.ForeignKey<TAnOrder>(e => e.OrderId));
            builder.Entity<TOrderLine>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ProductId));
            builder.Entity<TProductDetail>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ProductId, isUnique: true));
            builder.Entity<TProductReview>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ProductId));
            builder.Entity<TProductPhoto>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ProductId));
            builder.Entity<TProductWebFeature>().ForeignKeys(fk => fk.ForeignKey<TProductPhoto>(e => new { e.ProductId, e.PhotoId }));
            builder.Entity<TProductWebFeature>().ForeignKeys(fk => fk.ForeignKey<TProductReview>(e => new { e.ProductId, e.ReviewId }));
            builder.Entity<TResolution>().ForeignKeys(fk => fk.ForeignKey<TComplaint>(e => e.ResolutionId, isUnique: true));
            builder.Entity<TIncorrectScan>().ForeignKeys(fk => fk.ForeignKey<TBarcode>(e => e.ExpectedCode));
            builder.Entity<TCustomer>().ForeignKeys(fk => fk.ForeignKey<TCustomer>(e => e.CustomerId));
            builder.Entity<TCustomer>().ForeignKeys(fk => fk.ForeignKey<TCustomer>(e => e.HusbandId, isUnique: true));
            builder.Entity<TIncorrectScan>().ForeignKeys(fk => fk.ForeignKey<TBarcode>(e => e.ActualCode));
            builder.Entity<TBarcode>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ProductId));
            builder.Entity<TBarcodeDetail>().ForeignKeys(fk => fk.ForeignKey<TBarcode>(e => e.Code, isUnique: true));
            builder.Entity<TSuspiciousActivity>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.Username));
            builder.Entity<TRsaToken>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.Username, isUnique: true));
            builder.Entity<TSmartCard>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.Username, isUnique: true));
            builder.Entity<TPasswordReset>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.Username));
            builder.Entity<TPageView>().ForeignKeys(fk => fk.ForeignKey<TLogin>(e => e.Username));
            builder.Entity<TComputerDetail>().ForeignKeys(fk => fk.ForeignKey<TComputer>(e => e.ComputerDetailId, isUnique: true));
            builder.Entity<TLicense>().ForeignKeys(fk => fk.ForeignKey<TDriver>(e => e.Name, isUnique: true));

            // TODO: Many-to-many
            //builder.Entity<TSupplier>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.SupplierId));

            // TODO: Inheritance
            //builder.Entity<TBackOrderLine>().ForeignKeys(fk => fk.ForeignKey<TSupplier>(e => e.SupplierId));
            //builder.Entity<TDiscontinuedProduct>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ReplacementProductId));
            //builder.Entity<TProductPageView>().ForeignKeys(fk => fk.ForeignKey<TProduct>(e => e.ProductId));

            var model = builder.Model;

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

            // TODO: Use fluent API when available
            AddNavigationToPrincipal(model, typeof(TBarcode), "ProductId", "Product");
            AddNavigationToDependent(model, typeof(TBarcode), typeof(TIncorrectScan), "ExpectedCode", "BadScans");
            AddNavigationToDependent(model, typeof(TBarcode), typeof(TBarcodeDetail), "Code", "Detail");

            AddNavigationToPrincipal(model, typeof(TComplaint), "CustomerId", "Customer");
            AddNavigationToDependent(model, typeof(TComplaint), typeof(TResolution), "ResolutionId", "Resolution");

            AddNavigationToPrincipal(model, typeof(TComputerDetail), "ComputerDetailId", "Computer");
            AddNavigationToDependent(model, typeof(TComputer), typeof(TComputerDetail), "ComputerDetailId", "ComputerDetail");

            AddNavigationToDependent(model, typeof(TDriver), typeof(TLicense), "Name", "License");

            AddNavigationToPrincipal(model, typeof(TIncorrectScan), "ExpectedCode", "ExpectedBarcode");
            AddNavigationToPrincipal(model, typeof(TIncorrectScan), "ActualCode", "ActualBarcode");

            AddNavigationToPrincipal(model, typeof(TLastLogin), "Username", "Login");

            AddNavigationToPrincipal(model, typeof(TLicense), "Name", "Driver");

            AddNavigationToPrincipal(model, typeof(TMessage), "FromUsername", "Sender");
            AddNavigationToPrincipal(model, typeof(TMessage), "ToUsername", "Recipient");

            AddNavigationToPrincipal(model, typeof(TOrderLine), "OrderId", "Order");
            AddNavigationToPrincipal(model, typeof(TOrderLine), "ProductId", "Product");

            AddNavigationToPrincipal(model, typeof(TAnOrder), "CustomerId", "Customer");
            AddNavigationToPrincipal(model, typeof(TAnOrder), "Username", "Login");
            AddNavigationToDependent(model, typeof(TAnOrder), typeof(TOrderLine), "OrderId", "OrderLines");
            AddNavigationToDependent(model, typeof(TAnOrder), typeof(TOrderNote), "OrderId", "Notes");

            AddNavigationToPrincipal(model, typeof(TOrderNote), "OrderId", "Order");

            AddNavigationToPrincipal(model, typeof(TOrderQualityCheck), "OrderId", "Order");

            AddNavigationToPrincipal(model, typeof(TPageView), "Username", "Login");

            AddNavigationToPrincipal(model, typeof(TPasswordReset), "Username", "Login");

            AddNavigationToPrincipal(model, typeof(TProductDetail), "ProductId", "Product");

            AddNavigationToDependent(model, typeof(TProduct), typeof(TProductDetail), "ProductId", "Detail");
            AddNavigationToDependent(model, typeof(TProduct), typeof(TProductReview), "ProductId", "Reviews");
            AddNavigationToDependent(model, typeof(TProduct), typeof(TProductPhoto), "ProductId", "Photos");
            AddNavigationToDependent(model, typeof(TProduct), typeof(TBarcode), "ProductId", "Barcodes");

            AddNavigationToPrincipal(model, typeof(TProductWebFeature), "ProductId", "PhotoId", "Photo");
            AddNavigationToPrincipal(model, typeof(TProductWebFeature), "ProductId", "ReviewId", "Review");

            AddNavigationToDependent(model, typeof(TProductPhoto), typeof(TProductWebFeature), "ProductId", "PhotoId", "Features");

            AddNavigationToDependent(model, typeof(TProductReview), typeof(TProductWebFeature), "ProductId", "ReviewId", "Features");
            AddNavigationToPrincipal(model, typeof(TProductReview), "ProductId", "Product");

            AddNavigationToPrincipal(model, typeof(TResolution), "ResolutionId", "Complaint");

            AddNavigationToPrincipal(model, typeof(TRsaToken), "Username", "Login");

            AddNavigationToPrincipal(model, typeof(TSmartCard), "Username", "Login");
            AddNavigationToDependent(model, typeof(TSmartCard), typeof(TLastLogin), "SmartcardUsername", "LastLogin");

            AddNavigationToPrincipal(model, typeof(TSupplierInfo), "SupplierId", "Supplier");

            AddNavigationToDependent(model, typeof(TSupplier), typeof(TSupplierLogo), "SupplierId", "Logo");

            AddNavigationToDependent(model, typeof(TCustomer), typeof(TCustomerInfo), "CustomerInfoId", "Info");
            AddNavigationToDependent(model, typeof(TCustomer), typeof(TAnOrder), "CustomerId", "Orders");
            AddNavigationToDependent(model, typeof(TCustomer), typeof(TLogin), "CustomerId", "Logins");
            AddNavigationToPrincipal(model, typeof(TCustomer), "HusbandId", "Husband");
            AddNavigationToDependent(model, typeof(TCustomer), "HusbandId", "Wife");

            AddNavigationToPrincipal(model, typeof(TLogin), "CustomerId", "Customer");
            AddNavigationToDependent(model, typeof(TLogin), typeof(TLastLogin), "Username", "LastLogin");
            AddNavigationToDependent(model, typeof(TLogin), typeof(TMessage), "FromUsername", "SentMessages");
            AddNavigationToDependent(model, typeof(TLogin), typeof(TMessage), "ToUsername", "ReceivedMessages");
            AddNavigationToDependent(model, typeof(TLogin), typeof(TAnOrder), "Username", "Orders");
        }

        private static void AddNavigationToPrincipal(Model model, Type type, string fk, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    new Navigation(
                        model.GetEntityType(type).ForeignKeys.Single(
                            f => f.Properties.Count == 1 && f.Properties.Single().Name == fk),
                        navigation, pointsToPrincipal: true));
        }

        private static void AddNavigationToDependent(Model model, Type type, string fk, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    new Navigation(
                        model.GetEntityType(type).ForeignKeys.Single(
                            f => f.Properties.Count == 1 && f.Properties.Single().Name == fk),
                        navigation, pointsToPrincipal: false));
        }

        private static void AddNavigationToDependent(Model model, Type type, Type dependentType, string fk, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    new Navigation(
                        model.GetEntityType(dependentType).ForeignKeys.Single(
                            f => f.Properties.Count == 1 && f.Properties.Single().Name == fk),
                        navigation, pointsToPrincipal: false));
        }

        private static void AddNavigationToDependent(Model model, Type type, Type dependentType, string fk1, string fk2, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    new Navigation(
                        model.GetEntityType(dependentType).ForeignKeys.Single(
                            f => f.Properties.Count == 2
                                 && f.Properties.Any(p => p.Name == fk1)
                                 && f.Properties.Any(p => p.Name == fk2)),
                        navigation, pointsToPrincipal: false));
        }

        private static void AddNavigationToPrincipal(Model model, Type type, string fk1, string fk2, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    new Navigation(
                        model.GetEntityType(type).ForeignKeys.Single(
                            f => f.Properties.Count == 2
                                 && f.Properties.Any(p => p.Name == fk1)
                                 && f.Properties.Any(p => p.Name == fk2)),
                        navigation, pointsToPrincipal: true));
        }

        public override void SeedUsingFKs()
        {
            var customer0 = Add(new TCustomer { Name = "Eeky Bear" });
            var customer1 = Add(new TCustomer { Name = "Sheila Koalie" });
            var customer3 = Add(new TCustomer { Name = "Tarquin Tiger" });

            // TODO: Key propagation so all the additional SaveChanges calls can be removed
            SaveChanges();

            var customer2 = Add(new TCustomer { Name = "Sue Pandy", HusbandId = customer0.CustomerId });

            SaveChanges();

            var product1 = Add(new TProduct { Description = "Mrs Koalie's Famous Waffles", BaseConcurrency = "Pounds Sterling" });
            var product2 = Add(new TProduct { Description = "Chocolate Donuts", BaseConcurrency = "US Dollars" });
            var product3 = Add(new TProduct { Description = "Assorted Dog Treats", BaseConcurrency = "Stuffy Money" });

            SaveChanges();

            var barcode1 = Add(new TBarcode { Code = new byte[] { 1, 2, 3, 4 }, ProductId = product1.ProductId, Text = "Barcode 1 2 3 4" });
            var barcode2 = Add(new TBarcode { Code = new byte[] { 2, 2, 3, 4 }, ProductId = product2.ProductId, Text = "Barcode 2 2 3 4" });
            var barcode3 = Add(new TBarcode { Code = new byte[] { 3, 2, 3, 4 }, ProductId = product3.ProductId, Text = "Barcode 3 2 3 4" });

            SaveChanges();

            var barcodeDetails1 = Add(new TBarcodeDetail { Code = barcode1.Code, RegisteredTo = "Eeky Bear" });
            var barcodeDetails2 = Add(new TBarcodeDetail { Code = barcode2.Code, RegisteredTo = "Trent" });

            SaveChanges();

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

            SaveChanges();

            var complaint1 = Add(new TComplaint
                {
                    CustomerId = customer2.CustomerId,
                    Details = "Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 27, 19, 22, 26)
                });

            var complaint2 = Add(new TComplaint
                {
                    CustomerId = customer2.CustomerId,
                    Details = "Really! Don't give coffee to Eeky!",
                    Logged = new DateTime(2014, 5, 28, 19, 22, 26)
                });

            SaveChanges();

            var resolution = Add(new TResolution { ResolutionId = complaint2.ComplaintId, Details = "Destroyed all coffee in Redmond area." });

            SaveChanges();

            var login1 = Add(new TLogin { CustomerId = customer1.CustomerId, Username = "MrsKoalie73" });
            var login2 = Add(new TLogin { CustomerId = customer2.CustomerId, Username = "MrsBossyPants" });
            var login3 = Add(new TLogin { CustomerId = customer3.CustomerId, Username = "TheStripedMenace" });

            SaveChanges();

            var suspiciousActivity1 = Add(new TSuspiciousActivity { Activity = "Pig prints on keyboard", Username = login3.Username });
            var suspiciousActivity2 = Add(new TSuspiciousActivity { Activity = "Crumbs in the cupboard", Username = login3.Username });
            var suspiciousActivity3 = Add(new TSuspiciousActivity { Activity = "Donuts gone missing", Username = login3.Username });

            SaveChanges();

            var rsaToken1 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "1234", Username = login1.Username });
            var rsaToken2 = Add(new TRsaToken { Issued = DateTime.Now, Serial = "2234", Username = login2.Username });

            SaveChanges();

            var smartCard1 = Add(new TSmartCard { Username = login1.Username, CardSerial = rsaToken1.Serial, Issued = rsaToken1.Issued });
            var smartCard2 = Add(new TSmartCard { Username = login2.Username, CardSerial = rsaToken2.Serial, Issued = rsaToken2.Issued });

            SaveChanges();

            var reset1 = Add(new TPasswordReset
                {
                    EmailedTo = "trent@example.com",
                    ResetNo = 1,
                    TempPassword = "Rent-A-Mole",
                    Username = login3.Username
                });

            SaveChanges();

            var pageView1 = Add(new TPageView { PageUrl = "somePage1", Username = login1.Username, Viewed = DateTime.Now });
            var pageView2 = Add(new TPageView { PageUrl = "somePage2", Username = login1.Username, Viewed = DateTime.Now });
            var pageView3 = Add(new TPageView { PageUrl = "somePage3", Username = login1.Username, Viewed = DateTime.Now });

            SaveChanges();

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

            SaveChanges();

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

            SaveChanges();

            var order1 = Add(new TAnOrder { CustomerId = customer1.CustomerId, Username = login1.Username });
            var order2 = Add(new TAnOrder { CustomerId = customer2.CustomerId, Username = login2.Username });
            var order3 = Add(new TAnOrder { CustomerId = customer3.CustomerId, Username = login3.Username });

            SaveChanges();

            var orderNote1 = Add(new TOrderNote { Note = "Must have tea!", OrderId = order1.AnOrderId });
            var orderNote2 = Add(new TOrderNote { Note = "And donuts!", OrderId = order1.AnOrderId });
            var orderNote3 = Add(new TOrderNote { Note = "But no coffee. :-(", OrderId = order1.AnOrderId });

            SaveChanges();

            var orderQualityCheck1 = Add(new TOrderQualityCheck { OrderId = order1.AnOrderId, CheckedBy = "Eeky Bear" });
            var orderQualityCheck2 = Add(new TOrderQualityCheck { OrderId = order2.AnOrderId, CheckedBy = "Eeky Bear" });
            var orderQualityCheck3 = Add(new TOrderQualityCheck { OrderId = order3.AnOrderId, CheckedBy = "Eeky Bear" });

            SaveChanges();

            var orderLine1 = Add(new TOrderLine { OrderId = order1.AnOrderId, ProductId = product1.ProductId, Quantity = 7 });
            var orderLine2 = Add(new TOrderLine { OrderId = order1.AnOrderId, ProductId = product2.ProductId, Quantity = 1 });
            var orderLine3 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product3.ProductId, Quantity = 2 });
            var orderLine4 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product2.ProductId, Quantity = 3 });
            var orderLine5 = Add(new TOrderLine { OrderId = order2.AnOrderId, ProductId = product1.ProductId, Quantity = 4 });
            var orderLine6 = Add(new TOrderLine { OrderId = order3.AnOrderId, ProductId = product2.ProductId, Quantity = 5 });

            SaveChanges();

            var productDetail1 = Add(new TProductDetail { Details = "A Waffle Cart specialty!", ProductId = product1.ProductId });
            var productDetail2 = Add(new TProductDetail { Details = "Eeky Bear's favorite!", ProductId = product2.ProductId });

            SaveChanges();

            var productReview1 = Add(new TProductReview { ProductId = product1.ProductId, Review = "Better than Tarqies!" });
            var productReview2 = Add(new TProductReview { ProductId = product1.ProductId, Review = "Good with maple syrup." });
            var productReview3 = Add(new TProductReview { ProductId = product2.ProductId, Review = "Eeky says yes!" });

            SaveChanges();

            var productPhoto1 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 101, 102 } });
            var productPhoto2 = Add(new TProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 103, 104 } });
            var productPhoto3 = Add(new TProductPhoto { ProductId = product3.ProductId, Photo = new byte[] { 105, 106 } });

            SaveChanges();

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

            SaveChanges();

            var supplier1 = Add(new TSupplier { Name = "Trading As Trent" });
            var supplier2 = Add(new TSupplier { Name = "Ants By Boris" });

            SaveChanges();

            var supplierLogo1 = Add(new TSupplierLogo { SupplierId = supplier1.SupplierId, Logo = new byte[] { 201, 202 } });

            SaveChanges();

            var supplierInfo1 = Add(new TSupplierInfo { SupplierId = supplier1.SupplierId, Information = "Seems a bit dodgy." });
            var supplierInfo2 = Add(new TSupplierInfo { SupplierId = supplier1.SupplierId, Information = "Orange fur?" });
            var supplierInfo3 = Add(new TSupplierInfo { SupplierId = supplier2.SupplierId, Information = "Very expensive!" });

            SaveChanges();

            var customerInfo1 = Add(new TCustomerInfo { CustomerInfoId = customer1.CustomerId, Information = "Really likes tea." });
            var customerInfo2 = Add(new TCustomerInfo { CustomerInfoId = customer2.CustomerId, Information = "Mrs Bossy Pants!" });

            SaveChanges();

            var computer1 = Add(new TComputer { Name = "markash420" });
            var computer2 = Add(new TComputer { Name = "unicorns420" });

            SaveChanges();

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

            SaveChanges();

            var driver1 = Add(new TDriver { BirthDate = new DateTime(2006, 9, 19), Name = "Eeky Bear" });
            var driver2 = Add(new TDriver { BirthDate = new DateTime(2007, 9, 19), Name = "Splash Bear" });

            SaveChanges();

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

            SaveChanges();
        }
    }
}
