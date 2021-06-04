namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Common animation frame data
    /// </summary>
    public class ObjAnimation_Frame
    {
        public ObjAnimation_Frame(ObjAnimation_SpriteLayer[] spriteLayers, ObjAnimation_HitBoxLayer[] collisionLayers = null)
        {
            SpriteLayers = spriteLayers;
            CollisionLayers = collisionLayers ?? new ObjAnimation_HitBoxLayer[0];
        }

        /// <summary>
        /// The sprite layers
        /// </summary>
        public ObjAnimation_SpriteLayer[] SpriteLayers { get; }

        /// <summary>
        /// The collision layers
        /// </summary>
        public ObjAnimation_HitBoxLayer[] CollisionLayers { get; }
    }
}