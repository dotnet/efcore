// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel
{
    public class LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
    {
        public int ProfileId { get; set; }
        public string ProfileId1 { get; set; }
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
        public int ExtraProperty { get; set; }

        public virtual Profile Profile { get; set; }
    }
}
