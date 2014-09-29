using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Cud.Model
{
    public partial class Order
    {
        #region Primitive Properties

        public virtual int Id { get; set; }

        public virtual int CustomerId
        {
            get { return _customerId; }
            set
            {
                if (_customerId != value)
                {
                    if (Customer != null && Customer.Id != value)
                    {
                        Customer = null;
                    }
                    _customerId = value;
                }
            }
        }

        private int _customerId;

        public virtual DateTime Date { get; set; }

        #endregion

        #region Navigation Properties

        public virtual Customer Customer
        {
            get { return _customer; }
            set
            {
                if (!ReferenceEquals(_customer, value))
                {
                    var previousValue = _customer;
                    _customer = value;
                    FixupCustomer(previousValue);
                }
            }
        }

        private Customer _customer;

        public virtual ICollection<OrderLine> OrderLines
        {
            get
            {
                if (_orderLines == null)
                {
                    var newCollection = new FixupCollection<OrderLine>();
                    newCollection.CollectionChanged += FixupOrderLines;
                    _orderLines = newCollection;
                }
                return _orderLines;
            }
            set
            {
                if (!ReferenceEquals(_orderLines, value))
                {
                    var previousValue = _orderLines as FixupCollection<OrderLine>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupOrderLines;
                    }
                    _orderLines = value;
                    var newValue = value as FixupCollection<OrderLine>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupOrderLines;
                    }
                }
            }
        }

        private ICollection<OrderLine> _orderLines;

        #endregion

        #region Association Fixup

        private void FixupCustomer(Customer previousValue)
        {
            if (previousValue != null && previousValue.Orders.Contains(this))
            {
                previousValue.Orders.Remove(this);
            }

            if (Customer != null)
            {
                if (!Customer.Orders.Contains(this))
                {
                    Customer.Orders.Add(this);
                }
                if (CustomerId != Customer.Id)
                {
                    CustomerId = Customer.Id;
                }
            }
        }

        private void FixupOrderLines(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (OrderLine item in e.NewItems)
                {
                    item.Order = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (OrderLine item in e.OldItems)
                {
                    if (ReferenceEquals(item.Order, this))
                    {
                        item.Order = null;
                    }
                }
            }
        }

        #endregion
    }
}