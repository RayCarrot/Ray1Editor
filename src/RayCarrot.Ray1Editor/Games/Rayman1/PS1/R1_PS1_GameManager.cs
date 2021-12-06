using BinarySerializer;
using BinarySerializer.PS1;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace RayCarrot.Ray1Editor
{
    public abstract class R1_PS1_GameManager : R1_GameManager
    {
        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region PS1 Properties

        public string Path_DiscXMLFile => "disc.xml";
        public abstract string Path_ExeFile { get; }
        public abstract long EXEBaseAddress { get; }
        public abstract PS1_ExecutableConfig EXEConfig { get; }
        public abstract Dictionary<PS1_DefinedPointer, long> DefinedPointers { get; }
        public abstract int TileSetWidth { get; }

        #endregion

        #region Manager

        public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path) => GetLevels(((Games.R1_Game)game).EngineVersion);

        public override IEnumerable<ActionViewModel> GetActions(GameData data)
        {
            yield return new ActionViewModel("Export VRAM", () =>
            {
                var file = R1EServices.UI.GetSaveFilePath("Export VRAM", "vram.png", ".png", "Image Files | *.png");

                if (file == null)
                    return;

                try
                {
                    var vram = ((R1_PS1_GameData)data).Vram;

                    using var bitmap = new Bitmap(16 * 128, 2 * 256);

                    for (int x = 0; x < 16 * 128; x++)
                    {
                        for (int y = 0; y < 2 * 256; y++)
                        {
                            byte val = vram.GetPixel8(0, y / 256, x, y % 256);
                            bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(val, val, val));
                        }
                    }

                    bitmap.Save(file);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex, "Exporting VRAM");
                    R1EServices.UI.DisplayMessage("An error occurred exporting the VRAM", "Error", DialogMessageType.Error);
                }
            });
        }

        public override GameData Load(Context context, object settings, TextureManager textureManager)
        {
            // Make sure valid files are specified for saving
            var userData = R1EServices.App.UserData;

            if (!File.Exists(userData.PS1_mkpsxisoPath) || !File.Exists(context.BasePath + Path_DiscXMLFile))
                R1EServices.UI.DisplayMessage("To be able to save changes made to the PS1 versions of Rayman 1 you will need " +
                                                        "to specify a file path for the mkpsxiso program. This can be done " +
                                                        $"in the settings window (Tools > Settings).{Environment.NewLine}" +
                                                        "In order for this to work the game folder for the game must contain every file " +
                                                        "from the disc, including the .str and .raw files (which can't be copied normally). " +
                                                        $"It must also contain a {Path_DiscXMLFile} file for the game files and the " +
                                                        $"license to use.", "Notice about PS1 saving", DialogMessageType.Information);

            // Get settings
            var ray1Settings = (Ray1Settings)settings;
            var game = (Games.R1_Game)context.GetSettings<Games.Game>();

            // Add the settings
            context.AddSettings(ray1Settings);

            var exeConfig = EXEConfig;

            // Add the exe file
            context.AddFile(new MemoryMappedFile(context, Path_ExeFile, EXEBaseAddress)
            {
                RecreateOnWrite = false
            });

            context.AddPreDefinedPointers(DefinedPointers);

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
            var levFile = FileFactory.Read<SerializableEditorFile<PS1_LevFile>>(fileEntrylevel.ProcessedFilePath, context);
            var lev = levFile.FileData;

            // Initialize the random generation
            InitRandom(data);

            // Load the editor name tables
            var desNames = LoadNameTable_DES(data, game, levFile.RelocatedStructs);
            var etaNames = LoadNameTable_ETA(data, game, levFile.RelocatedStructs);

            // Load VRAM
            LoadVRAM(data, fix, wld, lev);

            // Load palettes
            LoadPalettes(data, wld);

            // Load fond (background)
            LoadFond(data, exe, textureManager);

            // Load map
            LoadMap(data, wld, lev.MapData, textureManager);

            // Load DES (sprites & animations)
            LoadDES(data, lev.ObjData, desNames, textureManager);

            // Load ETA (states)
            LoadETA(data, lev.ObjData, etaNames);

            // Load the editor event definitions
            LoadEditorEventDefinitions(data);

            // Load objects
            LoadObjects(data, lev.ObjData);

            // Load the wld objects (object templates)
            LoadWldObj(data, fix.AllfixData);

            // Load Rayman
            LoadRayman(data);

            return data;
        }

        public override void Save(Context context, GameData gameData)
        {
            Logger.Log(LogLevel.Info, "Saving R1 PS1");

            var userData = R1EServices.App.UserData;

            if (!File.Exists(userData.PS1_mkpsxisoPath))
                throw new Exception("The specified file path for mkpsxiso is not valid");

            if (!File.Exists(context.BasePath + Path_DiscXMLFile))
                throw new Exception("The disc.xml file does not exist");

            // Get settings
            var settings = context.GetSettings<Ray1Settings>();

            var data = (R1_PS1_GameData)gameData;

            var worldIndex = (int)settings.World - 1;
            var lvlIndex = settings.Level - 1;

            // Get the exe
            var exe = context.GetMainFileObject<PS1_Executable>(Path_ExeFile);

            // Get file entries
            var fileEntryFix = exe.PS1_FileTable[exe.GetFileTypeIndex(exe.Pre_PS1_Config, PS1_FileType.filefxs)];
            var fileEntryworld = exe.PS1_FileTable[exe.GetFileTypeIndex(exe.Pre_PS1_Config, PS1_FileType.wld_file) + worldIndex];
            var fileEntrylevel = exe.PS1_FileTable[exe.GetFileTypeIndex(exe.Pre_PS1_Config, PS1_FileType.map_file) + (worldIndex * 21 + lvlIndex)];

            // Get the level data
            var lev = context.GetMainFileObject<SerializableEditorFile<PS1_LevFile>>(fileEntrylevel.ProcessedFilePath);

            // Save objects
            var objData = lev.FileData.ObjData;
            SaveObjects(data, objData);

            // Save the map
            SaveMap(data, lev.FileData.MapData);

            // Save background
            exe.PS1_LevelBackgroundIndexTable[(int)settings.World - 1][settings.Level - 1] = (byte)data.Layers.OfType<BackgroundLayer>().First().SelectedBackgroundIndex;

            Logger.Log(LogLevel.Info, "Data has been saved. Relocating level data.");

            // Relocate level data
            RelocateLevelData(data, lev, exe);

            Logger.Log(LogLevel.Info, "Level data has been relocated. Writing files.");

            // Save files
            FileFactory.Write<PS1_AllfixFile>(fileEntryFix.ProcessedFilePath, context);
            FileFactory.Write<PS1_WorldFile>(fileEntryworld.ProcessedFilePath, context);
            FileFactory.Write<SerializableEditorFile<PS1_LevFile>>(fileEntrylevel.ProcessedFilePath, context);

            // Update the file table
            UpdateFileTable(context, exe, userData.PS1_mkpsxisoPath, Path_DiscXMLFile);

            // Save the exe
            FileFactory.Write<PS1_Executable>(Path_ExeFile, context);

            // Create an ISO with the modified files
            CreateISO(context, userData.PS1_mkpsxisoPath, Path_DiscXMLFile);

            Logger.Log(LogLevel.Info, "Saved R1 PS1");
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

        public uint? GetActualPointer(uint? pointer, RelocatedStruct[] relocatedStructs)
        {
            if (relocatedStructs == null || pointer == null)
                return pointer;

            var reloc = relocatedStructs.FirstOrDefault(x => x.OriginalPointer.AbsoluteOffset == pointer);
            return (uint?)reloc?.NewPointer.AbsoluteOffset ?? pointer;
        }

        protected virtual Dictionary<string, Dictionary<string, DESPointers>> LoadNameTable_DES(R1_GameData data, Games.R1_Game game, RelocatedStruct[] relocatedStructs)
        {
            // Read the name tables
            var nameTables = LoadEditorNameTable<Dictionary<string, Dictionary<string, DESPointers>>>(game.NameTablesName, EditorNameTableType.DES);

            // Only keep the files we have loaded
            nameTables = nameTables.Where(x => data.Context.FileExists(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            // Update any relocated pointers
            foreach (var des in nameTables.Values.SelectMany(x => x.Values))
            {
                des.Sprites = GetActualPointer(des.Sprites, relocatedStructs);
                des.Animations = GetActualPointer(des.Animations, relocatedStructs);
                des.ImageBuffer = GetActualPointer(des.ImageBuffer, relocatedStructs);
            }

            return nameTables;
        }

        protected virtual Dictionary<string, Dictionary<string, uint>> LoadNameTable_ETA(R1_GameData data, Games.R1_Game game, RelocatedStruct[] relocatedStructs)
        {
            // Read the name tables
            var nameTables = LoadEditorNameTable<Dictionary<string, Dictionary<string, uint>>>(game.NameTablesName, EditorNameTableType.ETA);

            // Only keep the files we have loaded
            nameTables = nameTables.Where(x => data.Context.FileExists(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            // Update any relocated pointers
            foreach (var eta in nameTables.Values)
            {
                foreach (var key in eta.Keys.ToArray())
                {
                    eta[key] = GetActualPointer(eta[key], relocatedStructs) ?? 0;
                }
            }

            return nameTables;
        }

        public abstract void LoadVRAM(R1_PS1_GameData data, PS1_AllfixFile allfix, PS1_WorldFile world, PS1_LevFile lev);

        public void LoadPalettes(R1_PS1_GameData data, PS1_WorldFile wld)
        {
            // Add tile palettes
            data.PS1_TilePalettes = wld.TilePalettes.Select((x, i) => new SerializablePalette<RGBA5551Color>(x, $"Tile Palette {i}")).ToArray();

            foreach (var pal in data.PS1_TilePalettes)
                data.TextureManager.AddPalette(pal);

            // Add sprite palettes
            data.PS1_SpritePalettes = data.Vram.Palettes.Select(x => new R1_PS1_GameData.LoadedPalette(x.Colors, new SerializablePalette<RGBA5551Color>(x.Colors, $"Sprite Palette ({x.X}, {x.Y})"))).ToArray();

            foreach (var pal in data.PS1_SpritePalettes)
                data.TextureManager.AddPalette(pal.Palette);
        }

        public void LoadDES(R1_PS1_GameData data, PS1_ObjBlock objData, Dictionary<string, Dictionary<string, DESPointers>> nameTable, TextureManager textureManager)
        {
            foreach (var des in GetLevelDES(data.Context, objData.Objects, nameTable))
            {
                if (des.SpritesPointer != null && data.DES.All(x => x.SpritesData.Offset != des.SpritesPointer))
                {
                    // Read or get the data
                    var s = data.Context.Deserializer;
                    var sprites = des.ObjData?.SpriteCollection ?? s.DoAt(des.SpritesPointer, () => s.SerializeObject<SpriteCollection>(default, x => x.Pre_SpritesCount = des.ImageDescriptorCount, name: $"Sprites"));
                    var animations = des.ObjData?.AnimationCollection ?? s.DoAt(des.AnimationsPointer, () => s.SerializeObject<AnimationCollection>(default, x => x.Pre_AnimationsCount = des.AnimationsCount, name: $"Animations"));
                    var imgBuffer = des.ObjData?.ImageBuffer ?? s.DoAt(des.ImageBufferPointer, () => s.SerializeArray<byte>(default, des.ImageBufferLength ?? 0, name: $"ImageBuffer"));

                    // Create the sprite sheet
                    var spriteSheet = new PalettedTextureSheet(textureManager, sprites.Sprites.Select(x => x.IsDummySprite() ? (Point?)null : new Point(x.Width, x.Height)).ToArray());

                    // Initialize the sprites
                    for (var i = 0; i < sprites.Sprites.Length; i++)
                    {
                        // Ignore dummy sprites
                        if (spriteSheet.Entries[i] == null)
                            continue;

                        InitializeSprite(data, data.Vram, sprites.Sprites[i], spriteSheet, i);
                    }

                    // Get the DES name
                    var desName = des.Name ?? nameTable?.TryGetValue(des.SpritesPointer?.File.FilePath)?.FirstOrDefault(x =>
                        x.Value.Sprites == des.SpritesPointer?.AbsoluteOffset &&
                        x.Value.Animations == des.AnimationsPointer?.AbsoluteOffset &&
                        (!x.Value.ImageBuffer.HasValue || x.Value.ImageBuffer == des.ImageBufferPointer?.AbsoluteOffset)).Key;

                    data.AddDES(new R1_GameData.DESData(sprites, spriteSheet, animations, animations.Animations.Select(x => ToCommonAnimation(x)).ToArray(), imgBuffer)
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

            var is4Bit = sprite.TexturePageInfo.TP == PS1_TSB.TexturePageTP.CLUT_4Bit;
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
                PS1_TSB.TexturePageTP.CLUT_4Bit => PalettedTextureData.ImageFormat.Linear_4bpp,
                PS1_TSB.TexturePageTP.CLUT_8Bit => PalettedTextureData.ImageFormat.Linear_8bpp,
                _ => throw new Exception($"Unsupported texture type {sprite.TexturePageInfo.TP}")
            };

            sheet.InitEntry(index, pal, imgData, format: format, paletteOffset: palOffset);
        }

        protected IEnumerable<DES> GetLevelDES(Context context, IEnumerable<ObjData> objects, Dictionary<string, Dictionary<string, DESPointers>> nameTable_R1PS1DES)
        {
            return objects.Select(x => new DES
            {
                SpritesPointer = x.SpritesPointer,
                AnimationsPointer = x.AnimationsPointer,
                ImageBufferPointer = x.ImageBufferPointer,
                ImageDescriptorCount = x.SpritesCount,
                AnimationsCount = x.AnimationsCount,
                ImageBufferLength = null,
                Name = null,
                ObjData = x
            }).Concat(nameTable_R1PS1DES?.SelectMany(d => d.Value.Select(des => new DES
            {
                SpritesPointer = des.Value.Sprites != null ? new Pointer(des.Value.Sprites.Value, context.GetFile(d.Key)) : null,
                AnimationsPointer = des.Value.Animations != null ? new Pointer(des.Value.Animations.Value, context.GetFile(d.Key)) : null,
                ImageBufferPointer = des.Value.ImageBuffer != null ? new Pointer(des.Value.ImageBuffer.Value, context.GetFile(d.Key)) : null,
                ImageDescriptorCount = des.Value.SpritesCount,
                AnimationsCount = des.Value.AnimationsCount,
                ImageBufferLength = des.Value.ImageBufferLength,
                Name = des.Key,
                ObjData = null
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

                    var etaObj = eta.ObjData?.ETA ?? s.DoAt(eta.ETAPointer, () => s.SerializeObject<BinarySerializer.Ray1.ETA>(default, name: $"ETA"));

                    // Add to the ETA
                    data.ETA.Add(new R1_GameData.ETAData(etaObj)
                    {
                        Name = etaName
                    });
                }
            }
        }

        protected IEnumerable<ETA> GetLevelETA(Context context, IEnumerable<ObjData> objects, Dictionary<string, Dictionary<string, uint>> nameTable_R1PS1ETA)
        {
            return objects.Select(x => new ETA
            {
                ETAPointer = x.ETAPointer,
                Name = null,
                ObjData = x
            }).Concat(nameTable_R1PS1ETA?.SelectMany(d => d.Value.Select(des => new ETA
            {
                ETAPointer = new Pointer(des.Value, context.GetFile(d.Key)),
                Name = des.Key,
                ObjData = null
            })) ?? new ETA[0]);
        }

        public void LoadObjects(R1_PS1_GameData data, PS1_ObjBlock objBlock)
        {
            data.Objects.AddRange(objBlock.Objects.Select(obj => new R1_GameObject(obj, FindMatchingEventDefinition(data, obj))));

            data.LinkTable = objBlock.ObjectLinkingTable.Select(x => (ushort)x).ToArray();
            InitLinkGroups(data.Objects, data.LinkTable);
        }

        public void LoadWldObj(R1_GameData data, PS1_AllfixBlock fix)
        {
            var wldObj = fix.WldObj;

            data.ObjTemplates[R1_GameData.WldObjType.Ray] = wldObj[0];
            data.ObjTemplates[R1_GameData.WldObjType.RayLittle] = wldObj[1];
            data.ObjTemplates[R1_GameData.WldObjType.ClockObj] = wldObj[2];
            data.ObjTemplates[R1_GameData.WldObjType.DivObj] = wldObj[3];
            data.ObjTemplates[R1_GameData.WldObjType.MapObj] = wldObj[4];
        }

        public void LoadMap(R1_PS1_GameData data, PS1_WorldFile wld, MapData map, TextureManager textureManager)
        {
            var tileSet = LoadTileSet(data, wld, textureManager);
            LoadMap(data, textureManager, tileSet, map.Tiles, map.Width, map.Height, map.Offset, TileSetWidth);
        }

        public TileSet LoadTileSet(R1_PS1_GameData data, PS1_WorldFile wld, TextureManager textureManager)
        {
            var tileSize = new Point(Ray1Settings.CellSize);

            var width = tileSize.X * TileSetWidth;
            var height = (wld.PalettedTiles.Length) / width;
            var texture = new Texture2D(textureManager.GraphicsDevice, width, height);

            var tileSheet = PalettedTextureSheet.FromTileSet(textureManager, texture, tileSize, wld.TilePaletteIndexTable.Length);

            var tileSet = new TileSet(tileSheet, tileSize);

            for (int i = 0; i < tileSet.TileSheet.Entries.Length; i++)
            {
                var palette = data.PS1_TilePalettes[wld.TilePaletteIndexTable[i]];

                var imgData = new byte[tileSet.TileSize.X * tileSet.TileSize.Y];

                var tileSetX = (i % TileSetWidth) * tileSet.TileSize.X;
                var tileSetY = (i / TileSetWidth) * tileSet.TileSize.Y;

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

        public void SaveObjects(R1_PS1_GameData data, PS1_ObjBlock objData)
        {
            objData.ObjectsCount = objData.ObjectLinksCount = (byte)data.Objects.Count;
            objData.Objects = data.Objects.Cast<R1_GameObject>().Select(x => x.ObjData).ToArray();
            objData.ObjectLinkingTable = SaveLinkGroups(data.Objects).Select(x => (byte)x).ToArray();

            foreach (var obj in objData.Objects)
            {
                obj.AnimationsPointer = obj.AnimationCollection.Offset;
                obj.AnimationsCount = (byte)(obj.Type == ObjType.TYPE_DEMI_RAYMAN ? 0 : obj.AnimationCollection.Animations.Length);
                obj.ImageBufferPointer = null;
                obj.SpritesPointer = obj.SpriteCollection.Offset;
                obj.SpritesCount = (ushort)obj.SpriteCollection.Sprites.Length;
                obj.ETAPointer = obj.ETA.Offset;
                // No need to update commands/labels pointers since they can't be changed in the editor
            }
        }

        public void SaveMap(R1_PS1_GameData data, MapData mapData)
        {
            var mapLayer = data.Layers.OfType<R1_TileMapLayer>().First();

            mapData.Width = (ushort)mapLayer.MapSize.X;
            mapData.Height = (ushort)mapLayer.MapSize.Y;

            mapData.Tiles = mapLayer.TileMap;
        }

        public void RelocateLevelData(R1_PS1_GameData data, SerializableEditorFile<PS1_LevFile> lev, PS1_Executable exe)
        {
            var objData = lev.FileData.ObjData;

            lev.AdditionalSerializationActions.Clear();

            // Now for the complicated part. We need to repack the level file. It is split into 4 blocks: background, obj, map and vram.
            // The background block we can ignore for now. The object block starts with an object array and ends with all the referenced data,
            // commands -> animations -> sprites -> states -> (animation layers -> animation frames)
            // The map block is just an array of the tiles and the vram block we leave as is. So what we need is to:
            // 1. Relocate all the referenced data
            // 2. Resize the object and map blocks
            // 3. Modify the block pointers
            // One issue with relocating the sprites is that the defined pointers in the name tables (the .json files used in the editor)
            // will no longer be accurate. We can solve this by appending a footer (using SerializableEditorFile) containing a
            // table with all the relocated data. That way we can match original pointers with their new ones.
            // Only other issue is the memory limit. We don't want to increase the level size too much or it will overlap other files (oh no!).
            // It appears the next mapped file is the exe. The game maps the files like this: FIX -> WLD -> FND -> LEV (where are sounds?)

            var relocatedStructs = new List<RelocatedStruct>();

            // Get the size of the objects (all have the same size)
            long objSize = 0;
            ObjData firstObj = objData.Objects.FirstOrDefault();

            if (firstObj != null)
            {
                firstObj.RecalculateSize();
                objSize = firstObj.Size;
            }

            // Correct the object pointers
            objData.ObjectsPointer = new Pointer(lev.FileData.ObjectBlockPointer.AbsoluteOffset + 16, lev.Offset.File);
            objData.ObjectLinksPointer = objData.ObjectsPointer + (objSize * objData.ObjectsCount);

            // Get the offset to start allocating the referenced data (right after the link table)
            var objDataOffset = objData.ObjectLinksPointer + objData.ObjectsCount;

            Pointer getNextPointer(long length, bool relativeToFile = false)
            {
                // Align by 4
                if (objDataOffset.AbsoluteOffset % 4 != 0)
                    objDataOffset += 4 - objDataOffset.AbsoluteOffset % 4;

                var offset = objDataOffset;

                // Increase the pointer
                objDataOffset += length;

                if (relativeToFile)
                    offset = new Pointer(offset.FileOffset, offset.File, offset.File.StartPointer);

                return offset;
            }

            var relocatedData = new HashSet<BinarySerializable>();

            foreach (var obj in objData.Objects)
            {
                if (obj.Commands != null && obj.Commands.Commands.Length > 0)
                {
                    obj.Commands.Init(getNextPointer(0));
                    obj.Commands.RecalculateSize();
                    obj.CommandsPointer = getNextPointer(obj.Commands.Size);
                }
                else
                {
                    obj.CommandsPointer = null;
                }

                if (obj.LabelOffsets != null && obj.LabelOffsets.Length > 0)
                    obj.LabelOffsetsPointer = getNextPointer(obj.LabelOffsets.Length * 2);
                else
                    obj.LabelOffsetsPointer = null;

                var hasRelocatedETA = relocatedData.Contains(obj.ETA);
                var hasRelocatedAnims = relocatedData.Contains(obj.AnimationCollection);

                // Relocate animations, sprites and states
                obj.AnimationsPointer = relocateData(obj.AnimationsPointer, obj.AnimationCollection);
                obj.SpritesPointer = relocateData(obj.SpritesPointer, obj.SpriteCollection);
                obj.ETAPointer = relocateData(obj.ETAPointer, obj.ETA);

                // Relocate states
                if (!hasRelocatedETA && obj.ETAPointer.File == lev.Offset.File)
                {
                    for (var i = 0; i < obj.ETA.States.Length; i++)
                        obj.ETA.EtatPointers[i] = getNextPointer(8 * obj.ETA.States[i].Length);
                }

                // Relocate animation layers and frames
                if (!hasRelocatedAnims && obj.AnimationsPointer.File == lev.Offset.File)
                {
                    foreach (var anim in obj.AnimationCollection.Animations)
                    {
                        anim.LayersPointer = getNextPointer(anim.LayersPerFrame * anim.FrameCount * 4);

                        // Not all animations have frames defined
                        if (anim.Frames?.Length > 0)
                            anim.FramesPointer = getNextPointer(anim.FrameCount * 4);
                        else
                            anim.FramesPointer = null;
                    }
                }

                Pointer relocateData(Pointer currentPointer, BinarySerializable refData)
                {
                    if (refData == null)
                        return currentPointer;

                    // Only relocate data in the level file
                    if (currentPointer.File != lev.Offset.File)
                        return currentPointer;

                    // If we've already relocated the data we use the relocated pointer
                    if (relocatedData.Contains(refData))
                        return refData.Offset;

                    // We need the find the original pointer for the data. If the file has been repacked before it will be in the relocation table
                    Pointer originalPointer = lev.RelocatedStructs?.FirstOrDefault(x => x.NewPointer == currentPointer)?.OriginalPointer;

                    // If we didn't find it we assume the current pointer is the original one
                    originalPointer ??= currentPointer;

                    // Next we get the size of the data to relocate
                    refData.RecalculateSize();
                    var size = refData.Size;

                    // Get the new pointer
                    var newPointer = getNextPointer(size);

                    Logger.Log(LogLevel.Info, $"Relocating data from 0x{originalPointer.AbsoluteOffset:X8} to 0x{newPointer.AbsoluteOffset:X8}.");

                    // Add pointer to table
                    relocatedStructs.Add(new RelocatedStruct
                    {
                        OriginalPointer = originalPointer,
                        NewPointer = newPointer,
                        DataSize = (uint)size
                    });

                    // Re-initialize the data at the new offset
                    refData.Init(newPointer);

                    relocatedData.Add(refData);

                    // Return the new pointer
                    return newPointer;
                }
            }

            // Relocated unreferenced DES/ETA in the level file so they don't get lost
            foreach (var des in data.DES)
            {
                var handledAnims = addUnreferencedData(des.AnimationsData);
                addUnreferencedData(des.SpritesData);

                if (handledAnims)
                {
                    foreach (var anim in des.AnimationsData.Animations)
                    {
                        anim.LayersPointer = getNextPointer(anim.LayersPerFrame * anim.FrameCount * 4);

                        // Not all animations have frames defined
                        if (anim.Frames?.Length > 0)
                            anim.FramesPointer = getNextPointer(anim.FrameCount * 4);
                        else
                            anim.FramesPointer = null;
                    }
                }
            }

            foreach (var eta in data.ETA)
            {
                if (addUnreferencedData(eta.ETA))
                {
                    for (var i = 0; i < eta.ETA.States.Length; i++)
                        eta.ETA.EtatPointers[i] = getNextPointer(8 * eta.ETA.States[i].Length);
                }
            }

            bool addUnreferencedData<T>(T unrefData)
                where T : BinarySerializable, new()
            {
                // Skip data not in the level file, they won't be relocated
                if (unrefData.Offset.File != lev.Offset.File)
                    return false;

                // Skip already relocated data, that is already referenced
                if (relocatedData.Contains(unrefData))
                    return false;

                // We need the find the original pointer for the data. If the file has been repacked before it will be in the relocation table
                Pointer originalPointer = lev.RelocatedStructs?.FirstOrDefault(x => x.NewPointer == unrefData.Offset)?.OriginalPointer;

                // If we didn't find it we assume the current pointer is the original one
                originalPointer ??= unrefData.Offset;

                // Next we get the size of the data
                unrefData.RecalculateSize();
                var size = unrefData.Size;

                var newPointer = getNextPointer(size);

                Logger.Log(LogLevel.Info, $"Relocating unreferenced data from 0x{originalPointer.AbsoluteOffset:X8} to 0x{newPointer.AbsoluteOffset:X8}.");

                // Add pointer to table
                relocatedStructs.Add(new RelocatedStruct
                {
                    OriginalPointer = originalPointer,
                    NewPointer = newPointer,
                    DataSize = (uint)size
                });

                // Add data to be serialized
                lev.AdditionalSerializationActions.Add(newPointer, s => s.SerializeObject<T>(unrefData, name: $"UnreferencedData"));

                return true;
            }

            // Move the map and vram block pointers due to the object block being resized
            lev.FileData.BlockPointers[2] = getNextPointer(lev.FileData.MapData.Width * lev.FileData.MapData.Height * 2 + 4, true);
            lev.FileData.BlockPointers[3] = getNextPointer(lev.FileData.TextureBlock.Length, true);
            var fileEndOffset = getNextPointer(0);
            lev.FileData.FileSize = (uint)fileEndOffset.FileOffset;

            lev.RelocatedStructs = relocatedStructs.ToArray();
            lev.RelocatedStructsCount = relocatedStructs.Count;

            // Make sure the level file isn't too big and overlaps with the exe
            if (fileEndOffset.AbsoluteOffset >= exe.Offset.AbsoluteOffset)
                throw new Exception($"The level data is too big! End offset: 0x{fileEndOffset.AbsoluteOffset:X8} overlaps with exe offset at 0x{exe.Offset.AbsoluteOffset:X8}.");
        }

        protected void UpdateFileTable(Context context, PS1_Executable exe, string mkpsxisoFilePath, string xmlFilePath)
        {
            // Close the context so the files can be accessed by other processes
            context.Close();

            // Create a temporary file for the LBA log
            using var lbaLogFile = new TempFile();

            // Recalculate the LBA for the files on the disc
            ProcessHelpers.RunProcess(mkpsxisoFilePath, new string[]
            {
                "-lba", ProcessHelpers.GetStringAsPathArg(lbaLogFile.TempPath), // Specify LBA log path
                "-noisogen", // Don't generate an ISO now
                xmlFilePath // The xml path
            }, workingDir: context.BasePath);

            // Read the LBA log
            using var lbaLogStream = lbaLogFile.OpenRead();
            using var reader = new StreamReader(lbaLogStream);

            // Skip initial lines
            for (int i = 0; i < 8; i++)
                reader.ReadLine();

            var logEntries = new List<LBALogEntry>();
            var currentDirs = new List<string>();

            // Read all log entries
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line == null)
                    break;

                var words = line.Split(' ').Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();

                if (!words.Any())
                    continue;

                var entry = new LBALogEntry(words, currentDirs);

                logEntries.Add(entry);

                if (entry.EntryType == LBALogEntry.Type.Dir)
                    currentDirs.Add(entry.Name);

                if (entry.EntryType == LBALogEntry.Type.End)
                    currentDirs.RemoveAt(currentDirs.Count - 1);
            }

            // Update every file path in the file table
            foreach (var fileEntry in exe.PS1_FileTable)
            {
                // Get the matching entry
                var entry = logEntries.FirstOrDefault(x => x.FullPath == fileEntry.FilePath);

                if (entry == null)
                {
                    if (!String.IsNullOrWhiteSpace(fileEntry.FilePath))
                        Logger.Log(LogLevel.Warn, $"LBA not updated for {fileEntry.FilePath}");

                    continue;
                }

                // Update the LBA and size
                fileEntry.LBA = entry.LBA;
                fileEntry.FileSize = (uint)entry.Bytes;
            }
        }

        protected void CreateISO(Context context, string mkpsxisoFilePath, string xmlFilePath)
        {
            // Close context so the exe can be accessed
            context.Close();

            // Create a new ISO
            ProcessHelpers.RunProcess(mkpsxisoFilePath, new string[]
            {
                "-y", // Set to always overwrite
                xmlFilePath // The xml path
            }, workingDir: context.BasePath, logInfo: false);
        }

        #endregion

        #region Data Types

        public class DESPointers
        {
            public uint? Sprites { get; set; }
            public ushort SpritesCount { get; set; }
            public uint? Animations { get; set; }
            public byte AnimationsCount { get; set; }
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
            public ObjData ObjData { get; set; }
        }
        protected class ETA
        {
            public Pointer ETAPointer { get; set; }
            public string Name { get; set; }
            public ObjData ObjData { get; set; }
        }

        private class LBALogEntry
        {
            public LBALogEntry(IReadOnlyList<string> words, IReadOnlyList<string> dirs)
            {
                EntryType = (Type)Enum.Parse(typeof(Type), words[0], true);
                Name = words[1];

                if (EntryType == Type.End)
                    return;

                Length = Int32.Parse(words[2]);
                LBA = Int32.Parse(words[3]);
                TimeCode = words[4];
                Bytes = Int32.Parse(words[5]);

                if (EntryType != Type.Dir)
                    SourceFile = words[6];

                FullPath = $"\\{String.Join("\\", dirs.Append(Name))}";
            }

            public Type EntryType { get; }
            public string Name { get; }
            public int Length { get; }
            public int LBA { get; }
            public string TimeCode { get; }
            public int Bytes { get; }
            public string SourceFile { get; }

            public string FullPath { get; }

            public enum Type
            {
                File,
                STR,
                XA,
                CDDA,
                Dir,
                End
            }
        }

        #endregion
    }
}