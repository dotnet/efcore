// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal class WrappedXunitException : XunitException
    {
        public WrappedXunitException(WrappedException ex)
            : base(ex.ToString(), "(See error message)")
        {
        }
    }
}