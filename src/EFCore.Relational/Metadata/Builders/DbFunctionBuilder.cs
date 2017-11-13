// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a <see cref="DbFunction" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class DbFunctionBuilder
    {
        private readonly InternalDbFunctionBuilder _builder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionBuilder([NotNull] DbFunction function)
        {
            Check.NotNull(function, nameof(function));

            _builder = new InternalDbFunctionBuilder(function);
        }

        /// <summary>
        ///     Metadata representing the function being configured.
        /// </summary>
        public virtual IMutableDbFunction Metadata => _builder.Metadata;

        /// <summary>
        ///     Sets the name of the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasName([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            _builder.HasName(name, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the schema of the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasSchema([CanBeNull] string schema)
        {
            _builder.HasSchema(schema, ConfigurationSource.Explicit);

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
        public virtual DbFunctionBuilder HasTranslation([NotNull] Func<IReadOnlyCollection<Expression>, Expression> translation)
        {
            Check.NotNull(translation, nameof(translation));

            _builder.HasTranslation(translation);

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
