using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.Layers
{
    public class BatchNormalisation : Layer
    {
        public Matrix scale; //Y (gamma)
        public Matrix shift; //B (beta)

        public int inputs = 0;
        public int batch = 0;

        //cache variables
        public Matrix _normalised;
        public Matrix _stdDev;
        public Matrix _xHat;

        //running variables
        public Matrix runningMean;
        public Matrix runningVariance;

        public float momentum = 0.9f;

        public BatchNormalisation(int _inputs, int _batch)
        {
            scale = new Matrix(_inputs, _batch, 1);
            scale.InitialiseNumbers(1.0f);

            shift = new Matrix(_inputs, _batch, 1);
            shift.InitialiseNumbers(0.0f);

            inputs = _inputs;
            batch = _batch;

            runningMean = new Matrix(inputs, 1, 1);
            runningVariance = new Matrix(inputs, 1, 1);

            runningVariance.InitialiseNumbers(1.0f);

            dCdB = new Matrix(inputs, 1, 1); //shift
            dCdY = new Matrix(inputs, 1, 1); //scale

            vb = new Matrix(inputs, 1, 1);
            vy = new Matrix(inputs, 1, 1);
            mb = new Matrix(inputs, 1, 1);
            my = new Matrix(inputs, 1, 1);
        }

        public override Matrix Forward(Matrix x)
        {
            int size = x.width;

            bool isTesting = size == 1;

            if (isTesting)
            {
                //testing mode, use the running mean and variance to perform the batch normalisation

                Matrix normalised = x - runningMean;
                Matrix stdDev = Matrix.SquareRoot(runningVariance);
                Matrix xHat = Matrix.PairwiseDivision(normalised, stdDev);
                Matrix bn = (Matrix.PairwiseMultiplication(xHat, scale)) + shift; //scale and shift

                return bn;
            }
            else
            {
                //learning mode, calculate running mean and variance using mean and variance for the batch

                float averaged = 1 / (float)size;

                Matrix sum = Matrix.Squash(x);
                Matrix mean = sum * averaged;
                Matrix batchMean = Matrix.Expand2(mean, size);

                _normalised = x - batchMean; //subtract mean
                Matrix variance = Matrix.PairwiseMultiplication(_normalised, _normalised); //square
                variance = Matrix.Squash(variance); //sum all 
                variance = variance * averaged; //average
                Matrix batchVariance = Matrix.Expand2(variance, size);

                _stdDev = Matrix.SquareRoot(batchVariance + EPSILON);
                _xHat = Matrix.PairwiseDivision(_normalised, _stdDev); //scale parameters to have mean of zero and sd of 1
                Matrix bn = (Matrix.PairwiseMultiplication(_xHat, scale)) + shift; //scale and shift

                //calculate running mean and variance
                runningMean = runningMean * momentum + mean * (1 - momentum);
                runningVariance = runningVariance * momentum + variance * (1 - momentum);

                return bn;
            }
        }

        //a = input into layer, dCdA = gradient from next layer
        public override Matrix Back(Matrix a, Matrix gradient)
        {
            float averaged = 1 / (float)batch;

            Matrix dCdY_i = Matrix.Squash(Matrix.PairwiseMultiplication(gradient, _xHat));
            Matrix dCdB_i = Matrix.Squash(gradient);

            dCdY += dCdY_i * averaged;
            dCdB += dCdB_i * averaged;

            float N = (float)inputs;

            //https://towardsdatascience.com/implementing-batch-normalization-in-python-a044b0369567
            //perform chain rule back propagation through the batch normalisation's operations
            Matrix dXNorm = Matrix.PairwiseMultiplication(gradient, scale);
            Matrix dXCentered = Matrix.PairwiseDivision(dXNorm, _stdDev);
            Matrix dMean = Matrix.Squash(dXCentered) * -1.0f + Matrix.Squash(_normalised) * (2 / N);
            Matrix dStd = Matrix.Squash(Matrix.PairwiseMultiplication(Matrix.PairwiseMultiplication(dXNorm, _normalised), Matrix.PairwiseMultiplication(_stdDev, _stdDev) * -1.0f));
            Matrix dVar = Matrix.PairwiseDivision(dStd * 0.5f, _stdDev);

            Matrix dCdX = dXCentered + (Matrix.PairwiseMultiplication(Matrix.Expand2(dMean + dVar * 2.0f, batch), _normalised)) * (1 / N);

            return dCdX;
        }

        //optimisation parameters
        public Matrix dCdB;
        public Matrix dCdY;

        public void Zero()
        {
            dCdB.InitialiseZeroes();
            dCdY.InitialiseZeroes();
        }

        public void ClipNorm(float threshold)
        {
            float dCdYsum = dCdY.AbsSum() + EPSILON;

            if (dCdYsum > threshold)
            {
                dCdY = dCdY * (1 / dCdYsum) * threshold;
            }

            float dCdBsum = dCdB.AbsSum() + EPSILON;

            if (dCdBsum > threshold)
            {
                dCdB = dCdB * (1 / dCdBsum) * threshold;
            }
        }

        public void Vanilla(float learningRate)
        {
            shift = shift - Matrix.Expand2(dCdB, batch) * learningRate;
            scale = scale - Matrix.Expand2(dCdY, batch) * learningRate;
        }

        public Matrix vb;
        public Matrix vy;
        
        public void RMSProp(float learningRate, float b1 = 0.9f, float epsilon = 1e-9f)
        {
            //moving average
            vb = vb * b1 + Matrix.PairwiseMultiplication(dCdB, dCdB) * (1 - b1);

            Matrix vbe = Matrix.PairwiseDivision(dCdB, Matrix.SquareRoot(vb + epsilon));
            vbe = Matrix.Expand2(vbe, batch);
            shift = shift - vbe * learningRate;

            //same operations for scale
            vy = vy * b1 + Matrix.PairwiseMultiplication(dCdY, dCdY) * (1 - b1);

            Matrix vye = Matrix.PairwiseDivision(dCdY, Matrix.SquareRoot(vy + epsilon));
            vye = Matrix.Expand2(vye, batch);
            scale = scale - vye * learningRate;
        }
        
        public Matrix mb;
        public Matrix my;
        public int i = 0;
        
        public void Adam(float learningRate, float b1 = 0.9f, float b2 = 0.999f, float epsilon = 1e-7f)
        {
            i++;
        
            mb = mb * b1 + dCdB * (1 - b1);
            vb = vb * b2 + Matrix.PairwiseMultiplication(dCdB, dCdB) * (1 - b2);
        
            //bias corrected terms
            Matrix mbh = mb * (1 / (1 - MathF.Pow(b1, i)));
            Matrix vbh = vb * (1 / (1 - MathF.Pow(b2, i)));

            Matrix db = Matrix.PairwiseDivision(mbh, Matrix.SquareRoot(vbh + epsilon));
            db = Matrix.Expand2(db, batch);

            shift = shift - db * learningRate;
        
            //same operations for scale   
            my = my * b1 + dCdY * (1 - b1);
            vy = vy * b2 + Matrix.PairwiseMultiplication(dCdY, dCdY) * (1 - b2);
        
            Matrix myh = my * (1 / (1 - MathF.Pow(b1, i)));
            Matrix vyh = vy * (1 / (1 - MathF.Pow(b2, i)));
        
            Matrix dy = Matrix.PairwiseDivision(myh, Matrix.SquareRoot(vyh + epsilon));
            dy = Matrix.Expand2(dy, batch);
        
            scale = scale - dy * learningRate;
        }
    }
}
