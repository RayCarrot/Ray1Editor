using BinarySerializer;
using BinarySerializer.Ray1;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Rayman 1 game data
    /// </summary>
    public class R1_GameData : GameData
    {
        #region Constructor

        public R1_GameData(Context context, TextureManager textureManager) : base(context, textureManager)
        {
            ObjTemplates = new Dictionary<WldObjType, ObjData>();
            Sprites = new Dictionary<SpriteCollection, PalettedTextureSheet>();
            Animations = new Dictionary<AnimationCollection, ObjAnimation[]>();
            DES = new List<DESData>();
            ETA = new List<ETAData>();
        }

        #endregion

        #region Public Properties

        // Random number generation
        public ushort[] RandomArray { get; set; }
        public byte RandomIndex { get; set; }

        // Objects
        public ushort[] LinkTable { get; set; }
        public Dictionary<WldObjType, ObjData> ObjTemplates { get; }
        public ObjData Rayman { get; set; }
        public R1_EventDefinition[] EventDefinitions { get; set; }

        // DES
        public List<DESData> DES { get; }
        public Dictionary<SpriteCollection, PalettedTextureSheet> Sprites { get; }
        public Dictionary<AnimationCollection, ObjAnimation[]> Animations { get; }

        // ETA
        public List<ETAData> ETA { get; }

        #endregion

        #region Public Methods

        public PalettedTextureSheet GetSprites(SpriteCollection sprites) => Sprites.TryGetValue(sprites);
        public ObjAnimation[] GetAnimations(AnimationCollection anims) => Animations.TryGetValue(anims);

        public ushort GetNextRandom(int max)
        {
            RandomIndex++;
            return (ushort)(RandomArray[RandomIndex % RandomArray.Length] % max);
        }

        public void AddDES(DESData des)
        {
            DES.Add(des);

            if (des == null)
                return;

            Sprites[des.SpritesData] = des.Sprites;
            Animations[des.AnimationsData] = des.Animations;
        }

        #endregion

        #region Data Types

        public class DESData
        {
            public DESData(SpriteCollection spritesData, PalettedTextureSheet sprites, AnimationCollection animationsData, ObjAnimation[] animations, byte[] imageBuffer)
            {
                SpritesData = spritesData;
                Sprites = sprites;
                AnimationsData = animationsData;
                Animations = animations;
                ImageBuffer = imageBuffer;
            }

            public SpriteCollection SpritesData { get; }
            public PalettedTextureSheet Sprites { get; }
            public AnimationCollection AnimationsData { get; }
            public ObjAnimation[] Animations { get; }
            public byte[] ImageBuffer { get; }
            public string Name { get; init; }
        }

        public class ETAData
        {
            public ETAData(ETA eta)
            {
                ETA = eta;
            }

            public ETA ETA { get; }
            public string Name { get; init; }
        }

        public enum WldObjType
        {
            Ray, // Template for Rayman
            RayLittle, // Template for small Rayman
            ClockObj, // The game over clock
            DivObj, // ?
            MapObj, // 24 map objects
        }

        #endregion
    }
}