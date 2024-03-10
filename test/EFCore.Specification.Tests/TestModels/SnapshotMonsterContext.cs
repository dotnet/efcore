// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Microsoft.EntityFrameworkCore.TestModels;

#nullable disable

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
        public BackOrderLine()
        {
            ETA = DateTime.Now;
        }

        public DateTime ETA { get; set; }

        public int SupplierId { get; set; }
        public virtual ISupplier Supplier { get; set; }
    }

    public class BarcodeDetail : IBarcodeDetail
    {
        public byte[] Code { get; set; }
        public string RegisteredTo { get; set; }
    }

    public class Barcode : IBarcode
    {
        public void InitializeCollections()
            => BadScans ??= new HashSet<IIncorrectScan>();

        public byte[] Code { get; set; }
        public int ProductId { get; set; }
        public string Text { get; set; }

        public virtual IProduct Product { get; set; }
        public virtual ICollection<IIncorrectScan> BadScans { get; set; }
        public virtual IBarcodeDetail Detail { get; set; }
    }

    public class Complaint : IComplaint
    {
        public int ComplaintId { get; set; }
        public int AlternateId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime Logged { get; set; }
        public string Details { get; set; }

        public virtual ICustomer Customer { get; set; }
        public virtual IResolution Resolution { get; set; }
    }

    public class ComputerDetail : IComputerDetail
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

        public IDimensions Dimensions { get; set; }

        public virtual IComputer Computer { get; set; }
    }

    public class Computer : IComputer
    {
        public int ComputerId { get; set; }
        public string Name { get; set; }

        public virtual IComputerDetail ComputerDetail { get; set; }
    }

    public class ConcurrencyInfo : IConcurrencyInfo
    {
        public bool Active { get; set; }
        public string Token { get; set; }
        public DateTime? QueriedDateTime { get; set; }
    }

    public class ContactDetails : IContactDetails
    {
        public ContactDetails()
        {
            HomePhone = new Phone();
            WorkPhone = new Phone();
            MobilePhone = new Phone();
        }

        public bool Active { get; set; }
        public string Email { get; set; }

        public IPhone HomePhone { get; set; }
        public IPhone WorkPhone { get; set; }
        public IPhone MobilePhone { get; set; }
    }

    public class CustomerInfo : ICustomerInfo
    {
        public int CustomerInfoId { get; set; }
        public string Information { get; set; }
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

        public virtual IProduct ReplacedBy { get; set; }
    }

    public class Driver : IDriver
    {
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }

        public virtual ILicense License { get; set; }
    }

    public class IncorrectScan : IIncorrectScan
    {
        public int IncorrectScanId { get; set; }
        public byte[] ExpectedCode { get; set; }
        public byte[] ActualCode { get; set; }
        public DateTime ScanDate { get; set; }
        public string Details { get; set; }

        public virtual IBarcode ExpectedBarcode { get; set; }
        public virtual IBarcode ActualBarcode { get; set; }
    }

    public class LastLogin : ILastLogin
    {
        public string Username { get; set; }
        public DateTime LoggedIn { get; set; }
        public DateTime? LoggedOut { get; set; }

        public string SmartcardUsername { get; set; }

        public virtual ILogin Login { get; set; }
    }

    public class License : ILicense
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

        public virtual IDriver Driver { get; set; }
    }

    public class Message : IMessage
    {
        public int MessageId { get; set; }
        public string FromUsername { get; set; }
        public string ToUsername { get; set; }
        public DateTime Sent { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsRead { get; set; }

        public virtual ILogin Sender { get; set; }
        public virtual ILogin Recipient { get; set; }
    }

    public class OrderLine : IOrderLine
    {
        public OrderLine()
        {
            Quantity = 1;
        }

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ConcurrencyToken { get; set; }

        public virtual IAnOrder Order { get; set; }
        public virtual IProduct Product { get; set; }
    }

    public class AnOrder : IAnOrder
    {
        public AnOrder()
        {
            Concurrency = new ConcurrencyInfo();
        }

        public void InitializeCollections()
        {
            OrderLines ??= new HashSet<IOrderLine>();
            Notes ??= new HashSet<IOrderNote>();
        }

        public int AnOrderId { get; set; }
        public int AlternateId { get; set; }
        public int? CustomerId { get; set; }

        public IConcurrencyInfo Concurrency { get; set; }

        public virtual ICustomer Customer { get; set; }
        public virtual ICollection<IOrderLine> OrderLines { get; set; }
        public virtual ICollection<IOrderNote> Notes { get; set; }

        public string Username { get; set; }
        public virtual ILogin Login { get; set; }
    }

    public class OrderNote : IOrderNote
    {
        public int NoteId { get; set; }
        public string Note { get; set; }

        public int OrderId { get; set; }
        public virtual IAnOrder Order { get; set; }
    }

    public class OrderQualityCheck : IOrderQualityCheck
    {
        public int OrderId { get; set; }
        public string CheckedBy { get; set; }
        public DateTime CheckedDateTime { get; set; }

        public virtual IAnOrder Order { get; set; }
    }

    public class PageView : IPageView
    {
        public int PageViewId { get; set; }
        public string Username { get; set; }
        public DateTime Viewed { get; set; }
        public string PageUrl { get; set; }

        public virtual ILogin Login { get; set; }
    }

    public class PasswordReset : IPasswordReset
    {
        public int ResetNo { get; set; }
        public string Username { get; set; }
        public string TempPassword { get; set; }
        public string EmailedTo { get; set; }

        public virtual ILogin Login { get; set; }
    }

    public class ProductDetail : IProductDetail
    {
        public int ProductId { get; set; }
        public string Details { get; set; }

        public virtual IProduct Product { get; set; }
    }

    public class Product : IProduct
    {
        public Product()
        {
            ComplexConcurrency = new ConcurrencyInfo();
            NestedComplexConcurrency = new AuditInfo();
        }

        public void InitializeCollections()
        {
            Suppliers ??= new HashSet<ISupplier>();
            Replaces ??= new HashSet<IDiscontinuedProduct>();
            Reviews ??= new HashSet<IProductReview>();
            Photos ??= new HashSet<IProductPhoto>();
            Barcodes ??= new HashSet<IBarcode>();
        }

        public int ProductId { get; set; }
        public string Description { get; set; }
        public string BaseConcurrency { get; set; }

        public IDimensions Dimensions { get; set; }
        public IConcurrencyInfo ComplexConcurrency { get; set; }
        public IAuditInfo NestedComplexConcurrency { get; set; }

        public virtual ICollection<ISupplier> Suppliers { get; set; }
        public virtual ICollection<IDiscontinuedProduct> Replaces { get; set; }
        public virtual IProductDetail Detail { get; set; }
        public virtual ICollection<IProductReview> Reviews { get; set; }
        public virtual ICollection<IProductPhoto> Photos { get; set; }
        public virtual ICollection<IBarcode> Barcodes { get; set; }
    }

    public class ProductPageView : PageView, IProductPageView
    {
        public int ProductId { get; set; }

        public virtual IProduct Product { get; set; }
    }

    public class ProductPhoto : IProductPhoto
    {
        public void InitializeCollections()
            => Features ??= new HashSet<IProductWebFeature>();

        public int ProductId { get; set; }
        public int PhotoId { get; set; }
        public byte[] Photo { get; set; }

        public virtual ICollection<IProductWebFeature> Features { get; set; }
    }

    public class ProductReview : IProductReview
    {
        public void InitializeCollections()
            => Features ??= new HashSet<IProductWebFeature>();

        public int ProductId { get; set; }
        public int ReviewId { get; set; }
        public string Review { get; set; }

        public virtual IProduct Product { get; set; }
        public virtual ICollection<IProductWebFeature> Features { get; set; }
    }

    public class ProductWebFeature : IProductWebFeature
    {
        public int FeatureId { get; set; }
        public int? ProductId { get; set; }
        public int? PhotoId { get; set; }
        public int ReviewId { get; set; }
        public string Heading { get; set; }

        public virtual IProductReview Review { get; set; }
        public virtual IProductPhoto Photo { get; set; }
    }

    public class Resolution : IResolution
    {
        public int ResolutionId { get; set; }
        public string Details { get; set; }

        public virtual IComplaint Complaint { get; set; }
    }

    public class RsaToken : IRsaToken
    {
        public string Serial { get; set; }
        public DateTime Issued { get; set; }

        public string Username { get; set; }
        public virtual ILogin Login { get; set; }
    }

    public class SmartCard : ISmartCard
    {
        public string Username { get; set; }
        public string CardSerial { get; set; }
        public DateTime Issued { get; set; }

        public virtual ILogin Login { get; set; }
        public virtual ILastLogin LastLogin { get; set; }
    }

    public class SupplierInfo : ISupplierInfo
    {
        public int SupplierInfoId { get; set; }
        public string Information { get; set; }

        public int SupplierId { get; set; }
        public virtual ISupplier Supplier { get; set; }
    }

    public class SupplierLogo : ISupplierLogo
    {
        public int SupplierId { get; set; }
        public byte[] Logo { get; set; }
    }

    public class Supplier : ISupplier
    {
        public void InitializeCollections()
        {
            Products ??= new HashSet<IProduct>();
            BackOrderLines = new HashSet<IBackOrderLine>();
        }

        public int SupplierId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<IProduct> Products { get; set; }
        public virtual ICollection<IBackOrderLine> BackOrderLines { get; set; }
        public virtual ISupplierLogo Logo { get; set; }
    }

    public class SuspiciousActivity : ISuspiciousActivity
    {
        public int SuspiciousActivityId { get; set; }
        public string Activity { get; set; }

        public string Username { get; set; }
    }

    public class AuditInfo : IAuditInfo
    {
        public AuditInfo()
        {
            Concurrency = new ConcurrencyInfo();
            ModifiedDate = DateTime.Now;
        }

        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        public IConcurrencyInfo Concurrency { get; set; }
    }

    public class Customer : ICustomer
    {
        public Customer()
        {
            ContactInfo = new ContactDetails();
            Auditing = new AuditInfo();
        }

        public void InitializeCollections()
        {
            Orders ??= new HashSet<IAnOrder>();
            Logins ??= new HashSet<ILogin>();
        }

        public int CustomerId { get; set; }
        public int? HusbandId { get; set; }
        public string Name { get; set; }

        public IContactDetails ContactInfo { get; set; }
        public IAuditInfo Auditing { get; set; }

        public virtual ICollection<IAnOrder> Orders { get; set; }
        public virtual ICollection<ILogin> Logins { get; set; }
        public virtual ICustomer Husband { get; set; }
        public virtual ICustomer Wife { get; set; }
        public virtual ICustomerInfo Info { get; set; }
    }

    public class Login : ILogin
    {
        public void InitializeCollections()
        {
            SentMessages ??= new HashSet<IMessage>();
            ReceivedMessages ??= new HashSet<IMessage>();
            Orders ??= new HashSet<IAnOrder>();
        }

        public string Username { get; set; }
        public string AlternateUsername { get; set; }
        public int CustomerId { get; set; }

        public virtual ICustomer Customer { get; set; }
        public virtual ILastLogin LastLogin { get; set; }
        public virtual ICollection<IMessage> SentMessages { get; set; }
        public virtual ICollection<IMessage> ReceivedMessages { get; set; }
        public virtual ICollection<IAnOrder> Orders { get; set; }
    }

    public class Phone : IPhone
    {
        public Phone()
        {
            Extension = "None";
        }

        public string PhoneNumber { get; set; }
        public string Extension { get; set; }
        public PhoneType PhoneType { get; set; }
    }
}
