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
            GameStateController.instance.TogglePauseMenu();
        }
        else if (isBackToMainMenu)
        {
            GameStateController.instance.TogglePauseMenu();
            QuitToMainMenu();
        }
    }

    private void QuitToMainMenu()
    {
        GameStateController.instance.LoadScene(Scene.MainMenu);
    }
}
