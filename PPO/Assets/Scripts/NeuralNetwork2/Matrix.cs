using System;
using System.Collections;
using System.Collections.Generic;

public class Matrix
{
    //3d matrix
    public float[,,] values;

    public int height = 0;
    public int width = 0;
    public int depth = 0; //mainly used for convolutional layers

    public System.Random random;

    public Matrix(int _h, int _w, int _d)
    {
        height = _h;
        width = _w;
        depth = _d;

        values = new float[height, width, depth];

        random = new Random();
    }

    public Matrix(float[,,] _values)
    {
        height = _values.GetLength(0);
        width = _values.GetLength(1);
        depth = _values.GetLength(2);

        values = new float[height, width, depth];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    values[i, j, k] = _values[i, j, k];
                }
            }
        }

        random = new Random();
    }

    public void InitialiseZeroes()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    values[i, j, k] = 0.0f;
                }
            }
        }
    }

    public void InitialiseNumbers(float number)
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    values[i, j, k] = number;
                }
            }
        }
    }

    public void InitialiseRandom(float min, float max)
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    values[i, j, k] = (float)random.NextDouble() * (max - min) + min;
                }
            }
        }
    }

    public static Matrix Expand(Matrix a, int n)
    {
        Matrix b = new Matrix(n, a.width, 1);

        for (int i = 0; i < a.width; i++)
        {
            for (int j = 0; j < a.height; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    b.values[j + k, i, 0] = a.values[j, i, 0];
                }
            }
        }

        return b;
    }

    public static Matrix Expand2(Matrix a, int n)
    {
        Matrix b = new Matrix(a.height, n, 1);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    b.values[i, j + k, 0] = a.values[i, j, 0];
                }
            }
        }

        return b;
    }

    //stacks 1D vectors sideways to form a 2D array
    public static Matrix StackVectors(Matrix[] v)
    {
        int vectors = v.GetLength(0);
        int height = v[0].height;

        Matrix matrix = new Matrix(height, vectors, 1);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < vectors; j++)
            {
                matrix.values[i, j, 0] = v[j].values[i, 0, 0];
            }
        }

        return matrix;
    }

    //stacks 1D vectors to the end of each other to form a longer 1D vector
    public static Matrix StackTensors(Matrix[] m)
    {
        int length = 0;

        for (int i = 0; i < m.GetLength(0); i++)
        {
            length += m[i].width;
        }

        Matrix tensor = new Matrix(1, length, 1);

        int index = 0;

        for (int i = 0; i < m.GetLength(0); i++)
        {
            for (int j = 0; j < m[i].width; j++)
            {
                tensor.values[0, index, 0] = m[i].values[0, j, 0];
                index++;
            }
        }

        return tensor;
    }

    //stacks 2D arrays to for a taller 2D array
    public static Matrix StackMatrices(Matrix[] m)
    {
        int height = 0;

        for (int i = 0; i < m.GetLength(0); i++)
        {
            height += m[i].height;
        }

        Matrix matrix = new Matrix(height, m[0].width, 1);
        int offset = 0;

        for (int x = 0; x < m.GetLength(0); x++)
        {
            for (int i = 0; i < m[x].height; i++)
            {
                for (int j = 0; j < m[x].width; j++)
                {
                    matrix.values[i + offset, j, 0] = m[x].values[i, j, 0];
                }
            }

            offset += m[x].height;
        }

        return matrix;
    }

    public static Matrix Squash(Matrix m)
    {
        Matrix v = new Matrix(m.height, 1, 1);

        for (int i = 0; i < m.height; i++)
        {
            float sum = 0.0f;

            for (int j = 0; j < m.width; j++)
            {
                sum += m.values[i, j, 0];
            }

            v.values[i, 0, 0] = sum;
        }

        return v;
    }

    public static Matrix Squash2(Matrix m)
    {
        Matrix v = new Matrix(1, m.width, 1);

        for (int i = 0; i < m.width; i++)
        {
            float sum = 0.0f;

            for (int j = 0; j < m.height; j++)
            {
                sum += m.values[j, i, 0];
            }

            v.values[0, i, 0] = sum;
        }

        return v;
    }

    public void Clip(float min = -1.0f, float max = 1.0f)
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    values[i, j, k] = Math.Clamp(values[i, j, k], min, max);
                }
            }
        }
    }

    public Matrix Transpose()
    {
        Matrix T = new Matrix(width, height, depth);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    T.values[j, i, k] = values[i, j, k];
                }
            }
        }

        return T;
    }

    //typically used for back propagation through a convolutional layer
    public Matrix FlipHorizontallyAndVertically()
    {
        Matrix T = new Matrix(height, width, depth);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    T.values[height - i - 1, width - j - 1, k] = values[i, j, k];
                }
            }
        }

        return T;
    }

    public static bool IsEqual(Matrix a, Matrix b, float EPSILON = 1e-7f)
    {
        if (a.height != b.height)
        {
            return false;
        }

        if (a.width != b.width)
        {
            return false;
        }

        if (a.depth != b.depth)
        {
            return false;
        }

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    float diff = MathF.Abs(a.values[i, j, k] - b.values[i, j, k]);

                    if (diff > EPSILON)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public static Matrix operator *(Matrix a, float s)
    {
        Matrix b = new Matrix(a.values);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    b.values[i, j, k] *= s;
                }
            }
        }

        return b;
    }

    public static Matrix operator +(Matrix a, float s)
    {
        Matrix c = new Matrix(a.values);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    c.values[i, j, k] += s;
                }
            }
        }

        return c;
    }

    public static Matrix operator -(Matrix a, float s)
    {
        Matrix c = new Matrix(a.values);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    c.values[i, j, k] -= s;
                }
            }
        }

        return c;
    }

    public static Matrix operator +(Matrix a, Matrix b)
    {
        Matrix c = new Matrix(a.values);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    c.values[i, j, k] += b.values[i, j, k];
                }
            }
        }

        return c;
    }

    public static Matrix operator -(Matrix a, Matrix b)
    {
        Matrix c = new Matrix(a.values);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    c.values[i, j, k] -= b.values[i, j, k];
                }
            }
        }

        return c;
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {
        int aw = a.width;
        int bh = b.height;

        int bw = b.width;
        int ah = a.height; //should also be equal

        int ad = a.depth;

        if (aw != bh)
        {
            throw new System.Exception("A's width does not match B's height");
        }

        Matrix c = new Matrix(ah, bw, 1);

        for (int i = 0; i < ah; i++)
        {
            for (int j = 0; j < bw; j++)
            {
                for (int k = 0; k < ad; k++)
                {
                    float sum = 0.0f;

                    for (int s = 0; s < aw; s++)
                    {
                        float val = a.values[i, s, k] * b.values[s, j, k];
                        sum += val;
                    }

                    //assign sum to array
                    c.values[i, j, k] = sum;
                }
            }
        }

        return c;
    }

    public static Matrix PairwiseMultiplication(Matrix a, Matrix b)
    {
        int ah = a.height;
        int aw = a.width;
        int ad = a.depth;

        Matrix c = new Matrix(ah, aw, ad);

        for (int i = 0; i < ah; i++)
        {
            for (int j = 0; j < aw; j++)
            {
                for (int k = 0; k < ad; k++)
                {
                    c.values[i, j, k] = a.values[i, j, k] * b.values[i, j, k];
                }
            }
        }

        return c;
    }

    public static Matrix PairwiseDivision(Matrix a, Matrix b)
    {
        int ah = a.height;
        int aw = a.width;
        int ad = a.depth;

        Matrix c = new Matrix(ah, aw, ad);

        for (int i = 0; i < ah; i++)
        {
            for (int j = 0; j < aw; j++)
            {
                for (int k = 0; k < ad; k++)
                {
                    c.values[i, j, k] = a.values[i, j, k] / b.values[i, j, k];
                }
            }
        }

        return c;
    }

    public static Matrix PairwiseMin(Matrix a, Matrix b)
    {
        Matrix c = new Matrix(a.height, a.width, a.depth);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    c.values[i, j, k] = MathF.Min(a.values[i, j, k], b.values[i, j, k]);
                }
            }
        }

        return c;
    }

    public static Matrix Reciprocal(Matrix a)
    {
        Matrix b = new Matrix(a.height, a.width, a.depth);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    b.values[i, j, k] = 1.0f / a.values[i, j, k];
                }
            }
        }

        return b;
    }

    public static Matrix Pow(Matrix a, float power)
    {
        Matrix b = new Matrix(a.height, a.width, a.depth);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    b.values[i, j, k] = MathF.Pow(a.values[i, j, k], power);
                }
            }
        }

        return b;
    }

    public static Matrix Exp(Matrix a)
    {
        Matrix b = new Matrix(a.height, a.width, a.depth);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    b.values[i, j, k] = MathF.Exp(a.values[i, j, k]);
                }
            }
        }

        return b;
    }

    public static Matrix SquareRoot(Matrix a)
    {
        Matrix b = new Matrix(a.height, a.width, a.depth);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    b.values[i, j, k] = MathF.Sqrt(a.values[i, j, k]);
                }
            }
        }

        return b;
    }

    public static Matrix Abs(Matrix a)
    {
        Matrix b = new Matrix(a.height, a.width, a.depth);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    b.values[i, j, k] = MathF.Abs(a.values[i, j, k]);
                }
            }
        }

        return b;
    }

    public Matrix SignedSquare()
    {
        Matrix c = new Matrix(height, width, depth);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    c.values[i, j, k] = MathF.Abs(values[i, j, k]) * values[i, j, k];
                }
            }
        }

        return c;
    }

    public static Matrix Log(Matrix a)
    {
        Matrix b = new Matrix(a.height, a.width, a.depth);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    b.values[i, j, k] = MathF.Log(a.values[i, j, k]);
                }
            }
        }

        return b;
    }

    public static Matrix Log2(Matrix a, float log2 = 0.30102999566f)
    {
        Matrix b = new Matrix(a.height, a.width, a.depth);

        for (int i = 0; i < a.height; i++)
        {
            for (int j = 0; j < a.width; j++)
            {
                for (int k = 0; k < a.depth; k++)
                {
                    b.values[i, j, k] = MathF.Log(a.values[i, j, k]) / log2;
                }
            }
        }

        return b;
    }

    public float Max()
    {
        float maxValue = float.MinValue;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    if (values[i,j,k] > maxValue)
                    {
                        maxValue = values[i, j, k];
                    }
                }
            }
        }

        return maxValue;
    }

    public float Sum()
    {
        float sum = 0.0f;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    sum += values[i, j, k];
                }
            }
        }

        return sum;
    }

    public float AbsSum()
    {
        float sum = 0.0f;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    sum += MathF.Abs(values[i, j, k]);
                }
            }
        }

        return sum;
    }

    public float SquaredSum()
    {
        float sum = 0.0f;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    sum += values[i, j, k] * values[i, j, k];
                }
            }
        }

        return sum;
    }

    public float Mean()
    {
        float sum = Sum();
        float mean = sum / (height * width * depth);

        return mean;
    }

    public float Variance()
    {
        float mean = Mean();

        Matrix normalised = new Matrix(values);
        normalised = normalised - mean;

        Matrix normalised2 = Matrix.PairwiseMultiplication(normalised, normalised);

        float variance = normalised2.Sum();
        variance /= (height * width * depth);

        return variance;
    }

    //basic convolution with no padding
    public static Matrix Convolution(Matrix x, Matrix f)
    {
        Matrix c = new Matrix(x.height - f.height + 1, x.width - f.width + 1, x.depth);

        for (int k = 0; k < c.depth; k++)
        {
            //--------------------------------------
            for (int i = 0; i < c.height; i++)
            {
                for (int j = 0; j < c.width; j++)
                {
                    float sum = 0.0f;

                    for (int l = 0; l < f.height; l++)
                    {
                        for (int m = 0; m < f.width; m++)
                        {
                            float value = x.values[i + l, j + m, k] * f.values[l, m, k];
                            sum += value;
                        }
                    }

                    c.values[i, j, k] = sum;
                }
            }
            //--------------------------------------
        }

        return c;
    }

    //convolution with maximum padding available to the filter
    public static Matrix FullConvolution(Matrix x, Matrix f)
    {
        Matrix c = new Matrix(x.height + f.height - 1, x.width + f.width - 1, x.depth);

        for (int k = 0; k < c.depth; k++)
        {
            //--------------------------------------
            for (int i = -f.height + 1; i < x.height; i++)
            {
                for (int j = -f.width + 1; j < x.width; j++)
                {
                    float sum = 0.0f;

                    for (int l = 0; l < f.height; l++)
                    {
                        for (int m = 0; m < f.width; m++)
                        {
                            bool ht = i + l >= 0 && i + l < x.height;
                            bool wt = j + m >= 0 && j + m < x.width;

                            float lookup = 0.0f;

                            if (ht && wt)
                            {
                                lookup = x.values[i + l, j + m, k];
                            }

                            float value = lookup * f.values[l, m, k];
                            sum += value;
                        }
                    }

                    c.values[i + f.height - 1, j + f.width - 1, k] = sum;
                }
            }
            //--------------------------------------
        }

        return c;
    }

    public string Serialise()
    {
        string b = "";

        b += height.ToString() + ",";
        b += width.ToString() + ",";
        b += depth.ToString() + ",";

        int counter = height * width * depth;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    b += values[i, j, k].ToString();

                    counter--;

                    if (counter > 0)
                    {
                        b += ",";
                    }
                }
            }
        }

        return b;
    }

    public static Matrix Deserialise(string data)
    {
        string[] values = data.Split(",");

        int height = int.Parse(values[0]);
        int width = int.Parse(values[1]);
        int depth = int.Parse(values[2]);

        Matrix matrix = new Matrix(height, width, depth);

        int counter = 3;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    matrix.values[i, j, k] = float.Parse(values[counter]);
                    counter++;
                }
            }
        }

        return matrix;
    }

    public static Matrix Deserialise(string[] values, ref int pointer)
    {
        int height = int.Parse(values[pointer + 0]);
        int width = int.Parse(values[pointer + 1]);
        int depth = int.Parse(values[pointer + 2]);

        Matrix matrix = new Matrix(height, width, depth);

        int counter = 3;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    matrix.values[i, j, k] = float.Parse(values[pointer + counter]);
                    counter++;
                }
            }
        }

        pointer += counter;

        return matrix;
    }
}
