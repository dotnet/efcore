// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels;

public abstract class MonsterContext : PoolableDbContext
{
    protected MonsterContext(DbContextOptions options)
        : base(options)
    {
    }

    public abstract IQueryable<ICustomer> Customers { get; }
    public abstract IQueryable<IBarcode> Barcodes { get; }
    public abstract IQueryable<IIncorrectScan> IncorrectScans { get; }
    public abstract IQueryable<IBarcodeDetail> BarcodeDetails { get; }
    public abstract IQueryable<IComplaint> Complaints { get; }
    public abstract IQueryable<IResolution> Resolutions { get; }
    public abstract IQueryable<ILogin> Logins { get; }
    public abstract IQueryable<ISuspiciousActivity> SuspiciousActivities { get; }
    public abstract IQueryable<ISmartCard> SmartCards { get; }
    public abstract IQueryable<IRsaToken> RsaTokens { get; }
    public abstract IQueryable<IPasswordReset> PasswordResets { get; }
    public abstract IQueryable<IPageView> PageViews { get; }
    public abstract IQueryable<ILastLogin> LastLogins { get; }
    public abstract IQueryable<IMessage> Messages { get; }
    public abstract IQueryable<IAnOrder> Orders { get; }
    public abstract IQueryable<IOrderNote> OrderNotes { get; }
    public abstract IQueryable<IOrderQualityCheck> OrderQualityChecks { get; }
    public abstract IQueryable<IOrderLine> OrderLines { get; }
    public abstract IQueryable<IProduct> Products { get; }
    public abstract IQueryable<IProductDetail> ProductDetails { get; }
    public abstract IQueryable<IProductReview> ProductReviews { get; }
    public abstract IQueryable<IProductPhoto> ProductPhotos { get; }
    public abstract IQueryable<IProductWebFeature> ProductWebFeatures { get; }
    public abstract IQueryable<ISupplier> Suppliers { get; }
    public abstract IQueryable<ISupplierLogo> SupplierLogos { get; }
    public abstract IQueryable<ISupplierInfo> SupplierInformation { get; }
    public abstract IQueryable<ICustomerInfo> CustomerInformation { get; }
    public abstract IQueryable<IComputer> Computers { get; }
    public abstract IQueryable<IComputerDetail> ComputerDetails { get; }
    public abstract IQueryable<IDriver> Drivers { get; }
    public abstract IQueryable<ILicense> Licenses { get; }

    public abstract Task SeedUsingFKs();
    public abstract Task SeedUsingNavigations(bool dependentNavs, bool principalNavs);
    public abstract Task SeedUsingNavigationsWithDeferredAdd();
}
