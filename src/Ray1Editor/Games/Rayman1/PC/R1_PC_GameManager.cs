﻿using BinarySerializer;
using BinarySerializer.Image;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ray1Editor.Rayman1;

/// <summary>
/// Game manager for Rayman 1 on PC
/// </summary>
public class R1_PC_GameManager : R1_GameManager
{
    #region Paths

    public string Path_DataDir => $"PCMAP/";
    public virtual string Path_VigFile(Ray1Settings settings) => $"VIGNET.DAT";
    public virtual string Path_FixFile => Path_DataDir + $"ALLFIX.DAT";
    public virtual string Path_WorldFile(Ray1Settings settings) => Path_DataDir + $"RAY{(int)settings.World}.WLD";
    public virtual string Path_LevelFile(Ray1Settings settings) => Path_DataDir + $"{GetWorldName(settings.World)}/" + $"RAY{settings.Level}.LEV";

    #endregion
        
    #region Manager

    public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path)
    {
        return GetLevels(((Games.R1_Game)game).EngineVersion, ((Games.R1_Game)game).PCVersion);
    }

    public override GameData Load(Context context, object settings, TextureManager textureManager)
    {
        // Get settings
        var ray1Settings = (Ray1Settings)settings;
        var game = (Games.R1_Game)context.GetRequiredSettings<Games.Game>();

        // Add the settings
        context.AddSettings(ray1Settings);

        // Add files
        context.AddFile(new LinearFile(context, Path_VigFile(ray1Settings)));
        context.AddFile(new LinearFile(context, Path_FixFile));
        context.AddFile(new LinearFile(context, Path_WorldFile(ray1Settings)));
        context.AddFile(new LinearFile(context, Path_LevelFile(ray1Settings)));

        // Create the data
        var data = new R1_PC_GameData(context, textureManager);

        // Read the files
        var fix = FileFactory.Read<PC_AllfixFile>(context, Path_FixFile);
        var wld = FileFactory.Read<PC_WorldFile>(context, Path_WorldFile(ray1Settings));
        var lev = FileFactory.Read<SerializableEditorFile<PC_LevFile>>(context, Path_LevelFile(ray1Settings)).FileData;

        // Initialize the random generation
        InitRandom(data);

        // Load the editor name tables
        var desNames = LoadNameTable_DES(data, game, ray1Settings);
        var etaNames = LoadNameTable_ETA(data, game, ray1Settings);

        // Store the level defines
        data.LevelDefines = lev.LevelDefines;
        data.ProfileDefines = lev.ProfileDefine;

        // Load palettes
        LoadPalettes(data, lev);

        // Load fond (background)
        LoadFond(data, wld, lev, textureManager);

        // Load map
        LoadMap(data, lev, textureManager);

        // Load DES (sprites & animations)
        LoadDES(data, fix, wld, textureManager, desNames);

        // Load ETA (states)
        LoadETA(data, fix, wld, etaNames);

        // Load the editor event definitions
        LoadEditorEventDefinitions(data);

        // Load objects
        LoadObjects(data, lev);

        // Load the wld objects (object templates)
        LoadWldObj(data, fix);

        // Load Rayman
        LoadRayman(data);

        return data;
    }

    public override void Save(Context context, GameData gameData)
    {
        // Get settings
        var ray1Settings = context.GetRequiredSettings<Ray1Settings>();

        var data = (R1_PC_GameData)gameData;

        // Get the level data
        var lvlData = context.GetMainFileObject<SerializableEditorFile<PC_LevFile>>(Path_LevelFile(ray1Settings)).FileData;

        // Save the objects
        var objData = lvlData.ObjData;
        objData.ObjCount = (ushort)gameData.Objects.Count;
        objData.Objects = gameData.Objects.Cast<R1_GameObject>().Select(x => x.ObjData).ToArray();

        var objCmds = new List<PC_CommandCollection>();

        foreach (var obj in objData.Objects)
        {
            obj.PC_AnimationsIndex = (uint)data.PC_LoadedAnimations.FindIndex(x => x == obj.AnimationCollection);
            obj.AnimationsCount = (byte)obj.AnimationCollection.Animations.Length;
            obj.PC_ImageBufferIndex = (uint)data.PC_DES.FindIndex(x => x?.ImageData == obj.ImageBuffer);
            obj.PC_SpritesIndex = (uint)data.PC_DES.FindIndex(x => x?.SpriteCollection == obj.SpriteCollection);
            obj.SpritesCount = (ushort)obj.SpriteCollection.Sprites.Length;
            obj.PC_ETAIndex = (uint)data.ETA.FindLastIndex(x => x.ETA == obj.ETA);

            var cmds = obj.Commands ?? new CommandCollection()
            {
                Commands = new Command[0]
            };
            var labelOffsets = obj.LabelOffsets ?? new ushort[0];

            // Add the event commands
            objCmds.Add(new PC_CommandCollection()
            {
                CommandLength = (ushort)cmds.Commands.Select(x => x.Length).Sum(),
                Commands = cmds,
                LabelOffsetCount = (ushort)labelOffsets.Length,
                LabelOffsetTable = labelOffsets
            });
        }

        objData.ObjCommands = objCmds.ToArray();
        objData.ObjLinkingTable = SaveLinkGroups(gameData.Objects);

        // Save the map
        var mapData = lvlData.MapData;
        var mapLayer = data.Layers.OfType<R1_TileMapLayer>().First();

        mapData.Width = (ushort)mapLayer.MapSize.X;
        mapData.Height = (ushort)mapLayer.MapSize.Y;

        mapData.Tiles = mapLayer.TileMap;

        // Save the background
        SaveFond(data, lvlData);

        // Save the file
        FileFactory.Write<SerializableEditorFile<PC_LevFile>>(context, Path_LevelFile(ray1Settings));
    }

    #endregion

    #region R1 Manager

    protected override int MaxObjType => (int)ObjType.TYPE_EDU_DIRECTION;

    protected virtual string[] LoadNameTable_DES(R1_GameData data, Games.R1_Game game, Ray1Settings settings)
    {
        return LoadEditorNameTable<string[][]>(game.NameTablesName, EditorNameTableType.DES)[(int)settings.World - 1];
    }

    protected virtual string[] LoadNameTable_ETA(R1_GameData data, Games.R1_Game game, Ray1Settings settings)
    {
        return LoadEditorNameTable<string[][]>(game.NameTablesName, EditorNameTableType.ETA)[(int)settings.World - 1];
    }

    public string GetWorldName(World world) => world switch
    {
        World.Jungle => "JUNGLE",
        World.Music => "MUSIC",
        World.Mountain => "MOUNTAIN",
        World.Image => "IMAGE",
        World.Cave => "CAVE",
        World.Cake => "CAKE",
        _ => throw new ArgumentOutOfRangeException(nameof(world), world, null)
    };

    public virtual string GetShortWorldName(World world) => world switch
    {
        World.Jungle => "JUN",
        World.Music => "MUS",
        World.Mountain => "MON",
        World.Image => "IMA",
        World.Cave => "CAV",
        World.Cake => "CAK",
        _ => throw new ArgumentOutOfRangeException(nameof(world), world, null)
    };

    public void LoadPalettes(R1_PC_GameData data, PC_LevFile lev)
    {
        data.PC_Palettes = lev.MapData.ColorPalettes.Select((x, i) => new SerializablePalette<RGB666Color>(x, $"Palette {i + 1}")
        {
            CanEditAlpha = false,
            IsFirstTransparent = true,
            SelectionGroup = lev.MapData.ColorPalettes.Length > 1 ? "R1_PC_Palette" : null // Name doesn't matter as long as it's the same for all palettes
        }).ToArray();

        foreach (var pal in data.PC_Palettes)
            data.TextureManager.AddPalette(pal);
    }

    public void LoadDES(R1_PC_GameData data, PC_AllfixFile fix, PC_WorldFile wld, TextureManager textureManager, string[] names)
    {
        data.PC_DES = new PC_DES[]
        {
            null
        }.Concat(fix.DesItems).Concat(wld.DesItems).ToArray();

        data.PC_LoadedAnimations = new AnimationCollection[data.PC_DES.Length];

        for (int i = 1; i < data.PC_DES.Length; i++)
        {
            var des = data.PC_DES[i];

            var processedImageData = des.IsAnimatedSprite ? PC_DES.ProcessImageData(des.ImageData) : des.ImageData;

            var spriteSheet = new PalettedTextureSheet(textureManager, des.SpriteCollection.Sprites.Select(x => x.IsDummySprite() ? (Point?)null : new Point(x.Width, x.Height)).ToArray());

            for (int j = 0; j < des.SpritesCount; j++)
            {
                var spriteSheetEntry = spriteSheet.Entries[j];

                if (spriteSheetEntry == null)
                    continue;

                var sprite = des.SpriteCollection.Sprites[j];

                spriteSheet.InitEntry(j, data.PC_Palettes[0], processedImageData, imgDataStartIndex: (int)sprite.ImageBufferOffset, imgDataLength: sprite.Width * sprite.Height);
            }

            var loadedAnim = des.Animations.Select(x => new Animation
            {
                LayersPerFrameSerialized = x.LayersPerFrame,
                FrameCountSerialized = x.FrameCount,
                Layers = x.Layers,
                Frames = x.Frames
            }).ToArray();
            var animCollection = new AnimationCollection()
            {
                Pre_AnimationsCount = loadedAnim.Length,
                Animations = loadedAnim,
            };
            data.PC_LoadedAnimations[i] = animCollection;
            data.AddDES(new R1_GameData.DESData(des.SpriteCollection, spriteSheet, animCollection, loadedAnim.Select(x => ToCommonAnimation(x)).ToArray(), des.ImageData)
            {
                Name = names.ElementAtOrDefault(i)
            });
        }
    }

    public void LoadETA(R1_GameData data, PC_AllfixFile fix, PC_WorldFile wld, string[] names)
    {
        loadETA(fix.Eta);
        loadETA(wld.Eta);

        void loadETA(IEnumerable<PC_ETA> eta)
        {
            var offset = data.ETA.Count;

            data.ETA.AddRange(eta.Select((x, i) => new R1_GameData.ETAData(new ETA()
            {
                States = x.States
            })
            {
                Name = names.ElementAtOrDefault(offset + i)
            }));
        }
    }

    public void LoadObjects(R1_PC_GameData data, PC_LevFile lev)
    {
        var objIndex = 0;

        foreach (var obj in lev.ObjData.Objects)
        {
            LoadObject(data, lev, obj, objIndex);

            data.Objects.Add(new R1_GameObject(obj, data.Context.GetRequiredSettings<Ray1Settings>(), FindMatchingEventDefinition(data, obj)));

            objIndex++;
        }

        data.LinkTable = lev.ObjData.ObjLinkingTable;
        InitLinkGroups(data.Objects, lev.ObjData.ObjLinkingTable);
    }

    public void LoadObject(R1_PC_GameData data, PC_LevFile lev, ObjData obj, int index)
    {
        obj.AnimationCollection = data.PC_LoadedAnimations[(int)obj.PC_AnimationsIndex];
        obj.ImageBuffer = data.PC_DES[obj.PC_SpritesIndex].ImageData;
        obj.SpriteCollection = data.PC_DES[obj.PC_SpritesIndex].SpriteCollection;
        obj.ETA = data.ETA[(int)obj.PC_ETAIndex].ETA;

        if (index != -1)
        {
            obj.Commands = lev.ObjData.ObjCommands[index].Commands;
            obj.LabelOffsets = lev.ObjData.ObjCommands[index].LabelOffsetTable;
        }
    }

    public void LoadWldObj(R1_PC_GameData data, PC_AllfixFile fix)
    {
        loadObj(R1_GameData.WldObjType.Ray, fix.DESIndex_Ray);
        loadObj(R1_GameData.WldObjType.RayLittle, fix.DESIndex_RayLittle);
        loadObj(R1_GameData.WldObjType.MapObj, fix.DESIndex_MapObj);
        loadObj(R1_GameData.WldObjType.ClockObj, fix.DESIndex_ClockObj);
        loadObj(R1_GameData.WldObjType.DivObj, fix.DESIndex_DivObj);

        void loadObj(R1_GameData.WldObjType type, uint desIndex)
        {
            data.ObjTemplates[type] = new ObjData()
            {
                PC_SpritesIndex = desIndex,
                PC_ImageBufferIndex = desIndex,
                PC_AnimationsIndex = desIndex,
                PC_ETAIndex = fix.DesItems[desIndex - 1].WldETAIndex
            };

            LoadObject(data, null, data.ObjTemplates[type], -1);
        }
    }

    public void LoadMap(R1_PC_GameData data, PC_LevFile lev, TextureManager textureManager)
    {
        var map = lev.MapData;

        var tileSet = LoadTileSet(data, lev, textureManager);
        LoadMap(data, textureManager, tileSet, map.Tiles, map.Width, map.Height, map.Offset);
    }

    public TileSet LoadTileSet(R1_PC_GameData data, PC_LevFile lev, TextureManager textureManager)
    {
        var tileSize = new Point(Ray1Settings.CellSize, Ray1Settings.CellSize);

        var tileSheet = new PalettedTextureSheet(textureManager, Enumerable.Repeat((Point?)tileSize, lev.TileTextureData.TexturesOffsetTable.Length).ToArray());

        var tileSet = new TileSet(tileSheet, tileSize);

        for (int i = 0; i < tileSet.TileSheet.Entries.Length; i++)
        {
            var tex = lev.TileTextureData.NonTransparentTextures.
                Concat(lev.TileTextureData.TransparentTextures).
                First(x => x.Offset == lev.TileTextureData.TexturesOffsetTable[i]);

            tileSheet.InitEntry(i, data.PC_Palettes[0], tex.ImgData.Select(x => (byte)(255 - x)).ToArray());
        }

        data.PC_TileSetTransparencyModes = lev.TileTextureData?.TexturesOffsetTable.Select(x => lev.TileTextureData.NonTransparentTextures.Concat(lev.TileTextureData.TransparentTextures).FirstOrDefault(t => t.Offset == x)).Select(x =>
        {
            if (x == null)
                return MapTile.PC_TransparencyMode.FullyTransparent;

            return x.TransparencyMode switch
            {
                0xAAAAAAAA => MapTile.PC_TransparencyMode.FullyTransparent,
                0x55555555 => MapTile.PC_TransparencyMode.NoTransparency,
                _ => MapTile.PC_TransparencyMode.PartiallyTransparent
            };
        }).ToArray();

        return tileSet;
    }

    public virtual void LoadFond(R1_PC_GameData data, PC_WorldFile wld, PC_LevFile lev, TextureManager textureManager)
    {
        // Load every available background
        var fondOptions = wld.Plan0NumPcx.Select(x => LoadFond(data, textureManager, x)).ToArray();

        // Create a layer for the normal and parallax backgrounds
        data.Layers.Add(new R1_PC_BackgroundLayer(fondOptions, Point.Zero, lev.FNDIndex, name: "Background"));
        data.Layers.Add(new R1_PC_BackgroundLayer(fondOptions, Point.Zero, lev.ScrollDiffFNDIndex, name: "Parallax Background", isVisible: false));
    }

    public R1_PC_BackgroundLayer.BackgroundEntry_R1_PC LoadFond(R1_PC_GameData data, TextureManager textureManager, int index)
    {
        var pcx = LoadArchiveFile<PCX>(data.Context, Path_VigFile(data.Context.GetRequiredSettings<Ray1Settings>()), index);

        var imgData = pcx.ScanLines.SelectMany(x => x).ToArray();

        var tex = new PalettedTextureData(textureManager.GraphicsDevice, imgData, new Point(pcx.ImageWidth, pcx.ImageHeight), PalettedTextureData.ImageFormat.Linear_8bpp, data.PC_Palettes[0]);

        tex.Apply();

        textureManager.AddPalettedTexture(tex);

        return new R1_PC_BackgroundLayer.BackgroundEntry_R1_PC(() => tex.Texture, pcx.Offset, $"{index}.pcx", pcx);
    }

    public virtual void SaveFond(R1_PC_GameData data, PC_LevFile lev)
    {
        lev.FNDIndex = (byte)data.Layers.OfType<BackgroundLayer>().ElementAt(0).SelectedBackgroundIndex;
        lev.ScrollDiffFNDIndex = (byte)data.Layers.OfType<BackgroundLayer>().ElementAt(1).SelectedBackgroundIndex;
    }

    public T LoadArchiveFile<T>(Context context, string archivePath, int fileIndex)
        where T : BinarySerializable, new()
    {
        if (context.MemoryMap.Files.All(x => x.FilePath != archivePath))
            return null;

        return FileFactory.Read<PC_FileArchive>(context, archivePath).ReadFile<T>(context, fileIndex);
    }

    #endregion
}