using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Microsoft.EntityFrameworkCore.MongoDB.Adapter
{
    public class IgnoreNullOrEmptyStringsConvention : ConventionBase, IMemberMapConvention
    {
        public IgnoreNullOrEmptyStringsConvention()
            : base(Regex.Replace(nameof(IgnoreNullOrEmptyStringsConvention), pattern: "Convention$", replacement: ""))
        {            
        }

        public virtual void Apply([NotNull] BsonMemberMap memberMap)
        {
            Check.NotNull(memberMap, nameof(memberMap));
            if (memberMap.MemberType == typeof(string))
            {
                SetShouldSerializeMethod(memberMap);
            }
        }

        private static void SetShouldSerializeMethod(BsonMemberMap memberMap)
        {
            var defaultString = memberMap.DefaultValue as string;
            if (!string.IsNullOrEmpty(defaultString))
            {
                ShouldSerializeIfNotDefault(memberMap, defaultString);
            }
            else
            {
                ShouldSerializeIfNotEmpty(memberMap);
            }
        }

        private static void ShouldSerializeIfNotEmpty(BsonMemberMap memberMap)
            => memberMap.SetShouldSerializeMethod(@object => !string.IsNullOrEmpty(memberMap.Getter(@object) as string));

        private static void ShouldSerializeIfNotDefault(BsonMemberMap memberMap, string defaultString)
            => memberMap.SetShouldSerializeMethod(@object => !string.Equals(defaultString, memberMap.Getter(@object) as string));
    }
}