public interface ISaveModule
{
    void Capture(GameSaveData saveData);
    void Restore(GameSaveData saveData);
}