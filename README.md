# Free Look

Look around while you walk. Hold **Left Alt** and the camera swings away from the direction you are travelling without turning your character, so you can watch a wolf on your flank or take in the view while still walking a straight line. Let go and the view swings smoothly back to where you are heading.

## Requirements

- The Long Dark (built against 2.55)
- [MelonLoader](https://melonwiki.xyz/)
- [ModSettings](https://github.com/DigitalzombieTLD/ModSettings) — optional. Without it the mod still works on its defaults; with it you get an in-game settings page.

## Installation

Drop `FreeLook.dll` into the game's `Mods` folder.

## Settings

Configurable in-game under **Mod Settings → Free Look**:

| Setting                            | Default  | What it does                                                                                                          |
| ---------------------------------- | -------- | --------------------------------------------------------------------------------------------------------------------- |
| Enable free look                   | On       | Turn the whole feature off and restore stock camera behaviour.                                                        |
| Free look key                      | Left Alt | The key held to look around. A mouse button works too.                                                                |
| Toggle instead of hold             | Off      | Tap to enter free look and tap again to leave, rather than holding.                                                   |
| Double tap to latch                | Off      | Holding still works, but a quick double tap latches free look on until you press again.                               |
| Look range                         | 155°     | How far the view may swing from your direction of travel, to each side.                                               |
| Return time                        | 150 ms   | How long the view takes to swing back once released. Zero snaps instantly.                                            |
| Turn to face your aim              | On       | Raising a weapon while looking around turns you to face where you were looking, rather than swinging your view back.  |
| Disable while aiming               | On       | Suppress free look while a weapon is raised, so your aim is never pointed somewhere you are not looking.              |
| Show held item while looking       | Off      | Keep whatever is in your hands on screen while looking around. Expect visible cut-off edges — see below.              |
| Hide beyond                        | 180°     | Used only when the held item is shown: how far you may look before it is hidden after all. At 180 it is never hidden. |
| No free look with an item equipped | Off      | Stand down entirely whenever something is in your hands.                                                              |
| No free look while crouched        | Off      | Stand down while crouched.                                                                                            |

## Notes

- Your first-person arms, clothing and held item are hidden while you look around, and restored when you release. The game only ever builds them to be seen from straight ahead, so from any other angle they show cut-off edges. **Show held item while looking** keeps your held tool or weapon on screen anyway, and **Hide beyond** sets how far round you may look before it is hidden after all.
- When you let go, your view swings back to the way you are heading — both the sideways turn and any up or down you added along the way.
- Your character does not turn while free look is held, so movement continues in the direction you were already facing.
- A latched free look is released whenever the game takes control: opening your inventory, the map or the pause menu, sleeping, or any scripted sequence.

## Credits

Idea: **Zaknafein** — https://youtu.be/KTwYDL6Oq-s?t=2702
