// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using EntityFramework.Microbenchmarks.QueryExecutionPerf.Model;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    public class QueryExecutionTPT : DbContext
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
                    b.ForRelational().Table("BackOrderLine", "dbo");
                    b.Key(e => new { e.OrderId, e.ProductId });
                });
            modelBuilder.Entity<Barcode>(b =>
                {
                    b.ForRelational().Table("Barcode", "dbo");
                    b.Key(e => e.Code);
                });
            modelBuilder.Entity<BarcodeDetail>(b =>
                {
                    b.ForRelational().Table("Detail", "dbo");
                    b.Key(e => e.Code);
                });
            modelBuilder.Entity<Complaint>(b => b.ForRelational().Table("Complaint", "dbo"));
            modelBuilder.Entity<Computer>(b => { b.ForRelational().Table("Computer", "dbo"); });
            modelBuilder.Entity<Customer>(b => { b.ForRelational().Table("Customer", "dbo"); });
            modelBuilder.Entity<CustomerInfo>(b => { b.ForRelational().Table("Info", "dbo"); });
            modelBuilder.Entity<DiscontinuedProduct>(b =>
                {
                    b.ForRelational().Table("DiscontinuedProduct", "dbo");
                    b.Key(e => e.ProductId);
                });
            modelBuilder.Entity<Driver>(b =>
                {
                    b.ForRelational().Table("Driver", "dbo");
                    b.Key(e => e.Name);
                });
            modelBuilder.Entity<IncorrectScan>(b => { b.ForRelational().Table("IncorrectScan", "dbo"); });

            modelBuilder.Entity<LastLogin>(b =>
                {
                    b.Key(e => e.Username);
                    b.ForRelational().Table("LastLogin", "dbo");
                });
            modelBuilder.Entity<License>(b =>
                {
                    b.Key(e => e.Name);
                    b.ForRelational().Table("License", "dbo");
                });
            modelBuilder.Entity<Login>(b =>
                {
                    b.Key(e => e.Username);
                    b.ForRelational().Table("Login", "dbo");
                });
            modelBuilder.Entity<Message>(b =>
                {
                    b.Key(e => new { e.MessageId, e.FromUsername });
                    b.ForRelational().Table("Message", "dbo");
                });
            modelBuilder.Entity<Order>(b =>
                {
                    b.ForRelational().Table("Order", "dbo");
                    b.Property(e => e.OrderId).GenerateValueOnAdd(false);
                });
            modelBuilder.Entity<OrderLine>(b =>
                {
                    b.Key(e => new { e.OrderId, e.ProductId });
                    b.ForRelational().Table("OrderLine", "dbo");
                });
            modelBuilder.Entity<OrderNote>(b =>
                {
                    b.Key(e => e.NoteId);
                    b.ForRelational().Table("OrderNote", "dbo");
                });
            modelBuilder.Entity<OrderQualityCheck>(b =>
                {
                    b.Key(e => e.OrderId);
                    b.ForRelational().Table("OrderQualityCheck", "dbo");
                });
            modelBuilder.Entity<PageView>(b => { b.ForRelational().Table("PageView", "dbo"); });
            modelBuilder.Entity<PasswordReset>(b =>
                {
                    b.Key(e => new { e.ResetNo, e.Username });
                    b.ForRelational().Table("PasswordReset", "dbo");
                });
            modelBuilder.Entity<Product>(b =>
                {
                    b.ForRelational().Table("Product", "dbo");
                    b.Property(e => e.ProductId).GenerateValueOnAdd(false);
                });
            modelBuilder.Entity<ProductDetail>(b =>
                {
                    b.Key(e => e.ProductId);
                    b.ForRelational().Table("ProductDetail", "dbo");
                });
            modelBuilder.Entity<ProductPageView>(b =>
                {
                    b.Key(e => e.PageViewId);
                    b.ForRelational().Table("ProductPageView", "dbo");
                });
            modelBuilder.Entity<ProductPhoto>(b =>
                {
                    b.Key(e => new { e.ProductId, e.PhotoId });
                    b.ForRelational().Table("ProductPhoto", "dbo");
                });
            modelBuilder.Entity<ProductReview>(b =>
                {
                    b.Key(e => new { e.ProductId, e.ReviewId });
                    b.ForRelational().Table("ProductReview", "dbo");
                });
            modelBuilder.Entity<ProductWebFeature>(b =>
                {
                    b.Key(e => e.FeatureId);
                    b.ForRelational().Table("ProductWebFeature", "dbo");
                });
            modelBuilder.Entity<Resolution>(b => { b.ForRelational().Table("Resolution", "dbo"); });
            modelBuilder.Entity<RSAToken>(b =>
                {
                    b.Key(e => e.Serial);
                    b.ForRelational().Table("RSAToken", "dbo");
                });
            modelBuilder.Entity<SmartCard>(b =>
                {
                    b.Key(e => e.Username);
                    b.ForRelational().Table("SmartCard", "dbo");
                });
            modelBuilder.Entity<Supplier>(b => { b.ForRelational().Table("Supplier", "dbo"); });
            modelBuilder.Entity<SupplierInfo>(b => { b.ForRelational().Table("SupplierInfo", "dbo"); });
            modelBuilder.Entity<SupplierLogo>(b =>
                {
                    b.Key(e => e.SupplierId);
                    b.ForRelational().Table("SupplierLogo", "dbo");
                });
            modelBuilder.Entity<SuspiciousActivity>(b => { b.ForRelational().Table("SuspiciousActivity", "dbo"); });

            modelBuilder.Entity<Order>(b =>
                {
                    b.OneToMany(e => (IEnumerable<OrderLine>)e.OrderLines, e => e.Order);
                    b.OneToMany(e => (IEnumerable<OrderNote>)e.Notes, e => e.Order)
                        .ReferencedKey(e => e.OrderId);
                });

            modelBuilder.Entity<OrderQualityCheck>(b =>
                {
                    b.Key(e => e.OrderId);
                    b.OneToOne(e => e.Order)
                        .ForeignKey<OrderQualityCheck>(e => e.OrderId)
                        .ReferencedKey<Order>(e => e.OrderId);
                });

            modelBuilder.Entity<Product>(b =>
                {
                    b.OneToMany(e => (IEnumerable<ProductReview>)e.Reviews, e => e.Product);
                    b.OneToMany(e => (IEnumerable<Barcode>)e.Barcodes, e => e.Product);
                    b.OneToMany(e => (IEnumerable<ProductPhoto>)e.Photos);
                    b.OneToOne(e => e.Detail, e => e.Product);
                    b.OneToOne<ProductWebFeature>();
                });

            modelBuilder.Entity<OrderLine>(b =>
                {
                    b.Key(e => new { e.OrderId, e.ProductId });
                    b.ManyToOne(e => e.Product);
                });

            modelBuilder.Entity<Supplier>().OneToOne(e => e.Logo);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.OneToMany(e => (IEnumerable<Order>)e.Orders, e => e.Customer);
                    b.OneToMany(e => (IEnumerable<Login>)e.Logins, e => e.Customer);
                    b.OneToOne(e => e.Info);

                    b.OneToOne(e => e.Wife, e => e.Husband)
                        .ForeignKey<Customer>(e => e.HusbandId);
                });

            modelBuilder.Entity<Complaint>(b =>
                {
                    b.ManyToOne(e => e.Customer)
                        .ForeignKey(e => e.CustomerId);
                    b.OneToOne(e => e.Resolution, e => e.Complaint)
                        .ReferencedKey<Complaint>(e => e.ComplaintId);
                });

            modelBuilder.Entity<ProductPhoto>(b =>
                {
                    b.Key(e => new { e.ProductId, e.PhotoId });

                    b.OneToMany(e => (IEnumerable<ProductWebFeature>)e.Features, e => e.Photo)
                        .ForeignKey(e => new { e.ProductId, e.PhotoId })
                        .ReferencedKey(e => new { e.ProductId, e.PhotoId });
                });

            modelBuilder.Entity<ProductReview>(b =>
                {
                    b.Key(e => new { e.ProductId, e.ReviewId });

                    b.OneToMany(e => (IEnumerable<ProductWebFeature>)e.Features, e => e.Review)
                        .ForeignKey(e => new { e.ProductId, e.ReviewId });
                });

            modelBuilder.Entity<Login>(b =>
                {
                    b.Key(e => e.Username);

                    b.OneToMany(e => (IEnumerable<Message>)e.SentMessages, e => e.Sender)
                        .ForeignKey(e => e.FromUsername);

                    b.OneToMany(e => (IEnumerable<Message>)e.ReceivedMessages, e => e.Recipient)
                        .ForeignKey(e => e.ToUsername);

                    b.OneToMany(e => (IEnumerable<Order>)e.Orders, e => e.Login)
                        .ForeignKey(e => e.Username);

                    b.OneToMany<SuspiciousActivity>()
                        .ForeignKey(e => e.Username);

                    b.OneToOne(e => e.LastLogin, e => e.Login);
                });

            modelBuilder.Entity<PasswordReset>(b =>
                {
                    b.Key(e => new { e.ResetNo, e.Username });
                    b.ManyToOne(e => e.Login)
                        .ForeignKey(e => e.Username)
                        .ReferencedKey(e => e.Username);
                });

            modelBuilder.Entity<PageView>()
                .ManyToOne(e => e.Login)
                .ForeignKey(e => e.Username);

            modelBuilder.Entity<Barcode>(b =>
                {
                    b.Key(e => e.Code);

                    b.OneToMany(e => (IEnumerable<IncorrectScan>)e.BadScans, e => e.ExpectedBarcode)
                        .ForeignKey(e => e.ExpectedCode);

                    b.OneToOne(e => e.Detail);
                });

            modelBuilder.Entity<IncorrectScan>()
                .ManyToOne(e => e.ActualBarcode)
                .ForeignKey(e => e.ActualCode);

            modelBuilder.Entity<SupplierInfo>().ManyToOne(e => e.Supplier);

            modelBuilder.Entity<Computer>().OneToOne(e => e.ComputerDetail, e => e.Computer);

            modelBuilder.Entity<Driver>(b =>
                {
                    b.Key(e => e.Name);
                    b.OneToOne(e => e.License, e => e.Driver);
                });

            modelBuilder.Entity<SmartCard>(b =>
                {
                    b.Key(e => e.Username);

                    b.OneToOne(e => e.Login)
                        .ForeignKey<SmartCard>(e => e.Username);

                    b.OneToOne(e => e.LastLogin)
                        .ForeignKey<LastLogin>(e => e.SmartcardUsername);
                });

            modelBuilder.Entity<RSAToken>(b =>
                {
                    b.Key(e => e.Serial);
                    b.OneToOne(e => e.Login)
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
            noteId.GenerateValueOnAdd = true;

            var featureId = model.GetEntityType(typeof(ProductWebFeature)).GetProperty("FeatureId");
            featureId.GenerateValueOnAdd = true;

            // TODO: Should key get by-convention value generation even if part of composite key?
            var reviewId = model.GetEntityType(typeof(ProductReview)).GetProperty("ReviewId");
            reviewId.GenerateValueOnAdd = true;

            var photoId = model.GetEntityType(typeof(ProductPhoto)).GetProperty("PhotoId");
            photoId.GenerateValueOnAdd = true;

            // TODO: Key should not get by-convention value generation if it is dependent of identifying relationship
            var detailId = model.GetEntityType(typeof(ComputerDetail)).GetProperty("ComputerDetailId");
            detailId.GenerateValueOnAdd = false;

            var resolutionId = model.GetEntityType(typeof(Resolution)).GetProperty("ResolutionId");
            resolutionId.GenerateValueOnAdd = false;

            var customerId = model.GetEntityType(typeof(CustomerInfo)).GetProperty("CustomerInfoId");
            customerId.GenerateValueOnAdd = false;
        }
    }
}
