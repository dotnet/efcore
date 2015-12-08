// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

using static Microsoft.Data.Sqlite.Interop.Constants;

namespace Microsoft.Data.Sqlite.Interop
{
    internal class Sqlite3StmtHandle : SafeHandle
    {
        private Sqlite3StmtHandle()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            var rc = NativeMethods.sqlite3_finalize(handle);
            handle = IntPtr.Zero;

            return rc == SQLITE_OK;
        }
    }
}
