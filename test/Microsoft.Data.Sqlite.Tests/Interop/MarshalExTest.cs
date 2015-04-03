// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.Data.Sqlite.Interop
{
    public class MarshalExTest
    {
        [Fact]
        public void GetExceptionForRC_returns_null_when_ok()
        {
            Assert.Null(MarshalEx.GetExceptionForRC(Constants.SQLITE_OK));
        }

        [Fact]
        public void PtrToStringUTF8_returns_null_when_zero()
        {
            Assert.Null(MarshalEx.PtrToStringUTF8(IntPtr.Zero));
        }

        [Fact]
        public void StringToHGlobalUTF8_returns_zero_when_null()
        {
            int size;
            var ptr = MarshalEx.StringToHGlobalUTF8(null, out size);

            Assert.Equal(IntPtr.Zero, ptr);
            Assert.Equal(0, size);
        }

        [Fact]
        public void StringToHGlobalUTF8_and_PtrToStringUTF8_roundtrip()
        {
            var s = "text";
            int size;
            var ptr = MarshalEx.StringToHGlobalUTF8(s, out size);
            try
            {
                var result = MarshalEx.PtrToStringUTF8(ptr);

                Assert.Equal(s, result);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        [Fact]
        public void ThrowExceptionForRC_is_noop_when_ok()
        {
            MarshalEx.ThrowExceptionForRC(Constants.SQLITE_OK);
        }
    }
}
