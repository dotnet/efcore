// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ConfigurationAnnotations
    {
        private readonly Dictionary<Type, Annotations> _annotations = new Dictionary<Type, Annotations>();

        public virtual Annotations this[[param: NotNull] Type serviceType]
        {
            get
            {
                Check.NotNull(serviceType, "serviceType");

                Annotations annotations;
                if (!_annotations.TryGetValue(serviceType, out annotations))
                {
                    annotations = new Annotations();
                    _annotations[serviceType] = annotations;
                }
                return annotations;
            }
        }

        public virtual bool HasAnnotations([NotNull] Type serviceType)
        {
            Check.NotNull(serviceType, "serviceType");

            return _annotations.ContainsKey(serviceType);
        }
    }
}
