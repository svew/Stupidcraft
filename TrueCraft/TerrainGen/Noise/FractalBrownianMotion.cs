﻿using System;
using TrueCraft.World;

// TODO: there are no references to this class.  What is it for?
// TODO: remove suppression of nullability warnings.
#pragma warning disable CS8618 // Suppress nullability warnings
namespace TrueCraft.TerrainGen.Noise
{
    public class FractalBrownianMotion : NoiseGen
    {
        // TODO: almost (if not) all of these should be private fields.
        public INoise Noise { get; set; }
        private int OctaveCount;
        public double Persistance { get; set; }
        public double Lacunarity { get; set; }
        private double[] SpectralWeights { get; set; }

        public FractalBrownianMotion(INoise Noise)
        {
            this.Noise = Noise;
            this.Octaves = 2;
            this.Persistance = 1;
            this.Lacunarity = 2;
        }

        public int Octaves
        {
            get { return OctaveCount; }
            set
            {
                //create new spectral weights when the octave count is set
                OctaveCount = value;
                SpectralWeights = new double[value];
                double Frequency = 1.0;
                for (int I = 0; I < Octaves; I++)
                {
                    SpectralWeights[I] = Math.Pow(Frequency, -Persistance);
                    Frequency *= Lacunarity;
                }
            }
        }

        public override double Value2D(double X, double Y)
        {
            // TODO: why are we allocating a new SpectralWeights and putting nothing in it?
            SpectralWeights = new double[Octaves];

            double Total = 0.0;
            double _X = X;
            double _Y = Y;
            for (int I = 0; I < Octaves; I++)
            {
                Total += Noise.Value2D(_X, _Y) * SpectralWeights[I];
                _X *= Lacunarity;
                _Y *= Lacunarity;
            }
            return Total;
        }

        public override double  Value3D(double X, double Y, double Z)
        {
            // TODO: why are we allocating a new SpectralWeights and putting nothing in it?
            SpectralWeights = new double[Octaves];

            double Total = 0.0;
            double _X = X;
            double _Y = Y;
            double _Z = Z;
            for (int I = 0; I < Octaves; I++)
            {
                Total += Noise.Value3D(_X, _Y, _Z) * SpectralWeights[I];
                _X *= Lacunarity;
                _Y *= Lacunarity;
                _Z *= Lacunarity;
            }
            return Total;
        }
    }
}
