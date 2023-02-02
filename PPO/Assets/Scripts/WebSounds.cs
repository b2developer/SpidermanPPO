using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSounds : MonoBehaviour
{
    public RagdollController controller;

    public AudioSource windSource;

    public float cutoffSpeed = 50.0f;
    public float minSpeed = 5.0f;
    public float maxSpeed = 15.0f;
    public float minVolume = 0.15f;
    public float maxVolume = 0.5f;
    public float minPitch = 0.75f;
    public float maxPitch = 1.25f;

    public AudioSource leftSource;
    public AudioSource rightSource;
    public AudioClip[] clips;

    public float pitchMin = 0.8f;
    public float pitchMax = 1.25f;

    public float rechargeTime = 0.2f;
    public float leftTimer = 0.0f;
    public float rightTimer = 0.0f;

    void Start()
    {
        controller.onWebShotLeft += OnWebShotLeft;
        controller.onWebShotRight += OnWebShotRight;
    }

    void Update()
    {
        if (leftTimer > 0.0f)
        {
            leftTimer -= Time.deltaTime;
        }

        if (rightTimer > 0.0f)
        {
            rightTimer -= Time.deltaTime;
        }

        Vector3 velocity = controller.pelvis.velocity;
        float speed = velocity.magnitude;
        
        if (speed > cutoffSpeed)
        {
            windSource.volume = 0.0f;
        }
        else
        {
            float lerp = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

            float volume = Mathf.Lerp(minVolume, maxVolume, lerp);
            float pitch = Mathf.Lerp(minPitch, maxPitch, lerp);

            windSource.volume = volume;
            windSource.pitch = pitch;
        }
    }

    public AudioClip SelectRandomClip()
    {
        int random = Random.Range(0, clips.GetLength(0));
        return clips[random];
    }

    public void OnWebShotLeft()
    {
        if (leftTimer > 0.0f)
        {
            return;
        }

        leftTimer = rechargeTime;

        leftSource.clip = SelectRandomClip();

        float pitch = Random.value * (pitchMax - pitchMin) + pitchMin;
        leftSource.pitch = pitch;

        leftSource.Play();
    }

    public void OnWebShotRight()
    {
        if (rightTimer > 0.0f)
        {
            return;
        }

        rightTimer = rechargeTime;

        rightSource.clip = SelectRandomClip();

        float pitch = Random.value * (pitchMax - pitchMin) + pitchMin;
        rightSource.pitch = pitch;

        rightSource.Play();
    }
}
