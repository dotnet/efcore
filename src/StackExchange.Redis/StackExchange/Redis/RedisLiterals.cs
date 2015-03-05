using System;
using System.Text;

namespace StackExchange.Redis
{
    internal static class RedisLiterals
    {
        // unlike primary commands, these do not get altered by the command-map; we may as
        // well compute the bytes once and share them
        public static readonly RedisValue
            ADDR = "ADDR",
            AFTER = "AFTER",
            AGGREGATE = "AGGREGATE",
            ALPHA = "ALPHA",
            AND = "AND",
            BEFORE = "BEFORE",
            BY = "BY",
            CHANNELS = "CHANNELS",
            COPY = "COPY",
            COUNT = "COUNT",
            DESC = "DESC",
            EX = "EX",
            EXISTS = "EXISTS",
            FLUSH = "FLUSH",
            GET = "GET",
            GETNAME = "GETNAME",
            ID = "ID",
            KILL = "KILL",
            LIMIT = "LIMIT",
            LIST = "LIST",
            LOAD = "LOAD",
            MATCH = "MATCH",
            MAX = "MAX",
            MIN = "MIN",
            NODES = "NODES",
            NOSAVE = "NOSAVE",
            NOT = "NOT",
            NUMPAT = "NUMPAT",
            NUMSUB = "NUMSUB",
            NX = "NX",
            OBJECT = "OBJECT",
            OR = "OR",
            PAUSE = "PAUSE",
            PING = "PING",
            PX = "PX",
            REPLACE = "REPLACE",
            RESET = "RESET",
            RESETSTAT = "RESETSTAT",
            REWRITE = "REWRITE",
            SAVE = "SAVE",
            SEGFAULT = "SEGFAULT",
            SET = "SET",
            SETNAME = "SETNAME",
            SKIPME = "SKIPME",
            STORE = "STORE",
            TYPE = "TYPE",
            WEIGHTS = "WEIGHTS",
            WITHSCORES = "WITHSCORES",
            XOR = "XOR",
            XX = "XX",



            // Sentinel Literals
            MASTERS = "MASTERS",
            MASTER = "MASTER",
            SLAVES = "SLAVES",
            GETMASTERADDRBYNAME = "GET-MASTER-ADDR-BY-NAME",
//            RESET = "RESET",
            FAILOVER = "FAILOVER", 

            // Sentinel Literals as of 2.8.4
            MONITOR = "MONITOR",
            REMOVE = "REMOVE",
//            SET = "SET",

            // DO NOT CHANGE CASE: these are configuration settings and MUST be as-is
            databases = "databases",
            no = "no",
            normal = "normal",
            pubsub = "pubsub",
            replication = "replication",
            server = "server",
            slave = "slave",
            slave_read_only = "slave-read-only",
            timeout = "timeout",
            yes = "yes",

            MinusSymbol = "-",
            PlusSumbol = "+",
            Wildcard = "*";

        public static readonly byte[] BytesOK = Encoding.UTF8.GetBytes("OK");
        public static readonly byte[] BytesPONG = Encoding.UTF8.GetBytes("PONG");
        public static readonly byte[] BytesBackgroundSavingStarted = Encoding.UTF8.GetBytes("Background saving started");
        public static readonly byte[] ByteWildcard = { (byte)'*' };
        internal static RedisValue Get(Bitwise operation)
        {
            switch(operation)
            {
                case Bitwise.And: return AND;
                case Bitwise.Or: return OR;
                case Bitwise.Xor: return XOR;
                case Bitwise.Not: return NOT;
                default: throw new ArgumentOutOfRangeException("operation");
            }
        }
    }
}