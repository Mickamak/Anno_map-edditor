﻿using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public abstract class IslandViewModel : MapElementViewModel
    {
        static readonly Dictionary<string, SolidColorBrush> BorderBrushes = new()
        {
            ["Normal"] = new(Color.FromArgb(255, 8, 172, 137)),
            ["Starter"] = new(Color.FromArgb(255, 130, 172, 8)),
            ["ThirdParty"] = new(Color.FromArgb(255, 189, 73, 228)),
            ["Decoration"] = new(Color.FromArgb(255, 151, 162, 125)),
            ["PirateIsland"] = new(Color.FromArgb(255, 186, 0, 36)),
            ["Cliff"] = new(Color.FromArgb(255, 103, 105, 114)),
            ["Selected"] = new(Color.FromArgb(255, 255, 255, 255))
        };
        static readonly Dictionary<string, SolidColorBrush> BackgroundBrushes = new()
        {
            ["Normal"] = new(Color.FromArgb(32, 8, 172, 137)),
            ["Starter"] = new(Color.FromArgb(32, 130, 172, 8)),
            ["ThirdParty"] = new(Color.FromArgb(32, 189, 73, 228)),
            ["Decoration"] = new(Color.FromArgb(32, 151, 162, 125)),
            ["PirateIsland"] = new(Color.FromArgb(32, 186, 0, 36)),
            ["Cliff"] = new(Color.FromArgb(32, 103, 105, 114)),
            ["Selected"] = new(Color.FromArgb(32, 255, 255, 255))
        };


        private readonly Session _session;

        public IslandElement Island { get; init; }

        public SolidColorBrush BackgroundBrush
        {
            get => _backgroundBrush;
            set => SetProperty(ref _backgroundBrush, value);
        }
        private SolidColorBrush _backgroundBrush = BackgroundBrushes["Normal"];

        public SolidColorBrush BorderBrush
        {
            get => _borderBrush;
            set => SetProperty(ref _borderBrush, value);
        }
        private SolidColorBrush _borderBrush = BorderBrushes["Normal"];

        public bool IsOutOfBounds
        {
            get => _isOutOfBounds;
            set => SetProperty(ref _isOutOfBounds, value);
        }
        private bool _isOutOfBounds;

        public abstract string? Label { get; }

        public abstract int SizeInTiles { get; }

        public virtual BitmapImage? Thumbnail { get; }

        public virtual int ThumbnailRotation { get; }


        public IslandViewModel(Session session, IslandElement island)
            : base(island)
        {
            _session = session;
            Island = island;

            UpdateBackground();

            PropertyChanged += This_PropertyChanged;
            Island.PropertyChanged += RandomIsland_PropertyChanged;
        }


        private void This_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsSelected))
                UpdateBackground();
        }

        private void RandomIsland_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IslandElement.IslandType))
                UpdateBackground();
        }

        private void UpdateBackground()
        {
            if (IsSelected)
            {
                BorderBrush = BorderBrushes["Selected"];
                BackgroundBrush = BackgroundBrushes["Selected"];
            }
            else
            {
                BorderBrush = BorderBrushes[Island.IslandType.Name];
                BackgroundBrush = BackgroundBrushes[Island.IslandType.Name];
            }
        }

        public override void OnDragged(Vector2 newPosition)
        {
            // mark the island if it is out of bounds
            var mapArea = new Rect2(_session.Size - SizeInTiles + Vector2.Tile);
            IsOutOfBounds = !newPosition.Within(mapArea);

            base.OnDragged(newPosition);
        }
    }
}