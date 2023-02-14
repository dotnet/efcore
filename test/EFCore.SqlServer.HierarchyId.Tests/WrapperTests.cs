using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer
{
    public class WrapperTests
    {
        [Fact]
        public void GetAncestor_returns_null_when_too_high()
            => Assert.Null(HierarchyId.Parse("/1/").GetAncestor(2));

        [Fact]
        public void GetReparentedValue_returns_null_when_newRoot_is_null()
            => Assert.Null(HierarchyId.Parse("/1/").GetReparentedValue(HierarchyId.GetRoot(), newRoot: null));
    }
}
