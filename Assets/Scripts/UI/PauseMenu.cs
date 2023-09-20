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
            GameStateController.instance.TogglePauseGame();
        }
        else if (isBackToMainMenu)
        {
            GameStateController.instance.TogglePauseGame();
            QuitToMainMenu();
        }
    }

    private void QuitToMainMenu()
    {
        GameStateController.instance.LoadScene(Scene.MainMenu);
    }
}
