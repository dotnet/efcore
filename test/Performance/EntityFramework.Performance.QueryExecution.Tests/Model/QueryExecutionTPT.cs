// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
	using System;
	using Microsoft.Data.Entity;
	using Microsoft.Data.Entity.Metadata;
    using System.Collections.Generic;

    public partial class QueryExecutionTPT : DbContext
    {
        private readonly string _connectionString;

        public QueryExecutionTPT(string connectionString, IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
            _connectionString = connectionString;
        }

        public virtual DbSet<BackOrderLine> BackOrderLines { get; set; }
        public virtual DbSet<Barcode> Barcodes { get; set; }
        public virtual DbSet<BarcodeDetail> BarcodeDetails { get; set; }
        public virtual DbSet<Complaint> Complaints { get; set; }
        public virtual DbSet<Computer> Computers { get; set; }
        public virtual DbSet<ComputerDetail> ComputerDetails { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerInfo> CustomerInfoes { get; set; }
        public virtual DbSet<DiscontinuedProduct> DiscontinuedProducts { get; set; }
        public virtual DbSet<Driver> Drivers { get; set; }
        public virtual DbSet<IncorrectScan> IncorrectScans { get; set; }
        public virtual DbSet<LastLogin> LastLogins { get; set; }
        public virtual DbSet<License> Licenses { get; set; }
        public virtual DbSet<Login> Logins { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderLine> OrderLines { get; set; }
        public virtual DbSet<OrderNote> OrderNotes { get; set; }
        public virtual DbSet<OrderQualityCheck> OrderQualityChecks { get; set; }
        public virtual DbSet<PageView> PageViews { get; set; }
        public virtual DbSet<PasswordReset> PasswordResets { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductDetail> ProductDetails { get; set; }
        public virtual DbSet<ProductPageView> ProductPageViews { get; set; }
        public virtual DbSet<ProductPhoto> ProductPhotoes { get; set; }
        public virtual DbSet<ProductReview> ProductReviews { get; set; }
        public virtual DbSet<ProductWebFeature> ProductWebFeatures { get; set; }
        public virtual DbSet<Resolution> Resolutions { get; set; }
        public virtual DbSet<RSAToken> RSATokens { get; set; }
        public virtual DbSet<SmartCard> SmartCards { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<SupplierInfo> SupplierInfoes { get; set; }
        public virtual DbSet<SupplierLogo> SupplierLogoes { get; set; }
        public virtual DbSet<SuspiciousActivity> SuspiciousActivities { get; set; }

        protected override void OnConfiguring(DbContextOptions builder)
        {
            builder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Keys and tables
            modelBuilder.Entity<BackOrderLine>(b =>
            {
                b.ToTable("BackOrderLine", "dbo");
                b.Key(e => new {e.OrderId, e.ProductId});
            });
            modelBuilder.Entity<Barcode>(b =>
            {
                b.ToTable("Barcode", "dbo");
                b.Key(e => e.Code);
            });
            modelBuilder.Entity<BarcodeDetail>(b =>
            {
                b.ToTable("Detail", "dbo");
                b.Key(e => e.Code);
            });
            modelBuilder.Entity<Complaint>(b => b.ToTable("Complaint", "dbo"));
            modelBuilder.Entity<Computer>(b =>
            {
                b.ToTable("Computer", "dbo");
            });
            modelBuilder.Entity<Customer>(b =>
            {
                b.ToTable("Customer", "dbo");
            });
            modelBuilder.Entity<CustomerInfo>(b =>
            {
                b.ToTable("Info", "dbo");
            });
            modelBuilder.Entity<DiscontinuedProduct>(b =>
            {
                b.ToTable("DiscontinuedProduct", "dbo");
                b.Key(e => e.ProductId);
            });
            modelBuilder.Entity<Driver>(b =>
            {
                b.ToTable("Driver", "dbo");
                b.Key(e => e.Name);
            });
            modelBuilder.Entity<IncorrectScan>(b =>
            {
                b.ToTable("IncorrectScan", "dbo");
            });

            modelBuilder.Entity<LastLogin>(b =>
            {
                b.Key(e => e.Username);
                b.ToTable("LastLogin", "dbo");
            });
            modelBuilder.Entity<License>(b =>
            {
                b.Key(e => e.Name);
                b.ToTable("License", "dbo");
            });
            modelBuilder.Entity<Login>(b =>
            {
                b.Key(e => e.Username);
                b.ToTable("Login", "dbo");
            });
            modelBuilder.Entity<Message>(b =>
            {
                b.Key(e => new {e.MessageId, e.FromUsername});
                b.ToTable("Message", "dbo");
            });
            modelBuilder.Entity<Order>(b =>
            {
                b.ToTable("Order", "dbo");
                b.Property(e => e.OrderId).GenerateValuesOnAdd(false);
            });
            modelBuilder.Entity<OrderLine>(b =>
            {
                b.Key(e => new {e.OrderId, e.ProductId});
                b.ToTable("OrderLine", "dbo");
            });
            modelBuilder.Entity<OrderNote>(b =>
            {
                b.Key(e => e.NoteId);
                b.ToTable("OrderNote", "dbo");
            });
            modelBuilder.Entity<OrderQualityCheck>(b =>
            {
                b.Key(e => e.OrderId);
                b.ToTable("OrderQualityCheck", "dbo");
            });
            modelBuilder.Entity<PageView>(b =>
            {
                b.ToTable("PageView", "dbo");
            });
            modelBuilder.Entity<PasswordReset>(b =>
            {
                b.Key(e => new {e.ResetNo, e.Username});
                b.ToTable("PasswordReset", "dbo");
            });
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Product", "dbo");
                b.Property(e => e.ProductId).GenerateValuesOnAdd(false);
            });
            modelBuilder.Entity<ProductDetail>(b =>
            {
                b.Key(e => e.ProductId);
                b.ToTable("ProductDetail", "dbo");
            });
            modelBuilder.Entity<ProductPageView>(b =>
            {
                b.Key(e => e.PageViewId);
                b.ToTable("ProductPageView", "dbo");
            });
            modelBuilder.Entity<ProductPhoto>(b =>
            {
                b.Key(e => new {e.ProductId, e.PhotoId});
                b.ToTable("ProductPhoto", "dbo");
            });
            modelBuilder.Entity<ProductReview>(b =>
            {
                b.Key(e => new {e.ProductId, e.ReviewId});
                b.ToTable("ProductReview", "dbo");
            });
            modelBuilder.Entity<ProductWebFeature>(b =>
            {
                b.Key(e => e.FeatureId);
                b.ToTable("ProductWebFeature", "dbo");
            });
            modelBuilder.Entity<Resolution>(b =>
            {
                b.ToTable("Resolution", "dbo");
            });
            modelBuilder.Entity<RSAToken>(b =>
            {
                b.Key(e => e.Serial);
                b.ToTable("RSAToken", "dbo");
            });
            modelBuilder.Entity<SmartCard>(b =>
            {
                b.Key(e => e.Username);
                b.ToTable("SmartCard", "dbo");
            });
            modelBuilder.Entity<Supplier>(b =>
            {
                b.ToTable("Supplier", "dbo");
            });
            modelBuilder.Entity<SupplierInfo>(b =>
            {
                b.ToTable("SupplierInfo", "dbo"); 
            });
            modelBuilder.Entity<SupplierLogo>(b =>
            {
                b.Key(e => e.SupplierId);
                b.ToTable("SupplierLogo", "dbo");
            });
            modelBuilder.Entity<SuspiciousActivity>(b =>
            {
                b.ToTable("SuspiciousActivity", "dbo");
            });

            modelBuilder.Entity<Order>(b =>
            {
                b.OneToMany(e => (IEnumerable<OrderLine>)e.OrderLines, e => (Order)e.Order);
                b.OneToMany(e => (IEnumerable<OrderNote>)e.Notes, e => (Order)e.Order)
                    .ReferencedKey(e => e.OrderId);
            });

            modelBuilder.Entity<OrderQualityCheck>(b =>
            {
                b.Key(e => e.OrderId);
                b.OneToOne(e => (Order)e.Order)
                    .ForeignKey<OrderQualityCheck>(e => e.OrderId)
                    .ReferencedKey<Order>(e => e.OrderId);
            });

            modelBuilder.Entity<Product>(b =>
            {
                b.OneToMany(e => (IEnumerable<ProductReview>)e.Reviews, e => (Product)e.Product);
                b.OneToMany(e => (IEnumerable<Barcode>)e.Barcodes, e => (Product)e.Product);
                b.OneToMany(e => (IEnumerable<ProductPhoto>)e.Photos);
                b.OneToOne(e => (ProductDetail)e.Detail, e => (Product)e.Product);
                b.OneToOne<ProductWebFeature>();
            });

            modelBuilder.Entity<OrderLine>(b =>
            {
                b.Key(e => new { e.OrderId, e.ProductId });
                b.ManyToOne(e => (Product)e.Product);
            });

            modelBuilder.Entity<Supplier>().OneToOne(e => (SupplierLogo)e.Logo);

            modelBuilder.Entity<Customer>(b =>
            {
                b.OneToMany(e => (IEnumerable<Order>)e.Orders, e => (Customer)e.Customer);
                b.OneToMany(e => (IEnumerable<Login>)e.Logins, e => (Customer)e.Customer);
                b.OneToOne(e => (CustomerInfo)e.Info);

                b.OneToOne(e => (Customer)e.Wife, e => (Customer)e.Husband)
                    .ForeignKey<Customer>(e => e.HusbandId);
            });

            modelBuilder.Entity<Complaint>(b =>
            {
                b.ManyToOne(e => (Customer)e.Customer)
                    .ForeignKey(e => e.CustomerId);
                b.OneToOne(e => (Resolution)e.Resolution, e => (Complaint)e.Complaint)
                    .ReferencedKey<Complaint>(e => e.ComplaintId);
            });

            modelBuilder.Entity<ProductPhoto>(b =>
            {
                b.Key(e => new { e.ProductId, e.PhotoId });

                b.OneToMany(e => (IEnumerable<ProductWebFeature>)e.Features, e => (ProductPhoto)e.Photo)
                    .ForeignKey(e => new { e.ProductId, e.PhotoId })
                    .ReferencedKey(e => new { e.ProductId, e.PhotoId });
            });

            modelBuilder.Entity<ProductReview>(b =>
            {
                b.Key(e => new { e.ProductId, e.ReviewId });

                b.OneToMany(e => (IEnumerable<ProductWebFeature>)e.Features, e => (ProductReview)e.Review)
                    .ForeignKey(e => new { e.ProductId, e.ReviewId });
            });

            modelBuilder.Entity<Login>(b =>
            {
                b.Key(e => e.Username);

                b.OneToMany(e => (IEnumerable<Message>)e.SentMessages, e => (Login)e.Sender)
                    .ForeignKey(e => e.FromUsername);

                b.OneToMany(e => (IEnumerable<Message>)e.ReceivedMessages, e => (Login)e.Recipient)
                    .ForeignKey(e => e.ToUsername);

                b.OneToMany(e => (IEnumerable<Order>)e.Orders, e => (Login)e.Login)
                    .ForeignKey(e => e.Username);

                b.OneToMany<SuspiciousActivity>()
                    .ForeignKey(e => e.Username);

                b.OneToOne(e => (LastLogin)e.LastLogin, e => (Login)e.Login);
            });

            modelBuilder.Entity<PasswordReset>(b =>
            {
                b.Key(e => new { e.ResetNo, e.Username });
                b.ManyToOne(e => (Login)e.Login)
                    .ForeignKey(e => e.Username)
                    .ReferencedKey(e => e.Username);
            });

            modelBuilder.Entity<PageView>()
                .ManyToOne(e => (Login)e.Login)
                .ForeignKey(e => e.Username);

            modelBuilder.Entity<Barcode>(b =>
            {
                b.Key(e => e.Code);

                b.OneToMany(e => (IEnumerable<IncorrectScan>)e.BadScans, e => (Barcode)e.ExpectedBarcode)
                    .ForeignKey(e => e.ExpectedCode);

                b.OneToOne(e => (BarcodeDetail)e.Detail);
            });

            modelBuilder.Entity<IncorrectScan>()
                .ManyToOne(e => (Barcode)e.ActualBarcode)
                .ForeignKey(e => e.ActualCode);

            modelBuilder.Entity<SupplierInfo>().ManyToOne(e => (Supplier)e.Supplier);

            modelBuilder.Entity<Computer>().OneToOne(e => (ComputerDetail)e.ComputerDetail, e => (Computer)e.Computer);

            modelBuilder.Entity<Driver>(b =>
            {
                b.Key(e => e.Name);
                b.OneToOne(e => (License)e.License, e => (Driver)e.Driver);
            });

            modelBuilder.Entity<SmartCard>(b =>
            {
                b.Key(e => e.Username);

                b.OneToOne(e => (Login)e.Login)
                    .ForeignKey<SmartCard>(e => e.Username);

                b.OneToOne(e => (LastLogin)e.LastLogin)
                    .ForeignKey<LastLogin>(e => e.SmartcardUsername);
            });

            modelBuilder.Entity<RSAToken>(b =>
            {
                b.Key(e => e.Serial);
                b.OneToOne(e => (Login)e.Login)
                    .ForeignKey<RSAToken>(e => e.Username);
            });

            // TODO: Many-to-many
            //modelBuilder.Entity<Supplier>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.SupplierId));

            // TODO: Inheritance
            //modelBuilder.Entity<TBackOrderLine>().ForeignKeys(fk => fk.ForeignKey<Supplier>(e => e.SupplierId));
            //modelBuilder.Entity<TDiscontinuedProduct>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ReplacemenProductId));
            //modelBuilder.Entity<ProductPageView>().ForeignKeys(fk => fk.ForeignKey<Product>(e => e.ProductId));

            var model = modelBuilder.Model;

            // TODO: Key should get by-convention value generation even if key is not discovered by convention
            var noteId = model.GetEntityType(typeof(OrderNote)).GetProperty("NoteId");
            noteId.ValueGeneration = ValueGeneration.OnAdd;

            var featureId = model.GetEntityType(typeof(ProductWebFeature)).GetProperty("FeatureId");
            featureId.ValueGeneration = ValueGeneration.OnAdd;

            // TODO: Should key get by-convention value generation even if part of composite key?
            var reviewId = model.GetEntityType(typeof(ProductReview)).GetProperty("ReviewId");
            reviewId.ValueGeneration = ValueGeneration.OnAdd;

            var photoId = model.GetEntityType(typeof(ProductPhoto)).GetProperty("PhotoId");
            photoId.ValueGeneration = ValueGeneration.OnAdd;

            // TODO: Key should not get by-convention value generation if it is dependent of identifying relationship
            var detailId = model.GetEntityType(typeof(ComputerDetail)).GetProperty("ComputerDetailId");
            detailId.ValueGeneration = ValueGeneration.None;

            var resolutionId = model.GetEntityType(typeof(Resolution)).GetProperty("ResolutionId");
            resolutionId.ValueGeneration = ValueGeneration.None;

            var customerId = model.GetEntityType(typeof(CustomerInfo)).GetProperty("CustomerInfoId");
            customerId.ValueGeneration = ValueGeneration.None;
        }
    }
}
