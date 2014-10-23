// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbContextOptions : IDbContextOptionsExtensions
    {
        private IModel _model;
        private readonly List<DbContextOptionsExtension> _extensions;
        private IReadOnlyDictionary<string, string> _rawOptions;

        public DbContextOptions()
        {
            _extensions = new List<DbContextOptionsExtension>();
            _rawOptions = ImmutableDictionary<string, string>.Empty;
        }

        protected DbContextOptions([NotNull] DbContextOptions copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");

            _model = copyFrom._model;
            _extensions = copyFrom._extensions.ToList();
            _rawOptions = copyFrom._rawOptions;
        }

        public virtual DbContextOptions Clone()
        {
            return new DbContextOptions(this);
        }

        public virtual DbContextOptions UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _model = model;

            return this;
        }

        [CanBeNull]
        public virtual IModel Model
        {
            get { return _model; }
        }

        void IDbContextOptionsExtensions.AddOrUpdateExtension<TExtension>(Action<TExtension> updater)
        {
            Check.NotNull(updater, "updater");

            var extension = _extensions.OfType<TExtension>().FirstOrDefault();

            if (extension == null)
            {
                extension = new TExtension();
                extension.Configure(_rawOptions);
                _extensions.Add(extension);
            }

            updater(extension);
        }

        void IDbContextOptionsExtensions.AddExtension(DbContextOptionsExtension extension)
        {
            Check.NotNull(extension, "extension");

            Contract.Assert(_extensions.All(e => e.GetType() != extension.GetType()));

            extension.Configure(_rawOptions);
            _extensions.Add(extension);
        }

        IReadOnlyList<DbContextOptionsExtension> IDbContextOptionsExtensions.Extensions
        {
            get { return _extensions; }
        }

        IReadOnlyDictionary<string, string> IDbContextOptionsExtensions.RawOptions
        {
            get { return _rawOptions; }
            set
            {
                Check.NotNull(value, "value");

                _rawOptions = value;
            }
        }
    }
}
