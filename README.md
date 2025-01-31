# Celeste Input History

![Example screenshot](screenshot.png)

This mod displays a history of inputs on the left hand side along with how long
each was pressed for (in frames). It currently uses TAS notation to display the
buttons (J = jump, X = dash, Z = crouch dash, G = grab).

It also can record TAS replays of levels (disabled by default). The replays are
stored in the `InputHistoryReplays` folder in the main Celeste directory. There
are a few limitations around multiple inputs that might cause it to desync, but
it should work if you only use at most two jump binds and one dash bind.

## TODO

These are things I might want to add in.

- Don't block the left side when Madeline is there (e.g. make it transparent or
move it to the right side).
- Make the buttons shown configurable. I didn't want to use the players key
bindings because the keys aren't all uniform width (e.g. space is wider).
- Show if inputs have been buffered or had no effect.
- Show screen transitions as a separate event (for help with transition boosts
or bubsdrop).