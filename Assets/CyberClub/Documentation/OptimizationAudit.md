# CyberClub — технический аудит оптимизации

Дата аудита: 2026-07-21  
Версия редактора: Unity 6000.4.0f1  
Основная сцена: `Assets/Scenes/MainScene.unity`

## 1. Целевые платформы

- Основная публикация: WebGL в Яндекс Играх.
- Мобильные устройства: Android и iOS; обе платформы назначены на профиль качества `Mobile`.
- Desktop: Windows/macOS и профиль качества `PC`.

WebGL сейчас также использует профиль `Mobile`. Активный профиль и баланс качества в ходе аудита не переключались.

## 2. Методика и ограничения измерения

Выполнены повторный статический аудит проекта, импорт `MainScene`, компиляция скриптов и структурная проверка сцены в Unity batchmode. Проверены настройки URP, Quality, WebGL Player Settings, импортёры текстур и аудио, код посетителей/NavMesh/UI, сериализованные ссылки и Missing Script.

Полноценная игровая сессия, GPU-профилирование, Frame Debugger, Development WebGL build и тест на телефоне в этой среде не запускались. В пакетах проекта не установлены Profile Analyzer и Memory Profiler. Готового Build Report и готовой WebGL-сборки в корне проекта не найдено. Поэтому FPS, CPU/GPU frame time, Batches, SetPass, GC Alloc/frame, пиковая память и размер WebGL-билда **не измерены**. Числа для них намеренно не приводятся.

Для сопоставимого замера нужен Development Build без Deep Profile, Autoconnect Profiler и одинаковый сценарий:

1. Пустой клуб — 60 секунд после загрузки.
2. Максимально доступное число посетителей — 120 секунд.
3. Обзор всей сцены сверху — 60 секунд.
4. Открытый магазин и прокрутка — 30 секунд.
5. Активные зелья и несколько работающих зон — 120 секунд.
6. Телефон — непрерывная сессия 10–15 минут с фиксацией памяти и температуры.

## 3. Текущее состояние

### Scene и UI

| Показатель | Фактическое значение |
|---|---:|
| GameObject, развёрнутые Unity при загрузке MainScene | 834 |
| Прямые GameObject-записи в YAML сцены | 733 |
| Прямые MonoBehaviour-записи в YAML | 440 |
| Canvas | 11 |
| World Space Canvas | 10 |
| World Space Canvas без GraphicRaycaster | 0 |
| World Space Canvas без явно заданной Event Camera | 10 |
| UI Graphic с `Raycast Target = On` в YAML | 146 |
| UI Graphic с `Raycast Target = Off` в YAML | 11 |
| ScrollRect | 4 |
| LayoutGroup | 4 |
| EventSystem / InputSystemUIInputModule | 1 / 1 |

Все World Space Canvas имеют GraphicRaycaster. При пустом `worldCamera` UGUI использует доступную event camera/Camera.main, но это необходимо подтвердить касанием на целевом устройстве; при появлении второй камеры ссылку лучше назначить явно. Второй EventSystem не добавлялся.

Технические Graphics `MobileLookArea` и `MobileVirtualJoystick` теперь имеют `Raycast Target = Off`. До захвата touch выполняется `EventSystem.RaycastAll`, поэтому кнопки, Selectable, ScrollRect, EventTrigger и pointer/drag/scroll handlers имеют приоритет.

### Rendering и сцена

| Показатель | Фактическое значение |
|---|---:|
| Light | 23 |
| Baked Light | 23 |
| Camera | 1 |
| Camera Far Clip Plane | 5000 |
| Camera Culling Mask | все слои |
| LODGroup | 0 |
| Reflection Probe | 0 |
| GameObject с ненулевыми Static Flags в YAML | 40 |
| Назначенные Occlusion Culling Data | 0 |
| Shader Graph | 11 |

Источники света запечённые, поэтому их Inspector shadow-настройки главным образом влияют на bake, а не создают 23 динамические тени каждый кадр. Камера разрешает Occlusion Culling, но в сцене нет назначенных occlusion data; фактической пользы от флага без bake нет. Far Clip 5000 выглядит чрезмерным для клубного помещения, однако снижать его следует после проверки видимых дальних объектов и теней.

### URP и Quality

Текущий `Mobile_RPAsset`: Render Scale 0.8, MSAA 1, HDR включён, depth/opaque texture выключены, main shadow 1024, shadow distance 50, один cascade, additional light shadows выключены, SRP Batcher включён, Renderer Features отсутствуют.

Текущий `PC_RPAsset`: Render Scale 1.0, MSAA 1, HDR включён, depth/opaque texture включены, main shadow 2048, shadow distance 50, четыре cascade, additional light shadows включены, SRP Batcher включён. `PC_Renderer` использует Deferred и активный SSAO.

### Текстуры и аудио

| Показатель | Фактическое значение |
|---|---:|
| TextureImporter | 489 |
| Mipmaps включены / выключены | 456 / 33 |
| Texture Read/Write включён | 0 |
| Явные platform override (`overridden: 1`) | 0 |
| AudioImporter | 12 |
| Audio Load Type 0 | 12 |

У текстур Read/Write не включён. Отдельный флаг Read/Write найден у demo-меша `Toon Shaders Pro/Demo/Meshes/suzanne.fbx`; это не игровой texture asset. В импортёрах преобладает Max Size 2048, но одна и та же текстура содержит default и platform-блоки, поэтому количество таких строк нельзя трактовать как количество фактически загружаемых текстур. Массовое уменьшение текстур без Memory Profiler не выполнялось.

Все 12 аудиоклипов используют Load Type 0 (`Decompress On Load`). Для коротких SFX это нормально; длинные дорожки следует переводить в Streaming/Compressed In Memory только после проверки длительности, размера и пиков памяти.

### WebGL

- Data Caching включён.
- Incremental GC и Strip Engine Code включены.
- Managed Stripping Level в сериализованной настройке WebGL: 4.
- Начальная память: 32 MB; максимум: 2048 MB; режим роста: 2.
- `webGLCompressionFormat` хранится как 0; перед релизом нужно подтвердить в Inspector требуемый Brotli/Gzip и корректные HTTP-заголовки хостинга Яндекс Игр.
- Development Build и debug symbols должны использоваться только для замеров, не для релиза.
- Размер сборки неизвестен: build и Build Report не создавались.

## 4. Посетители, NavMesh и CPU-код

### VisitorSpawner

В `MainScene` Inspector переопределяет значения скрипта: базовая задержка 3 секунды, задержка внутри группы 0.5 секунды, размер группы 1–10. Максимум активных посетителей вычисляется из числа устройств и рейтинга и дополнительно ограничен свободными местами очереди; статически назвать runtime-максимум нельзя.

Каждый посетитель создаётся через `Instantiate`, а при выходе удаляется через `Destroy`. Это кандидат для небольшого пула, но пул не внедрён: без Profiler нельзя доказать, что именно спавн является заметным пиком CPU/GC, а корректный reset посетителя затрагивает очередь, NavMeshAgent, Animator, эффекты, события и резерв устройства.

### VisitorMovement и NavMesh

Семь назначенных префабов посетителей содержат по одному NavMeshAgent и одному Animator. `SetDestination` вызывается один раз при `Move`, а не каждый кадр. `Update` выполняет проверки только при активной цели. Есть timeout движения, stuck timeout, проверка partial/invalid path и `SamplePosition`; постоянного path recalculation из кода не обнаружено.

Animator посетителей использует `m_CullingMode = 1` (`Cull Update Transforms`), поэтому дополнительная массовая смена culling не выполнялась. Нужно измерить число одновременно видимых/невидимых Animator в Profiler.

### VisitorService и DeviceRegistry

`VisitorService.Update` каждый кадр перебирает администраторов. Это остаётся кандидатом на событийный запуск при изменении очереди, состояния администратора или устройства. Автоматически сделана только безопасная часть: если у администратора нет посетителя, поиск свободного устройства больше не запускается.

`DeviceRegistry.GetRandomFreeDevice` раньше создавал временный список через `FindAll` на каждой попытке обслуживания. Теперь используется двухпроходный выбор без временной коллекции. `SpawnPointsHolder.AvailableSpawnPointCount` также больше не создаёт список через `FindAll`. `GetBreakableDevices` по-прежнему возвращает отдельный список, но вызывается для операции выбора поломки, а не в доказанном горячем цикле.

## 5. Приоритеты

### Критично

- Снять Development-профиль на реальном WebGL и телефоне. Без него нельзя утверждать, ограничен ли проект CPU, GPU или памятью.
- Построить релизный WebGL и сохранить Build Report: сейчас размер загрузки и вклад assets/managed code неизвестны.
- Один отсутствующий `LightmapParameters` в `Map/Skeleton/Cube (11)` найден и очищен. Финальная структурная проверка: Missing Script = 0, broken reference = 0.

### Важно

- Проверить пики `Instantiate/Destroy` VisitorSpawner. Если подтверждены — проектировать ограниченный пул только для семи visitor-prefab с полным reset-контрактом.
- Проверить `VisitorService.Update` при максимуме администраторов и посетителей; при заметной стоимости перевести обслуживание на события/редкий тик.
- Проверить 146 активных UI Raycast Target и отключить только декоративные Graphics, подтверждая клики после каждого Canvas-блока.
- Проверить Far Clip 5000. Предварительный диапазон для помещения — 100–300, но итог зависит от геометрии и камеры.
- Запечь Occlusion Culling для стационарной геометрии и сравнить Rendering Profiler/Frame Debugger до и после. Не включать bake вслепую для динамических зон.
- Проверить отсутствие LOD на крупных/часто повторяющихся моделях с дальним экранным размером. Для маленького помещения LOD может не окупить сложность.
- Разделить аудио по длительности: короткие SFX оставить Decompress On Load, длинную музыку/ambience проверить в Streaming.

### Желательно

- Profile Analyzer и Memory Profiler установить для воспроизводимого сравнения сессий и memory snapshot.
- Проверить Sprite Atlas для стабильных групп UI-спрайтов и реальный эффект по Batches/SetPass.
- Проверить Shader Graph variants в Build Report; не удалять варианты без воспроизводимого списка используемых материалов.
- Рассмотреть explicit Event Camera для World Space Canvas, если появятся дополнительные камеры.

## 6. Рекомендованные профили качества

Это стартовые диапазоны для A/B-замера, а не применённые изменения.

| Параметр | Desktop | Mobile | WebGL |
|---|---:|---:|---:|
| Render Scale | 1.0 | 0.75–0.85 | 0.75–0.9 |
| MSAA | 1–4 по GPU-запасу | 1–2 | 1–2 |
| Shadow Distance | 30–50 | 20–30 | 20–35 |
| Cascades | 2–4 | 1 | 1 |
| Additional Light Shadows | по сцене | Off | Off |
| Additional Lights/Object | 4 | 2–4 | 2–4 |
| HDR | On при нужном post FX | Off, если не нужен Bloom/tonemap | Off, если не нужен Bloom/tonemap |
| Bloom | умеренно | Off/Low | Off/Low |
| Texture Limit | 2048, 4096 только hero | 1024–2048 после snapshot | 1024–2048 после Build Report |

Mobile уже использует Render Scale 0.8, один cascade, отключённые additional shadows и пустой Renderer Features — это разумная база. HDR нужно отключать только после визуального сравнения, потому что материалы и post-processing могут зависеть от него. PC SSAO и четыре cascade следует оценить Frame Debugger/GPU Profiler, а не переносить в WebGL.

## 7. Что уже исправлено

1. `DeviceRegistry.GetRandomFreeDevice`: убрана временная `List` из горячего пути.
2. `SpawnPointsHolder.AvailableSpawnPointCount`: убрана временная `List`.
3. `VisitorService`: свободное устройство не сканируется, когда посетителя нет.
4. `Visitor` и `GemsPurchase`: удалены пустые `Start/Update`.
5. `BarrierDissolve`: вместо инстанцирования массива всех материалов создаётся только нужный material instance; instance явно уничтожается.
6. Технические Graphics мобильного джойстика и LookArea больше не участвуют в UI raycast.
7. Добавлен Editor-валидатор `Tools/CyberClub/Validate Main Scene` для Missing Script, broken reference и критических Inspector-связей.
8. Удалена единственная обнаруженная broken reference на отсутствующий LightmapParameters.

## 8. Сравнение до/после

| Участок | До | После | Численный runtime-эффект |
|---|---|---|---|
| Выбор свободного устройства | `FindAll` + временный список | два прохода без списка | не измерен |
| Подсчёт spawn points | `FindAll` + временный список | цикл без списка | не измерен |
| Нет посетителя у admin | всё равно сканировались устройства | ранний `continue` | не измерен |
| Visitor/GemsPurchase | 2 пустых Update | 0 пустых Update | не измерен |
| BarrierDissolve | `renderer.materials` инстанцировал все slots | один material instance + явный Destroy | не измерен |
| LookArea/joystick overlay | технический UI участвовал в raycast | `Raycast Target = Off` | функциональная правка; не измерен |

FPS, CPU/GPU ms, GC Alloc/frame и память до/после не указаны, поскольку игровой Profiler capture не выполнялся.

## 9. Что требует реального устройства

- Одновременные два пальца: joystick + look, включая потерю touch и системные края.
- World Space purchase/repair buttons и ScrollView без вращения камеры.
- Trackpad MacBook против обычной мыши при 30/60 FPS.
- First Person и Third Person с одинаковым Vector2 движения.
- Тепловой throttling и рост памяти за 10–15 минут.
- WebGL каталог/покупка внутри iframe Яндекс Игр, отключённая сеть, timeout и Retry.
- GPU time, overdraw, Bloom/HDR и тени на типичном слабом телефоне.

## 10. Чек-лист повторного теста

- [ ] Unity batchmode compilation завершается с кодом 0.
- [ ] `Tools/CyberClub/Validate Main Scene`: Missing Script = 0, broken reference = 0.
- [ ] Development WebGL: CPU Timeline, Rendering, GC Alloc и Memory записаны без Deep Profile.
- [ ] Build Report сохранён; отмечены top textures/audio/shaders/managed assemblies.
- [ ] Пустой клуб и максимум посетителей записаны отдельными Profiler capture.
- [ ] Frame Debugger проверен для обычного вида и вида сверху.
- [ ] Магазин/ScrollView не вызывает постоянных Canvas rebuild и TMP rebuild.
- [ ] Object pooling рассматривается только после подтверждённого пика Instantiate/Destroy.
- [ ] Mobile/PC/WebGL quality сравниваются одной камерой и одним сценарием.
- [ ] Релизная сборка не Development, без Autoconnect Profiler и debug symbols.
- [ ] На телефоне завершена 10–15-минутная сессия с фиксацией памяти/температуры.
