﻿using System;

namespace AnnoMapEditor.MapTemplates
{
    public struct IslandType
    {
        public static readonly IslandType Normal = new("Normal");
        public static readonly IslandType Starter = new("Starter");
        public static readonly IslandType ThirdParty = new("ThirdParty");
        public static readonly IslandType Decoration = new("Decoration");
        public static readonly IslandType PirateIsland = new("PirateIsland");
        public static readonly IslandType Cliff = new("Cliff");

        private static readonly string[] RandomIDs = new[]
        {
            "Normal",
            "Starter",
            "Decoration",
            "ThirdParty",
            "PirateIsland",
            "Cliff",
            "Normal" // clamp
        };

        private readonly string value;

        public IslandType(string? type)
        {
            if (type == "Starter" || type == "ThirdParty" || type == "Decoration")
            {
                value = type;
                return;
            }
            if (type == "PirateIsland" || type == "Pirate")
            {
                value = "PirateIsland";
                return;
            }
            if (type == "Cliff")
            {
                value = "Cliff";
                return;
            }

            value = "Normal";
        }

        public IslandType(short? type)
        {
            if (type is null)
            {
                value = "Normal";
                return;
            }

            value = RandomIDs[Math.Clamp((int)type, 0, RandomIDs.Length - 1)];
        }

        public override string ToString() => value;

        public override bool Equals(object? obj) => obj is IslandType other && value.Equals(other.value);
        public override int GetHashCode() => value.GetHashCode();

        public static bool operator !=(IslandType a, IslandType b) => !a.Equals(b);
        public static bool operator ==(IslandType a, IslandType b) => a.Equals(b);
    }
}