// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public interface ICity
{
    public string Name { get; set; }
}

public class City : ICity
{
    public class CityProxy : City, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
}

public class CityTpt : ICity
{
    public class CityTptProxy : CityTpt, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
}

public class CityTpc : ICity
{
    public class CityTpcProxy : CityTpc, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
}
