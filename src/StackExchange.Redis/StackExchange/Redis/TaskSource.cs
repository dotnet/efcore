using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    /// <summary>
    /// We want to prevent callers hijacking the reader thread; this is a bit nasty, but works;
    /// see http://stackoverflow.com/a/22588431/23354 for more information; a huge
    /// thanks to Eli Arbel for spotting this (even though it is pure evil; it is *my kind of evil*)
    /// </summary>
#if DEBUG
    public // for the unit tests in TaskTests.cs
#endif
    static class TaskSource
    {
        /// <summary>
        /// Indicates whether the specified task will not hijack threads when results are set
        /// </summary>
        public static readonly Func<Task, bool> IsSyncSafe;
        static TaskSource()
        {
            try
            {
                Type taskType = typeof(Task);
                FieldInfo continuationField = taskType.GetField("m_continuationObject", BindingFlags.Instance | BindingFlags.NonPublic);
                Type safeScenario = taskType.GetNestedType("SetOnInvokeMres", BindingFlags.NonPublic);
                if (continuationField != null && continuationField.FieldType == typeof(object) && safeScenario != null)
                {
                    var method = new DynamicMethod("IsSyncSafe", typeof(bool), new[] { typeof(Task) }, typeof(Task), true);
                    var il = method.GetILGenerator();
                    //var hasContinuation = il.DefineLabel();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, continuationField);
                    Label nonNull = il.DefineLabel(), goodReturn = il.DefineLabel();
                    // check if null
                    il.Emit(OpCodes.Brtrue_S, nonNull);
                    il.MarkLabel(goodReturn);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Ret);

                    // check if is a SetOnInvokeMres - if so, we're OK
                    il.MarkLabel(nonNull);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, continuationField);
                    il.Emit(OpCodes.Isinst, safeScenario);
                    il.Emit(OpCodes.Brtrue_S, goodReturn);

                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ret);

                    IsSyncSafe = (Func<Task, bool>)method.CreateDelegate(typeof(Func<Task, bool>));

                    // and test them (check for an exception etc)
                    var tcs = new TaskCompletionSource<int>();
                    bool expectTrue = IsSyncSafe(tcs.Task);
                    tcs.Task.ContinueWith(delegate { });
                    bool expectFalse = IsSyncSafe(tcs.Task);
                    tcs.SetResult(0);
                    if(!expectTrue || expectFalse)
                    {
                        Debug.WriteLine("IsSyncSafe reported incorrectly!");
                        Trace.WriteLine("IsSyncSafe reported incorrectly!");
                        // revert to not trusting /them
                        IsSyncSafe = null;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Trace.WriteLine(ex.Message);
                IsSyncSafe = null;
            }
            if (IsSyncSafe == null)
                IsSyncSafe = t => false; // assume: not
        }

        /// <summary>
        /// Create a new TaskCompletion source
        /// </summary>
        public static TaskCompletionSource<T> Create<T>(object asyncState)
        {
            return new TaskCompletionSource<T>(asyncState);
        }

        /// <summary>
        /// Create a new TaskCompletionSource that will not allow result-setting threads to be hijacked
        /// </summary>
        public static TaskCompletionSource<T> CreateDenyExecSync<T>(object asyncState)
        {
            var source = new TaskCompletionSource<T>(asyncState);
            //DenyExecSync(source.Task);
            return source;
        }
    }
}
