using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace StackExchange.Redis
{
    /// <summary>
    /// Indicates a range of slots served by a cluster node
    /// </summary>
    public struct SlotRange : IEquatable<SlotRange>, IComparable<SlotRange>, IComparable
    {
        private readonly short from, to;

        /// <summary>
        /// Create a new SlotRange value
        /// </summary>
        public SlotRange(int from, int to)
        {
            checked
            {
                this.from = (short)from;
                this.to = (short)to;
            }
        }

        private SlotRange(short from, short to)
        {
            this.from = from;
            this.to = to;
        }
        /// <summary>
        /// The start of the range (inclusive)
        /// </summary>
        public int From { get { return from; } }

        /// <summary>
        /// The end of the range (inclusive)
        /// </summary>
        public int To { get { return to; } }

        /// <summary>
        /// Indicates whether two ranges are not equal
        /// </summary>
        public static bool operator !=(SlotRange x, SlotRange y)
        {
            return x.from != y.from || x.to != y.to;
        }

        /// <summary>
        /// Indicates whether two ranges are equal
        /// </summary>
        public static bool operator ==(SlotRange x, SlotRange y)
        {
            return x.from == y.from && x.to == y.to;
        }

        /// <summary>
        /// Try to parse a string as a range
        /// </summary>
        public static bool TryParse(string range, out SlotRange value)
        {
            if (string.IsNullOrWhiteSpace(range))
            {
                value = default(SlotRange);
                return false;
            }
            int i = range.IndexOf('-');
            short from, to;
            if (i < 0)
            {
                if (TryParseInt16(range, 0, range.Length, out from))
                {
                    value = new SlotRange(from, from);
                    return true;
                }
            }
            else
            {
                if (TryParseInt16(range, 0, i++, out from) && TryParseInt16(range, i, range.Length - i, out to))
                {
                    value = new SlotRange(from, to);
                    return true;
                }
            }
            value = default(SlotRange);
            return false;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        public int CompareTo(SlotRange other)
        {
            int delta = (int)this.from - (int)other.from;
            return delta == 0 ? (int)this.to - (int)other.to : delta;
        }

        /// <summary>
        /// See Object.Equals
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is SlotRange)
            {
                return Equals((SlotRange)obj);
            }
            return false;
        }
        /// <summary>
        /// Indicates whether two ranges are equal
        /// </summary>
        public bool Equals(SlotRange range)
        {
            return range.from == this.from && range.to == this.to;
        }

        /// <summary>
        /// See Object.GetHashCode()
        /// </summary>
        public override int GetHashCode()
        {
            int x = from, y = to; // makes CS0675 a little happier
            return x | (y << 16);
        }

        /// <summary>
        /// See Object.ToString()
        /// </summary>
        public override string ToString()
        {
            return from == to ? from.ToString() : (from + "-" + to);
        }
        internal bool Includes(int hashSlot)
        {
            return hashSlot >= from && hashSlot <= to;
        }

        static bool TryParseInt16(string s, int offset, int count, out short value)
        {
            checked
            {
                value = 0;
                int tmp = 0;
                for (int i = 0; i < count; i++)
                {
                    char c = s[offset + i];
                    if (c < '0' || c > '9') return false;
                    tmp = (tmp * 10) + (c - '0');
                }
                value = (short)tmp;
                return true;
            }
        }

        int IComparable.CompareTo(object obj)
        {
            return obj is SlotRange ? CompareTo((SlotRange)obj) : -1;
        }
    }

    /// <summary>
    /// Describes the state of the cluster as reported by a single node
    /// </summary>
    public sealed class ClusterConfiguration
    {
        private readonly Dictionary<EndPoint, ClusterNode> nodeLookup = new Dictionary<EndPoint, ClusterNode>();

        private readonly EndPoint origin;
        private readonly ServerSelectionStrategy serverSelectionStrategy;
        internal ClusterConfiguration(ServerSelectionStrategy serverSelectionStrategy, string nodes, EndPoint origin)
        {
            this.serverSelectionStrategy = serverSelectionStrategy;
            this.origin = origin;
            using (var reader = new StringReader(nodes))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var node = new ClusterNode(this, line, origin);
                    nodeLookup.Add(node.EndPoint, node);
                }
            }
        }

        /// <summary>
        /// Gets all nodes contained in the configuration
        /// </summary>
        /// <returns></returns>
        public ICollection<ClusterNode> Nodes { get { return nodeLookup.Values; } }

        /// <summary>
        /// The node that was asked for the configuration
        /// </summary>
        public EndPoint Origin { get { return origin; } }

        /// <summary>
        /// Obtain the node relating to a specified endpoint
        /// </summary>
        public ClusterNode this[EndPoint endpoint]
        {
            get
            {
                ClusterNode result;
                return endpoint == null ? null
                    : nodeLookup.TryGetValue(endpoint, out result) ? result : null;
            }
        }

        internal ClusterNode this[string nodeId]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(nodeId)) return null;
                foreach (var pair in nodeLookup)
                {
                    if (pair.Value.NodeId == nodeId) return pair.Value;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the node that serves the specified slot
        /// </summary>
        public ClusterNode GetBySlot(int slot)
        {
            foreach(var node in Nodes)
            {
                if (!node.IsSlave && node.ServesSlot(slot)) return node;
            }
            return null;
        }
        /// <summary>
        /// Gets the node that serves the specified slot
        /// </summary>
        public ClusterNode GetBySlot(RedisKey key)
        {
            return GetBySlot(serverSelectionStrategy.HashSlot(key));
        }
    }


    /// <summary>
    /// Represents the configuration of a single node in a cluster configuration
    /// </summary>
    public sealed class ClusterNode :  IEquatable<ClusterNode>, IComparable<ClusterNode>, IComparable
    {
        private static readonly ClusterNode Dummy = new ClusterNode();

        private static readonly IList<ClusterNode> NoNodes = new ClusterNode[0];

        private static readonly IList<SlotRange> NoSlots = new SlotRange[0];

        private readonly ClusterConfiguration configuration;

        private readonly EndPoint endpoint;

        private readonly bool isSlave;

        private readonly string nodeId, parentNodeId, raw;

        private readonly IList<SlotRange> slots;

        private IList<ClusterNode> children;

        private ClusterNode parent;

        private string toString;

        internal ClusterNode() { }
        internal ClusterNode(ClusterConfiguration configuration, string raw, EndPoint origin)
        {
            this.configuration = configuration;
            this.raw = raw;
            var parts = raw.Split(StringSplits.Space);

            var flags = parts[2].Split(StringSplits.Comma);
            
            if (flags.Contains("myself"))
            {
                endpoint = origin;
            }
            else
            {
                endpoint = Format.TryParseEndPoint(parts[1]);
            }
            nodeId = parts[0];
            isSlave = flags.Contains("slave");
            parentNodeId = string.IsNullOrWhiteSpace(parts[3]) ? null : parts[3];

            List<SlotRange> slots = null;

            for (int i = 8; i < parts.Length; i++)
            {
                SlotRange range;
                if (SlotRange.TryParse(parts[i], out range))
                {
                    if(slots == null) slots = new List<SlotRange>(parts.Length - i);
                    slots.Add(range);
                }
            }
            this.slots = slots == null ? NoSlots : slots.AsReadOnly();
        }
        /// <summary>
        /// Gets all child nodes of the current node
        /// </summary>
        public IList<ClusterNode> Children
        {
            get
            {
                if (children != null) return children;

                List<ClusterNode> nodes = null;
                foreach (var node in configuration.Nodes)
                {
                    if (node.parentNodeId == this.nodeId)
                    {
                        if (nodes == null) nodes = new List<ClusterNode>();
                        nodes.Add(node);
                    }
                }
                children = nodes == null ? NoNodes : nodes.AsReadOnly();
                return children;
            }
        }

        /// <summary>
        /// Gets the endpoint of the current node
        /// </summary>
        public EndPoint EndPoint { get { return endpoint; } }

        /// <summary>
        /// Gets whether this node is a slave
        /// </summary>
        public bool IsSlave { get { return isSlave; } }

        /// <summary>
        /// Gets the unique node-id of the current node
        /// </summary>
        public string NodeId { get { return nodeId; } }

        /// <summary>
        /// Gets the parent node of the current node
        /// </summary>
        public ClusterNode Parent
        {
            get
            {
                if (parent != null) return parent == Dummy ? null : parent;
                ClusterNode found = configuration[parentNodeId];
                parent = found ?? Dummy;
                return found;
            }
        }

        /// <summary>
        /// Gets the unique node-id of the parent of the current node
        /// </summary>
        public string ParentNodeId { get { return parentNodeId; } }

        /// <summary>
        /// The configuration as reported by the server
        /// </summary>
        public string Raw { get { return raw; } }

        /// <summary>
        /// The slots owned by this server
        /// </summary>
        public IList<SlotRange> Slots {  get {  return slots; } }
        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        public int CompareTo(ClusterNode other)
        {
            if (other == null) return -1;

            if (this.isSlave != other.isSlave) return isSlave ? 1 : -1; // masters first

            if (isSlave) // both slaves? compare by parent, so we get masters A, B, C and then slaves of A, B, C
            {
                int i = string.CompareOrdinal(this.parentNodeId, other.parentNodeId);
                if (i != 0) return i;
            }
            return string.CompareOrdinal(this.nodeId, other.nodeId);

        }

        /// <summary>
        /// See Object.Equals
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as ClusterNode);
        }

        /// <summary>
        /// Indicates whether two ClusterNode instances are equivalent
        /// </summary>
        public bool Equals(ClusterNode node)
        {
            if (node == null) return false;

            return this.ToString() == node.ToString(); // lazy, but effective - plus only computes once
        }

        /// <summary>
        /// See object.GetHashCode()
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// See Object.ToString()
        /// </summary>
        public override string ToString()
        {
            if (toString != null) return toString;
            var sb = new StringBuilder().Append(nodeId).Append(" at ").Append(endpoint);
            if(isSlave)
            {
                sb.Append(", slave of ").Append(parentNodeId);
                var parent = Parent;
                if (parent != null) sb.Append(" at ").Append(parent.endpoint);
            }
            var childCount = Children.Count;
            switch(childCount)
            {
                case 0: break;
                case 1: sb.Append(", 1 slave"); break;
                default: sb.Append(", ").Append(childCount).Append(" slaves"); break;
            }
            if(slots.Count != 0)
            {
                sb.Append(", slots: ");
                foreach(var slot in slots)
                {
                    sb.Append(slot).Append(' ');
                }
                sb.Length -= 1; // remove tailing space
            }
            return toString = sb.ToString();
        }
        internal bool ServesSlot(int hashSlot)
        {
            foreach (var slot in slots)
            {
                if (slot.Includes(hashSlot)) return true;
            }
            return false;
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo(obj as ClusterNode);
        }
    }
}
