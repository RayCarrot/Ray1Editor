using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
using BinarySerializer.PS1;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    public class R1_PS1_GameManager : R1_GameManager
    {
        #region Paths

        public string Path_ExeFile => "SLUS-000.05"; // TODO: Different for each release!

        #endregion

        #region Manager

        public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path) => GetLevels(((Games.R1_Game)game).EngineVersion);

        public override GameData Load(Context context, object settings, TextureManager textureManager)
        {
            // Get settings
            var ray1Settings = (Ray1Settings)settings;
            var game = (Games.R1_Game)context.GetSettings<Games.Game>();

            // Add the settings
            context.AddSettings(ray1Settings);

            var exeAddress = 0x80125000 - 0x800; // TODO: Different for each release!
            var exeConfig = PS1_ExecutableConfig.PS1_US; // TODO: Different for each release!

            // Add the exe file
            context.AddFile(new MemoryMappedFile(context, Path_ExeFile, exeAddress)
            {
                RecreateOnWrite = false
            });

            context.AddPreDefinedPointers(PS1_DefinedPointers.PS1_US); // TODO: Different for each release!

            // Create the data
            var data = new R1_PS1_GameData(context, textureManager);

            // Read the exe file
            var exe = FileFactory.Read<PS1_Executable>(Path_ExeFile, context, onPreSerialize: (_, o) => o.Pre_PS1_Config = exeConfig);

            var worldIndex = (int)ray1Settings.World - 1;
            var lvlIndex = ray1Settings.Level - 1;

            // Get file entries
            var fileEntryFix = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.filefxs)];
            var fileEntryworld = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.wld_file) + worldIndex];
            var fileEntrylevel = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.map_file) + (worldIndex * 21 + lvlIndex)];

            // Add and read allfix
            LoadFile(context, fileEntryFix);
            var fix = FileFactory.Read<PS1_AllfixFile>(fileEntryFix.ProcessedFilePath, context);

            // Add and read world
            LoadFile(context, fileEntryworld);
            var wld = FileFactory.Read<PS1_WorldFile>(fileEntryworld.ProcessedFilePath, context);

            // Add and read level
            LoadFile(context, fileEntrylevel);
            var lev = FileFactory.Read<PS1_LevFile>(fileEntrylevel.ProcessedFilePath, context);

            // Initialize the random generation
            InitRandom(data);

            // Load the editor name tables
            var desNames = LoadNameTable_DES(data, game);
            var etaNames = LoadNameTable_ETA(data, game);

            // Load VRAM
            var vram = LoadVRAM(fix, wld, lev);

            // Load palettes
            LoadPalettes(data, vram, wld);

            // Load fond (background)
            LoadFond(data, exe, textureManager);

            // Load map
            LoadMap(data, wld, lev.MapData, textureManager);

            // Load DES (sprites & animations)
            LoadDES(data, vram, lev.ObjData, desNames, textureManager);

            // Load ETA (states)
            LoadETA(data, lev.ObjData, etaNames);

            // Load the editor event definitions
            LoadEditorEventDefinitions(data);

            // Load objects
            LoadObjects(data, lev.ObjData);

            // Load Rayman
            LoadRayman(data);

            return data;
        }

        public override void Save(Context context, GameData gameData)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region R1 Manager

        protected override int MaxObjType => (int)ObjType.TYPE_PLANCHES;

        public void LoadFile(Context context, PS1_FileTableEntry file)
        {
            context.AddFile(new PS1MemoryMappedFile(context, file.ProcessedFilePath, file.MemoryAddress, PS1MemoryMappedFile.InvalidPointerMode.DevPointerXOR, fileLength: file.FileSize)
            {
                RecreateOnWrite = false
            });
        }

        protected virtual Dictionary<string, Dictionary<string, DESPointers>> LoadNameTable_DES(R1_GameData data, Games.R1_Game game)
        {
            return LoadEditorNameTable<Dictionary<string, Dictionary<string, DESPointers>>>(game.NameTablesName, EditorNameTableType.DES);
        }

        protected virtual Dictionary<string, Dictionary<string, uint>> LoadNameTable_ETA(R1_GameData data, Games.R1_Game game)
        {
            return LoadEditorNameTable<Dictionary<string, Dictionary<string, uint>>>(game.NameTablesName, EditorNameTableType.ETA);
        }

        public PS1_VRAM LoadVRAM(PS1_AllfixFile allfix, PS1_WorldFile world, PS1_LevFile lev)
        {
            return PS1VramHelpers.PS1_FillVRAM(PS1VramHelpers.VRAMMode.Level, allfix, world, null, lev, null, true); // TODO: Set to not be US version for other versions!
        }

        public void LoadPalettes(R1_PS1_GameData data, PS1_VRAM vram, PS1_WorldFile wld)
        {
            // Add tile palettes
            data.PS1_TilePalettes = wld.TilePalettes.Select((x, i) => new Palette(x, $"Tile Palette {i}")).ToArray();

            foreach (var pal in data.PS1_TilePalettes)
                data.TextureManager.AddPalette(pal);

            // Add sprite palettes
            data.PS1_SpritePalettes = vram.Palettes.Select(x => new R1_PS1_GameData.LoadedPalette(x.Colors, new Palette(x.Colors, $"Sprite Palette {x.Y}"))).ToArray();

            foreach (var pal in data.PS1_SpritePalettes)
                data.TextureManager.AddPalette(pal.Palette);
        }

        public void LoadDES(R1_PS1_GameData data, PS1_VRAM vram, PS1_ObjBlock objData, Dictionary<string, Dictionary<string, DESPointers>> nameTable, TextureManager textureManager)
        {
            foreach (var des in GetLevelDES(data.Context, objData.Objects, nameTable))
            {
                if (des.SpritesPointer != null && data.DES.All(x => x.SpritesData[0].Offset != des.SpritesPointer))
                {
                    // Read or get the data
                    var s = data.Context.Deserializer;
                    var sprites = des.EventData?.Sprites ?? s.DoAt(des.SpritesPointer, () => s.SerializeObjectArray<Sprite>(default, des.ImageDescriptorCount, name: $"ImageDescriptors"));
                    var animations = des.EventData?.Animations ?? s.DoAt(des.AnimationsPointer, () => s.SerializeObjectArray<Animation>(default, des.AnimationsCount, name: $"AnimationDescriptors"));
                    var imgBuffer = des.EventData?.ImageBuffer ?? s.DoAt(des.ImageBufferPointer, () => s.SerializeArray<byte>(default, des.ImageBufferLength ?? 0, name: $"ImageBuffer"));

                    // Create the sprite sheet
                    var spriteSheet = new PalettedTextureSheet(textureManager, sprites.Select(x => x.IsDummySprite() ? (Point?)null : new Point(x.Width, x.Height)).ToArray());

                    // Initialize the sprites
                    for (var i = 0; i < sprites.Length; i++)
                    {
                        // Ignore dummy sprites
                        if (spriteSheet.Entries[i] == null)
                            continue;

                        InitializeSprite(data, vram, sprites[i], spriteSheet, i);
                    }

                    // Get the DES name
                    var desName = des.Name ?? nameTable?.TryGetValue(des.SpritesPointer?.File.FilePath)?.FirstOrDefault(x =>
                        x.Value.ImageDescriptors == des.SpritesPointer?.AbsoluteOffset &&
                        x.Value.AnimationDescriptors == des.AnimationsPointer?.AbsoluteOffset &&
                        (!x.Value.ImageBuffer.HasValue || x.Value.ImageBuffer == des.ImageBufferPointer?.AbsoluteOffset)).Key;

                    data.AddDES(new R1_GameData.DESData(sprites, spriteSheet, animations, animations.Select(x => ToCommonAnimation(x)).ToArray(), imgBuffer)
                    {
                        Name = desName
                    });
                }
            }
        }

        public virtual void InitializeSprite(R1_PS1_GameData data, PS1_VRAM vram, Sprite sprite, PalettedTextureSheet sheet, int index)
        {
            // Get the palette x and y offsets in the vram
            var palX = sprite.PaletteX * 16; // Each palette section is 16 colors
            var palY = sprite.PaletteY;

            const int pageWidth = 256; // 256 colors

            // If the offset is for a palette section we need to get the start of the full palette, and then the section index
            var palStartX = palX / pageWidth * pageWidth;
            var palOffset = palX % palStartX;

            // Find the palette in the vram
            var vramPal = vram.Palettes.FirstOrDefault(p => p.X == palStartX * 2 && p.Y == palY);

            // Make sure a palette was found
            if (vramPal == null)
                throw new Exception($"Failed to initialize sprite from {sprite.Offset} due to not finding a matching palette in the VRAM at ({palX}, {palY})");

            // Get the loaded palette
            var pal = data.PS1_SpritePalettes.First(x => x.Colors == vramPal.Colors).Palette;

            int pageX = sprite.TexturePageInfo.TX;
            int pageY = sprite.TexturePageInfo.TY;

            var is4Bit = sprite.TexturePageInfo.TP == PS1_TexturePageInfo.TexturePageTP.CLUT_4Bit;
            var w = is4Bit ? (int)Math.Ceiling(sprite.Width / 2f) : sprite.Width;
            var length = sprite.Width * sprite.Height;

            if (is4Bit)
                length = (int)Math.Ceiling(length / 2f);

            var imgData = new byte[length];

            for (int y = 0; y < sprite.Height; y++)
            {
                var imgDataYOffset = y * sprite.Width;

                for (int x = 0; x < sprite.Width; x++)
                {
                    var offsetX = sprite.ImageOffsetInPageX + x;

                    if (is4Bit)
                        offsetX /= 2;

                    var imgDataOffset = imgDataYOffset + x;

                    if (is4Bit)
                        imgDataOffset /= 2;

                    imgData[imgDataOffset] = vram.GetPixel8(pageX, pageY, offsetX, sprite.ImageOffsetInPageY + y);

                    if (is4Bit)
                        x++;
                }
            }

            var format = sprite.TexturePageInfo.TP switch
            {
                PS1_TexturePageInfo.TexturePageTP.CLUT_4Bit => PalettedTextureData.ImageFormat.Linear_4bpp,
                PS1_TexturePageInfo.TexturePageTP.CLUT_8Bit => PalettedTextureData.ImageFormat.Linear_8bpp,
                _ => throw new Exception($"Unsupported texture type {sprite.TexturePageInfo.TP}")
            };

            sheet.InitEntry(index, pal, imgData, format: format, paletteOffset: palOffset);
        }

        protected IEnumerable<DES> GetLevelDES(Context context, IEnumerable<ObjData> events, Dictionary<string, Dictionary<string, DESPointers>> nameTable_R1PS1DES)
        {
            return events.Select(x => new DES
            {
                SpritesPointer = x.SpritesPointer,
                AnimationsPointer = x.AnimationsPointer,
                ImageBufferPointer = x.ImageBufferPointer,
                ImageDescriptorCount = x.SpritesCount,
                AnimationsCount = x.AnimationsCount,
                ImageBufferLength = null,
                Name = null,
                EventData = x
            }).Concat(nameTable_R1PS1DES?.Where(d => context.FileExists(d.Key)).SelectMany(d => d.Value.Select(des => new DES
            {
                SpritesPointer = des.Value.ImageDescriptors != null ? new Pointer(des.Value.ImageDescriptors.Value, context.GetFile(d.Key)) : null,
                AnimationsPointer = des.Value.AnimationDescriptors != null ? new Pointer(des.Value.AnimationDescriptors.Value, context.GetFile(d.Key)) : null,
                ImageBufferPointer = des.Value.ImageBuffer != null ? new Pointer(des.Value.ImageBuffer.Value, context.GetFile(d.Key)) : null,
                ImageDescriptorCount = des.Value.ImageDescriptorsCount,
                AnimationsCount = des.Value.AnimationDescriptorsCount,
                ImageBufferLength = des.Value.ImageBufferLength,
                Name = des.Key,
                EventData = null
            })) ?? new DES[0]);
        }

        public void LoadETA(R1_GameData data, PS1_ObjBlock objBlock, Dictionary<string, Dictionary<string, uint>> nameTable)
        {
            foreach (var eta in GetLevelETA(data.Context, objBlock.Objects, nameTable))
            {
                // Add if not found
                if (eta.ETAPointer != null && data.ETA.All(x => x.ETA.Offset != eta.ETAPointer))
                {
                    var etaName = eta.Name ?? nameTable?.TryGetValue(eta.ETAPointer.File.FilePath)?.FirstOrDefault(x => x.Value == eta.ETAPointer.AbsoluteOffset).Key;

                    var s = data.Context.Deserializer;

                    var etaObj = eta.EventData?.ETA ?? s.DoAt(eta.ETAPointer, () => s.SerializeObject<BinarySerializer.Ray1.ETA>(default, name: $"ETA"));

                    // Add to the ETA
                    data.ETA.Add(new R1_GameData.ETAData(etaObj)
                    {
                        Name = etaName
                    });
                }
            }
        }

        protected IEnumerable<ETA> GetLevelETA(Context context, IEnumerable<ObjData> events, Dictionary<string, Dictionary<string, uint>> nameTable_R1PS1ETA)
        {
            return events.Select(x => new ETA
            {
                ETAPointer = x.ETAPointer,
                Name = null,
                EventData = x
            }).Concat(nameTable_R1PS1ETA?.Where(d => context.FileExists(d.Key)).SelectMany(d => d.Value.Select(des => new ETA
            {
                ETAPointer = new Pointer(des.Value, context.GetFile(d.Key)),
                Name = des.Key,
                EventData = null
            })) ?? new ETA[0]);
        }

        public void LoadObjects(R1_PS1_GameData data, PS1_ObjBlock objBlock)
        {
            data.Objects.AddRange(objBlock.Objects.Select(obj => new R1_GameObject(obj, FindMatchingEventDefinition(data, obj))));

            data.LinkTable = objBlock.ObjectLinkingTable.Select(x => (ushort)x).ToArray();
            InitLinkGroups(data.Objects, data.LinkTable);
        }

        public void LoadMap(R1_PS1_GameData data, PS1_WorldFile wld, MapData map, TextureManager textureManager)
        {
            var tileSetWidth = 16; // TODO: Different for each release!
            var tileSet = LoadTileSet(data, wld, textureManager);
            LoadMap(data, textureManager, tileSet, map.Tiles, map.Width, map.Height, map.Offset, tileSetWidth);
        }

        public TileSet LoadTileSet(R1_PS1_GameData data, PS1_WorldFile wld, TextureManager textureManager)
        {
            var tileSetWidth = 16; // TODO: Different for each release!
            var tileSize = new Point(Ray1Settings.CellSize);

            var width = tileSize.X * tileSetWidth;
            var height = (wld.PalettedTiles.Length) / width;
            var texture = new Texture2D(textureManager.GraphicsDevice, width, height);

            var tileSheet = PalettedTextureSheet.FromTileSet(textureManager, texture, tileSize, wld.TilePaletteIndexTable.Length);

            var tileSet = new TileSet(tileSheet, tileSize);

            for (int i = 0; i < tileSet.TileSheet.Entries.Length; i++)
            {
                var palette = data.PS1_TilePalettes[wld.TilePaletteIndexTable[i]];

                var imgData = new byte[tileSet.TileSize.X * tileSet.TileSize.Y];

                var tileSetX = (i % tileSetWidth) * tileSet.TileSize.X;
                var tileSetY = (i / tileSetWidth) * tileSet.TileSize.Y;

                for (int y = 0; y < tileSet.TileSize.Y; y++)
                {
                    for (int x = 0; x < tileSet.TileSize.X; x++)
                    {
                        imgData[y * tileSet.TileSize.X + x] = wld.PalettedTiles[(tileSetY + y) * width + (tileSetX + x)];
                    }
                }

                tileSheet.InitEntry(i, palette, imgData);
            }

            return tileSet;
        }

        public void LoadFond(R1_PS1_GameData data, PS1_Executable exe, TextureManager textureManager)
        {
            var context = data.Context;
            var settings = context.GetSettings<Ray1Settings>();

            // Get the current fond
            var fndIndex = exe.PS1_LevelBackgroundIndexTable[(int)settings.World - 1][settings.Level - 1];

            var fndCount = (int)exe.Pre_PS1_Config.FileTableInfos.First(x => x.FileType == PS1_FileType.fnd_file).Count;

            var backgrounds = Enumerable.Range(0, fndCount).Select(x =>
            {
                // Get the file entry
                var fndFileEntry = exe.PS1_FileTable[exe.GetFileTypeIndex(exe.Pre_PS1_Config, PS1_FileType.fnd_file) + x];
                
                // Have the background loading be in a func so it only loads when the background gets requested. Otherwise the 
                // initial load will be too slow since the PS1 version has a lot of backgrounds, and unlike the PC version
                // they're not split up into worlds.
                Texture2D getTex()
                {
                    using (context)
                    {
                        // Add the file to the context
                        LoadFile(context, fndFileEntry);

                        // Read the file
                        var fnd = FileFactory.Read<PS1_BackgroundVignetteFile>(fndFileEntry.ProcessedFilePath, context);
                        var img = fnd.ImageBlock;

                        var texture = LoadFond(img, settings, textureManager);

                        // Remove the file from the context
                        context.RemoveFile(fndFileEntry.ProcessedFilePath);

                        return texture;
                    }
                }

                return new BackgroundLayer.BackgroundEntry(getTex, null, fndFileEntry.ProcessedFilePath);
            }).ToArray();

            // Add the layer
            data.Layers.Add(new BackgroundLayer(backgrounds, Point.Zero, fndIndex, name: "Background"));
        }

        public Texture2D LoadFond(PS1_VignetteBlockGroup img, Ray1Settings settings, TextureManager textureManager)
        {
            // Get the block width
            var blockWidth = img.GetBlockWidth(settings.EngineVersion);

            // Create the texture
            var texture = textureManager.CreateTexture(img.Width, img.Height);

            var pixels = new Color[img.Width * img.Height];

            // Handle each block
            for (int blockIndex = 0; blockIndex < img.ImageBlocks.Length; blockIndex++)
            {
                // Get the block data
                var blockData = img.ImageBlocks[blockIndex];

                // Handle the block
                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < blockWidth; x++)
                    {
                        // Get the color
                        var c = blockData[x + (y * blockWidth)];

                        c.Alpha = Byte.MaxValue;

                        var imgX = x + (blockIndex * blockWidth);
                        var imgY = y;

                        // Set the pixel
                        pixels[imgY * img.Width + imgX] = new Color(c.Red, c.Green, c.Blue);
                    }
                }
            }

            // Set the texture data
            texture.SetData(pixels);

            return texture;
        }

        #endregion

        #region Data Types

        public class DESPointers
        {
            public uint? ImageDescriptors { get; set; }
            public ushort ImageDescriptorsCount { get; set; }
            public uint? AnimationDescriptors { get; set; }
            public byte AnimationDescriptorsCount { get; set; }
            public uint? ImageBuffer { get; set; }
            public uint ImageBufferLength { get; set; }
        }

        protected class DES
        {
            public Pointer SpritesPointer { get; set; }
            public Pointer AnimationsPointer { get; set; }
            public Pointer ImageBufferPointer { get; set; }
            public ushort ImageDescriptorCount { get; set; }
            public byte AnimationsCount { get; set; }
            public uint? ImageBufferLength { get; set; }
            public string Name { get; set; }
            public ObjData EventData { get; set; }
        }
        protected class ETA
        {
            public Pointer ETAPointer { get; set; }
            public string Name { get; set; }
            public ObjData EventData { get; set; }
        }

        #endregion
    }
}