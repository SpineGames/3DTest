using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace _3DTest._2_0
{
    public class Perlin
    {
        /// adapted from http://cs.nyu.edu/~perlin/noise/
        // JAVA REFERENCE IMPLEMENTATION OF IMPROVED NOISE - COPYRIGHT 2002 KEN PERLIN.

        private static int[] p = new int[512];
        private static int[] permutation = { 151,160,137,91,90,15,
               131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
               190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
               88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
               77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
               102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
               135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
               5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
               223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
               129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
               251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
               49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
               138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
               };

        static Perlin()
        {
            CalculateP();
        }

        private static int _octaves;
        private static int _halfLength = 256;

        public static void SetOctaves(int octaves)
        {
            _octaves = octaves;

            var len = (int)Math.Pow(2, octaves);

            permutation = new int[len];

            Reseed();
        }

        private static void CalculateP()
        {
            p = new int[permutation.Length * 2];
            _halfLength = permutation.Length;

            for (int i = 0; i < permutation.Length; i++)
                p[permutation.Length + i] = p[i] = permutation[i];
        }

        public static void Reseed()
        {
            var random = new Random();
            var perm = Enumerable.Range(0, permutation.Length).ToArray();

            for (var i = 0; i < perm.Length; i++)
            {
                var swapIndex = random.Next(perm.Length);

                var t = perm[i];

                perm[i] = perm[swapIndex];

                perm[swapIndex] = t;
            }

            permutation = perm;

            CalculateP();

        }

        public static float Get(Vector3 position, int octaves, ref float min, ref float max)
        {
            return Get(position.X, position.Y, position.Z, octaves, ref min, ref max);
        }

        public static float Get(float x, float y, float z, int octaves, ref float min, ref float max)
        {
            float frequency = 0.5f;
            float amplitude = 1f;

            var perlin = 0f;
            var octave = 1;

            for (var i = 0; i < octaves; i++)
            {
                var noise = Get(x * octave, y * octave, z * octave);

                perlin += noise / octave;
                
                frequency *= 2;
                amplitude /= 2;

                octave *= 2;
            }

            perlin = Math.Abs((float)Math.Pow(perlin, 2));
            max = Math.Max(perlin, max);
            min = Math.Min(perlin, min);

            //perlin = 1f - 2 * perlin;

            return perlin;
        }

        public static float Get(float x, float y, float z)
        {
            int X = (int)Math.Floor(x) % _halfLength;
            int Y = (int)Math.Floor(y) % _halfLength;
            int Z = (int)Math.Floor(z) % _halfLength;

            if (X < 0)
                X += _halfLength;

            if (Y < 0)
                Y += _halfLength;

            if (Z < 0)
                Z += _halfLength;

            x -= (int)Math.Floor(x);
            y -= (int)Math.Floor(y);
            z -= (int)Math.Floor(z);

            var u = Fade(x);
            var v = Fade(y);
            var w = Fade(z);

            int A = p[X] + Y, AA = p[A] + Z, AB = p[A + 1] + Z,      // HASH COORDINATES OF
                B = p[X + 1] + Y, BA = p[B] + Z, BB = p[B + 1] + Z;      // THE 8 CUBE CORNERS,


            return MathHelper.Lerp(
                    MathHelper.Lerp(
                         MathHelper.Lerp(
                            Grad(p[AA], x, y, z), // AND ADD                            
                            Grad(p[BA], x - 1, y, z),// BLENDED                             
                            u
                            )
                        ,
                        MathHelper.Lerp(
                            Grad(p[AB], x, y - 1, z),  // RESULTS                           
                            Grad(p[BB], x - 1, y - 1, z),                            
                            u
                            )
                        ,
                        v
                    )
                    ,
                    MathHelper.Lerp(
                        MathHelper.Lerp(
                            Grad(p[AA + 1], x, y, z - 1), // CORNERS                            
                            Grad(p[BA + 1], x - 1, y, z - 1), // OF CUBE                            
                            u
                            )
                        ,
                        MathHelper.Lerp(
                            Grad(p[AB + 1], x, y - 1, z - 1),                            
                            Grad(p[BB + 1], x - 1, y - 1, z - 1),                            
                            u
                            ),                        
                        v
                    ),                    
                    w
                );

        }

        public static float[] GetMap(
            int minX, int maxX, int minY, int maxY, int minZ, int maxZ, int octaves, bool reseed)
        {
            int width = maxX - minX;
            int height = maxY - minY;
            int depth = maxZ - minZ;

            float[] data = new float[height * width * depth];
            
            /// track min and max noise value. Used to normalize the result to the 0 to 1.0 range.
            float min = float.MaxValue;
            float max = float.MinValue;

            /// rebuild the permutation table to get a different noise pattern. 
            /// Leave this out if you want to play with changing the number of octaves while 
            /// maintaining the same overall pattern.
            if (reseed)
                Reseed();

            float frequency = 0.5f;
            float amplitude = 1f;
            float persistence = 0.25f;

            for (int octave = 0; octave < octaves; octave++)
            {
                /// parallel loop - easy and fast.
                Parallel.For(0, width * height * height,
                    offset =>
                    {
                        int xxx = offset / (height * depth) % width;
                        int yyy = offset / (depth) % height; //integer division
                        int zzz = offset% depth;

                        //int i = offset % width;
                        //int j = offset / width;
                        //int k = offset / width;

                        float noise = 
                            Get(
                            xxx * frequency * (1f / width), 
                            yyy * frequency * (1f / height),
                            zzz * frequency * (1F / depth));
                        noise = data[offset] += noise * amplitude;

                        min = Math.Min(min, noise);
                        max = Math.Max(max, noise);
                    }
                );

                frequency *= 2;
                amplitude /= 2;
            }

            return data;
        }

        static float Fade(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }

        static float Grad(int hash, float x, float y, float z)
        {
            int h = hash & 15;                      // CONVERT LO 4 BITS OF HASH CODE

            float u = h < 8 ? x : y,                 // INTO 12 GRADIENT DIRECTIONS.
                   v = h < 4 ? y : h == 12 || h == 14 ? x : z;

            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }

    /// <summary>
    /// Implements improved Perlin noise in 2D. 
    /// Transcribed from http://www.siafoo.net/snippet/144?nolinenos#perlin2003
    /// </summary>
    public static class Perlin2D
    {
        private static Random _random = new Random();
        private static int[] _permutation;

        private static Vector2[] _gradients;

        static Perlin2D()
        {
            CalculatePermutation(out _permutation);
            CalculateGradients(out _gradients);
        }

        private static void CalculatePermutation(out int[] p)
        {
            p = Enumerable.Range(0, 256).ToArray();

            /// shuffle the array
            for (var i = 0; i < p.Length; i++)
            {
                var source = _random.Next(p.Length);

                var t = p[i];
                p[i] = p[source];
                p[source] = t;
            }
        }

        /// <summary>
        /// generate a new permutation.
        /// </summary>
        public static void Reseed()
        {
            CalculatePermutation(out _permutation);
        }

        private static void CalculateGradients(out Vector2[] grad)
        {
            grad = new Vector2[256];

            for (var i = 0; i < grad.Length; i++)
            {
                Vector2 gradient;

                do
                {
                    gradient = new Vector2((float)(_random.NextDouble() * 2 - 1), (float)(_random.NextDouble() * 2 - 1));
                }
                while (gradient.LengthSquared() >= 1);

                gradient.Normalize();

                grad[i] = gradient;
            }

        }

        private static float Drop(float t)
        {
            t = Math.Abs(t);
            return 1f - t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Q(float u, float v)
        {
            return Drop(u) * Drop(v);
        }

        public static float Get(float x, float y)
        {
            var cell = new Vector2((float)Math.Floor(x), (float)Math.Floor(y));

            var total = 0f;

            var corners = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

            foreach (var n in corners)
            {
                var ij = cell + n;
                var uv = new Vector2(x - ij.X, y - ij.Y);

                var index = _permutation[(int)ij.X % _permutation.Length];
                index = _permutation[(index + (int)ij.Y) % _permutation.Length];

                var grad = _gradients[index % _gradients.Length];

                total += Q(uv.X, uv.Y) * Vector2.Dot(grad, uv);
            }

            return Math.Max(Math.Min(total, 1f), -1f);
        }

        public static void GenerateNoiseMap(int width, int height, ref Texture2D noiseTexture, int octaves)
        {
            float[] data = new float[width * height];

            /// track min and max noise value. Used to normalize the result to the 0 to 1.0 range.
            float min = float.MaxValue;
            float max = float.MinValue;

            /// rebuild the permutation table to get a different noise pattern. 
            /// Leave this out if you want to play with changing the number of octaves while 
            /// maintaining the same overall pattern.
            Reseed();

            float frequency = 0.5f;
            float amplitude = 1f;
            float persistence = 0.25f;

            for (int octave = 0; octave < octaves; octave++)
            {
                /// parallel loop - easy and fast.
                Parallel.For(0, width * height,
                    offset =>
                    {
                        int i = offset % width;
                        int j = offset / width;
                        float noise = Get(i * frequency * 1f / width, j * frequency * 1f / height);
                        data[j * width + i] += noise * amplitude;
                        noise = data[j * width + i];

                        min = Math.Min(min, noise);
                        max = Math.Max(max, noise);
                    }
                );

                frequency *= 2;
                amplitude /= 2;
            }
            
            Color[] colors = data.Select(
                f => { 
                        var norm = (f - min) / (max - min);
                        return new Color(norm, norm, norm, 1);
                      }
            ).ToArray();

            noiseTexture.SetData(colors);
        }
    }
}