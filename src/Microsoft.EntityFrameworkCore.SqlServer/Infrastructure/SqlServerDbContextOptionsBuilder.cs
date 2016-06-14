// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows SQL Server specific configuration to be performed on <see cref="DbContextOptions"/>.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from a call to 
    ///         <see cref="SqlServerDbContextOptionsExtensions.UseSqlServer(DbContextOptionsBuilder, string, System.Action{SqlServerDbContextOptionsBuilder})"/>
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class SqlServerDbContextOptionsBuilder
        : RelationalDbContextOptionsBuilder<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlServerDbContextOptionsBuilder"/> class.
        /// </summary>
        /// <param name="optionsBuilder"> The options builder. </param>
        public SqlServerDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        /// <summary>
        ///     Clones the configuration in this builder.
        /// </summary>
        /// <returns> The cloned configuration. </returns>
        protected override SqlServerOptionsExtension CloneExtension()
            => new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>());

        /// <summary>
        ///     Use a ROW_NUMBER() in queries instead of OFFSET/FETCH. This method is backwards-compatible to SQL Server 2005.
        /// </summary>
        public virtual void UseRowNumberForPaging() => SetOption(e => e.RowNumberPaging = true);
    }
}
