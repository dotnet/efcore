// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class ConvertToDoubleTranslator : ConvertTranslator
    {
        public ConvertToDoubleTranslator()
            : base(nameof(Convert.ToDouble))
        {
        }
    }
}
