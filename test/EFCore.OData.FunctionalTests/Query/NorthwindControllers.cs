// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class CustomersController(NorthwindODataContext context) : TestODataController, IDisposable
{
    private readonly NorthwindODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Customer> Get()
        => _context.Customers;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] string key)
    {
        var result = _context.Customers.FirstOrDefault(g => g.CustomerID == key);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}

public class OrdersController(NorthwindODataContext context) : TestODataController, IDisposable
{
    private readonly NorthwindODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Order> Get()
        => _context.Orders;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] int key)
    {
        var result = _context.Orders.FirstOrDefault(e => e.OrderID == key);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}

public class OrderDetailsController(NorthwindODataContext context) : TestODataController, IDisposable
{
    private readonly NorthwindODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<OrderDetail> Get()
        => _context.OrderDetails;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] int keyOrderId, [FromODataUri] int keyProductId)
    {
        var result = _context.OrderDetails.FirstOrDefault(e => e.OrderID == keyOrderId && e.ProductID == keyProductId);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}

public class EmployeesController(NorthwindODataContext context) : TestODataController, IDisposable
{
    private readonly NorthwindODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Employee> Get()
        => _context.Employees;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] uint key)
    {
        var result = _context.Employees.FirstOrDefault(e => e.EmployeeID == key);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}

public class ProductsController(NorthwindODataContext context) : TestODataController, IDisposable
{
    private readonly NorthwindODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Product> Get()
        => _context.Products;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] uint key)
    {
        var result = _context.Products.FirstOrDefault(e => e.ProductID == key);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}
