// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Internal
{
    public class SqlServerOptionsExtension : RelationalOptionsExtension
    {
        private bool? _rowNumberPaging;

        public SqlServerOptionsExtension()
        {
        }

        // NB: When adding new options, make sure to update the copy ctor below.

        public SqlServerOptionsExtension([NotNull] SqlServerOptionsExtension copyFrom)
            : base(copyFrom)
        {
            _rowNumberPaging = copyFrom._rowNumberPaging;
        }

        public virtual bool? RowNumberPaging
        {
            get { return _rowNumberPaging; }
            set { _rowNumberPaging = value; }
        }

        public override void ApplyServices(EntityFrameworkServicesBuilder builder)
            => Check.NotNull(builder, nameof(builder)).AddSqlServer();
    }
}
