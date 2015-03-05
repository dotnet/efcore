namespace StackExchange.Redis
{
    internal enum ResultType : byte
    {
        None = 0,
        SimpleString = 1,
        Error = 2,
        Integer = 3,
        BulkString = 4,
        MultiBulk = 5
    }
}
