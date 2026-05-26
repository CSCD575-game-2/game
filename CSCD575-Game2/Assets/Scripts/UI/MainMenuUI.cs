using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;


public class MainMenuUI : MonoBehaviour
{

    // This is so we can add and edit a scene manager from the Unity editor, while still keeping variables private and inaccessible from other scripts.
    [SerializeField] private SceneLoader loader;
    [SerializeField] private UIMainMenuManager manager;

    // Self explanatory, we are pre-instatiating buttons so we don't have to pass buttons as references in the script
    private Button newGame;
    private Button credits;
    private Button quit;
    // private UIDocument uiDoc;
    // private VisualElement root;

    // Once the game object is enabeld this runs
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;


        // New Game button(load to the 'Main' scene of the game)
        newGame = root.Q<Button>("NewGame") as Button;
        newGame.RegisterCallback<ClickEvent>(OnClickNewGame);

        // credits button(load the credits UI from the current UI context)
        credits = root.Q("Credits") as Button;
        credits.RegisterCallback<ClickEvent>(OnClickCredits);

        // Quit button(Quit out of the application)
        quit = root.Q("Quit") as Button;
        quit.RegisterCallback<ClickEvent>(OnClickQuit);

        // manager.ShowMainMenu();

    }

    // New game button scripts
    private void OnClickNewGame(ClickEvent clicked)
    {
        //load the 'Main' scene
        //Debug.Log("You pressed new game!");
        OnNewGameDisable();
        OnCreditsDisable();
        OnQuitDisable();
        loader.LoadByName("Main");
    }

    // Clears up any calls 'New Game' button makes once scene is unloaded.
    private void OnNewGameDisable()
    {
        newGame.UnregisterCallback<ClickEvent>(OnClickNewGame);
    }


    // Self explanatory
    private void OnClickCredits(ClickEvent clicked)
    {
        //Debug.Log("You pressed credits!");
        manager.ShowCredits();
        
    }

    // Again, unregisters button calls when new scene is loaded
    private void OnCreditsDisable()
    {
        credits.UnregisterCallback<ClickEvent>(OnClickCredits);
    }


    // Again, self explanatory
    private void OnClickQuit(ClickEvent clicked)
    {
        //Debug.Log("You pressed quit!");

        //This won't do anything in the Unity Editor view, but it will quit out once we have a proper build
        Application.Quit();
    }

    // See above 'button'Disable() functions for explanation
    private void OnQuitDisable()
    {
        quit.UnregisterCallback<ClickEvent>(OnClickQuit);
    }
}

        /* Old garbage code that was here before I realised I assigned the script to the wrong object */


       // newGame.clicked += () => null;
        // Debug.Log(root == null);
        // Debug.Log(GetComponent<UIDocument>()?.rootVisualElement);
        // UIDocument uiDoc = GetComponent<UIDocument>();