using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A texture sheet, containing multiple textures. This is useful to avoid performance issues when dealing
    /// with multiple small textures, such as sprites and tiles. The textures get mapped to the sheet on
    /// construction and set on <see cref="InitEntry"/>.
    /// </summary>
    public class TextureSheet
    {
        public TextureSheet(GraphicsDevice g, IList<Point?> dimensions, int sheetWidth = 1024)
        {
            Entries = new Entry[dimensions.Count];
            
            var x = 0;
            var y = 0;
            var maxHeight = 0;

            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i] == null)
                    continue;

                var (w, h) = dimensions[i].Value;

                if (w > sheetWidth)
                    throw new Exception($"Texture is wider than the sheet width");

                // Check if we need to go to a new line
                if (x + w > sheetWidth)
                {
                    y += maxHeight;
                    x = 0;
                    maxHeight = 0;
                }

                if (h > maxHeight)
                    maxHeight = h;

                Entries[i] = new Entry(new Rectangle(x, y, w, h));
                
                x += w;
            }

            var height = y + maxHeight;

            Sheet = new Texture2D(g, sheetWidth, height);
        }

        public Texture2D Sheet { get; }
        public Entry[] Entries { get; }

        public void InitEntry(Entry entry, Palette palette, IEnumerable<byte> imgData, int imgDataLength, ImageFormat format = ImageFormat.BPP_8)
        {
            if (format == ImageFormat.BPP_8)
            {
                Sheet.SetData<Color>(0, entry.Source, imgData.Select(x => palette.Colors[x]).ToArray(), 0, imgDataLength);
            }
            else
            {
                throw new Exception($"Image format {format} is not supported");
            }
        }

        public class Entry
        {
            public Entry(Rectangle source)
            {
                Source = source;
            }

            public Rectangle Source { get; }
        }

        public enum ImageFormat
        {
            BPP_8,
        }
    }
}