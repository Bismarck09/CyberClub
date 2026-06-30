using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrokenComputerRepairButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _fillImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Repair")]
    [SerializeField] private float _repairDistance = 2.5f;
    [SerializeField] private float _repairHoldSeconds = 2f;

    [Header("Optional reward text")]
    [SerializeField] private TMP_Text _rewardText;

    private DeviceEntry _deviceEntry;
    private GameDevice _device;
    private Transform _player;
    private Camera _camera;

    private CoinsData _coinsData;
    private ResourcesMultiplier _resourcesMultiplier;
    private RatingData _ratingData;
    private float _repairRewardMultiplier = 3f;

    private bool _isHolding;
    private bool _isCompleted;
    private float _holdTime;

    public void Initialize(
        DeviceEntry deviceEntry,
        Transform player,
        Camera camera,
        CoinsData coinsData,
        ResourcesMultiplier resourcesMultiplier,
        RatingData ratingData,
        float repairRewardMultiplier)
    {
        _deviceEntry = deviceEntry;
        _device = deviceEntry != null ? deviceEntry.Device : null;
        _player = player;
        _camera = camera != null ? camera : Camera.main;

        _coinsData = coinsData;
        _resourcesMultiplier = resourcesMultiplier;
        _ratingData = ratingData;
        _repairRewardMultiplier = Mathf.Max(1f, repairRewardMultiplier);

        Prepare();
        ResetHold();
        RefreshRewardText();
    }

    private void Awake()
    {
        Prepare();
    }

    private void OnDestroy()
    {
        RemovePointerEvents();
    }

    private void Update()
    {
        if (_device == null || !_device.IsBroken)
        {
            Destroy(gameObject);
            return;
        }

        LookAtCamera();
        RefreshInteractable();

        if (!_isHolding)
            return;

        if (!IsPlayerNear())
        {
            ResetHold();
            return;
        }

        _holdTime += Time.deltaTime;

        float progress = Mathf.Clamp01(_holdTime / Mathf.Max(0.1f, _repairHoldSeconds));

        if (_fillImage != null)
            _fillImage.fillAmount = 1f - progress;

        if (progress >= 1f)
            CompleteRepair();
    }

    private void Prepare()
    {
        if (_button == null)
            _button = GetComponentInChildren<Button>(true);

        if (_fillImage == null)
            _fillImage = GetComponentInChildren<Image>(true);

        if (_canvasGroup == null)
            _canvasGroup = GetComponentInChildren<CanvasGroup>(true);

        PrepareFill();
        AddPointerEvents();
    }

    private void PrepareFill()
    {
        if (_fillImage == null)
            return;

        _fillImage.raycastTarget = true;
        _fillImage.type = Image.Type.Filled;
        _fillImage.fillMethod = Image.FillMethod.Radial360;
        _fillImage.fillOrigin = (int)Image.Origin360.Top;
        _fillImage.fillClockwise = false;
        _fillImage.fillAmount = 1f;
    }

    private void AddPointerEvents()
    {
        if (_button == null)
            return;

        EventTrigger trigger = _button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = _button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();
        trigger.triggers.Clear();

        AddTrigger(trigger, EventTriggerType.PointerDown, StartHold);
        AddTrigger(trigger, EventTriggerType.PointerUp, StopHold);
        AddTrigger(trigger, EventTriggerType.PointerExit, StopHold);
    }

    private void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    private void RemovePointerEvents()
    {
        if (_button == null)
            return;

        EventTrigger trigger = _button.GetComponent<EventTrigger>();

        if (trigger != null)
            trigger.triggers.Clear();
    }

    private void StartHold(BaseEventData eventData)
    {
        if (_isCompleted || _device == null || !_device.IsBroken)
            return;

        if (!IsPlayerNear())
            return;

        _isHolding = true;
    }

    private void StopHold(BaseEventData eventData)
    {
        ResetHold();
    }

    private void CompleteRepair()
    {
        if (_isCompleted)
            return;

        _isCompleted = true;
        _isHolding = false;

        GiveRepairReward();

        if (_device != null)
            _device.Repair();

        Destroy(gameObject);
    }

    private void GiveRepairReward()
    {
        if (_coinsData == null || _deviceEntry == null)
            return;

        int reward = CalculateRepairReward();

        if (reward <= 0)
            return;

        _coinsData.AddResource(reward, 1f);
    }

    private int CalculateRepairReward()
    {
        if (_deviceEntry == null)
            return 0;

        float globalCoinsMultiplier = 1f;

        if (_resourcesMultiplier != null)
            globalCoinsMultiplier = _resourcesMultiplier.GetMultiplier(ResourceType.Coins);

        float roomCoinsMultiplier = _deviceEntry.RoomCoinsMultiplier;
        float ratingMultiplier = _ratingData != null ? _ratingData.IncomeMultiplier : 1f;

        float finalMultiplier = (globalCoinsMultiplier + roomCoinsMultiplier) * ratingMultiplier * _repairRewardMultiplier;

        return Mathf.RoundToInt(_deviceEntry.PriceOfHourCoins * finalMultiplier);
    }

    private void RefreshRewardText()
    {
        if (_rewardText == null)
            return;

        int reward = CalculateRepairReward();
        _rewardText.text = $"+{ResourceValueFormatter.Format(reward)}";
    }

    private void ResetHold()
    {
        _isHolding = false;
        _holdTime = 0f;

        if (_fillImage != null)
            _fillImage.fillAmount = 1f;
    }

    private void RefreshInteractable()
    {
        bool canRepair = IsPlayerNear();

        if (_button != null)
            _button.interactable = canRepair;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = canRepair ? 1f : 0.45f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    private bool IsPlayerNear()
    {
        if (_player == null || _device == null)
            return true;

        Vector3 playerPosition = _player.position;
        Vector3 devicePosition = _device.transform.position;

        playerPosition.y = 0f;
        devicePosition.y = 0f;

        return Vector3.Distance(playerPosition, devicePosition) <= _repairDistance;
    }

    private void LookAtCamera()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
            return;

        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
    }
}
