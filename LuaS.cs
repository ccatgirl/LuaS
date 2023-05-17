/*
 * This plugin will let you write MCGalaxy plugins and commands using Lua.
 *
 */

//reference System.dll
//reference System.Net.Sockets.dll
//reference NLua.dll

using MCGalaxy.Events;
using MCGalaxy.Events.PlayerDBEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Network;
using BlockID = System.UInt16;
using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using NLua;

namespace MCGalaxy {
        public class LuaContainer : object {
            public object contained;

            public LuaContainer (object contained) {
                this.contained = contained;
            }
        }

	public class LuaS : Plugin {
		public override string name { get { return "LuaS"; } }

		public override string MCGalaxy_Version { get { return "1.9.4.8"; } }

		public override string welcome { get { return "Loaded Message!"; } }

		public override string creator { get { return "[MCGalaxy] With Grapes"; } }
                
                private Lua state;
                private List<LuaTable> plugins; 

		public override void Load(bool startup) {
                  
                    #region ModAction register
                    OnModActionEvent.Register(HandleModAction, Priority.Critical);
                    #endregion ModAction register
                    #region PlayerDB register
                    OnInfoSaveEvent.Register(HandleInfoSave, Priority.Critical);
                    OnInfoSwapEvent.Register(HandleInfoSwap, Priority.Critical);
                    #endregion PlayerDB register
                    #region Player register
                    OnPlayerChatEvent.Register(HandlePlayerChat, Priority.Critical);
                    OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.Critical);
                    OnPlayerCommandEvent.Register(HandlePlayerCommand, Priority.Critical);
                    // Connecting
                    OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.Critical);
                    OnPlayerStartConnectingEvent.Register(HandlePlayerStartConnecting, Priority.Critical);
                    OnPlayerFinishConnectingEvent.Register(HandlePlayerFinishConnecting, Priority.Critical);
                    // Death
                    OnPlayerDyingEvent.Register(HandlePlayerDying, Priority.Critical);
                    OnPlayerDiedEvent.Register(HandlePlayerDied, Priority.Critical);
                    
                    OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.Critical);
                    
                    OnBlockChangingEvent.Register(HandleBlockChanging, Priority.Critical);
                    OnBlockChangedEvent.Register(HandleBlockChanged, Priority.Critical);

                    OnPlayerClickEvent.Register(HandlePlayerClick, Priority.Critical);

                    OnMessageRecievedEvent.Register(HandleMessageReceived, Priority.Critical);

                    OnSentMapEvent.Register(HandleSentMap, Priority.Critical);
                    OnJoiningLevelEvent.Register(HandleJoiningLevel, Priority.Critical);
                    OnJoinedLevelEvent.Register(HandleJoinedLevel, Priority.Critical);

                    OnPlayerActionEvent.Register(HandlePlayerAction, Priority.Critical);
                    OnSettingPrefixEvent.Register(HandleSettingPrefix, Priority.Critical);
                    OnGettingMotdEvent.Register(HandleGettingMotd, Priority.Critical);
                    OnSendingMotdEvent.Register(HandleSendingMotd, Priority.Critical);
                    OnPlayerSpawningEvent.Register(HandlePlayerSpawning, Priority.Critical);
                    OnChangedZoneEvent.Register(HandleChangedZone, Priority.Critical);
                    OnGettingCanSeeEvent.Register(HandleGettingCanSee, Priority.Critical);
                    #endregion Player register
                    #region Server register
                    OnSendingHeartbeatEvent.Register(HandleSendingHeartbeat, Priority.Critical);
                    OnShuttingDownEvent.Register(HandleShuttingDown, Priority.Critical);
                    OnConfigUpdatedEvent.Register(HandleConfigUpdated, Priority.Critical);
                    OnConnectionReceivedEvent.Register(HandleConnectionReceived, Priority.Critical);
                    OnChatSysEvent.Register(HandleChatSys, Priority.Critical);
                    OnChatFromEvent.Register(HandleChatFrom, Priority.Critical);
                    OnChatEvent.Register(HandleChat, Priority.Critical);
                    OnPluginMessageReceivedEvent.Register(HandlePluginMessageReceived, Priority.Critical);
                    #endregion Server register


                    this.plugins = new List<LuaTable>();
                    this.state = new Lua();
                    this.state.LoadCLRPackage();
                    this.state.DoString(@"
                            -- Initiate the MCGalaxy namespace.
                            MCGalaxy = import (""MCGalaxy_"", ""MCGalaxy"")
                            
                            function Plugin(name)
                                return {__pluginname = name}
                            end
                    ");

                    Command.Register(new CmdRunLua());
                    Command.Register(new CmdLua());

                    string[] paths = Directory.GetFiles("lua", "*.lua", SearchOption.TopDirectoryOnly);

                    foreach (string i in paths) {

                        object[] rvalue = this.state.DoFile(i);

                        if (rvalue.Length > 0 && rvalue[0].ToString() == "table") {
                            Logger.Log(LogType.SystemActivity, "LuaS: Loaded plugin " + i);
                            
                            LuaTable plug = (LuaTable) rvalue[0];
                            this.plugins.Add(plug);
                        }
                        else {
                            Logger.Log(
                                LogType.Warning, 
                                "LuaS: The plugin " 
                                + i 
                                + " could not be loaded because it has an invalid structure and does not return a table."
                            );
                        }
                    }

                    this.Call("load", startup);
                }

                #region ModAction

                void HandleModAction(ModAction e) {
                    this.Call("onModAction", e);
                }

                #endregion ModAction
                #region PlayerDB
                
                void HandleInfoSave(Player p, ref bool cancel) {
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    this.Call("onInfoSave", p, cancel_c);
                    cancel = (bool) cancel_c.contained;
                }

                void HandleInfoSwap(string src, string dst) {
                    this.Call("onInfoSwap", src, dst);
                }

                #endregion PlayerDB
                #region Player
                
                void HandlePlayerChat(Player p, string message) {
                    this.Call("onPlayerChat", p, message);
                }

                void HandlePlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel) {
                    LuaContainer cancel_c = new LuaContainer((object)cancel);
                    this.Call("onPlayerMove", p, next, yaw, pitch, cancel_c);
                    cancel = (bool) cancel_c.contained;
                }

                void HandlePlayerCommand(Player p, string cmd, string args, CommandData data) {
                    this.Call("onPlayerCommand", p, cmd, args, data);
                }

                void HandlePlayerConnect(Player p) {
                    this.Call("onPlayerConnect", p);
                }
                
                void HandlePlayerStartConnecting(Player p, string mppass) {
                    this.Call("onPlayerStartConnecting", p, mppass);
                }

                void HandlePlayerFinishConnecting(Player p) {
                    this.Call("onPlayerFinishConnecting", p);
                }

                void HandlePlayerDying(Player p, BlockID cause, ref bool cancel) {
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    this.Call("onPlayerDying", p, cause, cancel_c);
                    cancel = (bool) cancel_c.contained;
                }

                void HandlePlayerDied(Player p, BlockID cause, ref TimeSpan cooldown) {
                    LuaContainer cooldown_c = new LuaContainer((object) cooldown);
                    this.Call("onPlayerDied", p, cause, cooldown_c);
                    cooldown = (TimeSpan) cooldown_c.contained;
                }

                void HandlePlayerDisconnect(Player p, string reason) {
                    this.Call("onPlayerDisconnect", p, reason);
                }

                void HandleBlockChanging(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel) {
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    this.Call("onBlockChanging", p, x, y, z, block, placing, cancel_c);
                    cancel = (bool) cancel_c.contained;
                }

                void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult result) {
                    this.Call("onBlockChanged", p, x, y, z, result);
                }

                void HandlePlayerClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face) {
                    this.Call("onPlayerClick", p, button, action, yaw, pitch, entity, x, y, z, face);
                }

                void HandleMessageReceived(Player p, ref string message, ref bool cancel) {
                    LuaContainer message_c = new LuaContainer((object) message);
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    this.Call("onMessageReceived", p, message_c, cancel_c);
                    message = (string) message_c.contained;
                    cancel = (bool) cancel_c.contained;
                }

                void HandleSentMap(Player p, Level prevLevel, Level level) {
                    this.Call("onSentMap", p, prevLevel, level);
                }

                void HandleJoiningLevel(Player p, Level lvl, ref bool canJoin) {
                    LuaContainer canJoin_c = new LuaContainer((object) canJoin);
                    this.Call("onJoiningLevel", p, lvl, canJoin_c);
                    canJoin = (bool) canJoin_c.contained;
                }

                void HandleJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce) {
                    LuaContainer announce_c = new LuaContainer((object) announce);
                    this.Call("onJoinedLevel", p, prevLevel, level, announce_c);
                    announce = (bool) announce_c.contained;
                }

                void HandlePlayerAction(Player p, PlayerAction action, string message, bool stealth) {
                    this.Call("onPlayerAction", p, action, message, stealth);
                }

                void HandleSettingPrefix(Player p, List<string> prefixes) {
                    this.Call("onSettingPrefix", p, prefixes);
                }

                void HandleSettingColor(Player p, ref string color) {
                    LuaContainer color_c = new LuaContainer((object) color);
                    this.Call("onSettingColor", p, color_c);
                    color = (string) color_c.contained;
                }

                void HandleGettingMotd(Player p, ref string motd) {
                    LuaContainer motd_c = new LuaContainer((object) motd);
                    this.Call("onGettingMotd", p, motd_c);
                    motd = (string) motd_c.contained;
                }

                void HandleSendingMotd(Player p, ref string motd) {
                    LuaContainer motd_c = new LuaContainer((object) motd);
                    this.Call("onSendingMotd", p, motd_c);
                    motd = (string) motd_c.contained;
                }

                void HandlePlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning) {
                    LuaContainer pos_c = new LuaContainer((object) pos);
                    LuaContainer yaw_c = new LuaContainer((object) yaw);
                    LuaContainer pitch_c = new LuaContainer((object) pitch);
                    this.Call("onPlayerSpawning", p, pos_c, yaw_c, pitch_c, respawning);
                    pos = (Position) pos_c.contained;
                    yaw = (byte) yaw_c.contained;
                    pitch = (byte) pitch_c.contained;
                }

                void HandleChangedZone(Player p) {
                    this.Call("onChangedZone", p);
                }

                void HandleGettingCanSee(Player p, LevelPermission plRank, ref bool canSee, Player target) {
                    LuaContainer canSee_c = new LuaContainer((object) canSee);
                    this.Call("onChangedZone", p, plRank, canSee_c, target);
                    canSee = (bool) canSee_c.contained;
                }

                #endregion Player
                #region Server

                void HandleSendingHeartbeat(Heartbeat service, ref string name) {
                    LuaContainer name_c = new LuaContainer((object) name);
                    this.Call("onSendingHeartbeat", service, name_c);
                    name = (string) name_c.contained;
                }

                void HandleShuttingDown(bool restarting, string reason) {
                    this.Call("onShuttingDown", restarting, reason);
                }

                void HandleConfigUpdated() {
                    this.Call("onConfigUpdated");
                }

                void HandleConnectionReceived(Socket s, ref bool cancel, ref bool announce) {
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    LuaContainer announce_c = new LuaContainer((object) announce);
                    this.Call("onConnectionReceived", s, cancel_c, announce_c);
                    cancel = (bool) cancel_c.contained;
                    announce = (bool) announce_c.contained;
                }

                void HandleChatSys(ChatScope scope, string msg, object arg, ref ChatMessageFilter filter, bool relay) {
                    LuaContainer filter_c = new LuaContainer((object) filter);
                    this.Call("onChatSys", scope, msg, arg, filter, relay);
                    filter = (ChatMessageFilter) filter_c.contained;
                }

                void HandleChatFrom(ChatScope scope, Player source, string msg, object arg, ref ChatMessageFilter filter, bool relay) {
                    LuaContainer filter_c = new LuaContainer((object) filter);
                    this.Call("onChatFrom", scope, source, msg, arg, filter_c, relay);
                    filter = (ChatMessageFilter) filter_c.contained;
                }

                void HandleChat(ChatScope scope, Player source, string msg, object arg, ref ChatMessageFilter filter, bool relay) {
                    LuaContainer filter_c = new LuaContainer((object) filter);
                    this.Call("onChat", scope, source, msg, arg, filter_c, relay);
                    filter = (ChatMessageFilter) filter_c.contained;
                }

                void HandlePluginMessageReceived(Player p, byte channel, byte[] data) {
                    this.Call("onPluginMessageReceived", p, channel, data);
                }

                #endregion Server

		public override void Unload(bool shutdown) {
                    Command.Unregister(Command.Find("RunLua"));
                    Command.Unregister(Command.Find("Lua"));
                    
                    #region ModAction unregister
                    OnModActionEvent.Unregister(HandleModAction);
                    #endregion ModAction unregister
                    #region PlayerDB unregister
                    OnInfoSaveEvent.Unregister(HandleInfoSave);
                    OnInfoSwapEvent.Unregister(HandleInfoSwap);
                    #endregion PlayerDB unregister
                    #region Player unregister
                    OnPlayerChatEvent.Unregister(HandlePlayerChat);
                    OnPlayerMoveEvent.Unregister(HandlePlayerMove);
                    OnPlayerCommandEvent.Unregister(HandlePlayerCommand);
                    // Connecting events
                    OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
                    OnPlayerStartConnectingEvent.Unregister(HandlePlayerStartConnecting);
                    OnPlayerFinishConnectingEvent.Unregister(HandlePlayerFinishConnecting);
                    // Death
                    OnPlayerDyingEvent.Unregister(HandlePlayerDying);
                    OnPlayerDiedEvent.Unregister(HandlePlayerDied);
                    
                    OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);

                    OnBlockChangingEvent.Unregister(HandleBlockChanging);
                    OnBlockChangedEvent.Unregister(HandleBlockChanged);

                    OnPlayerClickEvent.Unregister(HandlePlayerClick);
                    
                    OnMessageRecievedEvent.Unregister(HandleMessageReceived);
                    
                    OnSentMapEvent.Unregister(HandleSentMap);
                    OnJoiningLevelEvent.Unregister(HandleJoiningLevel);
                    OnJoinedLevelEvent.Unregister(HandleJoinedLevel);

                    OnPlayerActionEvent.Unregister(HandlePlayerAction);
                    OnSettingPrefixEvent.Unregister(HandleSettingPrefix);
                    OnGettingMotdEvent.Unregister(HandleGettingMotd);
                    OnSendingMotdEvent.Unregister(HandleSendingMotd);
                    OnPlayerSpawningEvent.Unregister(HandlePlayerSpawning);
                    OnChangedZoneEvent.Unregister(HandleChangedZone);
                    OnGettingCanSeeEvent.Unregister(HandleGettingCanSee);
                    #endregion Player unregister
                    #region Server unregister
                    OnSendingHeartbeatEvent.Unregister(HandleSendingHeartbeat);
                    OnShuttingDownEvent.Unregister(HandleShuttingDown);
                    OnConfigUpdatedEvent.Unregister(HandleConfigUpdated);
                    OnConnectionReceivedEvent.Unregister(HandleConnectionReceived);
                    OnChatSysEvent.Unregister(HandleChatSys);
                    OnChatFromEvent.Unregister(HandleChatFrom);
                    OnChatEvent.Unregister(HandleChat);
                    OnPluginMessageReceivedEvent.Unregister(HandlePluginMessageReceived);
                    #endregion Server unregister

                    this.Call("unload", shutdown);
                    this.state.Close();
		}

		public override void Help(Player p) {
			p.Message("This plugin allows you to run Lua scripts on the MCGalaxy server.");
		}

                private void Call(string name, params object[] args) {
                    foreach(LuaTable plug in this.plugins) {
                        if (plug[name] != null && plug[name].ToString() == "function") {
                            ((LuaFunction) plug[name]).Call(args);
                        }
                    }
                }

	}

        public class CmdRunLua : Command {
            public override string name { get { return "RunLua"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Admin; }}
            public override string type { get { return "Scripting"; } }

            public override void Use (Player p, string args) {
                //Lua state = new Lua();
                //LuaTable res = (LuaTable)state.DoFile(args)[0];
                //state.Close();
            }

            public override void Help (Player p) {
                p.Message("Runs Lua code. /RunLua function returnV(v) if v == 1 then return \"v\"; else return \"no\"; end end return returnV(1);");
            }
        }

        public class CmdLua : Command {
            public override string name { get { return "Lua"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Admin; }}
            public override string type { get { return "Scripting"; } } 

            public override void Use (Player p, string args) {
                if (args.ToLower().StartsWith("new")) {
                    string[] words = args.Split(' ');

                    if (words.Length <= 1)
                        return;

                    if (File.Exists("lua/" + words[1].ToLower() + "_plugin.lua")) {
                        p.Message("Unfortunately that file already exists. Try another file name!");
                    }
                    else {
                        Util.TextFile file = new Util.TextFile("lua/" + words[1].ToLower() + "_plugin.lua",
                                String.Format(@"--[[
  Lua plugin skeleton.
]]

-- You are advised to make every 
-- plugin table local as all of 
-- the plugins share the same 
-- Lua state/context
local {0}Plugin = Plugin ""{0}""

function  {0}Plugin.load(startup)
    -- Put what you would put into the C# Load method here.
    if startup then
        -- All of MCGalaxy's API is accessible
        -- through the MCGalaxy namespace.
        MCGalaxy.Logger.Log(
            MCGalaxy.LogType.Warning, 
            ""This plugin loaded when you started up the server""
        )
    else
        MCGalaxy.Logger.Log(
            MCGalaxy.LogType.Warning, 
            ""This plugin got loaded in manually.""
        )
    end
end

function {0}Plugin.unload(shutdown) 
    -- Same deal here. put what you would put into
    -- the C# Unload method here.
end

-- The Plugin API expects you to
-- return the plugin table.
return {0}Plugin ",
                                words[1]
                            )
                        );
                        file.EnsureExists();
                        p.Message("Saved the file into lua/" + words[1].ToLower() + "_plugin.lua");
                    }
                }
            }

            public override void Help (Player p) {
                p.Message(@"A rather more advanced lua command.");
            }

        }
}
