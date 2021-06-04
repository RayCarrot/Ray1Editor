using System;

namespace RayCarrot.Ray1Editor
{
    public static class ArrayExtensions
    {
        public static int FindItemIndex<T>(this T[] array, Predicate<T> match) => Array.FindIndex(array, match);
    }
}