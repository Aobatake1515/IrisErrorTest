# IrisErrorTest
Minimal case of Unreal Engine error log: "Disallowed to write first packet in batch, with Iris this is not good!"

This happens often in projects with Iris and push model networking enabled and it's unclear if this log means that something is going wrong or not.

# Repro Steps
1. Clone repro and set up with engine version 5.5 (I used default 5.5.4). All this project does is enable Iris and push model in a simple empty project.
2. In `Editor Preferences -> Level Editor -> Play -> Multiplayer Options` set Enable Network Emulation to true and set incoming traffic minimum latency to 100. In a project with more initialization, the error may occur even without manually adding latency like this.
4. In an empty level, play in PIE as client. This works with or without PIE under one client.
5. This will cause the Iris error "Disallowed to write first packet in batch, with Iris this is not good!" to show up in the logs. The logs can be accessed at `IrisErrorExample/Saved/Logs`

# Likely Cause
1. The error happens when `UDataStreamChannel::Tick` is called but `IsNetReady` fails. This happens because the `UNetConnection` has a send buffer with more bits than its `QueuedBits`.
2. `UNetConnection::Tick` typically updates the net connection's `QueuedBits` to a reasonable amount that should be higher than the send buffer. If we wanted to ensure that `QueuedBits` will be more than the send buffer, we could set the net speed to an arbitrarily large number by setting the config values for ConfiguredInternetSpeed and ConfiguredLanSpeed, but that doesn't prevent the issue.
3. The culprit is most likely `World.cpp` lines 6383 and 6743 which get the connection and set `QueuedBits` to 0. The specific line to search for is `Connection->QueuedBits = 0;`. When it sets QueuedBits to 0 this means `IsNetReady` will fail if the send buffer has any bits in it.

It's unclear what the intent is, but it seems like it intends to prevent throttling on certain events (`UWorld::WelcomePlayer` and `UWorld::NotifyControlMessage` with message `NMT_Join`). These are the provided comments:
>`don't count initial join data for netspeed throttling as it's unnecessary, since connection won't be fully open until it all gets received, and this prevents later gameplay data from being delayed to "catch up"`

>`// @TODO FIXME - TEMP HACK? - clear queue on join`.

It seems like the intent may be to make `IsNetReady` always true regardless of the buffer size to prevent throttling. This would make sense in the context of it happening when the player joins. However this does the opposite and makes `IsNetReady` false instead.
# Reprocussions
So far we haven't seen any issues that could be traced back to this error, but we can't be sure that this error doesn't lead to any adverse effects.
# Potential Fix
Chainging `World.cpp` lines 6383 and 6743 to set `QueuedBits` to a large negative number would prevent this issue and allow for sending a large send buffer immediately after a player joins, but it's unclear if this is the correct fix without knowing the intent of the original code here.
