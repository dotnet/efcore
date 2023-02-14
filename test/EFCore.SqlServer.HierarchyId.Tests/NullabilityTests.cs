using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer
{
    public class NullabilityTests
    {
        [Fact]
        public void Null_against_null()
        {
            Assert.True((HierarchyId)null == (HierarchyId)null);
            Assert.False((HierarchyId)null != (HierarchyId)null);
            Assert.False((HierarchyId)null > (HierarchyId)null);
            Assert.False((HierarchyId)null >= (HierarchyId)null);
            Assert.False((HierarchyId)null < (HierarchyId)null);
            Assert.False((HierarchyId)null <= (HierarchyId)null);
        }

        [Fact]
        public void Null_against_nonNull()
        {
            var hid = HierarchyId.GetRoot();
            Assert.False(hid == (HierarchyId)null);
            Assert.False((HierarchyId)null == hid);

            Assert.True(hid != (HierarchyId)null);
            Assert.True((HierarchyId)null != hid);

            Assert.False(hid > (HierarchyId)null);
            Assert.False((HierarchyId)null > hid);

            Assert.False(hid >= (HierarchyId)null);
            Assert.False((HierarchyId)null >= hid);

            Assert.False(hid < (HierarchyId)null);
            Assert.False((HierarchyId)null < hid);

            Assert.False(hid <= (HierarchyId)null);
            Assert.False((HierarchyId)null <= hid);
        }

        [Fact]
        public void NullOnly_aggregates_equalTo_null()
        {
            var hid = (HierarchyId)null;
            var collection = new[] { (HierarchyId)null, (HierarchyId)null, };
            var min = collection.Min();
            var max = collection.Max();

            Assert.True(hid == min);
            Assert.True(min == hid);
            Assert.False(hid != min);
            Assert.False(min != hid);

            Assert.True(hid == max);
            Assert.True(max == hid);
            Assert.False(hid != max);
            Assert.False(max != hid);
        }

        [Fact]
        public void Aggregates_including_nulls_equalTo_nonNull()
        {
            var hid = HierarchyId.GetRoot();
            var collection = new[] { (HierarchyId)null, (HierarchyId)null, HierarchyId.GetRoot(), HierarchyId.GetRoot(), };
            var min = collection.Min();
            var max = collection.Max();

            Assert.True(hid == min);
            Assert.True(min == hid);
            Assert.False(hid != min);
            Assert.False(min != hid);

            Assert.True(hid == max);
            Assert.True(max == hid);
            Assert.False(hid != max);
            Assert.False(max != hid);
        }
    }
}
