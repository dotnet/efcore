using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Microsoft.EntityFrameworkCore.MongoDB.Adapter
{
    public class KeyAttributeConvention : ConventionBase, IMemberMapConvention
    {
        public KeyAttributeConvention()
            : base(Regex.Replace(nameof(KeyAttributeConvention), pattern: "Convention$", replacement: ""))
        {
        }

        public virtual void Apply([NotNull] BsonMemberMap memberMap)
        {
            Check.NotNull(memberMap, nameof(memberMap));
            if (memberMap.MemberInfo.IsDefined(typeof(KeyAttribute)))
            {
                memberMap.ClassMap.SetIdMember(memberMap);
            }
        }
    }
}