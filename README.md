# Free Look

Look around while you walk. Hold **Left Alt** and the camera swings away from the direction you are travelling without turning your character, so you can watch a wolf on your flank or take in the view while still walking a straight line. Let go and the view swings smoothly back to where you are heading.

## Requirements

- The Long Dark (built against 2.55)
- [MelonLoader](https://melonwiki.xyz/)
- [ModSettings](https://github.com/DigitalzombieTLD/ModSettings) - optional. Without it the mod still works on its defaults; with it you get an in-game settings page.

## Installation

Drop `FreeLook.dll` into the game's `Mods` folder.

## Settings

Configurable in-game under **Mod Settings → Free Look**:

| Setting                            | Default  | What it does                                                                                                          |
| ---------------------------------- | -------- | --------------------------------------------------------------------------------------------------------------------- |
| Enable free look                   | On       | Turn the whole feature off and restore stock camera behaviour.                                                        |
| Free look key                      | Left Alt | The key held to look around. A mouse button works too.                                                                |
| Toggle instead of hold             | Off      | Tap to enter free look and tap again to leave, rather than holding.                                                   |
| Double tap to latch                | Off      | Holding still works, but a quick double tap latches free look on, and another releases it.                               |
| Look range                         | 155°     | How far the view may swing from your direction of travel, to each side.                                               |
| Return time                        | 150 ms   | How long the view takes to swing back once released. Zero snaps instantly.                                            |
| Turn to face your aim              | On       | Raising a weapon while looking around turns you to face where you were looking, rather than swinging your view back.  |
| Disable while aiming               | On       | Suppress free look while a weapon is raised, so your aim is never pointed somewhere you are not looking.              |
| Show held item while looking       | Off      | Keep whatever is in your hands on screen while looking around. Expect visible cut-off edges - see below.              |
| Hide beyond                        | 180°     | Used only when the held item is shown: how far you may look before it is hidden after all. At 180 it is never hidden. |
| No free look with an item equipped | Off      | Stand down entirely whenever something is in your hands.                                                              |
| No free look while crouched        | Off      | Stand down while crouched.                                                                                            |

### The indicator

If you turn on **Toggle instead of hold** or **Double tap to latch**, free look can be latched on, and a latch outlives the key press. That is the state worth announcing, so there is a small icon on the interface to say so, sitting by default beside the stamina meter in the bottom right.

**On the default hold-only binding it never appears**, which is deliberate: a key under your finger is not one you forget. Set **Show an icon** to Whenever looking if you want it on the hold as well.

The choices are Never, While latched, or Whenever looking. Its corner, position, size and opacity are all adjustable, and it keeps the same place on any monitor and aspect ratio.

It is built from the game's own interface art and lives inside the game's own interface, so it hides with everything else in menus, on the map, during cutscenes, and in screenshots taken with F10.

## Notes

- Your first-person arms, clothing and held item are hidden while you look around, and restored when you release. The game only ever builds them to be seen from straight ahead, so from any other angle they show cut-off edges. **Show held item while looking** keeps your held tool or weapon on screen anyway, and **Hide beyond** sets how far round you may look before it is hidden after all.
- When you let go, your view swings back to the way you are heading - both the sideways turn and any up or down you added along the way.
- Your character does not turn while free look is held, so movement continues in the direction you were already facing.
- You can still interact with whatever you are looking at, so a locker or a bed off to one side can be used without cancelling free look first.
- A latched free look is released whenever the game takes control: opening your inventory, the map or the pause menu, sleeping, or any scripted sequence.

## Compatibility with other mods

Free look stands down on its own whenever something else takes the camera, and picks up again afterwards. It does not need to know which mod did it: it watches for the camera behaving in ways that only happen when it is no longer yours, so this holds for tools that did not exist when this mod was written.

[RecordingUtils](https://github.com/moosemeat817/RecordingUtils) by moosemeat817 is tested and works. While its FreeBird camera is flying, free look is completely inert, so your shot is exactly what FreeBird gives you, and leaving FreeBird restores normal behaviour with nothing left behind. Expect the same of other recording and free-camera tools, and please report it if you find one that misbehaves.

The developer fly mode is handled too, with one rough edge: between entering fly mode and first moving the camera, free look can still engage, because nothing has yet happened that distinguishes flying from standing still. It releases as soon as you move.

There are no known conflicts. Free look acts at a single point, the one where the game has already decided you are allowed to look, so anything upstream of that decision keeps its say.

## Credits

Idea: **Zaknafein** - [Road to 500 Days - Part 100](https://youtu.be/KTwYDL6Oq-s?t=2702)
