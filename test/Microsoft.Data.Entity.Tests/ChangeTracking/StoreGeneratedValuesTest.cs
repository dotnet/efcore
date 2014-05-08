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

using Microsoft.Data.Entity.ChangeTracking;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class StoreGeneratedValuesTest : SidecarTest
    {
        [Fact]
        public void Is_set_up_for_transparent_access_and_auto_commit()
        {
            var sidecar = CreateSidecar();

            Assert.True(sidecar.TransparentRead);
            Assert.True(sidecar.TransparentWrite);
            Assert.True(sidecar.AutoCommit);
        }

        [Fact]
        public void Has_expected_name()
        {
            Assert.Equal(Sidecar.WellKnownNames.StoreGeneratedValues, CreateSidecar().Name);
        }

        protected override Sidecar CreateSidecar(StateEntry entry = null)
        {
            return new StoreGeneratedValuesFactory().Create(entry ?? CreateStateEntry(), new[] { IdProperty, StateProperty });
        }
    }
}
