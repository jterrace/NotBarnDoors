# NotBarnDoors

Were you born in a barn? Close the damn door!

## Functionality

### Pull to open

If you have access to open a door, the mod adds the ability to pull open the
door. Normally, doors always open away from you, but by pressing alternative
interact on a door (default Shift+E on keyboard), you can pull the door open
towards you.

### Auto Close Time

For player made doors that you have access to, you can use Ctrl+E to set an
automatic closing time, in seconds. When the door is opened, it will be
automatically closed after that amount of time. The default value is 0, which
disables the auto closing functionality.

## Testing Scenarios

- Tested on all types of door (wooden door, wooden gate, iron gate, etc.)
- All functionality is disabled on doors that needs keys, like crypts and the
  queen's door.
- Tested on warded doors: functionality is disabled if you don't have access to
  the warded door and otherwise operates as normal if you do.
- Works with multiplayer scenarios as long as everyone has the mod installed.
- Tested when a door is opened when one player is the "zone owner" and then
  disconnects or leaves the area, leaving another player as the owner.
- Tested when a door is opened and everyone leaves the zone. The next time
  someone logs in or zones in near the door, the timer will start.
