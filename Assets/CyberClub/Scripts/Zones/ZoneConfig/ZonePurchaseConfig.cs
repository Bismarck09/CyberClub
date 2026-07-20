using UnityEngine;
using UnityEngine.Serialization;

public class ZonePurchaseConfig : MonoBehaviour
{
    [SerializeField] private GameObject _barrierObject;
    [SerializeField] private int _zonePrice;

    [Header("Authored purchase content")]
    [SerializeField] private ZoneInformation _zoneInformation;
    [SerializeField, TextArea(2, 5)] private string _description;
    [FormerlySerializedAs("_advantages")]
    [SerializeField, TextArea(2, 5)] private string _gameplayBenefit;
    [SerializeField, TextArea(2, 4)] private string _progressionHint;
    [FormerlySerializedAs("_icon")]
    [SerializeField] private Sprite _previewSprite;

    private bool _isInitialized;
    private bool _isUnlocked;
    private bool _isPurchaseInProgress;

    public GameObject BarrierObject => _barrierObject;
    public int ZonePrice => _zonePrice;
    public ZoneInformation ZoneInformation
    {
        get
        {
            if (_zoneInformation != null)
                return _zoneInformation;

            BarrierData barrierData = GetComponent<BarrierData>();
            return barrierData != null ? barrierData.ZoneInformation : null;
        }
    }

    public string DisplayName => ZoneInformation != null && !string.IsNullOrWhiteSpace(ZoneInformation.ZoneName)
        ? ZoneInformation.ZoneName
        : name;
    public string Description => _description;
    public string GameplayBenefit => _gameplayBenefit;
    public string ProgressionHint => _progressionHint;
    public Sprite PreviewSprite => _previewSprite;
    public int ComputerCapacity => ZoneInformation != null && ZoneInformation.SpawnPoints != null
        ? ZoneInformation.SpawnPoints.AvailableSpawnPointCount
        : 0;

    public bool IsUnlocked
    {
        get
        {
            EnsureRuntimeState();
            return _isUnlocked;
        }
    }

    public bool TryBeginPurchase()
    {
        EnsureRuntimeState();

        if (_isUnlocked || _isPurchaseInProgress)
            return false;

        _isPurchaseInProgress = true;
        return true;
    }

    public void CommitUnlockedState()
    {
        _isInitialized = true;
        _isUnlocked = true;
        _isPurchaseInProgress = false;
    }

    public void CancelPurchase()
    {
        _isPurchaseInProgress = false;
    }

    public void RestoreUnlockedState(bool isUnlocked)
    {
        _isInitialized = true;
        _isUnlocked = isUnlocked;
        _isPurchaseInProgress = false;

        if (_barrierObject != null)
            _barrierObject.SetActive(!isUnlocked);
    }

    private void EnsureRuntimeState()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        _isUnlocked = _barrierObject == null || !_barrierObject.activeSelf;
    }
}
