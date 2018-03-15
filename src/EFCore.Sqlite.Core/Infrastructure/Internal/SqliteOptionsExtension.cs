// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteOptionsExtension : RelationalOptionsExtension
    {
        private bool _enforceForeignKeys = true;
        private string _logFragment;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteOptionsExtension()
        {
        }

        // NB: When adding new options, make sure to update the copy ctor below.

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected SqliteOptionsExtension([NotNull] SqliteOptionsExtension copyFrom)
            : base(copyFrom)
        {
            _enforceForeignKeys = copyFrom._enforceForeignKeys;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalOptionsExtension Clone()
            => new SqliteOptionsExtension(this);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool EnforceForeignKeys => _enforceForeignKeys;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual SqliteOptionsExtension WithEnforceForeignKeys(bool enforceForeignKeys)
        {
            var clone = (SqliteOptionsExtension)Clone();

            clone._enforceForeignKeys = enforceForeignKeys;

            return clone;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool ApplyServices(IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            services.AddEntityFrameworkSqlite();

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append(base.LogFragment);

                    if (!_enforceForeignKeys)
                    {
                        builder.Append("SuppressForeignKeyEnforcement ");
                    }

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }
    }
}
