using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    public class Renderer
    {
        public Renderer(Camera camera, SpriteBatch spriteBatch)
        {
            Camera = camera;
            SpriteBatch = spriteBatch;
        }

        public Camera Camera { get; }
        public SpriteBatch SpriteBatch { get; }

        public void Draw(Texture2D texture, Rectangle destinationRectangle)
        {
            if (!Camera.IsInVisibleArea(destinationRectangle))
                return;

            SpriteBatch.Draw(texture, destinationRectangle, Color.White);
        }
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle)
        {
            if (!Camera.IsInVisibleArea(destinationRectangle))
                return;

            SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            if (!Camera.IsInVisibleArea(destinationRectangle))
                return;

            SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White, rotation, origin, effects, layerDepth);
        }
    }
}