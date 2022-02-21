using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Point = Microsoft.Xna.Framework.Point;

namespace Ray1Editor;

/// <summary>
/// A tile set
/// </summary>
public class TileSet
{
    public TileSet(TextureSheet tileSheet, Point tileSize, bool isFirstTransparent = true)
    {
        TileSheet = tileSheet;
        TileSize = tileSize;
        FullyTransparentTiles = new HashSet<int>();

        if (isFirstTransparent)
            FullyTransparentTiles.Add(0);
    }

    public TextureSheet TileSheet { get; }
    public Point TileSize { get; }
    protected HashSet<int> FullyTransparentTiles { get; }
    public bool IsFullyTransparent(int index) => FullyTransparentTiles.Contains(index);

    // IDEA: Use this? It takes around 250ms for a tile-set, but improves performance in the editor and is more reliable than the isFirstTransparent param
    public void FindTransparentTiles()
    {
        for (var i = 0; i < TileSheet.Entries.Length; i++)
        {
            var entry = TileSheet.Entries[i];
            var c = new Color[entry.Source.Width * entry.Source.Height];

            TileSheet.Sheet.GetData(0, entry.Source, c, 0, c.Length);

            if (c.All(x => x == Color.Transparent))
                FullyTransparentTiles.Add(i);
        }
    }
}