using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReceptionUIController : MonoBehaviour
{
    [SerializeField] private Image currentVisitorImage;

    public void OnClick_ShowCurrentVisitor()
    {
        var vm = VisitorManager.Instance;
        if (vm == null) return;

        var state = vm.GetFirstTodayVisitor();
        if (state == null)
        {
            Debug.Log("[ReceptionUI] No visitors today.");
            return;
        }

        var def = VisitorService.GetDefinition(state.id);
        if (def == null)
        {
            Debug.LogWarning("[ReceptionUI] No definition for visitor " + state.id);
            return;
        }

        currentVisitorImage.sprite = def.portrait;
    }
}