﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates
{
    public record Vector2
    {
        private int Normalize(int x) => (x + 4) / 8 * 8; 

        public int X 
        {
            get => _x;
            set => _x = Normalize(value);
        }
        private int _x = 0;

        public int Y
        {
            get => _y;
            set => _y = Normalize(value);
        }
        private int _y = 0;

        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2(string? position)
        {
            if (position is not null)
            {
                string[] parts = position.Split(' ');
                if (parts.Length == 2)
                {
                    X = int.Parse(parts[0]);
                    Y = int.Parse(parts[1]);
                }
            }
        }

        public Vector2(int[]? numbers)
        {
            if (numbers?.Length >= 2)
            {
                X = numbers[0];
                Y = numbers[1];
            }
        }
    }

    public struct Rect2
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rect2(string? area)
        {
            if (area is not null)
            {
                string[] parts = area.Split(' ');
                if (parts.Length == 4)
                {
                    X = int.Parse(parts[0]);
                    Y = int.Parse(parts[1]);
                    Width = int.Parse(parts[2]) - X;
                    Height = int.Parse(parts[3]) - Y;
                    return;
                }
            }

            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }

        public Rect2(int[]? numbers)
        {
            if (numbers?.Length == 4)
            {
                X = numbers[0];
                Y = numbers[1];
                Width = numbers[2] - X;
                Height = numbers[3] - Y;
                return;
            }

            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }
    }
}
