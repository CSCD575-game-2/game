using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TutorialUI : MonoBehaviour
{
    private Button startGame;
    [SerializeField] private SceneLoader loader;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        startGame = root.Q<Button>("StartGame") as Button;
        startGame.RegisterCallback<ClickEvent>(OnClickStartGame);
    }

    private void OnClickStartGame(ClickEvent clicked)
    {
        OnStartGameDisable();
        loader.LoadByName("Main");
    }

    private void OnStartGameDisable()
    { 
        startGame.UnregisterCallback<ClickEvent>(OnClickStartGame);
    }
}
