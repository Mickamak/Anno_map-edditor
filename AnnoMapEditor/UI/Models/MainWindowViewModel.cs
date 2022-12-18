﻿using AnnoMapEditor.DataArchives;
using AnnoMapEditor.MapTemplates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.UI.Models
{
    public class DataPathStatus
    {
        public string Status { get; set; } = string.Empty;
        public string? ToolTip { get; set; }
        public Visibility AutoDetect { get; set; } = Visibility.Collapsed;
        public Visibility Configure { get; set; } = Visibility.Visible;
        public string ConfigureText { get; set; } = string.Empty;
    }

    public class ExportStatus
    {
        public bool CanExportAsMod { get; set; }
        public string ExportAsModText { get; set; } = "";
    }

    public class MapGroup
    {
        public string Name;
        public List<MapInfo> Maps;

        public MapGroup(string name, IEnumerable<string> mapFiles, Regex regex)
        {
            Name = name;
            Maps = mapFiles.Select(x => new MapInfo()
            {
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    string.Join(' ', regex.Match(x).Groups.Values.Skip(1).Select(y => y.Value)).Replace("_", " ")),
                FileName = x
            }).ToList();
        }
    }

    public class MapInfo
    {
        public string? Name;
        public string? FileName;
    }

    public class MainWindowViewModel : ViewModelBase
    {
        public Session? Session
        {
            get => _session;
            private set
            {
                if (value != _session)
                {
                    SetProperty(ref _session, value, new string[] { nameof(CanExport) });
                    SelectedElement = null;

                    if(SessionProperties is not null) 
                        SessionProperties.SelectedRegionChanged -= SelectedRegionChanged;

                    SessionProperties = value is null ? null : new(value);
                    OnPropertyChanged(nameof(SessionProperties));

                    if(SessionProperties is not null)
                        SessionProperties.SelectedRegionChanged += SelectedRegionChanged;

                    SessionChecker = value is null ? null : new(value);
                    OnPropertyChanged(nameof(SessionChecker));
                }
            }
        }
        private Session? _session;
        public bool CanExport => _session is not null;
        public SessionPropertiesViewModel? SessionProperties { get; private set; }
        public SessionChecker? SessionChecker { get; private set; }

        public MapElement? SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }
        private MapElement? _selectedElement;

        public string? SessionFilePath
        {
            get => _sessionFilePath;
            private set => SetProperty(ref _sessionFilePath, value);
        }
        private string? _sessionFilePath;

        public DataPathStatus DataPathStatus
        {
            get => _dataPathStatus;
            private set => SetProperty(ref _dataPathStatus, value);
        }
        private DataPathStatus _dataPathStatus = new();

        public ExportStatus ExportStatus
        {
            get => _exportStatus;
            private set => SetProperty(ref _exportStatus, value);
        }
        private ExportStatus _exportStatus = new();

        public List<MapGroup>? Maps
        {
            get => _maps;
            private set => SetProperty(ref _maps, value);
        }
        private List<MapGroup>? _maps;

        public Settings Settings { get; private set; }

        public MainWindowViewModel(Settings settings)
        {
            Settings = settings;
            Settings.PropertyChanged += Settings_PropertyChanged;

            // trigger once ourselves
            Settings_PropertyChanged(this, new PropertyChangedEventArgs("IsValidDataPath"));
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsValidDataPath":
                    UpdateStatusAndMenus();
                    break;
                case "DataArchive":
                    UpdateStatusAndMenus();
                    break;
            }
        }

        private void SelectedRegionChanged(object? sender, EventArgs _)
        {
            UpdateExportStatus();
        }

        public async Task OpenMap(string filePath, bool fromArchive = false)
        {
            SessionFilePath = Path.GetFileName(filePath);

            if (fromArchive)
            {
                Stream? fs = Settings?.DataArchive.OpenRead(filePath);
                if (fs is not null)
                    Session = await Session.FromA7tinfoAsync(fs, filePath);
            }
            else
            {
                if (Path.GetExtension(filePath).ToLower() == ".a7tinfo")
                    Session = await Session.FromA7tinfoAsync(filePath);
                else
                    Session = await Session.FromXmlAsync(filePath);
            }

            UpdateExportStatus();
        }

        public void CreateNewMap()
        {
            const int DEFAULT_MAP_SIZE = 2560;
            const int DEFAULT_PLAYABLE_SIZE = 2160;

            SessionFilePath = null;

            Session = Session.FromNewMapDimensions(DEFAULT_MAP_SIZE, DEFAULT_PLAYABLE_SIZE, Region.Moderate);

            UpdateExportStatus();
        }

        public async Task SaveMap(string filePath)
        {
            if (Session is null)
                return;

            SessionFilePath = Path.GetFileName(filePath);

            if (Path.GetExtension(filePath).ToLower() == ".a7tinfo")
                await Session.SaveAsync(filePath, false);
            else
                await Session.SaveToXmlAsync(filePath);
        }

        private void UpdateExportStatus()
        {
            if (Settings.IsLoading)
            {
                // still loading
                ExportStatus = new ExportStatus()
                {
                    CanExportAsMod = false,
                    ExportAsModText = "(loading RDA...)"
                };
            }
            else if (Settings.IsValidDataPath)
            {
                bool supportedFormat = Mods.Mod.CanSave(Session);
                bool archiveReady = Settings.DataArchive is RdaDataArchive;

                ExportStatus = new ExportStatus()
                {
                    CanExportAsMod = archiveReady && supportedFormat,
                    ExportAsModText = archiveReady ? supportedFormat ? "As playable mod..." : "As mod: only works with Old World maps currently" : "As mod: set game path to save"
                };
            }
            else
            {
                ExportStatus = new ExportStatus()
                {
                    ExportAsModText = "As mod: set game path to save",
                    CanExportAsMod = false
                };
            }
        }

        private void UpdateStatusAndMenus()
        {
            if (Settings.IsLoading)
            {
                // still loading
                DataPathStatus = new DataPathStatus()
                {
                    Status = "loading RDA...",
                    ToolTip = "",
                    Configure = Visibility.Collapsed,
                    AutoDetect = Visibility.Collapsed,
                };
            }
            else if (Settings.IsValidDataPath)
            {
                DataPathStatus = new DataPathStatus()
                {
                    Status = Settings.DataArchive is RdaDataArchive ? "Game path set ✔" : "Extracted RDA path set ✔",
                    ToolTip = Settings.DataArchive.Path,
                    ConfigureText = "Change...",
                    AutoDetect = Settings.DataArchive is RdaDataArchive ? Visibility.Collapsed : Visibility.Visible,
                };

                Dictionary<string, Regex> templateGroups = new()
                {
                    ["DLCs"] = new(@"data\/(?!=sessions\/)([^\/]+)"),
                    ["Moderate"] = new(@"data\/sessions\/.+moderate"),
                    ["New World"] = new(@"data\/sessions\/.+colony01")
                };

                var mapTemplates = Settings.DataArchive.Find("**/*.a7tinfo");

                Maps = new()
                {
                    new MapGroup("Campaign", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/campaign")), new(@"\/campaign_([^\/]+)\.")),
                    new MapGroup("Moderate, Archipelago", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_archipel")), new(@"\/([^\/]+)\.")),
                    new MapGroup("Moderate, Atoll", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_atoll")), new(@"\/([^\/]+)\.")),
                    new MapGroup("Moderate, Corners", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_corners")), new(@"\/([^\/]+)\.")),
                    new MapGroup("Moderate, Island Arc", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_islandarc")), new(@"\/([^\/]+)\.")),
                    new MapGroup("Moderate, Snowflake", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_snowflake")), new(@"\/([^\/]+)\.")),
                    new MapGroup("New World, Large", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_l_")), new(@"\/([^\/]+)\.")),
                    new MapGroup("New World, Medium", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_m_")), new(@"\/([^\/]+)\.")),
                    new MapGroup("New World, Small", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_s_")), new(@"\/([^\/]+)\.")),
                    new MapGroup("DLCs", mapTemplates.Where(x => !x.StartsWith(@"data/sessions/")), new(@"data\/([^\/]+)\/.+\/maps\/([^\/]+)"))
                    //new MapGroup("Moderate", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate")), new(@"\/([^\/]+)\."))
                };
            }
            else
            {
                DataPathStatus = new DataPathStatus()
                {
                    Status = "⚠ Game or RDA path not valid.",
                    ToolTip = null,
                    ConfigureText = "Select...",
                    AutoDetect = Visibility.Visible,
                };

                Maps = new();
            }

            UpdateExportStatus();
        }
    }
}
