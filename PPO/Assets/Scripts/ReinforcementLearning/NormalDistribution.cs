using System;
using System.Collections;
using System.Collections.Generic;

public class NormalDistribution
{
    public static float EPSILON = 1e-8f;

    public float mu = 0.0f;
    public float sigma = 0.0f;

    System.Random random;
    float denom = 0.0f;

    public NormalDistribution(float mean, float stdDev)
    {
        mu = mean;
        sigma = stdDev;

        random = new Random();

        //precalculate to save performance
        denom = MathF.Sqrt(2.0f * MathF.PI);
    }

    //regular normal distribution
    public float Density(float x)
    {
        float f = 1.0f / (sigma * denom);
        float ef = (x - mu) / (sigma);
        float e = MathF.Exp(-0.5f * ef * ef);

        return f * e;
    }

    //log probability
    public float LogDensity(float x)
    {
        float n = x - mu;
        float f = -(n * n) / (2 * sigma * sigma);
        float t = f - MathF.Log(sigma) - MathF.Log(MathF.Sqrt(2 * MathF.PI));

        return t;
    }

    public float Entropy()
    {
        float e = MathF.Log(MathF.Sqrt(2.0f * MathF.PI * MathF.E) * sigma);

        return e;
    }

    //sample random variable from normal distribution
    public float Sample()
    {
        float random1 = (float)random.NextDouble(); //uniform randomness (0,1)
        float random2 = (float)random.NextDouble();

        //box muller transform
        float u1 = 1.0f - random1;
        float u2 = 1.0f - random2;
        float e = MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Sin(2.0f * MathF.PI * u2); //base distribution N(0,1)

        return mu + sigma * e;
    }

    //sample random variable from normal distribution
    public Matrix Sample(int height, int width, int depth)
    {
        Matrix s = new Matrix(height, width, depth);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    s.values[i, j, k] = Sample();
                }
            }
        }

        return s;
    }

    public class Tuple
    {
        public float x = 0.0f;
        public float e = 0.0f;

        public Tuple(float _x, float _e)
        {
            x = _x;
            e = _e;
        }
    }

    //sample random variable from normal distribution
    public Tuple SampleWithBase()
    {
        float random1 = (float)random.NextDouble(); //uniform randomness (0,1)
        float random2 = (float)random.NextDouble();

        //box muller transform
        float u1 = 1.0f - random1;
        float u2 = 1.0f - random2;
        float e = MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Sin(2.0f * MathF.PI * u2); //base distribution N(0,1)
        float x = mu + sigma * e; //location scale transformation

        Tuple tuple = new Tuple(x, e);

        return tuple;
    }

    public Matrix Density(Matrix x)
    {
        Matrix d = new Matrix(x.height, x.width, x.depth);

        for (int i = 0; i < x.height; i++)
        {
            for (int j = 0; j < x.width; j++)
            {
                for (int k = 0; k < x.depth; k++)
                {
                    d.values[i, j, k] = Density(x.values[i, j, k]);
                }
            }
        }

        return d;
    }

    public static Matrix Density(Matrix x, Matrix mu, Matrix sigma)
    {
        Matrix d = new Matrix(x.height, x.width, x.depth);

        for (int i = 0; i < x.height; i++)
        {
            for (int j = 0; j < x.width; j++)
            {
                for (int k = 0; k < x.depth; k++)
                {
                    NormalDistribution distribution = new NormalDistribution(mu.values[i, j, k], sigma.values[i, j, k]);
                    d.values[i, j, k] = distribution.Density(x.values[i, j, k]);
                }
            }
        }

        return d;
    }

    public static Matrix LogDensity(Matrix x, Matrix mu, Matrix sigma)
    {
        Matrix d = new Matrix(x.height, x.width, x.depth);

        for (int i = 0; i < x.height; i++)
        {
            for (int j = 0; j < x.width; j++)
            {
                for (int k = 0; k < x.depth; k++)
                {
                    NormalDistribution distribution = new NormalDistribution(mu.values[i, j, k], sigma.values[i, j, k]);
                    d.values[i, j, k] = distribution.LogDensity(x.values[i, j, k]);
                }
            }
        }

        return d;
    }

    public static Matrix Entropy(Matrix mu, Matrix sigma)
    {
        Matrix e = new Matrix(mu.height, mu.width, mu.depth);

        for (int i = 0; i < mu.height; i++)
        {
            for (int j = 0; j < mu.width; j++)
            {
                for (int k = 0; k < mu.depth; k++)
                {
                    NormalDistribution distribution = new NormalDistribution(mu.values[i, j, k], sigma.values[i, j, k]);
                    e.values[i, j, k] = distribution.Entropy();
                }
            }
        }

        return e;
    }

    //measures the distance between two probability distributions
    public static float KLDivergence(float mu1, float mu2, float sigma1, float sigma2)
    {
        float var1 = sigma1 * sigma1;
        float var2 = sigma2 * sigma2;

        float kl = MathF.Log((sigma2 / (sigma1 + EPSILON)) + EPSILON) + (var1 + (mu1 - mu2) * (mu1 - mu2)) / (2.0f * var2) - 0.5f;
        return kl;
    }

    public static Matrix KLDivergence(Matrix mu1, Matrix mu2, Matrix sigma1, Matrix sigma2)
    {
        Matrix kl = new Matrix(mu1.height, mu1.width, mu1.depth);

        for (int i = 0; i < kl.height; i++)
        {
            for (int j = 0; j < kl.width; j++)
            {
                for (int k = 0; k < kl.depth; k++)
                {
                    kl.values[i, j, k] = KLDivergence(mu1.values[i, j, k], mu2.values[i, j, k], sigma1.values[i, j, k], sigma2.values[i, j, k]);
                }
            }
        }

        return kl;
    }
}
