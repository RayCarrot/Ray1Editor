using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A tile set layer to be used with a <see cref="TileMapLayer{T}"/>
    /// </summary>
    /// <typeparam name="T">The tile type</typeparam>
    public class TileSetLayer<T> : TileMapBaseLayer<T>
    {
        #region Constructor

        public TileSetLayer(T[] tileMap, Point position, Point mapSize, TileSet tileSet, Func<T, int> getTileSetIndexFunc, Func<T, T, T> cloneTileFunc) : base(tileMap, position, mapSize, tileSet)
        {
            GetTileSetIndexFunc = getTileSetIndexFunc;
            CloneTileFunc = cloneTileFunc;
        }

        #endregion

        #region Protected Properties

        public Func<T, int> GetTileSetIndexFunc { get; }
        public Func<T, T, T> CloneTileFunc { get; }

        #endregion

        #region Public Properties

        public override bool CanEdit => false;
        public override LayerType Type => LayerType.Other;
        public override string Name => $"TileSet Map (read-only)";

        #endregion

        #region Protected Methods

        protected override int GetTileSetIndex(T tile) => GetTileSetIndexFunc(tile);
        protected override T CloneTile(T srcTile, T destTile) => CloneTileFunc(srcTile, destTile);

        #endregion

        #region Public Methods

        public override IEnumerable<EditorFieldViewModel> GetFields() => new EditorFieldViewModel[0];

        #endregion
    }
}