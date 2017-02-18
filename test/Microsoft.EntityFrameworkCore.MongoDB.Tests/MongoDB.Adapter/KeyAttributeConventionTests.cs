#if !(NET451 && DRIVER_NOT_SIGNED)
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore.MongoDB.Adapter;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.MongoDB.Adapter
{
    public class KeyAttributeConventionTests
    {
        [Theory]
        [InlineData(nameof(SimpleRecord.Id))]
        [InlineData(nameof(SimpleRecord.StringProperty))]
        [InlineData(nameof(SimpleRecord.IntProperty))]
        public void Sets_id_member_when_key_attribute_present(string memberName)
        {
            MemberInfo memberInfo = typeof(SimpleRecord)
                .GetTypeInfo()
                .GetProperty(memberName);
            bool isIdMember = memberInfo.IsDefined(typeof(KeyAttribute));
            var keyAttributeConvention = new KeyAttributeConvention();
            var bsonClasspMap = new BsonClassMap<SimpleRecord>();
            BsonMemberMap bsonMemberMap = bsonClasspMap.MapMember(typeof(SimpleRecord).GetTypeInfo().GetProperty(memberName));
            keyAttributeConvention.Apply(bsonMemberMap);
            Assert.Equal(isIdMember, bsonClasspMap.IdMemberMap == bsonMemberMap);
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)