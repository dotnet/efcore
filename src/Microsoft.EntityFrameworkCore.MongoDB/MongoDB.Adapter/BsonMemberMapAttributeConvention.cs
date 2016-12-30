using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Microsoft.EntityFrameworkCore.MongoDB.Adapter
{
    public abstract class BsonMemberMapAttributeConvention<TMemberMapAttribute> : ConventionBase, IMemberMapConvention
        where TMemberMapAttribute : Attribute, IBsonMemberMapAttribute
    {
        protected BsonMemberMapAttributeConvention()
            : base(Regex.Replace(nameof(BsonMemberMapAttributeConvention<TMemberMapAttribute>), pattern: "Convention$", replacement: ""))
        {
        }

        public virtual void Apply([NotNull] BsonMemberMap memberMap)
        {
            Check.NotNull(memberMap, nameof(memberMap));
            IEnumerable<TMemberMapAttribute> memberMapAttributes = memberMap.MemberInfo
                .GetCustomAttributes<TMemberMapAttribute>();
            foreach (TMemberMapAttribute memberMapAttribute in memberMapAttributes)
            {
                memberMapAttribute.Apply(memberMap);
            }
        }
    }
}