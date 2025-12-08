using UnityEngine;

public class VisitorListUI : MonoBehaviour
{
    [SerializeField] Transform content;            // ScrollView/Viewport/Content
    [SerializeField] GameObject visitorItemPrefab; // префаб с VisitorItemView

    public void Rebuild()
    {
        foreach (Transform c in content) Destroy(c.gameObject);

        var list = VisitorService.GetAvailableVisitors();
        foreach (var v in list)
        {
            var go = Instantiate(visitorItemPrefab, content);

            var view = go.GetComponent<VisitorItemView>();
            view.Setup(v.id, v.displayName, OnVisitorClicked);

            // вот это единственное добавление:
            //view.SetDialogues(v.dialogueTitles);
        }
    }

    void OnVisitorClicked(string id)
    {
        Debug.Log("Visitor clicked: " + id);
    }
}