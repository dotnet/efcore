// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;

namespace EntityFramework.Microbenchmarks.CudPerf.Model
{
    public class Product
    {
        #region Primitive Properties

        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        #endregion

        #region Navigation Properties

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

        private void FixupOrderLines(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (OrderLine item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (OrderLine item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        #endregion
    }
}
