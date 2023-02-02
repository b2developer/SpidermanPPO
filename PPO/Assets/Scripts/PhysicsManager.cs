using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork.Testing;

public class PhysicsManager : MonoBehaviour
{
    public enum EStepMode
    {
        REALTIME,
        FAST,
    }

    public bool startFlag = false;

    public EStepMode mode = EStepMode.REALTIME;

    public float timer = 0.0f;
    public float maxDeltatime = 0.5f;

    public Bootstrap bootstrap;
    public Environment[] environments;

    public int STEPS = 0;
    public static int ENVIRONMENTS = 0;

    public static ComputeShader matrixMultiplicationShader;
    public ComputeShader shader;

    void Start()
    {
        Application.targetFrameRate = 60;

        matrixMultiplicationShader = shader;

        UnitTester unitTester = new UnitTester();
        bool success = unitTester.AssertAll();

        Physics.autoSimulation = false;

        PPOAgent agent = new PPOAgent();

        //link references to agent
        bootstrap.agent = agent;

        ENVIRONMENTS = environments.GetLength(0);

        for (int env = 0; env < ENVIRONMENTS; env++)
        {
            environments[env].agent = agent;
        }

        //initialise agent's networks, either fresh or from existing data
        if (bootstrap.loadId == "" || !bootstrap.Exists(bootstrap.loadId))
        {
            agent.GenerateNetworks();
        }
        else
        {
            bootstrap.LoadFromFile(bootstrap.loadId);
        }
        
    }

    void Update()
    {
        if (Physics.autoSimulation)
        {
            return;
        }

        if (!startFlag)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                startFlag = true;
            }

            return;
        }

        bool isRunning = false;

        if (mode == EStepMode.REALTIME)
        {
            timer += Mathf.Min(Time.deltaTime, maxDeltatime);

            //run physics updates until deltaTime is depleted
            while (timer >= Time.fixedDeltaTime * Time.timeScale)
            {
                timer -= Time.fixedDeltaTime * Time.timeScale;

                Physics.Simulate(Time.fixedDeltaTime * Time.timeScale);
                Physics.autoSyncTransforms = true;

                for (int env = 0; env < ENVIRONMENTS; env++)
                {
                    environments[env].Step(Time.fixedDeltaTime * Time.timeScale);
                }
            }
        }
        else if (mode == EStepMode.FAST)
        {
            for (int step = 0; step < STEPS; step++)
            {
                Physics.Simulate(Time.fixedDeltaTime * Time.timeScale);
                Physics.autoSyncTransforms = true;

                for (int env = 0; env < ENVIRONMENTS; env++)
                {
                    environments[env].Step(Time.fixedDeltaTime * Time.timeScale);
                }
            }
        }

        for (int env = 0; env < ENVIRONMENTS; env++)
        {
            if (environments[env].isRunning)
            {
                isRunning = true;
            }
        }

        //all environments have completed, reset then and start again
        if (!isRunning)
        {
            for (int env = 0; env < ENVIRONMENTS; env++)
            {
                environments[env].ResetEnvironment();
                Physics.Simulate(Time.fixedDeltaTime * Time.timeScale);
            }
        }
    }
}
