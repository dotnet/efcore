// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbContextOptions : IDbContextOptionsExtensions
    {
        private bool _locked;
        private IModel _model;
        private readonly List<DbContextOptionsExtension> _extensions = new List<DbContextOptionsExtension>();

        public virtual DbContextOptions UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, "model");
            CheckNotLocked();

            _model = model;

            return this;
        }

        [CanBeNull]
        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual void Lock()
        {
            _locked = true;
        }

        public virtual bool IsLocked
        {
            get { return _locked; }
        }

        private void CheckNotLocked([CallerMemberName] string memberName = "")
        {
            if (_locked)
            {
                throw new InvalidOperationException(Strings.FormatEntityConfigurationLocked(memberName));
            }
        }

        void IDbContextOptionsExtensions.AddOrUpdateExtension<TExtension>(Action<TExtension> updater, string memberName)
        {
            Check.NotNull(updater, "updater");
            CheckNotLocked(memberName);

            var extension = _extensions.OfType<TExtension>().FirstOrDefault();

            if (extension == null)
            {
                extension = new TExtension();
                _extensions.Add(extension);
            }

            updater(extension);
        }

        void IDbContextOptionsExtensions.AddExtension(DbContextOptionsExtension extension, string memberName)
        {
            Check.NotNull(extension, "extension");
            CheckNotLocked(memberName);

            Contract.Assert(_extensions.All(e => e.GetType() != extension.GetType()));

            _extensions.Add(extension);
        }

        IReadOnlyList<DbContextOptionsExtension> IDbContextOptionsExtensions.Extensions
        {
            get { return _extensions; }
        }
    }
}
