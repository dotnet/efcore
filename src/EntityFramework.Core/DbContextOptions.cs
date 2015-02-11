// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    /// <summary>
    ///     <para>
    ///         Represents the options for a <see cref="DbContext" /> instance (such as the data store to be targeted). The
    ///         <see cref="DbContextOptions" /> for a context can be configured by overriding
    ///         <see cref="DbContext.OnConfiguring(DbContextOptions)" />
    ///         or externally creating a <see cref="DbContextOptions" /> and passing it to the <see cref="DbContext" />
    ///         constructor.
    ///     </para>
    ///     <para>
    ///         Data stores (and other extensions) typically define extension methods on this object that allow you to
    ///         configure the context.
    ///     </para>
    /// </summary>
    public class DbContextOptions : IDbContextOptions
    {
        private IModel _model;
        private readonly List<DbContextOptionsExtension> _extensions;
        private IReadOnlyDictionary<string, string> _rawOptions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions" /> class.
        /// </summary>
        public DbContextOptions()
        {
            _extensions = new List<DbContextOptionsExtension>();
            _rawOptions = ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions" /> class with options cloned from
        ///     another <see cref="DbContextOptions" /> instance.
        /// </summary>
        /// <param name="copyFrom"> The options to be cloned. </param>
        protected DbContextOptions([NotNull] DbContextOptions copyFrom)
        {
            Check.NotNull(copyFrom, nameof(copyFrom));

            _model = copyFrom._model;
            _extensions = copyFrom._extensions.ToList();
            _rawOptions = copyFrom._rawOptions;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DbContextOptions" /> class with options cloned from
        ///     another <see cref="DbContextOptions" /> instance.
        /// </summary>
        /// <returns> The new options instance. </returns>
        public virtual DbContextOptions Clone() => new DbContextOptions(this);

        /// <summary>
        ///     Sets the model to be used. If the model is set on <see cref="DbContextOptions" /> then
        ///     <see cref="DbContext.OnModelCreating(ModelBuilder)" /> will not be called on any context constructed
        ///     from the options.
        /// </summary>
        /// <param name="model"> The model to be used. </param>
        /// <returns>
        ///     The same <see cref="DbContextOptions" /> instance so that multiple configuration calls can be chained together.
        /// </returns>
        public virtual DbContextOptions UseModel(IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;

            return this;
        }

        /// <summary>
        ///     Gets the model configured on this options instance. Returns null if no model has been configured.
        /// </summary>
        public virtual IModel Model => _model;

        /// <inheritdoc />
        void IDbContextOptions.AddOrUpdateExtension<TExtension>(Action<TExtension> updater)
        {
            Check.NotNull(updater, nameof(updater));

            var extension = _extensions.OfType<TExtension>().FirstOrDefault();

            if (extension == null)
            {
                extension = new TExtension();
                extension.Configure(_rawOptions);
                _extensions.Add(extension);
            }

            updater(extension);
        }

        /// <inheritdoc />
        void IDbContextOptions.AddExtension(DbContextOptionsExtension extension)
        {
            Check.NotNull(extension, nameof(extension));

            Debug.Assert(_extensions.All(e => e.GetType() != extension.GetType()));

            extension.Configure(_rawOptions);
            _extensions.Add(extension);
        }

        /// <inheritdoc />
        IReadOnlyList<DbContextOptionsExtension> IDbContextOptions.Extensions => _extensions;

        /// <inheritdoc />
        IReadOnlyDictionary<string, string> IDbContextOptions.RawOptions
        {
            get { return _rawOptions; }
            set
            {
                Check.NotNull(value, nameof(value));

                _rawOptions = value;
            }
        }
    }
}
