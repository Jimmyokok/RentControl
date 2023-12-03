## Disclaimer

- This mod is experimental. It alters core game mechanisms, affects game data and has a short-term impact on saves. It may not be compatible with other mods. Please use at your own risk.
- This mod can be quite balance-breaking if not properly configurated.

## Land Value, Rent & Building Upgrade Control

- **Set a global and configurable upper bound for land value.** Land value of the city will stop rising when hitting this limit. This function can be turned off via a configuration entry.
- Two modifiers are added to control the **rent payment mechanism**:
  - <u>Rent payment factor</u>: controls how much% of the original rent is actually payed by residents and companies. <u>The actually rent payed = original rent * rent payment factor</u>. Storage companies are not affected. Disable this function by setting this factor to 1. The default configuration is 0.25. 
  - <u>Rent upgrade factor</u>: controls how much% of the original rent is actually added to the building upgrade progress. <u>The actually progress added= original rent * rent upgrade factor</u>. Disable this function by setting this factor to 1. The default configuration is 1.
- Two modifiers are added to control the **building upgrade mechanism**:
  - <u>Building upkeep factor</u>: controls how much% of the original building upkeep is actually subtracted from the building upgrade progress. <u>The actually building upkeep = original upkeep * building upkeep factor</u>. Disable this function by setting this factor to 1. The default configuration is 1.
  - <u>Garbage fee factor</u>: controls how much% of the original garbage fee is actually subtracted from the building upgrade progress. <u>The actually garbage fee = original garbage fee * garbage fee factor</u>. Disable this function by setting this factor to 1. The default configuration is 1.

## Background Information

- *Tips about the game's building upgrading mechanism*
  - *The building upgrade progress is the building's <u>landlord</u>.*
  - *Only the rent promotes the building upgrade progress.*
  - *Only the building upkeep and the garbage processing fee reduce this progress.*
  - *When the upgrade progress > the corresponding upgrade cost, the building gets upgraded*
  - *When the upgrade progress < -the corresponding upgrade cost, the building gets abandoned.*
- *Tips about the game's rent mechanism*
  - *The rent value rises with the land value, but not determined by it. Many other factors affects the rent, such as the building level and city services.*
  - *Storage companies are not affected by the rent. Their <u>landlords</u> (upgrade progress) afford their profits and losses.*
  - *How high rent causes abandoned buildings: High rent→renters cannot affort→renters move away→<u>landlords</u> receive insufficient rent→<u>landlords</u> go backrupt→building abandoned.*

## Configuring the Setting

- First launch the game with this mod loaded, and close the game (to generate the configuration file).
- Open the configuration file (..\Cities Skylines II\BepInEx\config\RentControl.cfg), modify each configuration entries and save the file.
- Now launch the game again with this mod loaded and enjoy it!
- Guidelines on configurating different factors:
  - To ensure that citizens/companies can affort their rent: <u>lower</u> land value cap and <u>lower</u> the rent payment factor.
  - To speed up building upgrade/prevent building abandon: <u>Increase</u> the rent upgrade factor, <u>lower</u> the building upkeep factor and <u>lower</u> the garbage fee factor.

## Requirements

- Game version 1.0.15f1.
- BepInEx 5

## Planned Features

- In-game configuration.

## Credits

- [Captain-Of-Coit](https://github.com/Captain-Of-Coit/cities-skylines-2-mod-template): A Cities: Skylines 2 mod template.
- [BepInEx](https://github.com/BepInEx/BepInEx): Unity / XNA game patcher and plugin framework.
- [Harmony](https://github.com/pardeike/Harmony): A library for patching, replacing and decorating .NET and Mono methods during runtime.
- [CSLBBS](https://www.cslbbs.net): A chinese Cities: Skylines 2 community, for extensive test and feedback efforts.
