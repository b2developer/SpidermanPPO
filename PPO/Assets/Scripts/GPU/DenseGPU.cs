using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.Layers
{
    public class GPUDense : Layer
    {
        public Matrix w;
        public Matrix b;

        public int batch = 0;

        public MultiplicationGPU forwardModule;
        public MultiplicationGPU batchModule;
        public MultiplicationGPU weightModule;
        public MultiplicationGPU backModule;

        public GPUDense(int inputs, int outputs, int _batch)
        {
            w = new Matrix(outputs, inputs, 1);

            //typically initilised with zeroes
            b = new Matrix(outputs, _batch, 1);

            batch = _batch;

            dCdW = new Matrix(outputs, inputs, 1);
            dCdB = new Matrix(outputs, 1, 1);

            vw = new Matrix(outputs, inputs, 1);
            vb = new Matrix(outputs, 1, 1); //ah yes the victorian variables
            mw = new Matrix(outputs, inputs, 1);
            mb = new Matrix(outputs, 1, 1);

            forwardModule = new MultiplicationGPU(inputs, outputs, 1);
            batchModule = new MultiplicationGPU(inputs, outputs, batch);
            weightModule = new MultiplicationGPU(batch, outputs, inputs);
            backModule = new MultiplicationGPU(outputs, inputs, batch);
        }

        public override Matrix Forward(Matrix x)
        {
            float[] data;

            if (x.width == 1)
            {
                forwardModule.LoadMatrices(w, x);
                data = forwardModule.Compute();
            }
            else
            {
                batchModule.LoadMatrices(w, x);
                data = batchModule.Compute();
            }

            Matrix y = new Matrix(w.height, x.width, 1);

            int items = y.height * y.width;
            int dataItems = data.GetLength(0);

            for (int i = 0; i < y.height; i++)
            {
                for (int j = 0; j < y.width; j++)
                {
                    y.values[i, j, 0] = data[i * y.width + j];
                }
            }

            y = y + b;

            return y;
        }

        //a = input into layer, dCdZ = gradient from next layer
        public override Matrix Back(Matrix a, Matrix dCdZ)
        {
            float averaged = 1 / (float)batch;

            weightModule.LoadMatrices(dCdZ, a.Transpose());
            float[] d1 = weightModule.Compute();

            Matrix dCdW_i = new Matrix(dCdZ.height, a.height, 1);

            for (int i = 0; i < dCdW_i.height; i++)
            {
                for (int j = 0; j < dCdW_i.width; j++)
                {
                    dCdW_i.values[i, j, 0] = d1[i * dCdW_i.width + j];
                }
            }

            Matrix dCdB_i = Matrix.Squash(dCdZ); //dZdB is always 1

            dCdW += dCdW_i * averaged;
            dCdB += dCdB_i * averaged;

            Matrix dCdA = new Matrix(w.width, dCdZ.width, 1);

            backModule.LoadMatrices(w.Transpose(), dCdZ);
            float[] d2 = backModule.Compute();

            for (int i = 0; i < dCdA.height; i++)
            {
                for (int j = 0; j < dCdA.width; j++)
                {
                    dCdA.values[i, j, 0] = d2[i * dCdA.width + j];
                }
            }

            return dCdA;
        }

        //optimisation parameters
        public Matrix dCdW;
        public Matrix dCdB;

        public void Zero()
        {
            dCdW.InitialiseZeroes();
            dCdB.InitialiseZeroes();
        }

        public void ClipNorm(float threshold)
        {
            float dCdWsum = dCdW.AbsSum() + EPSILON;

            if (dCdWsum > threshold)
            {
                dCdW = dCdW * (1 / dCdWsum) * threshold;
            }

            float dCdBsum = dCdB.AbsSum() + EPSILON;

            if (dCdBsum > threshold)
            {
                dCdB = dCdB * (1 / dCdBsum) * threshold;
            }
        }

        public void Vanilla(float learningRate)
        {
            w = w - dCdW * learningRate;
            b = b - Matrix.Expand2(dCdB, batch) * learningRate;
        }

        public Matrix vw;
        public Matrix vb;

        public void RMSProp(float learningRate, float b1 = 0.9f, float epsilon = 1e-9f)
        {
            //moving average
            vw = vw * b1 + Matrix.PairwiseMultiplication(dCdW, dCdW) * (1 - b1);
            w = w - Matrix.PairwiseDivision(dCdW, Matrix.SquareRoot(vw + epsilon)) * learningRate;

            //same operations for bias

            vb = vb * b1 + Matrix.PairwiseMultiplication(dCdB, dCdB) * (1 - b1);

            Matrix vbe = Matrix.PairwiseDivision(dCdB, Matrix.SquareRoot(vb + epsilon));
            vbe = Matrix.Expand2(vbe, batch);

            b = b - vbe * learningRate;
        }

        public Matrix mw;
        public Matrix mb;
        public int i = 0;

        public void Adam(float learningRate, float b1 = 0.9f, float b2 = 0.999f, float epsilon = 1e-8f)
        {
            i++;

            mw = mw * b1 + dCdW * (1 - b1);
            vw = vw * b2 + Matrix.PairwiseMultiplication(dCdW, dCdW) * (1 - b2);

            //bias corrected terms
            Matrix mwh = mw * (1 / (1 - MathF.Pow(b1, i)));
            Matrix vwh = vw * (1 / (1 - MathF.Pow(b2, i)));

            w = w - Matrix.PairwiseDivision(mwh, Matrix.SquareRoot(vwh + epsilon)) * learningRate;

            //same operations for bias

            mb = mb * b1 + dCdB * (1 - b1);
            vb = vb * b2 + Matrix.PairwiseMultiplication(dCdB, dCdB) * (1 - b2);

            Matrix mbh = mb * (1 / (1 - MathF.Pow(b1, i)));
            Matrix vbh = vb * (1 / (1 - MathF.Pow(b2, i)));

            Matrix db = Matrix.PairwiseDivision(mbh, Matrix.SquareRoot(vbh + epsilon));
            db = Matrix.Expand2(db, batch);

            b = b - db * learningRate;
        }
    }
}