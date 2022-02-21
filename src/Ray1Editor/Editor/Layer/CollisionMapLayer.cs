using Microsoft.Xna.Framework;

namespace Ray1Editor;

public abstract class CollisionMapLayer<T> : TileMapLayer<T>
{
    protected CollisionMapLayer(T[] tileMap, Point position, Point mapSize, TileSet tileSet) : base(tileMap, position, mapSize, tileSet)
    {
        IsVisible = false;
    }

    public override LayerType Type => LayerType.Collision;
    public override string Name => "Collision";
}