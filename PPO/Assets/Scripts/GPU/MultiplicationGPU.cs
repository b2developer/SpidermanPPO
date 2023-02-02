using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplicationGPU
{
    public static int THREADS = 256;

    public ComputeShader shader;

    public float[] arrayA;
    public float[] arrayB;

    public float[] arrayResult;

    public int aHeight = 0;
    public int bWidth = 0;
    public int length = 0;

    public MultiplicationGPU(int _length, int _aHeight, int _bWidth)
    {
        shader = PhysicsManager.matrixMultiplicationShader;

        //a's width is also b's height

        arrayA = new float[_length * _aHeight];
        arrayB = new float[_bWidth * _length];

        arrayResult = new float[_bWidth * _aHeight];

        aHeight = _aHeight;
        bWidth = _bWidth;
        length = _length;
    }

    public void LoadMatrices(Matrix A, Matrix B)
    {
        for (int i = 0; i < A.height; i++)
        {
            for (int j = 0; j < A.width; j++)
            {
                arrayA[j + i * A.width] = A.values[i, j, 0];
            }
        }

        for (int i = 0; i < B.height; i++)
        {
            for (int j = 0; j < B.width; j++)
            {
                arrayB[j + i * B.width] = B.values[i, j, 0];
            }
        }
    }

    public float[] Compute()
    {
        int handle = shader.FindKernel("MatMul");

        shader.SetInt("AH", aHeight);
        shader.SetInt("BW", bWidth);
        shader.SetInt("N", length);

        int aLength = arrayA.GetLength(0);
        ComputeBuffer bufferA = new ComputeBuffer(aLength, sizeof(float));
        bufferA.SetData(arrayA);
        shader.SetBuffer(handle, "bufferA", bufferA);

        int bLength = arrayB.GetLength(0);
        ComputeBuffer bufferB = new ComputeBuffer(bLength, sizeof(float));
        bufferB.SetData(arrayB);
        shader.SetBuffer(handle, "bufferB", bufferB);

        int resultLength = arrayResult.GetLength(0);
        ComputeBuffer bufferResult = new ComputeBuffer(resultLength, sizeof(float));
        bufferResult.SetData(arrayResult);
        shader.SetBuffer(handle, "result", bufferResult);

        int blocks = Mathf.CeilToInt(resultLength / (float)THREADS);

        shader.Dispatch(handle, blocks, 1, 1);

        bufferA.Dispose();
        bufferB.Dispose();

        bufferResult.GetData(arrayResult);
        bufferResult.Dispose();

        return arrayResult;
    }
}
