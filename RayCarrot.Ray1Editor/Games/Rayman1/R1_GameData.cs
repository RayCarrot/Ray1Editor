using BinarySerializer;
using BinarySerializer.Ray1;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Rayman 1 game data
    /// </summary>
    public class R1_GameData : GameData
    {
        public R1_GameData(Context context, TextureManager textureManager) : base(context, textureManager)
        {
            Sprites = new Dictionary<Sprite, PalettedTextureSheet>();
            Animations = new Dictionary<Animation, ObjAnimation[]>();
            DES = new List<DESData>();
            ETA = new List<ETAData>();
        }

        public ushort[] RandomArray { get; set; }
        public byte RandomIndex { get; set; }

        /// <summary>
        /// The loaded link table
        /// </summary>
        public ushort[] LinkTable { get; set; }

        public ObjData Rayman { get; set; }

        /// <summary>
        /// The available event definitions
        /// </summary>
        public R1_EventDefinition[] EventDefinitions { get; set; }

        /// <summary>
        /// The loaded DES
        /// </summary>
        public List<DESData> DES { get; }

        /// <summary>
        /// The loaded sprite sheets for each sprite array
        /// </summary>
        public Dictionary<Sprite, PalettedTextureSheet> Sprites { get; }

        /// <summary>
        /// The loaded editor animations for each animation array
        /// </summary>
        public Dictionary<Animation, ObjAnimation[]> Animations { get; }
        
        /// <summary>
        /// The loaded ETA
        /// </summary>
        public List<ETAData> ETA { get; }

        // TODO: Find better way of handling this. Right now we use the first entry in the sprites/animations array as the key. We can't use the array as the array instances can be different (PS1 for example where it's parsed per object - arrays are not cached, only objects).
        public PalettedTextureSheet GetSprites(Sprite[] sprites) => Sprites.TryGetValue(sprites.First());
        public ObjAnimation[] GetAnimations(Animation[] anims) => Animations.TryGetValue(anims.First());

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

            if (des.SpritesData.Any())
                Sprites[des.SpritesData.First()] = des.Sprites;
            if (des.AnimationsData.Any())
                Animations[des.AnimationsData.First()] = des.Animations;
        }

        public class DESData
        {
            public DESData(Sprite[] spritesData, PalettedTextureSheet sprites, Animation[] animationsData, ObjAnimation[] animations, byte[] imageBuffer)
            {
                SpritesData = spritesData;
                Sprites = sprites;
                AnimationsData = animationsData;
                Animations = animations;
                ImageBuffer = imageBuffer;
            }

            public Sprite[] SpritesData { get; }
            public PalettedTextureSheet Sprites { get; }
            public Animation[] AnimationsData { get; }
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
    }
}