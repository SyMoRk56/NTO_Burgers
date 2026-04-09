
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnButtonClickSound : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clip;
    public static OnButtonClickSound instance;
    private void Start()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        foreach (var buttin in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            print("ADD LISTENER");
            buttin.onClick.AddListener(() => PlaySound());
        }
    }
   
    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        foreach(var buttin in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            print("ADD LISTENER");
            buttin.onClick.AddListener(()=>PlaySound());
        }
    }
    void PlaySound()
    {
        Debug.Log("Button sound");
        source.PlayOneShot(clip);
    }
}
