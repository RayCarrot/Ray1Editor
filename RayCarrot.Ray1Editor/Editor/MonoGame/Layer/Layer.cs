using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A base layer element
    /// </summary>
    public abstract class Layer : EditorElement
    {
        public abstract string Name { get; }

        /// <summary>
        /// The position and size of the layer
        /// </summary>
        public abstract Rectangle Rectangle { get; }

        public abstract IEnumerable<EditorFieldViewModel> GetFields();

        public virtual void Update() { }
        public abstract void Draw(SpriteBatch s);
    }
}