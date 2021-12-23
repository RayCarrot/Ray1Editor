using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RayCarrot.Ray1Editor;

/// <summary>
/// Extension methods for an <see cref="ObservableCollection{T}"/>
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Adds a range of items to an <see cref="ObservableCollection{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection</typeparam>
    /// <param name="collection">The collection to add to</param>
    /// <param name="range">The range of items to add</param>
    /// <exception cref="ArgumentNullException"/>
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        if (range == null)
            throw new ArgumentNullException(nameof(range));

        foreach (T item in range)
            collection.Add(item);
    }

    /// <summary>
    /// Sorts all items in an <see cref="ObservableCollection{T}"/> based on the specified comparison
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection</typeparam>
    /// <param name="collection">The collection to sort</param>
    /// <param name="comparison">The comparison to use</param>
    /// <exception cref="ArgumentNullException"/>
    public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        if (comparison == null)
            throw new ArgumentNullException(nameof(comparison));

        // Create a list from the collection
        var sortableList = new List<T>(collection);

        // Sort the list
        sortableList.Sort(comparison);

        // Move each item to their new position based on the sorted list
        for (int i = 0; i < sortableList.Count; i++)
            collection.Move(collection.IndexOf(sortableList[i]), i);
    }
}