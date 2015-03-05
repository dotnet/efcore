namespace StackExchange.Redis
{
    /// <summary>
    /// The intrinsinc data-types supported by redis
    /// </summary>
    /// <remarks>http://redis.io/topics/data-types</remarks>
    public enum RedisType
    {
        /// <summary>
        /// The specified key does not exist
        /// </summary>
        None,
        /// <summary>
        /// Strings are the most basic kind of Redis value. Redis Strings are binary safe, this means that a Redis string can contain any kind of data, for instance a JPEG image or a serialized Ruby object.
        /// A String value can be at max 512 Megabytes in length.
        /// </summary>
        /// <remarks>http://redis.io/commands#string</remarks>
        String,
        /// <summary>
        /// Redis Lists are simply lists of strings, sorted by insertion order. It is possible to add elements to a Redis List pushing new elements on the head (on the left) or on the tail (on the right) of the list.
        /// </summary>
        /// <remarks>http://redis.io/commands#list</remarks>
        List,
        /// <summary>
        /// Redis Sets are an unordered collection of Strings. It is possible to add, remove, and test for existence of members in O(1) (constant time regardless of the number of elements contained inside the Set).
        /// Redis Sets have the desirable property of not allowing repeated members. Adding the same element multiple times will result in a set having a single copy of this element. Practically speaking this means that adding a member does not require a check if exists then add operation.
        /// </summary>
        /// <remarks>http://redis.io/commands#set</remarks>
        Set,
        /// <summary>
        /// Redis Sorted Sets are, similarly to Redis Sets, non repeating collections of Strings. The difference is that every member of a Sorted Set is associated with score, that is used in order to take the sorted set ordered, from the smallest to the greatest score. While members are unique, scores may be repeated.
        /// </summary>
        /// <remarks>http://redis.io/commands#sorted_set</remarks>
        SortedSet,
        /// <summary>
        /// Redis Hashes are maps between string fields and string values, so they are the perfect data type to represent objects (eg: A User with a number of fields like name, surname, age, and so forth)
        /// </summary>
        /// <remarks>http://redis.io/commands#hash</remarks>
        Hash,
        /// <summary>
        /// The data-type was not recognised by the client library
        /// </summary>
        Unknown,
    }

}
