!-!-!-!-! !BEFORE YOU DOWNLOAD! !-!-!-!-!
I don't maintain this plugin, nor has it been tested extensively.
It is very slow and has a lot of things done inefficiently. Ping goes from a normal of, say, 50ms to 200ms if you have the plugin loaded.
If you'd like to make a new plugin I'd recommend you look into implementing MCGalaxy's Compiler interface (or something like that) into a plugin that loads native glue code. Use LuaJIT because the libraries used by this 
Use this absolutely at your own risk.
Or just go and write another plugin that works with tcc-JIT instead of Lua and be a hero.
-------

This plugin will let you write MCGalaxy plugins and commands using Lua.

Plugins written in Lua contain many caveats and work-arounds
because of how MCGalaxy's and NLua's APIs are made.

What you would usually use as a reference/pointer is
a C# class because NLua does not support references and needs
"containers". To edit the container values you access the container's
.contained property.

Lua functions are not C# functions and therefore cannot be used to
register event functions. The work-around for that is to define functions
for each plugin. They follow the C# API's naming with the exception of them
using camelCase. So for example, OnLevelSave becomes onLevelSave.

Lua plugin speed compared to C# plugin speed will be slower, however,
the cost of the speed is probably paid off by the time it takes to write
a plugin with Lua's simplicity (and lack of special character spam).

This plugin is also in open testing, it is NOT stable at all.
If you experience any crashes or errors, please, open an issue.

You can also run Lua code directly from chat/messageblocks using /RunLua <code>

--- Installing ---
You will need
  liblua54.so (Assuming you're running a Linux server. You might need another file)
  KeraLua.dll (net46) https://www.nuget.org/packages/KeraLua/
  NLua.dll (net46) https://www.nuget.org/packages/NLua/
  System.Net.Sockets (net46) https://www.nuget.org/packages/System.Net.Sockets/
in the root directory of your server.

Then you should be able to successfully /pcompile the plugin.
Please do not download random ZIPs on the internet that claim to have
the plugin. They're likely to have malware .dlls/.sos so stay safe.
