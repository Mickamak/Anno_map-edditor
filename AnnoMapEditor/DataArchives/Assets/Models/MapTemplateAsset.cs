﻿using AnnoMapEditor.DataArchives.Assets.Attributes;
using AnnoMapEditor.DataArchives.Assets.Deserialization;
using AnnoMapEditor.Mods.Enums;
using System;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate("MapTemplate")]
    public class MapTemplateAsset : StandardAsset
    {
        public static readonly string TemplateName = "MapTemplate";


        public string TemplateFilename { get; init; }

        public string? EnlargedTemplateFilename { get; init; }

        public string TemplateRegionId { get; init; }

        [RegionIdReference(nameof(TemplateRegionId))]
        public RegionAsset TemplateRegion { get; set; }

        public MapType? TemplateMapType { get; init; }


        public MapTemplateAsset(XElement valuesXml)
            : base(valuesXml)
        {
            XElement mapTemplateValues = valuesXml.Element(TemplateName)
                ?? throw new Exception($"XML is not a valid {nameof(MapTemplateAsset)}. It does not have '{TemplateName}' section in its values.");

            TemplateFilename = mapTemplateValues.Element(nameof(TemplateFilename))?.Value
                ?? throw new Exception($"XML is not a valid {nameof(MapTemplateAsset)}. It does not have '{nameof(TemplateFilename)}' section in its values.");

            EnlargedTemplateFilename = mapTemplateValues.Element(nameof(EnlargedTemplateFilename))?.Value;

            // TemplateRegion defaults to Moderate. If the MapTemplate belongs to another region,
            // it must have TemplateRegion set explicitly within assets.xml.
            TemplateRegionId = mapTemplateValues.Element(nameof(TemplateRegion))?.Value ?? RegionAsset.REGION_MODERATE_REGIONID;

            string? templateMapTypeStr = mapTemplateValues.Element(nameof(TemplateMapType))?.Value;
            if (templateMapTypeStr != null)
                TemplateMapType = MapType.FromName(templateMapTypeStr);
        }
    }
}
