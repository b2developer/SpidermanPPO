using System;
using System.Collections;
using System.Collections.Generic;

//algorithm to keep track of the mean and variance using online data
public class WelfordDataSet
{
    public static float EPSILON = 1e-8f;

    public int size;
    public int n;

    public Matrix mean;
    public Matrix variance;
    public Matrix stdDev;

    public WelfordDataSet(int _size, int _n)
    {
        size = _size;
        n = _n;

        mean = new Matrix(_size, 1, 1);
        variance = new Matrix(_size, 1, 1);
        stdDev = new Matrix(_size, 1, 1);

        variance.InitialiseNumbers(1.0f);
        stdDev.InitialiseNumbers(1.0f);
    }

    //wrapper function for batch matrices
    public void AppendBatch(Matrix batch)
    {
        int batchSize = batch.width;

        for (int i = 0; i < batchSize; i++)
        {
            Matrix x = new Matrix(batch.height, 1, 1);

            for (int j = 0; j < size; j++)
            {
                x.values[j, 0, 0] = batch.values[j, i, 0];
            }

            Append(x);
        }
    }

    //main function for the welford mean and variance
    public void Append(Matrix x)
    {
        float invSize = 1.0f / n;
        float sizem1 = n - 1.0f;

        float n2n1 = ((float)n - 2.0f) / sizem1;
        Matrix norm = x - mean;

        variance = variance * n2n1 + Matrix.PairwiseMultiplication(norm, norm) * invSize;
        mean = mean + norm * invSize;

        stdDev = Matrix.SquareRoot(variance);
    }

    //normalises matrix according to current mean and variance estimates
    public Matrix Normalise(Matrix x)
    {
        Matrix meanBatch = Matrix.Expand2(mean, x.width);
        Matrix stdDevBatch = Matrix.Expand2(stdDev, x.width);

        return Matrix.PairwiseDivision((x - meanBatch), stdDevBatch + EPSILON);
    }
}
