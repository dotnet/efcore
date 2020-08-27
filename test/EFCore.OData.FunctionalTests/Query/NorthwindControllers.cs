// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CustomersController : TestODataController, IDisposable
    {
        private readonly NorthwindODataContext _context;

        public CustomersController(NorthwindODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Customer> Get()
        {
            return _context.Customers;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] string key)
        {
            var result = _context.Customers.FirstOrDefault(g => g.CustomerID == key);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class OrdersController : TestODataController, IDisposable
    {
        private readonly NorthwindODataContext _context;

        public OrdersController(NorthwindODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Order> Get()
        {
            return _context.Orders;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.Orders.FirstOrDefault(e => e.OrderID == key);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class OrderDetailsController : TestODataController, IDisposable
    {
        private readonly NorthwindODataContext _context;

        public OrderDetailsController(NorthwindODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<OrderDetail> Get()
        {
            return _context.OrderDetails;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int keyOrderId, [FromODataUri] int keyProductId)
        {
            var result = _context.OrderDetails.FirstOrDefault(e => e.OrderID == keyOrderId && e.ProductID == keyProductId);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class EmployeesController : TestODataController, IDisposable
    {
        private readonly NorthwindODataContext _context;

        public EmployeesController(NorthwindODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Employee> Get()
        {
            return _context.Employees;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] uint key)
        {
            var result = _context.Employees.FirstOrDefault(e => e.EmployeeID == key);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class ProductsController : TestODataController, IDisposable
    {
        private readonly NorthwindODataContext _context;

        public ProductsController(NorthwindODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Product> Get()
        {
            return _context.Products;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] uint key)
        {
            var result = _context.Products.FirstOrDefault(e => e.ProductID == key);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        public void Dispose()
        {
        }
    }
}
