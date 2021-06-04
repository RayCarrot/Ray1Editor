using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public class Palette
    {
        // TODO: Support wrap (such as for GBA 16x16 palettes), get color using method

        public Palette(IEnumerable<BaseColor> colors)
        {
            Colors = colors.Select(x => new Color(x.Red, x.Green, x.Blue, x.Alpha)).ToArray();
        }

        public Palette(IList<Color> colors)
        {
            Colors = colors;
        }

        public IList<Color> Colors { get; }

        public void SetFirstToTransparent()
        {
            Colors[0] = Color.Transparent;
        }
    }
}