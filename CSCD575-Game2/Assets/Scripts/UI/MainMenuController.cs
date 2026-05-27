using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument main;
    [SerializeField] private UIDocument credits;

    void Start()
    {
        OpenMain();
    }

    public void OpenCredits()
    {
        if(main != null) main.rootVisualElement.style.display = DisplayStyle.None;

        if(credits != null)
        {
            credits.enabled = true;
            credits.rootVisualElement.style.display = DisplayStyle.Flex;
        }
    }

    public void OpenMain()
    {
        if(credits != null) credits.rootVisualElement.style.display = DisplayStyle.None;

        if(main != null)
        {
            main.enabled = true;
            main.rootVisualElement.style.display = DisplayStyle.Flex;
        }
    }

}
