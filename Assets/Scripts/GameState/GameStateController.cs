using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public static GameStateController singleton;

    public GameObject playerPrefab;
    public GameObject alienPrefab;
    public GameObject cheesePrefab;
    public GameObject cheese;
    public HashSet<IsEnabled> isEnabled;

    private Toggler inputToggler;
    public CameraController cameraController;
    public Toggler cameraToggler;

    public bool isPaused;
    GameObject pauseMenu;
    GameObject gameOverBox;
    public AudioController audioState;
    public AIDirectorController aiDirector;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        singleton = null;
    }

    void Awake()
    {
        isEnabled = new HashSet<IsEnabled>();
        if (singleton == null)
        {
            DontDestroyOnLoad(this.gameObject);
            singleton = this;
            SetupScene();
        }
        else if (singleton != this)
        {
            Destroy(this.gameObject);
        }
    }

    public static void OnLoadScene(Scene scene)
    {
        singleton.StartCoroutine(LoadScene(scene));
    }

    private static IEnumerator LoadScene(Scene scene)
    {
        var asyncLoadLevel = SceneManager.LoadSceneAsync(scene.ToString());
        while (!asyncLoadLevel.isDone)
        {
            yield return null;
        }
        singleton.SetupScene();
    }

    private void SetupScene()
    {
        var activeScene = SceneManager.GetActiveScene();
        switch (activeScene.name)
        {
            case nameof(Scene.MainMenu):
                goto default;
            case nameof(Scene.GamePlay):
                aiDirector = GetComponentInChildren<AIDirectorController>();
                aiDirector.Initialize();
                StartCoroutine(StartGamePlay());
                goto default;
            default:
                audioState = GetComponentInChildren<AudioController>();
                StartCoroutine(audioState.InitializeAudio());
                break;
        }
    }

    private static IEnumerator StartGamePlay()
    {
        while (
            singleton.inputToggler == null
            || singleton.cameraController == null
            || singleton.cameraToggler == null
            || singleton.pauseMenu == null
            || singleton.gameOverBox == null
        )
        {
            singleton.inputToggler = GameObject
                .Find("InputControllerToggler")
                .GetComponent<Toggler>();
            singleton.cameraController = GameObject
                .Find("Cinemachine")
                .GetComponent<CameraController>();
            singleton.cameraToggler = GameObject
                .Find("CameraControllerToggler")
                .GetComponent<Toggler>();
            singleton.pauseMenu = GameObject.Find("PauseMenuBox");
            singleton.pauseMenu.SetActive(false);
            singleton.gameOverBox = GameObject.Find("GameOverBox");
            singleton.gameOverBox.SetActive(false);
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
        aiDirector.player = Spawn(playerPrefab, position);
    }

    public void SpawnAlien(Vector2 position)
    {
        aiDirector.aliens.Add(Spawn(alienPrefab, position));
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
        audioState.ToggleMusic(isPaused);
    }

    public void TogglePauseMenu()
    {
        TogglePauseGame();
        pauseMenu.SetActive(singleton.isPaused);
    }

    public void WinGame()
    {
        audioState.PlaySound(Sound.Win);
        StartCoroutine(GameOver("Win"));
    }

    public void LoseGame()
    {
        audioState.PlaySound(Sound.Lose);
        StartCoroutine(GameOver("Lose"));
    }

    private IEnumerator GameOver(string condition)
    {
        aiDirector.Disable();
        audioState.ToggleMusic(true);
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

        yield return new WaitForSeconds(5);
        OnLoadScene(Scene.MainMenu);
    }
}
