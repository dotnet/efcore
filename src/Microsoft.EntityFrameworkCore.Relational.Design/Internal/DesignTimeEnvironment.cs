namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     For internal use only. This relies on cross compiling.
    /// </summary>
    public static class DesignTimeEnvironment
    {
        // TODO create more rebust design-time detection of UWP
#if NETCORE50
        public static bool IsUwp() => true;
#else
        public static  bool IsUwp() => false;
#endif
    }
}
