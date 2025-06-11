using UnityEngine;
using UnityEngine.InputSystem;

public class MenuNavigation : MonoBehaviour
{
    public enum MenuState { Main, Fun, Movement, Visual }
    public MenuState currentState = MenuState.Main;

    private Menu mainMenu;
    private Fun fun;
    private Movement movement;
    private Visual visual;
    private Miscellaneous miscellaneous;
    private OPMods opMods

    private void Start()
    {
        mainMenu = GetComponent<Menu>();
        fun = GetComponent<Fun>();
        movement = GetComponent<Movement>();
        visual = GetComponent<Visual>();
        miscellaneous = GetComponent<Miscellaneous>();
        op = GetComponent<OP>();
    }

    private void Update()
    {
        if (Gamepad.current?.buttonEast.wasPressedThisFrame ?? false) // B button
        {
            ReturnToMain();
        }

        switch(currentState)
        {
            case MenuState.Main:
                mainMenu.UpdateMenu();
                break;
            case MenuState.Fun:
                funMods.ShowMenu();
                break;
            case MenuState.Movement:
                movementMods.ShowMenu();
                break;
            case MenuState.Visual:
                visualMods.ShowMenu();
                break;
        }
    }

    public void ReturnToMain()
    {
        currentState = MenuState.Main;
    }

    public void SelectMenuItem(int index)
    {
        switch(index)
        {
            case 0: currentState = MenuState.Fun; break;
            case 1: currentState = MenuState.Movement; break;
            case 2: currentState = MenuState.Visual; break;
        }
    }
}