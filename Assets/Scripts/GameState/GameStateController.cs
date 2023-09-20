using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Utility;
using Scene = Utility.Scene;
using System.Linq;

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
    public GameObject cheesePrefab;
    public GameObject cheese;
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
    public bool isPaused;
    GameObject pauseMenu;
    GameObject gameOverBox;

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
        var activeScene = SceneManager.GetActiveScene();
        switch (activeScene.name)
        {
            case nameof(Scene.MainMenu):
                break;
            case nameof(Scene.GamePlay):
                StartCoroutine(StartGamePlay());
                break;
        }
    }

    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    private IEnumerator StartGamePlay()
    {
        while (
            instance.inputToggler == null
            || instance.cameraController == null
            || instance.cameraToggler == null
            || instance.pauseMenu == null
            || instance.gameOverBox == null
        )
        {
            instance.inputToggler = GameObject
                .Find("InputControllerToggler")
                .GetComponent<Toggler>();
            instance.cameraController = GameObject
                .Find("Cinemachine")
                .GetComponent<CameraController>();
            instance.cameraToggler = GameObject
                .Find("CameraControllerToggler")
                .GetComponent<Toggler>();
            instance.pauseMenu = GameObject.Find("PauseMenuBox");
            instance.pauseMenu.SetActive(false);
            instance.gameOverBox = GameObject.Find("GameOverBox");
            instance.gameOverBox.SetActive(false);
            yield return null;
        }
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

    public void SpawnCheese(Vector2 position)
    {
        cheese = Spawn(cheesePrefab, position);
    }

    private GameObject Spawn(GameObject entity, Vector2 position)
    {
        return Instantiate(entity, position, Quaternion.identity);
    }

    private void TogglePauseGame()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
        }
        // ToggleAllAudio();
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(instance.isPaused);
        TogglePauseGame();
    }

    public void WinGame()
    {
        StartCoroutine(GameOver("Win"));
    }

    public void LoseGame()
    {
        StartCoroutine(GameOver("Lose"));
    }

    private IEnumerator GameOver(string condition)
    {
        var enums = Enum.GetValues(typeof(IsEnabled)).OfType<IsEnabled>().ToList();
        enums.ForEach(e => Toggle(e, false));
        gameOverBox.SetActive(true);
        var conditions = gameOverBox.GetComponentsInChildren<Transform>();
        foreach (var c in conditions)
        {
            if (!c.name.Contains(condition) && !c.gameObject.Equals(gameOverBox))
            {
                c.gameObject.SetActive(false);
            }
        }

        yield return new WaitForSeconds(2);
        LoadScene(Scene.MainMenu);
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
