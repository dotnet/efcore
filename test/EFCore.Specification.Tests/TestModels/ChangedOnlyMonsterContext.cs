// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable ConvertToAutoProperty
namespace Microsoft.EntityFrameworkCore.TestModels;

#nullable disable

public class ChangedOnlyMonsterContext(DbContextOptions options) : MonsterContext<
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
    ChangedOnlyMonsterContext.License, ChangedOnlyMonsterContext.ConcurrencyInfo, ChangedOnlyMonsterContext.AuditInfo,
    ChangedOnlyMonsterContext.ContactDetails, ChangedOnlyMonsterContext.Dimensions, ChangedOnlyMonsterContext.Phone,
    ChangedOnlyMonsterContext.BackOrderLine, ChangedOnlyMonsterContext.DiscontinuedProduct,
    ChangedOnlyMonsterContext.ProductPageView>(options)
{
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

    public class BackOrderLine2 : BackOrderLine;

    public class BackOrderLine : OrderLine, IBackOrderLine
    {
        private ISupplier _supplier;
        private int _supplierId;
        private DateTime _eta;

        public BackOrderLine()
        {
            ETA = DateTime.Now;
        }

        public DateTime ETA
        {
            get => _eta;
            set => SetWithNotify(value, ref _eta);
        }

        public int SupplierId
        {
            get => _supplierId;
            set => SetWithNotify(value, ref _supplierId);
        }

        public virtual ISupplier Supplier
        {
            get => _supplier;
            set => SetWithNotify(value, ref _supplier);
        }
    }

    public class BarcodeDetail : NotificationEntity, IBarcodeDetail
    {
        private byte[] _code;
        private string _registeredTo;

        public byte[] Code
        {
            get => _code;
            set => SetWithNotify(value, ref _code);
        }

        public string RegisteredTo
        {
            get => _registeredTo;
            set => SetWithNotify(value, ref _registeredTo);
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
            => BadScans ??= new ObservableCollection<IIncorrectScan>();

        public byte[] Code
        {
            get => _code;
            set => SetWithNotify(value, ref _code);
        }

        public int ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public string Text
        {
            get => _text;
            set => SetWithNotify(value, ref _text);
        }

        public virtual IProduct Product
        {
            get => _product;
            set => SetWithNotify(value, ref _product);
        }

        public virtual ICollection<IIncorrectScan> BadScans
        {
            get => _badScans;
            set => SetWithNotify(value, ref _badScans);
        }

        public virtual IBarcodeDetail Detail
        {
            get => _detail;
            set => SetWithNotify(value, ref _detail);
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
            get => _complaintId;
            set => SetWithNotify(value, ref _complaintId);
        }

        public int AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public int? CustomerId
        {
            get => _customerId;
            set => SetWithNotify(value, ref _customerId);
        }

        public DateTime Logged
        {
            get => _logged;
            set => SetWithNotify(value, ref _logged);
        }

        public string Details
        {
            get => _details;
            set => SetWithNotify(value, ref _details);
        }

        public virtual ICustomer Customer
        {
            get => _customer;
            set => SetWithNotify(value, ref _customer);
        }

        public virtual IResolution Resolution
        {
            get => _resolution;
            set => SetWithNotify(value, ref _resolution);
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
        private IDimensions _dimensions;
        private IComputer _computer;

        public ComputerDetail()
        {
            Dimensions = new Dimensions();
        }

        public int ComputerDetailId
        {
            get => _computerDetailId;
            set => SetWithNotify(value, ref _computerDetailId);
        }

        public string Manufacturer
        {
            get => _manufacturer;
            set => SetWithNotify(value, ref _manufacturer);
        }

        public string Model
        {
            get => _model;
            set => SetWithNotify(value, ref _model);
        }

        public string Serial
        {
            get => _serial;
            set => SetWithNotify(value, ref _serial);
        }

        public string Specifications
        {
            get => _specifications;
            set => SetWithNotify(value, ref _specifications);
        }

        public DateTime PurchaseDate
        {
            get => _purchaseDate;
            set => SetWithNotify(value, ref _purchaseDate);
        }

        public IDimensions Dimensions
        {
            get => _dimensions;
            set => SetWithNotify(value, ref _dimensions);
        }

        public virtual IComputer Computer
        {
            get => _computer;
            set => SetWithNotify(value, ref _computer);
        }
    }

    public class Computer : NotificationEntity, IComputer
    {
        private int _computerId;
        private string _name;
        private IComputerDetail _computerDetail;

        public int ComputerId
        {
            get => _computerId;
            set => SetWithNotify(value, ref _computerId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public virtual IComputerDetail ComputerDetail
        {
            get => _computerDetail;
            set => SetWithNotify(value, ref _computerDetail);
        }
    }

    public class ConcurrencyInfo : NotificationEntity, IConcurrencyInfo
    {
        private bool _active;
        private string _token;
        private DateTime? _queriedDateTime;

        public bool Active
        {
            get => _active;
            set => SetWithNotify(value, ref _active);
        }

        public string Token
        {
            get => _token;
            set => SetWithNotify(value, ref _token);
        }

        public DateTime? QueriedDateTime
        {
            get => _queriedDateTime;
            set => SetWithNotify(value, ref _queriedDateTime);
        }
    }

    public class ContactDetails : NotificationEntity, IContactDetails
    {
        private bool _active;
        private string _email;
        private IPhone _homePhone;
        private IPhone _workPhone;
        private IPhone _mobilePhone;

        public ContactDetails()
        {
            HomePhone = new Phone();
            WorkPhone = new Phone();
            MobilePhone = new Phone();
        }

        public bool Active
        {
            get => _active;
            set => SetWithNotify(value, ref _active);
        }

        public string Email
        {
            get => _email;
            set => SetWithNotify(value, ref _email);
        }

        public IPhone HomePhone
        {
            get => _homePhone;
            set => SetWithNotify(value, ref _homePhone);
        }

        public IPhone WorkPhone
        {
            get => _workPhone;
            set => SetWithNotify(value, ref _workPhone);
        }

        public IPhone MobilePhone
        {
            get => _mobilePhone;
            set => SetWithNotify(value, ref _mobilePhone);
        }
    }

    public class CustomerInfo : NotificationEntity, ICustomerInfo
    {
        private int _customerInfoId;
        private string _information;

        public int CustomerInfoId
        {
            get => _customerInfoId;
            set => SetWithNotify(value, ref _customerInfoId);
        }

        public string Information
        {
            get => _information;
            set => SetWithNotify(value, ref _information);
        }
    }

    public class Dimensions : NotificationEntity, IDimensions
    {
        private decimal _width;
        private decimal _height;
        private decimal _depth;

        public decimal Width
        {
            get => _width;
            set => SetWithNotify(value, ref _width);
        }

        public decimal Height
        {
            get => _height;
            set => SetWithNotify(value, ref _height);
        }

        public decimal Depth
        {
            get => _depth;
            set => SetWithNotify(value, ref _depth);
        }
    }

    public class DiscontinuedProduct : Product, IDiscontinuedProduct
    {
        private IProduct _replacedBy;
        private DateTime _discontinued;
        private int? _replacementProductId;

        public DateTime Discontinued
        {
            get => _discontinued;
            set => _discontinued = value;
        }

        public int? ReplacementProductId
        {
            get => _replacementProductId;
            set => _replacementProductId = value;
        }

        public virtual IProduct ReplacedBy
        {
            get => _replacedBy;
            set => _replacedBy = value;
        }
    }

    public class Driver : NotificationEntity, IDriver
    {
        private string _name;
        private DateTime _birthDate;
        private ILicense _license;

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public DateTime BirthDate
        {
            get => _birthDate;
            set => SetWithNotify(value, ref _birthDate);
        }

        public virtual ILicense License
        {
            get => _license;
            set => SetWithNotify(value, ref _license);
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
            get => _incorrectScanId;
            set => SetWithNotify(value, ref _incorrectScanId);
        }

        public byte[] ExpectedCode
        {
            get => _expectedCode;
            set => SetWithNotify(value, ref _expectedCode);
        }

        public byte[] ActualCode
        {
            get => _actualCode;
            set => SetWithNotify(value, ref _actualCode);
        }

        public DateTime ScanDate
        {
            get => _scanDate;
            set => SetWithNotify(value, ref _scanDate);
        }

        public string Details
        {
            get => _details;
            set => SetWithNotify(value, ref _details);
        }

        public virtual IBarcode ExpectedBarcode
        {
            get => _expectedBarcode;
            set => SetWithNotify(value, ref _expectedBarcode);
        }

        public virtual IBarcode ActualBarcode
        {
            get => _actualBarcode;
            set => SetWithNotify(value, ref _actualBarcode);
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
            get => _username;
            set => SetWithNotify(value, ref _username);
        }

        public DateTime LoggedIn
        {
            get => _loggedIn;
            set => SetWithNotify(value, ref _loggedIn);
        }

        public DateTime? LoggedOut
        {
            get => _loggedOut;
            set => SetWithNotify(value, ref _loggedOut);
        }

        public string SmartcardUsername
        {
            get => _smartcardUsername;
            set => SetWithNotify(value, ref _smartcardUsername);
        }

        public virtual ILogin Login
        {
            get => _login;
            set => SetWithNotify(value, ref _login);
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
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public string LicenseNumber
        {
            get => _licenseNumber;
            set => SetWithNotify(value, ref _licenseNumber);
        }

        public string LicenseClass
        {
            get => _licenseClass;
            set => SetWithNotify(value, ref _licenseClass);
        }

        public string Restrictions
        {
            get => _restrictions;
            set => SetWithNotify(value, ref _restrictions);
        }

        public DateTime ExpirationDate
        {
            get => _expirationDate;
            set => SetWithNotify(value, ref _expirationDate);
        }

        public LicenseState? State
        {
            get => _state;
            set => SetWithNotify(value, ref _state);
        }

        public virtual IDriver Driver
        {
            get => _driver;
            set => SetWithNotify(value, ref _driver);
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
            get => _messageId;
            set => SetWithNotify(value, ref _messageId);
        }

        public string FromUsername
        {
            get => _fromUsername;
            set => SetWithNotify(value, ref _fromUsername);
        }

        public string ToUsername
        {
            get => _toUsername;
            set => SetWithNotify(value, ref _toUsername);
        }

        public DateTime Sent
        {
            get => _sent;
            set => SetWithNotify(value, ref _sent);
        }

        public string Subject
        {
            get => _subject;
            set => SetWithNotify(value, ref _subject);
        }

        public string Body
        {
            get => _body;
            set => SetWithNotify(value, ref _body);
        }

        public bool IsRead
        {
            get => _isRead;
            set => SetWithNotify(value, ref _isRead);
        }

        public virtual ILogin Sender
        {
            get => _sender;
            set => SetWithNotify(value, ref _sender);
        }

        public virtual ILogin Recipient
        {
            get => _recipient;
            set => SetWithNotify(value, ref _recipient);
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
            get => _orderId;
            set => SetWithNotify(value, ref _orderId);
        }

        public int ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetWithNotify(value, ref _quantity);
        }

        public string ConcurrencyToken
        {
            get => _concurrencyToken;
            set => SetWithNotify(value, ref _concurrencyToken);
        }

        public virtual IAnOrder Order
        {
            get => _order;
            set => SetWithNotify(value, ref _order);
        }

        public virtual IProduct Product
        {
            get => _product;
            set => SetWithNotify(value, ref _product);
        }
    }

    public class AnOrder : NotificationEntity, IAnOrder
    {
        private int _anOrderId;
        private int _alternateId;
        private int? _customerId;
        private IConcurrencyInfo _concurrency;
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
            OrderLines ??= new ObservableCollection<IOrderLine>();
            Notes ??= new ObservableCollection<IOrderNote>();
        }

        public int AnOrderId
        {
            get => _anOrderId;
            set => SetWithNotify(value, ref _anOrderId);
        }

        public int AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public int? CustomerId
        {
            get => _customerId;
            set => SetWithNotify(value, ref _customerId);
        }

        public IConcurrencyInfo Concurrency
        {
            get => _concurrency;
            set => SetWithNotify(value, ref _concurrency);
        }

        public virtual ICustomer Customer
        {
            get => _customer;
            set => SetWithNotify(value, ref _customer);
        }

        public virtual ICollection<IOrderLine> OrderLines
        {
            get => _orderLines;
            set => SetWithNotify(value, ref _orderLines);
        }

        public virtual ICollection<IOrderNote> Notes
        {
            get => _notes;
            set => SetWithNotify(value, ref _notes);
        }

        public string Username
        {
            get => _username;
            set => SetWithNotify(value, ref _username);
        }

        public virtual ILogin Login
        {
            get => _login;
            set => SetWithNotify(value, ref _login);
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
            get => _noteId;
            set => SetWithNotify(value, ref _noteId);
        }

        public string Note
        {
            get => _note;
            set => SetWithNotify(value, ref _note);
        }

        public int OrderId
        {
            get => _orderId;
            set => SetWithNotify(value, ref _orderId);
        }

        public virtual IAnOrder Order
        {
            get => _order;
            set => SetWithNotify(value, ref _order);
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
            get => _orderId;
            set => SetWithNotify(value, ref _orderId);
        }

        public string CheckedBy
        {
            get => _checkedBy;
            set => SetWithNotify(value, ref _checkedBy);
        }

        public DateTime CheckedDateTime
        {
            get => _checkedDateTime;
            set => SetWithNotify(value, ref _checkedDateTime);
        }

        public virtual IAnOrder Order
        {
            get => _order;
            set => SetWithNotify(value, ref _order);
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
            get => _pageViewId;
            set => SetWithNotify(value, ref _pageViewId);
        }

        public string Username
        {
            get => _username;
            set => SetWithNotify(value, ref _username);
        }

        public DateTime Viewed
        {
            get => _viewed;
            set => SetWithNotify(value, ref _viewed);
        }

        public string PageUrl
        {
            get => _pageUrl;
            set => SetWithNotify(value, ref _pageUrl);
        }

        public virtual ILogin Login
        {
            get => _login;
            set => SetWithNotify(value, ref _login);
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
            get => _resetNo;
            set => SetWithNotify(value, ref _resetNo);
        }

        public string Username
        {
            get => _username;
            set => SetWithNotify(value, ref _username);
        }

        public string TempPassword
        {
            get => _tempPassword;
            set => SetWithNotify(value, ref _tempPassword);
        }

        public string EmailedTo
        {
            get => _emailedTo;
            set => SetWithNotify(value, ref _emailedTo);
        }

        public virtual ILogin Login
        {
            get => _login;
            set => SetWithNotify(value, ref _login);
        }
    }

    public class ProductDetail : NotificationEntity, IProductDetail
    {
        private int _productId;
        private string _details;
        private IProduct _product;

        public int ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public string Details
        {
            get => _details;
            set => SetWithNotify(value, ref _details);
        }

        public virtual IProduct Product
        {
            get => _product;
            set => SetWithNotify(value, ref _product);
        }
    }

    public class Product : NotificationEntity, IProduct
    {
        private int _productId;
        private string _description;
        private string _baseConcurrency;
        private IDimensions _dimensions;
        private IConcurrencyInfo _complexConcurrency;
        private IAuditInfo _nestedComplexConcurrency;
        private ICollection<ISupplier> _suppliers;
        private IProductDetail _detail;
        private ICollection<IProductReview> _reviews;
        private ICollection<IProductPhoto> _photos;
        private ICollection<IBarcode> _barcodes;
        private ICollection<IDiscontinuedProduct> _replaces;

        public Product()
        {
            ComplexConcurrency = new ConcurrencyInfo();
            NestedComplexConcurrency = new AuditInfo();
        }

        public void InitializeCollections()
        {
            Suppliers ??= new ObservableCollection<ISupplier>();
            Replaces = new ObservableCollection<IDiscontinuedProduct>();
            Reviews ??= new ObservableCollection<IProductReview>();
            Photos ??= new ObservableCollection<IProductPhoto>();
            Barcodes ??= new ObservableCollection<IBarcode>();
        }

        public int ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public string Description
        {
            get => _description;
            set => SetWithNotify(value, ref _description);
        }

        public string BaseConcurrency
        {
            get => _baseConcurrency;
            set => SetWithNotify(value, ref _baseConcurrency);
        }

        public IDimensions Dimensions
        {
            get => _dimensions;
            set => SetWithNotify(value, ref _dimensions);
        }

        public IConcurrencyInfo ComplexConcurrency
        {
            get => _complexConcurrency;
            set => SetWithNotify(value, ref _complexConcurrency);
        }

        public IAuditInfo NestedComplexConcurrency
        {
            get => _nestedComplexConcurrency;
            set => SetWithNotify(value, ref _nestedComplexConcurrency);
        }

        public virtual ICollection<ISupplier> Suppliers
        {
            get => _suppliers;
            set => SetWithNotify(value, ref _suppliers);
        }

        public virtual ICollection<IDiscontinuedProduct> Replaces
        {
            get => _replaces;
            set => SetWithNotify(value, ref _replaces);
        }

        public virtual IProductDetail Detail
        {
            get => _detail;
            set => SetWithNotify(value, ref _detail);
        }

        public virtual ICollection<IProductReview> Reviews
        {
            get => _reviews;
            set => SetWithNotify(value, ref _reviews);
        }

        public virtual ICollection<IProductPhoto> Photos
        {
            get => _photos;
            set => SetWithNotify(value, ref _photos);
        }

        public virtual ICollection<IBarcode> Barcodes
        {
            get => _barcodes;
            set => SetWithNotify(value, ref _barcodes);
        }
    }

    public class ProductPageView : PageView, IProductPageView
    {
        private IProduct _product;
        private int _productId;

        public int ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public virtual IProduct Product
        {
            get => _product;
            set => SetWithNotify(value, ref _product);
        }
    }

    public class ProductPhoto : NotificationEntity, IProductPhoto
    {
        private int _productId;
        private int _photoId;
        private byte[] _photo;
        private ICollection<IProductWebFeature> _features;

        public void InitializeCollections()
            => Features ??= new ObservableCollection<IProductWebFeature>();

        public int ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public int PhotoId
        {
            get => _photoId;
            set => SetWithNotify(value, ref _photoId);
        }

        public byte[] Photo
        {
            get => _photo;
            set => SetWithNotify(value, ref _photo);
        }

        public virtual ICollection<IProductWebFeature> Features
        {
            get => _features;
            set => SetWithNotify(value, ref _features);
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
            => Features ??= new ObservableCollection<IProductWebFeature>();

        public int ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public int ReviewId
        {
            get => _reviewId;
            set => SetWithNotify(value, ref _reviewId);
        }

        public string Review
        {
            get => _review;
            set => SetWithNotify(value, ref _review);
        }

        public virtual IProduct Product
        {
            get => _product;
            set => SetWithNotify(value, ref _product);
        }

        public virtual ICollection<IProductWebFeature> Features
        {
            get => _features;
            set => SetWithNotify(value, ref _features);
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
            get => _featureId;
            set => SetWithNotify(value, ref _featureId);
        }

        public int? ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public int? PhotoId
        {
            get => _photoId;
            set => SetWithNotify(value, ref _photoId);
        }

        public int ReviewId
        {
            get => _reviewId;
            set => SetWithNotify(value, ref _reviewId);
        }

        public string Heading
        {
            get => _heading;
            set => SetWithNotify(value, ref _heading);
        }

        public virtual IProductReview Review
        {
            get => _review;
            set => SetWithNotify(value, ref _review);
        }

        public virtual IProductPhoto Photo
        {
            get => _photo;
            set => SetWithNotify(value, ref _photo);
        }
    }

    public class Resolution : NotificationEntity, IResolution
    {
        private int _resolutionId;
        private string _details;
        private IComplaint _complaint;

        public int ResolutionId
        {
            get => _resolutionId;
            set => SetWithNotify(value, ref _resolutionId);
        }

        public string Details
        {
            get => _details;
            set => SetWithNotify(value, ref _details);
        }

        public virtual IComplaint Complaint
        {
            get => _complaint;
            set => SetWithNotify(value, ref _complaint);
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
            get => _serial;
            set => SetWithNotify(value, ref _serial);
        }

        public DateTime Issued
        {
            get => _issued;
            set => SetWithNotify(value, ref _issued);
        }

        public string Username
        {
            get => _username;
            set => SetWithNotify(value, ref _username);
        }

        public virtual ILogin Login
        {
            get => _login;
            set => SetWithNotify(value, ref _login);
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
            get => _username;
            set => SetWithNotify(value, ref _username);
        }

        public string CardSerial
        {
            get => _cardSerial;
            set => SetWithNotify(value, ref _cardSerial);
        }

        public DateTime Issued
        {
            get => _issued;
            set => SetWithNotify(value, ref _issued);
        }

        public virtual ILogin Login
        {
            get => _login;
            set => SetWithNotify(value, ref _login);
        }

        public virtual ILastLogin LastLogin
        {
            get => _lastLogin;
            set => SetWithNotify(value, ref _lastLogin);
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
            get => _supplierInfoId;
            set => SetWithNotify(value, ref _supplierInfoId);
        }

        public string Information
        {
            get => _information;
            set => SetWithNotify(value, ref _information);
        }

        public int SupplierId
        {
            get => _supplierId;
            set => SetWithNotify(value, ref _supplierId);
        }

        public virtual ISupplier Supplier
        {
            get => _supplier;
            set => SetWithNotify(value, ref _supplier);
        }
    }

    public class SupplierLogo : NotificationEntity, ISupplierLogo
    {
        private int _supplierId;
        private byte[] _logo;

        public int SupplierId
        {
            get => _supplierId;
            set => SetWithNotify(value, ref _supplierId);
        }

        public byte[] Logo
        {
            get => _logo;
            set => SetWithNotify(value, ref _logo);
        }
    }

    public class Supplier : NotificationEntity, ISupplier
    {
        private int _supplierId;
        private string _name;
        private ICollection<IProduct> _products;
        private ISupplierLogo _logo;
        private ICollection<IBackOrderLine> _backOrderLines;

        public void InitializeCollections()
        {
            Products ??= new ObservableCollection<IProduct>();
            BackOrderLines = new ObservableCollection<IBackOrderLine>();
        }

        public int SupplierId
        {
            get => _supplierId;
            set => SetWithNotify(value, ref _supplierId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public virtual ICollection<IProduct> Products
        {
            get => _products;
            set => SetWithNotify(value, ref _products);
        }

        public virtual ICollection<IBackOrderLine> BackOrderLines
        {
            get => _backOrderLines;
            set => SetWithNotify(value, ref _backOrderLines);
        }

        public virtual ISupplierLogo Logo
        {
            get => _logo;
            set => SetWithNotify(value, ref _logo);
        }
    }

    public class SuspiciousActivity : NotificationEntity, ISuspiciousActivity
    {
        private int _suspiciousActivityId;
        private string _activity;
        private string _username;

        public int SuspiciousActivityId
        {
            get => _suspiciousActivityId;
            set => SetWithNotify(value, ref _suspiciousActivityId);
        }

        public string Activity
        {
            get => _activity;
            set => SetWithNotify(value, ref _activity);
        }

        public string Username
        {
            get => _username;
            set => SetWithNotify(value, ref _username);
        }
    }

    public class AuditInfo : NotificationEntity, IAuditInfo
    {
        private DateTime _modifiedDate;
        private string _modifiedBy;
        private IConcurrencyInfo _concurrency;

        public AuditInfo()
        {
            Concurrency = new ConcurrencyInfo();
            ModifiedDate = DateTime.Now;
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set => SetWithNotify(value, ref _modifiedDate);
        }

        public string ModifiedBy
        {
            get => _modifiedBy;
            set => SetWithNotify(value, ref _modifiedBy);
        }

        public IConcurrencyInfo Concurrency
        {
            get => _concurrency;
            set => SetWithNotify(value, ref _concurrency);
        }
    }

    public class Customer : NotificationEntity, ICustomer
    {
        private int _customerId;
        private int? _husbandId;
        private string _name;
        private IContactDetails _contactInfo;
        private IAuditInfo _auditing;
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
            Orders ??= new ObservableCollection<IAnOrder>();
            Logins ??= new ObservableCollection<ILogin>();
        }

        public int CustomerId
        {
            get => _customerId;
            set => SetWithNotify(value, ref _customerId);
        }

        public int? HusbandId
        {
            get => _husbandId;
            set => SetWithNotify(value, ref _husbandId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public IContactDetails ContactInfo
        {
            get => _contactInfo;
            set => SetWithNotify(value, ref _contactInfo);
        }

        public IAuditInfo Auditing
        {
            get => _auditing;
            set => SetWithNotify(value, ref _auditing);
        }

        public virtual ICollection<IAnOrder> Orders
        {
            get => _orders;
            set => SetWithNotify(value, ref _orders);
        }

        public virtual ICollection<ILogin> Logins
        {
            get => _logins;
            set => SetWithNotify(value, ref _logins);
        }

        public virtual ICustomer Husband
        {
            get => _husband;
            set => SetWithNotify(value, ref _husband);
        }

        public virtual ICustomer Wife
        {
            get => _wife;
            set => SetWithNotify(value, ref _wife);
        }

        public virtual ICustomerInfo Info
        {
            get => _info;
            set => SetWithNotify(value, ref _info);
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
            SentMessages ??= new ObservableCollection<IMessage>();
            ReceivedMessages ??= new ObservableCollection<IMessage>();
            Orders ??= new ObservableCollection<IAnOrder>();
        }

        public string Username
        {
            get => _username;
            set => SetWithNotify(value, ref _username);
        }

        public string AlternateUsername
        {
            get => _alternateUsername;
            set => SetWithNotify(value, ref _alternateUsername);
        }

        public int CustomerId
        {
            get => _customerId;
            set => SetWithNotify(value, ref _customerId);
        }

        public virtual ICustomer Customer
        {
            get => _customer;
            set => SetWithNotify(value, ref _customer);
        }

        public virtual ILastLogin LastLogin
        {
            get => _lastLogin;
            set => SetWithNotify(value, ref _lastLogin);
        }

        public virtual ICollection<IMessage> SentMessages
        {
            get => _sentMessages;
            set => SetWithNotify(value, ref _sentMessages);
        }

        public virtual ICollection<IMessage> ReceivedMessages
        {
            get => _receivedMessages;
            set => SetWithNotify(value, ref _receivedMessages);
        }

        public virtual ICollection<IAnOrder> Orders
        {
            get => _orders;
            set => SetWithNotify(value, ref _orders);
        }
    }

    public class Phone : NotificationEntity, IPhone
    {
        private PhoneType _phoneType;
        private string _extension;
        private string _phoneNumber;

        public Phone()
        {
            Extension = "None";
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetWithNotify(value, ref _phoneNumber);
        }

        public string Extension
        {
            get => _extension;
            set => SetWithNotify(value, ref _extension);
        }

        public PhoneType PhoneType
        {
            get => _phoneType;
            set => SetWithNotify(value, ref _phoneType);
        }
    }
}
