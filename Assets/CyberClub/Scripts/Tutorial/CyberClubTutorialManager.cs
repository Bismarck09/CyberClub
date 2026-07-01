using UnityEngine;

public class CyberClubTutorialManager : MonoBehaviour
{
    private const string SaveKey = "CyberClub_Tutorial_Simplified_v1";

    private enum TutorialStep
    {
        Welcome,
        WaitFirstRoom,
        FirstRoomMessage,
        WaitFirstDevicePurchase,
        FirstDeviceMessage,
        InteriorMessage,
        Completed
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

    [Header("Control")]
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private TutorialInputBlocker _inputBlocker;

    [Header("Settings")]
    [SerializeField] private bool _startOnStart = true;
    [SerializeField] private bool _saveProgress;

    private TutorialStep _step;
    private bool _breakdownTutorialShown;
    private bool _ratingTutorialShown;

    private void Awake()
    {
        FindReferences();

        if (_saveProgress && PlayerPrefs.GetInt(SaveKey, 0) == 1)
            _step = TutorialStep.Completed;
        else
            _step = TutorialStep.Welcome;

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
    }

    private void OnDisable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged -= OnZoneChanged;

        if (_devicePurchase != null)
            _devicePurchase.OnDevicePurchased -= OnDevicePurchased;

        if (_ratingData != null)
            _ratingData.OnRatingChanged -= OnRatingChanged;
    }

    private void Start()
    {
        if (_startOnStart && _step == TutorialStep.Welcome)
            StartTutorial();
    }

    [ContextMenu("Reset Tutorial Save")]
    public void ResetTutorialSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        _step = TutorialStep.Welcome;
        _breakdownTutorialShown = false;
        _ratingTutorialShown = false;

        HideAllTutorialUI();
        EnableGameplay(false);
    }

    public void StartTutorial()
    {
        _step = TutorialStep.Welcome;

        BlockGameplay();
        ForceCursor(false);
        SetSpaceSwitchAllowed(true);

        if (_panel != null)
        {
            _panel.ShowWindow(
                "Добро пожаловать в CyberClub",
                "Ты управляешь компьютерным клубом.\n\nПокупай комнаты и компьютеры, обслуживай клиентов, улучшай интерьер и следи за рейтингом.\n\n<b>Сейчас курсор скрыт.</b> Чтобы нажать кнопку в этом окне, нажми <b>Пробел</b> — курсор появится. Потом нажми кнопку <b>Далее</b>.\n\nВ игре Пробел переключает режимы:\n• курсор виден — можно нажимать кнопки, но камера не вращается;\n• курсор скрыт — можно ходить и вращать камеру.",
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

        ShowBlockingWindow(
            "Поломка компьютера",
            "Иногда компьютеры ломаются. Сломанный компьютер не принимает клиентов.\n\nПодойди к нему, включи курсор через <b>Пробел</b> и зажми иконку поломки. За ремонт ты получишь бонусные монеты.",
            "Понял",
            () =>
            {
                EnableGameplay(false);
                HideAllTutorialUI();
            });
    }

    private void BeginGoToFirstRoom()
    {
        _step = TutorialStep.WaitFirstRoom;

        HidePanel();

        EnableGameplay(false);

        ShowObjective("Иди к красной комнате. Если она закрыта — купи её.");
        PointArrowTo(_firstRoomTarget);
    }

    private void BeginWaitFirstDevicePurchase()
    {
        _step = TutorialStep.WaitFirstDevicePurchase;

        HidePanel();
        ShowObjective("Купи первый компьютер в панели улучшений.");
        PointArrowTo(_buyDeviceButtonTarget);

        // Игрок должен нажать UI-кнопку покупки. Поэтому курсор виден,
        // движение и камера заблокированы, а Пробел временно отключён,
        // чтобы игрок случайно не спрятал курсор.
        BlockGameplay();
        ForceCursor(true);
        SetSpaceSwitchAllowed(false);
    }

    private void OnDevicePurchased()
    {
        if (_step != TutorialStep.WaitFirstDevicePurchase)
            return;

        _step = TutorialStep.FirstDeviceMessage;

        HideObjective();
        HideArrow();

        ShowBlockingWindow(
            "Первый компьютер куплен",
            "Теперь клиенты смогут садиться за компьютер и приносить монеты.\n\nЧем больше компьютеров, тем выше доход, но если клиентов станет слишком много, админы могут не успевать.",
            "Далее",
            ShowInteriorMessage
        );
    }

    private void ShowInteriorMessage()
    {
        _step = TutorialStep.InteriorMessage;

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

        HideAllTutorialUI();
        EnableGameplay(false);

        if (_saveProgress)
        {
            PlayerPrefs.SetInt(SaveKey, 1);
            PlayerPrefs.Save();
        }
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

        PointArrowTo(_ratingTarget);

        ShowBlockingWindow(
            "Рейтинг и админы",
            "Рейтинг начал падать — значит клиенты слишком долго ждут в очереди.\n\nКупи нового админа или прокачай текущих, чтобы они быстрее отправляли клиентов за компьютеры.",
            "Понял",
            () =>
            {
                HideArrow();
                EnableGameplay(false);
                HideAllTutorialUI();
            }
        );
    }

    private void ShowBlockingWindow(string title, string body, string button, System.Action onClick)
    {
        BlockGameplay();
        ForceCursor(true);
        SetSpaceSwitchAllowed(false);

        if (_panel != null)
            _panel.ShowWindow(title, body, button, onClick);
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

    private void FindReferences()
    {
        if (_panel == null)
            _panel = FindAnyObjectByType<TutorialPanel>();

        if (_objectiveHint == null)
            _objectiveHint = FindAnyObjectByType<TutorialObjectiveHint>();

        if (_arrow == null)
            _arrow = FindAnyObjectByType<TutorialArrowPointer>();

        if (_interactionWithUI == null)
            _interactionWithUI = FindAnyObjectByType<InteractionWithUI>();

        if (_inputBlocker == null)
            _inputBlocker = FindAnyObjectByType<TutorialInputBlocker>();

        if (_zoneSwitcher == null)
            _zoneSwitcher = FindAnyObjectByType<ZoneSwitcher>();

        if (_devicePurchase == null)
            _devicePurchase = FindAnyObjectByType<DevicePurchase>();

        if (_ratingData == null)
            _ratingData = FindAnyObjectByType<RatingData>();
    }
}
