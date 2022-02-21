using Microsoft.Xna.Framework.Graphics;

namespace Ray1Editor;

public class EditorTextures
{
    public EditorTextures(TextureManager textureManager)
    {
        TextureManager = textureManager;
    }

    public TextureManager TextureManager { get; }
    public Texture2D Icon_Object { get; protected set; }
    public Texture2D Icon_Trigger { get; protected set; }
    public Texture2D Icon_WayPoint { get; protected set; }

    public void Init()
    {
        Icon_Object = Assets.GetTextureAsset("Assets/Object/object.png", TextureManager);
        Icon_Trigger = Assets.GetTextureAsset("Assets/Object/trigger.png", TextureManager);
        Icon_WayPoint = Assets.GetTextureAsset("Assets/Object/waypoint.png", TextureManager);
    }
}