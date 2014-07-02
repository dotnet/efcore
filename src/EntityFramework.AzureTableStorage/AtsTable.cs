// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsTable
    {
        public AtsTable([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            Name = name;
        }

        public virtual string Name { get; private set; }

        protected bool Equals(AtsTable other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((AtsTable)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
