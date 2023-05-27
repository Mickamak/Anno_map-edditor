﻿using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.ComponentModel;
using System.Windows.Media;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class StartingSpotViewModel : MapElementViewModel
    {
        static readonly SolidColorBrush White = new(Color.FromArgb(255, 255, 255, 255));
        static readonly SolidColorBrush Yellow = new(Color.FromArgb(255, 234, 224, 83));
        static readonly SolidColorBrush Red = new(Color.FromArgb(255, 234, 83, 83));


        private readonly Session _session;

        private readonly StartingSpotElement _startingSpot;

        public SolidColorBrush BackgroundBrush
        {
            get => _backgroundBrush;
            set => SetProperty(ref _backgroundBrush, value);
        }
        private SolidColorBrush _backgroundBrush = White;

        public string Label { get; init; }


        public StartingSpotViewModel(Session session, StartingSpotElement startingSpot)
            : base(startingSpot)
        {
            _session = session;
            _startingSpot = startingSpot;

            Label = _startingSpot.Index switch
            {
                0 => "P",
                1 => "3",
                2 => "1",
                3 => "2",
                _ => _startingSpot.Index.ToString()
            };
            UpdateBackground();

            PropertyChanged += This_PropertyChanged;
            startingSpot.PropertyChanged += StartingSpot_PropertyChanged;
        }

        private void StartingSpot_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StartingSpotElement.Position))
                BoundsCheck();
        }

        private void This_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsSelected))
                UpdateBackground();
        }

        private void UpdateBackground()
        {
            if (IsSelected)
                BackgroundBrush = White;

            else
                BackgroundBrush = _startingSpot.Index == 0 ? Yellow : Red;
        }

        public void BoundsCheck(Vector2? pos = null)
        {
            IsOutOfBounds = !Element.Position.Within(_session.PlayableArea);
        }


        private static Logger<StartingSpotViewModel> LOGGER = new();

        public override void Move(Vector2 delta)
        {
            LOGGER.LogInformation("Moving " + delta);

            // prevent moving StartingSpots outside of the Session's playable area.
            Vector2 newPosition = Element.Position + delta;
            Element.Position = newPosition.Clamp(_session.PlayableArea);
        }
    }
}