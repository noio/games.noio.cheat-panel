using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace noio.RuntimeTools
{
    internal class RuntimeToggle : RuntimeToolItem
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField] TMP_Text _valueLabel;

        #endregion

        Button _button;
        MemberInfo _memberInfo;
        PropertyInfo _property;
        Object _targetObject;

        #region PROPERTIES

        bool PropertyValue
        {
            get => (bool)_property.GetValue(_targetObject);
            set
            {
                _property.SetValue(_targetObject, value);
                SetValueLabel();
            }
        }

        #endregion

        #region MONOBEHAVIOUR METHODS

        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Execute);
        }

        #endregion

        public void Init(Object targetObject, PropertyInfo property)
        {
            _targetObject = targetObject;
            _property = property;

            SetValueLabel();
        }

        void SetValueLabel()
        {
            _valueLabel.text = PropertyValue ? "ON" : "OFF";
        }

        protected override void Execute()
        {
            PropertyValue = !PropertyValue;
        }

        protected override void ExecuteAlt()
        {
            Execute();
        }
    }
}