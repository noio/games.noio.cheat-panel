using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace noio.RuntimeTools
{
    internal class RuntimeButton : RuntimeToolItem
    {
        Button _button;
        Object _targetObject;
        MethodInfo _method;

        #region MONOBEHAVIOUR METHODS

        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Execute);
        }

        #endregion

        public void Init(Object targetObject, MethodInfo method)
        {
            _targetObject = targetObject;
            _method = method;
        }

        protected override void Execute()
        {
            _method.Invoke(_targetObject, null);
        }

        protected override void ExecuteAlt()
        {
            Execute();
        }
    }
}