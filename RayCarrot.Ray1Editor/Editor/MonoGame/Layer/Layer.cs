using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A base layer element
    /// </summary>
    public abstract class Layer : EditorElement
    {
        /// <summary>
        /// The layer name to display
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The position and size of the layer
        /// </summary>
        public abstract Rectangle Rectangle { get; }

        /// <summary>
        /// Indicates if the layer is currently visible
        /// </summary>
        public bool IsVisible { get; set; }

        public abstract IEnumerable<EditorFieldViewModel> GetFields();

        public virtual void Update() { }
        public abstract void Draw(SpriteBatch s);
    }
}