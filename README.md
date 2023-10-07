# Chos5555Bot
A gaming server organization discord bot made with [Discord.Net](https://github.com/discord-net/Discord.Net), data saved in a PostgreSQL database. Deployed on Heroku.
## Features
- ### Manage multiple Discord servers
- ### Rule channel with reaction detection
- ### Games
  - Each game has it's own channels
  - Has a selection message in the game selection channel
  - User selects only games he wants
  - Option for an active role for games with playing seasons, active role requests have to be accepted by game moderators for security
- ### Activity tracking for games
  - Count hours that a user has been in a games room and reward them for being active with a eg. Veteran role
- ### Quests for games
  - Game mods can create quests for players to complete
  - Once player takes quests, it's unavailable for others, quest gets reset if not completed
  - Quests grant points, a leadeboard of players can be accessed and awarded accordingly
- ### Stage room
  - Stage room that let's only people with specified role to speak
  - Channel where you can ask for speaking priviledges (has to be accepted by speaker)
- ### Archiving old channels on deletion
- ### Send message on user leave
- ### Modify bots old messages
- ### Auto deploy updates on Heroku
