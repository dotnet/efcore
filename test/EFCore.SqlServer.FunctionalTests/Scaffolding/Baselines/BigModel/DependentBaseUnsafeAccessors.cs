// <auto-generated />
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Scaffolding;

#pragma warning disable 219, 612, 618
#nullable disable

namespace TestNamespace
{
    public static class DependentBaseUnsafeAccessors<TKey>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Id>k__BackingField")]
        public static extern ref TKey Id(CompiledModelTestBase.DependentBase<TKey> @this);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Principal>k__BackingField")]
        public static extern ref CompiledModelTestBase.PrincipalDerived<CompiledModelTestBase.DependentBase<TKey>> Principal(CompiledModelTestBase.DependentBase<TKey> @this);
    }
}
