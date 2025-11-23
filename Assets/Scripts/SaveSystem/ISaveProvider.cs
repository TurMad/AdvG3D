public interface ISaveProvider
{
    void SaveGameData(GameData data, int slot);
    GameData LoadGameData(int slot);
    bool HasSave(int slot);
    void DeleteSlot(int slot);
}