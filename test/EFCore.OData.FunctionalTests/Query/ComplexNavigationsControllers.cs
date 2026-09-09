// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class LevelOneController(ComplexNavigationsODataContext context) : TestODataController, IDisposable
{
    private readonly ComplexNavigationsODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Level1> Get()
        => _context.LevelOne;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] int key)
    {
        var result = _context.LevelOne.FirstOrDefault(e => e.Id == key);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}

public class LevelTwoController(ComplexNavigationsODataContext context) : TestODataController, IDisposable
{
    private readonly ComplexNavigationsODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Level2> Get()
        => _context.LevelTwo;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] int key)
    {
        var result = _context.LevelTwo.FirstOrDefault(e => e.Id == key);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}

public class LevelThreeController(ComplexNavigationsODataContext context) : TestODataController, IDisposable
{
    private readonly ComplexNavigationsODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Level3> Get()
        => _context.LevelThree;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] int key)
    {
        var result = _context.LevelThree.FirstOrDefault(e => e.Id == key);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}

public class LevelFourController(ComplexNavigationsODataContext context) : TestODataController, IDisposable
{
    private readonly ComplexNavigationsODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Level4> Get()
        => _context.LevelFour;

    [HttpGet]
    [EnableQuery]
    public ITestActionResult Get([FromODataUri] int key)
    {
        var result = _context.LevelFour.FirstOrDefault(e => e.Id == key);

        return result == null ? NotFound() : Ok(result);
    }

    public void Dispose()
    {
    }
}
