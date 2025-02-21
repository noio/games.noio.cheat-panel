// (C)2025 @noio_games
// Thomas van den Berg

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using noio.CheatPanel.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace noio.CheatPanel
{
public class CheatsPanel : MonoBehaviour
{
    const string HomePageTitle = "Home";
    static CheatsPanel _instance;

    #region SERIALIZED FIELDS

    [Tooltip("The action that activates the debug panel for the first time.")]
    [SerializeField]
    InputActionReference _activateAction;

    [Tooltip("After the debug panel is activated, this key toggles it invisible, but it stays enabled, " +
             "so that hotkeys still work.")]
    [SerializeField]
    InputActionReference _toggleAction;

    [SerializeField] Mode _initialModeInEditor = Mode.Invisible;
    [SerializeField] Mode _initialModeInDevelopmentBuild = Mode.Disabled;
    [SerializeField] Mode _initialModeInReleaseBuild = Mode.PermanentlyRemoved;
    [SerializeField] string _hotkeys = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    [SerializeField] string _excludedHotkeys = "WASD";
    [SerializeField] GameObject[] _bindToObjects;

    [Header("Internal")] //
    [SerializeField]
    RectTransform _contentParent;

    [SerializeField] CheatButton _buttonPrefab;
    [SerializeField] CheatSlider _sliderPrefab;
    [SerializeField] CheatToggle _togglePrefab;
    [SerializeField] CheatCategory _categoryPrefab;

    #endregion

    /*
     * Build Fields:
     */
    // readonly List<CheatUIElementBase> _currentItems = new();
    readonly Dictionary<string, Page> _pages = new();
    readonly Stack<Page> _pageStack = new();
    Page _currentPage;
    Canvas _canvas;

    /*
     * Runtime Fields
     */
    int _lastExecutedOncePerFrameAction;
    bool _isQuitting;
    Mode _mode;
    bool _originalCursorVisible;
    CursorLockMode _originalCursorLockState;
    float _updateTimer;
    Vector2 _canvasSize;

    #region PROPERTIES

    public static bool IsOpen => _instance != null && _instance._mode == Mode.Open;

    #endregion

    #region MONOBEHAVIOUR METHODS

    void Awake()
    {
        _isQuitting = false;
        Application.quitting -= HandleApplicationQuit;
        Application.quitting += HandleApplicationQuit;

        _instance = this;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanges;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanges;
#endif

        if (_activateAction != null && _activateAction.action != null)
        {
            _activateAction.action.Enable();
            _activateAction.action.performed -= HandleActivate;
            _activateAction.action.performed += HandleActivate;
        }

        if (_toggleAction != null && _toggleAction.action != null)
        {
            _toggleAction.action.Enable();
            _toggleAction.action.performed -= HandleToggle;
            _toggleAction.action.performed += HandleToggle;
        }

        _canvas = GetComponentInChildren<Canvas>(true);

        _originalCursorVisible = Cursor.visible;
        _originalCursorLockState = Cursor.lockState;

#if UNITY_EDITOR
        SetMode(_initialModeInEditor);
#elif DEVELOPMENT_BUILD
            SetMode(_initialModeInDevelopmentBuild);
#else
            SetMode(_initialModeInReleaseBuild);
#endif
    }

    void OnDestroy()
    {
        if (_activateAction != null && _activateAction.action != null)
        {
            _activateAction.action.performed -= HandleActivate;
        }

        if (_toggleAction != null && _toggleAction.action != null)
        {
            _toggleAction.action.performed -= HandleToggle;
        }
    }

    void Start()
    {
        var homePage = new Page(HomePageTitle);
        _pages.Add(HomePageTitle, homePage);

        foreach (var go in _bindToObjects)
        {
            if (go != null)
            {
                foreach (var component in go.GetComponents<Component>())
                {
                    AddBindings(CreateBindingsForComponent(component));
                }
            }
        }

        /*
         * Check if every page (except home) has a corresponding OpenPage action
         */
        foreach (var page in _pages.Values)
        {
            var pageTitle = page.Title;
            CreateOpenPageBinding(pageTitle);
        }

        foreach (var page in _pages.Values)
        {
            SortAndAssignHotkeys(page);
        }

        OpenPage(_pages[HomePageTitle]);
    }

    void CreateOpenPageBinding(string pageTitle, string preferredHotkeys = null)
    {
        if (pageTitle == HomePageTitle)
        {
            return;
        }

        var homePage = _pages[HomePageTitle];
        var openPageBinding = homePage.Bindings.FirstOrDefault(b =>
            b is CheatOpenPageBinding openPageBinding && openPageBinding.OpenPageWithTitle == pageTitle);

        if (openPageBinding == null)
        {
            homePage.Bindings.Add(new CheatOpenPageBinding(pageTitle, () => OpenPage(pageTitle))
            {
                Category = "_Pages",
                PreferredHotkeys = preferredHotkeys
            });
        }
    }

    void OnEnable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= HandleTextInput;
            Keyboard.current.onTextInput += HandleTextInput;
        }

        Canvas.preWillRenderCanvases += HandlePreWillRenderCanvases;
    }

    void OnDisable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= HandleTextInput;
        }

        Canvas.preWillRenderCanvases -= HandlePreWillRenderCanvases;
    }

    void Update()
    {
        _updateTimer += Time.deltaTime;
        if (_updateTimer >= 2)
        {
            _updateTimer -= 2;
            foreach (var binding in _currentPage.Bindings)
            {
                binding.RefreshLabel();
            }
        }
    }

    #endregion

    public static void Hide()
    {
        if (_instance && _instance._mode == Mode.Open)
        {
            _instance.SetMode(Mode.Invisible);
        }
    }

    public void OnCancelAction()
    {
        ClosePageOrPanel();
    }

    void SetMode(Mode newMode)
    {
        if (_mode != newMode)
        {
            switch (newMode)
            {
                case Mode.PermanentlyRemoved:
                    Destroy(gameObject);
                    break;

                case Mode.Disabled:
                    Cursor.visible = _originalCursorVisible;
                    Cursor.lockState = _originalCursorLockState;
                    gameObject.SetActive(false);
                    break;

                case Mode.Open:
                    _originalCursorVisible = Cursor.visible;
                    _originalCursorLockState = Cursor.lockState;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;

                    _canvas.gameObject.SetActive(true);
                    gameObject.SetActive(true);
                    UpdateCategoryGridHeights();
                    SelectFirstButton();
                    break;

                case Mode.Invisible:
                    Cursor.visible = _originalCursorVisible;
                    Cursor.lockState = _originalCursorLockState;
                    gameObject.SetActive(true);
                    _canvas.gameObject.SetActive(false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(newMode), newMode, null);
            }

            _mode = newMode;
        }
    }

#if UNITY_EDITOR

    #region EDITOR

    void HandlePlayModeStateChanges(PlayModeStateChange playModeStateChange)
    {
        if (playModeStateChange == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanges;
            HandleApplicationQuit();
        }
    }

    #endregion

#endif

    void HandleApplicationQuit()
    {
        Application.quitting -= HandleApplicationQuit;
        _isQuitting = true;
    }

    /// <summary>
    /// When calling this, the bindings must have pages and categories set.
    /// </summary>
    /// <param name="bindings"></param>
    void AddBindings(IEnumerable<CheatBinding> bindings)
    {
        foreach (var binding in bindings)
        {
            var pageName = binding.Page;
            if (string.IsNullOrEmpty(pageName))
            {
                pageName = HomePageTitle;
            }

            if (_pages.TryGetValue(pageName, out var page) == false)
            {
                page = _pages[pageName] = new Page(pageName);
            }

            page.Bindings.Add(binding);
        }
    }

    void OpenPage(string pageTitle, bool pushPreviousToStack = true)
    {
        if (_pages.ContainsKey(pageTitle))
        {
            OpenPage(_pages[pageTitle], pushPreviousToStack);
        }
    }

    void OpenPage(Page page, bool pushPreviousToStack = true)
    {
        SetPageCategoriesActive(_currentPage, false);

        if (_currentPage != null && pushPreviousToStack)
        {
            _pageStack.Push(_currentPage);
        }

        _currentPage = page;

        if (_currentPage.Bindings.Count > 0 && _currentPage.Categories.Count == 0)
        {
            BuildPageUI(_currentPage);
        }

        SetPageCategoriesActive(_currentPage, true);
        UpdateCategoryGridHeights();
    }

    void SetPageCategoriesActive(Page page, bool value)
    {
        if (page == null)
        {
            return;
        }

        foreach (var category in page.Categories)
        {
            category.gameObject.SetActive(value);
        }
    }

    void CloseCurrentPage()
    {
        var previousPage = _pageStack.Pop();
        OpenPage(previousPage, false);
    }

    void GoBackToFirstPage()
    {
        while (_pageStack.Count > 0)
        {
            CloseCurrentPage();
        }
    }

    IEnumerable<CheatBinding> CreateBindingsForComponent(Component component)
    {
        var cheatBindingEnumerableType = typeof(IEnumerable<CheatBinding>);

        var type = component.GetType();
        foreach (var member in type.GetMembers())
        {
            if (member.GetCustomAttribute(typeof(CheatAttribute), false) is
                CheatAttribute cheatAttribute)
            {
                /*
                 * Assign to home page by default, unless title is passed in CheatAttribute
                 */
                var page = string.IsNullOrEmpty(cheatAttribute.Page)
                    ? HomePageTitle
                    : cheatAttribute.Page;

                /*
                 * If no category is passed in CheatAttribute, assign page name,
                 * except on home page: assign component name
                 */
                var category = string.IsNullOrEmpty(cheatAttribute.Category)
                    ? page == HomePageTitle ? component.name : page
                    : cheatAttribute.Category;

                var label = string.IsNullOrEmpty(cheatAttribute.Label)
                    ? NicifyVariableName(member.Name)
                    : cheatAttribute.Label;

                switch (member)
                {
                    case MethodInfo method:
                    {
                        /*
                         * Either an action that we can call, or a method that returns a list
                         * of cheatbindings, let's find out.
                         */
                        if (method.ReturnType == typeof(void))
                        {
                            var labelGetterMethodName = method.Name + "Label";
                            var labelGetterMethod = type.GetMethods().FirstOrDefault(m =>
                                m.Name == labelGetterMethodName && m.ReturnType == typeof(string));

                            yield return new CheatActionBinding(label, () => method.Invoke(component, null))
                            {
                                PreferredHotkeys = cheatAttribute.PreferredHotkeys,
                                Page = page,
                                Category = category,
                                LabelGetter = labelGetterMethod != null
                                    ? () => labelGetterMethod.Invoke(component, null) as string
                                    : null
                            };
                        }
                        else if (cheatBindingEnumerableType.IsAssignableFrom(method.ReturnType))
                        {
                            var bindings = (IEnumerable<CheatBinding>)method.Invoke(component, null);
                            foreach (var binding in bindings)
                            {
                                /*
                                 * Assign Page and Category from the Attribute
                                 * (unless the returned binding overrides it)
                                 */
                                if (string.IsNullOrEmpty(binding.Page))
                                {
                                    binding.Page = page;
                                }

                                if (string.IsNullOrEmpty(binding.Category))
                                {
                                    binding.Category = category;
                                }

                                yield return binding;
                            }

                            if (string.IsNullOrEmpty(cheatAttribute.PreferredHotkeys) == false)
                            {
                                CreateOpenPageBinding(page, cheatAttribute.PreferredHotkeys);
                            }
                        }

                        break;
                    }
                    case PropertyInfo property:
                    {
                        if (property.PropertyType == typeof(float))
                        {
                            yield return new CheatFloatBinding(label,
                                () => (float)property.GetValue(component),
                                value => property.SetValue(component, value))
                            {
                                Min = cheatAttribute.Min,
                                Max = cheatAttribute.Max,
                                PreferredHotkeys = cheatAttribute.PreferredHotkeys,
                                Category = category
                            };
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            yield return new CheatBoolBinding(label,
                                () => (bool)property.GetValue(component),
                                value => property.SetValue(component, value))
                            {
                                PreferredHotkeys = cheatAttribute.PreferredHotkeys,
                                Category = category
                            };
                        }

                        break;
                    }
                }
            }
        }
    }

    void BuildPageUI(Page page)
    {
        foreach (var binding in page.Bindings.OrderBy(b => (b.Category, b.Label)))
        {
            switch (binding)
            {
                case CheatBinding<float> floatBinding:
                    InstantiateUIElement(_sliderPrefab, floatBinding, page);
                    break;
                case CheatBinding<bool> boolBinding:
                    InstantiateUIElement(_togglePrefab, boolBinding, page);
                    break;
                case CheatActionBinding actionBinding:
                    InstantiateUIElement(_buttonPrefab, actionBinding, page);
                    break;
            }
        }

        page.Categories.Sort((c1, c2) => string.Compare(c1.Title, c2.Title, StringComparison.Ordinal));

    }

    T InstantiateUIElement<T>(T prefab, CheatBinding binding, Page page) where T : CheatUIElementBase
    {
        var category = page.Categories.FirstOrDefault(c => c.Title == binding.Category);
        if (category == null)
        {
            category = Instantiate(_categoryPrefab, _contentParent);
            category.Title = binding.Category;
            page.Categories.Add(category);
        }

        var uiElement = Instantiate(prefab, category.ContentParent);
        uiElement.Initialize(binding, this);
        return uiElement;
    }

    void SortAndAssignHotkeys(Page page)
    {
        var occupiedHotkeys = "";
        /*
         * Start with those items that have set a Preferred Hotkey.
         */
        page.Bindings.Sort(((a, b) => a.HotkeyPrioritySortingKey.CompareTo(b.HotkeyPrioritySortingKey)));

        // var orderedBindings = page.Bindings.OrderBy(item => item.HotkeyPrioritySortingKey);
        foreach (var binding in page.Bindings)
        {
            var title = binding.Label.ToUpper();

            var possibleBindings = (binding.PreferredHotkeys + title + _hotkeys).ToUpper();

            var foundBinding = possibleBindings.FirstOrDefault(
                c => _hotkeys.Contains(c) &&
                     _excludedHotkeys.Contains(c) == false &&
                     occupiedHotkeys.Contains(c) == false);

            if (foundBinding != default)
            {
                occupiedHotkeys += foundBinding;
                binding.SetHotkey(foundBinding.ToString()[0]);
            }
        }
    }

    static string NicifyVariableName(string input)
    {
        var output = new StringBuilder();
        var inputArray = input.ToCharArray();
        var startIndex = 0;

        if (inputArray.Length > 1 && inputArray[0] == 'm' && inputArray[1] == '_')
        {
            startIndex += 2;
        }

        if (inputArray.Length > 1 && inputArray[0] == 'k' && inputArray[1] >= 'A' && inputArray[1] <= 'Z')
        {
            startIndex += 1;
        }

        if (inputArray.Length > 0 && char.IsLower(inputArray[0]))
        {
            inputArray[0] = char.ToUpper(inputArray[0]);
        }

        char prevChar = default;
        for (var i = startIndex; i < inputArray.Length; ++i)
        {
            var nextChar = inputArray[i];
            switch (nextChar)
            {
                case '_':
                    output.Append(' ');
                    continue;
                case >= 'A' and <= 'Z' when prevChar is < 'A' or > 'Z':
                    output.Append(' ');
                    break;
            }

            output.Append(nextChar);
            prevChar = nextChar;
        }

        return output.ToString().TrimStart(' ');
    }

    /// <summary>
    ///     Because Activate and Toggle can be the same button
    ///     deduplicate input from those with this hack.
    /// </summary>
    /// <param name="action"></param>
    void OncePerFrame(Action action)
    {
        var frame = Time.frameCount;
        if (_lastExecutedOncePerFrameAction != frame)
        {
            _lastExecutedOncePerFrameAction = frame;
            action();
        }
    }

    void SelectFirstButton()
    {
        EventSystem.current.SetSelectedGameObject(_currentPage.Categories[0]
                                                              .GetComponentInChildren<Selectable>()
                                                              .gameObject);
    }

    void UpdateCategoryGridHeights()
    {
        if (_currentPage != null)
        {
            foreach (var category in _currentPage.Categories)
            {
                category.UpdateGridHeight();
            }
        }
    }

    #region EVENT HANDLERS

    void HandlePreWillRenderCanvases()
    {
        var newRes = _canvas.renderingDisplaySize;
        if (Math.Abs(newRes.x - _canvasSize.x) > .5f || Math.Abs(newRes.y - _canvasSize.y) > .5f)
        {
            _canvasSize = newRes;

            UpdateCategoryGridHeights();
        }
    }

    void HandleTextInput(char inputChar)
    {
        /*
         * Don't do debug actions in frame one because that could be from CMD+P (play)
         */
        if (Application.isFocused &&
            _isQuitting == false &&
            _mode != Mode.Disabled &&
            Time.frameCount > 1)
        {
            /*
             * We really shouldn't rely on the ITEMS (the UI objects) to execute
             * the hotkeys. Ideally we'd just go through the bindings. But for that
             * we'd need to work on the CheatBinding<T> some more to give it an Execute method.
             * But if we do that, we can just cache the Bindings for ALL pages, and we don't have
             * to create the actual UI objects if the panel is closed!
             * Then the "CheatOpenPageBinding" could just point to the path of the page it wants to open
             */
            var binding = _currentPage.Bindings.FirstOrDefault(b => b.Hotkey == char.ToUpper(inputChar));
            if (binding != null)
            {
                Debug.Log(
                    $"F{Time.frameCount} Run Hotkey \"{char.ToUpper(inputChar)}\": {binding.Label}");

                binding.Execute(Keyboard.current.shiftKey.isPressed);

                if (_mode == Mode.Invisible && binding is not CheatOpenPageBinding)
                {
                    GoBackToFirstPage();
                }
            }
        }
    }

    void HandleActivate(InputAction.CallbackContext ctx)
    {
        if (_mode == Mode.Disabled)
        {
            OncePerFrame(() => { SetMode(Mode.Open); });
        }
    }

    void HandleToggle(InputAction.CallbackContext obj)
    {
        OncePerFrame(() =>
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_mode)
            {
                case Mode.Open:
                    ClosePageOrPanel();
                    break;
                case Mode.Invisible:
                    SetMode(Mode.Open);
                    break;
            }
        });
    }

    void ClosePageOrPanel()
    {
        if (_pageStack.Count > 0)
        {
            CloseCurrentPage();
        }
        else
        {
            SetMode(Mode.Invisible);
        }
    }

    #endregion
}

internal class Page
{
    #region PROPERTIES

    public Page(string title)
    {
        Title = title;
    }

    public string Title { get; set; }
    public List<CheatBinding> Bindings { get; } = new();
    public List<CheatCategory> Categories { get; } = new();

    #endregion
}

public enum Mode
{
    NotSet = 0,
    PermanentlyRemoved = 1,
    Disabled = 2,
    Invisible = 3,
    Open = 4
}
}