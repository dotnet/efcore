// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels;

public interface IBackOrderLine : IOrderLine
{
    DateTime ETA { get; set; }
    int SupplierId { get; set; }
    ISupplier Supplier { get; set; }
}

public interface IBarcodeDetail
{
    byte[] Code { get; set; }
    string RegisteredTo { get; set; }
}

public interface IBarcode
{
    byte[] Code { get; set; }
    int ProductId { get; set; }
    string Text { get; set; }
    IProduct Product { get; set; }
    ICollection<IIncorrectScan> BadScans { get; set; }
    IBarcodeDetail Detail { get; set; }
    void InitializeCollections();
}

public interface IComplaint
{
    int ComplaintId { get; set; }
    int AlternateId { get; set; }
    int? CustomerId { get; set; }
    DateTime Logged { get; set; }
    string Details { get; set; }
    ICustomer Customer { get; set; }
    IResolution Resolution { get; set; }
}

public interface IComputerDetail
{
    int ComputerDetailId { get; set; }
    string Manufacturer { get; set; }
    string Model { get; set; }
    string Serial { get; set; }
    string Specifications { get; set; }
    DateTime PurchaseDate { get; set; }
    IDimensions Dimensions { get; set; }
    IComputer Computer { get; set; }
}

public interface IComputer
{
    int ComputerId { get; set; }
    string Name { get; set; }
    IComputerDetail ComputerDetail { get; set; }
}

public interface IConcurrencyInfo
{
    bool Active { get; set; }
    string Token { get; set; }
    DateTime? QueriedDateTime { get; set; }
}

public interface IContactDetails
{
    bool Active { get; set; }
    string Email { get; set; }

    IPhone HomePhone { get; set; }
    IPhone WorkPhone { get; set; }
    IPhone MobilePhone { get; set; }
}

public interface ICustomerInfo
{
    int CustomerInfoId { get; set; }
    string Information { get; set; }
}

public interface IDimensions
{
    decimal Width { get; set; }
    decimal Height { get; set; }
    decimal Depth { get; set; }
}

public interface IDiscontinuedProduct : IProduct
{
    DateTime Discontinued { get; set; }
    int? ReplacementProductId { get; set; }
    IProduct ReplacedBy { get; set; }
}

public interface IDriver
{
    string Name { get; set; }
    DateTime BirthDate { get; set; }
    ILicense License { get; set; }
}

public interface IIncorrectScan
{
    int IncorrectScanId { get; set; }
    byte[] ExpectedCode { get; set; }
    byte[] ActualCode { get; set; }
    DateTime ScanDate { get; set; }
    string Details { get; set; }
    IBarcode ExpectedBarcode { get; set; }
    IBarcode ActualBarcode { get; set; }
}

public interface ILastLogin
{
    string Username { get; set; }
    DateTime LoggedIn { get; set; }
    DateTime? LoggedOut { get; set; }
    string SmartcardUsername { get; set; }
    ILogin Login { get; set; }
}

public interface ILicense
{
    string Name { get; set; }
    string LicenseNumber { get; set; }
    string LicenseClass { get; set; }
    string Restrictions { get; set; }
    DateTime ExpirationDate { get; set; }
    LicenseState? State { get; set; }
    IDriver Driver { get; set; }
}

public interface IMessage
{
    int MessageId { get; set; }
    string FromUsername { get; set; }
    string ToUsername { get; set; }
    DateTime Sent { get; set; }
    string Subject { get; set; }
    string Body { get; set; }
    bool IsRead { get; set; }
    ILogin Sender { get; set; }
    ILogin Recipient { get; set; }
}

public interface IOrderLine
{
    int OrderId { get; set; }
    int ProductId { get; set; }
    int Quantity { get; set; }
    string ConcurrencyToken { get; set; }
    IAnOrder Order { get; set; }
    IProduct Product { get; set; }
}

public interface IAnOrder
{
    int AnOrderId { get; set; }
    int AlternateId { get; set; }
    int? CustomerId { get; set; }
    IConcurrencyInfo Concurrency { get; set; }
    ICustomer Customer { get; set; }
    ICollection<IOrderLine> OrderLines { get; set; }
    ICollection<IOrderNote> Notes { get; set; }
    string Username { get; set; }
    ILogin Login { get; set; }
    void InitializeCollections();
}

public interface IOrderNote
{
    int NoteId { get; set; }
    string Note { get; set; }
    int OrderId { get; set; }
    IAnOrder Order { get; set; }
}

public interface IOrderQualityCheck
{
    int OrderId { get; set; }
    string CheckedBy { get; set; }
    DateTime CheckedDateTime { get; set; }
    IAnOrder Order { get; set; }
}

public interface IPageView
{
    int PageViewId { get; set; }
    string Username { get; set; }
    DateTime Viewed { get; set; }
    string PageUrl { get; set; }
    ILogin Login { get; set; }
}

public interface IPasswordReset
{
    int ResetNo { get; set; }
    string Username { get; set; }
    string TempPassword { get; set; }
    string EmailedTo { get; set; }
    ILogin Login { get; set; }
}

public interface IProductDetail
{
    int ProductId { get; set; }
    string Details { get; set; }
    IProduct Product { get; set; }
}

public interface IProduct
{
    int ProductId { get; set; }
    string Description { get; set; }
    string BaseConcurrency { get; set; }
    IDimensions Dimensions { get; set; }
    IConcurrencyInfo ComplexConcurrency { get; set; }
    IAuditInfo NestedComplexConcurrency { get; set; }
    ICollection<ISupplier> Suppliers { get; set; }
    ICollection<IDiscontinuedProduct> Replaces { get; set; }
    IProductDetail Detail { get; set; }
    ICollection<IProductReview> Reviews { get; set; }
    ICollection<IProductPhoto> Photos { get; set; }
    ICollection<IBarcode> Barcodes { get; set; }
    void InitializeCollections();
}

public interface IProductPageView : IPageView
{
    int ProductId { get; set; }
    IProduct Product { get; set; }
}

public interface IProductPhoto
{
    int ProductId { get; set; }
    int PhotoId { get; set; }
    byte[] Photo { get; set; }
    ICollection<IProductWebFeature> Features { get; set; }
    void InitializeCollections();
}

public interface IProductReview
{
    int ProductId { get; set; }
    int ReviewId { get; set; }
    string Review { get; set; }
    IProduct Product { get; set; }
    ICollection<IProductWebFeature> Features { get; set; }
    void InitializeCollections();
}

public interface IProductWebFeature
{
    int FeatureId { get; set; }
    int? ProductId { get; set; }
    int? PhotoId { get; set; }
    int ReviewId { get; set; }
    string Heading { get; set; }
    IProductReview Review { get; set; }
    IProductPhoto Photo { get; set; }
}

public interface IResolution
{
    int ResolutionId { get; set; }
    string Details { get; set; }
    IComplaint Complaint { get; set; }
}

public interface IRsaToken
{
    string Serial { get; set; }
    DateTime Issued { get; set; }
    string Username { get; set; }
    ILogin Login { get; set; }
}

public interface ISmartCard
{
    string Username { get; set; }
    string CardSerial { get; set; }
    DateTime Issued { get; set; }
    ILogin Login { get; set; }
    ILastLogin LastLogin { get; set; }
}

public interface ISupplierInfo
{
    int SupplierInfoId { get; set; }
    string Information { get; set; }
    int SupplierId { get; set; }
    ISupplier Supplier { get; set; }
}

public interface ISupplierLogo
{
    int SupplierId { get; set; }
    byte[] Logo { get; set; }
}

public interface ISupplier
{
    int SupplierId { get; set; }
    string Name { get; set; }
    ICollection<IProduct> Products { get; set; }
    ICollection<IBackOrderLine> BackOrderLines { get; set; }
    ISupplierLogo Logo { get; set; }
    void InitializeCollections();
}

public interface ISuspiciousActivity
{
    int SuspiciousActivityId { get; set; }
    string Activity { get; set; }
    string Username { get; set; }
}

public interface IAuditInfo
{
    DateTime ModifiedDate { get; set; }
    string ModifiedBy { get; set; }

    IConcurrencyInfo Concurrency { get; set; }
}

public interface ICustomer
{
    int CustomerId { get; set; }
    int? HusbandId { get; set; }
    string Name { get; set; }
    IContactDetails ContactInfo { get; set; }
    IAuditInfo Auditing { get; set; }
    ICollection<IAnOrder> Orders { get; set; }
    ICollection<ILogin> Logins { get; set; }
    ICustomer Husband { get; set; }
    ICustomer Wife { get; set; }
    ICustomerInfo Info { get; set; }
    void InitializeCollections();
}

public enum LicenseState
{
    Active = 1,
    Suspended = 2,
    Revoked = 3
}

public interface ILogin
{
    string Username { get; set; }
    string AlternateUsername { get; set; }
    int CustomerId { get; set; }
    ICustomer Customer { get; set; }
    ILastLogin LastLogin { get; set; }
    ICollection<IMessage> SentMessages { get; set; }
    ICollection<IMessage> ReceivedMessages { get; set; }
    ICollection<IAnOrder> Orders { get; set; }
    void InitializeCollections();
}

public interface IPhone
{
    string PhoneNumber { get; set; }
    string Extension { get; set; }
    PhoneType PhoneType { get; set; }
}

public enum PhoneType
{
    Cell = 1,
    Land = 2,
    Satellite = 3
}
