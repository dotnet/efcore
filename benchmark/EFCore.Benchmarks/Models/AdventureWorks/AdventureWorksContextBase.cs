// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public abstract class AdventureWorksContextBase : DbContext
{
    public AdventureWorksContextBase()
    {
    }

    public AdventureWorksContextBase(DbContextOptions options)
        : base(options)
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => ConfigureProvider(optionsBuilder);

    protected abstract void ConfigureProvider(DbContextOptionsBuilder optionsBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => ConfigureModel(modelBuilder);

    public static void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(
            entity =>
            {
                entity.ToTable("Address", "Person");

                entity.Property(e => e.AddressLine1).IsRequired();

                entity.Property(e => e.City).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.PostalCode).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.StateProvince)
                    .WithMany(p => p.Address)
                    .HasForeignKey(d => d.StateProvinceID);
            });

        modelBuilder.Entity<AddressType>(
            entity =>
            {
                entity.ToTable("AddressType", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<BillOfMaterials>(
            entity =>
            {
                entity.ToTable("BillOfMaterials", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.PerAssemblyQty)
                    .HasColumnType("decimal(8, 2)")
                    .HasDefaultValue(1.00m);

                entity.Property(e => e.StartDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.UnitMeasureCode).IsRequired();

                entity.HasOne(d => d.Component)
                    .WithMany(p => p.BillOfMaterials)
                    .HasForeignKey(d => d.ComponentID);

                entity.HasOne(d => d.ProductAssembly)
                    .WithMany(p => p.BillOfMaterialsNavigation)
                    .HasForeignKey(d => d.ProductAssemblyID);

                entity.HasOne(d => d.UnitMeasureCodeNavigation)
                    .WithMany(p => p.BillOfMaterials)
                    .HasForeignKey(d => d.UnitMeasureCode);
            });

        modelBuilder.Entity<BusinessEntity>(
            entity =>
            {
                entity.ToTable("BusinessEntity", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<BusinessEntityAddress>(
            entity =>
            {
                entity.HasKey(
                    e => new
                    {
                        e.BusinessEntityID,
                        e.AddressID,
                        e.AddressTypeID
                    });

                entity.ToTable("BusinessEntityAddress", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.BusinessEntityAddress)
                    .HasForeignKey(d => d.AddressID);

                entity.HasOne(d => d.AddressType)
                    .WithMany(p => p.BusinessEntityAddress)
                    .HasForeignKey(d => d.AddressTypeID);

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.BusinessEntityAddress)
                    .HasForeignKey(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<BusinessEntityContact>(
            entity =>
            {
                entity.HasKey(
                    e => new
                    {
                        e.BusinessEntityID,
                        e.PersonID,
                        e.ContactTypeID
                    });

                entity.ToTable("BusinessEntityContact", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.BusinessEntityContact)
                    .HasForeignKey(d => d.BusinessEntityID);

                entity.HasOne(d => d.ContactType)
                    .WithMany(p => p.BusinessEntityContact)
                    .HasForeignKey(d => d.ContactTypeID);

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.BusinessEntityContact)
                    .HasForeignKey(d => d.PersonID);
            });

        modelBuilder.Entity<ContactType>(
            entity =>
            {
                entity.ToTable("ContactType", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<CountryRegion>(
            entity =>
            {
                entity.HasKey(e => e.CountryRegionCode);

                entity.ToTable("CountryRegion", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<CountryRegionCurrency>(
            entity =>
            {
                entity.HasKey(e => new { e.CountryRegionCode, e.CurrencyCode });

                entity.ToTable("CountryRegionCurrency", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.CountryRegionCodeNavigation)
                    .WithMany(p => p.CountryRegionCurrency)
                    .HasForeignKey(d => d.CountryRegionCode);

                entity.HasOne(d => d.CurrencyCodeNavigation)
                    .WithMany(p => p.CountryRegionCurrency)
                    .HasForeignKey(d => d.CurrencyCode);
            });

        modelBuilder.Entity<CreditCard>(
            entity =>
            {
                entity.ToTable("CreditCard", "Sales");

                entity.Property(e => e.CardNumber).IsRequired();

                entity.Property(e => e.CardType).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
            });

        modelBuilder.Entity<Culture>(
            entity =>
            {
                entity.ToTable("Culture", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<Currency>(
            entity =>
            {
                entity.HasKey(e => e.CurrencyCode);

                entity.ToTable("Currency", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<CurrencyRate>(
            entity =>
            {
                entity.ToTable("CurrencyRate", "Sales");

                entity.Property(e => e.FromCurrencyCode).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ToCurrencyCode).IsRequired();

                entity.HasOne(d => d.FromCurrencyCodeNavigation)
                    .WithMany(p => p.CurrencyRate)
                    .HasForeignKey(d => d.FromCurrencyCode);

                entity.HasOne(d => d.ToCurrencyCodeNavigation)
                    .WithMany(p => p.CurrencyRateNavigation)
                    .HasForeignKey(d => d.ToCurrencyCode);
            });

        modelBuilder.Entity<Customer>(
            entity =>
            {
                entity.ToTable("Customer", "Sales");

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Customer)
                    .HasForeignKey(d => d.PersonID);

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Customer)
                    .HasForeignKey(d => d.StoreID);

                entity.HasOne(d => d.Territory)
                    .WithMany(p => p.Customer)
                    .HasForeignKey(d => d.TerritoryID);
            });

        modelBuilder.Entity<Department>(
            entity =>
            {
                entity.ToTable("Department", "HumanResources");

                entity.Property(e => e.GroupName).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<EmailAddress>(
            entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityID, e.EmailAddressID });

                entity.ToTable("EmailAddress", "Person");

                entity.Property(e => e.EmailAddress1).HasColumnName("EmailAddress");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.EmailAddress)
                    .HasForeignKey(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<Employee>(
            entity =>
            {
                entity.HasKey(e => e.BusinessEntityID);

                entity.ToTable("Employee", "HumanResources");

                entity.Property(e => e.CurrentFlag).HasDefaultValue(true);

                entity.Property(e => e.Gender).IsRequired();

                entity.Property(e => e.JobTitle).IsRequired();

                entity.Property(e => e.LoginID).IsRequired();

                entity.Property(e => e.MaritalStatus).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.NationalIDNumber).IsRequired();

                entity.Property(e => e.OrganizationLevel).ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.SalariedFlag).HasDefaultValue(true);

                entity.Property(e => e.SickLeaveHours).HasDefaultValue((short)0);

                entity.Property(e => e.VacationHours).HasDefaultValue((short)0);

                entity.HasOne(d => d.BusinessEntity)
                    .WithOne(p => p.Employee)
                    .HasForeignKey<Employee>(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<EmployeeDepartmentHistory>(
            entity =>
            {
                entity.HasKey(
                    e => new
                    {
                        e.BusinessEntityID,
                        e.StartDate,
                        e.DepartmentID,
                        e.ShiftID
                    });

                entity.ToTable("EmployeeDepartmentHistory", "HumanResources");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.EmployeeDepartmentHistory)
                    .HasForeignKey(d => d.BusinessEntityID);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.EmployeeDepartmentHistory)
                    .HasForeignKey(d => d.DepartmentID);

                entity.HasOne(d => d.Shift)
                    .WithMany(p => p.EmployeeDepartmentHistory)
                    .HasForeignKey(d => d.ShiftID);
            });

        modelBuilder.Entity<EmployeePayHistory>(
            entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityID, e.RateChangeDate });

                entity.ToTable("EmployeePayHistory", "HumanResources");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.EmployeePayHistory)
                    .HasForeignKey(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<Illustration>(
            entity =>
            {
                entity.ToTable("Illustration", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
            });

        modelBuilder.Entity<JobCandidate>(
            entity =>
            {
                entity.ToTable("JobCandidate", "HumanResources");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.JobCandidate)
                    .HasForeignKey(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<Location>(
            entity =>
            {
                entity.ToTable("Location", "Production");

                entity.Property(e => e.Availability)
                    .HasColumnType("decimal(8, 2)")
                    .HasDefaultValue(0.00m);

                entity.Property(e => e.CostRate).HasDefaultValue(0.00m);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<Password>(
            entity =>
            {
                entity.HasKey(e => e.BusinessEntityID);

                entity.ToTable("Password", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.PasswordHash).IsRequired();

                entity.Property(e => e.PasswordSalt).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.BusinessEntity).WithOne(p => p.Password).HasForeignKey<Password>(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<Person>(
            entity =>
            {
                entity.HasKey(e => e.BusinessEntityID);

                entity.ToTable("Person", "Person");

                entity.Property(e => e.EmailPromotion).HasDefaultValue(0);

                entity.Property(e => e.FirstName).IsRequired();

                entity.Property(e => e.LastName).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.NameStyle).HasDefaultValue(false);

                entity.Property(e => e.PersonType).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithOne(p => p.Person)
                    .HasForeignKey<Person>(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<PersonCreditCard>(
            entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityID, e.CreditCardID });

                entity.ToTable("PersonCreditCard", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.PersonCreditCard)
                    .HasForeignKey(d => d.BusinessEntityID);

                entity.HasOne(d => d.CreditCard)
                    .WithMany(p => p.PersonCreditCard)
                    .HasForeignKey(d => d.CreditCardID);
            });

        modelBuilder.Entity<PersonPhone>(
            entity =>
            {
                entity.HasKey(
                    e => new
                    {
                        e.BusinessEntityID,
                        e.PhoneNumber,
                        e.PhoneNumberTypeID
                    });

                entity.ToTable("PersonPhone", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.PersonPhone)
                    .HasForeignKey(d => d.BusinessEntityID);

                entity.HasOne(d => d.PhoneNumberType)
                    .WithMany(p => p.PersonPhone)
                    .HasForeignKey(d => d.PhoneNumberTypeID);
            });

        modelBuilder.Entity<PhoneNumberType>(
            entity =>
            {
                entity.ToTable("PhoneNumberType", "Person");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<Product>(
            entity =>
            {
                entity.ToTable("Product", "Production");

                entity.Property(e => e.FinishedGoodsFlag).HasDefaultValue(true);

                entity.Property(e => e.MakeFlag).HasDefaultValue(true);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.ProductNumber).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.Weight).HasColumnType("decimal(8, 2)");

                entity.HasOne(d => d.ProductModel)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.ProductModelID);

                entity.HasOne(d => d.ProductSubcategory)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.ProductSubcategoryID);

                entity.HasOne(d => d.SizeUnitMeasureCodeNavigation)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.SizeUnitMeasureCode);

                entity.HasOne(d => d.WeightUnitMeasureCodeNavigation)
                    .WithMany(p => p.ProductNavigation)
                    .HasForeignKey(d => d.WeightUnitMeasureCode);
            });

        modelBuilder.Entity<ProductCategory>(
            entity =>
            {
                entity.ToTable("ProductCategory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<ProductCostHistory>(
            entity =>
            {
                entity.HasKey(e => new { e.ProductID, e.StartDate });

                entity.ToTable("ProductCostHistory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductCostHistory)
                    .HasForeignKey(d => d.ProductID);
            });

        modelBuilder.Entity<ProductDescription>(
            entity =>
            {
                entity.ToTable("ProductDescription", "Production");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<ProductDocument>(
            entity =>
            {
                entity.HasKey(e => new { e.ProductID, e.DocumentNode });

                entity.ToTable("ProductDocument", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductDocument)
                    .HasForeignKey(d => d.ProductID);
            });

        modelBuilder.Entity<ProductInventory>(
            entity =>
            {
                entity.HasKey(e => new { e.ProductID, e.LocationID });

                entity.ToTable("ProductInventory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Quantity).HasDefaultValue((short)0);

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.Shelf).IsRequired();

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.ProductInventory)
                    .HasForeignKey(d => d.LocationID);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductInventory)
                    .HasForeignKey(d => d.ProductID);
            });

        modelBuilder.Entity<ProductListPriceHistory>(
            entity =>
            {
                entity.HasKey(e => new { e.ProductID, e.StartDate });

                entity.ToTable("ProductListPriceHistory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductListPriceHistory)
                    .HasForeignKey(d => d.ProductID);
            });

        modelBuilder.Entity<ProductModel>(
            entity =>
            {
                entity.ToTable("ProductModel", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<ProductModelIllustration>(
            entity =>
            {
                entity.HasKey(e => new { e.ProductModelID, e.IllustrationID });

                entity.ToTable("ProductModelIllustration", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.Illustration)
                    .WithMany(p => p.ProductModelIllustration)
                    .HasForeignKey(d => d.IllustrationID);

                entity.HasOne(d => d.ProductModel)
                    .WithMany(p => p.ProductModelIllustration)
                    .HasForeignKey(d => d.ProductModelID);
            });

        modelBuilder.Entity<ProductModelProductDescriptionCulture>(
            entity =>
            {
                entity.HasKey(
                    e => new
                    {
                        e.ProductModelID,
                        e.ProductDescriptionID,
                        e.CultureID
                    });

                entity.ToTable("ProductModelProductDescriptionCulture", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.Culture)
                    .WithMany(p => p.ProductModelProductDescriptionCulture)
                    .HasForeignKey(d => d.CultureID);

                entity.HasOne(d => d.ProductDescription)
                    .WithMany(p => p.ProductModelProductDescriptionCulture)
                    .HasForeignKey(d => d.ProductDescriptionID);

                entity.HasOne(d => d.ProductModel)
                    .WithMany(p => p.ProductModelProductDescriptionCulture)
                    .HasForeignKey(d => d.ProductModelID);
            });

        modelBuilder.Entity<ProductPhoto>(
            entity =>
            {
                entity.ToTable("ProductPhoto", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");
            });

        modelBuilder.Entity<ProductProductPhoto>(
            entity =>
            {
                entity.HasKey(e => new { e.ProductID, e.ProductPhotoID });

                entity.ToTable("ProductProductPhoto", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Primary).HasDefaultValue(false);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductProductPhoto)
                    .HasForeignKey(d => d.ProductID);

                entity.HasOne(d => d.ProductPhoto)
                    .WithMany(p => p.ProductProductPhoto)
                    .HasForeignKey(d => d.ProductPhotoID);
            });

        modelBuilder.Entity<ProductReview>(
            entity =>
            {
                entity.ToTable("ProductReview", "Production");

                entity.Property(e => e.EmailAddress).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ReviewDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ReviewerName).IsRequired();

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.ProductID);
            });

        modelBuilder.Entity<ProductSubcategory>(
            entity =>
            {
                entity.ToTable("ProductSubcategory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.ProductSubcategory)
                    .HasForeignKey(d => d.ProductCategoryID);
            });

        modelBuilder.Entity<ProductVendor>(
            entity =>
            {
                entity.HasKey(e => new { e.ProductID, e.BusinessEntityID });

                entity.ToTable("ProductVendor", "Purchasing");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.UnitMeasureCode).IsRequired();

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.ProductVendor)
                    .HasForeignKey(d => d.BusinessEntityID);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductVendor)
                    .HasForeignKey(d => d.ProductID);

                entity.HasOne(d => d.UnitMeasureCodeNavigation)
                    .WithMany(p => p.ProductVendor)
                    .HasForeignKey(d => d.UnitMeasureCode);
            });

        modelBuilder.Entity<PurchaseOrderDetail>(
            entity =>
            {
                entity.HasKey(e => new { e.PurchaseOrderID, e.PurchaseOrderDetailID });

                entity.ToTable("PurchaseOrderDetail", "Purchasing");

                entity.Property(e => e.LineTotal).ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ReceivedQty).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.RejectedQty).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.StockedQty)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("decimal(9, 2)");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.PurchaseOrderDetail)
                    .HasForeignKey(d => d.ProductID);

                entity.HasOne(d => d.PurchaseOrder)
                    .WithMany(p => p.PurchaseOrderDetail)
                    .HasForeignKey(d => d.PurchaseOrderID);
            });

        modelBuilder.Entity<PurchaseOrderHeader>(
            entity =>
            {
                entity.HasKey(e => e.PurchaseOrderID);

                entity.ToTable("PurchaseOrderHeader", "Purchasing");

                entity.Property(e => e.Freight).HasDefaultValue(0.00m);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.OrderDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.RevisionNumber).HasDefaultValue((byte)0);

                entity.Property(e => e.Status).HasDefaultValue((byte)1);

                entity.Property(e => e.SubTotal).HasDefaultValue(0.00m);

                entity.Property(e => e.TaxAmt).HasDefaultValue(0.00m);

                entity.Property(e => e.TotalDue).ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.PurchaseOrderHeader)
                    .HasForeignKey(d => d.EmployeeID);

                entity.HasOne(d => d.ShipMethod)
                    .WithMany(p => p.PurchaseOrderHeader)
                    .HasForeignKey(d => d.ShipMethodID);

                entity.HasOne(d => d.Vendor)
                    .WithMany(p => p.PurchaseOrderHeader)
                    .HasForeignKey(d => d.VendorID);
            });

        modelBuilder.Entity<SalesOrderDetail>(
            entity =>
            {
                entity.HasKey(e => new { e.SalesOrderID, e.SalesOrderDetailID });

                entity.ToTable("SalesOrderDetail", "Sales");

                entity.Property(e => e.LineTotal)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("numeric(38, 6)");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.UnitPriceDiscount).HasDefaultValue(0.0m);

                entity.HasOne(d => d.SalesOrder)
                    .WithMany(p => p.SalesOrderDetail)
                    .HasForeignKey(d => d.SalesOrderID);

                entity.HasOne(d => d.SpecialOfferProduct)
                    .WithMany(p => p.SalesOrderDetail)
                    .HasForeignKey(d => new { d.SpecialOfferID, d.ProductID });
            });

        modelBuilder.Entity<SalesOrderHeader>(
            entity =>
            {
                entity.HasKey(e => e.SalesOrderID);

                entity.ToTable("SalesOrderHeader", "Sales");

                entity.Property(e => e.Freight).HasDefaultValue(0.00m);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.OnlineOrderFlag).HasDefaultValue(true);

                entity.Property(e => e.OrderDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.RevisionNumber).HasDefaultValue((byte)0);

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.SalesOrderNumber)
                    .IsRequired()
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Status).HasDefaultValue((byte)1);

                entity.Property(e => e.SubTotal).HasDefaultValue(0.00m);

                entity.Property(e => e.TaxAmt).HasDefaultValue(0.00m);

                entity.Property(e => e.TotalDue).ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.BillToAddress)
                    .WithMany(p => p.SalesOrderHeader)
                    .HasForeignKey(d => d.BillToAddressID);

                entity.HasOne(d => d.CreditCard)
                    .WithMany(p => p.SalesOrderHeader)
                    .HasForeignKey(d => d.CreditCardID);

                entity.HasOne(d => d.CurrencyRate)
                    .WithMany(p => p.SalesOrderHeader)
                    .HasForeignKey(d => d.CurrencyRateID);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.SalesOrderHeader)
                    .HasForeignKey(d => d.CustomerID);

                entity.HasOne(d => d.SalesPerson)
                    .WithMany(p => p.SalesOrderHeader)
                    .HasForeignKey(d => d.SalesPersonID);

                entity.HasOne(d => d.ShipMethod)
                    .WithMany(p => p.SalesOrderHeader)
                    .HasForeignKey(d => d.ShipMethodID);

                entity.HasOne(d => d.ShipToAddress)
                    .WithMany(p => p.SalesOrderHeaderNavigation)
                    .HasForeignKey(d => d.ShipToAddressID);

                entity.HasOne(d => d.Territory)
                    .WithMany(p => p.SalesOrderHeader)
                    .HasForeignKey(d => d.TerritoryID);
            });

        modelBuilder.Entity<SalesOrderHeaderSalesReason>(
            entity =>
            {
                entity.HasKey(e => new { e.SalesOrderID, e.SalesReasonID });

                entity.ToTable("SalesOrderHeaderSalesReason", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.SalesOrder)
                    .WithMany(p => p.SalesOrderHeaderSalesReason)
                    .HasForeignKey(d => d.SalesOrderID);

                entity.HasOne(d => d.SalesReason)
                    .WithMany(p => p.SalesOrderHeaderSalesReason)
                    .HasForeignKey(d => d.SalesReasonID);
            });

        modelBuilder.Entity<SalesPerson>(
            entity =>
            {
                entity.HasKey(e => e.BusinessEntityID);

                entity.ToTable("SalesPerson", "Sales");

                entity.Property(e => e.Bonus).HasDefaultValue(0.00m);

                entity.Property(e => e.CommissionPct).HasDefaultValue(0.00m);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.SalesLastYear).HasDefaultValue(0.00m);

                entity.Property(e => e.SalesYTD).HasDefaultValue(0.00m);

                entity.HasOne(d => d.BusinessEntity)
                    .WithOne(p => p.SalesPerson)
                    .HasForeignKey<SalesPerson>(d => d.BusinessEntityID);

                entity.HasOne(d => d.Territory)
                    .WithMany(p => p.SalesPerson)
                    .HasForeignKey(d => d.TerritoryID);
            });

        modelBuilder.Entity<SalesPersonQuotaHistory>(
            entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityID, e.QuotaDate });

                entity.ToTable("SalesPersonQuotaHistory", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.SalesPersonQuotaHistory)
                    .HasForeignKey(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<SalesReason>(
            entity =>
            {
                entity.ToTable("SalesReason", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.ReasonType).IsRequired();
            });

        modelBuilder.Entity<SalesTaxRate>(
            entity =>
            {
                entity.ToTable("SalesTaxRate", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.TaxRate).HasDefaultValue(0.00m);

                entity.HasOne(d => d.StateProvince)
                    .WithMany(p => p.SalesTaxRate)
                    .HasForeignKey(d => d.StateProvinceID);
            });

        modelBuilder.Entity<SalesTerritory>(
            entity =>
            {
                entity.HasKey(e => e.TerritoryID);

                entity.ToTable("SalesTerritory", "Sales");

                entity.Property(e => e.CostLastYear).HasDefaultValue(0.00m);

                entity.Property(e => e.CostYTD).HasDefaultValue(0.00m);

                entity.Property(e => e.CountryRegionCode).IsRequired();

                entity.Property(e => e.Group).IsRequired();

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.SalesLastYear).HasDefaultValue(0.00m);

                entity.Property(e => e.SalesYTD).HasDefaultValue(0.00m);

                entity.HasOne(d => d.CountryRegionCodeNavigation)
                    .WithMany(p => p.SalesTerritory)
                    .HasForeignKey(d => d.CountryRegionCode);
            });

        modelBuilder.Entity<SalesTerritoryHistory>(
            entity =>
            {
                entity.HasKey(
                    e => new
                    {
                        e.BusinessEntityID,
                        e.StartDate,
                        e.TerritoryID
                    });

                entity.ToTable("SalesTerritoryHistory", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithMany(p => p.SalesTerritoryHistory)
                    .HasForeignKey(d => d.BusinessEntityID);

                entity.HasOne(d => d.Territory)
                    .WithMany(p => p.SalesTerritoryHistory)
                    .HasForeignKey(d => d.TerritoryID);
            });

        modelBuilder.Entity<ScrapReason>(
            entity =>
            {
                entity.ToTable("ScrapReason", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<Shift>(
            entity =>
            {
                entity.ToTable("Shift", "HumanResources");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<ShipMethod>(
            entity =>
            {
                entity.ToTable("ShipMethod", "Purchasing");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.ShipBase).HasDefaultValue(0.00m);

                entity.Property(e => e.ShipRate).HasDefaultValue(0.00m);
            });

        modelBuilder.Entity<ShoppingCartItem>(
            entity =>
            {
                entity.ToTable("ShoppingCartItem", "Sales");

                entity.Property(e => e.DateCreated).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Quantity).HasDefaultValue(1);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ShoppingCartItem)
                    .HasForeignKey(d => d.ProductID);
            });

        modelBuilder.Entity<SpecialOffer>(
            entity =>
            {
                entity.ToTable("SpecialOffer", "Sales");

                entity.Property(e => e.Category).IsRequired();

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.DiscountPct).HasDefaultValue(0.00m);

                entity.Property(e => e.MinQty).HasDefaultValue(0);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.Type).IsRequired();
            });

        modelBuilder.Entity<SpecialOfferProduct>(
            entity =>
            {
                entity.HasKey(e => new { e.SpecialOfferID, e.ProductID });

                entity.ToTable("SpecialOfferProduct", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.SpecialOfferProduct)
                    .HasForeignKey(d => d.ProductID);

                entity.HasOne(d => d.SpecialOffer)
                    .WithMany(p => p.SpecialOfferProduct)
                    .HasForeignKey(d => d.SpecialOfferID);
            });

        modelBuilder.Entity<StateProvince>(
            entity =>
            {
                entity.ToTable("StateProvince", "Person");

                entity.Property(e => e.CountryRegionCode).IsRequired();

                entity.Property(e => e.IsOnlyStateProvinceFlag).HasDefaultValue(true);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.Property(e => e.StateProvinceCode).IsRequired();

                entity.HasOne(d => d.CountryRegionCodeNavigation)
                    .WithMany(p => p.StateProvince)
                    .HasForeignKey(d => d.CountryRegionCode);

                entity.HasOne(d => d.Territory)
                    .WithMany(p => p.StateProvince)
                    .HasForeignKey(d => d.TerritoryID);
            });

        modelBuilder.Entity<Store>(
            entity =>
            {
                entity.HasKey(e => e.BusinessEntityID);

                entity.ToTable("Store", "Sales");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");

                entity.HasOne(d => d.BusinessEntity)
                    .WithOne(p => p.Store)
                    .HasForeignKey<Store>(d => d.BusinessEntityID);

                entity.HasOne(d => d.SalesPerson)
                    .WithMany(p => p.Store)
                    .HasForeignKey(d => d.SalesPersonID);
            });

        modelBuilder.Entity<TransactionHistory>(
            entity =>
            {
                entity.HasKey(e => e.TransactionID);

                entity.ToTable("TransactionHistory", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ReferenceOrderLineID).HasDefaultValue(0);

                entity.Property(e => e.TransactionDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.TransactionType).IsRequired();

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.TransactionHistory)
                    .HasForeignKey(d => d.ProductID);
            });

        modelBuilder.Entity<TransactionHistoryArchive>(
            entity =>
            {
                entity.HasKey(e => e.TransactionID);

                entity.ToTable("TransactionHistoryArchive", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.ReferenceOrderLineID).HasDefaultValue(0);

                entity.Property(e => e.TransactionDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.TransactionType).IsRequired();
            });

        modelBuilder.Entity<UnitMeasure>(
            entity =>
            {
                entity.HasKey(e => e.UnitMeasureCode);

                entity.ToTable("UnitMeasure", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();
            });

        modelBuilder.Entity<Vendor>(
            entity =>
            {
                entity.HasKey(e => e.BusinessEntityID);

                entity.ToTable("Vendor", "Purchasing");

                entity.Property(e => e.AccountNumber).IsRequired();

                entity.Property(e => e.ActiveFlag).HasDefaultValue(true);

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.PreferredVendorStatus).HasDefaultValue(true);

                entity.HasOne(d => d.BusinessEntity)
                    .WithOne(p => p.Vendor)
                    .HasForeignKey<Vendor>(d => d.BusinessEntityID);
            });

        modelBuilder.Entity<WorkOrder>(
            entity =>
            {
                entity.ToTable("WorkOrder", "Production");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.Property(e => e.StockedQty).ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.WorkOrder)
                    .HasForeignKey(d => d.ProductID);

                entity.HasOne(d => d.ScrapReason)
                    .WithMany(p => p.WorkOrder)
                    .HasForeignKey(d => d.ScrapReasonID);
            });

        modelBuilder.Entity<WorkOrderRouting>(
            entity =>
            {
                entity.HasKey(
                    e => new
                    {
                        e.WorkOrderID,
                        e.ProductID,
                        e.OperationSequence
                    });

                entity.ToTable("WorkOrderRouting", "Production");

                entity.Property(e => e.ActualResourceHrs).HasColumnType("decimal(9, 4)");

                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.WorkOrderRouting)
                    .HasForeignKey(d => d.LocationID);

                entity.HasOne(d => d.WorkOrder)
                    .WithMany(p => p.WorkOrderRouting)
                    .HasForeignKey(d => d.WorkOrderID);
            });
    }
}
