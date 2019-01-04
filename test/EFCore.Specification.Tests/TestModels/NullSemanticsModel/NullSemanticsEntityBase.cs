// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel
{
    public abstract class NullSemanticsEntityBase
    {
        public int Id { get; set; }
        public bool BoolA { get; set; }
        public bool BoolB { get; set; }
        public bool BoolC { get; set; }

        public bool? NullableBoolA { get; set; }
        public bool? NullableBoolB { get; set; }
        public bool? NullableBoolC { get; set; }

        public string StringA { get; set; }
        public string StringB { get; set; }
        public string StringC { get; set; }

        public string NullableStringA { get; set; }
        public string NullableStringB { get; set; }
        public string NullableStringC { get; set; }

        public int IntA { get; set; }
        public int IntB { get; set; }
        public int IntC { get; set; }

        public int? NullableIntA { get; set; }
        public int? NullableIntB { get; set; }
        public int? NullableIntC { get; set; }
    }
}
