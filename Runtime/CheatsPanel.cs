// (C)2024 @noio_games
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

namespace noio.CheatPanel
{
public class CheatsPanel : MonoBehaviour
{
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
    readonly List<CheatItem> _currentItems = new();
    readonly List<CheatCategory> _categories = new();
    readonly List<Page> _pages = new();
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
        var firstPage = CreatePage(_bindToObjects);
        OpenPage(firstPage);
    }

    void OnEnable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= HandleTextInput;
            Keyboard.current.onTextInput += HandleTextInput;
        }
    }

    void OnDisable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= HandleTextInput;
        }
    }

    void Update()
    {
        _updateTimer += Time.deltaTime;
        if (_updateTimer >= 2)
        {
            _updateTimer -= 2;
            foreach (var item in _currentItems)
            {
                item.RefreshLabel();
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

    Page CreatePage(GameObject[] fromObjects)
    {
        var page = new Page();

        foreach (var go in fromObjects)
        {
            foreach (var component in go.GetComponents<Component>())
            {
                var bindings = CreateBindingsFromAttributes(component);
                page.Bindings.AddRange(bindings);
            }
        }

        AssignHotkeys(page);
        _pages.Add(page);
        return page;
    }

    Page CreatePage(IEnumerable<CheatBinding> bindings)
    {
        var page = new Page();
        page.Bindings.AddRange(bindings);

        AssignHotkeys(page);
        _pages.Add(page);
        return page;
    }

    void OpenPage(Page page, bool pushPreviousToStack = true)
    {
        if (_currentPage != null && pushPreviousToStack)
        {
            _pageStack.Push(_currentPage);
        }

        _currentPage = page;

        for (var i = _contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_contentParent.GetChild(i).gameObject);
        }

        _currentItems.Clear();
        _categories.Clear();

        InstantiateUI(page);
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

    IEnumerable<CheatBinding> CreateBindingsFromAttributes(Component component)
    {
        var cheatBindingEnumerableType = typeof(IEnumerable<CheatBinding>);

        var type = component.GetType();
        foreach (var member in type.GetMembers())
        {
            if (member.GetCustomAttribute(typeof(CheatAttribute), false) is
                CheatAttribute cheatAttribute)
            {
                GetTitleAndCategory(cheatAttribute, member, component,
                    out var title, out var category);

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
                            yield return new CheatActionBinding(title,
                                () => method.Invoke(component, null))
                            {
                                PreferredHotkeys = cheatAttribute.PreferredHotkeys,
                                Category = category
                            };
                        }
                        else if (cheatBindingEnumerableType.IsAssignableFrom(method.ReturnType))
                        {
                            if (cheatAttribute.NewPage == false)
                            {
                                /*
                                 * Just add sub-bindings to the current page
                                 */
                                var bindings = (IEnumerable<CheatBinding>)method.Invoke(component, null);
                                foreach (var binding in bindings)
                                {
                                    yield return binding;
                                }
                            }
                            else
                            {
                                var bindings =
                                    (IEnumerable<CheatBinding>)method.Invoke(component, null);
                                var page = CreatePage(bindings);

                                var openPageBinding =
                                    new CheatOpenPageBinding(title, () => { OpenPage(page); })
                                    {
                                        PreferredHotkeys = cheatAttribute.PreferredHotkeys,
                                        Category = category
                                    };

                                yield return openPageBinding;
                            }
                        }

                        break;
                    }
                    case PropertyInfo property:
                    {
                        if (property.PropertyType == typeof(float))
                        {
                            yield return new CheatFloatBinding(title,
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
                            yield return new CheatBoolBinding(title,
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

    void InstantiateUI(Page page)
    {
        foreach (var binding in page.Bindings.OrderBy(b => b.Label))
        {
            switch (binding)
            {
                case CheatBinding<float> floatBinding:
                    InstantiateItem(_sliderPrefab, floatBinding);
                    break;
                case CheatBinding<bool> boolBinding:
                    InstantiateItem(_togglePrefab, boolBinding);
                    break;
                case CheatActionBinding actionBinding:
                    InstantiateItem(_buttonPrefab, actionBinding);
                    break;
            }
        }
    }

    CheatCategory InstantiateCategory(string title)
    {
        var category = Instantiate(_categoryPrefab, _contentParent);
        category.Title = title;

        return category;
    }

    static void GetTitleAndCategory(
        CheatAttribute attribute,
        MemberInfo memberInfo,
        Component component,
        out string title,
        out string category
    )
    {
        title = string.IsNullOrEmpty(attribute.Title)
            ? NicifyVariableName(memberInfo.Name)
            : attribute.Title;
        category = string.IsNullOrEmpty(attribute.Category)
            ? NicifyVariableName(component.name)
            : attribute.Category;
    }

    T InstantiateItem<T>(T prefab, CheatBinding binding) where T : CheatItem
    {
        var category = _categories.FirstOrDefault(c => c.Title == binding.Category);
        if (category == null)
        {
            category = InstantiateCategory(binding.Category);
            _categories.Add(category);
        }

        var item = Instantiate(prefab, category.ContentParent);
        item.Initialize(binding, this);

        // item.HueTint = (member.DeclaringType.Name.GetHashCode() / (float)int.MaxValue) * .5f + .5f;

        item.name = binding.Label;
        item.RefreshLabel();

        _currentItems.Add(item);
        return item;
    }

    void AssignHotkeys(Page page)
    {
        var occupiedHotkeys = "";
        /*
         * Start with those items that have set a Preferred Hotkey.
         */
        var orderedBindings = page.Bindings.OrderByDescending(item => item.HotkeyPrioritySortingKey);
        foreach (var binding in orderedBindings)
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
        if (_currentItems.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(_currentItems[0].gameObject);
        }
    }

    #region EVENT HANDLERS

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
            var item = _currentItems.FirstOrDefault(a => a.Binding.Hotkey == char.ToUpper(inputChar));
            if (item != null)
            {
                Debug.Log(
                    $"F{Time.frameCount} Run Hotkey \"{char.ToUpper(inputChar)}\": {item.Binding.Label}");

                item.Binding.Execute(Keyboard.current.shiftKey.isPressed);

                if (_mode == Mode.Invisible && item.Binding is not CheatOpenPageBinding)
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

    public List<CheatBinding> Bindings { get; } = new();

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