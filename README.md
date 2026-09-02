# Free Look

Look around while you walk. Hold **Left Alt** and the camera swings away from the direction you are traveling without turning your character, so you can watch a wolf on your flank or take in the view while still walking a straight line. Let go and the view swings smoothly back to where you are heading.

## Requirements

- The Long Dark (built against 2.55)
- [MelonLoader](https://melonwiki.xyz/)
- [ModSettings](https://github.com/DigitalzombieTLD/ModSettings) - for the in-game settings page.

## Installation

Drop `FreeLook.dll` into the game's `Mods` folder.

## Settings

Configurable in-game under **Mod Settings → Free Look**:

| Setting                            | Default  | What it does                                                                                                          |
| ---------------------------------- | -------- | --------------------------------------------------------------------------------------------------------------------- |
| Enable free look                   | On       | Turn the whole feature off and restore stock camera behavior.                                                        |
| Free look key                      | Left Alt | The key held to look around. A mouse button works too.                                                                |
| Toggle instead of hold             | Off      | Tap to enter free look and tap again to leave, rather than holding.                                                   |
| Double tap to latch                | Off      | Holding still works, but a quick double tap latches free look on, and another releases it.                            |
| Look range                         | 180°     | How far the view may swing from your direction of travel, to each side. 180 is straight behind you; the slider reaches 270. |
| Return speed                       | 600°/s | How fast the view swings back once released. A quick glance returns promptly, a full swing takes proportionally longer. For scale, a relaxed look around is around 150°/s and the human limit is roughly 800°/s. Zero snaps instantly. |
| Turn to face your aim              | On       | Raising a weapon while looking around turns you to face where you were looking, rather than swinging your view back.  |
| Disable while aiming               | On       | Suppress free look while a weapon is raised, so your aim is never pointed somewhere you are not looking.              |
| Show held item while looking       | Off      | Keep whatever is in your hands on screen while looking around. Expect visible cut-off edges - see below.              |
| Hide beyond                        | 180°     | Used only when the held item is shown: how far you may look before it is hidden after all. At 180 it is never hidden. |
| No free look with an item equipped | Off      | Stand down entirely whenever something is in your hands.                                                              |
| No free look while crouched        | Off      | Stand down while crouched.                                                                                            |

### The indicator

Using any latching setting such as **Toggle instead of hold** or **Double tap to latch**, will cause a small latched indicator icon to appear in the bottom right near the crouching icon area. Its position and size can be adjusted to accommodate other HUD mods.

## Notes

- The first-person arms, clothing, and held item were only ever built to be seen from straight ahead. If you chose to  show them by turning on **Show held item while looking**, be aware they will appear incomplete resulting in cut-off and/or clipping. You can also selectively hide them by changing **Hide beyond** to set how far you may look before they are hidden. Set it to 180 and they are always shown, glitches and all. 
- You can still interact with whatever you are looking at, so a locker or a bed off to one side can be used without canceling free look first.
- A latched free look is released whenever the game takes control: opening your inventory, the map or the pause menu, sleeping, or any scripted sequence.

## Using a controller

Gamepad buttons cannot be chosen in **Free look key**, because the game already binds every one of them. Turn on **Double tap to latch** instead: once you are playing on a controller, the game's own auto-walk control triggers free look as well. On a pad that is the left stick click, pressed without your thumb leaving the movement stick.

Auto-walk is a toggle, so the two presses of a double tap leave it exactly as they found it. Single presses drive auto-walk, pairs drive free look, in any order, without either disturbing the other. Once free look is latched you have movement on one stick and looking on the other, with nothing held down.

Your keyboard key keeps working throughout, and none of this applies until a controller has actually been used, so keyboard play is unaffected.

## Compatibility with other mods

Free look stands down on its own whenever something else takes the camera, and picks up again afterwards. It does not need to know which mod did it: it watches for the camera behaving in ways that only happen when it is no longer yours, so this holds for tools that did not exist when this mod was written. There are no known conflicts. 

## Credits

Idea: **Zaknafein** - [Road to 500 Days - Part 100](https://youtu.be/KTwYDL6Oq-s?t=2702)
