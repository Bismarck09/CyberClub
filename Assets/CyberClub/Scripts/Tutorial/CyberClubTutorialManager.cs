using System;
using System.Collections.Generic;
using UnityEngine;

public class CyberClubTutorialManager : MonoBehaviour
{
    private const string InitialTutorialKey = "Tutorial_Initial_Done";
    private const string FirstRoomTutorialKey = "Tutorial_FirstRoom_Done";
    private const string FirstDeviceTutorialKey = "Tutorial_FirstDevice_Done";
    private const string InteriorTutorialKey = "Tutorial_Interior_Done";
    private const string RatingAdminTutorialKey = "Tutorial_RatingAdmin_Done";
    private const string BreakdownTutorialKey = "Tutorial_Breakdown_Done";

    [Header("UI")]
    [SerializeField] private TutorialPanel _panel;
    [SerializeField] private TutorialWorldPointer _worldPointer;
    [SerializeField] private bool _usePointer = true;

    [Header("First room")]
    [SerializeField] private ZoneSwitcher _zoneSwitcher;
    [SerializeField] private ZoneInformation _firstRoomZone;
    [SerializeField] private string _firstRoomNamePart = "Красная";

    [Header("Pointer targets")]
    [SerializeField] private Transform _firstRoomTarget;
    [SerializeField] private Transform _deviceButtonTarget;
    [SerializeField] private Transform _ratingTarget;

    [Header("Services")]
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private GameplayInputBlocker _gameplayInputBlocker;
    [SerializeField] private DevicePurchase _devicePurchase;
    [SerializeField] private InteriorPurchase _interiorPurchase;
    [SerializeField] private RatingData _ratingData;

    [Header("Settings")]
    [SerializeField] private bool _startTutorialOnStart = true;
    [SerializeField] private bool _usePlayerPrefs = true;
    [SerializeField] private bool _resetProgressOnStartInEditor = true;

    private readonly Queue<TutorialMessage> _queue = new();

    private bool _isShowingBlockingSlide;
    private bool _waitingForFirstSpace;

    private bool _initialDone;
    private bool _firstRoomDone;
    private bool _firstDeviceDone;
    private bool _interiorDone;
    private bool _ratingAdminDone;
    private bool _breakdownDone;

    private void Awake()
    {
#if UNITY_EDITOR
        if (_resetProgressOnStartInEditor)
            ClearSavedProgress();
#endif

        FindMissingReferences();
        LoadProgress();

        if (!_usePointer)
            HidePointer();
    }

    private void OnEnable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged += OnInteractionModeChanged;

        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged += OnZoneChanged;

        if (_devicePurchase != null)
            _devicePurchase.OnDevicePurchased += OnDevicePurchased;

        if (_interiorPurchase != null)
            _interiorPurchase.OnInteriorPurchase += OnInteriorPurchased;

        if (_ratingData != null)
            _ratingData.OnRatingChanged += OnRatingChanged;
    }

    private void OnDisable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged -= OnInteractionModeChanged;

        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged -= OnZoneChanged;

        if (_devicePurchase != null)
            _devicePurchase.OnDevicePurchased -= OnDevicePurchased;

        if (_interiorPurchase != null)
            _interiorPurchase.OnInteriorPurchase -= OnInteriorPurchased;

        if (_ratingData != null)
            _ratingData.OnRatingChanged -= OnRatingChanged;
    }

    private void Start()
    {
        if (_startTutorialOnStart && !_initialDone)
            StartInitialTutorial();
    }

    public void StartInitialTutorial()
    {
        if (_initialDone)
            return;

        ShowBlockingSlide(
            "Добро пожаловать в CyberClub",
            "Ты управляешь компьютерным клубом.\n\nПокупай комнаты и компьютеры, обслуживай клиентов, улучшай интерьер и следи за рейтингом.",
            "Далее",
            ShowSpaceTutorial
        );
    }

    public void OnPlayerEnteredFirstRoom()
    {
        ShowFirstRoomTutorial();
    }

    public void OnBreakdownStartedManually()
    {
        ShowBreakdownTutorial();
    }

    [ContextMenu("Reset Tutorial Progress")]
    public void ResetTutorialProgress()
    {
        ClearSavedProgress();

        _initialDone = false;
        _firstRoomDone = false;
        _firstDeviceDone = false;
        _interiorDone = false;
        _ratingAdminDone = false;
        _breakdownDone = false;

        _queue.Clear();
        _isShowingBlockingSlide = false;
        _waitingForFirstSpace = false;

        if (_panel != null)
            _panel.Hide();

        HidePointer();
        EnterGameplayMode(false);
    }

    private void ShowSpaceTutorial()
    {
        _waitingForFirstSpace = true;

        BlockGameplay();
        ForceCursorActive();
        SetSpaceAllowed(true);

        _isShowingBlockingSlide = true;

        if (_panel != null)
        {
            _panel.Show(
                "Курсор и камера",
                "Сейчас курсор активен, поэтому ты можешь нажимать кнопки интерфейса.\n\nНажми <b>Пробел</b>, чтобы спрятать курсор, включить движение и снова вращать камеру.",
                "Нажми Пробел",
                null,
                false
            );
        }
    }

    private void OnInteractionModeChanged(bool isInteracts)
    {
        if (!_waitingForFirstSpace)
            return;

        if (isInteracts)
            return;

        _waitingForFirstSpace = false;
        _initialDone = true;
        SaveProgress();

        HideBlockingSlide();

        EnterGameplayMode(false);
        PointTo(_firstRoomTarget);

        ShowObjectiveHint(
            "Иди к красной комнате",
            "Иди к красной комнате. Если она закрыта — купи её. После входа игра подскажет, как купить первый компьютер."
        );
    }

    private void OnZoneChanged(ZoneInformation zone)
    {
        if (IsFirstRoom(zone))
            ShowFirstRoomTutorial();
    }

    private void ShowFirstRoomTutorial()
    {
        if (_firstRoomDone)
            return;

        _firstRoomDone = true;
        SaveProgress();

        HideObjectiveHint();
        PointTo(_deviceButtonTarget);

        EnqueueBlockingSlide(
            "Купи первый компьютер",
            "Ты в первой комнате. Теперь нужно купить компьютер в панели улучшений.\n\nКогда закроешь подсказку, курсор останется активным — нажми кнопку покупки компьютера.",
            "Понял",
            () =>
            {
                HideBlockingSlide();
                EnterGameplayMode(true);
                PointTo(_deviceButtonTarget);
            }
        );
    }

    private bool IsFirstRoom(ZoneInformation zone)
    {
        if (zone == null)
            return false;

        if (_firstRoomZone != null && zone == _firstRoomZone)
            return true;

        if (string.IsNullOrWhiteSpace(_firstRoomNamePart))
            return false;

        string zoneName = zone.ZoneName;

        if (!string.IsNullOrWhiteSpace(zoneName) && zoneName.Contains(_firstRoomNamePart))
            return true;

        return zone.name.Contains(_firstRoomNamePart);
    }

    private void OnDevicePurchased()
    {
        if (_firstDeviceDone)
            return;

        _firstDeviceDone = true;
        SaveProgress();

        HidePointer();

        EnqueueBlockingSlide(
            "Первый компьютер куплен",
            "Теперь клиенты смогут садиться за компьютер и приносить монеты.\n\nЧем больше компьютеров, тем выше доход, но если клиентов станет слишком много, админы могут не успевать.",
            "Далее",
            () =>
            {
                HideBlockingSlide();
                ShowInteriorTutorial();
            }
        );
    }

    private void ShowInteriorTutorial()
    {
        if (_interiorDone)
        {
            EnterGameplayMode(false);
            return;
        }

        _interiorDone = true;
        SaveProgress();

        EnqueueBlockingSlide(
            "Интерьер",
            "Интерьер повышает множитель комнаты. Это значит, что клиенты в этой комнате будут приносить больше монет.\n\nИногда интерьер выгоднее, чем просто ждать деньги на следующий компьютер.",
            "Понял",
            () =>
            {
                HideBlockingSlide();
                EnterGameplayMode(false);
            }
        );
    }

    private void OnInteriorPurchased(InteriorData interiorData)
    {
        if (_interiorDone)
            return;

        _interiorDone = true;
        SaveProgress();

        EnqueueBlockingSlide(
            "Комната стала прибыльнее",
            "Множитель комнаты вырос. Теперь клиенты здесь будут приносить больше монет.",
            "Отлично",
            () =>
            {
                HideBlockingSlide();
                EnterGameplayMode(false);
            }
        );
    }

    private void OnRatingChanged(float amount)
    {
        if (amount >= 0f)
            return;

        ShowRatingAdminTutorial();
    }

    private void ShowRatingAdminTutorial()
    {
        if (_ratingAdminDone)
            return;

        _ratingAdminDone = true;
        SaveProgress();

        PointTo(_ratingTarget);

        EnqueueBlockingSlide(
            "Рейтинг и админы",
            "Рейтинг начал падать — значит клиенты слишком долго ждут в очереди.\n\nКупи нового админа или прокачай текущих, чтобы они быстрее отправляли клиентов за компьютеры.",
            "Понял",
            () =>
            {
                HidePointer();
                HideBlockingSlide();
                EnterGameplayMode(false);
            }
        );
    }

    private void ShowBreakdownTutorial()
    {
        if (_breakdownDone)
            return;

        _breakdownDone = true;
        SaveProgress();

        EnqueueBlockingSlide(
            "Поломка компьютера",
            "Иногда компьютеры ломаются. Сломанный компьютер не принимает клиентов.\n\nПодойди к нему, включи курсор через <b>Пробел</b> и зажми иконку поломки. За ремонт ты получишь бонусные монеты.",
            "Понял",
            () =>
            {
                HideBlockingSlide();
                EnterGameplayMode(false);
            }
        );
    }

    private void ShowBlockingSlide(string title, string body, string buttonText, Action onNext)
    {
        BlockGameplay();
        ForceCursorActive();
        SetSpaceAllowed(false);

        _isShowingBlockingSlide = true;

        if (_panel != null)
            _panel.Show(title, body, buttonText, onNext, true);
    }

    private void EnqueueBlockingSlide(string title, string body, string buttonText, Action onNext)
    {
        TutorialMessage message = new TutorialMessage(title, body, buttonText, onNext);

        if (_isShowingBlockingSlide)
        {
            _queue.Enqueue(message);
            return;
        }

        ShowQueuedBlockingSlide(message);
    }

    private void ShowQueuedBlockingSlide(TutorialMessage message)
    {
        ShowBlockingSlide(message.Title, message.Body, message.ButtonText, () =>
        {
            message.OnNext?.Invoke();
            TryShowNextQueuedMessage();
        });
    }

    private void TryShowNextQueuedMessage()
    {
        if (_isShowingBlockingSlide || _queue.Count <= 0)
            return;

        ShowQueuedBlockingSlide(_queue.Dequeue());
    }

    private void HideBlockingSlide()
    {
        _isShowingBlockingSlide = false;

        if (_panel != null)
            _panel.Hide();
    }

    private void ShowObjectiveHint(string title, string body)
    {
        if (_panel != null)
            _panel.Show(title, body, string.Empty, null, false);
    }

    private void HideObjectiveHint()
    {
        if (_panel != null)
            _panel.Hide();
    }

    private void BlockGameplay()
    {
        if (_gameplayInputBlocker != null)
            _gameplayInputBlocker.SetBlocked(true);
    }

    private void EnterGameplayMode(bool keepCursorActive)
    {
        if (_gameplayInputBlocker != null)
            _gameplayInputBlocker.SetBlocked(false);

        SetSpaceAllowed(true);

        if (_interactionWithUI != null)
            _interactionWithUI.SetInteracts(keepCursorActive);
    }

    private void ForceCursorActive()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.SetInteracts(true);
    }

    private void SetSpaceAllowed(bool value)
    {
        if (_interactionWithUI != null)
            _interactionWithUI.SetModeSwitchAllowed(value);
    }

    private void PointTo(Transform target)
    {
        if (!_usePointer || _worldPointer == null || target == null)
            return;

        _worldPointer.PointTo(target);
    }

    private void HidePointer()
    {
        if (_worldPointer != null)
            _worldPointer.Hide();
    }

    private void FindMissingReferences()
    {
        if (_panel == null)
            _panel = FindFirstObjectByType<TutorialPanel>();

        if (_worldPointer == null)
            _worldPointer = FindFirstObjectByType<TutorialWorldPointer>();

        if (_interactionWithUI == null)
            _interactionWithUI = FindFirstObjectByType<InteractionWithUI>();

        if (_gameplayInputBlocker == null)
            _gameplayInputBlocker = FindFirstObjectByType<GameplayInputBlocker>();

        if (_zoneSwitcher == null)
            _zoneSwitcher = FindFirstObjectByType<ZoneSwitcher>();

        if (_devicePurchase == null)
            _devicePurchase = FindFirstObjectByType<DevicePurchase>();

        if (_interiorPurchase == null)
            _interiorPurchase = FindFirstObjectByType<InteriorPurchase>();

        if (_ratingData == null)
            _ratingData = FindFirstObjectByType<RatingData>();
    }

    private void LoadProgress()
    {
        if (!_usePlayerPrefs)
            return;

        _initialDone = PlayerPrefs.GetInt(InitialTutorialKey, 0) == 1;
        _firstRoomDone = PlayerPrefs.GetInt(FirstRoomTutorialKey, 0) == 1;
        _firstDeviceDone = PlayerPrefs.GetInt(FirstDeviceTutorialKey, 0) == 1;
        _interiorDone = PlayerPrefs.GetInt(InteriorTutorialKey, 0) == 1;
        _ratingAdminDone = PlayerPrefs.GetInt(RatingAdminTutorialKey, 0) == 1;
        _breakdownDone = PlayerPrefs.GetInt(BreakdownTutorialKey, 0) == 1;
    }

    private void SaveProgress()
    {
        if (!_usePlayerPrefs)
            return;

        PlayerPrefs.SetInt(InitialTutorialKey, _initialDone ? 1 : 0);
        PlayerPrefs.SetInt(FirstRoomTutorialKey, _firstRoomDone ? 1 : 0);
        PlayerPrefs.SetInt(FirstDeviceTutorialKey, _firstDeviceDone ? 1 : 0);
        PlayerPrefs.SetInt(InteriorTutorialKey, _interiorDone ? 1 : 0);
        PlayerPrefs.SetInt(RatingAdminTutorialKey, _ratingAdminDone ? 1 : 0);
        PlayerPrefs.SetInt(BreakdownTutorialKey, _breakdownDone ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ClearSavedProgress()
    {
        PlayerPrefs.DeleteKey(InitialTutorialKey);
        PlayerPrefs.DeleteKey(FirstRoomTutorialKey);
        PlayerPrefs.DeleteKey(FirstDeviceTutorialKey);
        PlayerPrefs.DeleteKey(InteriorTutorialKey);
        PlayerPrefs.DeleteKey(RatingAdminTutorialKey);
        PlayerPrefs.DeleteKey(BreakdownTutorialKey);
        PlayerPrefs.Save();
    }

    private readonly struct TutorialMessage
    {
        public readonly string Title;
        public readonly string Body;
        public readonly string ButtonText;
        public readonly Action OnNext;

        public TutorialMessage(string title, string body, string buttonText, Action onNext)
        {
            Title = title;
            Body = body;
            ButtonText = buttonText;
            OnNext = onNext;
        }
    }
}
