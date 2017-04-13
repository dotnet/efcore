using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
        public override string ToString() => string.Join("|", _indexMap.Select(i => $"{i.Value.Index}-{i.Key.ToString()}::{i.Value.ReferencedMap}"));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [IndexerName("ReferencedIndexes")]
        [NotNull]
        public virtual NavigationIndex this[[NotNull]IncludeSpecification value] => _indexMap[value];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeSpecification"></param>
        /// <param name="index"></param>
        public virtual void Add([NotNull]IncludeSpecification includeSpecification, [NotNull]NavigationIndex index) => _indexMap.Add(includeSpecification, index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeSpecification"></param>
        /// <returns></returns>
        public virtual bool Exist([NotNull]IncludeSpecification includeSpecification) => _indexMap.ContainsKey(includeSpecification);
    }
}
