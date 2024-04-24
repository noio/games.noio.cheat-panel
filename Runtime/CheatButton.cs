// (C)2024 @noio_games
// Thomas van den Berg

using System;
using UnityEngine.UI;

namespace noio.CheatPanel
{
    internal class CheatButton : CheatItem
    {
        Button _button;
        Action _action;
        CheatActionBinding _binding;

        #region MONOBEHAVIOUR METHODS

        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Execute);
        }

        #endregion
        

        public void Init(Action action)
        {
            _action = action;
        }

        protected override void Execute()
        {
            (Binding as CheatActionBinding)?.Action.Invoke();
            _action?.Invoke();
        }

        protected override void ExecuteAlt()
        {
            Execute();
        }
    }
}