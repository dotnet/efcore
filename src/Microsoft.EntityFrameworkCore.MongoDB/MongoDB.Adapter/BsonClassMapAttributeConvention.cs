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
    public class BsonClassMapAttributeConvention<TClassMapAttribute> : ConventionBase, IClassMapConvention
        where TClassMapAttribute : Attribute, IBsonClassMapAttribute
    {
        public BsonClassMapAttributeConvention()
            : base(Regex.Replace(nameof(BsonClassMapAttributeConvention<TClassMapAttribute>), pattern: "Convention$", replacement: ""))
        {
        }

        public virtual void Apply([NotNull] BsonClassMap classMap)
        {
            Check.NotNull(classMap, nameof(classMap));
            IEnumerable<TClassMapAttribute> classMapAttributes = classMap.ClassType
                .GetTypeInfo()
                .GetCustomAttributes<TClassMapAttribute>();
            foreach (TClassMapAttribute classMapAttribute in classMapAttributes)
            {
                classMapAttribute.Apply(classMap);
            }
        }
    }
}