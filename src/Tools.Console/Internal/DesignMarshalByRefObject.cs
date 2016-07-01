// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class DesignMarshalByRefObject
#if NET451
        : MarshalByRefObject
#endif
    {
    }
}
