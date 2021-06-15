using BinarySerializer;

namespace RayCarrot.Ray1Editor
{
    public class R1_PS1_GameData : R1_GameData
    {
        public R1_PS1_GameData(Context context, TextureManager textureManager) : base(context, textureManager) { }

        public Palette[] PS1_TilePalettes { get; set; }
        public LoadedPalette[] PS1_SpritePalettes { get; set; }

        public record LoadedPalette(RGBA5551Color[] Colors, Palette Palette);
    }
}