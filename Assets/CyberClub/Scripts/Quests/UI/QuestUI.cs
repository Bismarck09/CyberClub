using System;
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

    private Quest _trackedQuest;

    private QuestData _rewardQuestData;
    private IResource _rewardResourceData;
    private int _questIndex;
    private Action<int> _onRewardClaimed;

    private bool _isRewardAvailable;
    private bool _isClaimed;

    public void Init(QuestData data)
    {
        ClearQuestSubscriptions();

        _rewardQuestData = null;
        _rewardResourceData = null;
        _questIndex = -1;
        _onRewardClaimed = null;
        _isRewardAvailable = false;
        _isClaimed = false;

        SetText(data);
        SetProgress(0f);

        if (_startPanel != null)
            _startPanel.SetActive(true);

        if (_completePanel != null)
        {
            _completePanel.onClick.RemoveListener(ClaimReward);
            _completePanel.onClick.AddListener(ClaimReward);
            _completePanel.interactable = true;
            _completePanel.gameObject.SetActive(false);
        }
    }

    public void ActivateTracking(Quest quest, QuestData data, int questIndex, Action<int> onRewardClaimed)
    {
        Init(data);

        if (_startPanel != null)
            _startPanel.SetActive(false);

        _trackedQuest = quest;
        _rewardQuestData = data;
        _rewardResourceData = quest.ResourceData;
        _questIndex = questIndex;
        _onRewardClaimed = onRewardClaimed;

        _trackedQuest.OnProgressChanged += UpdateUI;

        UpdateUI(_trackedQuest.CurrentProgress, data.TargetValue);
    }

    public void ShowPendingReward(QuestData data, IResource resourceData, int questIndex, Action<int> onRewardClaimed)
    {
        Init(data);

        if (_startPanel != null)
            _startPanel.SetActive(false);

        _rewardQuestData = data;
        _rewardResourceData = resourceData;
        _questIndex = questIndex;
        _onRewardClaimed = onRewardClaimed;

        SetProgress(1f);
        MarkRewardAvailable(false);
    }

    public void MarkRewardAvailable(bool playPulse = true)
    {
        if (_isRewardAvailable || _isClaimed)
            return;

        ClearQuestSubscriptions();

        _isRewardAvailable = true;

        if (_completePanel != null)
        {
            _completePanel.interactable = true;
            _completePanel.gameObject.SetActive(true);
        }

        SetProgress(1f);

        if (playPulse && _questPulseFeedback != null)
            _questPulseFeedback.ActivatePulse();
    }

    public void Deactivate()
    {
        ClearQuestSubscriptions();

        if (_completePanel != null)
        {
            _completePanel.interactable = true;
            _completePanel.gameObject.SetActive(false);
        }

        _rewardQuestData = null;
        _rewardResourceData = null;
        _onRewardClaimed = null;
        _isRewardAvailable = false;
        _isClaimed = false;

        gameObject.SetActive(false);
    }

    private void ClaimReward()
    {
        if (_isClaimed)
            return;

        if (!_isRewardAvailable || _rewardQuestData == null || _rewardResourceData == null)
            return;

        _isClaimed = true;

        if (_completePanel != null)
            _completePanel.interactable = false;

        _rewardResourceData.AddResource(_rewardQuestData.RewardValue, 1f);

        _onRewardClaimed?.Invoke(_questIndex);

        Deactivate();
    }

    private void UpdateUI(int currentProgress, int targetProgress)
    {
        if (targetProgress <= 0)
        {
            SetProgress(0f);
            return;
        }

        SetProgress((float)currentProgress / targetProgress);
    }

    private void SetText(QuestData data)
    {
        if (data == null)
            return;

        if (_description != null)
            _description.text = data.Description;

        if (_reward != null)
            _reward.text = ResourceValueFormatter.Format(data.RewardValue);
    }

    private void SetProgress(float value)
    {
        if (_progressBar != null)
            _progressBar.fillAmount = Mathf.Clamp01(value);
    }

    private void ClearQuestSubscriptions()
    {
        if (_trackedQuest != null)
            _trackedQuest.OnProgressChanged -= UpdateUI;

        _trackedQuest = null;
    }

    private void OnDestroy()
    {
        if (_completePanel != null)
            _completePanel.onClick.RemoveListener(ClaimReward);

        ClearQuestSubscriptions();
    }
}
