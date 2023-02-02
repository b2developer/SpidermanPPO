using System;
using System.Collections.Generic;
using System.Text;

using NeuralNetwork.Layers;

namespace NeuralNetwork
{
    public class Network
    {
        public List<Layer> layers;
        public List<Dense> weights;
        public List<GPUDense> gpuWeights;
        public List<Dropout> dropouts; //lmao
        public List<BatchNormalisation> batchNormalisers;

        public int batch = 5;

        public Network(int _batch)
        {
            layers = new List<Layer>();
            weights = new List<Dense>();
            gpuWeights = new List<GPUDense>();
            dropouts = new List<Dropout>();
            batchNormalisers = new List<BatchNormalisation>();

            batch = _batch;
        }

        public void Dense(int inputs, int outputs)
        {
            Dense dense = new Dense(inputs, outputs, batch);
            layers.Add(dense);
            weights.Add(dense);
        }

        public void Dense(int inputs, int outputs, float min, float max)
        {
            Dense dense = new Dense(inputs, outputs, batch);
            dense.w.InitialiseRandom(min, max);
            layers.Add(dense);
            weights.Add(dense);
        }

        public void DenseHe(int inputs, int outputs)
        {
            Dense dense = new Dense(inputs, outputs, batch);

            float fanIn = (float)inputs;
            float std = MathF.Sqrt(2 / fanIn);

            NormalDistribution distribution = new NormalDistribution(0.0f, std);
            dense.w = distribution.Sample(dense.w.height, dense.w.width, dense.w.depth);

            layers.Add(dense);
            weights.Add(dense);
        }

        public void Activation(Activation.EActivator activator)
        {
            Activation activation = new Activation(activator);
            layers.Add(activation);
        }

        public void Dropout(int inputs, float chance)
        {
            Dropout dropout = new Dropout(inputs, batch, chance);
            layers.Add(dropout);
            dropouts.Add(dropout);
        }

        public void BatchNormalisation(int inputs)
        {
            BatchNormalisation batchNormalisation = new BatchNormalisation(inputs, batch);
            layers.Add(batchNormalisation);
            batchNormalisers.Add(batchNormalisation);
        }

        public void DenseGPU(int inputs, int outputs)
        {
            GPUDense gpuDense = new GPUDense(inputs, outputs, batch);
            layers.Add(gpuDense);
            gpuWeights.Add(gpuDense);
        }

        public void DenseGPU(int inputs, int outputs, float min, float max)
        {
            GPUDense denseGpu = new GPUDense(inputs, outputs, batch);
            denseGpu.w.InitialiseRandom(min, max);
            layers.Add(denseGpu);
            gpuWeights.Add(denseGpu);
        }

        public void DenseGPUHe(int inputs, int outputs)
        {
            GPUDense denseGpu = new GPUDense(inputs, outputs, batch);

            float fanIn = (float)inputs;
            float std = MathF.Sqrt(2 / fanIn);

            NormalDistribution distribution = new NormalDistribution(0.0f, std);
            denseGpu.w = distribution.Sample(denseGpu.w.height, denseGpu.w.width, denseGpu.w.depth);

            layers.Add(denseGpu);
            gpuWeights.Add(denseGpu);
        }

        public Matrix Forward(Matrix x)
        {
            int layerCount = layers.Count;

            for (int i = 0; i < layerCount; i++)
            {
                x = layers[i].Forward(x);
            }

            return x;
        }

        public class Cache
        {
            public List<Matrix> inputs; //inputs for each layer
            public Matrix output; //output from forward propagation

            public List<Matrix> masks; //probability masks from dropout layerss

            public List<Matrix> normalised;
            public List<Matrix> stdDev;
            public List<Matrix> xHat;

            public Cache()
            {
                inputs = new List<Matrix>();
                masks = new List<Matrix>();

                normalised = new List<Matrix>();
                stdDev = new List<Matrix>();
                xHat = new List<Matrix>();
            }
        }

        public Cache DetailedForward(Matrix x)
        {
            Cache prop = new Cache();

            int layerCount = layers.Count;

            for (int i = 0; i < layerCount; i++)
            {
                prop.inputs.Add(new Matrix(x.values));
                x = layers[i].Forward(x);
            }

            prop.output = new Matrix(x.values);

            int dropCount = dropouts.Count;

            for (int i = 0; i < dropCount; i++)
            {
                prop.masks.Add(new Matrix(dropouts[i].mask.values));
            }

            int batchNormCount = batchNormalisers.Count;

            for (int i = 0; i < batchNormCount; i++)
            {
                prop.normalised.Add(new Matrix(batchNormalisers[i]._normalised.values));
                prop.stdDev.Add(new Matrix(batchNormalisers[i]._stdDev.values));
                prop.xHat.Add(new Matrix(batchNormalisers[i]._xHat.values));
            }

            return prop;
        }

        public Matrix Back(Matrix g, Cache cache)
        {
            int dropCount = dropouts.Count;

            for (int i = 0; i < dropCount; i++)
            {
                dropouts[i].mask = cache.masks[i];
            }

            int batchNormCount = batchNormalisers.Count;

            for (int i = 0; i < batchNormCount; i++)
            {
                batchNormalisers[i]._normalised = cache.normalised[i];
                batchNormalisers[i]._stdDev = cache.stdDev[i];
                batchNormalisers[i]._xHat = cache.xHat[i];
            }

            int layerCount = layers.Count;

            for (int i = layerCount - 1; i >= 0; i--)
            {
                g = layers[i].Back(cache.inputs[i], g);
            }

            return g;
        }

        public void ZeroGradients()
        {
            int weightCount = weights.Count;

            for (int i = 0; i < weightCount; i++)
            {
                weights[i].Zero();
            }

            int batchNormCount = batchNormalisers.Count;

            for (int i = 0; i < batchNormCount; i++)
            {
                batchNormalisers[i].Zero();
            }

            int gpuWeightCount = gpuWeights.Count;

            for (int i = 0; i < gpuWeightCount; i++)
            {
                gpuWeights[i].Zero();
            }
        }

        public void ClipNorm(float threshold)
        {
            int weightCount = weights.Count;

            for (int i = 0; i < weightCount; i++)
            {
                weights[i].ClipNorm(threshold);
            }

            int batchNormCount = batchNormalisers.Count;

            for (int i = 0; i < batchNormCount; i++)
            {
                batchNormalisers[i].ClipNorm(threshold);
            }

            int gpuWeightCount = gpuWeights.Count;

            for (int i = 0; i < gpuWeightCount; i++)
            {
                gpuWeights[i].ClipNorm(threshold);
            }
        }

        public void GlobalClipNorm(float threshold)
        {
            float globalNorm = 0.0f;

            int weightCount = weights.Count;

            for (int i = 0; i < weightCount; i++)
            {
                globalNorm += weights[i].dCdW.SquaredSum();
                globalNorm += weights[i].dCdB.SquaredSum();
            }

            int batchNormCount = batchNormalisers.Count;

            for (int i = 0; i < batchNormCount; i++)
            {
                globalNorm += batchNormalisers[i].dCdB.SquaredSum();
                globalNorm += batchNormalisers[i].dCdY.SquaredSum();
            }

            int gpuWeightCount = gpuWeights.Count;

            for (int i = 0; i < gpuWeightCount; i++)
            {
                globalNorm += gpuWeights[i].dCdW.SquaredSum();
                globalNorm += gpuWeights[i].dCdB.SquaredSum();
            }

            globalNorm = MathF.Sqrt(globalNorm);

            float scalar = threshold / MathF.Max(threshold, globalNorm);

            for (int i = 0; i < weightCount; i++)
            {
                weights[i].dCdW = weights[i].dCdW * scalar;
                weights[i].dCdB = weights[i].dCdB * scalar;
            }

            for (int i = 0; i < batchNormCount; i++)
            {
                batchNormalisers[i].dCdB = batchNormalisers[i].dCdB * scalar;
                batchNormalisers[i].dCdY = batchNormalisers[i].dCdY * scalar;
            }

            for (int i = 0; i < gpuWeightCount; i++)
            {
                gpuWeights[i].dCdW = gpuWeights[i].dCdW * scalar;
                gpuWeights[i].dCdB = gpuWeights[i].dCdB * scalar;
            }
        }

        public void OptimiseWeights(float learningRate)
        {
            int weightCount = weights.Count;

            for (int i = 0; i < weightCount; i++)
            {
                weights[i].Adam(learningRate);
            }

            int batchNormCount = batchNormalisers.Count;

            for (int i = 0; i < batchNormCount; i++)
            {
                batchNormalisers[i].Adam(learningRate);
            }

            int gpuWeightCount = gpuWeights.Count;

            for (int i = 0; i < gpuWeightCount; i++)
            {
                gpuWeights[i].Adam(learningRate);
            }
        }
    }
}
