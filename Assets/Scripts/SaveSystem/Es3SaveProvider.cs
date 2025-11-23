public class Es3SaveProvider : ISaveProvider
{
    public void SaveGameData(GameData data, int slot)
    {
        ES3.Save(SaveSlots.Key, data, SaveSlots.File(slot));
    }

    public GameData LoadGameData(int slot)
        => ES3.Load<GameData>(SaveSlots.Key, SaveSlots.File(slot));
    
    public bool HasSave(int slot) => ES3.KeyExists(SaveSlots.Key, SaveSlots.File(slot));
    
    public void DeleteSlot(int slot)
    {
        if (ES3.FileExists(SaveSlots.File(slot)))
            ES3.DeleteFile(SaveSlots.File(slot));
    }
}