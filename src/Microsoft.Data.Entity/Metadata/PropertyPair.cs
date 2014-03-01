// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public sealed class PropertyPair : IPropertyPair
    {
        private readonly Property _principal;
        private readonly Property _dependent;

        public PropertyPair([NotNull] Property principal, [NotNull] Property dependent)
        {
            Check.NotNull(principal, "principal");
            Check.NotNull(dependent, "dependent");

            _principal = principal;
            _dependent = dependent;
        }

        public Property Principal
        {
            get { return _principal; }
        }

        public Property Dependent
        {
            get { return _dependent; }
        }

        IProperty IPropertyPair.Principal
        {
            get { return _principal; }
        }

        IProperty IPropertyPair.Dependent
        {
            get { return _dependent; }
        }
    }
}
