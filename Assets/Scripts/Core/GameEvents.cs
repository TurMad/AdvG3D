using System;

public static class GameEvents
{
    public static event Action<string> OnQuestCompleted;     
    public static event Action<string> OnBuildingConstructed; 
    public static event Action<string> OnItemPurchased;    

    public static void RaiseQuestCompleted(string questId)      => OnQuestCompleted?.Invoke(questId);
    public static void RaiseBuildingConstructed(string id)      => OnBuildingConstructed?.Invoke(id);
    public static void RaiseItemPurchased(string id)            => OnItemPurchased?.Invoke(id);
}