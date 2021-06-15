using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
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
            var exe = FileFactory.Read<PS1_Executable>(Path_ExeFile, context, onPreSerialize: (s, o) => o.Pre_PS1_Config = exeConfig);

            var worldIndex = (int)ray1Settings.World - 1;
            var lvlIndex = ray1Settings.Level - 1;

            // Get file entries
            var fileEntryFix = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.filefxs)];
            var fileEntryworld = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.wld_file) + worldIndex];
            var fileEntrylevel = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.map_file) + (worldIndex * 21 + lvlIndex)];

            // Add and read allfix
            LoadFile(context, fileEntryFix);
            FileFactory.Read<PS1_AllfixFile>(fileEntryFix.ProcessedFilePath, context);

            // Add and read world
            LoadFile(context, fileEntryworld);
            var wld = FileFactory.Read<PS1_WorldFile>(fileEntryworld.ProcessedFilePath, context);

            // Add and read level
            LoadFile(context, fileEntrylevel);
            var lev = FileFactory.Read<PS1_LevFile>(fileEntrylevel.ProcessedFilePath, context);

            // Initialize the random generation
            InitRandom(data);

            // Load the editor name tables
            //var desNames = LoadNameTable_DES(data, game, ray1Settings);
            //var etaNames = LoadNameTable_ETA(data, game, ray1Settings);

            // Load palettes
            LoadPalettes(data, wld);

            // Load fond (background)
            LoadFond(data, exe, textureManager);

            // Load map
            LoadMap(data, wld, lev.MapData, textureManager);

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

        public void LoadPalettes(R1_PS1_GameData data, PS1_WorldFile wld)
        {
            data.PS1_TilePalettes = wld.TilePalettes.Select((x, i) => new Palette(x, $"Tile Palette {i}")).ToArray();

            foreach (var pal in data.PS1_TilePalettes)
                data.TextureManager.AddPalette(pal);
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
    }
}