// (C)2024 @noio_games
// Thomas van den Berg

using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace noio.Cheats
{
    internal class CheatButton : CheatUIElementBase
    {
        Button _button;
        CheatActionBinding _binding;

        #region MONOBEHAVIOUR METHODS

        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                Binding.Execute(Keyboard.current.shiftKey.isPressed);
                RefreshLabel();
            });
            
        }

        #endregion

        public void Init(Action action)
        {
        }

        public override void RefreshValue()
        {
            
        }
    }
}