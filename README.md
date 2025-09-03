# Symbiosis

A cooperative topdown burrow defence game built with Monogame. 

I had been meaning to do some networked game development for a while before starting this project.
I then found a jam whose theme was 'symbiosis' and discovered the wonderful 
[Backdash](https://delta3.studio/Backdash/)
library, which made for a perfect opportunity to build a peer to peer coop game.

I scoped the project incredibly low knowing I'd run into trouble learning Monogame and making a networked engine for
the first time.

As per usual, the architecture of this project isn't where I'd like it to be.
Lots of things were left behind that I wanted to clean up or improve.

### Setup & Dependencies

Building this project requires the .NET 8.0 SDK which can be downloaded and installed from [here](https://dotnet.microsoft.com/download).
After installing .NET and grabbing the project sources, run `dotnet restore` in the project directory to install the required packages.

Assets for the game can be downloaded from the release page [here](https://github.com/swigof/Symbiosis/releases/download/1.0.0/assets.zip).
Simply unzip the contents of the archive into the project folder.

### Building & Packaging

Building is handled by [GameBundle](https://github.com/Ellpeck/GameBundle) using the following command:

```
dotnet gamebundle -wlz -N symbiosis --mg -e Symbiosis.dll -s Symbiosis-Advanced/Symbiosis-Advanced.csproj
```

This creates zip archives of the game for Windows and Linux in the `bin/Bundled` directory.
