using System;
using System.Reflection;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace noio.CheatPanel
{
    internal class CheatButton : CheatItem
    {
        Button _button;
        Action _action;
        Object _targetObject;
        MethodInfo _method;

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
            _action.Invoke();
        }

        protected override void ExecuteAlt()
        {
            Execute();
        }
    }
}