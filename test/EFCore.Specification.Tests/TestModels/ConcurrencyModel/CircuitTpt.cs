// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public abstract class CircuitTpt
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ulong ULongVersion { get; init; }

    [NotMapped]
    public List<byte> BinaryVersion { get; init; }
}

public class StreetCircuitTpt : CircuitTpt, IStreetCircuit<CityTpt>
{
    public class StreetCircuitTptProxy : StreetCircuitTpt, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public int Length { get; set; }

    [Required]
    public CityTpt City { get; set; }
}

public class OvalCircuitTpt : CircuitTpt
{
    public class OvalCircuitTptProxy : OvalCircuitTpt, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public double Banking { get; set; }
}
