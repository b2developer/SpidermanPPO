using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeuralNetwork;
using NeuralNetwork.Layers;

public class PPOAgent
{
    public int STATE_SIZE = 20;
    public int ACTION_SIZE = 6;

    public float ACTION_MIN = -1.0f;
    public float ACTION_MAX = 1.0f;

    public static float GAMMA = 0.99f; //discount factor
    public static float GAE_LAMBDA = 0.95f; //smoothing parameter for general advantage estimates

    public static int EPOCHS = 8; //how many learning updates to do at a time
    public static float LEARNING_RATE_ACTOR = 0.0003f; //learning rate for actor network
    public static float LEARNING_RATE_CRITIC = 0.0003f; //learning rate for critic network
    public static float MAX_GRAD_NORM = 0.5f; //maximum value of gradients after GlobalClipNorm is applied

    public static int BATCH_SIZE = 512; //batch size for training
    public static int BUFFER_SIZE = 8192; //max buffer size before learning is performed
    public static int TIME_HORIZON = 64; //how many timesteps to look ahead when calculating discounted return before using value function again

    public static float VALUE_COEFFICIENT = 0.5f;
    public static float ENTROPY_COEFFICIENT = 0.005f;

    public static float CLIPPING_EPSILON = 0.2f; //used to clip gradients for the ppo loss function Lclip(theta)
    public static float TARGET_KL = 0.1f; //how far the policy is allowed to diverge before early stopping is applied
    public static float EPSILON = 1e-8f; //stop logarithms from being -infinity when they are 0

    public static float CLIPPING_MIN = -10.0f;
    public static float CLIPPING_MAX = 10.0f;

    public static float SIGMA_SCALING = -3.5f; //SIGMA_SCALING = -1.5f;
    public static float SIGMA_MIN = 0.01f;
    public static float SIGMA_MAX = 10.0f;

    public bool testing = true;

    //schedules
    public static int totalSteps = 0;

    public Schedule learningRateSchedule;
    public Schedule clippingSchedule;
    public Schedule betaSchedule;

    public List<Episode> episodes; //episodes completed by actors under the current policy
    public List<DetailedTimestep> timesteps; //advanced timestep data

    public NeuralNetwork.Network muHead; //actor network responsible for mean
    public NeuralNetwork.Network sigmaHead; //actor network responsible for standard deviation
    public NeuralNetwork.Network valueNetwork; //computes V(s) for advantage function estimates

    public NormalDistribution distribution;
    public RunningMeanStd stateNormaliser;

    public WelfordDataSet rewardsDataSet;

    public PPOAgent()
    {
        int maxSteps = 20000000;

        learningRateSchedule = new Schedule(0, maxSteps, 0.0003f, 1e-8f);
        clippingSchedule = new Schedule(0, maxSteps, CLIPPING_EPSILON, 0.01f);
        betaSchedule = new Schedule(0, maxSteps, ENTROPY_COEFFICIENT, 1e-6f);

        episodes = new List<Episode>();
        timesteps = new List<DetailedTimestep>();

        distribution = new NormalDistribution(0.0f, 1.0f);

        rewardsDataSet = new WelfordDataSet(1, 15);
    }

    public void GenerateNetworks()
    {
        int HIDDEN_SIZE = 128;

        float WEIGHT_MIN = -0.01f;
        float WEIGHT_MAX = 0.01f;

        muHead = new NeuralNetwork.Network(BATCH_SIZE);

        muHead.DenseGPU(STATE_SIZE, HIDDEN_SIZE, WEIGHT_MIN, WEIGHT_MAX);
        muHead.Activation(Activation.EActivator.LEAKY_RELU);
        muHead.DenseGPU(HIDDEN_SIZE, ACTION_SIZE, WEIGHT_MIN, WEIGHT_MAX);
        muHead.Activation(Activation.EActivator.TANH);

        sigmaHead = new NeuralNetwork.Network(BATCH_SIZE);

        sigmaHead.DenseGPU(STATE_SIZE, ACTION_SIZE, WEIGHT_MIN * 0.1f, WEIGHT_MAX * 0.1f);
        sigmaHead.Activation(Activation.EActivator.NONE);

        valueNetwork = new NeuralNetwork.Network(BATCH_SIZE);

        valueNetwork.DenseGPUHe(STATE_SIZE, HIDDEN_SIZE);
        valueNetwork.Activation(Activation.EActivator.RELU);

        valueNetwork.DenseGPUHe(HIDDEN_SIZE, 1);
        valueNetwork.Activation(Activation.EActivator.NONE);

        stateNormaliser = new RunningMeanStd(STATE_SIZE);
    }

    public class ActionTuple
    {
        public Matrix action;
        public Matrix e;
        public Matrix logProb;
    }

    //sample actions according to the normal distribution (mu, sigma) output from the actor
    public ActionTuple SelectAction(ref Matrix state)
    {
        Matrix stateNorm = stateNormaliser.Normalise(state);

        Matrix mu = muHead.Forward(stateNorm);
        Matrix logSigma = sigmaHead.Forward(stateNorm) + SIGMA_SCALING;

        //sigma is log(sigma), use e^sigma to get actual sigma
        Matrix sigma = Matrix.Exp(logSigma);
        sigma.Clip(SIGMA_MIN, SIGMA_MAX);

        Matrix gaussian = new Matrix(ACTION_SIZE, 1, 1);
        Matrix e = new Matrix(ACTION_SIZE, 1, 1); //baseline

        //generate base distribution values
        for (int i = 0; i < ACTION_SIZE; i++)
        {
            NormalDistribution.Tuple tuple = distribution.SampleWithBase();

            //location scale transformation
            gaussian.values[i, 0, 0] = tuple.x;
            e.values[i, 0, 0] = tuple.e;
        }

        gaussian = mu + Matrix.PairwiseMultiplication(gaussian, sigma);

        Matrix logProbability = NormalDistribution.LogDensity(gaussian, mu, sigma);
        Matrix probability = Matrix.Exp(logProbability);

        Matrix action = new Matrix(gaussian.values);

        ActionTuple actionTuple = new ActionTuple();

        actionTuple.action = action;
        actionTuple.e = e;
        actionTuple.logProb = logProbability;

        totalSteps++;

        return actionTuple;
    }

    //get the value estimate for this state
    public float Value(Matrix state)
    {
        Matrix stateNorm = stateNormaliser.Normalise(state);

        Matrix value = valueNetwork.Forward(stateNorm);
        return value.values[0, 0, 0];
    }

    public float ProcessReward(float reward)
    {
        return reward;
    }

    public void EpisodeCompleted(Episode episode)
    {
        if (testing)
        {
            return;
        }

        Matrix totalRewardMatrix = new Matrix(1, 1, 1);
        totalRewardMatrix.values[0, 0, 0] = episode.totalReward;

        rewardsDataSet.Append(totalRewardMatrix);

        List<DetailedTimestep> currentSteps = new List<DetailedTimestep>();

        //compute the returns and advantage estimate for each timestep in the episode
        for (int k = 0; k < episode.length; k++)
        {
            DetailedTimestep timestep = new DetailedTimestep();

            timestep.state = episode.samples[k].state;
            timestep.action = episode.samples[k].action;
            timestep.reward = episode.samples[k].reward;

            timestep.e = episode.samples[k].e;
            timestep.logProb = episode.samples[k].logProb;

            float discount = 1.0f;
            float discountedSum = 0.0f;

            int relativeTimestep = 0;

            //discounted sum of rewards
            for (int t = k; t < episode.length; t++)
            {
                discountedSum += episode.samples[t].reward * discount;
                discount *= GAMMA;

                relativeTimestep++;

                if (relativeTimestep >= TIME_HORIZON)
                {
                    //don't add the value if the state is terminal
                    if (t < episode.length - 1)
                    {
                        discountedSum += discount * Value(episode.samples[t + 1].state);
                    }

                    break;
                }
            }

            float value = episode.values[k];
            float advantage = discountedSum - value;

            episode.returns.Add(discountedSum); //Gt
            episode.advantages.Add(advantage);

            timestep.value = value;
            timestep.returns = discountedSum;
            timestep.advantage = advantage;

            timesteps.Add(timestep);
            currentSteps.Add(timestep);
        }

        //compute generalised advantage estimate
        for (int k = episode.length - 1; k >= 0; k--)
        {
            DetailedTimestep timestep = currentSteps[k];

            timestep.delta = timestep.reward - timestep.value;

            float nextGae = 0.0f;

            if (k < episode.length - 1)
            {
                timestep.delta += GAMMA * currentSteps[k + 1].value;
                nextGae = currentSteps[k + 1].gae;
            }

            float gae = timestep.delta + GAMMA * GAE_LAMBDA * nextGae;

            timestep.gae = gae;
            timestep.returns = gae + timestep.value;
        }

        //add fully processed episode to the batch
        episodes.Add(episode);

        //all episodes have finished
        if (episodes.Count >= PhysicsManager.ENVIRONMENTS)
        {
            episodes.Clear();
            
            //buffer is large enough for a training step
            if (timesteps.Count >= BUFFER_SIZE)
            {
                Bootstrap.instance.WriteToFile(rewardsDataSet.mean.values[0, 0, 0]);
                TrainingStep();
            }
        }
    }

    public void TrainingStep()
    {
        NeuralNetwork.Network oldMuHead = CloneNetwork(muHead);
        NeuralNetwork.Network oldSigmaHead = CloneNetwork(sigmaHead);

        int timestepCount = timesteps.Count;

        float gaeMean = 0.0f;

        //calculate mean
        for (int i = 0; i < timestepCount; i++)
        {
            gaeMean += timesteps[i].gae;
        }

        gaeMean /= (float)timestepCount;

        float gaeVariance = 0.0f;

        //calculate variance
        for (int i = 0; i < timestepCount; i++)
        {
            float norm = timesteps[i].gae - gaeMean;
            float n2 = norm * norm;

            gaeVariance += n2;
        }

        gaeVariance /= (float)timestepCount;

        //variance = std ^ 2
        float gaeStd = System.MathF.Sqrt(gaeVariance);

        //normalise values
        for (int i = 0; i < timestepCount; i++)
        {
            timesteps[i].gae = (timesteps[i].gae - gaeMean) / (gaeVariance + EPSILON);
        }

        bool trainPolicy = true;

        for (int e = 0; e < EPOCHS; e++)
        {
            List<DetailedTimestep> pool = new List<DetailedTimestep>(timesteps);

            //M = N * T
            int batches = timestepCount / BATCH_SIZE;

            for (int b = 0; b < batches; b++)
            {
                //generate random batch
                List<DetailedTimestep> batch = new List<DetailedTimestep>();

                for (int j = 0; j < BATCH_SIZE; j++)
                {
                    //this guarentees that each entry is processed once, but in a random order
                    int index = Random.Range(0, pool.Count);

                    batch.Add(pool[index]);
                    pool.RemoveAt(index);
                }

                //on the first epoch, update the state normaliser
                if (e == 0)
                {
                    Matrix stateBatch = new Matrix(STATE_SIZE, BATCH_SIZE, 1);

                    for (int i = 0; i < STATE_SIZE; i++)
                    {
                        for (int j = 0; j < BATCH_SIZE; j++)
                        {
                            stateBatch.values[i, j, 0] = batch[j].state.values[i, 0, 0];
                        }
                    }

                    stateNormaliser.Update(stateBatch);
                }

                bool klDivergence = UpdateNetworks(batch, oldMuHead, oldSigmaHead, trainPolicy);

                //set the training flag once the epoch has been completed
                if (b == batches - 1 && trainPolicy)
                {
                    trainPolicy = klDivergence;
                }
            }
        }

        if (!trainPolicy)
        {
            Debug.Log("EARLY STOPPING TRIGGERED");
        }

        Debug.Log("TRAINING STEP COMPLETED");

        timesteps.Clear();

        Debug.Log("Mean: " + rewardsDataSet.mean.values[0, 0, 0].ToString() + ", Standard Dev: " + rewardsDataSet.stdDev.values[0, 0, 0].ToString());
    }

    public bool UpdateNetworks(List<DetailedTimestep> batch, NeuralNetwork.Network oldMuHead, NeuralNetwork.Network oldSigmaHead, bool trainPolicy = true)
    {
        valueNetwork.ZeroGradients();
        muHead.ZeroGradients();
        sigmaHead.ZeroGradients();

        float learningRate = learningRateSchedule.Evaluate(totalSteps);
        float clippingEpsilon = clippingSchedule.Evaluate(totalSteps);
        float beta = betaSchedule.Evaluate(totalSteps);

        Matrix stateBatch = new Matrix(STATE_SIZE, BATCH_SIZE, 1);

        for (int i = 0; i < STATE_SIZE; i++)
        {
            for (int j = 0; j < BATCH_SIZE; j++)
            {
                stateBatch.values[i, j, 0] = batch[j].state.values[i,0,0];
            }
        }

        Matrix stateNormBatch = stateNormaliser.Normalise(stateBatch);

        Matrix actionBatch = new Matrix(ACTION_SIZE, BATCH_SIZE, 1);
        Matrix eBatch = new Matrix(ACTION_SIZE, BATCH_SIZE, 1);
        Matrix logProbOldBatch = new Matrix(ACTION_SIZE, BATCH_SIZE, 1);

        for (int i = 0; i < ACTION_SIZE; i++)
        {
            for (int j = 0; j < BATCH_SIZE; j++)
            {
                actionBatch.values[i, j, 0] = batch[j].action.values[i, 0, 0];     
                eBatch.values[i, j, 0] = batch[j].e.values[i, 0, 0];
                logProbOldBatch.values[i, j, 0] = batch[j].logProb.values[i, 0, 0];
            }
        }

        Matrix returnsBatch = new Matrix(1, BATCH_SIZE, 1);
        Matrix advantageBatch = new Matrix(1, BATCH_SIZE, 1);
        
        for (int i = 0; i < BATCH_SIZE; i++)
        {
            returnsBatch.values[0, i, 0] = batch[i].returns;
            advantageBatch.values[0, i, 0] = batch[i].gae;
        }

        //update the critic by minimising V(st) - Gt
        //------------------------------------------------
        NeuralNetwork.Network.Cache valueCache = valueNetwork.DetailedForward(stateNormBatch);
        Matrix valueBatch = valueCache.output;

        //2 * V(St) - Vtarget
        Matrix criticLoss = (valueBatch - returnsBatch) * 2.0f * VALUE_COEFFICIENT;

        valueNetwork.Back(criticLoss, valueCache);
        valueNetwork.GlobalClipNorm(MAX_GRAD_NORM);
        valueNetwork.OptimiseWeights(learningRate);
        //------------------------------------------------

        //kl divergence was reached in a prior training epoch / batch
        if (!trainPolicy)
        {
            return false;
        }    

        //update the actor by maximise -Lclip(theta) - s2 * entropy
        //------------------------------------------------
        NeuralNetwork.Network.Cache muCache = muHead.DetailedForward(stateNormBatch);
        NeuralNetwork.Network.Cache sigmaCache = sigmaHead.DetailedForward(stateNormBatch);

        Matrix muBatch = muCache.output;
        Matrix logSigmaBatch = sigmaCache.output + SIGMA_SCALING;

        //sigma is log(sigma), use e^sigma to get actual sigma
        Matrix sigmaBatch = Matrix.Exp(logSigmaBatch);
        sigmaBatch.Clip(SIGMA_MIN, SIGMA_MAX);

        Matrix logProbBatch = NormalDistribution.LogDensity(actionBatch, muBatch, sigmaBatch);
        Matrix probBatch = Matrix.Exp(logProbBatch);

        //same as dividing the regular values
        Matrix r = Matrix.Exp(logProbBatch - logProbOldBatch); //ppo ratio
        
        Matrix rclipa = new Matrix(r.values);
        rclipa.Clip(1 - clippingEpsilon, 1 + clippingEpsilon);

        Matrix ra = new Matrix(r.values);

        //multiply by the advantage function
        for (int i = 0; i < ACTION_SIZE; i++)
        {
            for (int j = 0; j < BATCH_SIZE; j++)
            {
                float a = advantageBatch.values[0, j, 0];

                ra.values[i, j, 0] = r.values[i, j, 0] * a;
                rclipa.values[i, j, 0] *= a;
            }
        }

        //derivatives of min and clip functions, keeps new policy from diverging too far from old policy
        //-----------------------
        Matrix g1 = new Matrix(ACTION_SIZE, BATCH_SIZE, 1);

        for (int i = 0; i < ACTION_SIZE; i++)
        {
            for (int j = 0; j < BATCH_SIZE; j++)
            {
                g1.values[i, j, 0] = (ra.values[i, j, 0] <= rclipa.values[i, j, 0] ? 1.0f : 0.0f) * advantageBatch.values[0, j, 0];
            }
        }

        Matrix g2 = new Matrix(ACTION_SIZE, BATCH_SIZE, 1);

        for (int i = 0; i < ACTION_SIZE; i++)
        {
            for (int j = 0; j < BATCH_SIZE; j++)
            {
                g2.values[i, j, 0] = (rclipa.values[i, j, 0] < ra.values[i, j, 0] ? 1.0f : 0.0f) * advantageBatch.values[0, j, 0];
            }
        }

        Matrix g3 = new Matrix(ACTION_SIZE, BATCH_SIZE, 1);
        Matrix probabilityOldBatchReciprocal = Matrix.Reciprocal(Matrix.Exp(logProbOldBatch));

        for (int i = 0; i < ACTION_SIZE; i++)
        {
            for (int j = 0; j < BATCH_SIZE; j++)
            {
                g3.values[i, j, 0] = (r.values[i, j, 0] >= 1 - clippingEpsilon && r.values[i, j, 0] <= 1 + clippingEpsilon ? 1.0f : 0.0f);
            }
        }
        //-----------------------

        //derivative of Lclip
        Matrix dLdP = Matrix.PairwiseMultiplication((g1 + Matrix.PairwiseMultiplication(g2, g3)) * -1.0f, probabilityOldBatchReciprocal);

        //entropy bonus
        //-----------------------
        Matrix entropy = NormalDistribution.Entropy(muBatch, sigmaBatch) * beta;
        dLdP = dLdP - entropy;
        //-----------------------

        //derivative of mu
        //-----------------------
        Matrix normalisedBatch = actionBatch - muBatch;

        Matrix varianceBatch = Matrix.PairwiseMultiplication(sigmaBatch, sigmaBatch);

        Matrix dlogPdMu = Matrix.PairwiseDivision(normalisedBatch, varianceBatch);
        Matrix dPdMu = Matrix.PairwiseMultiplication(dlogPdMu, probBatch);

        //appply chain rule to get derivative of L with respect to mu
        Matrix dLdMu = Matrix.PairwiseMultiplication(dLdP, dPdMu);
        //-----------------------

        //derivative of sigma
        //-----------------------
        Matrix std3Batch = Matrix.PairwiseMultiplication(varianceBatch, sigmaBatch);
        Matrix dlogPdSigma = Matrix.PairwiseDivision(Matrix.PairwiseMultiplication(normalisedBatch, normalisedBatch) - varianceBatch, std3Batch);
        Matrix dPdSigma = Matrix.PairwiseMultiplication(dlogPdSigma, probBatch);

        //appply chain rule to get derivative of L with respect to sigma
        Matrix dLdSigma = Matrix.PairwiseMultiplication(dLdP, dPdSigma);
        //-----------------------

        //sigma = e ^ log(sigma), derivative wrt. sigma = e ^ log(sigma)
        Matrix dLdlogSigma = Matrix.PairwiseMultiplication(dLdSigma, sigmaBatch); 

        muHead.Back(dLdMu, muCache);
        muHead.GlobalClipNorm(MAX_GRAD_NORM);
        muHead.OptimiseWeights(learningRate);

        sigmaHead.Back(dLdlogSigma, sigmaCache);
        sigmaHead.GlobalClipNorm(MAX_GRAD_NORM);
        sigmaHead.OptimiseWeights(learningRate);

        //KL-divergence test, early stopping is done if the KL-divergence between the old and new policy is too high
        //-----------------------
        Matrix oldMuBatch = oldMuHead.Forward(stateBatch);
        Matrix oldLogSigmaBatch = oldSigmaHead.Forward(stateBatch) + SIGMA_SCALING;

        //sigma is log(sigma), use e^sigma to get actual sigma
        Matrix oldSigmaBatch = Matrix.Exp(oldLogSigmaBatch);
        oldSigmaBatch.Clip(SIGMA_MIN, SIGMA_MAX);

        //check kl divergence to activate early stopping of policy training
        Matrix klMatrix = NormalDistribution.KLDivergence(oldMuBatch, muBatch, oldSigmaBatch, sigmaBatch);

        klMatrix = Matrix.Squash(klMatrix);
        klMatrix = klMatrix * (1.0f / (float)BATCH_SIZE);

        for (int i = 0; i < ACTION_SIZE; i++)
        {
            float kl = klMatrix.values[i, 0, 0];

            if (kl > TARGET_KL * 1.5f)
            {
                //return false;
            }
        }

        return true;
    }

    public NeuralNetwork.Network CloneNetwork(NeuralNetwork.Network network)
    {
        NeuralNetwork.Network networkClone = new NeuralNetwork.Network(network.batch);

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
                networkClone.Dense(dense.w.width, dense.w.height);

                Dense denseClone = networkClone.layers[i] as Dense;
                denseClone.w = new Matrix(dense.w.values);
                denseClone.b = new Matrix(dense.b.values);

                denseClone.dCdW = new Matrix(dense.dCdW.values);
                denseClone.dCdB = new Matrix(dense.dCdB.values);

                denseClone.vw = new Matrix(dense.vw.values);
                denseClone.vb = new Matrix(dense.vb.values);
                denseClone.mw = new Matrix(dense.mw.values);
                denseClone.mb = new Matrix(dense.mb.values);

                denseClone.i = dense.i;
            }             
            else if (activation != null)
            {
                networkClone.Activation(activation.activator);
            }
            else if (dropout != null)
            {
                networkClone.Dropout(dropout.mask.height, dropout.chance);
            }
            else if (batchNorm != null)
            {
                networkClone.BatchNormalisation(batchNorm.inputs);

                BatchNormalisation batchNormClone = networkClone.layers[i] as BatchNormalisation;

                batchNormClone.scale = new Matrix(batchNorm.scale.values);
                batchNormClone.shift = new Matrix(batchNorm.shift.values);

                batchNormClone.runningMean = new Matrix(batchNorm.runningMean.values);
                batchNormClone.runningVariance = new Matrix(batchNorm.runningVariance.values);

                batchNormClone.dCdB = new Matrix(batchNorm.dCdB.values);
                batchNormClone.dCdY = new Matrix(batchNorm.dCdY.values);

                batchNormClone.vb = new Matrix(batchNorm.vb.values);
                batchNormClone.vy = new Matrix(batchNorm.vy.values);
                batchNormClone.mb = new Matrix(batchNorm.mb.values);
                batchNormClone.my = new Matrix(batchNorm.my.values);
            }
            else if(gpuDense != null)
            {
                networkClone.DenseGPU(gpuDense.w.width, gpuDense.w.height);

                GPUDense gpuDenseClone = networkClone.layers[i] as GPUDense;
                gpuDenseClone.w = new Matrix(gpuDense.w.values);
                gpuDenseClone.b = new Matrix(gpuDense.b.values);

                gpuDenseClone.dCdW = new Matrix(gpuDense.dCdW.values);
                gpuDenseClone.dCdB = new Matrix(gpuDense.dCdB.values);

                gpuDenseClone.vw = new Matrix(gpuDense.vw.values);
                gpuDenseClone.vb = new Matrix(gpuDense.vb.values);
                gpuDenseClone.mw = new Matrix(gpuDense.mw.values);
                gpuDenseClone.mb = new Matrix(gpuDense.mb.values);

                gpuDenseClone.i = gpuDense.i;
            }
        }

        return networkClone;
    }
}
