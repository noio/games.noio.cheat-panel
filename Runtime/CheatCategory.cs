using TMPro;
using UnityEngine;

namespace noio.CheatPanel
{
    internal class CheatCategory : MonoBehaviour
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField] RectTransform _contentParent;
        [SerializeField] TMP_Text _label;

        #endregion

        #region PROPERTIES

        public RectTransform ContentParent => _contentParent;

        public string Title
        {
            get => _label.text;
            set => _label.text = value;
        }

        #endregion
    }
}