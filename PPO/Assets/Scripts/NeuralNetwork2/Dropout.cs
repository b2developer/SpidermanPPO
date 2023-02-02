using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.Layers
{
    public class Dropout : Layer
    {
        public Matrix mask;
        public float chance = 0.0f;

        public Dropout(int inputs, int batch, float _chance)
        {
            mask = new Matrix(inputs, batch, 1);
            chance = _chance;
        }

        public override Matrix Forward(Matrix x)
        {
            //don't apply dropout unless training
            if (x.width == 1)
            {
                return x;
            }

            //gemerate new mask
            mask.InitialiseRandom(0.0f, 1.0f);

            for (int i = 0; i < mask.height; i++)
            {
                for (int j = 0; j < mask.width; j++)
                {
                    if (mask.values[i, j, 0] > chance)
                    {
                        mask.values[i, j, 0] = 1.0f;
                    }
                    else
                    {
                        mask.values[i, j, 0] = 0.0f;
                    }
                }
            }

            return Matrix.PairwiseMultiplication(x, mask);
        }

        //a = input into layer, dCdA = gradient from next layer
        public override Matrix Back(Matrix a, Matrix gradient)
        {
            return Matrix.PairwiseMultiplication(gradient, mask);
        }
    }
}
