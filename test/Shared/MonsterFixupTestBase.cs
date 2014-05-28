// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class MonsterFixupTestBase
    {
        [Fact]
        public void Can_build_monster_model_and_seed_data_using_FKs()
        {
            var serviceProvider = CreateServiceProvider();

            using (var context = new MonsterContext(serviceProvider, CreateOptions("Monster")))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.SeedUsingFKs();
            }

            SimpleVerification(serviceProvider, "Monster");

            FkVerification(serviceProvider, "Monster");
        }

        protected void SimpleVerification(IServiceProvider serviceProvider, string databaseName)
        {
            using (var context = new MonsterContext(serviceProvider, CreateOptions(databaseName)))
            {
                Assert.Equal(
                    new[] { "Sheila Koalie", "Sue Pandy", "Tarquin Tiger" },
                    context.Customers.Select(c => c.Name).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Assorted Dog Treats", "Chocolate Donuts", "Mrs Koalie's Famous Waffles" },
                    context.Products.Select(c => c.Description).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Barcode 1 2 3 4", "Barcode 2 2 3 4", "Barcode 3 2 3 4" },
                    context.Barcodes.Select(c => c.Text).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Barcode 1 2 3 4", "Barcode 2 2 3 4", "Barcode 3 2 3 4" },
                    context.Barcodes.Select(c => c.Text).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Eeky Bear", "Trent" },
                    context.BarcodeDetails.Select(c => c.RegisteredTo).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Treats not Donuts", "Wot no waffles?" },
                    context.IncorrectScans.Select(c => c.Details).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Don't give coffee to Eeky!", "Really! Don't give coffee to Eeky!" },
                    context.Complaints.Select(c => c.Details).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Destroyed all coffee in Redmond area." },
                    context.Resolutions.Select(c => c.Details).OrderBy(n => n));

                Assert.Equal(
                    new[] { "MrsBossyPants", "MrsKoalie73", "TheStripedMenace" },
                    context.Logins.Select(c => c.Username).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Crumbs in the cupboard", "Donuts gone missing", "Pig prints on keyboard" },
                    context.SuspiciousActivities.Select(c => c.Activity).OrderBy(n => n));

                Assert.Equal(
                    new[] { "1234", "2234" },
                    context.RsaTokens.Select(c => c.Serial).OrderBy(n => n));

                Assert.Equal(
                    new[] { "MrsBossyPants", "MrsKoalie73" },
                    context.SmartCards.Select(c => c.Username).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Rent-A-Mole" },
                    context.PasswordResets.Select(c => c.TempPassword).OrderBy(n => n));

                Assert.Equal(
                    new[] { "somePage1", "somePage2", "somePage3" },
                    context.PageViews.Select(c => c.PageUrl).OrderBy(n => n));

                Assert.Equal(
                    new[] { "MrsBossyPants", "MrsKoalie73" },
                    context.LastLogins.Select(c => c.Username).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Fancy a cup of tea?", "I'll put the kettle on.", "Love one!" },
                    context.Messages.Select(c => c.Body).OrderBy(n => n));

                Assert.Equal(
                    new[] { "MrsBossyPants", "MrsKoalie73", "TheStripedMenace" },
                    context.Orders.Select(c => c.Username).OrderBy(n => n));

                Assert.Equal(
                    new[] { "And donuts!", "But no coffee. :-(", "Must have tea!" },
                    context.OrderNotes.Select(c => c.Note).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Eeky Bear", "Eeky Bear", "Eeky Bear" },
                    context.OrderQualityChecks.Select(c => c.CheckedBy).OrderBy(n => n));

                Assert.Equal(
                    new[] { 1, 2, 3, 4, 5, 7 },
                    context.OrderLines.Select(c => c.Quantity).OrderBy(n => n));

                Assert.Equal(
                    new[] { "A Waffle Cart specialty!", "Eeky Bear's favorite!" },
                    context.ProductDetails.Select(c => c.Details).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Better than Tarqies!", "Eeky says yes!", "Good with maple syrup." },
                    context.ProductReviews.Select(c => c.Review).OrderBy(n => n));

                // TODO: Remove ToArray once LINQ to EF7 can support this query
                Assert.Equal(
                    new[] { "101", "103", "105" },
                    context.ProductPhotos.ToArray().Select(c => c.Photo.First().ToString()).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Waffle Style", "What does the waffle say?" },
                    context.ProductWebFeatures.Select(c => c.Heading).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Ants By Boris", "Trading As Trent" },
                    context.Suppliers.Select(c => c.Name).OrderBy(n => n));

                Assert.Equal(
                    new[] { "201", "202" },
                    context.SupplierLogos.SelectMany(c => c.Logo).Select(l => l.ToString()).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Orange fur?", "Seems a bit dodgy.", "Very expensive!" },
                    context.SupplierInformation.Select(c => c.Information).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Mrs Bossy Pants!", "Really likes tea." },
                    context.CustomerInformation.Select(c => c.Information).OrderBy(n => n));

                Assert.Equal(
                    new[] { "markash420", "unicorns420" },
                    context.Computers.Select(c => c.Name).OrderBy(n => n));

                Assert.Equal(
                    new[] { "It's a Dell!", "It's not a Dell!" },
                    context.ComputerDetails.Select(c => c.Specifications).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Eeky Bear", "Splash Bear" },
                    context.Drivers.Select(c => c.Name).OrderBy(n => n));

                Assert.Equal(
                    new[] { "10", "11" },
                    context.Licenses.Select(c => c.LicenseNumber).OrderBy(n => n));
            }
        }

        protected void FkVerification(IServiceProvider serviceProvider, string databaseName)
        {
            using (var context = new MonsterContext(serviceProvider, CreateOptions(databaseName)))
            {
                var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
                var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
                var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

                var product1 = context.Products.Single(e => e.Description.StartsWith("Mrs"));
                var product2 = context.Products.Single(e => e.Description.StartsWith("Chocolate"));
                var product3 = context.Products.Single(e => e.Description.StartsWith("Assorted"));

                var barcode1 = context.Barcodes.Single(e => e.Text == "Barcode 1 2 3 4");
                var barcode2 = context.Barcodes.Single(e => e.Text == "Barcode 2 2 3 4");
                var barcode3 = context.Barcodes.Single(e => e.Text == "Barcode 3 2 3 4");

                Assert.Equal(product1.ProductId, barcode1.ProductId);
                Assert.Equal(product2.ProductId, barcode2.ProductId);
                Assert.Equal(product3.ProductId, barcode3.ProductId);

                var barcodeDetails1 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Eeky Bear");
                var barcodeDetails2 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Trent");

                Assert.Equal(barcode1.Code, barcodeDetails1.Code);
                Assert.Equal(barcode2.Code, barcodeDetails2.Code);

                var incorrectScan1 = context.IncorrectScans.Single(e => e.Details.StartsWith("Treats"));
                var incorrectScan2 = context.IncorrectScans.Single(e => e.Details.StartsWith("Wot"));

                Assert.Equal(barcode3.Code, incorrectScan1.ActualCode);
                Assert.Equal(barcode2.Code, incorrectScan1.ExpectedCode);
                Assert.Equal(barcode2.Code, incorrectScan2.ActualCode);
                Assert.Equal(barcode1.Code, incorrectScan2.ExpectedCode);

                var complaint1 = context.Complaints.Single(e => e.Details.StartsWith("Don't"));
                var complaint2 = context.Complaints.Single(e => e.Details.StartsWith("Really"));

                Assert.Equal(customer2.CustomerId, complaint1.CustomerId);
                Assert.Equal(customer2.CustomerId, complaint2.CustomerId);

                var resolution = context.Resolutions.Single(e => e.Details.StartsWith("Destroyed"));

                Assert.Equal(complaint2.ComplaintId, resolution.ResolutionId);

                var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
                var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
                var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

                Assert.Equal(customer1.CustomerId, login1.CustomerId);
                Assert.Equal(customer2.CustomerId, login2.CustomerId);
                Assert.Equal(customer3.CustomerId, login3.CustomerId);

                var suspiciousActivity1 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Pig"));
                var suspiciousActivity2 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Crumbs"));
                var suspiciousActivity3 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Donuts"));

                Assert.Equal(login3.Username, suspiciousActivity1.Username);
                Assert.Equal(login3.Username, suspiciousActivity2.Username);
                Assert.Equal(login3.Username, suspiciousActivity3.Username);

                var rsaToken1 = context.RsaTokens.Single(e => e.Serial == "1234");
                var rsaToken2 = context.RsaTokens.Single(e => e.Serial == "2234");

                Assert.Equal(login1.Username, rsaToken1.Username);
                Assert.Equal(login2.Username, rsaToken2.Username);

                var smartCard1 = context.SmartCards.Single(e => e.Username == "MrsKoalie73");
                var smartCard2 = context.SmartCards.Single(e => e.Username == "MrsBossyPants");

                Assert.Equal(rsaToken1.Serial, smartCard1.CardSerial);
                Assert.Equal(rsaToken2.Serial, smartCard2.CardSerial);
                Assert.Equal(rsaToken1.Issued, smartCard1.Issued);
                Assert.Equal(rsaToken2.Issued, smartCard2.Issued);

                var reset1 = context.PasswordResets.Single(e => e.EmailedTo == "trent@example.com");

                Assert.Equal(login3.Username, reset1.Username);

                var pageView1 = context.PageViews.Single(e => e.PageUrl == "somePage1");
                var pageView2 = context.PageViews.Single(e => e.PageUrl == "somePage1");
                var pageView3 = context.PageViews.Single(e => e.PageUrl == "somePage1");

                Assert.Equal(login1.Username, pageView1.Username);
                Assert.Equal(login1.Username, pageView2.Username);
                Assert.Equal(login1.Username, pageView3.Username);

                var lastLogin1 = context.LastLogins.Single(e => e.Username == "MrsKoalie73");
                var lastLogin2 = context.LastLogins.Single(e => e.Username == "MrsBossyPants");

                Assert.Equal(smartCard1.Username, lastLogin1.SmartcardUsername);
                Assert.Equal(smartCard2.Username, lastLogin2.SmartcardUsername);

                var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
                var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
                var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login2.Username, message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);

                var order1 = context.Orders.Single(e => e.Username == "MrsKoalie73");
                var order2 = context.Orders.Single(e => e.Username == "MrsBossyPants");
                var order3 = context.Orders.Single(e => e.Username == "TheStripedMenace");

                Assert.Equal(customer1.CustomerId, order1.CustomerId);
                Assert.Equal(customer2.CustomerId, order2.CustomerId);
                Assert.Equal(customer3.CustomerId, order3.CustomerId);

                var orderLine1 = context.OrderLines.Single(e => e.Quantity == 7);
                var orderLine2 = context.OrderLines.Single(e => e.Quantity == 1);
                var orderLine3 = context.OrderLines.Single(e => e.Quantity == 2);
                var orderLine4 = context.OrderLines.Single(e => e.Quantity == 3);
                var orderLine5 = context.OrderLines.Single(e => e.Quantity == 4);
                var orderLine6 = context.OrderLines.Single(e => e.Quantity == 5);

                Assert.Equal(product1.ProductId, orderLine1.ProductId);
                Assert.Equal(product2.ProductId, orderLine2.ProductId);
                Assert.Equal(product3.ProductId, orderLine3.ProductId);
                Assert.Equal(product2.ProductId, orderLine4.ProductId);
                Assert.Equal(product1.ProductId, orderLine5.ProductId);
                Assert.Equal(product2.ProductId, orderLine6.ProductId);
                Assert.Equal(order1.AnOrderId, orderLine1.OrderId);
                Assert.Equal(order1.AnOrderId, orderLine2.OrderId);
                Assert.Equal(order2.AnOrderId, orderLine3.OrderId);
                Assert.Equal(order2.AnOrderId, orderLine4.OrderId);
                Assert.Equal(order2.AnOrderId, orderLine5.OrderId);
                Assert.Equal(order3.AnOrderId, orderLine6.OrderId);

                var productDetail1 = context.ProductDetails.Single(e => e.Details.StartsWith("A"));
                var productDetail2 = context.ProductDetails.Single(e => e.Details.StartsWith("Eeky"));

                Assert.Equal(product1.ProductId, productDetail1.ProductId);
                Assert.Equal(product2.ProductId, productDetail2.ProductId);

                var productReview1 = context.ProductReviews.Single(e => e.Review.StartsWith("Better"));
                var productReview2 = context.ProductReviews.Single(e => e.Review.StartsWith("Good"));
                var productReview3 = context.ProductReviews.Single(e => e.Review.StartsWith("Eeky"));

                Assert.Equal(product1.ProductId, productReview1.ProductId);
                Assert.Equal(product1.ProductId, productReview2.ProductId);
                Assert.Equal(product2.ProductId, productReview3.ProductId);

                var productPhoto1 = context.ProductPhotos.Single(e => e.Photo[0] == 101);
                var productPhoto2 = context.ProductPhotos.Single(e => e.Photo[0] == 103);
                var productPhoto3 = context.ProductPhotos.Single(e => e.Photo[0] == 105);

                Assert.Equal(product1.ProductId, productPhoto1.ProductId);
                Assert.Equal(product1.ProductId, productPhoto2.ProductId);
                Assert.Equal(product3.ProductId, productPhoto3.ProductId);

                var productWebFeature1 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("Waffle"));
                var productWebFeature2 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("What"));

                Assert.Equal(product1.ProductId, productWebFeature1.ProductId);
                Assert.Equal(product2.ProductId, productWebFeature2.ProductId);
                Assert.Equal(productPhoto1.PhotoId, productWebFeature1.PhotoId);
                Assert.Null(productWebFeature2.PhotoId);
                Assert.Equal(productReview1.ReviewId, productWebFeature1.ReviewId);
                Assert.Equal(productReview3.ReviewId, productWebFeature2.ReviewId);

                var supplier1 = context.Suppliers.Single(e => e.Name.StartsWith("Trading"));
                var supplier2 = context.Suppliers.Single(e => e.Name.StartsWith("Ants"));

                var supplierLogo1 = context.SupplierLogos.Single(e => e.Logo[0] == 201);

                Assert.Equal(supplier1.SupplierId, supplierLogo1.SupplierId);

                var supplierInfo1 = context.SupplierInformation.Single(e => e.Information.StartsWith("Seems"));
                var supplierInfo2 = context.SupplierInformation.Single(e => e.Information.StartsWith("Orange"));
                var supplierInfo3 = context.SupplierInformation.Single(e => e.Information.StartsWith("Very"));

                Assert.Equal(supplier1.SupplierId, supplierInfo1.SupplierId);
                Assert.Equal(supplier1.SupplierId, supplierInfo2.SupplierId);
                Assert.Equal(supplier2.SupplierId, supplierInfo3.SupplierId);

                var customerInfo1 = context.CustomerInformation.Single(e => e.Information.StartsWith("Really"));
                var customerInfo2 = context.CustomerInformation.Single(e => e.Information.StartsWith("Mrs"));

                Assert.Equal(customer1.CustomerId, customerInfo1.CustomerInfoId);
                Assert.Equal(customer2.CustomerId, customerInfo2.CustomerInfoId);

                var computer1 = context.Computers.Single(e => e.Name == "markash420");
                var computer2 = context.Computers.Single(e => e.Name == "unicorns420");

                var computerDetail1 = context.ComputerDetails.Single(e => e.Specifications == "It's a Dell!");
                var computerDetail2 = context.ComputerDetails.Single(e => e.Specifications == "It's not a Dell!");

                Assert.Equal(computer1.ComputerId, computerDetail1.ComputerDetailId);
                Assert.Equal(computer2.ComputerId, computerDetail2.ComputerDetailId);

                var driver1 = context.Drivers.Single(e => e.Name == "Eeky Bear");
                var driver2 = context.Drivers.Single(e => e.Name == "Splash Bear");

                // TODO: Quering for actual entity currently throws, so projecting to just FK instead
                var licenseName1 = context.Licenses.Where(e => e.LicenseNumber == "10").Select(e => e.Name).Single();
                var licenseName2 = context.Licenses.Where(e => e.LicenseNumber == "11").Select(e => e.Name).Single();

                Assert.Equal(driver1.Name, licenseName1);
                Assert.Equal(driver2.Name, licenseName2);
            }
        }

        protected abstract IServiceProvider CreateServiceProvider();

        protected abstract DbContextOptions CreateOptions(string databaseName);

        protected class MonsterContext : DbContext
        {
            public MonsterContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Barcode> Barcodes { get; set; }
            public DbSet<IncorrectScan> IncorrectScans { get; set; }
            public DbSet<BarcodeDetail> BarcodeDetails { get; set; }
            public DbSet<Complaint> Complaints { get; set; }
            public DbSet<Resolution> Resolutions { get; set; }
            public DbSet<Login> Logins { get; set; }
            public DbSet<SuspiciousActivity> SuspiciousActivities { get; set; }
            public DbSet<SmartCard> SmartCards { get; set; }
            public DbSet<RsaToken> RsaTokens { get; set; }
            public DbSet<PasswordReset> PasswordResets { get; set; }
            public DbSet<PageView> PageViews { get; set; }
            public DbSet<LastLogin> LastLogins { get; set; }
            public DbSet<Message> Messages { get; set; }
            public DbSet<AnOrder> Orders { get; set; }
            public DbSet<OrderNote> OrderNotes { get; set; }
            public DbSet<OrderQualityCheck> OrderQualityChecks { get; set; }
            public DbSet<OrderLine> OrderLines { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<ProductDetail> ProductDetails { get; set; }
            public DbSet<ProductReview> ProductReviews { get; set; }
            public DbSet<ProductPhoto> ProductPhotos { get; set; }
            public DbSet<ProductWebFeature> ProductWebFeatures { get; set; }
            public DbSet<Supplier> Suppliers { get; set; }
            public DbSet<SupplierLogo> SupplierLogos { get; set; }
            public DbSet<SupplierInfo> SupplierInformation { get; set; }
            public DbSet<CustomerInfo> CustomerInformation { get; set; }
            public DbSet<Computer> Computers { get; set; }
            public DbSet<ComputerDetail> ComputerDetails { get; set; }
            public DbSet<Driver> Drivers { get; set; }
            public DbSet<License> Licenses { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<Barcode>().Key(e => e.Code);
                builder.Entity<BarcodeDetail>().Key(e => e.Code);
                builder.Entity<Login>().Key(e => e.Username);
                builder.Entity<SmartCard>().Key(e => e.Username);
                builder.Entity<RsaToken>().Key(e => e.Serial);
                builder.Entity<PasswordReset>().Key(e => new { e.ResetNo, e.Username });
                builder.Entity<LastLogin>().Key(e => e.Username);
                builder.Entity<Message>().Key(e => new { e.MessageId, e.FromUsername });
                builder.Entity<OrderNote>().Key(e => e.NoteId);
                builder.Entity<OrderQualityCheck>().Key(e => e.OrderId);
                builder.Entity<OrderLine>().Key(e => new { e.OrderId, e.ProductId });
                builder.Entity<ProductDetail>().Key(e => e.ProductId);
                builder.Entity<ProductReview>().Key(e => new { e.ProductId, e.ReviewId });
                builder.Entity<ProductPhoto>().Key(e => new { e.ProductId, e.PhotoId });
                builder.Entity<ProductWebFeature>().Key(e => e.ProductId);
                builder.Entity<SupplierLogo>().Key(e => e.SupplierId);
                builder.Entity<Driver>().Key(e => e.Name);
                builder.Entity<License>().Key(e => e.Name);

                builder.Entity<Complaint>().ForeignKeys(fk => fk.ForeignKey<Customer>(e => e.CustomerId));
                builder.Entity<Message>().ForeignKeys(fk => fk.ForeignKey<Login>(e => e.FromUsername));
                builder.Entity<CustomerInfo>().ForeignKeys(fk => fk.ForeignKey<Customer>(e => e.CustomerInfoId));
                builder.Entity<SupplierInfo>().ForeignKeys(fk => fk.ForeignKey<Supplier>(e => e.SupplierId));
                builder.Entity<AnOrder>().ForeignKeys(fk => fk.ForeignKey<Login>(e => e.Username));
                builder.Entity<OrderNote>().ForeignKeys(fk => fk.ForeignKey<AnOrder>(e => e.OrderId));
                builder.Entity<OrderQualityCheck>().ForeignKeys(fk => fk.ForeignKey<AnOrder>(e => e.OrderId));
                builder.Entity<SupplierLogo>().ForeignKeys(fk => fk.ForeignKey<Supplier>(e => e.SupplierId));
                builder.Entity<AnOrder>().ForeignKeys(fk => fk.ForeignKey<Customer>(e => e.CustomerId));
                builder.Entity<Login>().ForeignKeys(fk => fk.ForeignKey<Customer>(e => e.CustomerId));
                builder.Entity<LastLogin>().ForeignKeys(fk => fk.ForeignKey<Login>(e => e.Username));
                builder.Entity<LastLogin>().ForeignKeys(fk => fk.ForeignKey<SmartCard>(e => e.SmartcardUsername));
                builder.Entity<OrderLine>().ForeignKeys(fk => fk.ForeignKey<AnOrder>(e => e.OrderId));
                builder.Entity<OrderLine>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ProductId));
                builder.Entity<ProductDetail>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ProductId));
                builder.Entity<ProductReview>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ProductId));
                builder.Entity<ProductPhoto>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ProductId));
                builder.Entity<ProductWebFeature>().ForeignKeys(fk => fk.ForeignKey<ProductPhoto>(e => new { e.ProductId, e.PhotoId }));
                builder.Entity<ProductWebFeature>().ForeignKeys(fk => fk.ForeignKey<ProductReview>(e => new { e.ProductId, e.ReviewId }));
                builder.Entity<Resolution>().ForeignKeys(fk => fk.ForeignKey<Complaint>(e => e.ResolutionId));
                builder.Entity<IncorrectScan>().ForeignKeys(fk => fk.ForeignKey<Barcode>(e => e.ExpectedCode));
                builder.Entity<Customer>().ForeignKeys(fk => fk.ForeignKey<Customer>(e => e.CustomerId));
                builder.Entity<IncorrectScan>().ForeignKeys(fk => fk.ForeignKey<Barcode>(e => e.ActualCode));
                builder.Entity<Barcode>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ProductId));
                builder.Entity<BarcodeDetail>().ForeignKeys(fk => fk.ForeignKey<Barcode>(e => e.Code));
                builder.Entity<SuspiciousActivity>().ForeignKeys(fk => fk.ForeignKey<Login>(e => e.Username));
                builder.Entity<RsaToken>().ForeignKeys(fk => fk.ForeignKey<Login>(e => e.Username));
                builder.Entity<SmartCard>().ForeignKeys(fk => fk.ForeignKey<Login>(e => e.Username));
                builder.Entity<PasswordReset>().ForeignKeys(fk => fk.ForeignKey<Login>(e => e.Username));
                builder.Entity<PageView>().ForeignKeys(fk => fk.ForeignKey<Login>(e => e.Username));
                builder.Entity<ComputerDetail>().ForeignKeys(fk => fk.ForeignKey<Computer>(e => e.ComputerDetailId));
                builder.Entity<License>().ForeignKeys(fk => fk.ForeignKey<Driver>(e => e.Name));

                // TODO: Many-to-many
                //builder.Entity<Supplier>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.SupplierId));

                // TODO: Inheritance
                //builder.Entity<BackOrderLine>().ForeignKeys(fk => fk.ForeignKey<Supplier>(e => e.SupplierId));
                //builder.Entity<DiscontinuedProduct>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ReplacementProductId));
                //builder.Entity<ProductPageView>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ProductId));

                // TODO: Key should get by-convention value generation even if key is not discovered by convention
                var noteId = builder.Model.GetEntityType(typeof(OrderNote)).GetProperty("NoteId");
                noteId.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
                noteId.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

                var featureId = builder.Model.GetEntityType(typeof(ProductWebFeature)).GetProperty("FeatureId");
                featureId.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
                featureId.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

                // TODO: Should key get by-convention value generation even if part of composite key?
                var reviewId = builder.Model.GetEntityType(typeof(ProductReview)).GetProperty("ReviewId");
                reviewId.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
                reviewId.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

                var photoId = builder.Model.GetEntityType(typeof(ProductPhoto)).GetProperty("PhotoId");
                photoId.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
                photoId.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

                // TODO: Key should not get by-convention value generation if it is dependent of identifying relationship
                var detailId = builder.Model.GetEntityType(typeof(ComputerDetail)).GetProperty("ComputerDetailId");
                detailId.ValueGenerationOnAdd = ValueGenerationOnAdd.None;
                detailId.ValueGenerationOnSave = ValueGenerationOnSave.None;

                var resolutionId = builder.Model.GetEntityType(typeof(Resolution)).GetProperty("ResolutionId");
                resolutionId.ValueGenerationOnAdd = ValueGenerationOnAdd.None;
                resolutionId.ValueGenerationOnSave = ValueGenerationOnSave.None;
            }

            public void SeedUsingFKs()
            {
                var customer1 = Add(new Customer { Name = "Sheila Koalie" });
                var customer2 = Add(new Customer { Name = "Sue Pandy" });
                var customer3 = Add(new Customer { Name = "Tarquin Tiger" });

                // TODO: Key propagation so all the additional SaveChanges calls can be removed
                SaveChanges();

                var product1 = Add(new Product { Description = "Mrs Koalie's Famous Waffles", BaseConcurrency = "Pounds Sterling" });
                var product2 = Add(new Product { Description = "Chocolate Donuts", BaseConcurrency = "US Dollars" });
                var product3 = Add(new Product { Description = "Assorted Dog Treats", BaseConcurrency = "Stuffy Money" });

                SaveChanges();

                var barcode1 = Add(new Barcode { Code = new byte[] { 1, 2, 3, 4 }, ProductId = product1.ProductId, Text = "Barcode 1 2 3 4" });
                var barcode2 = Add(new Barcode { Code = new byte[] { 2, 2, 3, 4 }, ProductId = product2.ProductId, Text = "Barcode 2 2 3 4" });
                var barcode3 = Add(new Barcode { Code = new byte[] { 3, 2, 3, 4 }, ProductId = product3.ProductId, Text = "Barcode 3 2 3 4" });

                SaveChanges();

                var barcodeDetails1 = Add(new BarcodeDetail { Code = barcode1.Code, RegisteredTo = "Eeky Bear" });
                var barcodeDetails2 = Add(new BarcodeDetail { Code = barcode2.Code, RegisteredTo = "Trent" });

                SaveChanges();

                var incorrectScan1 = Add(
                    new IncorrectScan
                        {
                            ScanDate = new DateTime(2014, 5, 28, 19, 9, 6),
                            Details = "Treats not Donuts",
                            ActualCode = barcode3.Code,
                            ExpectedCode = barcode2.Code
                        });

                var incorrectScan2 = Add(
                    new IncorrectScan
                        {
                            ScanDate = new DateTime(2014, 5, 28, 19, 15, 31),
                            Details = "Wot no waffles?",
                            ActualCode = barcode2.Code,
                            ExpectedCode = barcode1.Code
                        });

                SaveChanges();

                var complaint1 = Add(new Complaint
                    {
                        CustomerId = customer2.CustomerId,
                        Details = "Don't give coffee to Eeky!",
                        Logged = new DateTime(2014, 5, 27, 19, 22, 26)
                    });

                var complaint2 = Add(new Complaint
                    {
                        CustomerId = customer2.CustomerId,
                        Details = "Really! Don't give coffee to Eeky!",
                        Logged = new DateTime(2014, 5, 28, 19, 22, 26)
                    });

                SaveChanges();

                var resolution = Add(new Resolution { ResolutionId = complaint2.ComplaintId, Details = "Destroyed all coffee in Redmond area." });

                SaveChanges();

                var login1 = Add(new Login { CustomerId = customer1.CustomerId, Username = "MrsKoalie73" });
                var login2 = Add(new Login { CustomerId = customer2.CustomerId, Username = "MrsBossyPants" });
                var login3 = Add(new Login { CustomerId = customer3.CustomerId, Username = "TheStripedMenace" });

                SaveChanges();

                var suspiciousActivity1 = Add(new SuspiciousActivity { Activity = "Pig prints on keyboard", Username = login3.Username });
                var suspiciousActivity2 = Add(new SuspiciousActivity { Activity = "Crumbs in the cupboard", Username = login3.Username });
                var suspiciousActivity3 = Add(new SuspiciousActivity { Activity = "Donuts gone missing", Username = login3.Username });

                SaveChanges();

                var rsaToken1 = Add(new RsaToken { Issued = DateTime.Now, Serial = "1234", Username = login1.Username });
                var rsaToken2 = Add(new RsaToken { Issued = DateTime.Now, Serial = "2234", Username = login2.Username });

                SaveChanges();

                var smartCard1 = Add(new SmartCard { Username = login1.Username, CardSerial = rsaToken1.Serial, Issued = rsaToken1.Issued });
                var smartCard2 = Add(new SmartCard { Username = login2.Username, CardSerial = rsaToken2.Serial, Issued = rsaToken2.Issued });

                SaveChanges();

                var reset1 = Add(new PasswordReset
                    {
                        EmailedTo = "trent@example.com",
                        ResetNo = 1,
                        TempPassword = "Rent-A-Mole",
                        Username = login3.Username
                    });

                SaveChanges();

                var pageView1 = Add(new PageView { PageUrl = "somePage1", Username = login1.Username, Viewed = DateTime.Now });
                var pageView2 = Add(new PageView { PageUrl = "somePage2", Username = login1.Username, Viewed = DateTime.Now });
                var pageView3 = Add(new PageView { PageUrl = "somePage3", Username = login1.Username, Viewed = DateTime.Now });

                SaveChanges();

                var lastLogin1 = Add(new LastLogin
                    {
                        LoggedIn = new DateTime(2014, 5, 27, 10, 22, 26),
                        LoggedOut = new DateTime(2014, 5, 27, 11, 22, 26),
                        Username = login1.Username,
                        SmartcardUsername = smartCard1.Username
                    });

                var lastLogin2 = Add(new LastLogin
                    {
                        LoggedIn = new DateTime(2014, 5, 27, 12, 22, 26),
                        LoggedOut = new DateTime(2014, 5, 27, 13, 22, 26),
                        Username = login2.Username,
                        SmartcardUsername = smartCard2.Username
                    });

                SaveChanges();

                var message1 = Add(new Message
                    {
                        Subject = "Tea?",
                        Body = "Fancy a cup of tea?",
                        FromUsername = login1.Username,
                        ToUsername = login2.Username,
                        Sent = DateTime.Now,
                    });

                var message2 = Add(new Message
                    {
                        Subject = "Re: Tea?",
                        Body = "Love one!",
                        FromUsername = login2.Username,
                        ToUsername = login1.Username,
                        Sent = DateTime.Now,
                    });

                var message3 = Add(new Message
                    {
                        Subject = "Re: Tea?",
                        Body = "I'll put the kettle on.",
                        FromUsername = login1.Username,
                        ToUsername = login2.Username,
                        Sent = DateTime.Now,
                    });

                SaveChanges();

                var order1 = Add(new AnOrder { CustomerId = customer1.CustomerId, Username = login1.Username });
                var order2 = Add(new AnOrder { CustomerId = customer2.CustomerId, Username = login2.Username });
                var order3 = Add(new AnOrder { CustomerId = customer3.CustomerId, Username = login3.Username });

                SaveChanges();

                var orderNote1 = Add(new OrderNote { Note = "Must have tea!", OrderId = order1.AnOrderId });
                var orderNote2 = Add(new OrderNote { Note = "And donuts!", OrderId = order1.AnOrderId });
                var orderNote3 = Add(new OrderNote { Note = "But no coffee. :-(", OrderId = order1.AnOrderId });

                SaveChanges();

                var orderQualityCheck1 = Add(new OrderQualityCheck { OrderId = order1.AnOrderId, CheckedBy = "Eeky Bear" });
                var orderQualityCheck2 = Add(new OrderQualityCheck { OrderId = order2.AnOrderId, CheckedBy = "Eeky Bear" });
                var orderQualityCheck3 = Add(new OrderQualityCheck { OrderId = order3.AnOrderId, CheckedBy = "Eeky Bear" });

                SaveChanges();

                var orderLine1 = Add(new OrderLine { OrderId = order1.AnOrderId, ProductId = product1.ProductId, Quantity = 7 });
                var orderLine2 = Add(new OrderLine { OrderId = order1.AnOrderId, ProductId = product2.ProductId, Quantity = 1 });
                var orderLine3 = Add(new OrderLine { OrderId = order2.AnOrderId, ProductId = product3.ProductId, Quantity = 2 });
                var orderLine4 = Add(new OrderLine { OrderId = order2.AnOrderId, ProductId = product2.ProductId, Quantity = 3 });
                var orderLine5 = Add(new OrderLine { OrderId = order2.AnOrderId, ProductId = product1.ProductId, Quantity = 4 });
                var orderLine6 = Add(new OrderLine { OrderId = order3.AnOrderId, ProductId = product2.ProductId, Quantity = 5 });

                SaveChanges();

                var productDetail1 = Add(new ProductDetail { Details = "A Waffle Cart specialty!", ProductId = product1.ProductId });
                var productDetail2 = Add(new ProductDetail { Details = "Eeky Bear's favorite!", ProductId = product2.ProductId });

                SaveChanges();

                var productReview1 = Add(new ProductReview { ProductId = product1.ProductId, Review = "Better than Tarqies!" });
                var productReview2 = Add(new ProductReview { ProductId = product1.ProductId, Review = "Good with maple syrup." });
                var productReview3 = Add(new ProductReview { ProductId = product2.ProductId, Review = "Eeky says yes!" });

                SaveChanges();

                var productPhoto1 = Add(new ProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 101, 102 } });
                var productPhoto2 = Add(new ProductPhoto { ProductId = product1.ProductId, Photo = new byte[] { 103, 104 } });
                var productPhoto3 = Add(new ProductPhoto { ProductId = product3.ProductId, Photo = new byte[] { 105, 106 } });

                SaveChanges();

                var productWebFeature1 = Add(new ProductWebFeature
                    {
                        Heading = "Waffle Style",
                        PhotoId = productPhoto1.PhotoId,
                        ProductId = product1.ProductId,
                        ReviewId = productReview1.ReviewId
                    });

                var productWebFeature2 = Add(new ProductWebFeature
                    {
                        Heading = "What does the waffle say?",
                        ProductId = product2.ProductId,
                        ReviewId = productReview3.ReviewId
                    });

                SaveChanges();

                var supplier1 = Add(new Supplier { Name = "Trading As Trent" });
                var supplier2 = Add(new Supplier { Name = "Ants By Boris" });

                SaveChanges();

                var supplierLogo1 = Add(new SupplierLogo { SupplierId = supplier1.SupplierId, Logo = new byte[] { 201, 202 } });

                SaveChanges();

                var supplierInfo1 = Add(new SupplierInfo { SupplierId = supplier1.SupplierId, Information = "Seems a bit dodgy." });
                var supplierInfo2 = Add(new SupplierInfo { SupplierId = supplier1.SupplierId, Information = "Orange fur?" });
                var supplierInfo3 = Add(new SupplierInfo { SupplierId = supplier2.SupplierId, Information = "Very expensive!" });

                SaveChanges();

                var customerInfo1 = Add(new CustomerInfo { CustomerInfoId = customer1.CustomerId, Information = "Really likes tea." });
                var customerInfo2 = Add(new CustomerInfo { CustomerInfoId = customer2.CustomerId, Information = "Mrs Bossy Pants!" });

                SaveChanges();

                var computer1 = Add(new Computer { Name = "markash420" });
                var computer2 = Add(new Computer { Name = "unicorns420" });

                SaveChanges();

                var computerDetail1 = Add(new ComputerDetail
                    {
                        ComputerDetailId = computer1.ComputerId,
                        Manufacturer = "Dell",
                        Model = "420",
                        PurchaseDate = new DateTime(2008, 4, 1),
                        Serial = "4201",
                        Specifications = "It's a Dell!"
                    });

                var computerDetail2 = Add(new ComputerDetail
                    {
                        ComputerDetailId = computer2.ComputerId,
                        Manufacturer = "Not A Dell",
                        Model = "Not 420",
                        PurchaseDate = new DateTime(2012, 4, 1),
                        Serial = "4202",
                        Specifications = "It's not a Dell!"
                    });

                SaveChanges();

                var driver1 = Add(new Driver { BirthDate = new DateTime(2006, 9, 19), Name = "Eeky Bear" });
                var driver2 = Add(new Driver { BirthDate = new DateTime(2007, 9, 19), Name = "Splash Bear" });

                SaveChanges();

                var license1 = Add(new License
                    {
                        Name = driver1.Name,
                        LicenseClass = "C",
                        LicenseNumber = "10",
                        Restrictions = "None",
                        State = LicenseState.Active,
                        ExpirationDate = new DateTime(2018, 9, 19)
                    });

                var license2 = Add(new License
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

        protected class BackOrderLine2 : BackOrderLine
        {
        }

        protected class BackOrderLine : OrderLine
        {
            public DateTime ETA { get; set; }

            public int SupplierId { get; set; }
            public virtual Supplier Supplier { get; set; }
        }

        protected class BarcodeDetail
        {
            public byte[] Code { get; set; }
            public string RegisteredTo { get; set; }
        }

        protected class Barcode
        {
            public Barcode()
            {
                BadScans = new HashSet<IncorrectScan>();
            }

            public byte[] Code { get; set; }
            public int ProductId { get; set; }
            public string Text { get; set; }

            public virtual Product Product { get; set; }
            public virtual ICollection<IncorrectScan> BadScans { get; set; }
            public virtual BarcodeDetail Detail { get; set; }
        }

        protected class Complaint
        {
            public int ComplaintId { get; set; }
            public int? CustomerId { get; set; }
            public DateTime Logged { get; set; }
            public string Details { get; set; }

            public virtual Customer Customer { get; set; }
            public virtual Resolution Resolution { get; set; }
        }

        protected class ComputerDetail
        {
            public ComputerDetail()
            {
                Dimensions = new Dimensions();
            }

            public int ComputerDetailId { get; set; }
            public string Manufacturer { get; set; }
            public string Model { get; set; }
            public string Serial { get; set; }
            public string Specifications { get; set; }
            public DateTime PurchaseDate { get; set; }

            public Dimensions Dimensions { get; set; }

            public virtual Computer Computer { get; set; }
        }

        protected class Computer
        {
            public int ComputerId { get; set; }
            public string Name { get; set; }

            public virtual ComputerDetail ComputerDetail { get; set; }
        }

        protected class ConcurrencyInfo
        {
            public string Token { get; set; }
            public DateTime? QueriedDateTime { get; set; }
        }

        protected class ContactDetails
        {
            public ContactDetails()
            {
                HomePhone = new Phone();
                WorkPhone = new Phone();
                MobilePhone = new Phone();
            }

            public string Email { get; set; }

            public Phone HomePhone { get; set; }
            public Phone WorkPhone { get; set; }
            public Phone MobilePhone { get; set; }
        }

        protected class CustomerInfo
        {
            public int CustomerInfoId { get; set; }
            public string Information { get; set; }
        }

        protected class Dimensions
        {
            public decimal Width { get; set; }
            public decimal Height { get; set; }
            public decimal Depth { get; set; }
        }

        protected class DiscontinuedProduct : Product
        {
            public DateTime Discontinued { get; set; }
            public int? ReplacementProductId { get; set; }

            public virtual Product ReplacedBy { get; set; }
        }

        protected class Driver
        {
            public string Name { get; set; }
            public DateTime BirthDate { get; set; }

            public virtual License License { get; set; }
        }

        protected class IncorrectScan
        {
            public int IncorrectScanId { get; set; }
            public byte[] ExpectedCode { get; set; }
            public byte[] ActualCode { get; set; }
            public DateTime ScanDate { get; set; }
            public string Details { get; set; }

            public virtual Barcode ExpectedBarcode { get; set; }
            public virtual Barcode ActualBarcode { get; set; }
        }

        protected class LastLogin
        {
            public string Username { get; set; }
            public DateTime LoggedIn { get; set; }
            public DateTime? LoggedOut { get; set; }

            public string SmartcardUsername { get; set; }

            public virtual Login Login { get; set; }
        }

        protected class License
        {
            public License()
            {
                LicenseClass = "C";
            }

            public string Name { get; set; }
            public string LicenseNumber { get; set; }
            public string LicenseClass { get; set; }
            public string Restrictions { get; set; }
            public DateTime ExpirationDate { get; set; }
            public LicenseState? State { get; set; }

            public virtual Driver Driver { get; set; }
        }

        protected class Message
        {
            public int MessageId { get; set; }
            public string FromUsername { get; set; }
            public string ToUsername { get; set; }
            public DateTime Sent { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public bool IsRead { get; set; }

            public virtual Login Sender { get; set; }
            public virtual Login Recipient { get; set; }
        }

        protected class OrderLine
        {
            public OrderLine()
            {
                Quantity = 1;
            }

            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public string ConcurrencyToken { get; set; }

            public virtual AnOrder Order { get; set; }
            public virtual Product Product { get; set; }
        }

        protected class AnOrder
        {
            public AnOrder()
            {
                OrderLines = new HashSet<OrderLine>();
                Notes = new HashSet<OrderNote>();
                Concurrency = new ConcurrencyInfo();
            }

            public int AnOrderId { get; set; }
            public int? CustomerId { get; set; }

            public ConcurrencyInfo Concurrency { get; set; }

            public virtual Customer Customer { get; set; }
            public virtual ICollection<OrderLine> OrderLines { get; set; }
            public virtual ICollection<OrderNote> Notes { get; set; }

            public string Username { get; set; }
            public virtual Login Login { get; set; }
        }

        protected class OrderNote
        {
            public int NoteId { get; set; }
            public string Note { get; set; }

            public int OrderId { get; set; }
            public virtual AnOrder Order { get; set; }
        }

        protected class OrderQualityCheck
        {
            public int OrderId { get; set; }
            public string CheckedBy { get; set; }
            public DateTime CheckedDateTime { get; set; }

            public virtual AnOrder Order { get; set; }
        }

        protected class PageView
        {
            public int PageViewId { get; set; }
            public string Username { get; set; }
            public DateTime Viewed { get; set; }
            public string PageUrl { get; set; }

            public virtual Login Login { get; set; }
        }

        protected class PasswordReset
        {
            public int ResetNo { get; set; }
            public string Username { get; set; }
            public string TempPassword { get; set; }
            public string EmailedTo { get; set; }

            public virtual Login Login { get; set; }
        }

        protected class ProductDetail
        {
            public int ProductId { get; set; }
            public string Details { get; set; }

            public virtual Product Product { get; set; }
        }

        protected class Product
        {
            public Product()
            {
                Suppliers = new HashSet<Supplier>();
                Replaces = new HashSet<DiscontinuedProduct>();
                Reviews = new HashSet<ProductReview>();
                Photos = new HashSet<ProductPhoto>();
                Barcodes = new HashSet<Barcode>();
                Dimensions = new Dimensions();
                ComplexConcurrency = new ConcurrencyInfo();
                NestedComplexConcurrency = new AuditInfo();
            }

            public int ProductId { get; set; }
            public string Description { get; set; }
            public string BaseConcurrency { get; set; }

            public Dimensions Dimensions { get; set; }
            public ConcurrencyInfo ComplexConcurrency { get; set; }
            public AuditInfo NestedComplexConcurrency { get; set; }

            public virtual ICollection<Supplier> Suppliers { get; set; }
            public virtual ICollection<DiscontinuedProduct> Replaces { get; set; }
            public virtual ProductDetail Detail { get; set; }
            public virtual ICollection<ProductReview> Reviews { get; set; }
            public virtual ICollection<ProductPhoto> Photos { get; set; }
            public virtual ICollection<Barcode> Barcodes { get; set; }
        }

        protected class ProductPageView : PageView
        {
            public int ProductId { get; set; }

            public virtual Product Product { get; set; }
        }

        protected class ProductPhoto
        {
            public ProductPhoto()
            {
                Features = new HashSet<ProductWebFeature>();
            }

            public int ProductId { get; set; }
            public int PhotoId { get; set; }
            public byte[] Photo { get; set; }

            public virtual ICollection<ProductWebFeature> Features { get; set; }
        }

        protected class ProductReview
        {
            public ProductReview()
            {
                Features = new HashSet<ProductWebFeature>();
            }

            public int ProductId { get; set; }
            public int ReviewId { get; set; }
            public string Review { get; set; }

            public virtual Product Product { get; set; }
            public virtual ICollection<ProductWebFeature> Features { get; set; }
        }

        protected class ProductWebFeature
        {
            public int FeatureId { get; set; }
            public int? ProductId { get; set; }
            public int? PhotoId { get; set; }
            public int ReviewId { get; set; }
            public string Heading { get; set; }

            public virtual ProductReview Review { get; set; }
            public virtual ProductPhoto Photo { get; set; }
        }

        protected class Resolution
        {
            public int ResolutionId { get; set; }
            public string Details { get; set; }

            public virtual Complaint Complaint { get; set; }
        }

        protected class RsaToken
        {
            public string Serial { get; set; }
            public DateTime Issued { get; set; }

            public string Username { get; set; }
            public virtual Login Login { get; set; }
        }

        protected class SmartCard
        {
            public string Username { get; set; }
            public string CardSerial { get; set; }
            public DateTime Issued { get; set; }

            public virtual Login Login { get; set; }
            public virtual LastLogin LastLogin { get; set; }
        }

        protected class SupplierInfo
        {
            public int SupplierInfoId { get; set; }
            public string Information { get; set; }

            public int SupplierId { get; set; }
            public virtual Supplier Supplier { get; set; }
        }

        protected class SupplierLogo
        {
            public int SupplierId { get; set; }
            public byte[] Logo { get; set; }
        }

        protected class Supplier
        {
            public Supplier()
            {
                Products = new HashSet<Product>();
                BackOrderLines = new HashSet<BackOrderLine>();
            }

            public int SupplierId { get; set; }
            public string Name { get; set; }

            public virtual ICollection<Product> Products { get; set; }
            public virtual ICollection<BackOrderLine> BackOrderLines { get; set; }
            public virtual SupplierLogo Logo { get; set; }
        }

        protected class SuspiciousActivity
        {
            public int SuspiciousActivityId { get; set; }
            public string Activity { get; set; }

            public string Username { get; set; }
        }

        protected class AuditInfo
        {
            public AuditInfo()
            {
                Concurrency = new ConcurrencyInfo();
            }

            public DateTime ModifiedDate { get; set; }
            public string ModifiedBy { get; set; }

            public ConcurrencyInfo Concurrency { get; set; }
        }

        protected class Customer
        {
            public Customer()
            {
                Orders = new HashSet<AnOrder>();
                Logins = new HashSet<Login>();
                ContactInfo = new ContactDetails();
                Auditing = new AuditInfo();
            }

            public int CustomerId { get; set; }
            public string Name { get; set; }

            public ContactDetails ContactInfo { get; set; }
            public AuditInfo Auditing { get; set; }

            public virtual ICollection<AnOrder> Orders { get; set; }
            public virtual ICollection<Login> Logins { get; set; }
            public virtual Customer Husband { get; set; }
            public virtual Customer Wife { get; set; }
            public virtual CustomerInfo Info { get; set; }
        }

        public enum LicenseState
        {
            Active = 1,
            Suspended = 2,
            Revoked = 3
        }

        protected class Login
        {
            public Login()
            {
                SentMessages = new HashSet<Message>();
                ReceivedMessages = new HashSet<Message>();
                Orders = new HashSet<AnOrder>();
            }

            public string Username { get; set; }
            public int CustomerId { get; set; }

            public virtual Customer Customer { get; set; }
            public virtual LastLogin LastLogin { get; set; }
            public virtual ICollection<Message> SentMessages { get; set; }
            public virtual ICollection<Message> ReceivedMessages { get; set; }
            public virtual ICollection<AnOrder> Orders { get; set; }
        }

        protected class Phone
        {
            public Phone()
            {
                Extension = "None";
            }

            public string PhoneNumber { get; set; }
            public string Extension { get; set; }
            public PhoneType PhoneType { get; set; }
        }

        public enum PhoneType
        {
            Cell = 1,
            Land = 2,
            Satellite = 3,
        }
    }
}
