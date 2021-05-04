using UnityEngine.Audio;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0.0f, 1.0f)]
    public float volume;

    [HideInInspector]
    public AudioSource source;


    public void FadeOut(float numSeconds)
    {
        float currentTime = 0.0f;
        while(currentTime < numSeconds)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(volume, 0.0f, currentTime / numSeconds);
        }

        source.Stop();
        source.time = 0.0f;
        source.volume = volume;
    }

    public void CopyValuesFrom(Sound otherSoundObj)
    {
        this.name = otherSoundObj.name;
        this.clip = otherSoundObj.clip;
        this.volume = otherSoundObj.volume;
        this.source = otherSoundObj.source;
    }
}
