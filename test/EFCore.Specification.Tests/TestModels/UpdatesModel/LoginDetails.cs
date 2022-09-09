// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class
    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
{
    public int ProfileId { get; set; }
    public string? ProfileId1 { get; set; }
    public Guid ProfileId2 { get; set; }
    public decimal ProfileId3 { get; set; }
    public bool ProfileId4 { get; set; }
    public byte ProfileId5 { get; set; }
    public short ProfileId6 { get; set; }
    public long ProfileId7 { get; set; }
    public DateTime ProfileId8 { get; set; }
    public DateTimeOffset ProfileId9 { get; set; }
    public TimeSpan ProfileId10 { get; set; }
    public byte? ProfileId11 { get; set; }
    public short? ProfileId12 { get; set; }
    public int? ProfileId13 { get; set; }
    public long? ProfileId14 { get; set; }

    public int
        ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
    {
        get;
        set;
    }

    public string?
        ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyWhenTruncatedNamesCollide
    {
        get;
        set;
    }

    public virtual
        LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
        ?
        Login { get; set; }
}
