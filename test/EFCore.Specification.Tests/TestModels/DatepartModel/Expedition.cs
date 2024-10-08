// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.DatepartModel;

public class Expedition
{
    public int Id { get; set; }
    public string? Destination { get; set; }
    public DateTime StartDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public TimeSpan Duration { get; set; }
}
