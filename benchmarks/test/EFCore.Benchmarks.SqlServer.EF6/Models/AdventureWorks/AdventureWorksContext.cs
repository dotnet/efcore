// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class AdventureWorksContext : DbContext
    {
        public AdventureWorksContext(string connectionString)
            : base(connectionString)
        {
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ConfigureModel(modelBuilder);
        }

        public static void ConfigureModel(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>().ToTable("Address", "Person");

            modelBuilder.Entity<Address>().Property(e => e.AddressLine1).IsRequired();

            modelBuilder.Entity<Address>().Property(e => e.City).IsRequired();

            modelBuilder.Entity<Address>().Property(e => e.PostalCode).IsRequired();

            modelBuilder.Entity<Address>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Address>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Address>()
                .HasRequired(d => d.StateProvince)
                .WithMany(p => p.Address)
                .HasForeignKey(d => d.StateProvinceID);

            modelBuilder.Entity<AddressType>().ToTable("AddressType", "Person");

            modelBuilder.Entity<AddressType>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<AddressType>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<AddressType>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BillOfMaterials>().ToTable("BillOfMaterials", "Production");

            modelBuilder.Entity<BillOfMaterials>().Property(e => e.PerAssemblyQty).HasPrecision(8, 2);

            modelBuilder.Entity<BillOfMaterials>().Property(e => e.UnitMeasureCode).IsRequired();

            modelBuilder.Entity<BillOfMaterials>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BillOfMaterials>().Property(e => e.StartDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BillOfMaterials>()
                .HasRequired(d => d.Component)
                .WithMany(p => p.BillOfMaterials)
                .HasForeignKey(d => d.ComponentID);

            modelBuilder.Entity<BillOfMaterials>()
                .HasOptional(d => d.ProductAssembly)
                .WithMany(p => p.BillOfMaterialsNavigation)
                .HasForeignKey(d => d.ProductAssemblyID);

            modelBuilder.Entity<BillOfMaterials>()
                .HasRequired(d => d.UnitMeasureCodeNavigation)
                .WithMany(p => p.BillOfMaterials)
                .HasForeignKey(d => d.UnitMeasureCode);

            modelBuilder.Entity<BusinessEntity>().ToTable("BusinessEntity", "Person");

            modelBuilder.Entity<BusinessEntity>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BusinessEntity>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BusinessEntityAddress>().HasKey(
                e => new
                {
                    e.BusinessEntityID,
                    e.AddressID,
                    e.AddressTypeID
                });

            modelBuilder.Entity<BusinessEntityAddress>().ToTable("BusinessEntityAddress", "Person");

            modelBuilder.Entity<BusinessEntityAddress>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BusinessEntityAddress>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BusinessEntityAddress>()
                .HasRequired(d => d.Address)
                .WithMany(p => p.BusinessEntityAddress)
                .HasForeignKey(d => d.AddressID);

            modelBuilder.Entity<BusinessEntityAddress>()
                .HasRequired(d => d.AddressType)
                .WithMany(p => p.BusinessEntityAddress)
                .HasForeignKey(d => d.AddressTypeID);

            modelBuilder.Entity<BusinessEntityAddress>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.BusinessEntityAddress)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<BusinessEntityContact>().HasKey(e => new { e.BusinessEntityID, e.PersonID, e.ContactTypeID });

            modelBuilder.Entity<BusinessEntityContact>().ToTable("BusinessEntityContact", "Person");

            modelBuilder.Entity<BusinessEntityContact>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BusinessEntityContact>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<BusinessEntityContact>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.BusinessEntityContact)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<BusinessEntityContact>()
                .HasRequired(d => d.ContactType)
                .WithMany(p => p.BusinessEntityContact)
                .HasForeignKey(d => d.ContactTypeID);

            modelBuilder.Entity<BusinessEntityContact>()
                .HasRequired(d => d.Person)
                .WithMany(p => p.BusinessEntityContact)
                .HasForeignKey(d => d.PersonID);

            modelBuilder.Entity<ContactType>().ToTable("ContactType", "Person");

            modelBuilder.Entity<ContactType>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<ContactType>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<CountryRegion>().HasKey(e => e.CountryRegionCode);

            modelBuilder.Entity<CountryRegion>().ToTable("CountryRegion", "Person");

            modelBuilder.Entity<CountryRegion>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<CountryRegion>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<CountryRegionCurrency>().HasKey(e => new { e.CountryRegionCode, e.CurrencyCode });

            modelBuilder.Entity<CountryRegionCurrency>().ToTable("CountryRegionCurrency", "Sales");

            modelBuilder.Entity<CountryRegionCurrency>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<CountryRegionCurrency>()
                .HasRequired(d => d.CountryRegionCodeNavigation)
                .WithMany(p => p.CountryRegionCurrency)
                .HasForeignKey(d => d.CountryRegionCode);

            modelBuilder.Entity<CountryRegionCurrency>()
                .HasRequired(d => d.CurrencyCodeNavigation)
                .WithMany(p => p.CountryRegionCurrency)
                .HasForeignKey(d => d.CurrencyCode);

            modelBuilder.Entity<CreditCard>().ToTable("CreditCard", "Sales");

            modelBuilder.Entity<CreditCard>().Property(e => e.CardNumber).IsRequired();

            modelBuilder.Entity<CreditCard>().Property(e => e.CardType).IsRequired();

            modelBuilder.Entity<CreditCard>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Culture>().ToTable("Culture", "Production");

            modelBuilder.Entity<Culture>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<Culture>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Currency>().HasKey(e => e.CurrencyCode);

            modelBuilder.Entity<Currency>().ToTable("Currency", "Sales");

            modelBuilder.Entity<Currency>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<Currency>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<CurrencyRate>().ToTable("CurrencyRate", "Sales");

            modelBuilder.Entity<CurrencyRate>().Property(e => e.FromCurrencyCode).IsRequired();

            modelBuilder.Entity<CurrencyRate>().Property(e => e.ToCurrencyCode).IsRequired();

            modelBuilder.Entity<CurrencyRate>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<CurrencyRate>()
                .HasRequired(d => d.FromCurrencyCodeNavigation)
                .WithMany(p => p.CurrencyRate)
                .HasForeignKey(d => d.FromCurrencyCode);

            modelBuilder.Entity<CurrencyRate>()
                .HasRequired(d => d.ToCurrencyCodeNavigation)
                .WithMany(p => p.CurrencyRateNavigation)
                .HasForeignKey(d => d.ToCurrencyCode);

            modelBuilder.Entity<Customer>().ToTable("Customer", "Sales");

            modelBuilder.Entity<Customer>().Property(e => e.AccountNumber)
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Customer>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Customer>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Customer>()
                .HasRequired(d => d.Person)
                .WithMany(p => p.Customer)
                .HasForeignKey(d => d.PersonID);

            modelBuilder.Entity<Customer>()
                .HasRequired(d => d.Store)
                .WithMany(p => p.Customer)
                .HasForeignKey(d => d.StoreID);

            modelBuilder.Entity<Customer>()
                .HasRequired(d => d.Territory)
                .WithMany(p => p.Customer)
                .HasForeignKey(d => d.TerritoryID);

            modelBuilder.Entity<Department>().ToTable("Department", "HumanResources");

            modelBuilder.Entity<Department>().Property(e => e.GroupName).IsRequired();

            modelBuilder.Entity<Department>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<Department>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<EmailAddress>().HasKey(e => new { e.BusinessEntityID, e.EmailAddressID });

            modelBuilder.Entity<EmailAddress>().ToTable("EmailAddress", "Person");

            modelBuilder.Entity<EmailAddress>().Property(e => e.EmailAddress1).HasColumnName("EmailAddress");

            modelBuilder.Entity<EmailAddress>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<EmailAddress>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<EmailAddress>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.EmailAddress)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<Employee>().HasKey(e => e.BusinessEntityID);

            modelBuilder.Entity<Employee>().ToTable("Employee", "HumanResources");

            modelBuilder.Entity<Employee>().Property(e => e.Gender).IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.JobTitle).IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.LoginID).IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.MaritalStatus).IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.NationalIDNumber).IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.OrganizationLevel).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Employee>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Employee>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Employee>()
                .HasRequired(d => d.BusinessEntity)
                .WithOptional(p => p.Employee);

            modelBuilder.Entity<EmployeeDepartmentHistory>().HasKey(e => new { e.BusinessEntityID, e.StartDate, e.DepartmentID, e.ShiftID });

            modelBuilder.Entity<EmployeeDepartmentHistory>().ToTable("EmployeeDepartmentHistory", "HumanResources");

            modelBuilder.Entity<EmployeeDepartmentHistory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<EmployeeDepartmentHistory>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.EmployeeDepartmentHistory)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<EmployeeDepartmentHistory>()
                .HasRequired(d => d.Department)
                .WithMany(p => p.EmployeeDepartmentHistory)
                .HasForeignKey(d => d.DepartmentID);

            modelBuilder.Entity<EmployeeDepartmentHistory>()
                .HasRequired(d => d.Shift)
                .WithMany(p => p.EmployeeDepartmentHistory)
                .HasForeignKey(d => d.ShiftID);

            modelBuilder.Entity<EmployeePayHistory>().HasKey(e => new { e.BusinessEntityID, e.RateChangeDate });

            modelBuilder.Entity<EmployeePayHistory>().ToTable("EmployeePayHistory", "HumanResources");

            modelBuilder.Entity<EmployeePayHistory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<EmployeePayHistory>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.EmployeePayHistory)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<Illustration>().ToTable("Illustration", "Production");

            modelBuilder.Entity<Illustration>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<JobCandidate>().ToTable("JobCandidate", "HumanResources");

            modelBuilder.Entity<JobCandidate>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<JobCandidate>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.JobCandidate)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<Location>().ToTable("Location", "Production");

            modelBuilder.Entity<Location>().Property(e => e.Availability).HasPrecision(8, 2);

            modelBuilder.Entity<Location>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<Location>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Password>().HasKey(e => e.BusinessEntityID);

            modelBuilder.Entity<Password>().ToTable("Password", "Person");

            modelBuilder.Entity<Password>().Property(e => e.PasswordHash).IsRequired();

            modelBuilder.Entity<Password>().Property(e => e.PasswordSalt).IsRequired();

            modelBuilder.Entity<Password>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Password>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Password>()
                .HasRequired(d => d.BusinessEntity)
                .WithOptional(p => p.Password);

            modelBuilder.Entity<Person>().HasKey(e => e.BusinessEntityID);

            modelBuilder.Entity<Person>().ToTable("Person", "Person");

            modelBuilder.Entity<Person>().Property(e => e.FirstName).IsRequired();

            modelBuilder.Entity<Person>().Property(e => e.LastName).IsRequired();

            modelBuilder.Entity<Person>().Property(e => e.PersonType).IsRequired();

            modelBuilder.Entity<Person>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Person>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Person>()
                .HasRequired(d => d.BusinessEntity)
                .WithOptional(p => p.Person);

            modelBuilder.Entity<PersonCreditCard>().HasKey(e => new { e.BusinessEntityID, e.CreditCardID });

            modelBuilder.Entity<PersonCreditCard>().ToTable("PersonCreditCard", "Sales");

            modelBuilder.Entity<PersonCreditCard>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<PersonCreditCard>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.PersonCreditCard)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<PersonCreditCard>()
                .HasRequired(d => d.CreditCard)
                .WithMany(p => p.PersonCreditCard)
                .HasForeignKey(d => d.CreditCardID);

            modelBuilder.Entity<PersonPhone>().HasKey(e => new { e.BusinessEntityID, e.PhoneNumber, e.PhoneNumberTypeID });

            modelBuilder.Entity<PersonPhone>().ToTable("PersonPhone", "Person");

            modelBuilder.Entity<PersonPhone>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<PersonPhone>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.PersonPhone)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<PersonPhone>()
                .HasRequired(d => d.PhoneNumberType)
                .WithMany(p => p.PersonPhone)
                .HasForeignKey(d => d.PhoneNumberTypeID);

            modelBuilder.Entity<PhoneNumberType>().ToTable("PhoneNumberType", "Person");

            modelBuilder.Entity<PhoneNumberType>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<PhoneNumberType>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Product>().ToTable("Product", "Production");

            modelBuilder.Entity<Product>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<Product>().Property(e => e.ProductNumber).IsRequired();

            modelBuilder.Entity<Product>().Property(e => e.Weight).HasPrecision(8, 2);

            modelBuilder.Entity<Product>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Product>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Product>()
                .HasRequired(d => d.ProductModel)
                .WithMany(p => p.Product)
                .HasForeignKey(d => d.ProductModelID);

            modelBuilder.Entity<Product>()
                .HasRequired(d => d.ProductSubcategory)
                .WithMany(p => p.Product)
                .HasForeignKey(d => d.ProductSubcategoryID);

            modelBuilder.Entity<Product>()
                .HasRequired(d => d.SizeUnitMeasureCodeNavigation)
                .WithMany(p => p.Product)
                .HasForeignKey(d => d.SizeUnitMeasureCode);

            modelBuilder.Entity<Product>().HasRequired(d => d.WeightUnitMeasureCodeNavigation)
                .WithMany(p => p.ProductNavigation)
                .HasForeignKey(d => d.WeightUnitMeasureCode);

            modelBuilder.Entity<ProductCategory>().ToTable("ProductCategory", "Production");

            modelBuilder.Entity<ProductCategory>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<ProductCategory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductCategory>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductCostHistory>().HasKey(e => new { e.ProductID, e.StartDate });

            modelBuilder.Entity<ProductCostHistory>().ToTable("ProductCostHistory", "Production");

            modelBuilder.Entity<ProductCostHistory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductCostHistory>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.ProductCostHistory)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<ProductDescription>().ToTable("ProductDescription", "Production");

            modelBuilder.Entity<ProductDescription>().Property(e => e.Description).IsRequired();

            modelBuilder.Entity<ProductDescription>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductDescription>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductDocument>().HasKey(e => new { e.ProductID, e.DocumentNode });

            modelBuilder.Entity<ProductDocument>().ToTable("ProductDocument", "Production");

            modelBuilder.Entity<ProductDocument>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductDocument>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.ProductDocument)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<ProductInventory>().HasKey(e => new { e.ProductID, e.LocationID });

            modelBuilder.Entity<ProductInventory>().ToTable("ProductInventory", "Production");

            modelBuilder.Entity<ProductInventory>().Property(e => e.Shelf).IsRequired();

            modelBuilder.Entity<ProductInventory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductInventory>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductInventory>()
                .HasRequired(d => d.Location)
                .WithMany(p => p.ProductInventory)
                .HasForeignKey(d => d.LocationID);

            modelBuilder.Entity<ProductInventory>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.ProductInventory)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<ProductListPriceHistory>().HasKey(e => new { e.ProductID, e.StartDate });

            modelBuilder.Entity<ProductListPriceHistory>().ToTable("ProductListPriceHistory", "Production");

            modelBuilder.Entity<ProductListPriceHistory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductListPriceHistory>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.ProductListPriceHistory)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<ProductModel>().ToTable("ProductModel", "Production");

            modelBuilder.Entity<ProductModel>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<ProductModel>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductModel>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductModelIllustration>().HasKey(e => new { e.ProductModelID, e.IllustrationID });

            modelBuilder.Entity<ProductModelIllustration>().ToTable("ProductModelIllustration", "Production");

            modelBuilder.Entity<ProductModelIllustration>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductModelIllustration>()
                .HasRequired(d => d.Illustration)
                .WithMany(p => p.ProductModelIllustration)
                .HasForeignKey(d => d.IllustrationID);

            modelBuilder.Entity<ProductModelIllustration>()
                .HasRequired(d => d.ProductModel)
                .WithMany(p => p.ProductModelIllustration)
                .HasForeignKey(d => d.ProductModelID);

            modelBuilder.Entity<ProductModelProductDescriptionCulture>().HasKey(e => new { e.ProductModelID, e.ProductDescriptionID, e.CultureID });

            modelBuilder.Entity<ProductModelProductDescriptionCulture>().ToTable("ProductModelProductDescriptionCulture", "Production");

            modelBuilder.Entity<ProductModelProductDescriptionCulture>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductModelProductDescriptionCulture>()
                .HasRequired(d => d.Culture)
                .WithMany(p => p.ProductModelProductDescriptionCulture)
                .HasForeignKey(d => d.CultureID);

            modelBuilder.Entity<ProductModelProductDescriptionCulture>()
                .HasRequired(d => d.ProductDescription)
                .WithMany(p => p.ProductModelProductDescriptionCulture)
                .HasForeignKey(d => d.ProductDescriptionID);

            modelBuilder.Entity<ProductModelProductDescriptionCulture>()
                .HasRequired(d => d.ProductModel)
                .WithMany(p => p.ProductModelProductDescriptionCulture)
                .HasForeignKey(d => d.ProductModelID);

            modelBuilder.Entity<ProductPhoto>().ToTable("ProductPhoto", "Production");

            modelBuilder.Entity<ProductPhoto>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductProductPhoto>().HasKey(e => new { e.ProductID, e.ProductPhotoID });

            modelBuilder.Entity<ProductProductPhoto>().ToTable("ProductProductPhoto", "Production");

            modelBuilder.Entity<ProductProductPhoto>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductProductPhoto>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.ProductProductPhoto)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<ProductProductPhoto>()
                .HasRequired(d => d.ProductPhoto)
                .WithMany(p => p.ProductProductPhoto)
                .HasForeignKey(d => d.ProductPhotoID);

            modelBuilder.Entity<ProductReview>().ToTable("ProductReview", "Production");

            modelBuilder.Entity<ProductReview>().Property(e => e.EmailAddress).IsRequired();

            modelBuilder.Entity<ProductReview>().Property(e => e.ReviewerName).IsRequired();

            modelBuilder.Entity<ProductReview>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductReview>().Property(e => e.ReviewDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductReview>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.ProductReview)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<ProductSubcategory>().ToTable("ProductSubcategory", "Production");

            modelBuilder.Entity<ProductSubcategory>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<ProductSubcategory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductSubcategory>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductSubcategory>()
                .HasRequired(d => d.ProductCategory)
                .WithMany(p => p.ProductSubcategory)
                .HasForeignKey(d => d.ProductCategoryID);

            modelBuilder.Entity<ProductVendor>().HasKey(e => new { e.ProductID, e.BusinessEntityID });

            modelBuilder.Entity<ProductVendor>().ToTable("ProductVendor", "Purchasing");

            modelBuilder.Entity<ProductVendor>().Property(e => e.UnitMeasureCode).IsRequired();

            modelBuilder.Entity<ProductVendor>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ProductVendor>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.ProductVendor)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<ProductVendor>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.ProductVendor)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<ProductVendor>()
                .HasRequired(d => d.UnitMeasureCodeNavigation)
                .WithMany(p => p.ProductVendor)
                .HasForeignKey(d => d.UnitMeasureCode);

            modelBuilder.Entity<PurchaseOrderDetail>().HasKey(e => new { e.PurchaseOrderID, e.PurchaseOrderDetailID });

            modelBuilder.Entity<PurchaseOrderDetail>().ToTable("PurchaseOrderDetail", "Purchasing");

            modelBuilder.Entity<PurchaseOrderDetail>().Property(e => e.ReceivedQty).HasPrecision(8, 2);

            modelBuilder.Entity<PurchaseOrderDetail>().Property(e => e.RejectedQty).HasPrecision(8, 2);

            modelBuilder.Entity<PurchaseOrderDetail>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<PurchaseOrderDetail>().Property(e => e.StockedQty)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasPrecision(9, 2);

            modelBuilder.Entity<PurchaseOrderDetail>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.PurchaseOrderDetail)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<PurchaseOrderDetail>()
                .HasRequired(d => d.PurchaseOrder)
                .WithMany(p => p.PurchaseOrderDetail)
                .HasForeignKey(d => d.PurchaseOrderID);

            modelBuilder.Entity<PurchaseOrderHeader>().HasKey(e => e.PurchaseOrderID);

            modelBuilder.Entity<PurchaseOrderHeader>().ToTable("PurchaseOrderHeader", "Purchasing");

            modelBuilder.Entity<PurchaseOrderHeader>().Property(e => e.TotalDue).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<PurchaseOrderHeader>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<PurchaseOrderHeader>().Property(e => e.OrderDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<PurchaseOrderHeader>()
                .HasRequired(d => d.Employee)
                .WithMany(p => p.PurchaseOrderHeader)
                .HasForeignKey(d => d.EmployeeID);

            modelBuilder.Entity<PurchaseOrderHeader>()
                .HasRequired(d => d.ShipMethod)
                .WithMany(p => p.PurchaseOrderHeader)
                .HasForeignKey(d => d.ShipMethodID);

            modelBuilder.Entity<PurchaseOrderHeader>()
                .HasRequired(d => d.Vendor)
                .WithMany(p => p.PurchaseOrderHeader)
                .HasForeignKey(d => d.VendorID);

            modelBuilder.Entity<SalesOrderDetail>().HasKey(e => new { e.SalesOrderID, e.SalesOrderDetailID });

            modelBuilder.Entity<SalesOrderDetail>().ToTable("SalesOrderDetail", "Sales");

            modelBuilder.Entity<SalesOrderDetail>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesOrderDetail>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesOrderDetail>().Property(e => e.LineTotal)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnType("numeric")
                .HasPrecision(38, 6);

            modelBuilder.Entity<SalesOrderDetail>()
                .HasRequired(d => d.SalesOrder)
                .WithMany(p => p.SalesOrderDetail)
                .HasForeignKey(d => d.SalesOrderID);

            modelBuilder.Entity<SalesOrderDetail>()
                .HasRequired(d => d.SpecialOfferProduct)
                .WithMany(p => p.SalesOrderDetail)
                .HasForeignKey(d => new { d.SpecialOfferID, d.ProductID });

            modelBuilder.Entity<SalesOrderHeader>().HasKey(e => e.SalesOrderID);

            modelBuilder.Entity<SalesOrderHeader>().ToTable("SalesOrderHeader", "Sales");

            modelBuilder.Entity<SalesOrderHeader>().Property(e => e.SalesOrderNumber)
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<SalesOrderHeader>().Property(e => e.TotalDue).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<SalesOrderHeader>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesOrderHeader>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesOrderHeader>().Property(e => e.OrderDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasRequired(d => d.BillToAddress)
                .WithMany(p => p.SalesOrderHeader)
                .HasForeignKey(d => d.BillToAddressID);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasOptional(d => d.CreditCard)
                .WithMany(p => p.SalesOrderHeader)
                .HasForeignKey(d => d.CreditCardID);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasOptional(d => d.CurrencyRate)
                .WithMany(p => p.SalesOrderHeader)
                .HasForeignKey(d => d.CurrencyRateID);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasRequired(d => d.Customer)
                .WithMany(p => p.SalesOrderHeader)
                .HasForeignKey(d => d.CustomerID);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasOptional(d => d.SalesPerson)
                .WithMany(p => p.SalesOrderHeader)
                .HasForeignKey(d => d.SalesPersonID);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasRequired(d => d.ShipMethod)
                .WithMany(p => p.SalesOrderHeader)
                .HasForeignKey(d => d.ShipMethodID);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasRequired(d => d.ShipToAddress)
                .WithMany(p => p.SalesOrderHeaderNavigation)
                .HasForeignKey(d => d.ShipToAddressID);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasOptional(d => d.Territory)
                .WithMany(p => p.SalesOrderHeader)
                .HasForeignKey(d => d.TerritoryID);

            modelBuilder.Entity<SalesOrderHeaderSalesReason>().HasKey(e => new { e.SalesOrderID, e.SalesReasonID });

            modelBuilder.Entity<SalesOrderHeaderSalesReason>().ToTable("SalesOrderHeaderSalesReason", "Sales");

            modelBuilder.Entity<SalesOrderHeaderSalesReason>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesOrderHeaderSalesReason>()
                .HasRequired(d => d.SalesOrder)
                .WithMany(p => p.SalesOrderHeaderSalesReason)
                .HasForeignKey(d => d.SalesOrderID);

            modelBuilder.Entity<SalesOrderHeaderSalesReason>()
                .HasRequired(d => d.SalesReason)
                .WithMany(p => p.SalesOrderHeaderSalesReason)
                .HasForeignKey(d => d.SalesReasonID);

            modelBuilder.Entity<SalesPerson>().HasKey(e => e.BusinessEntityID);

            modelBuilder.Entity<SalesPerson>().ToTable("SalesPerson", "Sales");

            modelBuilder.Entity<SalesPerson>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesPerson>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesPerson>()
                .HasRequired(d => d.BusinessEntity)
                .WithOptional(p => p.SalesPerson);

            modelBuilder.Entity<SalesPerson>()
                .HasOptional(d => d.Territory)
                .WithMany(p => p.SalesPerson)
                .HasForeignKey(d => d.TerritoryID);

            modelBuilder.Entity<SalesPersonQuotaHistory>().HasKey(e => new { e.BusinessEntityID, e.QuotaDate });

            modelBuilder.Entity<SalesPersonQuotaHistory>().ToTable("SalesPersonQuotaHistory", "Sales");

            modelBuilder.Entity<SalesPersonQuotaHistory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesPersonQuotaHistory>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesPersonQuotaHistory>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.SalesPersonQuotaHistory)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<SalesReason>().ToTable("SalesReason", "Sales");

            modelBuilder.Entity<SalesReason>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<SalesReason>().Property(e => e.ReasonType).IsRequired();

            modelBuilder.Entity<SalesReason>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesTaxRate>().ToTable("SalesTaxRate", "Sales");

            modelBuilder.Entity<SalesTaxRate>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<SalesTaxRate>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesTaxRate>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesTaxRate>()
                .HasRequired(d => d.StateProvince)
                .WithMany(p => p.SalesTaxRate)
                .HasForeignKey(d => d.StateProvinceID);

            modelBuilder.Entity<SalesTerritory>().HasKey(e => e.TerritoryID);

            modelBuilder.Entity<SalesTerritory>().ToTable("SalesTerritory", "Sales");

            modelBuilder.Entity<SalesTerritory>().Property(e => e.CountryRegionCode).IsRequired();

            modelBuilder.Entity<SalesTerritory>().Property(e => e.Group).IsRequired();

            modelBuilder.Entity<SalesTerritory>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<SalesTerritory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesTerritory>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesTerritory>()
                .HasRequired(d => d.CountryRegionCodeNavigation)
                .WithMany(p => p.SalesTerritory)
                .HasForeignKey(d => d.CountryRegionCode);

            modelBuilder.Entity<SalesTerritoryHistory>().HasKey(e => new { e.BusinessEntityID, e.StartDate, e.TerritoryID });

            modelBuilder.Entity<SalesTerritoryHistory>().ToTable("SalesTerritoryHistory", "Sales");

            modelBuilder.Entity<SalesTerritoryHistory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesTerritoryHistory>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SalesTerritoryHistory>()
                .HasRequired(d => d.BusinessEntity)
                .WithMany(p => p.SalesTerritoryHistory)
                .HasForeignKey(d => d.BusinessEntityID);

            modelBuilder.Entity<SalesTerritoryHistory>()
                .HasRequired(d => d.Territory)
                .WithMany(p => p.SalesTerritoryHistory)
                .HasForeignKey(d => d.TerritoryID);

            modelBuilder.Entity<ScrapReason>().ToTable("ScrapReason", "Production");

            modelBuilder.Entity<ScrapReason>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<ScrapReason>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Shift>().ToTable("Shift", "HumanResources");

            modelBuilder.Entity<Shift>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<Shift>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ShipMethod>().ToTable("ShipMethod", "Purchasing");

            modelBuilder.Entity<ShipMethod>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<ShipMethod>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ShipMethod>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ShoppingCartItem>().ToTable("ShoppingCartItem", "Sales");

            modelBuilder.Entity<ShoppingCartItem>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ShoppingCartItem>().Property(e => e.DateCreated).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<ShoppingCartItem>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.ShoppingCartItem)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<SpecialOffer>().ToTable("SpecialOffer", "Sales");

            modelBuilder.Entity<SpecialOffer>().Property(e => e.Category).IsRequired();

            modelBuilder.Entity<SpecialOffer>().Property(e => e.Description).IsRequired();

            modelBuilder.Entity<SpecialOffer>().Property(e => e.Type).IsRequired();

            modelBuilder.Entity<SpecialOffer>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SpecialOffer>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SpecialOfferProduct>().HasKey(e => new { e.SpecialOfferID, e.ProductID });

            modelBuilder.Entity<SpecialOfferProduct>().ToTable("SpecialOfferProduct", "Sales");

            modelBuilder.Entity<SpecialOfferProduct>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SpecialOfferProduct>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<SpecialOfferProduct>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.SpecialOfferProduct)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<SpecialOfferProduct>()
                .HasRequired(d => d.SpecialOffer)
                .WithMany(p => p.SpecialOfferProduct)
                .HasForeignKey(d => d.SpecialOfferID);

            modelBuilder.Entity<StateProvince>().ToTable("StateProvince", "Person");

            modelBuilder.Entity<StateProvince>().Property(e => e.CountryRegionCode).IsRequired();

            modelBuilder.Entity<StateProvince>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<StateProvince>().Property(e => e.StateProvinceCode).IsRequired();

            modelBuilder.Entity<StateProvince>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<StateProvince>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<StateProvince>()
                .HasRequired(d => d.CountryRegionCodeNavigation)
                .WithMany(p => p.StateProvince)
                .HasForeignKey(d => d.CountryRegionCode);

            modelBuilder.Entity<StateProvince>()
                .HasRequired(d => d.Territory)
                .WithMany(p => p.StateProvince)
                .HasForeignKey(d => d.TerritoryID);

            modelBuilder.Entity<Store>().HasKey(e => e.BusinessEntityID);

            modelBuilder.Entity<Store>().ToTable("Store", "Sales");

            modelBuilder.Entity<Store>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<Store>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Store>().Property(e => e.rowguid).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Store>()
                .HasRequired(d => d.BusinessEntity)
                .WithOptional(p => p.Store);

            modelBuilder.Entity<Store>()
                .HasRequired(d => d.SalesPerson)
                .WithMany(p => p.Store)
                .HasForeignKey(d => d.SalesPersonID);

            modelBuilder.Entity<TransactionHistory>().HasKey(e => e.TransactionID);

            modelBuilder.Entity<TransactionHistory>().ToTable("TransactionHistory", "Production");

            modelBuilder.Entity<TransactionHistory>().Property(e => e.TransactionType).IsRequired();

            modelBuilder.Entity<TransactionHistory>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<TransactionHistory>().Property(e => e.TransactionDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<TransactionHistory>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.TransactionHistory)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<TransactionHistoryArchive>().HasKey(e => e.TransactionID);

            modelBuilder.Entity<TransactionHistoryArchive>().ToTable("TransactionHistoryArchive", "Production");

            modelBuilder.Entity<TransactionHistoryArchive>().Property(e => e.TransactionType).IsRequired();

            modelBuilder.Entity<TransactionHistoryArchive>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<TransactionHistoryArchive>().Property(e => e.TransactionDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<UnitMeasure>().HasKey(e => e.UnitMeasureCode);

            modelBuilder.Entity<UnitMeasure>().ToTable("UnitMeasure", "Production");

            modelBuilder.Entity<UnitMeasure>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<UnitMeasure>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Vendor>().HasKey(e => e.BusinessEntityID);

            modelBuilder.Entity<Vendor>().ToTable("Vendor", "Purchasing");

            modelBuilder.Entity<Vendor>().Property(e => e.AccountNumber).IsRequired();

            modelBuilder.Entity<Vendor>().Property(e => e.Name).IsRequired();

            modelBuilder.Entity<Vendor>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Vendor>()
                .HasRequired(d => d.BusinessEntity)
                .WithOptional(p => p.Vendor);

            modelBuilder.Entity<WorkOrder>().ToTable("WorkOrder", "Production");

            modelBuilder.Entity<WorkOrder>().Property(e => e.StockedQty).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<WorkOrder>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<WorkOrder>()
                .HasRequired(d => d.Product)
                .WithMany(p => p.WorkOrder)
                .HasForeignKey(d => d.ProductID);

            modelBuilder.Entity<WorkOrder>()
                .HasOptional(d => d.ScrapReason)
                .WithMany(p => p.WorkOrder)
                .HasForeignKey(d => d.ScrapReasonID);

            modelBuilder.Entity<WorkOrderRouting>().HasKey(e => new { e.WorkOrderID, e.ProductID, e.OperationSequence });

            modelBuilder.Entity<WorkOrderRouting>().ToTable("WorkOrderRouting", "Production");

            modelBuilder.Entity<WorkOrderRouting>().Property(e => e.ActualResourceHrs).HasPrecision(9, 4);

            modelBuilder.Entity<WorkOrderRouting>().Property(e => e.ModifiedDate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<WorkOrderRouting>()
                .HasRequired(d => d.Location)
                .WithMany(p => p.WorkOrderRouting)
                .HasForeignKey(d => d.LocationID);

            modelBuilder.Entity<WorkOrderRouting>()
                .HasRequired(d => d.WorkOrder)
                .WithMany(p => p.WorkOrderRouting)
                .HasForeignKey(d => d.WorkOrderID);
        }
    }
}
