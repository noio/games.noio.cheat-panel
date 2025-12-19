// (C)2023 @noio_games
// Thomas van den Berg

namespace noio.Cheats
{
    public class CheatItemData
    {
        #region PROPERTIES

        public string PreferredHotkeys { get; set; }
        public string Title { get; set; }

        /// <summary>
        ///     Set this to specify an explicit category.
        ///     If you leave this unset, the category is named after the component name
        ///     that the method is on.
        /// </summary>
        public string OverrideCategory { get; set; }

        #endregion

        public void SetTitleIfEmpty(string nicifyVariableName)
        {
            if (string.IsNullOrEmpty(Title))
            {
                Title = nicifyVariableName;
            }
        }
    }
}