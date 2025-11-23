using UnityEngine;

public static class SaveSlots
{
    public const int Count = 4;             
    public static int Active { get; private set; } = 1; 

    public static void SetActive(int i) => Active = Mathf.Clamp(i, 1, Count);
    public static string File(int i) => $"slot{i}/save.es3";
    public const string Key = "GameData";
}