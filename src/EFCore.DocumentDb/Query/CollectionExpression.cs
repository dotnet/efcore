// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CollectionExpression : SourceExpression
    {
        public CollectionExpression(IQuerySource querySource, string alias, string collectionName)
            : base(querySource, alias)
        {
            CollectionName = collectionName;
        }

        public string CollectionName { get; }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((CollectionExpression)obj);
        }

        private bool Equals(CollectionExpression other)
            => string.Equals(CollectionName, other.CollectionName)
               && string.Equals(Alias, other.Alias)
               && Equals(QuerySource, other.QuerySource);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CollectionName.GetHashCode();
                hashCode = (hashCode * 397) ^ Alias.GetHashCode();
                hashCode = (hashCode * 397) ^ QuerySource.GetHashCode();

                return hashCode;
            }
        }

        public override string ToString() => $"{CollectionName} {Alias}";
    }
}
