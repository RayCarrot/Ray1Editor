using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A texture sheet, containing multiple textures. This is useful to avoid performance issues when dealing
    /// with multiple small textures, such as sprites and tiles.
    /// </summary>
    public class TextureSheet : IDisposable
    {
        protected TextureSheet(TextureManager manager, IList<Point?> dimensions, int sheetWidth = 1024)
        {
            manager.AddTextureSheet(this);

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

            Sheet = new Texture2D(manager.GraphicsDevice, sheetWidth, height);

            Logger.Log(LogLevel.Trace, "Created texture sheet with {0} entries and a size of {1}", Entries.Length, Sheet.Bounds.Size);
        }

        public TextureSheet(TextureManager manager, string path, IEnumerable<Rectangle> textures)
        {
            manager.AddTextureSheet(this);

            using var stream = Assets.GetAsset(path);
            Sheet = Texture2D.FromStream(manager.GraphicsDevice, stream);
            Entries = textures.Select(x => new Entry(x)).ToArray();
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Texture2D Sheet { get; }
        public Entry[] Entries { get; }

        public void Dispose()
        {
            Sheet?.Dispose();
        }

        public class Entry
        {
            public Entry(Rectangle source)
            {
                Source = source;
            }

            public Rectangle Source { get; }
        }
    }
}