// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels;

public interface IBackOrderLine : IOrderLine
{
    public DateTime ETA { get; set; }
    public int SupplierId { get; set; }
    public ISupplier Supplier { get; set; }
}

public interface IBarcodeDetail
{
    public byte[] Code { get; set; }
    public string RegisteredTo { get; set; }
}

public interface IBarcode
{
    public byte[] Code { get; set; }
    public int ProductId { get; set; }
    public string Text { get; set; }
    public IProduct Product { get; set; }
    public ICollection<IIncorrectScan> BadScans { get; set; }
    public IBarcodeDetail Detail { get; set; }
    public void InitializeCollections();
}

public interface IComplaint
{
    public int ComplaintId { get; set; }
    public int AlternateId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime Logged { get; set; }
    public string Details { get; set; }
    public ICustomer Customer { get; set; }
    public IResolution Resolution { get; set; }
}

public interface IComputerDetail
{
    public int ComputerDetailId { get; set; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string Serial { get; set; }
    public string Specifications { get; set; }
    public DateTime PurchaseDate { get; set; }
    public IDimensions Dimensions { get; set; }
    public IComputer Computer { get; set; }
}

public interface IComputer
{
    public int ComputerId { get; set; }
    public string Name { get; set; }
    public IComputerDetail ComputerDetail { get; set; }
}

public interface IConcurrencyInfo
{
    public bool Active { get; set; }
    public string Token { get; set; }
    public DateTime? QueriedDateTime { get; set; }
}

public interface IContactDetails
{
    public bool Active { get; set; }
    public string Email { get; set; }

    public IPhone HomePhone { get; set; }
    public IPhone WorkPhone { get; set; }
    public IPhone MobilePhone { get; set; }
}

public interface ICustomerInfo
{
    public int CustomerInfoId { get; set; }
    public string Information { get; set; }
}

public interface IDimensions
{
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Depth { get; set; }
}

public interface IDiscontinuedProduct : IProduct
{
    public DateTime Discontinued { get; set; }
    public int? ReplacementProductId { get; set; }
    public IProduct ReplacedBy { get; set; }
}

public interface IDriver
{
    public string Name { get; set; }
    public DateTime BirthDate { get; set; }
    public ILicense License { get; set; }
}

public interface IIncorrectScan
{
    public int IncorrectScanId { get; set; }
    public byte[] ExpectedCode { get; set; }
    public byte[] ActualCode { get; set; }
    public DateTime ScanDate { get; set; }
    public string Details { get; set; }
    public IBarcode ExpectedBarcode { get; set; }
    public IBarcode ActualBarcode { get; set; }
}

public interface ILastLogin
{
    public string Username { get; set; }
    public DateTime LoggedIn { get; set; }
    public DateTime? LoggedOut { get; set; }
    public string SmartcardUsername { get; set; }
    public ILogin Login { get; set; }
}

public interface ILicense
{
    public string Name { get; set; }
    public string LicenseNumber { get; set; }
    public string LicenseClass { get; set; }
    public string Restrictions { get; set; }
    public DateTime ExpirationDate { get; set; }
    public LicenseState? State { get; set; }
    public IDriver Driver { get; set; }
}

public interface IMessage
{
    public int MessageId { get; set; }
    public string FromUsername { get; set; }
    public string ToUsername { get; set; }
    public DateTime Sent { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsRead { get; set; }
    public ILogin Sender { get; set; }
    public ILogin Recipient { get; set; }
}

public interface IOrderLine
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string ConcurrencyToken { get; set; }
    public IAnOrder Order { get; set; }
    public IProduct Product { get; set; }
}

public interface IAnOrder
{
    public int AnOrderId { get; set; }
    public int AlternateId { get; set; }
    public int? CustomerId { get; set; }
    public IConcurrencyInfo Concurrency { get; set; }
    public ICustomer Customer { get; set; }
    public ICollection<IOrderLine> OrderLines { get; set; }
    public ICollection<IOrderNote> Notes { get; set; }
    public string Username { get; set; }
    public ILogin Login { get; set; }
    public void InitializeCollections();
}

public interface IOrderNote
{
    public int NoteId { get; set; }
    public string Note { get; set; }
    public int OrderId { get; set; }
    public IAnOrder Order { get; set; }
}

public interface IOrderQualityCheck
{
    public int OrderId { get; set; }
    public string CheckedBy { get; set; }
    public DateTime CheckedDateTime { get; set; }
    public IAnOrder Order { get; set; }
}

public interface IPageView
{
    public int PageViewId { get; set; }
    public string Username { get; set; }
    public DateTime Viewed { get; set; }
    public string PageUrl { get; set; }
    public ILogin Login { get; set; }
}

public interface IPasswordReset
{
    public int ResetNo { get; set; }
    public string Username { get; set; }
    public string TempPassword { get; set; }
    public string EmailedTo { get; set; }
    public ILogin Login { get; set; }
}

public interface IProductDetail
{
    public int ProductId { get; set; }
    public string Details { get; set; }
    public IProduct Product { get; set; }
}

public interface IProduct
{
    public int ProductId { get; set; }
    public string Description { get; set; }
    public string BaseConcurrency { get; set; }
    public IDimensions Dimensions { get; set; }
    public IConcurrencyInfo ComplexConcurrency { get; set; }
    public IAuditInfo NestedComplexConcurrency { get; set; }
    public ICollection<ISupplier> Suppliers { get; set; }
    public ICollection<IDiscontinuedProduct> Replaces { get; set; }
    public IProductDetail Detail { get; set; }
    public ICollection<IProductReview> Reviews { get; set; }
    public ICollection<IProductPhoto> Photos { get; set; }
    public ICollection<IBarcode> Barcodes { get; set; }
    public void InitializeCollections();
}

public interface IProductPageView : IPageView
{
    public int ProductId { get; set; }
    public IProduct Product { get; set; }
}

public interface IProductPhoto
{
    public int ProductId { get; set; }
    public int PhotoId { get; set; }
    public byte[] Photo { get; set; }
    public ICollection<IProductWebFeature> Features { get; set; }
    public void InitializeCollections();
}

public interface IProductReview
{
    public int ProductId { get; set; }
    public int ReviewId { get; set; }
    public string Review { get; set; }
    public IProduct Product { get; set; }
    public ICollection<IProductWebFeature> Features { get; set; }
    public void InitializeCollections();
}

public interface IProductWebFeature
{
    public int FeatureId { get; set; }
    public int? ProductId { get; set; }
    public int? PhotoId { get; set; }
    public int ReviewId { get; set; }
    public string Heading { get; set; }
    public IProductReview Review { get; set; }
    public IProductPhoto Photo { get; set; }
}

public interface IResolution
{
    public int ResolutionId { get; set; }
    public string Details { get; set; }
    public IComplaint Complaint { get; set; }
}

public interface IRsaToken
{
    public string Serial { get; set; }
    public DateTime Issued { get; set; }
    public string Username { get; set; }
    public ILogin Login { get; set; }
}

public interface ISmartCard
{
    public string Username { get; set; }
    public string CardSerial { get; set; }
    public DateTime Issued { get; set; }
    public ILogin Login { get; set; }
    public ILastLogin LastLogin { get; set; }
}

public interface ISupplierInfo
{
    public int SupplierInfoId { get; set; }
    public string Information { get; set; }
    public int SupplierId { get; set; }
    public ISupplier Supplier { get; set; }
}

public interface ISupplierLogo
{
    public int SupplierId { get; set; }
    public byte[] Logo { get; set; }
}

public interface ISupplier
{
    public int SupplierId { get; set; }
    public string Name { get; set; }
    public ICollection<IProduct> Products { get; set; }
    public ICollection<IBackOrderLine> BackOrderLines { get; set; }
    public ISupplierLogo Logo { get; set; }
    public void InitializeCollections();
}

public interface ISuspiciousActivity
{
    public int SuspiciousActivityId { get; set; }
    public string Activity { get; set; }
    public string Username { get; set; }
}

public interface IAuditInfo
{
    public DateTime ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }

    public IConcurrencyInfo Concurrency { get; set; }
}

public interface ICustomer
{
    public int CustomerId { get; set; }
    public int? HusbandId { get; set; }
    public string Name { get; set; }
    public IContactDetails ContactInfo { get; set; }
    public IAuditInfo Auditing { get; set; }
    public ICollection<IAnOrder> Orders { get; set; }
    public ICollection<ILogin> Logins { get; set; }
    public ICustomer Husband { get; set; }
    public ICustomer Wife { get; set; }
    public ICustomerInfo Info { get; set; }
    public void InitializeCollections();
}

public enum LicenseState
{
    Active = 1,
    Suspended = 2,
    Revoked = 3
}

public interface ILogin
{
    public string Username { get; set; }
    public string AlternateUsername { get; set; }
    public int CustomerId { get; set; }
    public ICustomer Customer { get; set; }
    public ILastLogin LastLogin { get; set; }
    public ICollection<IMessage> SentMessages { get; set; }
    public ICollection<IMessage> ReceivedMessages { get; set; }
    public ICollection<IAnOrder> Orders { get; set; }
    public void InitializeCollections();
}

public interface IPhone
{
    public string PhoneNumber { get; set; }
    public string Extension { get; set; }
    public PhoneType PhoneType { get; set; }
}

public enum PhoneType
{
    Cell = 1,
    Land = 2,
    Satellite = 3
}
