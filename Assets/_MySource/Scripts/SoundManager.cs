using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundManager : MonoBehaviour
{

    public AudioSource UISoundSource;
    public AudioSource BackgroundSoundSource;
    public AudioSource EffectSoundSource;
    public AudioSource GameplaySoundSource;
    public AudioSource ParralaxEffect;

    public List<AudioClip> listUISnd;
    public List<AudioClip> listBackgroundSnd;
    public List<AudioClip> listEffectSnd;
    public List<AudioClip> listSuggestionSnd;
    public List<AudioClip> listParralaxEffect;

    public static SoundManager instance;

    private bool _enableBgMusic = true;
    private bool _enableMusic = true;

   


    // Use this for initialization
    void Awake()
    {
 

        if (instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }

    public bool isEnableMusic
    {
        set
        {
            _enableMusic = value;
            if (value)
            {
                if (_enableBgMusic)
                {
                    if (BackgroundSoundSource)
                    {
                        //BackgroundSoundSource.Play ();
                    }
                }
            }
            else
            {
                if (UISoundSource)
                {
                    UISoundSource.Stop();
                }
                if (BackgroundSoundSource)
                {
                    //BackgroundSoundSource.Stop ();
                }
                if (EffectSoundSource)
                {
                    EffectSoundSource.Stop();
                }

                if (GameplaySoundSource)
                {
                    GameplaySoundSource.Stop();
                }

                if (ParralaxEffect)
                {
                    ParralaxEffect.Stop();
                }
            }
        }
    }


    public bool isEnableBGM
    {
        set
        {
            _enableBgMusic = value;
            if (value)
            {
                BackgroundSoundSource.Play();
            }
            else
            {
                BackgroundSoundSource.Stop();
            }
        }
    }


    public void PlayUISound(string clip)
    {
        //Debug.Log("Sound " + clip);
        AudioClip ac = GetSpeechAudioClipFromName(clip);
        UISoundSource.clip = ac;
        if (_enableMusic)
        {
            if (ac != null)
            {
                //Debug.Log("Play Sound UI " + clip);
                UISoundSource.Play();
            }
        }

    }


    public void PlayUISound(int id)
    {
        if (_enableMusic)
        {

            AudioClip ac = GetSpeechAudioClipByID(id);
            if (ac != null)
            {
                UISoundSource.clip = ac;
                UISoundSource.Play();
            }
        }
    }

    public void PlayBackgroundSound(string clip)
    {
       
        AudioClip ac = GetBackgroundAudioClipFromName(clip);
        BackgroundSoundSource.clip = ac;
        if (_enableMusic && _enableBgMusic)
        {

            if (ac != null)
            {
               // Debug.Log("PlayBackgroundSound " + clip+" play");
                BackgroundSoundSource.Play();
            }
        }
    }

    public void playParralaxEffect(string clip)
    {
        if (_enableMusic)
        {

            AudioClip ac = getParralaxAudioClipFromName(clip);
            if (ac != null)
            {
                ParralaxEffect.clip = ac;
                ParralaxEffect.Play();
            }
        }
    }


    public void PlayEffectSound(string clip)
    {
        AudioClip ac = GetEffectAudioClipFromName(clip);
        EffectSoundSource.clip = ac;
        //AudioClip ac2 = GetGameplayAudioClipFromName(clip);
        //GameplaySoundSource.clip = ac2;
        if (_enableMusic)
        {
            if (ac != null)
            {
                EffectSoundSource.Play();
            }
            //else
            //{
            //    if (ac2 != null)
            //    {
            //        GameplaySoundSource.Play();
            //    }
            //}
        }
    }

    public void PlayGameplaySound(string clip)
    {
        AudioClip ac = GetGameplayAudioClipFromName(clip);
        GameplaySoundSource.clip = ac;
        AudioClip ac2 = GetEffectAudioClipFromName(clip);
        EffectSoundSource.clip = ac2;
        if (_enableMusic)
        {

            if (ac != null)
            {

                GameplaySoundSource.Play();
            }
            else
            {

                if (ac2 != null)
                {
                    EffectSoundSource.Play();
                }
            }
        }
    }

    public void playSuggestion(int id)
    {
        if (_enableMusic)
        {

            AudioClip ac = GetGameplayAudioClipByID(id);
            if (ac != null)
            {
                GameplaySoundSource.clip = ac;
                GameplaySoundSource.Play();
            }
        }
    }


    public void PauseBackgroundSound(bool isPause)
    {
        if (_enableMusic && _enableMusic)
        {
            if (isPause)
                BackgroundSoundSource.Pause();
            else
                BackgroundSoundSource.UnPause();
        }
    }


    public AudioClip GetSpeechAudioClipFromName(string name)
    {


        try
        {   
            foreach (AudioClip ac in listUISnd)
            {
                if (name == ac.name)
                    return ac;
            }
        }
        catch (System.Exception ex)
        {
            return null;
        }
        return null;

    }

    public AudioClip GetSpeechAudioClipByID(int nameID)
    {
        return listUISnd.ElementAt(nameID);
    }

    public AudioClip GetEffectAudioClipFromName(string name)
    {
        try
        {
            foreach (AudioClip ac in listEffectSnd)
            {
                if (name == ac.name)
                    return ac;
            }
        }
        catch (System.Exception ex)
        {
            return null;
        }
        return null;

    }

    public AudioClip GetGameplayAudioClipFromName(string name)
    {
        foreach (AudioClip ac in listSuggestionSnd)
        {
            if (name == ac.name)
                return ac;
        }

        return null;
    }

    public AudioClip GetGameplayAudioClipByID(int id)
    {
        if (id >= 0 && id < listSuggestionSnd.Count())
        {
            return listSuggestionSnd.ElementAt(id);
        }
        else
        {
            Debug.LogError("Out of list suggestion");
            return null;
        }
    }

    public AudioClip GetBackgroundAudioClipFromName(string name)
    {
        try
        {
            foreach (AudioClip ac in listBackgroundSnd)
            {
                if (name == ac.name)
                    return ac;
            }          
        }
        catch (System.Exception ex)
        {
            return null;
        }
        return null;

    }

    public AudioClip getParralaxAudioClipFromName(string name)
    {
        foreach (AudioClip ac in listParralaxEffect)
        {
            if (name == ac.name)
                return ac;
        }

        return null;
    }


}
