using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    /// 
    /// </summary>
    public class NavigationIndex
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="referencedMap"></param>
        public NavigationIndex(int index, NavigationIndexMap referencedMap)
        {
            ReferencedMap = referencedMap;
            Index = index;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int Index { get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual NavigationIndexMap ReferencedMap { [NotNull] get; }
    }
}
