using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork.Layers;

public class Bootstrap : MonoBehaviour
{
    public static Bootstrap instance = null;

    public bool testing = true;

    public string loadId = "";

    public string folderPath = "";
    public string runId = "";

    public PPOAgent agent;

    public int counter = 0;
    public int nextCheckpoint = 0;
    public int checkpointInterval = 65536;

    void Start()
    {
        instance = this;
        nextCheckpoint += PPOAgent.totalSteps + checkpointInterval + 1;
    }

    void Update()
    {
        agent.testing = testing;

        if (agent.testing)
        {
            return;
        }

        if (!agent.testing && PPOAgent.totalSteps > nextCheckpoint)
        {
            WriteToFile(agent.rewardsDataSet.mean.values[0, 0, 0]);
            nextCheckpoint += checkpointInterval;
        }
    }

    public bool Exists(string existingId)
    {
        string path1 = folderPath + "\\" + existingId + "_value.txt";
        string path2 = folderPath + "\\" + existingId + "_mu.txt";
        string path3 = folderPath + "\\" + existingId + "_sigma.txt";
        string path4 = folderPath + "\\" + existingId + "_running.txt";

        return File.Exists(path1) && File.Exists(path2) && File.Exists(path3) && File.Exists(path4);
    }

    public void LoadFromFile(string existingId)
    {
        using (StreamReader sr = File.OpenText(folderPath + "\\" + existingId + "_value.txt"))
        {
            string data = sr.ReadToEnd();
            agent.valueNetwork = DeserialiseNetwork(data);
        }

        using (StreamReader sr = File.OpenText(folderPath + "\\" + existingId + "_mu.txt"))
        {
            string data = sr.ReadToEnd();
            agent.muHead = DeserialiseNetwork(data);
        }

        using (StreamReader sr = File.OpenText(folderPath + "\\" + existingId + "_sigma.txt"))
        {
            string data = sr.ReadToEnd();
            agent.sigmaHead = DeserialiseNetwork(data);
        }

        using (StreamReader sr = File.OpenText(folderPath + "\\" + existingId + "_running.txt"))
        {
            string data = sr.ReadToEnd();

            int pointer = 0;
            string[] parts = data.Split(",");

            int count = int.Parse(parts[pointer]);
            pointer++;

            Matrix mean = Matrix.Deserialise(parts, ref pointer);
            Matrix variance = Matrix.Deserialise(parts, ref pointer);
            Matrix stdDev = Matrix.Deserialise(parts, ref pointer);

            agent.stateNormaliser = new RunningMeanStd(mean.width);
            agent.stateNormaliser.mean = mean;
            agent.stateNormaliser.variance = variance;
            agent.stateNormaliser.stdDev = stdDev;
        }
    }

    public void WriteToFile(float rewardMean)
    {
        int rewardRound = (int)Mathf.Max(0.0f, rewardMean);

        using (StreamWriter sw = File.CreateText(folderPath + "\\" + runId + "_" + counter.ToString() + "_" + rewardRound.ToString() + "_value.txt"))
        {
            string valueString = SerialiseNetwork(agent.valueNetwork);
            sw.WriteLine(valueString);
        }

        using (StreamWriter sw = File.CreateText(folderPath + "\\" + runId + "_" + counter.ToString() + "_" + rewardRound.ToString() + "_mu.txt"))
        {
            string muString = SerialiseNetwork(agent.muHead);
            sw.WriteLine(muString);
        }

        using (StreamWriter sw = File.CreateText(folderPath + "\\" + runId + "_" + counter.ToString() + "_" + rewardRound.ToString() + "_sigma.txt"))
        {
            string sigmaString = SerialiseNetwork(agent.sigmaHead);
            sw.WriteLine(sigmaString);
        }

        using (StreamWriter sw = File.CreateText(folderPath + "\\" + runId + "_" + counter.ToString() + "_" + rewardRound.ToString() + "_running.txt"))
        {
            string runningString = "";

            runningString += agent.stateNormaliser.count.ToString() + ",";
            runningString += agent.stateNormaliser.mean.Serialise() + ",";
            runningString += agent.stateNormaliser.variance.Serialise() + ",";
            runningString += agent.stateNormaliser.stdDev.Serialise() + ",";

            sw.WriteLine(runningString);
        }

        counter++;
    }

    public static string SerialiseNetwork(NeuralNetwork.Network network)
    {
        string b = "";

        b += network.batch.ToString() + ",";

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
                b += "0,";
                b += dense.w.Serialise() + ",";

                dense.b.width = 1;
                b += dense.b.Serialise() + ",";
                dense.b.width = network.batch;
            }
            else if (activation != null)
            {
                b += "1,";
                b += ((int)activation.activator).ToString() + ",";
            }
            else if (dropout != null)
            {
                b += "2,";
                b += dropout.mask.height.ToString() + ",";
                b += dropout.chance.ToString() + ",";
            }
            else if (batchNorm != null)
            {
                b += "3,";
                b += batchNorm.inputs.ToString() + ",";

                batchNorm.scale.width = 1;
                b += batchNorm.scale.Serialise() + ",";
                batchNorm.scale.width = network.batch;

                batchNorm.shift.width = 1;
                b += batchNorm.shift.Serialise() + ",";
                batchNorm.shift.width = network.batch;

                b += batchNorm.runningMean.Serialise() + ",";
                b += batchNorm.runningVariance.Serialise() + ",";
            }
            else if (gpuDense != null)
            {
                b += "4,";
                b += gpuDense.w.Serialise() + ",";

                gpuDense.b.width = 1;
                b += gpuDense.b.Serialise() + ",";
                gpuDense.b.width = network.batch;
            }
        }

        return b;
    }

    public static NeuralNetwork.Network DeserialiseNetwork(string data)
    {
        string[] values = data.Split(",");

        int batch = int.Parse(values[0]);

        NeuralNetwork.Network network = new NeuralNetwork.Network(batch);

        int pointer = 1;
        int length = values.GetLength(0);
        int layer = -1;

        while (pointer < length)
        {
            string header = values[pointer];
            pointer++;

            if (header == "0")
            {
                //dense
                Matrix w = Matrix.Deserialise(values, ref pointer);
                Matrix b = Matrix.Deserialise(values, ref pointer);
                b = Matrix.Expand2(b, network.batch);

                network.Dense(w.width, w.height);
                layer++;

                Dense denseClone = network.layers[layer] as Dense;
                denseClone.w = new Matrix(w.values);
                denseClone.b = new Matrix(b.values);
            }
            else if (header == "1")
            {
                //activation
                int type = int.Parse(values[pointer]);
                pointer++;

                Activation.EActivator activator = (Activation.EActivator)type;
                
                network.Activation(activator);
                layer++;
            }
            else if (header == "2")
            {
                //dropout
                int inputs = int.Parse(values[pointer]);
                pointer++;

                float chance = float.Parse(values[pointer]);
                pointer++;

                network.Dropout(inputs, chance);
                layer++;
            }
            else if (header == "3")
            {
                //batch normalisation
                int inputs = int.Parse(values[pointer]);
                pointer++;

                Matrix scale = Matrix.Deserialise(values, ref pointer);
                scale = Matrix.Expand2(scale, network.batch);

                Matrix shift = Matrix.Deserialise(values, ref pointer);
                shift = Matrix.Expand2(shift, network.batch);

                Matrix runningMean = Matrix.Deserialise(values, ref pointer);
                Matrix runningVariance = Matrix.Deserialise(values, ref pointer);

                network.BatchNormalisation(inputs);
                layer++;

                BatchNormalisation batchNorm = network.layers[layer] as BatchNormalisation;

                batchNorm.scale = new Matrix(scale.values);
                batchNorm.shift = new Matrix(shift.values);

                batchNorm.runningMean = new Matrix(runningMean.values);
                batchNorm.runningVariance = new Matrix(runningVariance.values);
            }
            else if (header == "4")
            {
                //dense gpu
                Matrix w = Matrix.Deserialise(values, ref pointer);
                Matrix b = Matrix.Deserialise(values, ref pointer);
                b = Matrix.Expand2(b, network.batch);

                network.DenseGPU(w.width, w.height);
                layer++;

                GPUDense gpuDense = network.layers[layer] as GPUDense;
                gpuDense.w = new Matrix(w.values);
                gpuDense.b = new Matrix(b.values);
            }
        }
        
        return network;
    }
}
