// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ImmutableDbContextOptions : IDbContextOptionsConstruction
    {
        private bool _locked;
        private IModel _model;
        private readonly List<EntityConfigurationExtension> _extensions = new List<EntityConfigurationExtension>();

        [CanBeNull]
        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual IReadOnlyList<EntityConfigurationExtension> Extensions
        {
            get { return _extensions; }
        }

        IModel IDbContextOptionsConstruction.Model
        {
            set
            {
                CheckNotLocked();

                _model = value;
            }
        }

        void IDbContextOptionsConstruction.AddOrUpdateExtension<TExtension>(Action<TExtension> updater)
        {
            Check.NotNull(updater, "updater");

            CheckNotLocked();

            var extension = _extensions.OfType<TExtension>().FirstOrDefault();

            if (extension == null)
            {
                extension = new TExtension();
                _extensions.Add(extension);
            }

            updater(extension);
        }

        void IDbContextOptionsConstruction.Lock()
        {
            _locked = true;
        }

        private void CheckNotLocked([CallerMemberName] String memberName = "")
        {
            if (_locked)
            {
                throw new InvalidOperationException(Strings.FormatEntityConfigurationLocked(memberName));
            }
        }
    }
}
