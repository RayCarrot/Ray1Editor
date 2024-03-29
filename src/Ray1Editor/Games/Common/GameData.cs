﻿using System;
using System.Collections.Generic;
using BinarySerializer;

namespace Ray1Editor;

/// <summary>
/// Game data to be stored for the editor and used by the game manager
/// </summary>
public abstract class GameData : IDisposable
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="context">The serializer context</param>
    /// <param name="textureManager">The texture manager</param>
    protected GameData(Context context, TextureManager textureManager)
    {
        Context = context;
        TextureManager = textureManager;
        Random = new Random();
        Objects = new List<GameObject>();
        Layers = new List<Layer>();
    }

    /// <summary>
    /// The serializer context
    /// </summary>
    public Context Context { get; }

    /// <summary>
    /// The texture manager
    /// </summary>
    public TextureManager TextureManager { get; }

    /// <summary>
    /// The current random number generator
    /// </summary>
    public Random Random { get; }

    /// <summary>
    /// The objects
    /// </summary>
    public List<GameObject> Objects { get; }

    /// <summary>
    /// The layers. Can be tile maps, backgrounds etc.
    /// </summary>
    public List<Layer> Layers { get; }

    /// <summary>
    /// Loads the editor elements stored in the data
    /// </summary>
    /// <param name="e">The editor scene to load to</param>
    public void LoadElements(EditorScene e)
    {
        // Load objects
        foreach (var obj in Objects)
            e.LoadElement(obj);

        // Load layers
        foreach (var layer in Layers)
            e.LoadElement(layer);
    }

    public void Dispose()
    {
        Context?.Dispose();
        TextureManager?.Dispose();
    }
}