using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeuralNetwork.Layers;

namespace NeuralNetwork.Testing
{
    public class UnitTester
    {
        public delegate bool Assertion();

        public List<Assertion> tests;

        public UnitTester()
        {
            tests = new List<Assertion>();
        }

        public bool AssertAll()
        {
            //Console.WriteLine("Starting Unit Tests...");

            //insert tests here
            //----------------------------
            ASSERT(SigmoidTest);
            ASSERT(SigmoidTest2);
            ASSERT(SigmoidTest3);
            ASSERT(SigmoidDerTest);
            ASSERT(SigmoidDerTest2);
            ASSERT(TanhTest);
            ASSERT(TanhTest2);
            ASSERT(TanhTest3);
            ASSERT(TanhDerTest);
            ASSERT(TanhDerTest2);
            ASSERT(TanhDerTest3);
            ASSERT(WeightTest1);
            ASSERT(WeightTest2);
            ASSERT(BatchNormTest1);
            ASSERT(GPUWeightTest1);
            ASSERT(SerialiseTest);
            ASSERT(SerialiseNeuralNetworkTest);
            ASSERT(SerialiseNeuralNetworkTest2);
            //ASSERT(BigGPUWeightTest); //extremely slow
            //----------------------------

            bool failure = false;

            int testCount = tests.Count;

            for (int i =0; i < testCount; i++)
            {
                bool result = tests[i]();

                if (!result)
                {
                    Debug.Log("[x] " + tests[i].Method.Name);
                    failure = true;
                }
            }

            if (failure)
            {
                Debug.Log("Unit Tests Were Failed!");
            }
            else
            {
                Debug.Log("Unit Testing Successful!");
            }

            tests.Clear();
            return !failure;
        }

        public void ASSERT(Assertion testFunction)
        {
            tests.Add(testFunction);
        }

        public bool SigmoidTest()
        {
            Activation act = new Activation(Activation.EActivator.SIGMOID);

            Matrix x = new Matrix(new float[1, 1, 1] { { { 0.0f } } });
            Matrix y = act.Forward(x);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.5f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool SigmoidTest2()
        {
            Activation act = new Activation(Activation.EActivator.SIGMOID);

            Matrix x = new Matrix(new float[1, 1, 1] { { { 1.0f } } });
            Matrix y = act.Forward(x);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.7310585786f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool SigmoidTest3()
        {
            Activation act = new Activation(Activation.EActivator.SIGMOID);

            Matrix x = new Matrix(new float[1, 1, 1] { { { -1.0f } } });
            Matrix y = act.Forward(x);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.268941421369f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool SigmoidDerTest()
        {
            Activation act = new Activation(Activation.EActivator.SIGMOID);

            Matrix one = new Matrix(new float[1, 1, 1] { { { 1.0f } } });

            Matrix x = new Matrix(new float[1, 1, 1] { { { 0.0f } } });
            Matrix y = act.Back(x, one);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.25f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool SigmoidDerTest2()
        {
            Activation act = new Activation(Activation.EActivator.SIGMOID);

            Matrix one = new Matrix(new float[1, 1, 1] { { { 1.0f } } });

            Matrix x = new Matrix(new float[1, 1, 1] { { { 5.0f } } });
            Matrix y = act.Back(x, one);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.0066480566707f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool TanhTest()
        {
            Activation act = new Activation(Activation.EActivator.TANH);

            Matrix x = new Matrix(new float[1, 1, 1] { { { 0.5f } } });
            Matrix y = act.Forward(x);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.4621171572600f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool TanhTest2()
        {
            Activation act = new Activation(Activation.EActivator.TANH);

            Matrix x = new Matrix(new float[1, 1, 1] { { { -2.0f } } });
            Matrix y = act.Forward(x);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { -0.9640275800758f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool TanhTest3()
        {
            Activation act = new Activation(Activation.EActivator.TANH);

            Matrix x = new Matrix(new float[1, 1, 1] { { { 3.0f } } });
            Matrix y = act.Forward(x);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.99505475368673f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool TanhDerTest()
        {
            Activation act = new Activation(Activation.EActivator.TANH);

            Matrix one = new Matrix(new float[1, 1, 1] { { { 1.0f } } });

            Matrix x = new Matrix(new float[1, 1, 1] { { { 0.0f } } });
            Matrix y = act.Back(x, one);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 1.0f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool TanhDerTest2()
        {
            Activation act = new Activation(Activation.EActivator.TANH);

            Matrix one = new Matrix(new float[1, 1, 1] { { { 1.0f } } });

            Matrix x = new Matrix(new float[1, 1, 1] { { { 2.0f } } });
            Matrix y = act.Back(x, one);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.07065082485f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool TanhDerTest3()
        {
            Activation act = new Activation(Activation.EActivator.TANH);

            Matrix one = new Matrix(new float[1, 1, 1] { { { 1.0f } } });

            Matrix x = new Matrix(new float[1, 1, 1] { { { -5.0f } } });
            Matrix y = act.Back(x, one);

            Matrix yhat = new Matrix(new float[1, 1, 1] { { { 0.00018158323f } } });

            return Matrix.IsEqual(y, yhat);
        }

        public bool WeightTest1()
        {
            Dense dense = new Dense(3, 4, 1);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    dense.w.values[j, i, 0] = 0.5f;
                }
            }

            Matrix x = new Matrix(3, 1, 1);
            x.values = new float[3,1,1] { { { 1.0f } },  { { 1.0f } } , { { 1.0f } } };

            Matrix y = dense.Forward(x);

            Matrix yhat = new Matrix(4, 1, 1);
            yhat.values = new float[4, 1, 1] { { { 1.5f } }, { { 1.5f } }, { { 1.5f } }, { { 1.5f } } };

            return Matrix.IsEqual(y, yhat);
        }

        public bool WeightTest2()
        {
            Dense dense = new Dense(3, 4, 1);

            dense.w.values[0, 0, 0] = 0.5f;
            dense.w.values[0, 1, 0] = 0.25f;
            dense.w.values[0, 2, 0] = 0.5f;

            dense.w.values[1, 0, 0] = 0.5f;
            dense.w.values[1, 1, 0] = 0.25f;
            dense.w.values[1, 2, 0] = 0.5f;

            dense.w.values[2, 0, 0] = 0.25f;
            dense.w.values[2, 1, 0] = 0.1f;
            dense.w.values[2, 2, 0] = 0.3f;

            dense.w.values[3, 0, 0] = 0.4f;
            dense.w.values[3, 1, 0] = 0.5f;
            dense.w.values[3, 2, 0] = 0.3f;

            Matrix x = new Matrix(3, 1, 1);
            x.values = new float[3, 1, 1] { { { 1.0f } }, { { 2.0f } }, { { 3.0f } } };

            Matrix y = dense.Forward(x);

            Matrix yhat = new Matrix(4, 1, 1);
            yhat.values = new float[4, 1, 1] { { { 2.5f } }, { { 2.5f } }, { { 1.35f } }, { { 2.3f } } };

            return Matrix.IsEqual(y, yhat);
        }

        public bool BatchNormTest1()
        {
            BatchNormalisation batchNorm = new BatchNormalisation(3, 3);

            Matrix x = new Matrix(3, 3, 1);

            x.values[0, 0, 0] = 1.0f;
            x.values[1, 0, 0] = 0.0f;
            x.values[2, 0, 0] = 0.0f;

            x.values[0, 1, 0] = 2.0f;
            x.values[1, 1, 0] = 5.0f;
            x.values[2, 1, 0] = -3.0f;

            x.values[0, 2, 0] = 0.2f;
            x.values[1, 2, 0] = -10.0f;
            x.values[2, 2, 0] = 0.5f;

            Matrix y = batchNorm.Forward(x);

            return true;
        }

        public bool GPUWeightTest1()
        {
            int h = 15;
            int w = 2;
            int depth = 5;

            MultiplicationGPU mg = new MultiplicationGPU(depth, h, w);

            Matrix a = new Matrix(h, depth, 1);
            a.InitialiseRandom(-5.0f, 5.0f);

            Matrix b = new Matrix(depth, w, 1);
            b.InitialiseRandom(-5.0f, 5.0f);

            Matrix c = a * b;

            mg.LoadMatrices(a, b);
            float[] array = mg.Compute();

            Matrix c_gpu = new Matrix(h, w, 1);

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    c_gpu.values[i, j, 0] = array[i * w + j];
                }
            }

            //use bigger epsilon since GPU calculates matrix multiplication with different architecture??
            return Matrix.IsEqual(c, c_gpu, 1e-4f);
        }

        public bool BigGPUWeightTest()
        {
            int h = 512;
            int w = 512;
            int depth = 512;

            MultiplicationGPU mg = new MultiplicationGPU(depth, h, w);

            Matrix a = new Matrix(h, depth, 1);
            a.InitialiseRandom(-5.0f, 5.0f);

            Matrix b = new Matrix(depth, w, 1);
            b.InitialiseRandom(-5.0f, 5.0f);

            Matrix c = a * b;

            mg.LoadMatrices(a, b);
            float[] array = mg.Compute();

            Matrix c_gpu = new Matrix(h, w, 1);

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    c_gpu.values[i, j, 0] = array[i * w + j];
                }
            }

            //use bigger epsilon since GPU calculates matrix multiplication with different architecture??
            return Matrix.IsEqual(c, c_gpu, 1e-2f);
        }

        public bool SerialiseTest()
        {
            Matrix random = new Matrix(10, 15, 5);
            random.InitialiseRandom(-10.0f, 10.0f);

            string data = random.Serialise();

            Matrix deserialMatrix = Matrix.Deserialise(data);

            return Matrix.IsEqual(random, deserialMatrix, 1e-5f);
        }

        public bool SerialiseNeuralNetworkTest()
        {
            NeuralNetwork.Network network = new Network(5);

            network.Dense(10, 15, -0.1f, 10.0f);
            network.BatchNormalisation(15);
            network.Activation(Activation.EActivator.RELU);
            network.Dropout(15, 0.1123f);
            network.DenseGPU(15, 2, -2.0f, -1.5f);
            network.Activation(Activation.EActivator.SIGMOID);
            network.Dense(2, 103);
            network.BatchNormalisation(15);
            network.Activation(Activation.EActivator.TANH);

            string data = Bootstrap.SerialiseNetwork(network);

            NeuralNetwork.Network copy = Bootstrap.DeserialiseNetwork(data);

            if (network.batch != copy.batch)
            {
                return false;
            }

            float epsilon = 1e-5f;

            int layers = network.layers.Count;

            for (int i = 0; i < layers; i++)
            {
                Dense dense = network.layers[i] as Dense;
                Activation activation = network.layers[i] as Activation;
                Dropout dropout = network.layers[i] as Dropout;
                BatchNormalisation batchNorm = network.layers[i] as BatchNormalisation;
                GPUDense gpuDense = network.layers[i] as GPUDense;

                if (dense != null)
                {
                    Dense denseCopy = copy.layers[i] as Dense;
                    
                    if (!Matrix.IsEqual(dense.w, denseCopy.w, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(dense.b, denseCopy.b, epsilon))
                    {
                        return false;
                    }
                }
                else if (activation != null)
                {
                    Activation activationCopy = copy.layers[i] as Activation;

                    if (activation.activator != activationCopy.activator)
                    {
                        return false;
                    }
                }
                else if (dropout != null)
                {
                    Dropout dropoutCopy = copy.layers[i] as Dropout;

                    if (!Matrix.IsEqual(dropout.mask, dropoutCopy.mask, epsilon))
                    {
                        return false;
                    }

                    if (Mathf.Abs(dropout.chance - dropoutCopy.chance) > epsilon)
                    {
                        return false;
                    }
                }
                else if (batchNorm != null)
                {
                    BatchNormalisation batchNormCopy = network.layers[i] as BatchNormalisation;

                    if (!Matrix.IsEqual(batchNorm.scale, batchNormCopy.scale, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(batchNorm.shift, batchNormCopy.shift, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(batchNorm.runningMean, batchNormCopy.runningMean, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(batchNorm.runningVariance, batchNormCopy.runningVariance, epsilon))
                    {
                        return false;
                    }
                }
                else if (gpuDense != null)
                {
                    GPUDense gpuDenseCopy = copy.layers[i] as GPUDense;

                    if (!Matrix.IsEqual(gpuDense.w, gpuDenseCopy.w, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(gpuDense.b, gpuDenseCopy.b, epsilon))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool SerialiseNeuralNetworkTest2()
        {
            NeuralNetwork.Network network = new Network(1);

            network.Activation(Activation.EActivator.RELU);
            network.Dense(10, 15, -0.1f, 10.0f);
            network.BatchNormalisation(15);
            network.BatchNormalisation(15);
            network.Dropout(15, 0.1123f);
            network.DenseGPU(15, 2, -2.0f, -1.5f);
            network.Activation(Activation.EActivator.SIGMOID);
            network.Dense(2, 103);
            network.Activation(Activation.EActivator.TANH);

            string data = Bootstrap.SerialiseNetwork(network);

            NeuralNetwork.Network copy = Bootstrap.DeserialiseNetwork(data);

            if (network.batch != copy.batch)
            {
                return false;
            }

            float epsilon = 1e-5f;

            int layers = network.layers.Count;

            for (int i = 0; i < layers; i++)
            {
                Dense dense = network.layers[i] as Dense;
                Activation activation = network.layers[i] as Activation;
                Dropout dropout = network.layers[i] as Dropout;
                BatchNormalisation batchNorm = network.layers[i] as BatchNormalisation;
                GPUDense gpuDense = network.layers[i] as GPUDense;

                if (dense != null)
                {
                    Dense denseCopy = copy.layers[i] as Dense;

                    if (!Matrix.IsEqual(dense.w, denseCopy.w, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(dense.b, denseCopy.b, epsilon))
                    {
                        return false;
                    }
                }
                else if (activation != null)
                {
                    Activation activationCopy = copy.layers[i] as Activation;

                    if (activation.activator != activationCopy.activator)
                    {
                        return false;
                    }
                }
                else if (dropout != null)
                {
                    Dropout dropoutCopy = copy.layers[i] as Dropout;

                    if (!Matrix.IsEqual(dropout.mask, dropoutCopy.mask, epsilon))
                    {
                        return false;
                    }

                    if (Mathf.Abs(dropout.chance - dropoutCopy.chance) > epsilon)
                    {
                        return false;
                    }
                }
                else if (batchNorm != null)
                {
                    BatchNormalisation batchNormCopy = network.layers[i] as BatchNormalisation;

                    if (!Matrix.IsEqual(batchNorm.scale, batchNormCopy.scale, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(batchNorm.shift, batchNormCopy.shift, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(batchNorm.runningMean, batchNormCopy.runningMean, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(batchNorm.runningVariance, batchNormCopy.runningVariance, epsilon))
                    {
                        return false;
                    }
                }
                else if (gpuDense != null)
                {
                    GPUDense gpuDenseCopy = copy.layers[i] as GPUDense;

                    if (!Matrix.IsEqual(gpuDense.w, gpuDenseCopy.w, epsilon))
                    {
                        return false;
                    }

                    if (!Matrix.IsEqual(gpuDense.b, gpuDenseCopy.b, epsilon))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
