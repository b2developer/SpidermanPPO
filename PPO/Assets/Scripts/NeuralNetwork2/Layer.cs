using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.Layers
{
    public class Layer
    {

        public static float EPSILON = 1e-8f;

        public virtual Matrix Forward(Matrix x)
        {
            return null;
        }

        public virtual Matrix Back(Matrix x, Matrix g)
        {
            return null;
        }
    }
}
