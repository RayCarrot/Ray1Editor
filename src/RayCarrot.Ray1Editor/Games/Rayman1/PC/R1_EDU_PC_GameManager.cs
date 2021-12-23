using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.Image;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor;

public class R1_EDU_PC_GameManager : R1_PC_GameManager
{
    #region Paths

    public virtual string Path_VolumeDir(string volume) => Path_DataDir + $"{volume}/";
    public override string Path_VigFile(Ray1Settings settings) => Path_VolumeDir(settings.Volume) + "VIGNET.DAT";
    public override string Path_WorldFile(Ray1Settings settings) => Path_VolumeDir(settings.Volume) + $"RAY{(int)settings.World:00}.WLD";
    public override string Path_LevelFile(Ray1Settings settings) => Path_VolumeDir(settings.Volume) + $"{GetShortWorldName(settings.World)}{settings.Level:00}.LEV";

    #endregion

    #region Manager

    public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path)
    {
        var g = (Games.R1_Game)game;

        foreach (var volDir in Directory.GetDirectories(Path.Combine(path, Path_DataDir)))
        {
            var vol = Path.GetFileName(volDir);

            foreach (var world in WorldHelpers.EnumerateWorlds())
            {
                var shortName = GetShortWorldName(world);

                yield return new LoadGameLevelViewModel($"{world} ({vol})", null);

                foreach (var levFile in Directory.GetFiles(volDir, $"{shortName}*.LEV", SearchOption.TopDirectoryOnly))
                {
                    var lvl = Int32.Parse(Path.GetFileNameWithoutExtension(levFile)[3..]);

                    yield return new LoadGameLevelViewModel($"{world} {lvl}", new Ray1Settings(g.EngineVersion, world, lvl, volume: vol));
                }
            }
        }
    }

    public override IEnumerable<EditorFieldViewModel> GetEditorLevelAttributeFields(GameData gameData)
    {
        var data = (R1_PC_GameData)gameData;
        var lvlDefines = data.LevelDefines;
        var settings = data.Context.GetSettings<Ray1Settings>();

        yield return new EditorIntFieldViewModel(
            header: "Music track",
            info: "The music track to use. The index corresponds to the CD track.",
            getValueAction: () => lvlDefines.CDTrack,
            setValueAction: x => lvlDefines.CDTrack = (byte)x,
            max: 255);

        yield return new EditorBoolFieldViewModel(
            header: "Power: Fist",
            info: null,
            getValueAction: () => lvlDefines.RayEvts.HasFlag(RayEvts.Fist),
            setValueAction: x => lvlDefines.RayEvts = lvlDefines.RayEvts.SetFlag(RayEvts.Fist, x));

        yield return new EditorBoolFieldViewModel(
            header: "Power: Hang",
            info: null,
            getValueAction: () => lvlDefines.RayEvts.HasFlag(RayEvts.Hang),
            setValueAction: x => lvlDefines.RayEvts = lvlDefines.RayEvts.SetFlag(RayEvts.Hang, x));

        yield return new EditorBoolFieldViewModel(
            header: "Power: Helico",
            info: null,
            getValueAction: () => lvlDefines.RayEvts.HasFlag(RayEvts.Helico),
            setValueAction: x => lvlDefines.RayEvts = lvlDefines.RayEvts.SetFlag(RayEvts.Helico, x));

        yield return new EditorBoolFieldViewModel(
            header: "Power: SuperHelico",
            info: null,
            getValueAction: () => lvlDefines.RayEvts.HasFlag(RayEvts.SuperHelico),
            setValueAction: x => lvlDefines.RayEvts = lvlDefines.RayEvts.SetFlag(RayEvts.SuperHelico, x));

        if (settings.World == World.Jungle)
            yield return new EditorBoolFieldViewModel(
                header: "Power: Seed",
                info: null,
                getValueAction: () => lvlDefines.RayEvts.HasFlag(RayEvts.Seed),
                setValueAction: x => lvlDefines.RayEvts = lvlDefines.RayEvts.SetFlag(RayEvts.Seed, x));

        yield return new EditorBoolFieldViewModel(
            header: "Power: Grab",
            info: null,
            getValueAction: () => lvlDefines.RayEvts.HasFlag(RayEvts.Grab),
            setValueAction: x => lvlDefines.RayEvts = lvlDefines.RayEvts.SetFlag(RayEvts.Grab, x));

        yield return new EditorBoolFieldViewModel(
            header: "Power: Run",
            info: null,
            getValueAction: () => lvlDefines.RayEvts.HasFlag(RayEvts.Run),
            setValueAction: x => lvlDefines.RayEvts = lvlDefines.RayEvts.SetFlag(RayEvts.Run, x));

        yield return new EditorBoolFieldViewModel(
            header: "Effect: Firefly",
            info: null,
            getValueAction: () => lvlDefines.RayEvts.HasFlag(RayEvts.Firefly),
            setValueAction: x => lvlDefines.RayEvts = lvlDefines.RayEvts.SetFlag(RayEvts.Firefly, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: 0",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Effect_0),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Effect_0, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: 1",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Effect_1),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Effect_1, x));

        yield return new EditorBoolFieldViewModel(
            header: "Effect: Lock camera (horizontally)",
            info: null,
            getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.LockHorizontalCamera),
            setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.LockHorizontalCamera, x));

        yield return new EditorBoolFieldViewModel(
            header: "Effect: Lock camera (vertically)",
            info: null,
            getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.LockVerticalCamera),
            setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.LockVerticalCamera, x));

        yield return new EditorBoolFieldViewModel(
            header: "Effect: Cutscene border",
            info: null,
            getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.BetillaBorder),
            setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.BetillaBorder, x));

        yield return new EditorBoolFieldViewModel(
            header: "Effect: Storm",
            info: null,
            getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Storm),
            setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Storm, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: RainOrSnow_0",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.RainOrSnow_0),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.RainOrSnow_0, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: RainOrSnow_1",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.RainOrSnow_1),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.RainOrSnow_1, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: Wind",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Wind),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Wind, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: Effect_9",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Effect_9),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Effect_9, x));

        yield return new EditorBoolFieldViewModel(
            header: "Effect: Hot",
            info: null,
            getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.HotEffect),
            setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.HotEffect, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: Effect_11",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Effect_11),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Effect_11, x));

        yield return new EditorBoolFieldViewModel(
            header: "Effect: Hide HUD",
            info: null,
            getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.HideHUD),
            setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.HideHUD, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: Effect_13",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Effect_13),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Effect_13, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: Effect_14",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Effect_14),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Effect_14, x));

        //yield return new EditorBoolFieldViewModel(
        //    header: "Effect: Effect_15",
        //    info: null,
        //    getValueAction: () => lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Effect_15),
        //    setValueAction: x => lvlDefines.EffectFlags = lvlDefines.EffectFlags.SetFlag(PC_LevelDefines.LevelEffectFlags.Effect_15, x));
    }

    public override void Save(Context context, GameData gameData)
    {
        var data = (R1_PC_GameData)gameData;
        var lvlDefines = data.LevelDefines;

        // Verify level define combinations
        if (lvlDefines.RayEvts.HasFlag(RayEvts.Firefly) && lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Storm))
            throw new Exception("Firefly can't be used with storm");

        if (lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.RainOrSnow_0) && lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.RainOrSnow_1))
            throw new Exception("Rain can't be used with snow");

        if (lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.Wind) && (!lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.RainOrSnow_0) && !lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.RainOrSnow_1)))
            throw new Exception("Wind can't be used without rain or snow");

        if (lvlDefines.EffectFlags.HasFlag(PC_LevelDefines.LevelEffectFlags.HotEffect) && lvlDefines.FNDIndex != lvlDefines.ScrollDiffFNDIndex)
            throw new Exception("Hot effect can't be used with differential scroll");

        base.Save(context, gameData);
    }

    #endregion

    #region R1 Manager

    protected override int MaxObjType => (int)ObjType.MS_scintillement;

    public override void LoadFond(R1_PC_GameData data, PC_WorldFile wld, PC_LevFile lev, TextureManager textureManager)
    {
        // Load every available background
        var fondOptions = wld.Plan0NumPcxFiles.Select(x => LoadFond(data, textureManager, x)).ToArray();

        // Create a layer for the normal background
        data.Layers.Add(new R1_PC_BackgroundLayer(fondOptions, Point.Zero, lev.LevelDefines.FNDIndex, name: "Background"));
    }

    public R1_PC_BackgroundLayer.BackgroundEntry_R1_PC LoadFond(R1_PC_GameData data, TextureManager textureManager, string fileName)
    {
        var pcx = LoadArchiveFile<PCX>(data.Context, Path_VigFile(data.Context.GetSettings<Ray1Settings>()), fileName);

        var imgData = pcx.ScanLines.SelectMany(x => x).ToArray();

        var tex = new PalettedTextureData(textureManager.GraphicsDevice, imgData, new Point(pcx.ImageWidth, pcx.ImageHeight), PalettedTextureData.ImageFormat.Linear_8bpp, data.PC_Palettes[0]);

        tex.Apply();

        textureManager.AddPalettedTexture(tex);

        return new R1_PC_BackgroundLayer.BackgroundEntry_R1_PC(() => tex.Texture, pcx.Offset, $"{fileName}.pcx", pcx);
    }

    public override void SaveFond(R1_PC_GameData data, PC_LevFile lev)
    {
        lev.LevelDefines.FNDIndex = lev.LevelDefines.ScrollDiffFNDIndex = (byte)data.Layers.OfType<BackgroundLayer>().ElementAt(0).SelectedBackgroundIndex;
    }

    public T LoadArchiveFile<T>(Context context, string archivePath, string fileName)
        where T : BinarySerializable, new()
    {
        if (context.MemoryMap.Files.All(x => x.FilePath != archivePath))
            return null;

        return FileFactory.Read<PC_FileArchive>(archivePath, context).ReadFile<T>(context, fileName);
    }

    #endregion
}