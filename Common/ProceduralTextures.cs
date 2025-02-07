using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using Terraria;
using Microsoft.Xna.Framework;

public static class ProceduralTextures
{
    public static Dictionary<int, Texture2D> AllProceduralTextures = new Dictionary<int, Texture2D>();

    public static Texture2D GetTexFromItem(Item item)
    {
        if (!AllProceduralTextures.ContainsKey(item.type))
        {
            CreateTextureStamp(item);
        }
        return AllProceduralTextures[item.type];
    }

    private static int GetSeedFromName(string name)
    {
        int hash = 0;
        foreach (char c in name)
        {
            hash = (hash * 31) + c;
        }
        return Math.Abs(hash);
    }

    public static void CreateTextureStamp(Item item)
    {
        try
        {
            int pos = GetSeedFromName(item.Name);
            Random seededRandom = new Random();
            PerlinNoise noise = new PerlinNoise(pos);
            Texture2D newTexture = new Texture2D(Main.graphics.GraphicsDevice, 40, 40);
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;

            Color[] itemTextureData = new Color[itemTexture.Width * itemTexture.Height];
            itemTexture.GetData(itemTextureData);

            List<Vector3> hslColors = new List<Vector3>();
            foreach (Color color in itemTextureData)
            {
                if (color.A > 0)
                {
                    Vector3 hsl = ColorToHSL(color);
                    if (hsl.Z > 0.2f)
                    {
                        hslColors.Add(hsl);
                    }
                }
            }

            if (hslColors.Count == 0)
            {
                hslColors.Add(new Vector3(0, 0, 1));
            }

            hslColors.Sort((a, b) => a.X.CompareTo(b.X));

            Color[] newTextureData = new Color[40 * 40];

            float frequency = 0.02f;
            float octaves = 2;
            float persistence = 0.8f;
            float lacunarity = 1.0f;

            for (int y = 0; y < 40; y++)
            {
                for (int x = 0; x < 40; x++)
                {
                    float noiseValue = 0;
                    float freq = frequency;
                    float amp = 1f;

                    for (int i = 0; i < octaves; i++)
                    {
                        noiseValue += noise.Noise(x * freq, y * freq, seededRandom.Next()) * amp;
                        freq *= lacunarity;
                        amp *= persistence;
                    }

                    noiseValue = (noiseValue + 1) / 2;

                    int colorIndex = (int)(noiseValue * (hslColors.Count - 1));
                    colorIndex = Math.Max(0, Math.Min(colorIndex, hslColors.Count - 1));

                    Color finalColor = HSLToColor(hslColors[colorIndex]);

                    int index = y * 40 + x;
                    newTextureData[index] = finalColor;
                }
            }

            newTexture.SetData(newTextureData);

            if (AllProceduralTextures.ContainsKey(item.type))
            {
                AllProceduralTextures[item.type] = newTexture;
            }
            else
            {
                AllProceduralTextures.Add(item.type, newTexture);
            }
        }
        catch (Exception e)
        {
            Main.NewText($"Error: {e.Message}", Color.Red);
        }
    }

    private static Vector3 ColorToHSL(Color color)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;
        float max = Math.Max(Math.Max(r, g), b);
        float min = Math.Min(Math.Min(r, g), b);
        float h = 0, s = 0, l = (max + min) / 2;

        if (max != min)
        {
            float d = max - min;
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
        float h = hsl.X;
        float s = hsl.Y;
        float l = hsl.Z;

        float r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
            float p = 2 * l - q;

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
    private int[] permutations;
    private int[] gradients;

    public PerlinNoise(int seed)
    {
        try
        {
            permutations = CreatePermutations(seed);
            gradients = CreateGradients(seed);
        }
        catch (Exception e)
        {
            Main.NewText($"Error initializing PerlinNoise: {e.Message}", Color.Red);
            permutations = new int[512];
            gradients = new int[512];
        }
    }

    private int[] CreatePermutations(int seed)
    {
        int[] p = new int[512];
        for (int i = 0; i < 256; i++)
            p[i] = p[i + 256] = i;

        Random rand = new Random(seed);
        for (int i = 1; i < 256; i++)
        {
            int j = rand.Next(i, 256);
            int temp = p[i];
            p[i] = p[j];
            p[j] = temp;
        }

        return p;
    }

    private int[] CreateGradients(int seed)
    {
        int[] g = new int[512];
        Random rand = new Random(seed);
        for (int i = 0; i < 256; i++)
        {
            g[i] = g[i + 256] = rand.Next(0, 32896);
        }
        return g;
    }

    private float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    private float Grad(int hash, float x, float y)
    {
        int h = hash & 3;
        float u = h < 2 ? x : y;
        float v = h < 2 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    public float Noise(float x, float y, int seed)
    {
        Random rand = new Random(seed);
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;
        x -= (int)Math.Floor(x);
        y -= (int)Math.Floor(y);

        int aa = permutations[X] + Y;
        int ba = permutations[X + 1] + Y;
        int ab = permutations[X] + Y + 1;
        int bb = permutations[X + 1] + Y + 1;

        float x1 = Fade(x);
        float y1 = Fade(y);

        float n00 = Grad(gradients[aa], x, y);
        float n01 = Grad(gradients[ab], x, y - 1);
        float n10 = Grad(gradients[ba], x - 1, y);
        float n11 = Grad(gradients[bb], x - 1, y - 1);

        float x2 = Lerp(x1, n00, n10);
        float x3 = Lerp(x1, n01, n11);

        return Lerp(y1, x2, x3);
    }
}