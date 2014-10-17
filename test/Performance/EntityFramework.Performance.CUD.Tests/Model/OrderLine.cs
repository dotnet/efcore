// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Cud.Model
{
    public class OrderLine
    {
        #region Primitive Properties

        public virtual int Id { get; set; }

        public virtual int OrderId
        {
            get { return _orderId; }
            set
            {
                if (_orderId != value)
                {
                    if (Order != null
                        && Order.Id != value)
                    {
                        Order = null;
                    }
                    _orderId = value;
                }
            }
        }

        private int _orderId;

        public virtual int ProductId
        {
            get { return _productId; }
            set
            {
                if (_productId != value)
                {
                    if (Product != null
                        && Product.Id != value)
                    {
                        Product = null;
                    }
                    _productId = value;
                }
            }
        }

        private int _productId;

        public virtual int Quantity { get; set; }

        public virtual decimal Price { get; set; }

        #endregion

        #region Navigation Properties

        public virtual Order Order
        {
            get { return _order; }
            set
            {
                if (!ReferenceEquals(_order, value))
                {
                    var previousValue = _order;
                    _order = value;
                    FixupOrder(previousValue);
                }
            }
        }

        private Order _order;

        public virtual Product Product
        {
            get { return _product; }
            set
            {
                if (!ReferenceEquals(_product, value))
                {
                    var previousValue = _product;
                    _product = value;
                    FixupProduct(previousValue);
                }
            }
        }

        private Product _product;

        #endregion

        #region Association Fixup

        private void FixupOrder(Order previousValue)
        {
            if (previousValue != null
                && previousValue.OrderLines.Contains(this))
            {
                previousValue.OrderLines.Remove(this);
            }

            if (Order != null)
            {
                if (!Order.OrderLines.Contains(this))
                {
                    Order.OrderLines.Add(this);
                }
                if (OrderId != Order.Id)
                {
                    OrderId = Order.Id;
                }
            }
        }

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null
                && previousValue.OrderLines.Contains(this))
            {
                previousValue.OrderLines.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.OrderLines.Contains(this))
                {
                    Product.OrderLines.Add(this);
                }
                if (ProductId != Product.Id)
                {
                    ProductId = Product.Id;
                }
            }
        }

        #endregion
    }
}
