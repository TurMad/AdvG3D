using System;
using UnityEngine;

public class MainUIAndCameraController : MonoBehaviour
{
    [SerializeField] private GameObject[] roomsAndUIElements;

    private void Start()
    {
        ShowCabinet();
    }

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

    public void GoToMap()
    {
        HideAllElements();
        CameraController.Instance.ShowMapTop();
    }
}
