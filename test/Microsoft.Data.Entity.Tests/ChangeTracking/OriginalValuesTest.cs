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
using Microsoft.Data.Entity.ChangeTracking;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class OriginalValuesTest : SidecarTest
    {
        [Fact]
        public void Is_not_set_up_for_transparent_access_and_auto_commit()
        {
            var sidecar = CreateSidecar();

            Assert.False(sidecar.TransparentRead);
            Assert.False(sidecar.TransparentWrite);
            Assert.False(sidecar.AutoCommit);
        }

        [Fact]
        public void Has_expected_name()
        {
            Assert.Equal(Sidecar.WellKnownNames.OriginalValues, CreateSidecar().Name);
        }

        [Fact]
        public void Throws_on_attempt_to_read_when_original_value_cannot_be_stored()
        {
            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Name", "Banana"),
                Assert.Throws<InvalidOperationException>(() => CreateSidecar()[NameProperty]).Message);
        }

        [Fact]
        public void Throws_on_attempt_to_write_when_original_value_cannot_be_stored()
        {
            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Name", "Banana"),
                Assert.Throws<InvalidOperationException>(() => CreateSidecar()[NameProperty] = "Yellow").Message);
        }

        protected override Sidecar CreateSidecar(StateEntry entry = null)
        {
            return new OriginalValuesFactory().Create(entry ?? CreateStateEntry());
        }
    }
}
