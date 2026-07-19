using UnityEngine;

public class CyberClubTutorialManager : MonoBehaviour
{
    private enum TutorialStep
    {
        Welcome,
        WaitFirstRoom,
        FirstRoomMessage,
        WaitFirstDevicePurchase,
        FirstDeviceMessage,
        InteriorMessage,
        Completed,
        WaitFirstIncome,
        FirstIncomeMessage
    }

    [Header("UI")]
    [SerializeField] private TutorialPanel _panel;
    [SerializeField] private TutorialObjectiveHint _objectiveHint;
    [SerializeField] private TutorialArrowPointer _arrow;

    [Header("Targets")]
    [SerializeField] private Transform _firstRoomTarget;
    [SerializeField] private Transform _buyDeviceButtonTarget;
    [SerializeField] private Transform _ratingTarget;

    [Header("Game events")]
    [SerializeField] private ZoneSwitcher _zoneSwitcher;
    [SerializeField] private ZoneInformation _firstRoomZone;
    [SerializeField] private string _firstRoomNamePart = "Красная";
    [SerializeField] private DevicePurchase _devicePurchase;
    [SerializeField] private RatingData _ratingData;
    [SerializeField] private VisitorService _visitorService;
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private SaveLoadManager _saveLoadManager;

    [Header("Control")]
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private TutorialInputBlocker _inputBlocker;

    [Header("Settings")]
    [SerializeField] private bool _startOnStart = true;

    private TutorialStep _step;
    private bool _breakdownTutorialShown;
    private bool _ratingTutorialShown;
    private bool _hasFirstVisitorIncome;
    private bool _firstComputerCompensationGranted;
    private bool _wasRestored;

    public int StepIndex => (int)_step;
    public bool BreakdownTutorialShown => _breakdownTutorialShown;
    public bool RatingTutorialShown => _ratingTutorialShown;
    public bool CanHireAdditionalAdmins =>
        _hasFirstVisitorIncome || _step == TutorialStep.Completed;

    public event System.Action OnTutorialStateChanged;

    private void Awake()
    {
        HideAllTutorialUI();
    }

    private void OnEnable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged += OnZoneChanged;

        if (_devicePurchase != null)
            _devicePurchase.OnDevicePurchased += OnDevicePurchased;

        if (_ratingData != null)
            _ratingData.OnRatingChanged += OnRatingChanged;

        if (_visitorService != null)
            _visitorService.OnVisitorServiced += OnFirstVisitorServiced;
    }

    private void OnDisable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged -= OnZoneChanged;

        if (_devicePurchase != null)
            _devicePurchase.OnDevicePurchased -= OnDevicePurchased;

        if (_ratingData != null)
            _ratingData.OnRatingChanged -= OnRatingChanged;

        if (_visitorService != null)
            _visitorService.OnVisitorServiced -= OnFirstVisitorServiced;
    }

    private void Start()
    {
        if (_wasRestored)
        {
            ReconcileFirstDeviceProgress();
            return;
        }

        if (_startOnStart)
            StartTutorial();
    }

    public TutorialSaveData CaptureSave()
    {
        return new TutorialSaveData
        {
            HasTutorialSave = true,
            Step = (int)_step,
            BreakdownTutorialShown = _breakdownTutorialShown,
            RatingTutorialShown = _ratingTutorialShown,
            HasFirstVisitorIncome = _hasFirstVisitorIncome,
            FirstComputerCompensationGranted = _firstComputerCompensationGranted
        };
    }

    public void RestoreSave(TutorialSaveData data)
    {
        if (data == null || data.HasTutorialSave == false)
            return;

        _step = (TutorialStep)Mathf.Clamp(data.Step, 0, (int)TutorialStep.FirstIncomeMessage);
        _breakdownTutorialShown = data.BreakdownTutorialShown;
        _ratingTutorialShown = data.RatingTutorialShown;
        _hasFirstVisitorIncome = data.HasFirstVisitorIncome || _step == TutorialStep.Completed;
        _firstComputerCompensationGranted = data.FirstComputerCompensationGranted;
        _wasRestored = true;

        TryRepairBrokenFirstComputerSave();

        HideAllTutorialUI();

        switch (_step)
        {
            case TutorialStep.Completed:
                EnableGameplay(false);
                break;

            case TutorialStep.WaitFirstRoom:
                EnableGameplay(false);
                ShowObjective("Иди к красной комнате. Если она закрыта — купи её.");
                PointArrowTo(_firstRoomTarget);
                break;

            case TutorialStep.FirstRoomMessage:
                _step = TutorialStep.WaitFirstRoom;
                EnterFirstRoom();
                break;

            case TutorialStep.WaitFirstDevicePurchase:
                if (HasFirstComputer())
                {
                    CompleteFirstDeviceRequirement();
                    break;
                }

                HidePanel();
                ShowObjective("Купи первый компьютер в панели улучшений.");
                PointArrowTo(_buyDeviceButtonTarget);
                BlockGameplay();
                ForceCursor(true);
                SetSpaceSwitchAllowed(false);
                break;

            case TutorialStep.FirstDeviceMessage:
                ShowFirstDevicePurchasedMessage();
                break;

            case TutorialStep.WaitFirstIncome:
                BeginWaitFirstIncome();
                break;

            case TutorialStep.FirstIncomeMessage:
                ShowFirstIncomeMessage();
                break;

            case TutorialStep.InteriorMessage:
                ShowInteriorMessage();
                break;

            default:
                StartTutorial();
                break;
        }
    }

    public void StartTutorial()
    {
        _step = TutorialStep.Welcome;

        BlockGameplay();
        ForceCursor(true);
        SetSpaceSwitchAllowed(false);

        if (_panel != null)
        {
            _panel.ShowWindow(
                "Добро пожаловать в Кибер Клуб",
                "Ты управляешь компьютерным клубом.\n\nПокупай комнаты и компьютеры, обслуживай клиентов, улучшай интерьер и следи за рейтингом.\n\n<b>Чтобы нажимать кнопки:</b> на ПК нажми <b>Пробел</b>, а на телефоне — кнопку <b>Меню</b>. Затем нажми <b>Далее</b>.\n\nВ режиме интерфейса можно нажимать и прокручивать панели; в игровом режиме — ходить и вращать камеру.",
                "Далее",
                BeginGoToFirstRoom
            );
        }
    }

    public void EnterFirstRoom()
    {
        if (_step != TutorialStep.WaitFirstRoom)
            return;

        _step = TutorialStep.FirstRoomMessage;
        SaveProgress();

        if (HasFirstComputer())
        {
            CompleteFirstDeviceRequirement();
            return;
        }

        HideObjective();
        PointArrowTo(_buyDeviceButtonTarget);

        ShowBlockingWindow(
            "Купи первый компьютер",
            "Ты в первой комнате. Теперь нужно купить первый компьютер в панели улучшений.\n\nКогда закроешь это окно, курсор останется активным. Нажми кнопку покупки компьютера.",
            "Понял",
            BeginWaitFirstDevicePurchase
        );
    }

    public void ShowBreakdownTutorial()
    {
        if (_breakdownTutorialShown || _step != TutorialStep.Completed)
            return;

        _breakdownTutorialShown = true;
        SaveProgress();

        ShowBlockingWindow(
            "Поломка компьютера",
            "Иногда компьютеры ломаются. Сломанный компьютер не принимает клиентов.\n\nПодойди к нему, включи интерфейс через <b>Пробел</b> на ПК или <b>Меню</b> на телефоне и зажми иконку поломки. За ремонт ты получишь бонусные монеты.",
            "Понял",
            ClosePopupTutorialAndReturnToGame
        );
    }

    private void BeginGoToFirstRoom()
    {
        _step = TutorialStep.WaitFirstRoom;
        SaveProgress();

        HidePanel();
        EnableGameplay(false);

        ShowObjective("Иди к красной комнате. Если она закрыта — купи её.");
        PointArrowTo(_firstRoomTarget);
    }

    private void BeginWaitFirstDevicePurchase()
    {
        if (_step != TutorialStep.FirstRoomMessage &&
            _step != TutorialStep.WaitFirstDevicePurchase)
        {
            return;
        }

        if (HasFirstComputer())
        {
            CompleteFirstDeviceRequirement();
            return;
        }

        _step = TutorialStep.WaitFirstDevicePurchase;
        SaveProgress();

        HidePanel();
        ShowObjective("Купи первый компьютер в панели улучшений.");
        PointArrowTo(_buyDeviceButtonTarget);

        BlockGameplay();
        ForceCursor(true);
        SetSpaceSwitchAllowed(false);
    }

    private void OnDevicePurchased()
    {
        if (_step != TutorialStep.FirstRoomMessage &&
            _step != TutorialStep.WaitFirstDevicePurchase)
        {
            return;
        }

        CompleteFirstDeviceRequirement();
    }

    private void CompleteFirstDeviceRequirement()
    {
        if (_step == TutorialStep.FirstDeviceMessage ||
            _step == TutorialStep.WaitFirstIncome ||
            _step == TutorialStep.FirstIncomeMessage ||
            _step == TutorialStep.InteriorMessage ||
            _step == TutorialStep.Completed)
        {
            return;
        }

        if (!HasFirstComputer())
            return;

        _step = TutorialStep.FirstDeviceMessage;
        SaveProgress();
        ShowFirstDevicePurchasedMessage();
    }

    private bool HasFirstComputer()
    {
        return _firstRoomZone != null && _firstRoomZone.CurrentDevicePurchases > 0;
    }

    private void ReconcileFirstDeviceProgress()
    {
        if ((_step == TutorialStep.FirstRoomMessage ||
             _step == TutorialStep.WaitFirstDevicePurchase) &&
            HasFirstComputer())
        {
            CompleteFirstDeviceRequirement();
        }
    }

    private void ShowFirstDevicePurchasedMessage()
    {

        HideObjective();
        HideArrow();

        ShowBlockingWindow(
            "Первый компьютер куплен",
            "Теперь клиенты смогут садиться за компьютер и приносить монеты.\n\nЧем больше компьютеров, тем выше доход, но если клиентов станет слишком много, админы могут не успевать.",
            "Далее",
            BeginWaitFirstIncome
        );
    }

    private void BeginWaitFirstIncome()
    {
        _step = TutorialStep.WaitFirstIncome;
        SaveProgress();

        HidePanel();
        HideArrow();
        ShowObjective("Дождись первого посетителя и первого дохода.");
        EnableGameplay(false);
    }

    private void OnFirstVisitorServiced(DeviceEntry unusedDevice)
    {
        if (_step != TutorialStep.WaitFirstIncome)
            return;

        _hasFirstVisitorIncome = true;
        _step = TutorialStep.FirstIncomeMessage;
        SaveProgress();
        ShowFirstIncomeMessage();
    }

    private void ShowFirstIncomeMessage()
    {
        HideObjective();

        ShowBlockingWindow(
            "Первый доход получен",
            "Первый посетитель обслужен, и клуб начал приносить доход. Теперь можно развивать интерьер и нанимать дополнительных администраторов.",
            "Далее",
            ShowInteriorMessage);
    }

    private void ShowInteriorMessage()
    {
        _step = TutorialStep.InteriorMessage;
        SaveProgress();

        ShowBlockingWindow(
            "Интерьер",
            "Интерьер повышает множитель комнаты. Это значит, что клиенты в этой комнате будут приносить больше монет.\n\nИногда интерьер выгоднее, чем просто ждать деньги на следующий компьютер.",
            "Понял",
            CompleteBasicTutorial
        );
    }

    private void CompleteBasicTutorial()
    {
        _step = TutorialStep.Completed;
        _hasFirstVisitorIncome = true;
        SaveProgress();
        ClosePopupTutorialAndReturnToGame();
    }

    private void ClosePopupTutorialAndReturnToGame()
    {
        HideAllTutorialUI();
        EnableGameplay(false);
    }

    private void OnZoneChanged(ZoneInformation zone)
    {
        if (!IsFirstRoom(zone))
            return;

        EnterFirstRoom();
    }

    private bool IsFirstRoom(ZoneInformation zone)
    {
        if (zone == null)
            return false;

        if (_firstRoomZone != null && zone == _firstRoomZone)
            return true;

        if (string.IsNullOrWhiteSpace(_firstRoomNamePart))
            return false;

        if (!string.IsNullOrWhiteSpace(zone.ZoneName) && zone.ZoneName.Contains(_firstRoomNamePart))
            return true;

        return zone.name.Contains(_firstRoomNamePart);
    }

    private void OnRatingChanged(float amount)
    {
        if (amount >= 0f)
            return;

        if (_ratingTutorialShown || _step != TutorialStep.Completed)
            return;

        _ratingTutorialShown = true;
        SaveProgress();

        PointArrowTo(_ratingTarget);

        ShowBlockingWindow(
            "Рейтинг и админы",
            "Рейтинг начал падать — значит клиенты слишком долго ждут в очереди.\n\nКупи нового админа или прокачай текущих, чтобы они быстрее отправляли клиентов за компьютеры.",
            "Понял",
            ClosePopupTutorialAndReturnToGame
        );
    }

    private void ShowBlockingWindow(string title, string body, string button, System.Action onClick)
    {
        BlockGameplay();
        ForceCursor(true);
        SetSpaceSwitchAllowed(false);

        if (_panel != null)
        {
            _panel.ShowWindow(title, body, button, () =>
            {
                onClick?.Invoke();
            });
        }
    }

    private void BlockGameplay()
    {
        if (_inputBlocker != null)
            _inputBlocker.SetBlocked(true);
    }

    private void UnblockGameplay()
    {
        if (_inputBlocker != null)
            _inputBlocker.SetBlocked(false);
    }

    private void EnableGameplay(bool cursorActive)
    {
        UnblockGameplay();
        SetSpaceSwitchAllowed(true);
        ForceCursor(cursorActive);
    }

    private void ForceCursor(bool active)
    {
        if (_interactionWithUI != null)
            _interactionWithUI.SetInteracts(active);
    }

    private void SetSpaceSwitchAllowed(bool allowed)
    {
        if (_interactionWithUI != null)
            _interactionWithUI.SetSwitchAllowed(allowed);
    }

    private void ShowObjective(string text)
    {
        if (_objectiveHint != null)
            _objectiveHint.Show(text);
    }

    private void HideObjective()
    {
        if (_objectiveHint != null)
            _objectiveHint.Hide();
    }

    private void PointArrowTo(Transform target)
    {
        if (_arrow != null && target != null)
            _arrow.PointTo(target);
    }

    private void HideArrow()
    {
        if (_arrow != null)
            _arrow.Hide();
    }

    private void HidePanel()
    {
        if (_panel != null)
            _panel.Hide();
    }

    private void HideAllTutorialUI()
    {
        HidePanel();
        HideObjective();
        HideArrow();
    }

    private void SaveProgress()
    {
        // ИЗМЕНЕНО: важные флаги WebGL сохраняются в момент изменения,
        // а не только по таймеру или при потере фокуса.
        NotifyTutorialStateChangedSafely();
        _saveLoadManager?.SaveGame();
    }

    private void NotifyTutorialStateChangedSafely()
    {
        if (OnTutorialStateChanged == null)
            return;

        foreach (System.Delegate handler in OnTutorialStateChanged.GetInvocationList())
        {
            try
            {
                ((System.Action)handler).Invoke();
            }
            catch (System.Exception exception)
            {
                // ИЗМЕНЕНО: ошибка UI-подписчика не должна отменять немедленное сохранение туториала.
                Debug.LogException(exception, this);
            }
        }
    }

    private void TryRepairBrokenFirstComputerSave()
    {
        if (!GameSaveRepository.HasSave ||
            _firstComputerCompensationGranted ||
            _firstRoomZone == null ||
            _coinsData == null ||
            _firstRoomZone.CurrentDevicePurchases > 0)
        {
            return;
        }

        bool progressRequiresComputer =
            _step == TutorialStep.FirstRoomMessage ||
            _step == TutorialStep.WaitFirstDevicePurchase ||
            _step == TutorialStep.FirstDeviceMessage ||
            _step == TutorialStep.WaitFirstIncome;

        if (!progressRequiresComputer)
            return;

        int firstComputerPrice = Mathf.Max(0, _firstRoomZone.CurrentDevicePrice);
        int missingCoins = Mathf.Max(0, firstComputerPrice - _coinsData.CurrentCoins);

        if (missingCoins <= 0)
            return;

        _coinsData.AddResource(missingCoins, 1f);
        _firstComputerCompensationGranted = true;

        // ИЗМЕНЕНО: компенсация выдаёт только недостающую сумму и сразу
        // фиксирует отдельный одноразовый migration-флаг.
        SaveProgress();
    }
}
