﻿using BinarySerializer;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Ray1Editor;

public class Palette
{
    public Palette(IList<Color> colors, Pointer pointer, string name)
    {
        Colors = colors;
        Pointer = pointer;
        Name = name;
    }

    public IList<Color> Colors { get; }
    public Pointer Pointer { get; }
    public string Name { get; }
    public bool CanEditAlpha { get; init; }
    public bool IsFirstTransparent { get; init; }
    public int? DisplayWrap { get; init; }

    /// <summary>
    /// The name of the group of palettes this palette belongs to. This allows a single palette in the group to be
    /// selected at a time.
    /// </summary>
    public string SelectionGroup { get; init; }

    public Color GetColor(int index) => IsFirstTransparent && index == 0 ? Color.Transparent : Colors[index];

    public virtual void Update() { }
}