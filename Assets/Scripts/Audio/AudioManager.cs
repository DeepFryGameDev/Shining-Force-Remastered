using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattleThemes
{
    BATTLE,
    PLAYERFIGHT,
    ENEMYFIGHT
}

public enum UISoundEffects
{
    CONFIRM,
    CANCEL,
    HOVER,
    INVALIDACTION,
    OPENMENU,
    CLOSEMENU
}

namespace DeepFry
{
    public class AudioManager : MonoBehaviour
    {
        AudioSource musicSource;
        AudioSource uiSource;
        AudioSource effectSource;

        public AudioClip currentlyPlaying;

        AudioClip dungeonBattle;
        AudioClip heroFight;
        AudioClip enemyFight;

        AudioClip townTheme;

        AudioClip invalidAction;

        AudioClip openMenu;
        AudioClip closeMenu;

        AudioClip confirm;
        AudioClip cancel;

        AudioClip hover1;
        AudioClip hover2;
        AudioClip hover3;
        AudioClip hover4;
        AudioClip hover5;
        AudioClip hover6;
        AudioClip hover7;
        AudioClip hover8;
        AudioClip hover9;
        AudioClip hover10;


        void Start()
        {
            PrepManager();

            PlaySceneTheme();
        }

        void PrepManager() // this should be optimized later to reduce on load times when scene is loaded
        {
            musicSource = transform.GetChild(0).GetComponent<AudioSource>();
            uiSource = transform.GetChild(1).GetComponent<AudioSource>();
            effectSource = transform.GetChild(2).GetComponent<AudioSource>();

            // BGM
            townTheme = Resources.Load<AudioClip>("Audio/Music/LivelyTown");
            // Battle BGM
            dungeonBattle = Resources.Load<AudioClip>("Audio/Music/DungeonBattle");
            heroFight = Resources.Load<AudioClip>("Audio/Music/HeroFight");
            enemyFight = Resources.Load<AudioClip>("Audio/Music/EnemyFight");

            //SFX
            invalidAction = Resources.Load<AudioClip>("Audio/UISE/Error");

            openMenu = Resources.Load<AudioClip>("Audio/UISE/OpenMenu");
            closeMenu = Resources.Load<AudioClip>("Audio/UISE/CloseMenu");

            confirm = Resources.Load<AudioClip>("Audio/UISE/Confirm");
            cancel = Resources.Load<AudioClip>("Audio/UISE/Cancel");

            hover1 = Resources.Load<AudioClip>("Audio/UISE/Hover1");
            hover2 = Resources.Load<AudioClip>("Audio/UISE/Hover2");
            hover3 = Resources.Load<AudioClip>("Audio/UISE/Hover3");
            hover4 = Resources.Load<AudioClip>("Audio/UISE/Hover4");
            hover5 = Resources.Load<AudioClip>("Audio/UISE/Hover5");
            hover6 = Resources.Load<AudioClip>("Audio/UISE/Hover6");
            hover7 = Resources.Load<AudioClip>("Audio/UISE/Hover7");
            hover8 = Resources.Load<AudioClip>("Audio/UISE/Hover8");
            hover9 = Resources.Load<AudioClip>("Audio/UISE/Hover9");
            hover10 = Resources.Load<AudioClip>("Audio/UISE/Hover10");
        }

        void PlaySceneTheme()
        {
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 0: // Town
                    PlayMusic(townTheme);
                    break;
                case 1: // Battle
                    PlayMusic(dungeonBattle);
                    break;
            }
        }

        public void PlayBattleMusic(BattleThemes theme)
        {
            switch (theme)
            {
                case BattleThemes.BATTLE:
                    PlayMusic(dungeonBattle);
                    break;
                case BattleThemes.ENEMYFIGHT:
                    PlayMusic(enemyFight);
                    break;
                case BattleThemes.PLAYERFIGHT:
                    PlayMusic(heroFight);
                    break;
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            currentlyPlaying = clip;
            musicSource.clip = currentlyPlaying;
            musicSource.Play();
        }

        public void PlayUI(UISoundEffects uise)
        {
            Debug.Log("AudioManager) Playing UI SE: " + uise.ToString());

            switch (uise)
            {
                case UISoundEffects.HOVER:
                    playUISE(GetRandomHoverAudioClip());
                    break;
                case UISoundEffects.CONFIRM:
                    playUISE(confirm);
                    break;
                case UISoundEffects.CANCEL:
                    playUISE(cancel);
                    break;
                case UISoundEffects.INVALIDACTION:
                    playUISE(invalidAction);
                    break;
                case UISoundEffects.OPENMENU:
                    playUISE(openMenu);
                    break;
                case UISoundEffects.CLOSEMENU:
                    playUISE(closeMenu);
                    break;
            }
        }

        void playUISE(AudioClip se)
        {
            uiSource.PlayOneShot(se);
        }

        public void PlayEffect(AudioClip clip)
        {
            effectSource.PlayOneShot(clip);
        }

        AudioClip GetRandomHoverAudioClip()
        {
            int rand = GetRandomBetweenRange(1, 10);

            switch (rand)
            {
                case 1:
                    return hover1;
                case 2:
                    return hover2;
                case 3:
                    return hover3;
                case 4:
                    return hover4;
                case 5:
                    return hover5;
                case 6:
                    return hover6;
                case 7:
                    return hover7;
                case 8:
                    return hover8;
                case 9:
                    return hover9;
                case 10:
                    return hover10;
            }

            return null;
        }

        public int GetRandomBetweenRange(int min, int max)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            return Random.Range(min, max + 1);
        }
    }
}
