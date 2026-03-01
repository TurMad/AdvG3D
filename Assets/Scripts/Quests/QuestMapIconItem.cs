using UnityEngine;

public class QuestMapIconItem : MonoBehaviour
{
    [SerializeField] private string questId;

    public string QuestId => questId;

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    private void OnMouseDown()
    {
        OnClick();
    }

    // повесь на кнопку/коллайдер/ивент
    public void OnClick()
    {
        if (QuestSendUIController.Instance != null)
            QuestSendUIController.Instance.OpenMapQuestWindow(questId);
        Debug.Log(questId);
    }
}