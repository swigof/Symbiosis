# Symbiosis

### Project Setup

```
TODO
```

### Building & Packaging

Building is handled by [GameBundle](https://github.com/Ellpeck/GameBundle) using the following command:

```
dotnet gamebundle -wlz -N symbiosis -e Symbiosis.dll -s Symbiosis-Advanced/Symbiosis-Advanced.csproj
```

This creates zip archives of the game for Windows and Linux in the `bin/Bundled` directory.
