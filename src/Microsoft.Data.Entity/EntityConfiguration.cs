// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityConfiguration : IEntityConfigurationConstruction
    {
        private bool _locked;
        private IModel _model;
        private readonly List<EntityConfigurationExtension> _extensions = new List<EntityConfigurationExtension>();

        [CanBeNull]
        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual IReadOnlyList<EntityConfigurationExtension> Extensions()
        {
            return _extensions;
        }

        IModel IEntityConfigurationConstruction.Model
        {
            set
            {
                CheckNotLocked();

                _model = value;
            }
        }

        void IEntityConfigurationConstruction.AddExtension<TExtension>(TExtension extension)
        {
            Check.NotNull(extension, "extension");

            CheckNotLocked();

            foreach (var existing in _extensions.OfType<TExtension>().ToArray())
            {
                _extensions.Remove(existing);
            }

            _extensions.Add(extension);
        }

        void IEntityConfigurationConstruction.Lock()
        {
            _locked = true;
        }

        private void CheckNotLocked([CallerMemberName] String memberName = "")
        {
            if (_locked)
            {
                // TODO: Message
                throw new InvalidOperationException("Locked: " + memberName);
            }
        }
    }
}
