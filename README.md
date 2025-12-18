# Neuro SDK for C# / MonoGame

## This is not meant to be used with unity.
This SDK is not intended for use with Unity or Unity game modding. The [official Unity SDK](https://github.com/VedalAI/neuro-game-sdk/tree/main/Unity) should be used for those purposes.


## Overview
This is an implementation of the [Neuro Game API](https://github.com/VedalAI/neuro-game-sdk) for C# and MonoGame, allowing for C# support with more than just unity.

This has only been tested with MonoGame (an example can be found in the `MonoGame` branch) and has not been fully integrated into a game, so changes may be made to the project.

While it should be able to be used with other frameworks or engines out of the box, it still may require changes for best results. If you need to make changes for a specific framework or engine that is not MonoGame making your own fork of the main branch is recommended. For more general improvements or MonoGame specific changes, please consider opening a pull request to the respective branch.


This implementation is heavily based on the [Unity SDK](https://github.com/VedalAI/neuro-game-sdk/tree/main/Unity) from the original [Neuro Game SDK](https://github.com/VedalAI/neuro-game-sdk).

## Installation
There is currently no way to install this through NuGet and there are no plans too currently. To use this, you either have to, build it yourself or use a precompiled dll from the releases tab (The main branch is "V" whereas the Mono Game version is "MG-V"). The first option is recommended.
### Dependencies
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)
#### MonoGame branch:
- [MonoGame 3.8.4](https://www.nuget.org/packages/MonoGame.Framework.DesktopGL/3.8.4)

Other versions of MonoGame may work, they have not been tested though.
