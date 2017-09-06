// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestModelCustomizer : IAdditionalModelCustomizer
    {
        private readonly Action<ModelBuilder, DbContext> _onModelCreating;

        public TestModelCustomizer(Action<ModelBuilder> onModelCreating)
            : this((mb, c) => onModelCreating(mb))
        {
        }

        public TestModelCustomizer(Action<ModelBuilder, DbContext> onModelCreating)
        {
            _onModelCreating = onModelCreating;
        }

        public void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            _onModelCreating(modelBuilder, context);
        }
    }
}
