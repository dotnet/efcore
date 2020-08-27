// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class LevelOneController : TestODataController, IDisposable
    {
        private readonly ComplexNavigationsODataContext _context;

        public LevelOneController(ComplexNavigationsODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Level1> Get()
        {
            return _context.LevelOne;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.LevelOne.FirstOrDefault(e => e.Id == key);
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

    public class LevelTwoController : TestODataController, IDisposable
    {
        private readonly ComplexNavigationsODataContext _context;

        public LevelTwoController(ComplexNavigationsODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Level2> Get()
        {
            return _context.LevelTwo;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.LevelTwo.FirstOrDefault(e => e.Id == key);
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

    public class LevelThreeController : TestODataController, IDisposable
    {
        private readonly ComplexNavigationsODataContext _context;

        public LevelThreeController(ComplexNavigationsODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Level3> Get()
        {
            return _context.LevelThree;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.LevelThree.FirstOrDefault(e => e.Id == key);
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

    public class LevelFourController : TestODataController, IDisposable
    {
        private readonly ComplexNavigationsODataContext _context;

        public LevelFourController(ComplexNavigationsODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Level4> Get()
        {
            return _context.LevelFour;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.LevelFour.FirstOrDefault(e => e.Id == key);
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
