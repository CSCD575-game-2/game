using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class CreditsUI : MonoBehaviour
{
    private Button backArrow;
    [SerializeField] private MainMenuController controller;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        backArrow = root.Q<Button>("BackArrow") as Button;
        backArrow.RegisterCallback<ClickEvent>(OnBackClick);
    }

    private void OnBackClick(ClickEvent clicked)
    {
        controller.OpenMain();
    }

    private void OnBackDisable()
    {
        backArrow.UnregisterCallback<ClickEvent>(OnBackClick);
    }
}
