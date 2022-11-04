using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class AudioManager : MonoBehaviour
    {
        AudioSource musicSource;
        AudioSource effectSource;

        public AudioClip battleTheme;
        public AudioClip heroFightTheme;
        public AudioClip enemyFightTheme;

        // Start is called before the first frame update
        void Start()
        {
            musicSource = transform.GetChild(0).GetComponent<AudioSource>();
            effectSource = transform.GetChild(1).GetComponent<AudioSource>();

            musicSource.clip = battleTheme;
            musicSource.Play();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PlayMusic(AudioClip clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlaySE(AudioClip clip)
        {
            effectSource.PlayOneShot(clip);
        }
    }
}
