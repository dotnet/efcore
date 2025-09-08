// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Linq;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGConnectionSettings
    {
        public XGConnectionSettings()
        {
        }

        public XGConnectionSettings(DbConnection connection)
            : this(connection.ConnectionString)
        {
        }

        public XGConnectionSettings(string connectionString)
        {
            var csb = new XGConnectionStringBuilder(connectionString);
        }

        public virtual bool? TreatTinyAsBoolean { get; }

        protected virtual bool Equals(XGConnectionSettings other)
        {
            return TreatTinyAsBoolean == other.TreatTinyAsBoolean;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((XGConnectionSettings)obj);
        }

        public override int GetHashCode()
            => HashCode.Combine(TreatTinyAsBoolean);
    }
}
