using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MusicManager : MonoBehaviour
{
    public AudioSource AudioPlayer;
    public Song[] songs;
    static Song[] internalSongs;
    static bool FirstSet;
    public ScenePreset[] SongDefaults;
    public int CurrentSong = -1;
    static int PriorSong;
    public float TimeTillAudioForget = 120f;
    public float AudioBacktrack = 1f;
    public float Fade = 0.8f;
    void Start()
    {
        //Setting Internals
        if (FirstSet == false)
        {
            internalSongs = songs;
            FirstSet = true;
        }
        //ChooseSong
        string SceneName = SceneManager.GetActiveScene().name;
        foreach (ScenePreset S in SongDefaults)
        {
            if (S.Name == SceneName)
            {
                int indexID = 0;
                foreach (Song so in internalSongs)
                {
                    if (so.Name == S.InitialSong)
                    {
                        CurrentSong = indexID;
                        break;
                    }
                    indexID++;
                }
                if (CurrentSong != -1)
                {
                    break;
                }
            }
        }
        //Falling Back if no song was found
        if (CurrentSong == -1)
        {
            Debug.LogError("Failed to find propper song");
            CurrentSong = 1;
        }

        UpdateSong();
    }
    private void UpdateSong()
    { 
        //Check if song has been played within the TimeTillAudioForget
        if (internalSongs[CurrentSong].LastPlayed + TimeTillAudioForget > Time.time && CurrentSong != PriorSong) //The Song has not been played and is not the same song
        {
            AudioPlayer.clip = internalSongs[CurrentSong].AC;
            if (internalSongs[CurrentSong].PointInSong - AudioBacktrack > 0)
            {
                AudioPlayer.time = internalSongs[CurrentSong].PointInSong - AudioBacktrack;
                StartCoroutine(FadeIn(AudioPlayer, Fade)) ;
            }
            else
            {
                AudioPlayer.time = 0;
            }
            AudioPlayer.Play();
        }
        else if(CurrentSong == PriorSong) // Same song as before
        {
            AudioPlayer.clip = internalSongs[CurrentSong].AC;
            AudioPlayer.time = internalSongs[CurrentSong].PointInSong;
            AudioPlayer.Play();
        }
        else // The song has been played in the last 2 minuits
        {
            AudioPlayer.clip = internalSongs[CurrentSong].AC;
            AudioPlayer.time = 0;
            AudioPlayer.Play();
        }
    }
    public void RequestNewSong(string Name)
    {
        int indexID = 0;
        foreach (Song so in internalSongs)
        {
            if (so.Name == Name)
            {
                CurrentSong = indexID;
                break;
            }
            indexID++;
        }
        //Falling Back if no song was found
        if (CurrentSong == -1)
        {
            Debug.LogError("Failed to find propper song");
            CurrentSong = 1;
        }
        UpdateSong();
    }
    void Update()
    {
        internalSongs[CurrentSong].PointInSong = AudioPlayer.time;
        internalSongs[CurrentSong].LastPlayed = Time.time;
        PriorSong = CurrentSong;
        if(!InFade)
        {
            AudioPlayer.volume = 1;
        }
    }
    public void PlaySound(AudioClip Sound)
    {
        AudioPlayer.PlayOneShot(Sound,1);
    }
    private static bool InFade;
    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        InFade = true;
        float startVolume = 0.2f;
        float TrueVolume = 1;
        float SubstituteVolume = 0;
        audioSource.volume = SubstituteVolume * TrueVolume;
        audioSource.Play();
        while (SubstituteVolume < 1.0f)
        {
            SubstituteVolume += startVolume * Time.fixedDeltaTime / FadeTime;
            audioSource.volume = SubstituteVolume * TrueVolume;
            yield return new WaitForEndOfFrame();
        }
        SubstituteVolume = 1f;
        audioSource.volume = SubstituteVolume * TrueVolume;
        InFade = false;
    }
}
[System.Serializable]
public struct Song
{
    public string Name;
    public AudioClip AC;
    public float PointInSong;
    public float LastPlayed;
}
[System.Serializable]
public struct ScenePreset
{
    public string Name;
    public string InitialSong;
}