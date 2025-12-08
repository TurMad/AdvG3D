using UnityEngine;

public class MainUIAndCameraController : MonoBehaviour
{
    [SerializeField] private GameObject[] roomsAndUIElements;
    
    public void ShowReception()
    {
        HideAllElements();
        roomsAndUIElements[0].SetActive(true);
    }

    public void ShowCabinet()
    {
        HideAllElements();
        roomsAndUIElements[1].SetActive(true);
    }
    
    private void HideAllElements()
    {
        foreach (var var in roomsAndUIElements)
        {
            var.SetActive(false);
        }
    }
}
