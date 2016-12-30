using System;
using System.Collections;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Microsoft.EntityFrameworkCore.MongoDB.Adapter
{
    public class IgnoreEmptyEnumerablesConvention : ConventionBase, IMemberMapConvention
    {
        public IgnoreEmptyEnumerablesConvention()
            : base(Regex.Replace(nameof(IgnoreEmptyEnumerablesConvention), pattern: "Convention$", replacement: ""))
        {
        }

        public virtual void Apply([NotNull] BsonMemberMap memberMap)
        {
            Check.NotNull(memberMap, nameof(memberMap));
            if (memberMap.MemberType.TryGetSequenceType() != null)
            {
                memberMap.SetShouldSerializeMethod(@object =>
                    {
                        object value = memberMap.Getter(@object);
                        return (value as IEnumerable)?.GetEnumerator().MoveNext() ?? false;
                    });
            }
        }
    }
}