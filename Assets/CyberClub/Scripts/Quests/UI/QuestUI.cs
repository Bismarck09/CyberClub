using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUI  : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _reward;
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private Button _completePanel;
    [SerializeField] private Image _progressBar;
    
    private Quest _quest;

    public void Init(QuestData data)
    {
        _description.text = $"Обслужи {data.TargetValue} клиентов";
        _reward.text = data.RewardValue.ToString();
    }

    public void Activate(Quest quest)
    {
        _startPanel.gameObject.SetActive(false);
        
        _quest = quest;
        
        _quest.OnProgressChanged += UpdateUI;
        _quest.OnCompleted += CompleteQuest;
        
        _completePanel.onClick.AddListener(ClaimReward);
    }

    private void CompleteQuest()
    {
        _completePanel.gameObject.SetActive(true);
        
        _quest.OnCompleted -= CompleteQuest;
        _quest.OnProgressChanged -= UpdateUI;
    }

    private void ClaimReward()
    {
        _quest.ResourceData.AddResource(_quest.GetData().RewardValue, 1);
        
        Destroy(gameObject);
    }
    
    private void UpdateUI(int currentProgress, int targetProgress)
    {
        _progressBar.fillAmount = (float)currentProgress / targetProgress;
    }
}
