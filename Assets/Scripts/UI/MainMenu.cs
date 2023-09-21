using UnityEngine;
using Scene = Utility.Scene;

public class MainMenu : MenuController
{
    public bool isPlay;
    public bool isQuit;

    // Start is called before the first frame update


    protected override void HandleClick()
    {
        base.HandleClick();
        if (isPlay)
        {
            GameStateController.OnLoadScene(Scene.GamePlay);
        }
        else if (isQuit)
        {
            Application.Quit();
        }
    }
}
