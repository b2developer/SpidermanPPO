using System;
using System.Collections;
using System.Collections.Generic;

public class RunningMeanStd
{
    public static float EPSILON = 1e-8f;

    public int size = 0;

    public Matrix mean;
    public Matrix variance;
    public Matrix stdDev;

    public int count = 0;

    public RunningMeanStd(int _size)
    {
        size = _size;

        mean = new Matrix(size, 1, 1);
        variance = new Matrix(size, 1, 1);
        variance.InitialiseNumbers(1.0f);

        stdDev = new Matrix(variance.values);
    }

    public void Update(Matrix input)
    {
        int batch = input.width;
        Matrix sum = Matrix.Squash(input);
        Matrix mean = sum * (1.0f / batch);

        Matrix meanExpanded = Matrix.Expand2(mean, batch);

        Matrix norm = input - meanExpanded;
        norm = Matrix.PairwiseMultiplication(norm, norm);

        Matrix variance = Matrix.Squash(norm);
        variance = variance * (1.0f / batch);

        UpdateState(mean, variance, batch);
    }

    public void UpdateState(Matrix _mean, Matrix _variance,  int _count)
    {
        Matrix delta = _mean - mean;
        int totalCount = count + _count;

        Matrix newMean = mean + delta * (_count / (float)totalCount);
        Matrix ma = variance * count;
        Matrix mb = _variance * _count;
        Matrix m2 = ma + mb + Matrix.PairwiseMultiplication(delta, delta) * ((count * _count) / (float)totalCount);
        Matrix newVariance = m2 * (1.0f / (float)totalCount);

        mean = newMean;
        variance = newVariance;
        stdDev = Matrix.SquareRoot(variance + EPSILON);
        count = totalCount;
    }

    public Matrix Normalise(Matrix input)
    {
        int batch = input.width;

        Matrix meanExpanded = Matrix.Expand2(mean, batch);
        Matrix stdDevExpanded = Matrix.Expand2(stdDev, batch);

        Matrix norm = Matrix.PairwiseDivision(input - meanExpanded, stdDevExpanded);
        return norm;
    }
}
