// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks;
using Microsoft.Data.Entity;
using System;

namespace EntityFramework.Microbenchmarks.Models.AdventureWorks
{
    public partial class AdventureWorksContext : DbContext
    {
        private readonly string _connectionString;

        public AdventureWorksContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public AdventureWorksContext(string connectionString, IServiceProvider serviceProvider)
            :base(serviceProvider)
        {
            _connectionString = connectionString;
        }

        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<AddressType> AddressType { get; set; }
        public virtual DbSet<BillOfMaterials> BillOfMaterials { get; set; }
        public virtual DbSet<BusinessEntity> BusinessEntity { get; set; }
        public virtual DbSet<BusinessEntityAddress> BusinessEntityAddress { get; set; }
        public virtual DbSet<BusinessEntityContact> BusinessEntityContact { get; set; }
        public virtual DbSet<ContactType> ContactType { get; set; }
        public virtual DbSet<CountryRegion> CountryRegion { get; set; }
        public virtual DbSet<CountryRegionCurrency> CountryRegionCurrency { get; set; }
        public virtual DbSet<CreditCard> CreditCard { get; set; }
        public virtual DbSet<Culture> Culture { get; set; }
        public virtual DbSet<Currency> Currency { get; set; }
        public virtual DbSet<CurrencyRate> CurrencyRate { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<EmailAddress> EmailAddress { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
        public virtual DbSet<EmployeePayHistory> EmployeePayHistory { get; set; }
        public virtual DbSet<Illustration> Illustration { get; set; }
        public virtual DbSet<JobCandidate> JobCandidate { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Password> Password { get; set; }
        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<PersonCreditCard> PersonCreditCard { get; set; }
        public virtual DbSet<PersonPhone> PersonPhone { get; set; }
        public virtual DbSet<PhoneNumberType> PhoneNumberType { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductCategory> ProductCategory { get; set; }
        public virtual DbSet<ProductCostHistory> ProductCostHistory { get; set; }
        public virtual DbSet<ProductDescription> ProductDescription { get; set; }
        public virtual DbSet<ProductDocument> ProductDocument { get; set; }
        public virtual DbSet<ProductInventory> ProductInventory { get; set; }
        public virtual DbSet<ProductListPriceHistory> ProductListPriceHistory { get; set; }
        public virtual DbSet<ProductModel> ProductModel { get; set; }
        public virtual DbSet<ProductModelIllustration> ProductModelIllustration { get; set; }
        public virtual DbSet<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCulture { get; set; }
        public virtual DbSet<ProductPhoto> ProductPhoto { get; set; }
        public virtual DbSet<ProductProductPhoto> ProductProductPhoto { get; set; }
        public virtual DbSet<ProductReview> ProductReview { get; set; }
        public virtual DbSet<ProductSubcategory> ProductSubcategory { get; set; }
        public virtual DbSet<ProductVendor> ProductVendor { get; set; }
        public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
        public virtual DbSet<PurchaseOrderHeader> PurchaseOrderHeader { get; set; }
        public virtual DbSet<SalesOrderDetail> SalesOrderDetail { get; set; }
        public virtual DbSet<SalesOrderHeader> SalesOrderHeader { get; set; }
        public virtual DbSet<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReason { get; set; }
        public virtual DbSet<SalesPerson> SalesPerson { get; set; }
        public virtual DbSet<SalesPersonQuotaHistory> SalesPersonQuotaHistory { get; set; }
        public virtual DbSet<SalesReason> SalesReason { get; set; }
        public virtual DbSet<SalesTaxRate> SalesTaxRate { get; set; }
        public virtual DbSet<SalesTerritory> SalesTerritory { get; set; }
        public virtual DbSet<SalesTerritoryHistory> SalesTerritoryHistory { get; set; }
        public virtual DbSet<ScrapReason> ScrapReason { get; set; }
        public virtual DbSet<Shift> Shift { get; set; }
        public virtual DbSet<ShipMethod> ShipMethod { get; set; }
        public virtual DbSet<ShoppingCartItem> ShoppingCartItem { get; set; }
        public virtual DbSet<SpecialOffer> SpecialOffer { get; set; }
        public virtual DbSet<SpecialOfferProduct> SpecialOfferProduct { get; set; }
        public virtual DbSet<StateProvince> StateProvince { get; set; }
        public virtual DbSet<Store> Store { get; set; }
        public virtual DbSet<TransactionHistory> TransactionHistory { get; set; }
        public virtual DbSet<TransactionHistoryArchive> TransactionHistoryArchive { get; set; }
        public virtual DbSet<UnitMeasure> UnitMeasure { get; set; }
        public virtual DbSet<Vendor> Vendor { get; set; }
        public virtual DbSet<WorkOrder> WorkOrder { get; set; }
        public virtual DbSet<WorkOrderRouting> WorkOrderRouting { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureModel(modelBuilder);
        }

        public static void ConfigureModel(ModelBuilder modelBuilder)
        { 
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address", "Person");

                entity.Property(e => e.AddressLine1).Required();

                entity.Property(e => e.City).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.PostalCode).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
                
                entity.Reference(d => d.StateProvince)
                      .InverseCollection(p => p.Address)
                      .ForeignKey(d => d.StateProvinceID);
            });

            modelBuilder.Entity<AddressType>(entity =>
            {
                entity.ToTable("AddressType", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

            modelBuilder.Entity<BillOfMaterials>(entity =>
            {
                entity.ToTable("BillOfMaterials", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.PerAssemblyQty)
                    .HasColumnType("decimal(8, 2)")
                    .HasDefaultValue(1.00m);

                entity.Property(e => e.StartDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.UnitMeasureCode).Required();

                entity.Reference(d => d.Component)
                      .InverseCollection(p => p.BillOfMaterials)
                      .ForeignKey(d => d.ComponentID);

                entity.Reference(d => d.ProductAssembly)
                      .InverseCollection(p => p.BillOfMaterialsNavigation)
                      .ForeignKey(d => d.ProductAssemblyID);

                entity.Reference(d => d.UnitMeasureCodeNavigation)
                      .InverseCollection(p => p.BillOfMaterials)
                      .ForeignKey(d => d.UnitMeasureCode);
            });

            modelBuilder.Entity<BusinessEntity>(entity =>
            {
                entity.ToTable("BusinessEntity", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

            modelBuilder.Entity<BusinessEntityAddress>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.AddressID, e.AddressTypeID });

                entity.ToTable("BusinessEntityAddress", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.Address)
                      .InverseCollection(p => p.BusinessEntityAddress)
                      .ForeignKey(d => d.AddressID);

                entity.Reference(d => d.AddressType)
                      .InverseCollection(p => p.BusinessEntityAddress)
                      .ForeignKey(d => d.AddressTypeID);

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.BusinessEntityAddress)
                      .ForeignKey(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<BusinessEntityContact>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.PersonID, e.ContactTypeID });

                entity.ToTable("BusinessEntityContact", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.BusinessEntityContact)
                      .ForeignKey(d => d.BusinessEntityID);

                entity.Reference(d => d.ContactType)
                      .InverseCollection(p => p.BusinessEntityContact)
                      .ForeignKey(d => d.ContactTypeID);

                entity.Reference(d => d.Person)
                      .InverseCollection(p => p.BusinessEntityContact)
                      .ForeignKey(d => d.PersonID);
            });

            modelBuilder.Entity<ContactType>(entity =>
            {
                entity.ToTable("ContactType", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<CountryRegion>(entity =>
            {
                entity.Key(e => e.CountryRegionCode);

                entity.ToTable("CountryRegion", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<CountryRegionCurrency>(entity =>
            {
                entity.Key(e => new { e.CountryRegionCode, e.CurrencyCode });

                entity.ToTable("CountryRegionCurrency", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.CountryRegionCodeNavigation)
                      .InverseCollection(p => p.CountryRegionCurrency)
                      .ForeignKey(d => d.CountryRegionCode);

                entity.Reference(d => d.CurrencyCodeNavigation)
                      .InverseCollection(p => p.CountryRegionCurrency)
                      .ForeignKey(d => d.CurrencyCode);
            });

            modelBuilder.Entity<CreditCard>(entity =>
            {
                entity.ToTable("CreditCard", "Sales");

                entity.Property(e => e.CardNumber).Required();

                entity.Property(e => e.CardType).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
            });

            modelBuilder.Entity<Culture>(entity =>
            {
                entity.ToTable("Culture", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.Key(e => e.CurrencyCode);

                entity.ToTable("Currency", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<CurrencyRate>(entity =>
            {
                entity.ToTable("CurrencyRate", "Sales");

                entity.Property(e => e.FromCurrencyCode).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ToCurrencyCode).Required();

                entity.Reference(d => d.FromCurrencyCodeNavigation)
                      .InverseCollection(p => p.CurrencyRate)
                      .ForeignKey(d => d.FromCurrencyCode);

                entity.Reference(d => d.ToCurrencyCodeNavigation)
                      .InverseCollection(p => p.CurrencyRateNavigation)
                      .ForeignKey(d => d.ToCurrencyCode);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer", "Sales");

                entity.Property(e => e.AccountNumber)
                    .Required()
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.Person)
                      .InverseCollection(p => p.Customer)
                      .ForeignKey(d => d.PersonID);

                entity.Reference(d => d.Store)
                      .InverseCollection(p => p.Customer)
                      .ForeignKey(d => d.StoreID);

                entity.Reference(d => d.Territory)
                      .InverseCollection(p => p.Customer)
                      .ForeignKey(d => d.TerritoryID);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Department", "HumanResources");

                entity.Property(e => e.GroupName).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<EmailAddress>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.EmailAddressID });

                entity.ToTable("EmailAddress", "Person");

                entity.Property(e => e.EmailAddress1).HasColumnName("EmailAddress");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.EmailAddress)
                      .ForeignKey(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Key(e => e.BusinessEntityID);

                entity.ToTable("Employee", "HumanResources");

                entity.Property(e => e.CurrentFlag).HasDefaultValue(true);

                entity.Property(e => e.Gender).Required();

                entity.Property(e => e.JobTitle).Required();

                entity.Property(e => e.LoginID).Required();

                entity.Property(e => e.MaritalStatus).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.NationalIDNumber).Required();

                entity.Property(e => e.OrganizationLevel).ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.SalariedFlag).HasDefaultValue(true);

                entity.Property(e => e.SickLeaveHours).HasDefaultValue(0);

                entity.Property(e => e.VacationHours).HasDefaultValue(0);

                entity.Reference(d => d.BusinessEntity)
                      .InverseReference(p => p.Employee)
                      .ForeignKey<Employee>(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<EmployeeDepartmentHistory>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.StartDate, e.DepartmentID, e.ShiftID });

                entity.ToTable("EmployeeDepartmentHistory", "HumanResources");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.EmployeeDepartmentHistory)
                      .ForeignKey(d => d.BusinessEntityID);

                entity.Reference(d => d.Department)
                      .InverseCollection(p => p.EmployeeDepartmentHistory)
                      .ForeignKey(d => d.DepartmentID);

                entity.Reference(d => d.Shift)
                      .InverseCollection(p => p.EmployeeDepartmentHistory)
                      .ForeignKey(d => d.ShiftID);
            });

            modelBuilder.Entity<EmployeePayHistory>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.RateChangeDate });

                entity.ToTable("EmployeePayHistory", "HumanResources");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.EmployeePayHistory)
                      .ForeignKey(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<Illustration>(entity =>
            {
                entity.ToTable("Illustration", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
            });

            modelBuilder.Entity<JobCandidate>(entity =>
            {
                entity.ToTable("JobCandidate", "HumanResources");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.JobCandidate)
                      .ForeignKey(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Location", "Production");

                entity.Property(e => e.Availability)
                    .HasColumnType("decimal(8, 2)")
                    .HasDefaultValue(0.00m);

                entity.Property(e => e.CostRate).HasDefaultValue(0.00m);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<Password>(entity =>
            {
                entity.Key(e => e.BusinessEntityID);

                entity.ToTable("Password", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.PasswordHash).Required();

                entity.Property(e => e.PasswordSalt).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.BusinessEntity).InverseReference(p => p.Password).ForeignKey<Password>(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.Key(e => e.BusinessEntityID);

                entity.ToTable("Person", "Person");

                entity.Property(e => e.EmailPromotion).HasDefaultValue(0);

                entity.Property(e => e.FirstName).Required();

                entity.Property(e => e.LastName).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.NameStyle).HasDefaultValue(false);

                entity.Property(e => e.PersonType).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseReference(p => p.Person)
                      .ForeignKey<Person>(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<PersonCreditCard>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.CreditCardID });

                entity.ToTable("PersonCreditCard", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.PersonCreditCard)
                      .ForeignKey(d => d.BusinessEntityID);

                entity.Reference(d => d.CreditCard)
                      .InverseCollection(p => p.PersonCreditCard)
                      .ForeignKey(d => d.CreditCardID);
            });

            modelBuilder.Entity<PersonPhone>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.PhoneNumber, e.PhoneNumberTypeID });

                entity.ToTable("PersonPhone", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.PersonPhone)
                      .ForeignKey(d => d.BusinessEntityID);

                entity.Reference(d => d.PhoneNumberType)
                      .InverseCollection(p => p.PersonPhone)
                      .ForeignKey(d => d.PhoneNumberTypeID);
            });

            modelBuilder.Entity<PhoneNumberType>(entity =>
            {
                entity.ToTable("PhoneNumberType", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product", "Production");

                entity.Property(e => e.FinishedGoodsFlag).HasDefaultValue(true);

                entity.Property(e => e.MakeFlag).HasDefaultValue(true);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.ProductNumber).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.Weight).HasColumnType("decimal(8, 2)");

                entity.Reference(d => d.ProductModel)
                      .InverseCollection(p => p.Product)
                      .ForeignKey(d => d.ProductModelID);

                entity.Reference(d => d.ProductSubcategory)
                      .InverseCollection(p => p.Product)
                      .ForeignKey(d => d.ProductSubcategoryID);

                entity.Reference(d => d.SizeUnitMeasureCodeNavigation)
                      .InverseCollection(p => p.Product)
                      .ForeignKey(d => d.SizeUnitMeasureCode);

                entity.Reference(d => d.WeightUnitMeasureCodeNavigation)
                      .InverseCollection(p => p.ProductNavigation)
                      .ForeignKey(d => d.WeightUnitMeasureCode);
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

            modelBuilder.Entity<ProductCostHistory>(entity =>
            {
                entity.Key(e => new { e.ProductID, e.StartDate });

                entity.ToTable("ProductCostHistory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.ProductCostHistory)
                      .ForeignKey(d => d.ProductID);
            });

            modelBuilder.Entity<ProductDescription>(entity =>
            {
                entity.ToTable("ProductDescription", "Production");

                entity.Property(e => e.Description).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

            modelBuilder.Entity<ProductDocument>(entity =>
            {
                entity.Key(e => new { e.ProductID, e.DocumentNode });

                entity.ToTable("ProductDocument", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.ProductDocument)
                      .ForeignKey(d => d.ProductID);
            });

            modelBuilder.Entity<ProductInventory>(entity =>
            {
                entity.Key(e => new { e.ProductID, e.LocationID });

                entity.ToTable("ProductInventory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Quantity).HasDefaultValue(0);

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.Shelf).Required();

                entity.Reference(d => d.Location)
                      .InverseCollection(p => p.ProductInventory)
                      .ForeignKey(d => d.LocationID);

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.ProductInventory)
                      .ForeignKey(d => d.ProductID);
            });

            modelBuilder.Entity<ProductListPriceHistory>(entity =>
            {
                entity.Key(e => new { e.ProductID, e.StartDate });

                entity.ToTable("ProductListPriceHistory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.ProductListPriceHistory)
                      .ForeignKey(d => d.ProductID);
            });

            modelBuilder.Entity<ProductModel>(entity =>
            {
                entity.ToTable("ProductModel", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

            modelBuilder.Entity<ProductModelIllustration>(entity =>
            {
                entity.Key(e => new { e.ProductModelID, e.IllustrationID });

                entity.ToTable("ProductModelIllustration", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.Illustration)
                      .InverseCollection(p => p.ProductModelIllustration)
                      .ForeignKey(d => d.IllustrationID);

                entity.Reference(d => d.ProductModel)
                      .InverseCollection(p => p.ProductModelIllustration)
                      .ForeignKey(d => d.ProductModelID);
            });

            modelBuilder.Entity<ProductModelProductDescriptionCulture>(entity =>
            {
                entity.Key(e => new { e.ProductModelID, e.ProductDescriptionID, e.CultureID });

                entity.ToTable("ProductModelProductDescriptionCulture", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.Culture)
                      .InverseCollection(p => p.ProductModelProductDescriptionCulture)
                      .ForeignKey(d => d.CultureID);

                entity.Reference(d => d.ProductDescription)
                      .InverseCollection(p => p.ProductModelProductDescriptionCulture)
                      .ForeignKey(d => d.ProductDescriptionID);

                entity.Reference(d => d.ProductModel)
                      .InverseCollection(p => p.ProductModelProductDescriptionCulture)
                      .ForeignKey(d => d.ProductModelID);
            });

            modelBuilder.Entity<ProductPhoto>(entity =>
            {
                entity.ToTable("ProductPhoto", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
            });

            modelBuilder.Entity<ProductProductPhoto>(entity =>
            {
                entity.Key(e => new { e.ProductID, e.ProductPhotoID });

                entity.ToTable("ProductProductPhoto", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Primary).HasDefaultValue(false);

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.ProductProductPhoto)
                      .ForeignKey(d => d.ProductID);

                entity.Reference(d => d.ProductPhoto)
                      .InverseCollection(p => p.ProductProductPhoto)
                      .ForeignKey(d => d.ProductPhotoID);
            });

            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.ToTable("ProductReview", "Production");

                entity.Property(e => e.EmailAddress).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ReviewDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ReviewerName).Required();

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.ProductReview)
                      .ForeignKey(d => d.ProductID);
            });

            modelBuilder.Entity<ProductSubcategory>(entity =>
            {
                entity.ToTable("ProductSubcategory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.ProductCategory)
                      .InverseCollection(p => p.ProductSubcategory)
                      .ForeignKey(d => d.ProductCategoryID);
            });

            modelBuilder.Entity<ProductVendor>(entity =>
            {
                entity.Key(e => new { e.ProductID, e.BusinessEntityID });

                entity.ToTable("ProductVendor", "Purchasing");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.UnitMeasureCode).Required();

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.ProductVendor)
                      .ForeignKey(d => d.BusinessEntityID);

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.ProductVendor)
                      .ForeignKey(d => d.ProductID);

                entity.Reference(d => d.UnitMeasureCodeNavigation)
                      .InverseCollection(p => p.ProductVendor)
                      .ForeignKey(d => d.UnitMeasureCode);
            });

            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.Key(e => new { e.PurchaseOrderID, e.PurchaseOrderDetailID });

                entity.ToTable("PurchaseOrderDetail", "Purchasing");

                entity.Property(e => e.LineTotal).ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ReceivedQty).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.RejectedQty).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.StockedQty)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("decimal(9, 2)");

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.PurchaseOrderDetail)
                      .ForeignKey(d => d.ProductID);

                entity.Reference(d => d.PurchaseOrder)
                      .InverseCollection(p => p.PurchaseOrderDetail)
                      .ForeignKey(d => d.PurchaseOrderID);
            });

            modelBuilder.Entity<PurchaseOrderHeader>(entity =>
            {
                entity.Key(e => e.PurchaseOrderID);

                entity.ToTable("PurchaseOrderHeader", "Purchasing");

                entity.Property(e => e.Freight).HasDefaultValue(0.00m);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.OrderDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.RevisionNumber).HasDefaultValue(0);

                entity.Property(e => e.Status).HasDefaultValue(1);

                entity.Property(e => e.SubTotal).HasDefaultValue(0.00m);

                entity.Property(e => e.TaxAmt).HasDefaultValue(0.00m);

                entity.Property(e => e.TotalDue).ValueGeneratedOnAddOrUpdate();

                entity.Reference(d => d.Employee)
                      .InverseCollection(p => p.PurchaseOrderHeader)
                      .ForeignKey(d => d.EmployeeID);

                entity.Reference(d => d.ShipMethod)
                      .InverseCollection(p => p.PurchaseOrderHeader)
                      .ForeignKey(d => d.ShipMethodID);

                entity.Reference(d => d.Vendor)
                      .InverseCollection(p => p.PurchaseOrderHeader)
                      .ForeignKey(d => d.VendorID);
            });

            modelBuilder.Entity<SalesOrderDetail>(entity =>
            {
                entity.Key(e => new { e.SalesOrderID, e.SalesOrderDetailID });

                entity.ToTable("SalesOrderDetail", "Sales");

                entity.Property(e => e.LineTotal)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("numeric(38, 6)");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.UnitPriceDiscount).HasDefaultValue(0.0m);

                entity.Reference(d => d.SalesOrder)
                      .InverseCollection(p => p.SalesOrderDetail)
                      .ForeignKey(d => d.SalesOrderID);

                entity.Reference(d => d.SpecialOfferProduct)
                      .InverseCollection(p => p.SalesOrderDetail)
                      .ForeignKey(d => new { d.SpecialOfferID, d.ProductID });
            });

            modelBuilder.Entity<SalesOrderHeader>(entity =>
            {
                entity.Key(e => e.SalesOrderID);

                entity.ToTable("SalesOrderHeader", "Sales");

                entity.Property(e => e.Freight).HasDefaultValue(0.00m);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.OnlineOrderFlag).HasDefaultValue(true);

                entity.Property(e => e.OrderDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.RevisionNumber).HasDefaultValue(0);

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.SalesOrderNumber)
                    .Required()
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Status).HasDefaultValue(1);

                entity.Property(e => e.SubTotal).HasDefaultValue(0.00m);

                entity.Property(e => e.TaxAmt).HasDefaultValue(0.00m);

                entity.Property(e => e.TotalDue).ValueGeneratedOnAddOrUpdate();

                entity.Reference(d => d.BillToAddress)
                      .InverseCollection(p => p.SalesOrderHeader)
                      .ForeignKey(d => d.BillToAddressID);

                entity.Reference(d => d.CreditCard)
                      .InverseCollection(p => p.SalesOrderHeader)
                      .ForeignKey(d => d.CreditCardID);

                entity.Reference(d => d.CurrencyRate)
                      .InverseCollection(p => p.SalesOrderHeader)
                      .ForeignKey(d => d.CurrencyRateID);

                entity.Reference(d => d.Customer)
                      .InverseCollection(p => p.SalesOrderHeader)
                      .ForeignKey(d => d.CustomerID);

                entity.Reference(d => d.SalesPerson)
                      .InverseCollection(p => p.SalesOrderHeader)
                      .ForeignKey(d => d.SalesPersonID);

                entity.Reference(d => d.ShipMethod)
                      .InverseCollection(p => p.SalesOrderHeader)
                      .ForeignKey(d => d.ShipMethodID);

                entity.Reference(d => d.ShipToAddress)
                      .InverseCollection(p => p.SalesOrderHeaderNavigation)
                      .ForeignKey(d => d.ShipToAddressID);

                entity.Reference(d => d.Territory)
                      .InverseCollection(p => p.SalesOrderHeader)
                      .ForeignKey(d => d.TerritoryID);
            });

            modelBuilder.Entity<SalesOrderHeaderSalesReason>(entity =>
            {
                entity.Key(e => new { e.SalesOrderID, e.SalesReasonID });

                entity.ToTable("SalesOrderHeaderSalesReason", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Reference(d => d.SalesOrder)
                      .InverseCollection(p => p.SalesOrderHeaderSalesReason)
                      .ForeignKey(d => d.SalesOrderID);

                entity.Reference(d => d.SalesReason)
                      .InverseCollection(p => p.SalesOrderHeaderSalesReason)
                      .ForeignKey(d => d.SalesReasonID);
            });

            modelBuilder.Entity<SalesPerson>(entity =>
            {
                entity.Key(e => e.BusinessEntityID);

                entity.ToTable("SalesPerson", "Sales");

                entity.Property(e => e.Bonus).HasDefaultValue(0.00m);

                entity.Property(e => e.CommissionPct).HasDefaultValue(0.00m);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.SalesLastYear) .HasDefaultValue(0.00m);

                entity.Property(e => e.SalesYTD).HasDefaultValue(0.00m);

                entity.Reference(d => d.BusinessEntity)
                      .InverseReference(p => p.SalesPerson)
                      .ForeignKey<SalesPerson>(d => d.BusinessEntityID);

                entity.Reference(d => d.Territory)
                      .InverseCollection(p => p.SalesPerson)
                      .ForeignKey(d => d.TerritoryID);
            });

            modelBuilder.Entity<SalesPersonQuotaHistory>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.QuotaDate });

                entity.ToTable("SalesPersonQuotaHistory", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
                
                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.SalesPersonQuotaHistory)
                      .ForeignKey(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<SalesReason>(entity =>
            {
                entity.ToTable("SalesReason", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.ReasonType).Required();
            });

            modelBuilder.Entity<SalesTaxRate>(entity =>
            {
                entity.ToTable("SalesTaxRate", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
                
                entity.Property(e => e.TaxRate).HasDefaultValue(0.00m);
                
                entity.Reference(d => d.StateProvince)
                      .InverseCollection(p => p.SalesTaxRate)
                      .ForeignKey(d => d.StateProvinceID);
            });

            modelBuilder.Entity<SalesTerritory>(entity =>
            {
                entity.Key(e => e.TerritoryID);

                entity.ToTable("SalesTerritory", "Sales");

                entity.Property(e => e.CostLastYear).HasDefaultValue(0.00m);

                entity.Property(e => e.CostYTD).HasDefaultValue(0.00m);

                entity.Property(e => e.CountryRegionCode).Required();

                entity.Property(e => e.Group).Required();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.SalesLastYear).HasDefaultValue(0.00m);

                entity.Property(e => e.SalesYTD).HasDefaultValue(0.00m);

                entity.Reference(d => d.CountryRegionCodeNavigation)
                      .InverseCollection(p => p.SalesTerritory)
                      .ForeignKey(d => d.CountryRegionCode);
            });

            modelBuilder.Entity<SalesTerritoryHistory>(entity =>
            {
                entity.Key(e => new { e.BusinessEntityID, e.StartDate, e.TerritoryID });

                entity.ToTable("SalesTerritoryHistory", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseCollection(p => p.SalesTerritoryHistory)
                      .ForeignKey(d => d.BusinessEntityID);

                entity.Reference(d => d.Territory)
                      .InverseCollection(p => p.SalesTerritoryHistory)
                      .ForeignKey(d => d.TerritoryID);
            });

            modelBuilder.Entity<ScrapReason>(entity =>
            {
                entity.ToTable("ScrapReason", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<Shift>(entity =>
            {
                entity.ToTable("Shift", "HumanResources");
                
                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<ShipMethod>(entity =>
            {
                entity.ToTable("ShipMethod", "Purchasing");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.ShipBase).HasDefaultValue(0.00m);

                entity.Property(e => e.ShipRate).HasDefaultValue(0.00m);
            });

            modelBuilder.Entity<ShoppingCartItem>(entity =>
            {
                entity.ToTable("ShoppingCartItem", "Sales");

                entity.Property(e => e.DateCreated) .HasDefaultValueSql("getdate()");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
                
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                
                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.ShoppingCartItem)
                      .ForeignKey(d => d.ProductID);
            });

            modelBuilder.Entity<SpecialOffer>(entity =>
            {
                entity.ToTable("SpecialOffer", "Sales");

                entity.Property(e => e.Category).Required();

                entity.Property(e => e.Description).Required();

                entity.Property(e => e.DiscountPct).HasDefaultValue(0.00m);
                
                entity.Property(e => e.MinQty).HasDefaultValue(0);

                entity.Property(e => e.ModifiedDate) .HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
                
                entity.Property(e => e.Type).Required();
            });

            modelBuilder.Entity<SpecialOfferProduct>(entity =>
            {
                entity.Key(e => new { e.SpecialOfferID, e.ProductID });

                entity.ToTable("SpecialOfferProduct", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.SpecialOfferProduct)
                      .ForeignKey(d => d.ProductID);

                entity.Reference(d => d.SpecialOffer)
                      .InverseCollection(p => p.SpecialOfferProduct)
                      .ForeignKey(d => d.SpecialOfferID);
            });

            modelBuilder.Entity<StateProvince>(entity =>
            {
                entity.ToTable("StateProvince", "Person");

                entity.Property(e => e.CountryRegionCode).Required();

                entity.Property(e => e.IsOnlyStateProvinceFlag).HasDefaultValue(true);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.StateProvinceCode).Required();
                
                entity.Reference(d => d.CountryRegionCodeNavigation)
                      .InverseCollection(p => p.StateProvince)
                      .ForeignKey(d => d.CountryRegionCode);

                entity.Reference(d => d.Territory)
                      .InverseCollection(p => p.StateProvince)
                      .ForeignKey(d => d.TerritoryID);
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.Key(e => e.BusinessEntityID);

                entity.ToTable("Store", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Reference(d => d.BusinessEntity)
                      .InverseReference(p => p.Store)
                      .ForeignKey<Store>(d => d.BusinessEntityID);

                entity.Reference(d => d.SalesPerson)
                      .InverseCollection(p => p.Store)
                      .ForeignKey(d => d.SalesPersonID);
            });

            modelBuilder.Entity<TransactionHistory>(entity =>
            {
                entity.Key(e => e.TransactionID);

                entity.ToTable("TransactionHistory", "Production");
                
                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
                
                entity.Property(e => e.ReferenceOrderLineID).HasDefaultValue(0);

                entity.Property(e => e.TransactionDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.TransactionType).Required();

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.TransactionHistory)
                      .ForeignKey(d => d.ProductID);
            });

            modelBuilder.Entity<TransactionHistoryArchive>(entity =>
            {
                entity.Key(e => e.TransactionID);

                entity.ToTable("TransactionHistoryArchive", "Production");
                
                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
                
                entity.Property(e => e.ReferenceOrderLineID).HasDefaultValue(0);

                entity.Property(e => e.TransactionDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.TransactionType).Required();
            });

            modelBuilder.Entity<UnitMeasure>(entity =>
            {
                entity.Key(e => e.UnitMeasureCode);

                entity.ToTable("UnitMeasure", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();
            });

            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.Key(e => e.BusinessEntityID);

                entity.ToTable("Vendor", "Purchasing");

                entity.Property(e => e.AccountNumber).Required();

                entity.Property(e => e.ActiveFlag).HasDefaultValue(true);
                
                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).Required();

                entity.Property(e => e.PreferredVendorStatus).HasDefaultValue(true);

                entity.Reference(d => d.BusinessEntity)
                      .InverseReference(p => p.Vendor)
                      .ForeignKey<Vendor>(d => d.BusinessEntityID);
            });

            modelBuilder.Entity<WorkOrder>(entity =>
            {
                entity.ToTable("WorkOrder", "Production");
                
                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
                
                entity.Property(e => e.StockedQty).ValueGeneratedOnAddOrUpdate();

                entity.Reference(d => d.Product)
                      .InverseCollection(p => p.WorkOrder)
                      .ForeignKey(d => d.ProductID);

                entity.Reference(d => d.ScrapReason)
                      .InverseCollection(p => p.WorkOrder)
                      .ForeignKey(d => d.ScrapReasonID);
            });

            modelBuilder.Entity<WorkOrderRouting>(entity =>
            {
                entity.Key(e => new { e.WorkOrderID, e.ProductID, e.OperationSequence });

                entity.ToTable("WorkOrderRouting", "Production");

                entity.Property(e => e.ActualResourceHrs).HasColumnType("decimal(9, 4)");
                
                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
                
                entity.Reference(d => d.Location)
                      .InverseCollection(p => p.WorkOrderRouting)
                      .ForeignKey(d => d.LocationID);

                entity.Reference(d => d.WorkOrder)
                      .InverseCollection(p => p.WorkOrderRouting)
                      .ForeignKey(d => d.WorkOrderID);
            });
        }
    }
}