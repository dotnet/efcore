// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal
{
    public class InMemoryOptionsExtension : IDbContextOptionsExtension
    {
        private string _storeName;

        public InMemoryOptionsExtension()
        {
        }

        public InMemoryOptionsExtension([NotNull] InMemoryOptionsExtension copyFrom)
        {
            _storeName = copyFrom._storeName;
        }

        public virtual string StoreName
        {
            get { return _storeName; }
            [param: NotNull] set { _storeName = value; }
        }

        public virtual void ApplyServices(IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            services.AddEntityFrameworkInMemoryDatabase();
        }
    }
}
