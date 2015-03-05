using System.Threading.Tasks;

namespace StackExchange.Redis
{
    internal static class CompletedTask<T>
    {
        private readonly static Task<T> @default = FromResult(default(T), null);

        public static Task<T> Default(object asyncState)
        {
            return asyncState == null ? @default : FromResult(default(T), asyncState);
        }
        public static Task<T> FromResult(T value, object asyncState)
        {
            // note we do not need to deny exec-sync here; the value will be known
            // before we hand it to them
            var tcs = TaskSource.Create<T>(asyncState);
            tcs.SetResult(value);
            return tcs.Task;
        }
    }
}
