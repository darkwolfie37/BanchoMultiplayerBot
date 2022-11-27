## Bancho Multiplayer Bot

### Overview
An osu! multiplayer bot that will maintain a queue and pass the host around every match automatically.

### Player Commands

`!queue`

Displays the current auto host rotate queue.

`!skip`

Allows you to skip your turn as host, or initiate a vote skip for the current host as a non-host player.

`!start [<seconds>]`

Allows the host to start a match start timer, or for the non-host players to vote to start the match.

`!stop`

Stops any ongoing match start timer

`!regulations`

Displays the current map regulations, such as star rating and/or map length.

`!abort`

Aborts the current match, may only be used by match host.

*Notice:* Some commands, such as `!regulations` and `!queue` have shortened versions, like `!r` and `!q` respectively.

### Admin Commands

`!forceskip`

Will skip the current host, without any vote.

`!sethost <name>`

Sets a new host for the round.