# Free Look

Look around while you walk. Hold **Left Alt** and the camera swings away from the direction you are travelling without turning your character, so you can watch a wolf on your flank, keep an eye on a ridgeline, or just take in the view while still walking a straight line. Let go and the view swings smoothly back to where you are heading.

## Requirements

- The Long Dark (built against 2.55)
- [MelonLoader](https://melonwiki.xyz/)
- [ModSettings](https://github.com/DigitalzombieTLD/ModSettings) — optional. Without it the mod still works on its defaults; with it you get an in-game settings page.

## Installation

Drop `FreeLook.dll` into the game's `Mods` folder.

## Settings

Configurable in-game under **Mod Settings → Free Look**:

| Setting | Default | What it does |
|---|---|---|
| Enable free look | On | Turn the whole feature off and restore stock camera behaviour. |
| Free look key | Left Alt | The key held to look around. A mouse button works too. |
| Toggle instead of hold | Off | Tap to enter free look and tap again to leave, rather than holding. |
| Double tap to latch | Off | Holding still works, but a quick double tap latches free look on until you press again. |
| Look range | 155° | How far the view may swing from your direction of travel, to each side. The default is about as far as you can look while still walking forwards — neck, eyes and a twist from the hips. |
| Return time | 150 ms | How long the view takes to swing back once released. Zero snaps instantly. |
| Turn to face your aim | On | Raising a weapon while looking around turns you to face where you were looking, rather than swinging your view back. |
| Disable while aiming | On | Suppress free look while a weapon is raised, so your aim is never pointed somewhere you are not looking. |
| Show held item while looking | Off | Keep whatever is in your hands on screen while looking around. Expect visible cut-off edges — see below. |
| Hide beyond | 180° | Used only when the held item is shown: how far you may look before it is hidden after all. At 180 it is never hidden. |
| No free look with an item equipped | Off | Stand down entirely whenever something is in your hands. |
| No free look while crouched | Off | Stand down while crouched. |
| Hide beyond | 180° | Used only when the held item is shown: how far you may look before it is hidden after all. At 180 it is never hidden. |
| No free look with an item equipped | Off | Stand down entirely whenever something is in your hands. |
| No free look while crouched | Off | Stand down while crouched. |

### First-person arms

The game only ever builds your arms, worn clothing and held items to be seen from straight ahead, because until now nothing could look at them from any other angle. Free look can — and the camera can even end up inside them.

So by default they are simply hidden while you are looking around, and restored when you release. Nothing is drawn, so nothing can look wrong.

If you would rather keep your tool or weapon on screen, turn on **Show held item while looking** — but know what you are choosing. These models were only ever built for a view pointing straight ahead, so once you turn far enough you start seeing where they were cut off: hollow edges, geometry that stops in mid-air, and surfaces you can see straight through because their backs were never modelled. Look far enough down or to the side and the camera passes inside the mesh entirely.

**Hide beyond** then decides whether it is taken away again once you look far enough round. It defaults to 180°, which never hides — having chosen to see your item, you get to see it. Lower it if you would rather the cutoff were hidden past some angle; where each mesh was cut is the artist's decision, so set it by eye to taste.

Your bare arms always hide immediately regardless, since there is no angle at which a bare arm and torso stub looks right.

## Behaviour notes

- Only the horizontal swing is taken over. Vertical look is left entirely to the game, so its pitch limits, damping and weapon recoil behave exactly as they always did.
- Free look stays out of the way during cutscenes, scripted animations and anything else where the game takes the camera, because it can only act at the point where the game has already decided you are allowed to look.
- Your character does not turn while free look is held, so movement continues in the direction you were already facing.

## Credits

By **Lycanthor**.
