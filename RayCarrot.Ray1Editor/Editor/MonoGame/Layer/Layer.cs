﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

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
        /// Indicates if the layer can be edited
        /// </summary>
        public abstract bool CanEdit { get; }

        /// <summary>
        /// Indicates if the layer is currently selected for editing
        /// </summary>
        public bool IsSelected { get; protected set; }

        /// <summary>
        /// Indicates if the layer is currently visible
        /// </summary>
        public bool IsVisible { get; set; }

        public abstract IEnumerable<EditorFieldViewModel> GetFields();

        public virtual void Select()
        {
            IsSelected = true;
            IsVisible = true;

            if (Data == null)
                return;

            foreach (var l in Data.Layers.Where(x => x != this))
                l.IsSelected = false;
        }

        public virtual void Update(EditorUpdateData updateData) { }
        public virtual void UpdateLayerEditing(EditorUpdateData updateData) { }
        public virtual void ResetLayerEditing() { }
        public abstract void Draw(SpriteBatch s);
    }
}