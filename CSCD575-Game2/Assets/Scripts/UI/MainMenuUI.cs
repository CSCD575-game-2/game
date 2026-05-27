using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{

    private Button newGame;
    private Button credits;
    private Button quit;
    // private UIDocument uiDoc;
    // private VisualElement root;
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;


        newGame = root.Q<Button>("NewGame") as Button;
        newGame.RegisterCallback<ClickEvent>(OnClickNewGame);

        credits = root.Q("Credits") as Button;
        credits.RegisterCallback<ClickEvent>(OnClickCredits);

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


    private void OnClickCredits(ClickEvent clicked)
    {
        Debug.Log("You pressed credits!");
    }

    private void OnCreditsDisable()
    {
        credits.UnregisterCallback<ClickEvent>(OnClickCredits);
    }


    private void OnClickQuit(ClickEvent clicked)
    {
        // Debug.Log("You pressed quit!");
        Application.Quit();
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