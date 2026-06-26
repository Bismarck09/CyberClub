using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private UIQuestPulseFeedback _questPulseFeedback;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _reward;
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private Button _completePanel;
    [SerializeField] private Image _progressBar;

    private Quest _quest;

    public void Init(QuestData data)
    {
        _description.text = data.GetDescription();
        _reward.text = data.RewardValue.ToString();

        _completePanel.gameObject.SetActive(false);

        if (_progressBar != null)
            _progressBar.fillAmount = 0f;
    }

    public void Activate(Quest quest)
    {
        _startPanel.SetActive(false);

        _quest = quest;

        _quest.OnProgressChanged += UpdateUI;
        _quest.OnCompleted += CompleteQuest;
        _quest.OnCompleted += _questPulseFeedback.ActivatePulse;

        _completePanel.onClick.RemoveListener(ClaimReward);
        _completePanel.onClick.AddListener(ClaimReward);
    }

    private void CompleteQuest()
    {
        _completePanel.gameObject.SetActive(true);

        _quest.OnCompleted -= CompleteQuest;
        _quest.OnCompleted -= _questPulseFeedback.ActivatePulse;
        _quest.OnProgressChanged -= UpdateUI;
    }

    private void ClaimReward()
    {
        if (_quest == null)
            return;

        _quest.ResourceData.AddResource(_quest.GetData().RewardValue, 1);

        _completePanel.onClick.RemoveListener(ClaimReward);
        _questPulseFeedback.StopPulse();
        Destroy(gameObject);
    }

    private void UpdateUI(int currentProgress, int targetProgress)
    {
        if (_progressBar == null || targetProgress <= 0)
            return;

        _progressBar.fillAmount = (float)currentProgress / targetProgress;
    }

    private void OnDestroy()
    {
        if (_quest == null)
            return;

        _quest.OnCompleted -= CompleteQuest;
        _quest.OnProgressChanged -= UpdateUI;
        _quest.OnCompleted -= _questPulseFeedback.ActivatePulse;
        
        _completePanel.onClick.RemoveListener(ClaimReward);
        _questPulseFeedback.StopPulse();
    }
}