using System.Collections.Generic;

namespace Ray1Editor;

public static class ListExtensions
{
    public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
    {
        var item = list[oldIndex];

        list.RemoveAt(oldIndex);

        if (newIndex > oldIndex) 
            newIndex--;

        list.Insert(newIndex, item);
    }
}