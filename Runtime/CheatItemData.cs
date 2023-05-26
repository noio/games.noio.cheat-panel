namespace noio.CheatPanel
{
    public class CheatItemData
    {
        public string PreferredHotkeys { get; set; }
        public string Title { get; set; }

        public void SetTitleIfEmpty(string nicifyVariableName)
        {
            if (string.IsNullOrEmpty(Title))
            {
                Title = nicifyVariableName;
            }
        }
    }
}