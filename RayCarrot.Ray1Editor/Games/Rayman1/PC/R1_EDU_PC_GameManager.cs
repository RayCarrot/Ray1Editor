using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.Image;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
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

            var tex = new PalettedTextureData(textureManager.GraphicsDevice, imgData, new Point(pcx.ImageWidth, pcx.ImageHeight), PalettedTextureData.ImageFormat.BPP_8, data.PC_Palettes[0]);

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
}