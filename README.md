<p align="center">
    <img src="https://sr.ht/3O-k.png" width="728" />
</p>

A completely [clean-room](https://en.wikipedia.org/wiki/Clean_room_design) implementation of Minecraft beta 1.7.3 (circa September 2011). No decompiled code has been used in the development of this software. This is an **implementation** - not a clone. TrueCraft is compatible with Minecraft beta 1.7.3 clients and servers.

![](https://sr.ht/87Ov.png)

*Screenshot taken with [Eldpack](http://eldpack.com/)*

I miss the old days of Minecraft, when it was a simple game. It was nearly perfect. Most of what Mojang has added since beta 1.7.3 is fluff, life support for a game that was "done" years ago. This is my attempt to get back to the original spirit of Minecraft, before there were things like the End, or all-in-one redstone devices, or village gift shops. A simple sandbox where you can build and explore and fight with your friends. I miss that.

The goal of this project is effectively to fork Minecraft. Your contribution is welcome, but keep in mind that I will mercilessly reject changes that aren't in line with the vision. If you like the new Minecraft, please feel free to keep playing it. If you miss the old Minecraft, join me.

## Compiling

**Use a recursive git clone.**

    git clone --recursive git://github.com/mrj001/TrueCraft.git

From the root directory of the git repository. Then run:

    dotnet build

To compile it and you'll receive binaries in `TrueCraft.Launcher/bin/Debug/net5.0/`. Run `dotnet TrueCraft.Launcher.dll` to run the client and connect to servers and play singleplayer and so on. Run `dotnet TrueCraft.Server.exe` to host a server for others to play on.

Note: TrueCraft requires .NET 5.0 or newer.

Note: I'm doing updates on MacOS, and haven't yet gotten around to Windows or Linux native dependencies.

## Assets

TrueCraft is compatible with Minecraft beta 1.7.3 texture packs. We ship the Pixeludi Pack (by Wojtek Mroczek) by default. You can install the Mojang assets through the TrueCraft launcher if you wish.

## Other Stuff

TrueCraft is not associated with Mojang or Minecraft in any sort of official capacity.
