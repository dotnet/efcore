// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public GearsController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Gear> Get()
        {
            return _context.Gears;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Officer> GetFromOfficer()
        {
            return _context.Gears.OfType<Officer>();
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] string keyNickname, [FromODataUri] int keySquadId)
        {
            var result = _context.Gears.FirstOrDefault(e => e.Nickname == keyNickname && e.SquadId == keySquadId);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class SquadsController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public SquadsController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Squad> Get()
        {
            return _context.Squads;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.Squads.FirstOrDefault(e => e.Id == key);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class TagsController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public TagsController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<CogTag> Get()
        {
            return _context.Tags;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] Guid key)
        {
            var result = _context.Tags.FirstOrDefault(e => e.Id == key);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class WeaponsController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public WeaponsController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Weapon> Get()
        {
            return _context.Weapons;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.Weapons.FirstOrDefault(e => e.Id == key);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class CitiesController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public CitiesController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<City> Get()
        {
            return _context.Cities;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] string key)
        {
            var result = _context.Cities.FirstOrDefault(e => e.Name == key);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class MissionsController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public MissionsController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Mission> Get()
        {
            return _context.Missions;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.Missions.FirstOrDefault(e => e.Id == key);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class SquadMissionsController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public SquadMissionsController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<SquadMission> Get()
        {
            return _context.SquadMissions;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int keySquadId, [FromODataUri] int keyMissionId)
        {
            var result = _context.SquadMissions.FirstOrDefault(e => e.SquadId == keySquadId && e.MissionId == keyMissionId);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class FactionsController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public FactionsController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Faction> Get()
        {
            return _context.Factions;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<LocustHorde> GetFromLocustHorde()
        {
            return _context.Factions.OfType<LocustHorde>();
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.Factions.FirstOrDefault(e => e.Id == key);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class LocustLeadersController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public LocustLeadersController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<LocustLeader> Get()
        {
            return _context.LocustLeaders;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<LocustCommander> GetFromLocustCommander()
        {
            return _context.LocustLeaders.OfType<LocustCommander>();
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] string key)
        {
            var result = _context.LocustLeaders.FirstOrDefault(e => e.Name == key);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }

    public class LocustHighCommandsController : TestODataController, IDisposable
    {
        private readonly GearsOfWarODataContext _context;

        public LocustHighCommandsController(GearsOfWarODataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<LocustHighCommand> Get()
        {
            return _context.LocustHighCommands;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var result = _context.LocustHighCommands.FirstOrDefault(e => e.Id == key);

            return result == null ? NotFound() : Ok(result);
        }

        public void Dispose()
        {
        }
    }
}
