using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

public static class ProceduralTextures
{
    private static readonly Dictionary<int, Texture2D> AllProceduralTextures = new();

    public static Texture2D GetTexFromItem(Item item)
    {
        if (!AllProceduralTextures.ContainsKey(item.type)) CreateTextureStamp(item);
        return AllProceduralTextures[item.type];
    }

    private static int GetSeedFromName(string name)
    {
        var hash = 0;
        foreach (var c in name) hash = hash * 31 + c;
        return Math.Abs(hash);
    }

    private static void CreateTextureStamp(Item item)
    {
        try
        {
            var pos = GetSeedFromName(item.Name);
            var seededRandom = new Random();
            var noise = new PerlinNoise(pos);
            var newTexture = new Texture2D(Main.graphics.GraphicsDevice, 40, 40);
            var itemTexture = TextureAssets.Item[item.type].Value;

            var itemTextureData = new Color[itemTexture.Width * itemTexture.Height];
            itemTexture.GetData(itemTextureData);

            var hslColors = new List<Vector3>();
            foreach (var color in itemTextureData)
                if (color.A > 0)
                {
                    var hsl = ColorToHSL(color);
                    if (hsl.Z > 0.2f) hslColors.Add(hsl);
                }

            if (hslColors.Count == 0) hslColors.Add(new Vector3(0, 0, 1));

            hslColors.Sort((a, b) => a.X.CompareTo(b.X));

            var newTextureData = new Color[40 * 40];

            const float frequency = 0.02f;
            const float octaves = 2;
            const float persistence = 0.8f;
            const float lacunarity = 1.0f;

            for (var y = 0; y < 40; y++)
            for (var x = 0; x < 40; x++)
            {
                float noiseValue = 0;
                var freq = frequency;
                var amp = 1f;

                for (var i = 0; i < octaves; i++)
                {
                    noiseValue += noise.Noise(x * freq, y * freq, seededRandom.Next()) * amp;
                    freq *= lacunarity;
                    amp *= persistence;
                }

                noiseValue = (noiseValue + 1) / 2;

                var colorIndex = (int)(noiseValue * (hslColors.Count - 1));
                colorIndex = Math.Max(0, Math.Min(colorIndex, hslColors.Count - 1));

                var finalColor = HSLToColor(hslColors[colorIndex]);

                var index = y * 40 + x;
                newTextureData[index] = finalColor;
            }

            newTexture.SetData(newTextureData);

            AllProceduralTextures[item.type] = newTexture;
        }
        catch (Exception e)
        {
            Main.NewText($"Error: {e.Message}", Color.Red);
        }
    }

    private static Vector3 ColorToHSL(Color color)
    {
        var r = color.R / 255f;
        var g = color.G / 255f;
        var b = color.B / 255f;
        var max = Math.Max(Math.Max(r, g), b);
        var min = Math.Min(Math.Min(r, g), b);
        float h = 0, s = 0, l = (max + min) / 2;

        if (max != min)
        {
            var d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

            if (max == r)
                h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g)
                h = (b - r) / d + 2;
            else if (max == b)
                h = (r - g) / d + 4;

            h /= 6f;
        }

        return new Vector3(h, s, l);
    }

    private static Color HSLToColor(Vector3 hsl)
    {
        var h = hsl.X;
        var s = hsl.Y;
        var l = hsl.Z;

        float r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            var q = l < 0.5f ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;

            r = HueToRGB(p, q, h + 1f / 3f);
            g = HueToRGB(p, q, h);
            b = HueToRGB(p, q, h - 1f / 3f);
        }

        return new Color((int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

    private static float HueToRGB(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
}

public class PerlinNoise
{
    private readonly int[] _gradients;
    private readonly int[] _permutations;

    public PerlinNoise(int seed)
    {
        try
        {
            _permutations = CreatePermutations(seed);
            _gradients = CreateGradients(seed);
        }
        catch (Exception e)
        {
            Main.NewText($"Error initializing PerlinNoise: {e.Message}", Color.Red);
            _permutations = new int[512];
            _gradients = new int[512];
        }
    }

    private static int[] CreatePermutations(int seed)
    {
        var p = new int[512];
        for (var i = 0; i < 256; i++)
            p[i] = p[i + 256] = i;

        var rand = new Random(seed);
        for (var i = 1; i < 256; i++)
        {
            var j = rand.Next(i, 256);
            (p[i], p[j]) = (p[j], p[i]);
        }

        return p;
    }

    private static int[] CreateGradients(int seed)
    {
        var g = new int[512];
        var rand = new Random(seed);
        for (var i = 0; i < 256; i++) g[i] = g[i + 256] = rand.Next(0, 32896);
        return g;
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    private static float Grad(int hash, float x, float y)
    {
        var h = hash & 3;
        var u = h < 2 ? x : y;
        var v = h < 2 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    public float Noise(float x, float y, int seed)
    {
        //var rand = new Random(seed);
        var gridX = (int)Math.Floor(x) & 255;
        var gridY = (int)Math.Floor(y) & 255;
        x -= (int)Math.Floor(x);
        y -= (int)Math.Floor(y);

        var aa = _permutations[gridX] + gridY;
        var ba = _permutations[gridX + 1] + gridY;
        var ab = _permutations[gridX] + gridY + 1;
        var bb = _permutations[gridX + 1] + gridY + 1;

        var x1 = Fade(x);
        var y1 = Fade(y);

        var n00 = Grad(_gradients[aa], x, y);
        var n01 = Grad(_gradients[ab], x, y - 1);
        var n10 = Grad(_gradients[ba], x - 1, y);
        var n11 = Grad(_gradients[bb], x - 1, y - 1);

        var x2 = Lerp(x1, n00, n10);
        var x3 = Lerp(x1, n01, n11);

        return Lerp(y1, x2, x3);
    }
}