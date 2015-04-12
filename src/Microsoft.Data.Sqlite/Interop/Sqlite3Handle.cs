// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Sqlite.Interop
{
    internal class Sqlite3Handle : SafeHandle
    {
        private Sqlite3Handle()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            var rc = NativeMethods.sqlite3_close_v2(handle);
            handle = IntPtr.Zero;

            return rc == Constants.SQLITE_OK;
        }
    }
}
