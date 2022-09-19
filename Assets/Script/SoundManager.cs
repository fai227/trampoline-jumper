using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region Instance Setting
    public static SoundManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Component Setting
    [SerializeField] private AudioSource audioSourceForBGM;
    [SerializeField] private AudioSource audioSourceForSE;
    #endregion

    #region Methods
    /// <summary>
    /// PlayOneShot
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volumeScale"></param>
    public static void PlayOneShot(AudioClip clip, float volumeScale = -1f)
    {
        if(volumeScale < 0f)
        {
            Instance.audioSourceForSE.PlayOneShot(clip, defaultSEVolume);
        }
        else
        {
            Instance.audioSourceForSE.PlayOneShot(clip, volumeScale);
        }
    } 

    private static float defaultSEVolume = 1f;
    public static void Initialize()
    {
        Instance.audioSourceForBGM.volume = PlayerPrefs.GetFloat("BGM");
        defaultSEVolume = PlayerPrefs.GetFloat("SE");
    }
    #endregion
}
