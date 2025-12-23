using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    public static WeatherController Instance;

    [SerializeField] GameObject rainObject;
    [SerializeField] GameObject snowObject;
    [SerializeField] GameObject petalsObject;

    ParticleSystem[] snowParticles;

    private void Awake()
    {
        Instance = this;

        if(rainObject) rainObject.SetActive(false);

        if (snowObject)
        {
            snowObject.SetActive(false);
            snowParticles = snowObject.GetComponentsInChildren<ParticleSystem>(true);
        }

        if(petalsObject) petalsObject.SetActive(false);
    }

    public void SetRain(bool on)
    {
        if (!rainObject) return;

        rainObject.SetActive(on);

        var ps = rainObject.GetComponentInChildren<ParticleSystem>(true);
        if(ps)
        {
            if(on) ps.Play();
            else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (on)
        {
            SetSnow(false);
            SetPetals(false);
        }
    }
    
    public void SetSnow(bool on)
    {
        if (!snowObject || snowParticles == null) return;

        snowObject.SetActive(on);

        foreach (var ps in snowParticles)
        {
            if (!ps) continue;

            if (on) ps.Play();
            else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (on)
        {
            SetRain(false);
            SetPetals(false);
        }
    }

    public void SetPetals(bool on)
    {
        if(!petalsObject) return;

        petalsObject.SetActive(on);

        var ps = petalsObject.GetComponentInChildren<ParticleSystem>(true);
        if (ps)
        {
            if (on) ps.Play();
            else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (on)
        {
            SetRain(false);
            SetSnow(false);
        }
    }

    public void ClearAllWeather()
    {
        SetRain(false);
        SetSnow(false);
        SetPetals(false);
    }
}
