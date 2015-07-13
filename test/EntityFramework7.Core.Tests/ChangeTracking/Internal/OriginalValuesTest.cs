// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
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
                Strings.OriginalValueNotTracked("Name", typeof(Banana).FullName),
                Assert.Throws<InvalidOperationException>(() => CreateSidecar()[NameProperty]).Message);
        }

        [Fact]
        public void Throws_on_attempt_to_write_when_original_value_cannot_be_stored()
        {
            Assert.Equal(
                Strings.OriginalValueNotTracked("Name", typeof(Banana).FullName),
                Assert.Throws<InvalidOperationException>(() => CreateSidecar()[NameProperty] = "Yellow").Message);
        }

        protected override Sidecar CreateSidecar(InternalEntityEntry entry = null)
        {
            return new OriginalValuesFactory().Create(entry ?? CreateInternalEntry());
        }
    }
}
