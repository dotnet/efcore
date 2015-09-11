// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class SqlServerOptionsExtension : RelationalOptionsExtension
    {
        public SqlServerOptionsExtension()
        {
        }

        public SqlServerOptionsExtension([NotNull] SqlServerOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        public virtual bool? RowNumberPaging { get; set; }

        public override void ApplyServices(EntityFrameworkServicesBuilder builder)
            => Check.NotNull(builder, nameof(builder)).AddSqlServer();
    }
}
