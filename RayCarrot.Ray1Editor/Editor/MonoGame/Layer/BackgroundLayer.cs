using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    // TODO: Allowing tiling (i.e. duplicate texture horizontally and/or vertically to fill map)

    /// <summary>
    /// A background layer, showing a single texture
    /// </summary>
    public class BackgroundLayer : Layer
    {
        // TODO: Pass in list of available backgrounds to select, each with a name
        public BackgroundLayer(Texture2D texture, Point position, string name = "Background")
        {
            Texture = texture;
            Rectangle = new Rectangle(position, Texture.Bounds.Size);
            Name = name;

            IsVisible = true;
        }

        public override string Name { get; }
        public override Rectangle Rectangle { get; }
        public override bool CanEdit => false;
        public Texture2D Texture { get; }
        public override IEnumerable<EditorFieldViewModel> GetFields()
        {
            // TODO: Add background selection which differs per game
            return new EditorFieldViewModel[0];
        }

        public override void Draw(SpriteBatch s)
        {
            s.Draw(Texture, Rectangle, Color.White);
        }
    }
}