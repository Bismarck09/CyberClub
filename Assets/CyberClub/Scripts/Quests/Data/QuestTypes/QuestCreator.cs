using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class QuestCreator : MonoBehaviour
{
    [SerializeField] private List<QuestData> _quests;
    [SerializeField] private VisitorService _visitorService;
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private GameObject _questPrefab;
    [SerializeField] private Transform _content;
    
    private List<Quest> _allQuestsTypes = new List<Quest>();
    private List<GameObject> _allQuestsPrefabs = new List<GameObject>();
    private VisitorServiceQuest _visitorServiceQuest;

    private int _currentQuestIndex;


    private void Start()
    {
        _visitorServiceQuest = new VisitorServiceQuest(_visitorService, _coinsData);
        
        _allQuestsTypes.Add(_visitorServiceQuest);
        
        CreateQuests();
        StartQuest();
    }

    private void CreateQuests()
    {
        foreach (var quest in _quests)
        {
            GameObject go = Instantiate(_questPrefab, _content);
            go.GetComponent<QuestUI>().Init(quest);
            
            _allQuestsPrefabs.Add(go);
            
        }
    }

    private void StartQuest()
    {
        if (_currentQuestIndex >= _quests.Count)
            return;
        
        QuestData quest = _quests[_currentQuestIndex];
        
        for (int i = 0; i < _allQuestsTypes.Capacity; i++)
        {
            if (_allQuestsTypes[i].Type == quest.Type)
            {
                _allQuestsTypes[i].Activate(quest);
                _allQuestsPrefabs[_currentQuestIndex].GetComponent<QuestUI>().Activate(_allQuestsTypes[i]);
                
                _currentQuestIndex++;
                _allQuestsTypes[i].OnCompleted += StartQuest;
                break;
            }
        }
    }
}
