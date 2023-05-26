using System.Collections.Generic;

namespace noio.CheatPanel
{
    /// <summary>
    /// Implement this interface to have an object return a dynamic list of Cheat Buttons
    /// This can be useful for e.g. spawning any of a dynamic list of objects.
    /// </summary>
    public interface ICheatButtonProvider
    {
        public IEnumerable<(CheatItemData, System.Action)> GetCheatButtons();
    }
}