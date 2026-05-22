using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Camera rtsCamera;
    [SerializeField] private Camera shipCamera;

    private bool usingRTS = true;

    private void Start()
    {
        SetActiveCamera(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            usingRTS = !usingRTS;
            SetActiveCamera(usingRTS);
        }
    }

    private void SetActiveCamera(bool useRTS)
    {
        rtsCamera.gameObject.SetActive(useRTS);
        shipCamera.gameObject.SetActive(!useRTS);
    }
}
