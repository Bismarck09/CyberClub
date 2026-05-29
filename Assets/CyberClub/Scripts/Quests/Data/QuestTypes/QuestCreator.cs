using System.Collections.Generic;
using UnityEngine;

public class QuestCreator : MonoBehaviour
{
    [SerializeField] private List<QuestData> _quests;
    [SerializeField] private VisitorService _visitorService;
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private GameObject _questPrefab;
    [SerializeField] private Transform _content;

    private readonly List<Quest> _allQuestsTypes = new();
    private List<GameObject> _allQuestsPrefabs = new();

    private int _currentQuestIndex;

    private void Start()
    {
        _allQuestsTypes.Add(new VisitorServiceQuest(_visitorService, _coinsData));

        CreateQuests();
        StartQuest();
    }

    private void CreateQuests()
    {
        foreach (QuestData quest in _quests)
        {
            GameObject go = Instantiate(_questPrefab, _content);

            if (go.TryGetComponent(out QuestUI questUI))
            {
                questUI.Init(quest);
            }

            go.SetActive(false);
            _allQuestsPrefabs.Add(go);
        }
    }

    private void StartQuest()
    {
        if (_currentQuestIndex >= _quests.Count)
        {
            Debug.Log("Все квесты выполнены");
            return;
        }

        QuestData questData = _quests[_currentQuestIndex];
        Quest quest = GetQuestByType(questData.Type);

        if (quest == null)
        {
            Debug.LogError($"Не найден обработчик для квеста типа: {questData.Type}");
            return;
        }

        if (_currentQuestIndex >= _allQuestsPrefabs.Count)
        {
            Debug.LogError("Для квеста не найден UI-префаб");
            return;
        }

        quest.OnCompleted -= StartQuest;
        quest.OnCompleted += StartQuest;

        quest.Activate(questData);

        GameObject questObject = _allQuestsPrefabs[_currentQuestIndex];
        questObject.SetActive(true);

        if (questObject.TryGetComponent(out QuestUI questUI))
        {
            questUI.Activate(quest);
        }

        _currentQuestIndex++;
    }

    private Quest GetQuestByType(QuestType type)
    {
        for (int i = 0; i < _allQuestsTypes.Count; i++)
        {
            if (_allQuestsTypes[i].Type == type)
                return _allQuestsTypes[i];
        }

        return null;
    }
}