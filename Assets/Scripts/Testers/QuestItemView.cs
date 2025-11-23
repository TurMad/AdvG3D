using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class QuestItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button btn;
    private string _id;
    private Action<string> _onClick;

    public void Setup(string questId, Action<string> onClick)
    {
        _id = questId;
        _onClick = onClick;
        if (label) label.text = questId; // по ТЗ показываем ID
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(()=>_onClick?.Invoke(_id));
    }
}