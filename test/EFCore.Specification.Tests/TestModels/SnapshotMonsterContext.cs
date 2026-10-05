// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Microsoft.EntityFrameworkCore.TestModels;

public class SnapshotMonsterContext(DbContextOptions options) : MonsterContext<
    SnapshotMonsterContext.Customer, SnapshotMonsterContext.Barcode, SnapshotMonsterContext.IncorrectScan,
    SnapshotMonsterContext.BarcodeDetail, SnapshotMonsterContext.Complaint, SnapshotMonsterContext.Resolution,
    SnapshotMonsterContext.Login, SnapshotMonsterContext.SuspiciousActivity, SnapshotMonsterContext.SmartCard,
    SnapshotMonsterContext.RsaToken, SnapshotMonsterContext.PasswordReset, SnapshotMonsterContext.PageView,
    SnapshotMonsterContext.LastLogin, SnapshotMonsterContext.Message, SnapshotMonsterContext.AnOrder,
    SnapshotMonsterContext.OrderNote, SnapshotMonsterContext.OrderQualityCheck, SnapshotMonsterContext.OrderLine,
    SnapshotMonsterContext.Product, SnapshotMonsterContext.ProductDetail, SnapshotMonsterContext.ProductReview,
    SnapshotMonsterContext.ProductPhoto, SnapshotMonsterContext.ProductWebFeature, SnapshotMonsterContext.Supplier,
    SnapshotMonsterContext.SupplierLogo, SnapshotMonsterContext.SupplierInfo, SnapshotMonsterContext.CustomerInfo,
    SnapshotMonsterContext.Computer, SnapshotMonsterContext.ComputerDetail, SnapshotMonsterContext.Driver,
    SnapshotMonsterContext.License, SnapshotMonsterContext.ConcurrencyInfo, SnapshotMonsterContext.AuditInfo,
    SnapshotMonsterContext.ContactDetails, SnapshotMonsterContext.Dimensions, SnapshotMonsterContext.Phone,
    SnapshotMonsterContext.BackOrderLine, SnapshotMonsterContext.DiscontinuedProduct, SnapshotMonsterContext.ProductPageView>(options)
{
    public class BackOrderLine2 : BackOrderLine;

    public class BackOrderLine : OrderLine, IBackOrderLine
    {
        public DateTime ETA { get; set; } = DateTime.Now;

        public int SupplierId { get; set; }
        public virtual ISupplier Supplier { get; set; } = null!;
    }

    public class BarcodeDetail : IBarcodeDetail
    {
        public byte[] Code { get; set; } = null!;
        public string RegisteredTo { get; set; } = null!;
    }

    public class Barcode : IBarcode
    {
        public void InitializeCollections()
            => BadScans ??= new HashSet<IIncorrectScan>();

        public byte[] Code { get; set; } = null!;
        public int ProductId { get; set; }
        public string Text { get; set; } = null!;

        public virtual IProduct Product { get; set; } = null!;
        public virtual ICollection<IIncorrectScan> BadScans { get; set; } = null!;
        public virtual IBarcodeDetail Detail { get; set; } = null!;
    }

    public class Complaint : IComplaint
    {
        public int ComplaintId { get; set; }
        public int AlternateId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime Logged { get; set; }
        public string Details { get; set; } = null!;

        public virtual ICustomer Customer { get; set; } = null!;
        public virtual IResolution Resolution { get; set; } = null!;
    }

    public class ComputerDetail : IComputerDetail
    {
        public int ComputerDetailId { get; set; }
        public string Manufacturer { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Serial { get; set; } = null!;
        public string Specifications { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }

        public IDimensions Dimensions { get; set; } = new Dimensions();

        public virtual IComputer Computer { get; set; } = null!;
    }

    public class Computer : IComputer
    {
        public int ComputerId { get; set; }
        public string Name { get; set; } = null!;

        public virtual IComputerDetail ComputerDetail { get; set; } = null!;
    }

    public class ConcurrencyInfo : IConcurrencyInfo
    {
        public bool Active { get; set; }
        public string? Token { get; set; }
        public DateTime? QueriedDateTime { get; set; }
    }

    public class ContactDetails : IContactDetails
    {
        public bool Active { get; set; }
        public string? Email { get; set; }

        public IPhone HomePhone { get; set; } = new Phone();
        public IPhone WorkPhone { get; set; } = new Phone();
        public IPhone MobilePhone { get; set; } = new Phone();
    }

    public class CustomerInfo : ICustomerInfo
    {
        public int CustomerInfoId { get; set; }
        public string Information { get; set; } = null!;
    }

    public class Dimensions : IDimensions
    {
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Depth { get; set; }
    }

    public class DiscontinuedProduct : Product, IDiscontinuedProduct
    {
        public DateTime Discontinued { get; set; }
        public int? ReplacementProductId { get; set; }

        public virtual IProduct ReplacedBy { get; set; } = null!;
    }

    public class Driver : IDriver
    {
        public string Name { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        public virtual ILicense License { get; set; } = null!;
    }

    public class IncorrectScan : IIncorrectScan
    {
        public int IncorrectScanId { get; set; }
        public byte[]? ExpectedCode { get; set; }
        public byte[]? ActualCode { get; set; }
        public DateTime ScanDate { get; set; }
        public string Details { get; set; } = null!;

        public virtual IBarcode? ExpectedBarcode { get; set; }
        public virtual IBarcode? ActualBarcode { get; set; }
    }

    public class LastLogin : ILastLogin
    {
        public string Username { get; set; } = null!;
        public DateTime LoggedIn { get; set; }
        public DateTime? LoggedOut { get; set; }

        public string? SmartcardUsername { get; set; }

        public virtual ILogin Login { get; set; } = null!;
    }

    public class License : ILicense
    {
        public string Name { get; set; } = null!;
        public string LicenseNumber { get; set; } = null!;
        public string LicenseClass { get; set; } = "C";
        public string Restrictions { get; set; } = null!;
        public DateTime ExpirationDate { get; set; }
        public LicenseState? State { get; set; }

        public virtual IDriver Driver { get; set; } = null!;
    }

    public class Message : IMessage
    {
        public int MessageId { get; set; }
        public string FromUsername { get; set; } = null!;
        public string? ToUsername { get; set; }
        public DateTime Sent { get; set; }
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsRead { get; set; }

        public virtual ILogin Sender { get; set; } = null!;
        public virtual ILogin? Recipient { get; set; }
    }

    public class OrderLine : IOrderLine
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? ConcurrencyToken { get; set; }

        public virtual IAnOrder Order { get; set; } = null!;
        public virtual IProduct Product { get; set; } = null!;
    }

    public class AnOrder : IAnOrder
    {
        public void InitializeCollections()
        {
            OrderLines ??= new HashSet<IOrderLine>();
            Notes ??= new HashSet<IOrderNote>();
        }

        public int AnOrderId { get; set; }
        public int AlternateId { get; set; }
        public int? CustomerId { get; set; }

        public IConcurrencyInfo Concurrency { get; set; } = new ConcurrencyInfo();

        public virtual ICustomer Customer { get; set; } = null!;
        public virtual ICollection<IOrderLine> OrderLines { get; set; } = null!;
        public virtual ICollection<IOrderNote> Notes { get; set; } = null!;

        public string Username { get; set; } = null!;
        public virtual ILogin Login { get; set; } = null!;
    }

    public class OrderNote : IOrderNote
    {
        public int NoteId { get; set; }
        public string Note { get; set; } = null!;

        public int OrderId { get; set; }
        public virtual IAnOrder Order { get; set; } = null!;
    }

    public class OrderQualityCheck : IOrderQualityCheck
    {
        public int OrderId { get; set; }
        public string CheckedBy { get; set; } = null!;
        public DateTime CheckedDateTime { get; set; }

        public virtual IAnOrder Order { get; set; } = null!;
    }

    public class PageView : IPageView
    {
        public int PageViewId { get; set; }
        public string Username { get; set; } = null!;
        public DateTime Viewed { get; set; }
        public string PageUrl { get; set; } = null!;

        public virtual ILogin Login { get; set; } = null!;
    }

    public class PasswordReset : IPasswordReset
    {
        public int ResetNo { get; set; }
        public string Username { get; set; } = null!;
        public string TempPassword { get; set; } = null!;
        public string EmailedTo { get; set; } = null!;

        public virtual ILogin Login { get; set; } = null!;
    }

    public class ProductDetail : IProductDetail
    {
        public int ProductId { get; set; }
        public string Details { get; set; } = null!;

        public virtual IProduct Product { get; set; } = null!;
    }

    public class Product : IProduct
    {
        public void InitializeCollections()
        {
            Suppliers ??= new HashSet<ISupplier>();
            Replaces ??= new HashSet<IDiscontinuedProduct>();
            Reviews ??= new HashSet<IProductReview>();
            Photos ??= new HashSet<IProductPhoto>();
            Barcodes ??= new HashSet<IBarcode>();
        }

        public int ProductId { get; set; }
        public string Description { get; set; } = null!;
        public string BaseConcurrency { get; set; } = null!;

        public IDimensions Dimensions { get; set; } = null!;
        public IConcurrencyInfo ComplexConcurrency { get; set; } = new ConcurrencyInfo();
        public IAuditInfo NestedComplexConcurrency { get; set; } = new AuditInfo();

        public virtual ICollection<ISupplier> Suppliers { get; set; } = null!;
        public virtual ICollection<IDiscontinuedProduct> Replaces { get; set; } = null!;
        public virtual IProductDetail Detail { get; set; } = null!;
        public virtual ICollection<IProductReview> Reviews { get; set; } = null!;
        public virtual ICollection<IProductPhoto> Photos { get; set; } = null!;
        public virtual ICollection<IBarcode> Barcodes { get; set; } = null!;
    }

    public class ProductPageView : PageView, IProductPageView
    {
        public int ProductId { get; set; }

        public virtual IProduct Product { get; set; } = null!;
    }

    public class ProductPhoto : IProductPhoto
    {
        public void InitializeCollections()
            => Features ??= new HashSet<IProductWebFeature>();

        public int ProductId { get; set; }
        public int PhotoId { get; set; }
        public byte[] Photo { get; set; } = null!;

        public virtual ICollection<IProductWebFeature> Features { get; set; } = null!;
    }

    public class ProductReview : IProductReview
    {
        public void InitializeCollections()
            => Features ??= new HashSet<IProductWebFeature>();

        public int ProductId { get; set; }
        public int ReviewId { get; set; }
        public string Review { get; set; } = null!;

        public virtual IProduct Product { get; set; } = null!;
        public virtual ICollection<IProductWebFeature> Features { get; set; } = null!;
    }

    public class ProductWebFeature : IProductWebFeature
    {
        public int FeatureId { get; set; }
        public int? ProductId { get; set; }
        public int? PhotoId { get; set; }
        public int ReviewId { get; set; }
        public string Heading { get; set; } = null!;

        public virtual IProductReview Review { get; set; } = null!;
        public virtual IProductPhoto Photo { get; set; } = null!;
    }

    public class Resolution : IResolution
    {
        public int ResolutionId { get; set; }
        public string Details { get; set; } = null!;

        public virtual IComplaint Complaint { get; set; } = null!;
    }

    public class RsaToken : IRsaToken
    {
        public string Serial { get; set; } = null!;
        public DateTime Issued { get; set; }

        public string Username { get; set; } = null!;
        public virtual ILogin Login { get; set; } = null!;
    }

    public class SmartCard : ISmartCard
    {
        public string Username { get; set; } = null!;
        public string CardSerial { get; set; } = null!;
        public DateTime Issued { get; set; }

        public virtual ILogin Login { get; set; } = null!;
        public virtual ILastLogin? LastLogin { get; set; }
    }

    public class SupplierInfo : ISupplierInfo
    {
        public int SupplierInfoId { get; set; }
        public string Information { get; set; } = null!;

        public int SupplierId { get; set; }
        public virtual ISupplier Supplier { get; set; } = null!;
    }

    public class SupplierLogo : ISupplierLogo
    {
        public int SupplierId { get; set; }
        public byte[] Logo { get; set; } = null!;
    }

    public class Supplier : ISupplier
    {
        public void InitializeCollections()
        {
            Products ??= new HashSet<IProduct>();
            BackOrderLines = new HashSet<IBackOrderLine>();
        }

        public int SupplierId { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<IProduct> Products { get; set; } = null!;
        public virtual ICollection<IBackOrderLine> BackOrderLines { get; set; } = null!;
        public virtual ISupplierLogo Logo { get; set; } = null!;
    }

    public class SuspiciousActivity : ISuspiciousActivity
    {
        public int SuspiciousActivityId { get; set; }
        public string Activity { get; set; } = null!;

        public string Username { get; set; } = null!;
    }

    public class AuditInfo : IAuditInfo
    {
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }

        public IConcurrencyInfo Concurrency { get; set; } = new ConcurrencyInfo();
    }

    public class Customer : ICustomer
    {
        public void InitializeCollections()
        {
            Orders ??= new HashSet<IAnOrder>();
            Logins ??= new HashSet<ILogin>();
        }

        public int CustomerId { get; set; }
        public int? HusbandId { get; set; }
        public string Name { get; set; } = null!;

        public IContactDetails ContactInfo { get; set; } = new ContactDetails();
        public IAuditInfo Auditing { get; set; } = new AuditInfo();

        public virtual ICollection<IAnOrder> Orders { get; set; } = null!;
        public virtual ICollection<ILogin> Logins { get; set; } = null!;
        public virtual ICustomer Husband { get; set; } = null!;
        public virtual ICustomer Wife { get; set; } = null!;
        public virtual ICustomerInfo Info { get; set; } = null!;
    }

    public class Login : ILogin
    {
        public void InitializeCollections()
        {
            SentMessages ??= new HashSet<IMessage>();
            ReceivedMessages ??= new HashSet<IMessage>();
            Orders ??= new HashSet<IAnOrder>();
        }

        public string Username { get; set; } = null!;
        public string AlternateUsername { get; set; } = null!;
        public int CustomerId { get; set; }

        public virtual ICustomer Customer { get; set; } = null!;
        public virtual ILastLogin LastLogin { get; set; } = null!;
        public virtual ICollection<IMessage> SentMessages { get; set; } = null!;
        public virtual ICollection<IMessage> ReceivedMessages { get; set; } = null!;
        public virtual ICollection<IAnOrder> Orders { get; set; } = null!;
    }

    public class Phone : IPhone
    {
        public string? PhoneNumber { get; set; }
        public string Extension { get; set; } = "None";
        public PhoneType PhoneType { get; set; }
    }
}
