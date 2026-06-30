using UnityEngine;

public class CyberClubTutorialManager : MonoBehaviour
{
    private const string InitialTutorialKey = "Tutorial_Initial_Done";
    private const string FirstRoomTutorialKey = "Tutorial_FirstRoom_Done";
    private const string FirstDeviceTutorialKey = "Tutorial_FirstDevice_Done";
    private const string InteriorTutorialKey = "Tutorial_Interior_Done";
    private const string RatingTutorialKey = "Tutorial_Rating_Done";
    private const string AdminTutorialKey = "Tutorial_Admin_Done";
    private const string BreakdownTutorialKey = "Tutorial_Breakdown_Done";

    [Header("UI")]
    [SerializeField] private TutorialPanel _panel;
    [SerializeField] private TutorialWorldPointer _worldPointer;

    [Header("Targets")]
    [SerializeField] private Transform _firstRoomTarget;
    [SerializeField] private Transform _upgradePanelTarget;
    [SerializeField] private Transform _ratingTarget;

    [Header("Game services")]
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private DevicePurchase _devicePurchase;
    [SerializeField] private InteriorPurchase _interiorPurchase;
    [SerializeField] private RatingData _ratingData;
    [SerializeField] private ComputerBreakdownService _breakdownService;

    [Header("Settings")]
    [SerializeField] private bool _startTutorialOnStart = true;
    [SerializeField] private bool _usePlayerPrefs = true;

    private int _initialStage;
    private bool _waitingForCursorOn;
    private bool _waitingForCursorOff;

    private void Awake()
    {
        if (_interactionWithUI == null)
            _interactionWithUI = FindFirstObjectByType<InteractionWithUI>();

        if (_devicePurchase == null)
            _devicePurchase = FindFirstObjectByType<DevicePurchase>();

        if (_interiorPurchase == null)
            _interiorPurchase = FindFirstObjectByType<InteriorPurchase>();

        if (_ratingData == null)
            _ratingData = FindFirstObjectByType<RatingData>();

        if (_breakdownService == null)
            _breakdownService = FindFirstObjectByType<ComputerBreakdownService>();
    }

    private void OnEnable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged += OnInteractionModeChanged;

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

        if (_devicePurchase != null)
            _devicePurchase.OnDevicePurchased -= OnDevicePurchased;

        if (_interiorPurchase != null)
            _interiorPurchase.OnInteriorPurchase -= OnInteriorPurchased;

        if (_ratingData != null)
            _ratingData.OnRatingChanged -= OnRatingChanged;
    }

    private void Start()
    {
        if (_startTutorialOnStart && !IsDone(InitialTutorialKey))
            StartInitialTutorial();
    }

    public void StartInitialTutorial()
    {
        _initialStage = 0;
        ShowInitialStage();
    }

    public void OnPlayerEnteredFirstRoom()
    {
        if (IsDone(FirstRoomTutorialKey))
            return;

        SetDone(FirstRoomTutorialKey);

        if (_worldPointer != null)
            _worldPointer.PointTo(_upgradePanelTarget);

        Show(
            "Первая комната",
            "Отлично, ты дошёл до первой комнаты. Теперь нужно купить компьютер. Нажми <b>Пробел</b>, чтобы включить мышку, и нажми кнопку покупки компьютера в панели улучшений.",
            "Понял",
            null
        );
    }

    public void OnBreakdownStartedManually()
    {
        ShowBreakdownTutorial();
    }

    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey(InitialTutorialKey);
        PlayerPrefs.DeleteKey(FirstRoomTutorialKey);
        PlayerPrefs.DeleteKey(FirstDeviceTutorialKey);
        PlayerPrefs.DeleteKey(InteriorTutorialKey);
        PlayerPrefs.DeleteKey(RatingTutorialKey);
        PlayerPrefs.DeleteKey(AdminTutorialKey);
        PlayerPrefs.DeleteKey(BreakdownTutorialKey);
        PlayerPrefs.Save();
    }

    private void ShowInitialStage()
    {
        switch (_initialStage)
        {
            case 0:
                Show(
                    "Добро пожаловать в CyberClub",
                    "Твоя задача — развивать компьютерный клуб: покупать компьютеры, обслуживать клиентов, улучшать комнаты и следить за рейтингом.",
                    "Далее",
                    () =>
                    {
                        _initialStage++;
                        ShowInitialStage();
                    }
                );
                break;

            case 1:
                _waitingForCursorOn = true;

                Show(
                    "Режим мышки",
                    "Главное управление: нажми <b>Пробел</b>, чтобы включить активную мышку. В этом режиме можно нажимать кнопки интерфейса, но камера не вращается.",
                    "Нажми Пробел",
                    null,
                    false
                );
                break;

            case 2:
                _waitingForCursorOff = true;

                Show(
                    "Верни управление камерой",
                    "Теперь нажми <b>Пробел</b> ещё раз. Мышка спрячется, и ты снова сможешь вращать камеру.",
                    "Нажми Пробел",
                    null,
                    false
                );
                break;

            case 3:
                if (_worldPointer != null)
                    _worldPointer.PointTo(_firstRoomTarget);

                Show(
                    "Иди к красной комнате",
                    "Начни с красной комнаты. Подойди к ней и открой панель улучшений, чтобы купить первый компьютер.",
                    "Понял",
                    () =>
                    {
                        SetDone(InitialTutorialKey);
                        Hide();
                    }
                );
                break;
        }
    }

    private void OnInteractionModeChanged(bool isInteracts)
    {
        if (_waitingForCursorOn && isInteracts)
        {
            _waitingForCursorOn = false;
            _initialStage = 2;
            ShowInitialStage();
            return;
        }

        if (_waitingForCursorOff && !isInteracts)
        {
            _waitingForCursorOff = false;
            _initialStage = 3;
            ShowInitialStage();
        }
    }

    private void OnDevicePurchased()
    {
        if (IsDone(FirstDeviceTutorialKey))
            return;

        SetDone(FirstDeviceTutorialKey);

        if (_worldPointer != null)
            _worldPointer.Hide();

        Show(
            "Первый компьютер куплен",
            "Компьютер начнёт приносить деньги, когда клиент сядет за него. Чем больше компьютеров, тем больше доход, но следи за очередью: слабые админы могут не успевать обслуживать клиентов.",
            "Далее",
            () =>
            {
                if (!IsDone(InteriorTutorialKey))
                    ShowInteriorTutorial();
                else
                    Hide();
            }
        );
    }

    private void ShowInteriorTutorial()
    {
        Show(
            "Интерьер",
            "Интерьер — это не просто украшение. Он повышает множитель комнаты, значит каждый клиент приносит больше монет. Если не знаешь, что купить дальше, интерьер часто хороший выбор.",
            "Понял",
            () =>
            {
                SetDone(InteriorTutorialKey);
                Hide();
            }
        );
    }

    private void OnInteriorPurchased(InteriorData interiorData)
    {
        if (IsDone(InteriorTutorialKey))
            return;

        SetDone(InteriorTutorialKey);

        Show(
            "Комната стала прибыльнее",
            "После покупки интерьера множитель комнаты вырос. Теперь клиенты в этой комнате будут приносить больше денег.",
            "Отлично",
            Hide
        );
    }

    private void OnRatingChanged(float amount)
    {
        if (!IsDone(RatingTutorialKey))
            ShowRatingTutorial();

        if (amount < 0f && !IsDone(AdminTutorialKey))
            ShowAdminTutorial();
    }

    private void ShowRatingTutorial()
    {
        SetDone(RatingTutorialKey);

        if (_worldPointer != null)
            _worldPointer.PointTo(_ratingTarget);

        Show(
            "Рейтинг клуба",
            "Рейтинг влияет на доход и поток клиентов. Если клиенты долго ждут в очереди, рейтинг падает. Если обслуживание быстрое — рейтинг растёт.",
            "Понял",
            () =>
            {
                if (_worldPointer != null)
                    _worldPointer.Hide();

                Hide();
            }
        );
    }

    private void ShowAdminTutorial()
    {
        SetDone(AdminTutorialKey);

        Show(
            "Админы не успевают",
            "Рейтинг начал падать — значит клиенты слишком долго стоят в очереди. Купи нового админа или прокачай текущих, чтобы они быстрее отправляли клиентов за компьютеры.",
            "Понял",
            Hide
        );
    }

    private void ShowBreakdownTutorial()
    {
        if (IsDone(BreakdownTutorialKey))
            return;

        SetDone(BreakdownTutorialKey);

        Show(
            "Поломки компьютеров",
            "Иногда компьютеры ломаются. Сломанный компьютер не принимает клиентов. Подойди к нему, включи мышку через <b>Пробел</b> и зажми иконку поломки, чтобы починить компьютер и получить бонус.",
            "Понял",
            Hide
        );
    }

    private void Show(string title, string body, string button, System.Action onNext, bool showButton = true)
    {
        if (_panel == null)
            return;

        _panel.Show(title, body, button, onNext, showButton);
    }

    private void Hide()
    {
        if (_panel != null)
            _panel.Hide();
    }

    private bool IsDone(string key)
    {
        return _usePlayerPrefs && PlayerPrefs.GetInt(key, 0) == 1;
    }

    private void SetDone(string key)
    {
        if (!_usePlayerPrefs)
            return;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }
}
