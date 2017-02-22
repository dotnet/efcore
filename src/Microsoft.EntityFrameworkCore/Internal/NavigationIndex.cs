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
        public virtual int Index { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual NavigationIndexMap ReferencedMap
        {
            [NotNull]
            get;
            [param: NotNull]
            set; 
        }
    }
}
