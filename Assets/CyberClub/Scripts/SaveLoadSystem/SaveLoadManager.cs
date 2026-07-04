using System.Collections.Generic;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    [Header("Modules")]
    [SerializeField] private List<MonoBehaviour> _moduleBehaviours = new();

    [Header("Auto")]
    [SerializeField] private bool _loadOnStart = true;
    [SerializeField] private bool _autoSave = true;
    [SerializeField] private float _autoSaveInterval = 15f;

    private readonly List<ISaveModule> _modules = new();
    private float _timer;
    private bool _isLoaded;

    private void Awake() => BuildModulesList();

    private void Start()
    {
        if (_loadOnStart)
            LoadGame();
    }

    private void Update()
    {
        if (!_autoSave || !_isLoaded)
            return;

        _timer += Time.unscaledDeltaTime;

        if (_timer < _autoSaveInterval)
            return;

        _timer = 0f;
        SaveGame();
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        foreach (ISaveModule module in _modules)
            module?.Capture(data);

        GameSaveRepository.Save(data);
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        GameSaveData data = GameSaveRepository.Load();

        foreach (ISaveModule module in _modules)
            module?.Restore(data);

        _isLoaded = true;
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        GameSaveRepository.Delete();
        Debug.Log("SaveLoadManager: сохранение удалено.");
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && _isLoaded)
            SaveGame();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && _isLoaded)
            SaveGame();
    }

    private void OnApplicationQuit()
    {
        if (_isLoaded)
            SaveGame();
    }

    private void BuildModulesList()
    {
        _modules.Clear();

        foreach (MonoBehaviour behaviour in _moduleBehaviours)
        {
            if (behaviour == null)
                continue;

            if (behaviour is ISaveModule module)
                _modules.Add(module);
            else
                Debug.LogError($"{behaviour.name} не реализует ISaveModule.");
        }
    }
}
