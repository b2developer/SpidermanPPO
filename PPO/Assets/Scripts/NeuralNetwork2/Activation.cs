using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.Layers
{
    public class Activation : Layer
    {
        public enum EActivator
        {
            NONE = 0,
            RELU = 1,
            SIGMOID = 2,
            TANH = 3,
            LEAKY_RELU = 4,
        }

        public EActivator activator;

        public Activation(EActivator _activator)
        {
            activator = _activator;
        }

        public override Matrix Forward(Matrix x)
        {
            switch (activator)
            {
                case EActivator.NONE: return None(x);
                case EActivator.RELU: return Relu(x);
                case EActivator.SIGMOID: return Sigmoid(x);
                case EActivator.TANH: return Tanh(x);
                case EActivator.LEAKY_RELU: return LeakyRelu(x);
                default: return null;
            }
        }

        //z = input into layer, dCdA = gradient from next layer
        public override Matrix Back(Matrix z, Matrix dCdA)
        {
            Matrix dAdZ;

            switch (activator)
            {
                case EActivator.NONE: dAdZ = NoneDerivative(z); break;
                case EActivator.RELU: dAdZ = ReluDerivative(z); break;
                case EActivator.SIGMOID: dAdZ = SigmoidDerivative(z); break;
                case EActivator.TANH: dAdZ = TanhDerivative(z); break;
                case EActivator.LEAKY_RELU: dAdZ = LeakyReluDerivative(z); break;
                default: dAdZ = null; break;
            }

            //dCdZ = dCdA * dAdZ
            Matrix dCdZ = Matrix.PairwiseMultiplication(dCdA, dAdZ);
            return dCdZ;
        }

        public static Matrix None(Matrix x)
        {
            Matrix y = new Matrix(x.values);
            return y;
        }

        public static Matrix NoneDerivative(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        y.values[i, j, k] = 1.0f;
                    }
                }
            }

            return y;
        }

        public static Matrix Relu(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        y.values[i, j, k] = MathF.Max(x.values[i, j, k], 0.0f);
                    }
                }
            }

            return y;
        }

        public static Matrix ReluDerivative(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        float val = x.values[i, j, k];

                        if (val > 0.0f)
                        {
                            y.values[i, j, k] = 1.0f;
                        }
                        else
                        {
                            y.values[i, j, k] = 0.0f;
                        }
                    }
                }
            }

            return y;
        }

        public static Matrix Sigmoid(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        float val = x.values[i, j, k];
                        y.values[i, j, k] = 1.0f / (1.0f + MathF.Exp(-val));
                    }
                }
            }

            return y;
        }

        public static Matrix SigmoidDerivative(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        float val = x.values[i, j, k];
                        float sig = 1.0f / (1.0f + MathF.Exp(-val));
                        y.values[i, j, k] = sig * (1.0f - sig);
                    }
                }
            }

            return y;
        }

        public static Matrix Tanh(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        float val = x.values[i, j, k];
                        y.values[i, j, k] = MathF.Tanh(val);
                    }
                }
            }

            return y;
        }

        public static Matrix TanhDerivative(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        float val = x.values[i, j, k];
                        float tanh = MathF.Tanh(val);
                        y.values[i, j, k] = 1.0f - tanh * tanh;
                    }
                }
            }

            return y;
        }

        public static Matrix LeakyRelu(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        y.values[i, j, k] = MathF.Max(x.values[i, j, k], 0.01f * x.values[i, j, k]);
                    }
                }
            }

            return y;
        }

        public static Matrix LeakyReluDerivative(Matrix x)
        {
            Matrix y = new Matrix(x.height, x.width, x.depth);

            for (int i = 0; i < x.height; i++)
            {
                for (int j = 0; j < x.width; j++)
                {
                    for (int k = 0; k < x.depth; k++)
                    {
                        float val = x.values[i, j, k];

                        if (val > 0.0f)
                        {
                            y.values[i, j, k] = 1.0f;
                        }
                        else
                        {
                            y.values[i, j, k] = 0.01f;
                        }
                    }
                }
            }

            return y;
        }
    }
}
