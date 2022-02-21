using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
using Microsoft.Xna.Framework;

namespace Ray1Editor;

public class SerializablePalette<T> : Palette
    where T : BaseColor, new()
{
    public SerializablePalette(IList<T> colors, string name) : base(colors.Select(x => new Color(x.Red, x.Green, x.Blue, x.Alpha)).ToArray(), colors.FirstOrDefault()?.Offset, name)
    {
        SerializableColors = colors;
    }

    public IList<T> SerializableColors { get; }

    public override void Update()
    {
        // Update base
        base.Update();

        for (int i = 0; i < SerializableColors.Count; i++)
        {
            SerializableColors[i] = new T()
            {
                Red = Colors[i].R / 255f,
                Green = Colors[i].G / 255f,
                Blue = Colors[i].B / 255f,
                Alpha = Colors[i].A / 255f,
            };
        }
    }
}