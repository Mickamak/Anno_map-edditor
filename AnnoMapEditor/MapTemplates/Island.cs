﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using AnnoMapEditor.Utils;
using AnnoMapEditor.MapTemplates.Serializing;

namespace AnnoMapEditor.MapTemplates
{
    static class SpecialIslands
    {
        public static readonly Dictionary<string, int> CachedSizes = new Dictionary<string, int>()
        {
            ["data/dlc01/sessions/islands/pool/moderate_c_01/moderate_c_01.a7m"] = 768,
            ["data/dlc01/sessions/islands/pool/moderate_3rdparty06_01/moderate_3rdparty06_01.a7m"] = 128,
            ["data/dlc01/sessions/islands/pool/moderate_dst_04/moderate_dst_04.a7m"] = 192,
            ["data/sessions/islands/pool/moderate/community_island/community_island.a7m"] = 384,
            ["data/dlc01/sessions/islands/pool/moderate_encounter_01/moderate_encounter_01.a7m"] = 256
        };
    }

    public class Island
    {
        public int ElementType { get; set; } = 0;
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public IslandSize Size { get; set; } = IslandSize.Small;
        public int SizeInTiles { get; set; } = 0;
        public IslandType Type { get; set; } = IslandType.Normal;
        public bool Hide { get; set; } = false;
        public string? ImageFile { get; set; }
        public int Rotation { get; set; } = 0;
        public string? MapPath { get; private set; }
        public string? AssumedMapPath { get; private set; }
        public bool IsPool => string.IsNullOrEmpty(MapPath);
        public string? Label { get; set; }

        private static readonly Random rnd = new((int)DateTime.Now.Ticks);

        private Serializing.A7tinfo.TemplateElement template;

        private Island()
        {
            template = new Serializing.A7tinfo.TemplateElement();
        }

        public static async Task<Island> FromSerialized(Serializing.A7tinfo.TemplateElement templateElement, Region region)
        {
            var element = templateElement.Element;
            var island = new Island()
            {
                ElementType = templateElement.ElementType ?? 0,
                Position = new Vector2(element?.Position),
                Size = new IslandSize(element?.Size),
                Type = new IslandType(element?.RandomIslandConfig?.value?.Type?.id ?? element?.Config?.Type?.id),
                Rotation = Math.Clamp((int?)element?.Rotation90 ?? 0, 0, 3),
                MapPath = element?.MapFilePath?.ToString(),
                Label = element?.IslandLabel?.ToString(),
                template = templateElement
            };

            await island.InitAsync(region);
            return island;
        }

        public Serializing.A7tinfo.TemplateElement ToTemplate()
        {
            return template;
        }

        public async Task UpdateExternalDataAsync()
        {
            if (AssumedMapPath is null)
                return;

            // fallback to read out map file
            int sizeInTiles = await IslandReader.ReadTileInSizeFromFileAsync(AssumedMapPath);
            if (sizeInTiles != 0)
                SizeInTiles = sizeInTiles;

            if (Settings.Instance.DataPath is not null)
            {
                string activeMapImagePath = Path.Combine(Path.GetDirectoryName(AssumedMapPath) ?? "", "_gamedata", Path.GetFileNameWithoutExtension(AssumedMapPath), "mapimage.png");
                ImageFile = activeMapImagePath;
            }
        }

        private async Task InitAsync(Region region)
        {
            if (ElementType == 2)
            {
                SizeInTiles = 1;
                return;
            }

            string? mapPath = MapPath;
            if (mapPath is not null && SpecialIslands.CachedSizes.ContainsKey(mapPath))
                SizeInTiles = SpecialIslands.CachedSizes[mapPath];

            if (mapPath == null)
            {
                mapPath = region.GetRandomIslandPath(Size);
                Rotation = rnd.Next(0, 3);
            }

            if (mapPath != null)
            {
                //if (mapPath.Contains("_dst_"))
                //    Hide = true;
                // else
                if (mapPath.Contains("_l_") && Size.IsDefault)
                    Size = IslandSize.Large;
                else if (mapPath.Contains("_m_") && Size.IsDefault)
                    Size = IslandSize.Medium;
            }

            if (SizeInTiles == 0)
                SizeInTiles = Size.InTiles;

            AssumedMapPath = mapPath;
            await UpdateExternalDataAsync();
        }
    }
}