// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ExecuteMergeModel;

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Note { get; set; }
}

public class BlogRow
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Note { get; set; }
}

public class BlogWithoutName
{
    public int Id { get; set; }
    public string? Note { get; set; }
}

public class BlogImport
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
}

public class DailyVisit
{
    public int UserId { get; set; }
    public string Day { get; set; } = null!;
    public int Visits { get; set; }
}

public class Account
{
    public int Id { get; set; }
    public int ExternalId { get; set; }
    public int Balance { get; set; }
    public AccountDetails Details { get; set; } = new();
}

public class AccountDetails
{
    public int Balance { get; set; }
}

public class Gadget
{
    public int Id { get; set; }
    public int Price { get; set; }
    public int Tax { get; set; }
}

public class Keyless
{
    public int Value { get; set; }
}

public class Measurement
{
    public int Id { get; set; }
    public int Value { get; set; }
    public int ConvertedValue { get; set; }
}
