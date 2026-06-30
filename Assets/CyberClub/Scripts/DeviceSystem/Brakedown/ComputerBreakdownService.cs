using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComputerBreakdownService : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private BrokenComputerRepairButton _repairButtonPrefab;
    [SerializeField] private Transform _repairButtonsParent;
    [SerializeField] private Transform _player;
    [SerializeField] private Camera _uiEventCamera;
    [SerializeField] private ComputerBreakdownNotification _notification;

    [Header("Reward")]
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private ResourcesMultiplier _resourcesMultiplier;
    [SerializeField] private RatingData _ratingData;
    [SerializeField] private float _repairRewardMultiplier = 3f;

    [Header("Breakdown timing")]
    [SerializeField] private bool _autoStart = true;
    [SerializeField] private float _startDelay = 45f;
    [SerializeField] private float _minTimeBetweenBreakdowns = 60f;
    [SerializeField] private float _maxTimeBetweenBreakdowns = 120f;

    [Header("Breakdown rules")]
    [SerializeField] private int _maxBrokenDevicesAtSameTime = 1;
    [Range(0, 100)]
    [SerializeField] private int _breakdownChancePercent = 100;

    [Header("Repair button")]
    [SerializeField] private Vector3 _buttonWorldOffset = new Vector3(0f, 2.2f, 0f);

    private readonly Dictionary<GameDevice, BrokenComputerRepairButton> _repairButtons = new();
    private Coroutine _breakdownCoroutine;

    private void Start()
    {
        if (_uiEventCamera == null)
            _uiEventCamera = Camera.main;

        if (_autoStart)
            StartBreakdownLoop();
    }

    private void OnDisable()
    {
        StopBreakdownLoop();
    }

    public void StartBreakdownLoop()
    {
        if (_breakdownCoroutine != null)
            return;

        _breakdownCoroutine = StartCoroutine(BreakdownLoop());
    }

    public void StopBreakdownLoop()
    {
        if (_breakdownCoroutine == null)
            return;

        StopCoroutine(_breakdownCoroutine);
        _breakdownCoroutine = null;
    }

    [ContextMenu("Break Random Device Now")]
    public void BreakRandomDeviceNow()
    {
        TryBreakRandomDevice();
    }

    private IEnumerator BreakdownLoop()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, _startDelay));

        while (true)
        {
            float minTime = Mathf.Min(_minTimeBetweenBreakdowns, _maxTimeBetweenBreakdowns);
            float maxTime = Mathf.Max(_minTimeBetweenBreakdowns, _maxTimeBetweenBreakdowns);

            yield return new WaitForSeconds(Random.Range(minTime, maxTime));

            if (Random.Range(0, 100) >= _breakdownChancePercent)
                continue;

            TryBreakRandomDevice();
        }
    }

    private void TryBreakRandomDevice()
    {
        if (_deviceRegistry == null)
            return;

        if (GetBrokenDevicesCount() >= Mathf.Max(1, _maxBrokenDevicesAtSameTime))
            return;

        List<DeviceEntry> devices = _deviceRegistry.GetBreakableDevices();

        if (devices.Count == 0)
            return;

        DeviceEntry entry = devices[Random.Range(0, devices.Count)];
        GameDevice device = entry.Device;

        if (device == null || !device.BreakDown())
            return;

        CreateRepairButton(entry);

        if (_notification != null)
            _notification.ShowBreakdown(entry.ZoneName);
    }

    private void CreateRepairButton(DeviceEntry entry)
    {
        if (_repairButtonPrefab == null || entry == null || entry.Device == null)
            return;

        Transform parent = _repairButtonsParent != null ? _repairButtonsParent : entry.Device.transform;

        BrokenComputerRepairButton button = Instantiate(_repairButtonPrefab, parent);
        button.transform.position = entry.Device.transform.position + _buttonWorldOffset;

        SetupWorldSpaceCanvas(button.gameObject);

        button.Initialize(
            entry,
            _player,
            _uiEventCamera,
            _coinsData,
            _resourcesMultiplier,
            _ratingData,
            _repairRewardMultiplier
        );

        _repairButtons[entry.Device] = button;
        entry.Device.OnRepaired += RemoveRepairButton;
    }

    private void SetupWorldSpaceCanvas(GameObject repairButtonObject)
    {
        if (_uiEventCamera == null)
            _uiEventCamera = Camera.main;

        Canvas[] canvases = repairButtonObject.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                canvas.worldCamera = _uiEventCamera;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void RemoveRepairButton(GameDevice device)
    {
        if (device == null)
            return;

        device.OnRepaired -= RemoveRepairButton;

        if (!_repairButtons.TryGetValue(device, out BrokenComputerRepairButton button))
            return;

        _repairButtons.Remove(device);

        if (button != null)
            Destroy(button.gameObject);
    }

    private int GetBrokenDevicesCount()
    {
        if (_deviceRegistry == null)
            return 0;

        int count = 0;

        foreach (DeviceEntry entry in _deviceRegistry.Devices)
        {
            if (entry?.Device != null && entry.Device.IsBroken)
                count++;
        }

        return count;
    }
}
