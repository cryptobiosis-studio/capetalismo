using UnityEngine;

public class Music : MonoBehaviour
{
    private static Music instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        AudioSource src = GetComponent<AudioSource>();
        if (src != null && !src.isPlaying)
            src.Play();
    }
}
