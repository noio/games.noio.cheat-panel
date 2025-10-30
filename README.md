# Cheat Panel

A Unity plugin to allow quickly adding 'cheats' or 'debug commands' to a game.

## How to use:

1. Install (use Package Manager "**Install package from git URL...**")
2. Add the **Cheat Panel** prefab to your scene.
3. Create a **new GameObject with a new Component** that will hold your debug actions.
4. Add your newly created object into the Panel's **Bind To Objects** list.
5. On the Panel, configure **Input Actions** to open the panel. (**Activate Action** and **Toggle Action**)
6. On your **new Component**, mark properties and methods with **[Cheat]** and they will automatically be added to the panel.

## Functionality

### Boolean Properties

Add **[Cheat]** to a bool property to create a toggle:

```csharp
[Cheat("P", category: "Gameplay")]
public bool PauseSpawning
{
    get => Main.Battles.SpawningPaused;
    set => Main.Battles.SpawningPaused = value;
}
```

### Methods

Add **[Cheat]** to a method and a button will be created to call that method. If you add a method _with the same name + 'Label'_, that will be used
as a label for the button:

```csharp
[Cheat(category: "Gameplay")]
public void ChangeGameSpeed()
{
    Time.timeScale = Time.timeScale switch
    {
        <= .5f => 1,
        <= 1   => 2,
        <= 2   => 4,
        _      => .5f
    };
}

// THIS IS OPTIONAL:
public string ChangeGameSpeedLabel()
{
    return $"Change Game Speed (x{Time.timeScale})";
}
```

### Advanced Use

To do..

### Common

You do not *have* to add a hotkey. Just adding **[Cheat]** without parameters will automatically assign hotkeys based on property/method names.
