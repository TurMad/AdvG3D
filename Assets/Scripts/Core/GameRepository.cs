using UnityEngine;

public static class GameRepository
{
    private static ISaveProvider _provider = new Es3SaveProvider();
    public static GameData Data { get; private set; }

    public static void SelectSlot(int index) => SaveSlots.SetActive(index);
    public static bool SlotHasSave(int index) => _provider.HasSave(index);

    public static void NewOrContinue(int slot)
    {
        SelectSlot(slot);
        Data = SlotHasSave(slot) ? _provider.LoadGameData(slot) : LoadDefaultsFromPreset();
        QuestService.SyncWithRegistry(Data);    
        VisitorService.SyncWithRegistry(Data);
        AdventurerService.SyncWithRegistry(Data); 
        if (!SlotHasSave(slot)) _provider.SaveGameData(Data, slot);
    }
    
    public static void DeleteSlot(int index)
    {
        _provider.DeleteSlot(index);
        if (SaveSlots.Active == index) Data = null; 
    }

    public static void InitOrLoad()
    {
        int s = SaveSlots.Active;
        Data = SlotHasSave(s) ? _provider.LoadGameData(s) : LoadDefaultsFromPreset();
        QuestService.SyncWithRegistry(Data);  
        VisitorService.SyncWithRegistry(Data);
    }

    public static void Save()
    {
        _provider.SaveGameData(Data, SaveSlots.Active);
    }

    public static string GetSlotLabel(int slot)
    {
        if (!SlotHasSave(slot)) return "Слот пустой";
        var d = _provider.LoadGameData(slot);
        return $"Гильдия {d.guildLevel} | Золото {d.gold}";
    }

    private static GameData LoadDefaultsFromPreset()
    {
        var preset = Resources.Load<GameDataPreset>("DefaultGameData");
        var d = new GameData();
        if (preset != null)
        {
            d.gold        = preset.gold;
            d.reputation  = preset.reputation;
            d.guildLevel  = preset.guildLevel;
            d.guildExp    = preset.guildExp;
            d.quests      = new(preset.quests);
            d.adventurers = new(preset.adventurers);
        }
        return d;
    }
}