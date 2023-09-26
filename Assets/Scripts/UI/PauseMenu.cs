using Scene = Utility.Scene;

public class PauseMenu : MenuController
{
    public bool isContinue;
    public bool isBackToMainMenu;

    protected override void HandleClick()
    {
        base.HandleClick();
        if (isContinue)
        {
            GameStateController.singleton.TogglePauseMenu();
        }
        else if (isBackToMainMenu)
        {
            GameStateController.singleton.TogglePauseMenu();
            QuitToMainMenu();
        }
    }

    private void QuitToMainMenu()
    {
        GameStateController.singleton.DisableScene();
        GameStateController.OnLoadScene(Scene.MainMenu);
    }
}
