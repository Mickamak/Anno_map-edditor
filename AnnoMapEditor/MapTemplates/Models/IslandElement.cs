﻿using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Utilities;
using IslandType = AnnoMapEditor.MapTemplates.Enums.IslandType;

namespace AnnoMapEditor.MapTemplates.Models
{
    public abstract class IslandElement : MapElement
    {
        public string? Label
        { 
            get => _label;
            set {
                if (value == "")
                    value = null;

                SetProperty(ref _label, value);
            }
        }
        private string? _label;

        public IslandType IslandType
        {
            get => _islandType;
            set => SetProperty(ref _islandType, value);
        }
        private IslandType _islandType;

        public IslandDifficulty? IslandDifficulty
        {
            get => _islandDifficulty;
            set => SetProperty(ref _islandDifficulty, value);
        }
        private IslandDifficulty? _islandDifficulty;

        public int SizeInTiles
        {
            get => _sizeInTiles;
            protected set
            {
                SetProperty(ref _sizeInTiles, value);
                Area = new(Position, new Vector2(value, value));
            }
        }
        private int _sizeInTiles;

        public Rect2 Area
        {
            get => _area;
            private set => SetProperty(ref _area, value);
        }
        private Rect2 _area;



        public IslandElement(IslandType islandType)
        {
            _islandType = islandType;

            PropertyChanged += This_PropertyChanged;
        }


        private void This_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Position))
                UpdateArea();
        }

        private void UpdateArea()
        {
            Area = new(Position, new Vector2(_sizeInTiles, _sizeInTiles));
        }


        // ---- Serialization ----

        public IslandElement(Element sourceTemplate)
            : base(sourceTemplate)
        {
            _label            = sourceTemplate.IslandLabel != null ? (string)sourceTemplate.IslandLabel : null;
            _islandType       = IslandType.FromElementValue(sourceTemplate.Config?.Type?.id ?? sourceTemplate.RandomIslandConfig?.value?.Type?.id);
            _islandDifficulty = IslandDifficulty.FromElementValue(sourceTemplate.Config?.Difficulty?.id ?? sourceTemplate.RandomIslandConfig?.value?.Difficulty?.id);
        }

        protected override void ToTemplate(Element resultElement)
        {
            if (_label != null)
                resultElement.IslandLabel = _label;
        }
    }
}
