using UnityEngine;
using System;

// Lớp Sound để dễ dàng quản lý các audio clip trong Inspector
[System.Serializable]
public class Sound
{
    public string name; // Tên để gọi âm thanh, ví dụ: "win", "lose", "explode"
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sounds; // Mảng chứa tất cả các âm thanh của game
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại AudioManager khi chuyển scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hàm để các script khác gọi đến và phát âm thanh theo tên
    public void Play(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Không tìm thấy âm thanh: " + soundName);
            return;
        }
        audioSource.PlayOneShot(s.clip);
    }
}