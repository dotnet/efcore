// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class DocumentDbTestHelpers : TestHelpers
    {
        protected DocumentDbTestHelpers()
        {
        }

        public static DocumentDbTestHelpers Instance { get; } = new DocumentDbTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkDocumentDb();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseDocumentDb(
                new Uri("https://localhost"),
                string.Empty,
                nameof(DocumentDbTestHelpers));
    }
}
