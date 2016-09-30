// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels
{
    public class ChangedOnlyMonsterContext : MonsterContext<
        ChangedOnlyMonsterContext.Customer, ChangedOnlyMonsterContext.Barcode, ChangedOnlyMonsterContext.IncorrectScan,
        ChangedOnlyMonsterContext.BarcodeDetail, ChangedOnlyMonsterContext.Complaint, ChangedOnlyMonsterContext.Resolution,
        ChangedOnlyMonsterContext.Login, ChangedOnlyMonsterContext.SuspiciousActivity, ChangedOnlyMonsterContext.SmartCard,
        ChangedOnlyMonsterContext.RsaToken, ChangedOnlyMonsterContext.PasswordReset, ChangedOnlyMonsterContext.PageView,
        ChangedOnlyMonsterContext.LastLogin, ChangedOnlyMonsterContext.Message, ChangedOnlyMonsterContext.AnOrder,
        ChangedOnlyMonsterContext.OrderNote, ChangedOnlyMonsterContext.OrderQualityCheck, ChangedOnlyMonsterContext.OrderLine,
        ChangedOnlyMonsterContext.Product, ChangedOnlyMonsterContext.ProductDetail, ChangedOnlyMonsterContext.ProductReview,
        ChangedOnlyMonsterContext.ProductPhoto, ChangedOnlyMonsterContext.ProductWebFeature, ChangedOnlyMonsterContext.Supplier,
        ChangedOnlyMonsterContext.SupplierLogo, ChangedOnlyMonsterContext.SupplierInfo, ChangedOnlyMonsterContext.CustomerInfo,
        ChangedOnlyMonsterContext.Computer, ChangedOnlyMonsterContext.ComputerDetail, ChangedOnlyMonsterContext.Driver,
        ChangedOnlyMonsterContext.License>
    {
        public ChangedOnlyMonsterContext(DbContextOptions options, Action<ModelBuilder> onModelCreating)
            : base(options, onModelCreating)
        {
        }

        public class NotificationEntity : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
                {
                    field = value;
                    NotifyChanged(propertyName);
                }
            }
        }

        // TODO: Inheritance
        //public class BackOrderLine2 : BackOrderLine
        //{
        //}

        //public class BackOrderLine : OrderLine
        //{
        //    public DateTime ETA { get; set; }

        //    public int SupplierId { get; set; }
        //    public virtual ISupplier Supplier { get; set; }
        //}

        public class BarcodeDetail : NotificationEntity, IBarcodeDetail
        {
            private byte[] _code;
            private string _registeredTo;

            public byte[] Code
            {
                get { return _code; }
                set { SetWithNotify(value, ref _code); }
            }

            public string RegisteredTo
            {
                get { return _registeredTo; }
                set { SetWithNotify(value, ref _registeredTo); }
            }
        }

        public class Barcode : NotificationEntity, IBarcode
        {
            private byte[] _code;
            private int _productId;
            private string _text;
            private IProduct _product;
            private ICollection<IIncorrectScan> _badScans;
            private IBarcodeDetail _detail;

            public void InitializeCollections()
            {
                BadScans = BadScans ?? new ObservableCollection<IIncorrectScan>();
            }

            public byte[] Code
            {
                get { return _code; }
                set { SetWithNotify(value, ref _code); }
            }

            public int ProductId
            {
                get { return _productId; }
                set { SetWithNotify(value, ref _productId); }
            }

            public string Text
            {
                get { return _text; }
                set { SetWithNotify(value, ref _text); }
            }

            public virtual IProduct Product
            {
                get { return _product; }
                set { SetWithNotify(value, ref _product); }
            }

            public virtual ICollection<IIncorrectScan> BadScans
            {
                get { return _badScans; }
                set { SetWithNotify(value, ref _badScans); }
            }

            public virtual IBarcodeDetail Detail
            {
                get { return _detail; }
                set { SetWithNotify(value, ref _detail); }
            }
        }

        public class Complaint : NotificationEntity, IComplaint
        {
            private ICustomer _customer;
            private IResolution _resolution;
            private int _complaintId;
            private int _alternateId;
            private int? _customerId;
            private DateTime _logged;
            private string _details;

            public int ComplaintId
            {
                get { return _complaintId; }
                set { SetWithNotify(value, ref _complaintId); }
            }

            public int AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public int? CustomerId
            {
                get { return _customerId; }
                set { SetWithNotify(value, ref _customerId); }
            }

            public DateTime Logged
            {
                get { return _logged; }
                set { SetWithNotify(value, ref _logged); }
            }

            public string Details
            {
                get { return _details; }
                set { SetWithNotify(value, ref _details); }
            }

            public virtual ICustomer Customer
            {
                get { return _customer; }
                set { SetWithNotify(value, ref _customer); }
            }

            public virtual IResolution Resolution
            {
                get { return _resolution; }
                set { SetWithNotify(value, ref _resolution); }
            }
        }

        public class ComputerDetail : NotificationEntity, IComputerDetail
        {
            private int _computerDetailId;
            private string _manufacturer;
            private string _model;
            private string _serial;
            private string _specifications;
            private DateTime _purchaseDate;
            private Dimensions _dimensions;
            private IComputer _computer;

            public ComputerDetail()
            {
                Dimensions = new Dimensions();
            }

            public int ComputerDetailId
            {
                get { return _computerDetailId; }
                set { SetWithNotify(value, ref _computerDetailId); }
            }

            public string Manufacturer
            {
                get { return _manufacturer; }
                set { SetWithNotify(value, ref _manufacturer); }
            }

            public string Model
            {
                get { return _model; }
                set { SetWithNotify(value, ref _model); }
            }

            public string Serial
            {
                get { return _serial; }
                set { SetWithNotify(value, ref _serial); }
            }

            public string Specifications
            {
                get { return _specifications; }
                set { SetWithNotify(value, ref _specifications); }
            }

            public DateTime PurchaseDate
            {
                get { return _purchaseDate; }
                set { SetWithNotify(value, ref _purchaseDate); }
            }

            public Dimensions Dimensions
            {
                get { return _dimensions; }
                set { SetWithNotify(value, ref _dimensions); }
            }

            public virtual IComputer Computer
            {
                get { return _computer; }
                set { SetWithNotify(value, ref _computer); }
            }
        }

        public class Computer : NotificationEntity, IComputer
        {
            private int _computerId;
            private string _name;
            private IComputerDetail _computerDetail;

            public int ComputerId
            {
                get { return _computerId; }
                set { SetWithNotify(value, ref _computerId); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public virtual IComputerDetail ComputerDetail
            {
                get { return _computerDetail; }
                set { SetWithNotify(value, ref _computerDetail); }
            }
        }

        public class CustomerInfo : NotificationEntity, ICustomerInfo
        {
            private int _customerInfoId;
            private string _information;

            public int CustomerInfoId
            {
                get { return _customerInfoId; }
                set { SetWithNotify(value, ref _customerInfoId); }
            }

            public string Information
            {
                get { return _information; }
                set { SetWithNotify(value, ref _information); }
            }
        }

        // TODO: Inheritance
        //public class DiscontinuedProduct : NotificationEntity, Product
        //{
        //    public DateTime Discontinued { get; set; }
        //    public int? ReplacementProductId { get; set; }

        //    public virtual IProduct ReplacedBy { get; set; }
        //}

        public class Driver : NotificationEntity, IDriver
        {
            private string _name;
            private DateTime _birthDate;
            private ILicense _license;

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public DateTime BirthDate
            {
                get { return _birthDate; }
                set { SetWithNotify(value, ref _birthDate); }
            }

            public virtual ILicense License
            {
                get { return _license; }
                set { SetWithNotify(value, ref _license); }
            }
        }

        public class IncorrectScan : NotificationEntity, IIncorrectScan
        {
            private int _incorrectScanId;
            private byte[] _expectedCode;
            private byte[] _actualCode;
            private DateTime _scanDate;
            private string _details;
            private IBarcode _expectedBarcode;
            private IBarcode _actualBarcode;

            public int IncorrectScanId
            {
                get { return _incorrectScanId; }
                set { SetWithNotify(value, ref _incorrectScanId); }
            }

            public byte[] ExpectedCode
            {
                get { return _expectedCode; }
                set { SetWithNotify(value, ref _expectedCode); }
            }

            public byte[] ActualCode
            {
                get { return _actualCode; }
                set { SetWithNotify(value, ref _actualCode); }
            }

            public DateTime ScanDate
            {
                get { return _scanDate; }
                set { SetWithNotify(value, ref _scanDate); }
            }

            public string Details
            {
                get { return _details; }
                set { SetWithNotify(value, ref _details); }
            }

            public virtual IBarcode ExpectedBarcode
            {
                get { return _expectedBarcode; }
                set { SetWithNotify(value, ref _expectedBarcode); }
            }

            public virtual IBarcode ActualBarcode
            {
                get { return _actualBarcode; }
                set { SetWithNotify(value, ref _actualBarcode); }
            }
        }

        public class LastLogin : NotificationEntity, ILastLogin
        {
            private string _username;
            private DateTime _loggedIn;
            private DateTime? _loggedOut;
            private string _smartcardUsername;
            private ILogin _login;

            public string Username
            {
                get { return _username; }
                set { SetWithNotify(value, ref _username); }
            }

            public DateTime LoggedIn
            {
                get { return _loggedIn; }
                set { SetWithNotify(value, ref _loggedIn); }
            }

            public DateTime? LoggedOut
            {
                get { return _loggedOut; }
                set { SetWithNotify(value, ref _loggedOut); }
            }

            public string SmartcardUsername
            {
                get { return _smartcardUsername; }
                set { SetWithNotify(value, ref _smartcardUsername); }
            }

            public virtual ILogin Login
            {
                get { return _login; }
                set { SetWithNotify(value, ref _login); }
            }
        }

        public class License : NotificationEntity, ILicense
        {
            private string _name;
            private string _licenseNumber;
            private string _licenseClass;
            private string _restrictions;
            private DateTime _expirationDate;
            private LicenseState? _state;
            private IDriver _driver;

            public License()
            {
                LicenseClass = "C";
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public string LicenseNumber
            {
                get { return _licenseNumber; }
                set { SetWithNotify(value, ref _licenseNumber); }
            }

            public string LicenseClass
            {
                get { return _licenseClass; }
                set { SetWithNotify(value, ref _licenseClass); }
            }

            public string Restrictions
            {
                get { return _restrictions; }
                set { SetWithNotify(value, ref _restrictions); }
            }

            public DateTime ExpirationDate
            {
                get { return _expirationDate; }
                set { SetWithNotify(value, ref _expirationDate); }
            }

            public LicenseState? State
            {
                get { return _state; }
                set { SetWithNotify(value, ref _state); }
            }

            public virtual IDriver Driver
            {
                get { return _driver; }
                set { SetWithNotify(value, ref _driver); }
            }
        }

        public class Message : NotificationEntity, IMessage
        {
            private int _messageId;
            private string _fromUsername;
            private string _toUsername;
            private DateTime _sent;
            private string _subject;
            private string _body;
            private bool _isRead;
            private ILogin _sender;
            private ILogin _recipient;

            public int MessageId
            {
                get { return _messageId; }
                set { SetWithNotify(value, ref _messageId); }
            }

            public string FromUsername
            {
                get { return _fromUsername; }
                set { SetWithNotify(value, ref _fromUsername); }
            }

            public string ToUsername
            {
                get { return _toUsername; }
                set { SetWithNotify(value, ref _toUsername); }
            }

            public DateTime Sent
            {
                get { return _sent; }
                set { SetWithNotify(value, ref _sent); }
            }

            public string Subject
            {
                get { return _subject; }
                set { SetWithNotify(value, ref _subject); }
            }

            public string Body
            {
                get { return _body; }
                set { SetWithNotify(value, ref _body); }
            }

            public bool IsRead
            {
                get { return _isRead; }
                set { SetWithNotify(value, ref _isRead); }
            }

            public virtual ILogin Sender
            {
                get { return _sender; }
                set { SetWithNotify(value, ref _sender); }
            }

            public virtual ILogin Recipient
            {
                get { return _recipient; }
                set { SetWithNotify(value, ref _recipient); }
            }
        }

        public class OrderLine : NotificationEntity, IOrderLine
        {
            private int _orderId;
            private int _productId;
            private int _quantity;
            private string _concurrencyToken;
            private IAnOrder _order;
            private IProduct _product;

            public OrderLine()
            {
                Quantity = 1;
            }

            public int OrderId
            {
                get { return _orderId; }
                set { SetWithNotify(value, ref _orderId); }
            }

            public int ProductId
            {
                get { return _productId; }
                set { SetWithNotify(value, ref _productId); }
            }

            public int Quantity
            {
                get { return _quantity; }
                set { SetWithNotify(value, ref _quantity); }
            }

            public string ConcurrencyToken
            {
                get { return _concurrencyToken; }
                set { SetWithNotify(value, ref _concurrencyToken); }
            }

            public virtual IAnOrder Order
            {
                get { return _order; }
                set { SetWithNotify(value, ref _order); }
            }

            public virtual IProduct Product
            {
                get { return _product; }
                set { SetWithNotify(value, ref _product); }
            }
        }

        public class AnOrder : NotificationEntity, IAnOrder
        {
            private int _anOrderId;
            private int _alternateId;
            private int? _customerId;
            private ConcurrencyInfo _concurrency;
            private ICustomer _customer;
            private ICollection<IOrderLine> _orderLines;
            private ICollection<IOrderNote> _notes;
            private string _username;
            private ILogin _login;

            public AnOrder()
            {
                Concurrency = new ConcurrencyInfo();
            }

            public void InitializeCollections()
            {
                OrderLines = OrderLines ?? new ObservableCollection<IOrderLine>();
                Notes = Notes ?? new ObservableCollection<IOrderNote>();
            }

            public int AnOrderId
            {
                get { return _anOrderId; }
                set { SetWithNotify(value, ref _anOrderId); }
            }

            public int AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public int? CustomerId
            {
                get { return _customerId; }
                set { SetWithNotify(value, ref _customerId); }
            }

            public ConcurrencyInfo Concurrency
            {
                get { return _concurrency; }
                set { SetWithNotify(value, ref _concurrency); }
            }

            public virtual ICustomer Customer
            {
                get { return _customer; }
                set { SetWithNotify(value, ref _customer); }
            }

            public virtual ICollection<IOrderLine> OrderLines
            {
                get { return _orderLines; }
                set { SetWithNotify(value, ref _orderLines); }
            }

            public virtual ICollection<IOrderNote> Notes
            {
                get { return _notes; }
                set { SetWithNotify(value, ref _notes); }
            }

            public string Username
            {
                get { return _username; }
                set { SetWithNotify(value, ref _username); }
            }

            public virtual ILogin Login
            {
                get { return _login; }
                set { SetWithNotify(value, ref _login); }
            }
        }

        public class OrderNote : NotificationEntity, IOrderNote
        {
            private int _noteId;
            private string _note;
            private int _orderId;
            private IAnOrder _order;

            public int NoteId
            {
                get { return _noteId; }
                set { SetWithNotify(value, ref _noteId); }
            }

            public string Note
            {
                get { return _note; }
                set { SetWithNotify(value, ref _note); }
            }

            public int OrderId
            {
                get { return _orderId; }
                set { SetWithNotify(value, ref _orderId); }
            }

            public virtual IAnOrder Order
            {
                get { return _order; }
                set { SetWithNotify(value, ref _order); }
            }
        }

        public class OrderQualityCheck : NotificationEntity, IOrderQualityCheck
        {
            private int _orderId;
            private string _checkedBy;
            private DateTime _checkedDateTime;
            private IAnOrder _order;

            public int OrderId
            {
                get { return _orderId; }
                set { SetWithNotify(value, ref _orderId); }
            }

            public string CheckedBy
            {
                get { return _checkedBy; }
                set { SetWithNotify(value, ref _checkedBy); }
            }

            public DateTime CheckedDateTime
            {
                get { return _checkedDateTime; }
                set { SetWithNotify(value, ref _checkedDateTime); }
            }

            public virtual IAnOrder Order
            {
                get { return _order; }
                set { SetWithNotify(value, ref _order); }
            }
        }

        public class PageView : NotificationEntity, IPageView
        {
            private int _pageViewId;
            private string _username;
            private DateTime _viewed;
            private string _pageUrl;
            private ILogin _login;

            public int PageViewId
            {
                get { return _pageViewId; }
                set { SetWithNotify(value, ref _pageViewId); }
            }

            public string Username
            {
                get { return _username; }
                set { SetWithNotify(value, ref _username); }
            }

            public DateTime Viewed
            {
                get { return _viewed; }
                set { SetWithNotify(value, ref _viewed); }
            }

            public string PageUrl
            {
                get { return _pageUrl; }
                set { SetWithNotify(value, ref _pageUrl); }
            }

            public virtual ILogin Login
            {
                get { return _login; }
                set { SetWithNotify(value, ref _login); }
            }
        }

        public class PasswordReset : NotificationEntity, IPasswordReset
        {
            private int _resetNo;
            private string _username;
            private string _tempPassword;
            private string _emailedTo;
            private ILogin _login;

            public int ResetNo
            {
                get { return _resetNo; }
                set { SetWithNotify(value, ref _resetNo); }
            }

            public string Username
            {
                get { return _username; }
                set { SetWithNotify(value, ref _username); }
            }

            public string TempPassword
            {
                get { return _tempPassword; }
                set { SetWithNotify(value, ref _tempPassword); }
            }

            public string EmailedTo
            {
                get { return _emailedTo; }
                set { SetWithNotify(value, ref _emailedTo); }
            }

            public virtual ILogin Login
            {
                get { return _login; }
                set { SetWithNotify(value, ref _login); }
            }
        }

        public class ProductDetail : NotificationEntity, IProductDetail
        {
            private int _productId;
            private string _details;
            private IProduct _product;

            public int ProductId
            {
                get { return _productId; }
                set { SetWithNotify(value, ref _productId); }
            }

            public string Details
            {
                get { return _details; }
                set { SetWithNotify(value, ref _details); }
            }

            public virtual IProduct Product
            {
                get { return _product; }
                set { SetWithNotify(value, ref _product); }
            }
        }

        public class Product : NotificationEntity, IProduct
        {
            private int _productId;
            private string _description;
            private string _baseConcurrency;
            private Dimensions _dimensions;
            private ConcurrencyInfo _complexConcurrency;
            private AuditInfo _nestedComplexConcurrency;
            private ICollection<ISupplier> _suppliers;
            private IProductDetail _detail;
            private ICollection<IProductReview> _reviews;
            private ICollection<IProductPhoto> _photos;
            private ICollection<IBarcode> _barcodes;

            public Product()
            {
                ComplexConcurrency = new ConcurrencyInfo();
                NestedComplexConcurrency = new AuditInfo();
            }

            public void InitializeCollections()
            {
                Suppliers = Suppliers ?? new ObservableCollection<ISupplier>();
                //Replaces = new ObservableCollection<DiscontinuedProduct>();
                Reviews = Reviews ?? new ObservableCollection<IProductReview>();
                Photos = Photos ?? new ObservableCollection<IProductPhoto>();
                Barcodes = Barcodes ?? new ObservableCollection<IBarcode>();
                Dimensions = Dimensions ?? new Dimensions();
            }

            public int ProductId
            {
                get { return _productId; }
                set { SetWithNotify(value, ref _productId); }
            }

            public string Description
            {
                get { return _description; }
                set { SetWithNotify(value, ref _description); }
            }

            public string BaseConcurrency
            {
                get { return _baseConcurrency; }
                set { SetWithNotify(value, ref _baseConcurrency); }
            }

            public Dimensions Dimensions
            {
                get { return _dimensions; }
                set { SetWithNotify(value, ref _dimensions); }
            }

            public ConcurrencyInfo ComplexConcurrency
            {
                get { return _complexConcurrency; }
                set { SetWithNotify(value, ref _complexConcurrency); }
            }

            public AuditInfo NestedComplexConcurrency
            {
                get { return _nestedComplexConcurrency; }
                set { SetWithNotify(value, ref _nestedComplexConcurrency); }
            }

            public virtual ICollection<ISupplier> Suppliers
            {
                get { return _suppliers; }
                set { SetWithNotify(value, ref _suppliers); }
            }

            //public virtual ICollection<DiscontinuedProduct> Replaces { get; set; }

            public virtual IProductDetail Detail
            {
                get { return _detail; }
                set { SetWithNotify(value, ref _detail); }
            }

            public virtual ICollection<IProductReview> Reviews
            {
                get { return _reviews; }
                set { SetWithNotify(value, ref _reviews); }
            }

            public virtual ICollection<IProductPhoto> Photos
            {
                get { return _photos; }
                set { SetWithNotify(value, ref _photos); }
            }

            public virtual ICollection<IBarcode> Barcodes
            {
                get { return _barcodes; }
                set { SetWithNotify(value, ref _barcodes); }
            }
        }

        // TODO: Inheritance
        //public class ProductPageView : NotificationEntity, PageView
        //{
        //    public int ProductId { get; set; }

        //    public virtual IProduct Product { get; set; }
        //}

        public class ProductPhoto : NotificationEntity, IProductPhoto
        {
            private int _productId;
            private int _photoId;
            private byte[] _photo;
            private ICollection<IProductWebFeature> _features;

            public void InitializeCollections()
            {
                Features = Features ?? new ObservableCollection<IProductWebFeature>();
            }

            public int ProductId
            {
                get { return _productId; }
                set { SetWithNotify(value, ref _productId); }
            }

            public int PhotoId
            {
                get { return _photoId; }
                set { SetWithNotify(value, ref _photoId); }
            }

            public byte[] Photo
            {
                get { return _photo; }
                set { SetWithNotify(value, ref _photo); }
            }

            public virtual ICollection<IProductWebFeature> Features
            {
                get { return _features; }
                set { SetWithNotify(value, ref _features); }
            }
        }

        public class ProductReview : NotificationEntity, IProductReview
        {
            private int _productId;
            private int _reviewId;
            private string _review;
            private IProduct _product;
            private ICollection<IProductWebFeature> _features;

            public void InitializeCollections()
            {
                Features = Features ?? new ObservableCollection<IProductWebFeature>();
            }

            public int ProductId
            {
                get { return _productId; }
                set { SetWithNotify(value, ref _productId); }
            }

            public int ReviewId
            {
                get { return _reviewId; }
                set { SetWithNotify(value, ref _reviewId); }
            }

            public string Review
            {
                get { return _review; }
                set { SetWithNotify(value, ref _review); }
            }

            public virtual IProduct Product
            {
                get { return _product; }
                set { SetWithNotify(value, ref _product); }
            }

            public virtual ICollection<IProductWebFeature> Features
            {
                get { return _features; }
                set { SetWithNotify(value, ref _features); }
            }
        }

        public class ProductWebFeature : NotificationEntity, IProductWebFeature
        {
            private int _featureId;
            private int? _productId;
            private int? _photoId;
            private int _reviewId;
            private string _heading;
            private IProductReview _review;
            private IProductPhoto _photo;

            public int FeatureId
            {
                get { return _featureId; }
                set { SetWithNotify(value, ref _featureId); }
            }

            public int? ProductId
            {
                get { return _productId; }
                set { SetWithNotify(value, ref _productId); }
            }

            public int? PhotoId
            {
                get { return _photoId; }
                set { SetWithNotify(value, ref _photoId); }
            }

            public int ReviewId
            {
                get { return _reviewId; }
                set { SetWithNotify(value, ref _reviewId); }
            }

            public string Heading
            {
                get { return _heading; }
                set { SetWithNotify(value, ref _heading); }
            }

            public virtual IProductReview Review
            {
                get { return _review; }
                set { SetWithNotify(value, ref _review); }
            }

            public virtual IProductPhoto Photo
            {
                get { return _photo; }
                set { SetWithNotify(value, ref _photo); }
            }
        }

        public class Resolution : NotificationEntity, IResolution
        {
            private int _resolutionId;
            private string _details;
            private IComplaint _complaint;

            public int ResolutionId
            {
                get { return _resolutionId; }
                set { SetWithNotify(value, ref _resolutionId); }
            }

            public string Details
            {
                get { return _details; }
                set { SetWithNotify(value, ref _details); }
            }

            public virtual IComplaint Complaint
            {
                get { return _complaint; }
                set { SetWithNotify(value, ref _complaint); }
            }
        }

        public class RsaToken : NotificationEntity, IRsaToken
        {
            private string _serial;
            private DateTime _issued;
            private string _username;
            private ILogin _login;

            public string Serial
            {
                get { return _serial; }
                set { SetWithNotify(value, ref _serial); }
            }

            public DateTime Issued
            {
                get { return _issued; }
                set { SetWithNotify(value, ref _issued); }
            }

            public string Username
            {
                get { return _username; }
                set { SetWithNotify(value, ref _username); }
            }

            public virtual ILogin Login
            {
                get { return _login; }
                set { SetWithNotify(value, ref _login); }
            }
        }

        public class SmartCard : NotificationEntity, ISmartCard
        {
            private string _username;
            private string _cardSerial;
            private DateTime _issued;
            private ILogin _login;
            private ILastLogin _lastLogin;

            public string Username
            {
                get { return _username; }
                set { SetWithNotify(value, ref _username); }
            }

            public string CardSerial
            {
                get { return _cardSerial; }
                set { SetWithNotify(value, ref _cardSerial); }
            }

            public DateTime Issued
            {
                get { return _issued; }
                set { SetWithNotify(value, ref _issued); }
            }

            public virtual ILogin Login
            {
                get { return _login; }
                set { SetWithNotify(value, ref _login); }
            }

            public virtual ILastLogin LastLogin
            {
                get { return _lastLogin; }
                set { SetWithNotify(value, ref _lastLogin); }
            }
        }

        public class SupplierInfo : NotificationEntity, ISupplierInfo
        {
            private int _supplierInfoId;
            private string _information;
            private int _supplierId;
            private ISupplier _supplier;

            public int SupplierInfoId
            {
                get { return _supplierInfoId; }
                set { SetWithNotify(value, ref _supplierInfoId); }
            }

            public string Information
            {
                get { return _information; }
                set { SetWithNotify(value, ref _information); }
            }

            public int SupplierId
            {
                get { return _supplierId; }
                set { SetWithNotify(value, ref _supplierId); }
            }

            public virtual ISupplier Supplier
            {
                get { return _supplier; }
                set { SetWithNotify(value, ref _supplier); }
            }
        }

        public class SupplierLogo : NotificationEntity, ISupplierLogo
        {
            private int _supplierId;
            private byte[] _logo;

            public int SupplierId
            {
                get { return _supplierId; }
                set { SetWithNotify(value, ref _supplierId); }
            }

            public byte[] Logo
            {
                get { return _logo; }
                set { SetWithNotify(value, ref _logo); }
            }
        }

        public class Supplier : NotificationEntity, ISupplier
        {
            private int _supplierId;
            private string _name;
            private ICollection<IProduct> _products;
            private ISupplierLogo _logo;

            public void InitializeCollections()
            {
                Products = Products ?? new ObservableCollection<IProduct>();
                //BackOrderLines = new ObservableCollection<BackOrderLine>();
            }

            public int SupplierId
            {
                get { return _supplierId; }
                set { SetWithNotify(value, ref _supplierId); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public virtual ICollection<IProduct> Products
            {
                get { return _products; }
                set { SetWithNotify(value, ref _products); }
            }

            //public virtual ICollection<BackOrderLine> BackOrderLines { get; set; }

            public virtual ISupplierLogo Logo
            {
                get { return _logo; }
                set { SetWithNotify(value, ref _logo); }
            }
        }

        public class SuspiciousActivity : NotificationEntity, ISuspiciousActivity
        {
            private int _suspiciousActivityId;
            private string _activity;
            private string _username;

            public int SuspiciousActivityId
            {
                get { return _suspiciousActivityId; }
                set { SetWithNotify(value, ref _suspiciousActivityId); }
            }

            public string Activity
            {
                get { return _activity; }
                set { SetWithNotify(value, ref _activity); }
            }

            public string Username
            {
                get { return _username; }
                set { SetWithNotify(value, ref _username); }
            }
        }

        public class Customer : NotificationEntity, ICustomer
        {
            private int _customerId;
            private int? _husbandId;
            private string _name;
            private ContactDetails _contactInfo;
            private AuditInfo _auditing;
            private ICollection<IAnOrder> _orders;
            private ICollection<ILogin> _logins;
            private ICustomer _husband;
            private ICustomer _wife;
            private ICustomerInfo _info;

            public Customer()
            {
                ContactInfo = new ContactDetails();
                Auditing = new AuditInfo();
            }

            public void InitializeCollections()
            {
                Orders = Orders ?? new ObservableCollection<IAnOrder>();
                Logins = Logins ?? new ObservableCollection<ILogin>();
            }

            public int CustomerId
            {
                get { return _customerId; }
                set { SetWithNotify(value, ref _customerId); }
            }

            public int? HusbandId
            {
                get { return _husbandId; }
                set { SetWithNotify(value, ref _husbandId); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public ContactDetails ContactInfo
            {
                get { return _contactInfo; }
                set { SetWithNotify(value, ref _contactInfo); }
            }

            public AuditInfo Auditing
            {
                get { return _auditing; }
                set { SetWithNotify(value, ref _auditing); }
            }

            public virtual ICollection<IAnOrder> Orders
            {
                get { return _orders; }
                set { SetWithNotify(value, ref _orders); }
            }

            public virtual ICollection<ILogin> Logins
            {
                get { return _logins; }
                set { SetWithNotify(value, ref _logins); }
            }

            public virtual ICustomer Husband
            {
                get { return _husband; }
                set { SetWithNotify(value, ref _husband); }
            }

            public virtual ICustomer Wife
            {
                get { return _wife; }
                set { SetWithNotify(value, ref _wife); }
            }

            public virtual ICustomerInfo Info
            {
                get { return _info; }
                set { SetWithNotify(value, ref _info); }
            }
        }

        public class Login : NotificationEntity, ILogin
        {
            private string _username;
            private string _alternateUsername;
            private int _customerId;
            private ICustomer _customer;
            private ILastLogin _lastLogin;
            private ICollection<IMessage> _sentMessages;
            private ICollection<IMessage> _receivedMessages;
            private ICollection<IAnOrder> _orders;

            public void InitializeCollections()
            {
                SentMessages = SentMessages ?? new ObservableCollection<IMessage>();
                ReceivedMessages = ReceivedMessages ?? new ObservableCollection<IMessage>();
                Orders = Orders ?? new ObservableCollection<IAnOrder>();
            }

            public string Username
            {
                get { return _username; }
                set { SetWithNotify(value, ref _username); }
            }

            public string AlternateUsername
            {
                get { return _alternateUsername; }
                set { SetWithNotify(value, ref _alternateUsername); }
            }

            public int CustomerId
            {
                get { return _customerId; }
                set { SetWithNotify(value, ref _customerId); }
            }

            public virtual ICustomer Customer
            {
                get { return _customer; }
                set { SetWithNotify(value, ref _customer); }
            }

            public virtual ILastLogin LastLogin
            {
                get { return _lastLogin; }
                set { SetWithNotify(value, ref _lastLogin); }
            }

            public virtual ICollection<IMessage> SentMessages
            {
                get { return _sentMessages; }
                set { SetWithNotify(value, ref _sentMessages); }
            }

            public virtual ICollection<IMessage> ReceivedMessages
            {
                get { return _receivedMessages; }
                set { SetWithNotify(value, ref _receivedMessages); }
            }

            public virtual ICollection<IAnOrder> Orders
            {
                get { return _orders; }
                set { SetWithNotify(value, ref _orders); }
            }
        }
    }
}
