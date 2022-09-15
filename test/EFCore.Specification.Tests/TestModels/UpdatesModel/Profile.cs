// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Profile
{
    public int Id { get; set; }
    public string? Id1 { get; set; }
    public Guid Id2 { get; set; }
    public decimal Id3 { get; set; }
    public bool Id4 { get; set; }
    public byte Id5 { get; set; }
    public short Id6 { get; set; }
    public long Id7 { get; set; }
    public DateTime Id8 { get; set; }
    public DateTimeOffset Id9 { get; set; }
    public TimeSpan Id10 { get; set; }
    public byte? Id11 { get; set; }
    public short? Id12 { get; set; }
    public int? Id13 { get; set; }
    public long? Id14 { get; set; }

    public virtual
        LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
        ?
        User { get; set; }
}
