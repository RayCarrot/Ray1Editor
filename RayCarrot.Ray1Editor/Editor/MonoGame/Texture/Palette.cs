using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public class Palette
    {
        public Palette(IEnumerable<BaseColor> colors, string name, int? wrap = null) : this(colors.Select(x => new Color(x.Red, x.Green, x.Blue, x.Alpha)).ToArray(), name, wrap) { }

        public Palette(IList<Color> colors, string name, int? wrap = null)
        {
            Wrap = wrap ?? colors.Count;
            Colors = colors;
            Name = name;
            SectionsCount = wrap != null ? (int)Math.Ceiling(TotalLength / (float) Wrap) : 1;
        }

        public IList<Color> Colors { get; }
        public string Name { get; }
        public int TotalLength => Colors.Count;
        public int SectionsCount { get; }
        public int Wrap { get; }
        public bool CanEditAlpha { get; init; }
        public bool IsFirstTransparent { get; init; }

        public Color GetColor(int index) => IsFirstTransparent && index == 0 ? Color.Transparent : Colors[index];
        public Color GetColor(int primaryIndex, int secondaryIndex) => GetColor(primaryIndex * Wrap + secondaryIndex);
    }
}