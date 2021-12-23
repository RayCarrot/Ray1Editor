using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor;

public class ObjAnimation_SpriteLayer
{
    public int SpriteIndex { get; set; }

    public Point Position { get; set; }

    public bool IsFlippedHorizontally { get; set; }
    public bool IsFlippedVertically { get; set; }
}