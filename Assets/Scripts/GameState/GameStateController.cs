using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum IsEnabled
{
    INPUT,
    CAMERA
}

public class GameStateController : MonoBehaviour
{
    public static GameStateController instance;

    public GameObject playerPrefab;
    public GameObject player;
    public GameObject alienPrefab;
    public GameObject alien;
    public HashSet<IsEnabled> isEnabled;

    private Toggler inputToggler;
    public CameraController cameraController;
    public Toggler cameraToggler;

    // public List<string> hasMetTheseFriends;
    // public List<string> hasMetTheseBosses;
    // public GameObject ammo;
    // public GameObject startAmmo;
    // public int sanity;
    // public int startSanity;
    // public int deathThreshold;
    // public int hasDiedInTheLab;
    // public int hasDiedInMensa;
    // public int hasDiedInTheOffice;
    // public int hasDiedInTheEnd;
    // public bool isPaused;
    // public AudioSource currentDungeonMusic;
    // public AudioSource currentBossMusic;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        instance = null;
    }

    void Awake()
    {
        isEnabled = new HashSet<IsEnabled>();
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        StartScene();
    }

    private void StartScene()
    {
        inputToggler = GameObject.Find("InputControllerToggler").GetComponent<Toggler>();
        cameraController = GameObject.Find("Cinemachine").GetComponent<CameraController>();
        cameraToggler = GameObject.Find("CameraControllerToggler").GetComponent<Toggler>();
    }

    public void Toggle(IsEnabled isEnabled, bool enabled)
    {
        switch (isEnabled)
        {
            case IsEnabled.INPUT:
                Toggle(enabled, inputToggler);
                break;
            case IsEnabled.CAMERA:
                Toggle(enabled, cameraToggler);
                break;
        }
        if (enabled)
        {
            this.isEnabled.Add(isEnabled);
        }
        else
        {
            this.isEnabled.Remove(isEnabled);
        }
    }

    private void Toggle(bool enabled, Toggler toggler)
    {
        toggler.ToggleController(enabled);
    }

    public void SpawnPlayer(Vector2 position)
    {
        player = Spawn(playerPrefab, position);
    }

    public void SpawnAlien(Vector2 position)
    {
        alien = Spawn(alienPrefab, position);
    }

    private GameObject Spawn(GameObject entity, Vector2 position)
    {
        return Instantiate(entity, position, Quaternion.identity);
    }

    // private void Start()
    // {
    //     currentBossMusic = new AudioSource();
    // }

    // public void StartDungeonMusicInMostDungeons()
    // {
    //     if (SceneManager.GetActiveScene().name != "EndDungeon")
    //     {
    //         StartDungeonMusic();
    //     }
    // }

    // public void StartDungeonMusic()
    // {
    //     if (!currentDungeonMusic)
    //     {
    //         currentDungeonMusic = GameObject.Find("DungeonMusic").GetComponent<AudioSource>();
    //         currentDungeonMusic.Play();
    //     }
    // }

    // public void StopDungeonMusic()
    // {
    //     if (currentDungeonMusic) currentDungeonMusic.Pause();
    //     currentDungeonMusic = null;
    // }

    // public void StartBossMusic(AudioSource bossMusic)
    // {
    //     currentBossMusic = bossMusic;
    //     bossMusic.Play();
    // }

    // public void TogglePauseGame(GameObject pauseMenu)
    // {
    //     isPaused = !isPaused;
    //     pauseMenu.SetActive(GameStateController.Instance.isPaused);
    //     if (isPaused)
    //     {
    //         Time.timeScale = 0;
    //     }
    //     else
    //     {
    //         Cursor.visible = false;
    //         Cursor.lockState = CursorLockMode.Locked;
    //         Time.timeScale = 1;
    //     }
    //     ToggleAllAudio();
    // }

    // private void ToggleAllAudio()
    // {
    //     if (isPaused)
    //     {
    //         if (currentDungeonMusic) currentDungeonMusic.Pause();
    //         if (currentBossMusic) currentBossMusic.Pause();
    //     }
    //     else
    //     {
    //         if (currentDungeonMusic) currentDungeonMusic.Play();
    //         if (currentBossMusic) currentBossMusic.Play();
    //     }
    // }
}
