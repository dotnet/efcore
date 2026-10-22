// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class GearsController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Gear> Get()
        => _context.Gears;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Officer> GetFromOfficer()
        => _context.Gears.OfType<Officer>();

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

public class SquadsController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Squad> Get()
        => _context.Squads;

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

public class TagsController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<CogTag> Get()
        => _context.Tags;

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

public class WeaponsController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Weapon> Get()
        => _context.Weapons;

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

public class CitiesController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<City> Get()
        => _context.Cities;

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

public class MissionsController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Mission> Get()
        => _context.Missions;

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

public class SquadMissionsController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<SquadMission> Get()
        => _context.SquadMissions;

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

public class FactionsController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Faction> Get()
        => _context.Factions;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<LocustHorde> GetFromLocustHorde()
        => _context.Factions.OfType<LocustHorde>();

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

public class LocustLeadersController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<LocustLeader> Get()
        => _context.LocustLeaders;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<LocustCommander> GetFromLocustCommander()
        => _context.LocustLeaders.OfType<LocustCommander>();

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

public class LocustHighCommandsController(GearsOfWarODataContext context) : TestODataController, IDisposable
{
    private readonly GearsOfWarODataContext _context = context;

    [HttpGet]
    [EnableQuery]
    public IEnumerable<LocustHighCommand> Get()
        => _context.LocustHighCommands;

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
