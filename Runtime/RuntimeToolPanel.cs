using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using noio.RuntimeTools.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace noio.RuntimeTools
{
    public class RuntimeToolPanel : MonoBehaviour
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [Tooltip("The action that activates the debug panel for the first time.")]
        [SerializeField]
        InputActionReference _activateAction;

        [Tooltip("After the debug panel is activated, this key toggles it invisible, but it stays enabled, " +
                 "so that hotkeys still work.")]
        [SerializeField]
        InputActionReference _toggleAction;

        [SerializeField] Mode _initialMode = Mode.Invisible;
        [SerializeField] string _hotkeys = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        [SerializeField] string _excludedHotkeys = "WASD";
        [SerializeField] GameObject _bindToObject;

        [Header("Internal")] //
        [SerializeField]
        RectTransform _itemParent;

        [SerializeField] RuntimeButton _buttonPrefab;
        [SerializeField] RuntimeSlider _sliderPrefab;
        [SerializeField] RuntimeToggle _togglePrefab;

        #endregion

        Canvas _canvas;
        List<RuntimeToolItem> _items;
        int _lastExecutedOncePerFrameAction;
        bool _isQuitting;
        Mode _mode;

        #region MONOBEHAVIOUR METHODS

        void Awake()
        {
            _isQuitting = false;
            Application.quitting -= HandleApplicationQuit;
            Application.quitting += HandleApplicationQuit;

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

            SetMode(_initialMode);
            BuildUI();
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

        void OnEnable()
        {
            Keyboard.current.onTextInput -= HandleTextInput;
            Keyboard.current.onTextInput += HandleTextInput;
        }

        void OnDisable()
        {
            Keyboard.current.onTextInput -= HandleTextInput;
        }

        #endregion

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

        void BuildUI()
        {
            for (var i = _itemParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_itemParent.GetChild(i).gameObject);
            }

            _items = new List<RuntimeToolItem>();

            foreach (var component in _bindToObject.GetComponents<Component>())
            {
                AddUIForObject(component);
            }

            AssignHotkeys();
        }

        void AddUIForObject(Component component)
        {
            var type = component.GetType();

            foreach (var member in type.GetMembers())
            {
                if (member.GetCustomAttribute(typeof(RuntimeButtonAttribute), false) is
                    RuntimeButtonAttribute attribute)
                {
                    if (member is MethodInfo method)
                    {
                        var button = InstantiateItem(_buttonPrefab, method, attribute);
                        button.Init(component, method);
                    }
                }
            }

            foreach (var member in type.GetMembers())
            {
                if (member.GetCustomAttribute(typeof(RuntimeSliderAttribute), false) is
                    RuntimeSliderAttribute sliderAttribute)
                {
                    if (member is PropertyInfo property)
                    {
                        var slider = InstantiateItem(_sliderPrefab, property, sliderAttribute);
                        slider.Init(component, property, sliderAttribute.Min, sliderAttribute.Max);
                    }
                    else
                    {
                        Debug.LogWarning($"{type.Name}.{member.Name} has [RuntimeSlider] attribute " +
                                         "but it's not a float or int property");
                    }
                }

                if (member.GetCustomAttribute(typeof(RuntimeToggleAttribute), false) is
                    RuntimeToggleAttribute toggleAttribute)
                {
                    if (member is PropertyInfo property)
                    {
                        var toggle = InstantiateItem(_togglePrefab, property, toggleAttribute);
                        toggle.Init(component, property);
                    }
                    else
                    {
                        Debug.LogWarning($"{type.Name}.{member.Name} has [RuntimeSlider] attribute " +
                                         "but it's not a float or int property");
                    }
                }
            }
        }

        T InstantiateItem<T>(
            T                    prefab,
            MemberInfo           member,
            RuntimeToolAttribute attribute) where T : RuntimeToolItem
        {
            var item = Instantiate(prefab, _itemParent);

            item.Title = string.IsNullOrEmpty(attribute.Title)
                ? NicifyVariableName(member.Name)
                : attribute.Title;

            item.PreferredHotkeys = attribute.PreferredHotkeys;
            item.name = item.Title;
            _items.Add(item);
            return item;
        }

        void AssignHotkeys()
        {
            var occupiedHotkeys = "";
            /*
             * Start with those items that have set a Preferred Hotkey.
             */
            var orderedItems = _items.OrderBy(item => string.IsNullOrEmpty(item.PreferredHotkeys));
            foreach (var item in orderedItems)
            {
                var title = item.Title.ToUpper();

                var possibleBindings = (item.PreferredHotkeys + title + _hotkeys).ToUpper();

                var foundBinding = possibleBindings.FirstOrDefault(
                    c => _hotkeys.Contains(c) &&
                         _excludedHotkeys.Contains(c) == false &&
                         occupiedHotkeys.Contains(c) == false);

                if (foundBinding != default)
                {
                    occupiedHotkeys += foundBinding;
                    item.Hotkey = foundBinding.ToString()[0];
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

        void SetMode(Mode newMode)
        {
            if (_mode != newMode)
            {
                switch (newMode)
                {
                    case Mode.Disabled:
                        gameObject.SetActive(false);
                        break;

                    case Mode.Enabled:
                        _canvas.gameObject.SetActive(true);
                        gameObject.SetActive(true);
                        SelectFirstButton();
                        break;

                    case Mode.Invisible:
                        gameObject.SetActive(true);
                        _canvas.gameObject.SetActive(false);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(newMode), newMode, null);
                }

                _mode = newMode;
            }
        }

        void SelectFirstButton()
        {
            if (_items.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(_items[0].gameObject);
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
                var item = _items.FirstOrDefault(a => a.Hotkey == char.ToUpper(inputChar));
                if (item != null)
                {
                    Debug.Log($"F{Time.frameCount} Run Hotkey \"{char.ToUpper(inputChar)}\": {item.Title}");
                    item.OnHotkeyUsed(Keyboard.current.shiftKey.isPressed);
                }
            }
        }

        void HandleActivate(InputAction.CallbackContext ctx)
        {
            if (_mode == Mode.Disabled)
            {
                OncePerFrame(() => { SetMode(Mode.Enabled); });
            }
        }

        void HandleToggle(InputAction.CallbackContext obj)
        {
            OncePerFrame(() =>
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (_mode)
                {
                    case Mode.Enabled:
                        SetMode(Mode.Invisible);
                        break;
                    case Mode.Invisible:
                        SetMode(Mode.Enabled);
                        break;
                }
            });
        }

        #endregion
    }

    internal enum Mode
    {
        Disabled,
        Invisible,
        Enabled
    }
}