using System;

namespace Ray1Editor;

public static class ArrayExtensions
{
    public static int FindIndex<T>(this T[] array, Predicate<T> match) => Array.FindIndex(array, match);
}