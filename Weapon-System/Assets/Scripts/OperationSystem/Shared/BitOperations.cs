using System.Runtime.CompilerServices;

namespace ECS.Shared
{
    internal static class BitOperations
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int TrailingZeroCount(ulong value)
        {
            if (value == 0)
                return 64;

            var count = 0;
            while ((value & 1) == 0)
            {
                count++;
                value >>= 1;
            }

            return count;
        }
    }
}