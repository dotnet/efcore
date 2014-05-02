// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.Data.SQLite.Interop
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
