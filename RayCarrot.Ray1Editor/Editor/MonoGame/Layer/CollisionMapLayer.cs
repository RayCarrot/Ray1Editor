using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public abstract class CollisionMapLayer<T> : TileMapLayer<T>
    {
        protected CollisionMapLayer(T[] tileMap, Point position, Point mapSize, TileSet tileSet) : base(tileMap, position, mapSize, tileSet)
        {
            IsVisible = false;
        }

        public override string Name => "Collision";
    }
}