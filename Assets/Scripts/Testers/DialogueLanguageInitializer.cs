using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueLanguageInitializer : MonoBehaviour
{
    [SerializeField] DialogueSystemController dialogueSystemController;
    [SerializeField] string playerPrefsKey = "Language"; 
    // тот же ключ, который стоит в UILocalizationManager → Current Language Player Prefs Key

    void Start()
    {
        if (dialogueSystemController == null)
        {
            Debug.LogWarning("[DialogueLanguageInitializer] DialogueSystemController is not assigned.");
            return;
        }

        // 1. читаем язык, который меню уже сохранило через UILocalizationManager
        //    если по какой-то причине ключа нет (первая сессия), считаем что "Default"
        string langCode = PlayerPrefs.GetString(playerPrefsKey, "Default");

        Debug.Log("[DialogueLanguageInitializer] Applying language to Dialogue System: " + langCode);

        // 2. сказать диалоговой системе использовать этот язык
        dialogueSystemController.displaySettings.localizationSettings.language = langCode;
    }
}