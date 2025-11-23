using UnityEngine;

public class QuestListUI : MonoBehaviour
{
    [SerializeField] private Transform content;        // ScrollView/Viewport/Content
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private QuestDetailsUI details;

    private void Start()
    {
        GameRepository.InitOrLoad(); // активный слот уже выбран в меню
        ReBuildList();
        
    }

    public void ReBuildList()
    {
        foreach (Transform child in content) Destroy(child.gameObject);
        foreach (var q in QuestService.GetUnlockedQuestStates())
        {
            var go = Instantiate(questItemPrefab, content);
            go.GetComponent<QuestItemView>().Setup(q.id, OnQuestClicked);
        }
    }

    private void OnQuestClicked(string id) => details.Bind(id);
}