// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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

        public virtual IReadOnlyList<EntityConfigurationExtension> Extensions
        {
            get { return _extensions; }
        }

        IModel IEntityConfigurationConstruction.Model
        {
            set
            {
                CheckNotLocked();

                _model = value;
            }
        }

        void IEntityConfigurationConstruction.AddOrUpdateExtension<TExtension>(Action<TExtension> updater)
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

        void IEntityConfigurationConstruction.Lock()
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
