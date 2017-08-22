// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal
{
    public class OracleOptionsExtension : RelationalOptionsExtension
    {
        public OracleOptionsExtension()
        {
        }

        // NB: When adding new options, make sure to update the copy ctor below.

        protected OracleOptionsExtension([NotNull] OracleOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        protected override RelationalOptionsExtension Clone()
            => new OracleOptionsExtension(this);

        public virtual OracleOptionsExtension WithRowNumberPaging(bool rowNumberPaging)
        {
            var clone = (OracleOptionsExtension)Clone();

            return clone;
        }

        public override bool ApplyServices(IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            services.AddEntityFrameworkOracle();

            return true;
        }
    }
}
