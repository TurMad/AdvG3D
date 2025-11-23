using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using PixelCrushers.DialogueSystem; // добавляем, чтобы вызывать DialogueManager

public class VisitorItemView : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] Button button;

    Action<string> onClick;
    string _id;

    // новый кусок: список разговоров для этого визитёра
    string[] _dialogueTitles;

    // этот метод остался тем же по сигнатуре и логике UI
    public void Setup(string id, string displayName, Action<string> onClick)
    {
        _id = id;
        this.onClick = onClick;

        if (label) label.text = displayName;

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                // твой обычный клик (логика, которая уже есть в VisitorListUI)
                this.onClick?.Invoke(_id);

                // наш диалог
                TryStartDialogue();
            });
        }
    }

    // новый метод: сюда мы положим массив диалогов визитёра
    public void SetDialogues(string[] dialogueTitles)
    {
        _dialogueTitles = dialogueTitles;
    }

    void TryStartDialogue()
    {
        Debug.Log("work");

        if (DialogueManager.instance == null)
        {
            Debug.LogWarning("NO DIALOGUE MANAGER IN SCENE");
            return;
        }

        if (_dialogueTitles == null)
        {
            Debug.LogWarning("_dialogueTitles == null for visitor " + _id);
            return;
        }

        if (_dialogueTitles.Length == 0)
        {
            Debug.LogWarning("_dialogueTitles.Length == 0 for visitor " + _id);
            return;
        }

        var index = UnityEngine.Random.Range(0, _dialogueTitles.Length);
        var convo = _dialogueTitles[index];

        Debug.Log("[VisitorItemView] start dialogue: " + convo + " (visitor: " + _id + ")");

        DialogueManager.StartConversation(convo);
    }
}