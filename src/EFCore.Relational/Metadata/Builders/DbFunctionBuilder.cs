// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IMutableDbFunction" />.
    /// </summary>
    public class DbFunctionBuilder : DbFunctionBuilderBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public DbFunctionBuilder([NotNull] IMutableDbFunction function)
            : base(function)
        {
        }

        /// <summary>
        ///     Sets the name of the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual DbFunctionBuilder HasName([NotNull] string name)
            => (DbFunctionBuilder)base.HasName(name);

        /// <summary>
        ///     Sets the schema of the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual DbFunctionBuilder HasSchema([CanBeNull] string schema)
            => (DbFunctionBuilder)base.HasSchema(schema);

        /// <summary>
        ///     Marks whether the database function is built-in.
        /// </summary>
        /// <param name="builtIn"> The value indicating whether the database function is built-in. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual DbFunctionBuilder IsBuiltIn(bool builtIn = true)
            => (DbFunctionBuilder)base.IsBuiltIn(builtIn);

        /// <summary>
        ///     Marks whether the database function can return null value.
        /// </summary>
        /// <param name="nullable"> The value indicating whether the database function can return null. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilderBase IsNullable(bool nullable = true)
        {
            Builder.IsNullable(nullable, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the return store type of the database function.
        /// </summary>
        /// <param name="storeType"> The return store type of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasStoreType([CanBeNull] string storeType)
        {
            Builder.HasStoreType(storeType, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Sets a callback that will be invoked to perform custom translation of this
        ///         function. The callback takes a collection of expressions corresponding to
        ///         the parameters passed to the function call. The callback should return an
        ///         expression representing the desired translation.
        ///     </para>
        ///     <para>
        ///         See https://go.microsoft.com/fwlink/?linkid=852477 for more information.
        ///     </para>
        /// </summary>
        /// <param name="translation"> The translation to use. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasTranslation([NotNull] Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation)
        {
            Builder.HasTranslation(translation, ConfigurationSource.Explicit);

            return this;
        }
    }
}
