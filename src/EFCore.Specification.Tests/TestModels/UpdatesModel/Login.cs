// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel
{
    public class LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
    {
        public int LoginId { get; set; }
        public string LoginId1 { get; set; }
        public Guid LoginId2 { get; set; }
        public decimal LoginId3 { get; set; }
        public bool LoginId4 { get; set; }
        public byte LoginId5 { get; set; }
        public short LoginId6 { get; set; }
        public long LoginId7 { get; set; }
        public DateTime LoginId8 { get; set; }
        public DateTimeOffset LoginId9 { get; set; }
        public TimeSpan LoginId10 { get; set; }
        public byte? LoginId11 { get; set; }
        public short? LoginId12 { get; set; }
        public int? LoginId13 { get; set; }
        public long? LoginId14 { get; set; }

        public virtual Profile Profile { get; set; }
    }
}
