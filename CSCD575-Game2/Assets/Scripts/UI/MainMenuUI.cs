using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{

    private Button newGame;
    private Button settings;
    private Button quit;
    // private UIDocument uiDoc;
    // private VisualElement root;
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;


        newGame = root.Q<Button>("NewGame") as Button;
        newGame.RegisterCallback<ClickEvent>(OnClickNewGame);

        settings = root.Q("Settings") as Button;
        settings.RegisterCallback<ClickEvent>(OnClickSettings);

        quit = root.Q("Quit") as Button;
        quit.RegisterCallback<ClickEvent>(OnClickQuit);

    }

    private void OnClickNewGame(ClickEvent clicked)
    {
        Debug.Log("You pressed new game!");
    }

    private void OnNewGameDisable()
    {
        newGame.UnregisterCallback<ClickEvent>(OnClickNewGame);
    }


    private void OnClickSettings(ClickEvent clicked)
    {
        Debug.Log("You pressed settings!");
    }

    private void OnSettingsDisable()
    {
        settings.UnregisterCallback<ClickEvent>(OnClickSettings);
    }


    private void OnClickQuit(ClickEvent clicked)
    {
        Debug.Log("You pressed quit!");
    }

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