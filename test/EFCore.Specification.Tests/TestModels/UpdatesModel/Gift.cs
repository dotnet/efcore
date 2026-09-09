// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Gift
{
    public int Id { get; set; }
    public string? Recipient { get; set; }

    public GiftObscurer? Obscurer { get; set; }
}

public abstract class GiftObscurer
{
    public int Id { get; set; }
    public string? Pattern { get; set; }
}

public class GiftBag : GiftObscurer
{
    public int Size { get; set; }
}

public class GiftPaper : GiftObscurer
{
    public int Thickness { get; set; }
}
