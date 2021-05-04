using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [HideInInspector]
    public static AudioManager instance;

    public List<Sound> SFX;
    public List<Sound> buildThemes;
    public List<Sound> battleThemes;
    public List<Sound> menuThemes;

    private Sound currentPlayingTheme;
    private int buildIndex = 0;
    private int battleIndex = 0;
    private int menuIndex = 0;

    private enum ThemeTypes { BUILD, BATTLE, MENU, NONE }
    private ThemeTypes activeTheme = ThemeTypes.NONE;

    public enum EventThemeOptions { MAINMENU, WINSCREEN, LOSESCREEN, FINALROUND, FINALROUNDWAVE }
    public Sound mainMenuTheme;
    public Sound winTheme;
    public Sound loseTheme;
    public Sound lastRound;
    public Sound lastRoundLastWave;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitSoundFiles();

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(currentPlayingTheme != null && currentPlayingTheme.source != null)
        {
            if(!currentPlayingTheme.source.isPlaying)
            {
                switch (activeTheme)
                {
                    case ThemeTypes.BUILD:
                        PlayBuildThemePlaylist();
                        break;
                    case ThemeTypes.BATTLE:
                        PlayBattleThemePlaylist();
                        break;
                    case ThemeTypes.MENU:
                        PlayMenuThemePlaylist();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void InitSoundFiles()
    {
        foreach (Sound s in SFX)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
        }

        foreach (Sound s in buildThemes)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
        }

        foreach (Sound s in battleThemes)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
        }

        foreach (Sound s in menuThemes)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
        }

        mainMenuTheme.source = gameObject.AddComponent<AudioSource>();
        mainMenuTheme.source.clip = mainMenuTheme.clip;
        mainMenuTheme.source.volume = mainMenuTheme.volume;

        winTheme.source = gameObject.AddComponent<AudioSource>();
        winTheme.source.clip = winTheme.clip;
        winTheme.source.volume = winTheme.volume;

        loseTheme.source = gameObject.AddComponent<AudioSource>();
        loseTheme.source.clip = loseTheme.clip;
        loseTheme.source.volume = loseTheme.volume;

        lastRound.source = gameObject.AddComponent<AudioSource>();
        lastRound.source.clip = lastRound.clip;
        lastRound.source.volume = lastRound.volume;

        lastRoundLastWave.source = gameObject.AddComponent<AudioSource>();
        lastRoundLastWave.source.clip = lastRoundLastWave.clip;
        lastRoundLastWave.source.volume = lastRoundLastWave.volume;
    }

    public void PlaySFX(string soundName)
    {
        foreach(Sound s in SFX)
        {
            if(s.name == soundName)
            {
                s.source.Play();
                return;
            }
        }

        Debug.Log("Sound: \"" + soundName + "\" was requested to be played, but could not be found!");
    }

    public void PlayBuildThemePlaylist()
    {
        if (currentPlayingTheme != null)
        {
            if (currentPlayingTheme.source.isPlaying)
            {
                int index = (buildIndex - 1 + buildThemes.Count) % buildThemes.Count;
                if (currentPlayingTheme.name == buildThemes[index].name)
                    return;
            }
        }

        activeTheme = ThemeTypes.BUILD;

        FadeCurrentPlayingTheme();
        currentPlayingTheme = buildThemes[buildIndex];
        currentPlayingTheme.source.Play();

        buildIndex = (buildIndex + 1) % buildThemes.Count;
    }

    public void PlayBattleThemePlaylist()
    {
        if (currentPlayingTheme != null)
        {
            if (currentPlayingTheme.source.isPlaying)
            {
                int index = (battleIndex - 1 + battleThemes.Count) % battleThemes.Count;
                if (currentPlayingTheme.name == battleThemes[index].name)
                    return;
            }
        }

        activeTheme = ThemeTypes.BATTLE;

        FadeCurrentPlayingTheme();
        currentPlayingTheme = battleThemes[battleIndex];
        currentPlayingTheme.source.Play();

        battleIndex = (battleIndex + 1) % battleThemes.Count;
    }

    public void PlayMenuThemePlaylist()
    {
        if (currentPlayingTheme != null)
        {
            if (currentPlayingTheme.source.isPlaying)
            {
                int index = (menuIndex - 1 + menuThemes.Count) % menuThemes.Count;
                if (currentPlayingTheme.name == menuThemes[index].name)
                    return;
            }
        }

        activeTheme = ThemeTypes.MENU;

        FadeCurrentPlayingTheme();
        currentPlayingTheme = menuThemes[menuIndex];
        currentPlayingTheme.source.Play();

        menuIndex = (menuIndex + 1) % menuThemes.Count;
    }

    public void PlayTheme(EventThemeOptions eventTheme)
    {
        switch (eventTheme)
        {
            case EventThemeOptions.MAINMENU:
                activeTheme = ThemeTypes.MENU;
                if (currentPlayingTheme != null && currentPlayingTheme.name == mainMenuTheme.name)
                    return;

                FadeCurrentPlayingTheme();
                currentPlayingTheme = mainMenuTheme;
                break;
            case EventThemeOptions.WINSCREEN:
                activeTheme = ThemeTypes.MENU;
                FadeCurrentPlayingTheme();
                currentPlayingTheme = winTheme;
                break;
            case EventThemeOptions.LOSESCREEN:
                activeTheme = ThemeTypes.MENU;
                FadeCurrentPlayingTheme();
                currentPlayingTheme = loseTheme;
                break;
            case EventThemeOptions.FINALROUND:
                activeTheme = ThemeTypes.BATTLE;
                FadeCurrentPlayingTheme();
                currentPlayingTheme = lastRound;
                break;
            case EventThemeOptions.FINALROUNDWAVE:
                activeTheme = ThemeTypes.BATTLE;
                FadeCurrentPlayingTheme();
                currentPlayingTheme = lastRoundLastWave;
                break;
            default:
                break;
        }

        currentPlayingTheme.source.Play();
    }

    private void FadeCurrentPlayingTheme()
    {
        if (currentPlayingTheme != null && currentPlayingTheme.source != null)
        {
            if (currentPlayingTheme.source.isPlaying)
            {
                currentPlayingTheme.FadeOut(3.0f);
            }
        }
    }
}
