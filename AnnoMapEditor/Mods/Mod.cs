﻿using AnnoMapEditor.MapTemplates;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

/*
 * Map size is somehow relies on the exact map file path. (not true I guess)
 * 
 * Modloader doesn't support a7t because they are loaded as .rda archive.
 * They specified with "mods/[Map] xyz/data/..."
 * Mistakes lead to endless loading.
 * 
 * The same map file path can't be used for differente TemplateSize at the same time. Leads to endless loading.
 * 
 * corners_ll_01, snowflake_ll_01 are unused. do they work?
 */

namespace AnnoMapEditor.Mods
{
    internal class Mod
    {
        private Session session;

        public Mod(Session session)
        {
            this.session = session;
        }

        public async Task Save(string modsFolderPath, string modName, string? modID)
        {
            string fullModName = "[Map] " + modName;
            string modPath = Path.Combine(modsFolderPath, fullModName);

            try
            {
                string mapGroupName = "archipel";

                string? sizeSourceMapName = ConvertSizeToMapName(session.PlayableArea.Width);
                if (sizeSourceMapName is null)
                    return; // TODO message
                var mapTemplateInfo = mapGuids.GetValueOrDefault(sizeSourceMapName);
                if (mapTemplateInfo is null)
                    return; // TODO message

                if (Directory.Exists(modPath))
                    Directory.Delete(modPath, true);
                Directory.CreateDirectory(modPath);
                await WriteMetaJson(modPath, modName, modID);

                await WriteLanguageXml(modPath, modName, mapTemplateInfo.MapType);

                string mapFilePath = $@"data\ame\maps\pool\moderate\{mapGroupName}";
                await WriteAssetsXml(modPath, fullModName, mapFilePath, mapTemplateInfo.Templates);

                await session.SaveAsync(Path.Combine(modPath, $"{mapFilePath}-l.a7tinfo"));
                await session.SaveAsync(Path.Combine(modPath, $"{mapFilePath}-m.a7tinfo"));
                await session.SaveAsync(Path.Combine(modPath, $"{mapFilePath}-s.a7tinfo"));

                await CopyFromArchive(modPath, $"{mapFilePath}e");
                await CopyFromArchive(modPath, mapFilePath);
            }
            catch
            {
                MessageBox.Show("Could not save mod.", "Save as mod", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private async Task WriteMetaJson(string modPath, string modName, string? modID)
        {
            Modinfo? modinfo = null;

            string modinfoPath = Path.Combine(modPath, "modinfo.json");
            if (File.Exists(modinfoPath))
            {
                modinfo = JsonConvert.DeserializeObject<Modinfo?>(File.ReadAllText(modinfoPath));
            }

            //if (modinfo is null)
            {
                modinfo = new()
                {
                    Version = "1",
                    ModID = modID ?? ("ame_" + MakeSafeName(modName)),
                    ModName = new(modName),
                    Category = new("Map"),
                    Description = new($"Select Map Type '{modName}' to play this map.\nStarting world size and island size will be ignored.\n\nThis mod has been created by {App.Title}.\n\nYou can download the editor at:\nhttps://github.com/anno-mods/AnnoMapEditor/releases/latest"),
                    CreatorName = App.TitleShort,
                    CreatorContact = "https://github.com/anno-mods/AnnoMapEditor"
                };
            }

            using StreamWriter writer = new(File.Create(modinfoPath));
            await writer.WriteAsync(JsonConvert.SerializeObject(modinfo, Formatting.Indented));
        }

        private async Task WriteLanguageXml(string modPath, string name, string guid)
        {
            string languageXmlPath = Path.Combine(modPath, @"data\config\gui\texts_english.xml");
            string? languageXmlDir = Path.GetDirectoryName(languageXmlPath);
            if (languageXmlDir is not null)
                Directory.CreateDirectory(languageXmlDir);

            string content = 
                $"<ModOps>\n" +
                $"  <ModOp Type=\"replace\" Path=\"//TextExport/Texts/Text[GUID='{guid}']/Text\">\n" +
                $"    <Text>{name}</Text>\n" +
                $"  </ModOp>\n" +
                $"</ModOps>\n";

            using StreamWriter writer = new(File.Create(languageXmlPath));
            await writer.WriteAsync(content);
        }

        private async Task WriteAssetsXml(string modPath, string fullModName, string mapFilePath, string[]? guids)
        {
            if (guids is null)
                return;

            string assetsXmlPath = Path.Combine(modPath, @"data\config\export\main\asset\assets.xml");
            string? assetsXmlDir = Path.GetDirectoryName(assetsXmlPath);
            if (assetsXmlDir is not null)
                Directory.CreateDirectory(assetsXmlDir);

            string content = "<ModOps>\n";

            string[] sizeEndings = new[] { "l", "m", "s" };
            IEnumerable<string> guidsPart = guids.Take(3);

            foreach (var ending in sizeEndings)
            {
                content +=
                    $"  <ModOp Type=\"replace\" GUID=\"{string.Join(',', guidsPart.Take(3))}\" Path=\"/Values/MapTemplate/TemplateFilename\">\n" +
                    $"    <TemplateFilename>mods/{fullModName}{mapFilePath.Replace("\\", "/")}-{ending}.a7t</TemplateFilename>\n" +
                    $"  </ModOp>\n";
                guidsPart = guidsPart.Skip(3);
            }
            content += "</ModOps>\n";

            using StreamWriter writer = new(File.Create(assetsXmlPath));
            await writer.WriteAsync(content);
        }

        private static string MakeSafeName(string unsafeName) => new Regex(@"\W").Replace(unsafeName, "_").ToLower();

        private class MapTemplateInfo
        {
            public string MapType;
            public string[] Templates;
        };

        private static MapTemplateInfo Archipelago = new() { MapType = "17079", Templates = new[] { "142012", "141265", "141264", "141269", "141268", "141267", "141272", "141271", "141270" } };

        private static Dictionary<string, MapTemplateInfo> mapGuids = new()
        {
            ["moderate_archipel_ll_01"] = Archipelago,
            ["moderate_archipel_lm_01"] = Archipelago,
            ["moderate_archipel_ls_01"] = Archipelago,
            ["moderate_archipel_ml_01"] = Archipelago,
            ["moderate_archipel_mm_01"] = Archipelago,
            ["moderate_archipel_ms_01"] = Archipelago,
            ["moderate_archipel_sl_01"] = Archipelago,
            ["moderate_archipel_sm_01"] = Archipelago,
            ["moderate_archipel_ss_01"] = Archipelago,
        };

        private static string? ConvertSizeToMapName(int playableSize)
        {
            Dictionary<int, string[]> pairs = new()
            {
                [2160] = new[] { "moderate_atoll_ll_01", "moderate_atoll_lm_01", "moderate_atoll_ls_01",
                    "moderate_islandarc_ll_01", "moderate_islandarc_lm_01", "moderate_islandarc_ls_01" },

                [1650] = new[] { "moderate_archipel_ll_01", "moderate_archipel_lm_01", "moderate_archipel_ls_01", 
                    "moderate_corners_ll_02", "moderate_corners_lm_01", "moderate_corners_ls_01", 
                    "moderate_snowflake_ll_02", "moderate_snowflake_lm_01", "moderate_snowflake_ls_01" },
                [1648] = new[] { "moderate_atoll_ml_01", "moderate_atoll_mm_01", "moderate_atoll_ms_01",
                    "moderate_islandarc_ml_01", "moderate_islandarc_mm_01", "moderate_islandarc_ms_01" },
                [1436] = new[] { "moderate_corners_ml_01" },
                [1392] = new[] { "moderate_archipel_ml_01" },
                [1366] = new[] { "moderate_snowflake_mm_01" },
                [1358] = new[] { "moderate_corners_sl_01" },
                [1356] = new[] { "moderate_snowflake_ml_01" },
                [1348] = new[] { "moderate_snowflake_sm_01" },
                [1338] = new[] { "moderate_corners_sm_01" },
                [1336] = new[] { "moderate_atoll_sl_01", "moderate_atoll_sm_01", "moderate_atoll_ss_01",
                    "moderate_corners_mm_01", "moderate_corners_ms_01",
                    "moderate_islandarc_sl_01", "moderate_islandarc_sm_01", "moderate_islandarc_ss_01",
                    "moderate_snowflake_ms_01" },
                [1327] = new[] { "moderate_snowflake_sl_01" },
                [1312] = new[] { "moderate_archipel_mm_01", "moderate_archipel_ms_01" },
                [1220] = new[] { "moderate_archipel_sm_01" },
                [1208] = new[] { "moderate_corners_ss_01", "moderate_snowflake_ss_01" },
                [1200] = new[] { "moderate_archipel_sl_01", "moderate_archipel_ss_01" },
            };

            string[]? maps = pairs.GetValueOrDefault(playableSize);
            if (maps is null || maps.Length == 0)
                return null;

            return maps[0];
        }

        private async Task CopyFromArchive(string modPath, string filePath)
        {
            var archive = Settings.Instance.DataArchive;
            if (archive is null)
                return;

            await Task.Run(() =>
            {
                using StreamWriter writer = new(File.Create(Path.Combine(modPath, filePath)));
                var stream = archive.OpenRead(filePath);
                if (stream is not null)
                    writer.Write(stream);
            });
        }
    }
}