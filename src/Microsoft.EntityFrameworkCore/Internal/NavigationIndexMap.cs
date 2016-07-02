using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    /// 
    /// </summary>
    public class NavigationIndexMap
    {
        private readonly Dictionary<IncludeSpecification, NavigationIndex> _indexMap =
            new Dictionary<IncludeSpecification, NavigationIndex>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join("|", _indexMap.Select(i => $"{i.Value.Index}-{i.Key.ToString()}::{i.Value.ReferencedMap}"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [IndexerName("ReferencedIndexes")]
        public NavigationIndex this[IncludeSpecification value] => _indexMap[value];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeSpecification"></param>
        /// <param name="index"></param>
        public void Add(IncludeSpecification includeSpecification, NavigationIndex index)
        {
            _indexMap.Add(includeSpecification, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeSpecification"></param>
        /// <returns></returns>
        public bool Exist(IncludeSpecification includeSpecification)
        {
            return _indexMap.ContainsKey(includeSpecification);
        }
    }
}
