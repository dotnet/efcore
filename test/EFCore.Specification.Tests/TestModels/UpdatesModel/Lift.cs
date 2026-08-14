// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Lift
{
    public int Id { get; set; }
    public string? Recipient { get; set; }

    public LiftObscurer Obscurer { get; set; } = null!;
}

public abstract class LiftObscurer
{
    public int Id { get; set; }

    public int LiftId { get; set; }
    public string? Pattern { get; set; }
}

public class LiftBag : LiftObscurer
{
    public int Size { get; set; }
}

public class LiftPaper : LiftObscurer
{
    public int Thickness { get; set; }
}
