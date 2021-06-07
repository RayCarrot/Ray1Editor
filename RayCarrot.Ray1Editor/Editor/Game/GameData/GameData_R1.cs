using BinarySerializer;
using BinarySerializer.Ray1;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Rayman 1 game data
    /// </summary>
    public class GameData_R1 : GameData
    {
        public GameData_R1(Context context) : base(context)
        {
            Sprites = new Dictionary<Sprite[], PalettedTextureSheet>();
            Animations = new Dictionary<Animation[], ObjAnimation[]>();
            ETA = new List<ETA>();
        }

        /// <summary>
        /// The loaded sprite sheets for each sprite array
        /// </summary>
        public Dictionary<Sprite[], PalettedTextureSheet> Sprites { get; }

        /// <summary>
        /// The loaded editor animations for each animation array
        /// </summary>
        public Dictionary<Animation[], ObjAnimation[]> Animations { get; }
        
        /// <summary>
        /// The loaded ETA
        /// </summary>
        public List<ETA> ETA { get; }

        // TODO: Create GameData_R1_PC and store these properties there
        public PC_DES[] PC_DES { get; set; }
        public Animation[][] PC_LoadedAnimations { get; set; }
        public Palette[] PC_Palettes { get; set; }

        public override IEnumerable<Palette> Palettes => PC_Palettes;
    }
}