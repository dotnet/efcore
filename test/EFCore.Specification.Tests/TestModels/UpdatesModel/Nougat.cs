// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public abstract class Nougat
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class CrunchyNougat : Nougat
{
    public NougatFilling? Filling { get; set; }
}

public class SoftNougat : Nougat
{
    public NougatFilling? Filling { get; set; }
}

public class NougatFilling
{
    public NougatFillingKind Kind { get; set; }
    public bool IsFresh { get; set; }
}

public enum NougatFillingKind
{
    Unknown = 0,
    Peanut = 1,
    Almond = 2,
}
