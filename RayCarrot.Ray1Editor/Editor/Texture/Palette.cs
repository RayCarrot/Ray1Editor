using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public class Palette
    {
        public Palette(IEnumerable<BaseColor> colors, string name)
        {
            Pointer pointer = null;
            var convertedColors = colors.Select((x, i) =>
            {
                if (i == 0)
                    pointer = x.Offset;

                return new Color(x.Red, x.Green, x.Blue, x.Alpha);
            }).ToArray();

            Colors = convertedColors;
            Pointer = pointer;
            Name = name;
        }

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

        public Color GetColor(int index) => IsFirstTransparent && index == 0 ? Color.Transparent : Colors[index];

        public T[] ToBaseColorArray<T>() where T : BaseColor, new() => Colors.Select(x => new T()
        {
            Red = x.R / 255f,
            Green = x.G / 255f,
            Blue = x.B / 255f,
            Alpha = x.A / 255f,
        }).ToArray();
    }
}