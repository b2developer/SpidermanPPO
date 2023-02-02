using System.Collections;
using System.Collections.Generic;

public class Schedule
{
    public int startingStep = 0;
    public int endingStep = 0;

    public float startingValue = 0.0f;
    public float endingValue = 0.0f;

    public Schedule(int _startingStep, int _endingStep, float _startingValue, float _endingValue)
    {
        startingStep = _startingStep;
        endingStep = _endingStep;
        startingValue = _startingValue;
        endingValue = _endingValue;
    }

    public float Evaluate(int steps)
    {
        float sf = (float)steps;

        sf -= (float)startingStep;
        float lerp = sf / (float)(endingStep - startingStep);
        
        if (lerp < 0.0f)
        {
            lerp = 0.0f;
        }
        else if (lerp > 1.0f)
        {
            lerp = 1.0f;
        }

        float value = startingValue + (endingValue - startingValue) * lerp;

        return value;
    }
}
