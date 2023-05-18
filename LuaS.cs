/*
 * This plugin will let you write MCGalaxy plugins and commands using Lua.
 * 
 * Plugins written in Lua contain many caveats and work-arounds
 * because of how MCGalaxy's and NLua's APIs are made.
 *
 * What you would usually use as a reference/pointer is
 * a C# class because NLua does not support references and needs
 * "containers". To edit the container values you access the container's
 * .contained property.
 *
 * Lua functions are not C# functions and therefore cannot be used to
 * register event functions. The work-around for that is to define functions
 * for each plugin. They follow the C# API's naming with the exception of them
 * using camelCase. So for example, OnLevelSave becomes onLevelSave.
 *
 * Lua plugin speed compared to C# plugin speed will be slower, however,
 * the cost of the speed is repaid by the time it takes to write
 * a plugin with Lua's simplicity (and less special character spam).
 * 
 * 
 */

//reference System.dll
//reference System.Net.Sockets.dll
//reference NLua.dll

using MCGalaxy.Games;
using MCGalaxy.Network;
using MCGalaxy.Blocks.Physics;
using MCGalaxy.Events;
using MCGalaxy.Events.EntityEvents;
using MCGalaxy.Events.GameEvents;
using MCGalaxy.Events.GroupEvents;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerDBEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
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

                    #region Entitiy register
                    OnTabListEntryAddedEvent.Register(HandleTabListEntryAdded, Priority.Critical);
                    OnTabListEntryRemovedEvent.Register(HandleTabListEntryRemoved, Priority.Critical);

                    OnEntitySpawnedEvent.Register(HandleEntitySpawned, Priority.Critical);
                    OnEntityDespawnedEvent.Register(HandleEntityDespawned, Priority.Critical);

                    OnSendingModelEvent.Register(HandleSendingModel, Priority.Critical);
                    OnGettingCanSeeEntityEvent.Register(HandleGettingCanSeeEntity, Priority.Critical);
                    #endregion Entity register
                    #region Games register
                    OnStateChangedEvent.Register(HandleStateChanged, Priority.Critical);
                    OnMapsChangedEvent.Register(HandleMapsChanged, Priority.Critical);
                    #endregion Games register
                    #region Group register 
                    OnGroupLoadedEvent.Register(HandleGroupLoaded, Priority.Critical);
                    OnGroupLoadEvent.Register(HandleGroupLoad, Priority.Critical); 
                    OnGroupSaveEvent.Register(HandleGroupSave, Priority.Critical); 
                    OnChangingGroupEvent.Register(HandleChangingGroup, Priority.Critical);
                    #endregion Group register
                    #region Level register
                    OnLevelLoadedEvent.Register(HandleLevelLoaded, Priority.Critical);
                    OnLevelLoadEvent.Register(HandleLevelLoad, Priority.Critical);
                    
                    OnLevelSaveEvent.Register(HandleLevelSave, Priority.Critical);
                    
                    OnLevelUnloadEvent.Register(HandleLevelUnload, Priority.Critical);
                    
                    OnLevelAddedEvent.Register(HandleLevelAdded, Priority.Critical);
                    OnLevelRemovedEvent.Register(HandleLevelRemoved, Priority.Critical);
                    
                    OnPhysicsStateChangedEvent.Register(HandlePhysicsStateChanged, Priority.Critical);
                    OnPhysicsUpdateEvent.Register(HandlePhysicsUpdate, Priority.Critical);
                    
                    OnLevelRenamedEvent.Register(HandleLevelRenamed, Priority.Critical);
                    OnLevelCopiedEvent.Register(HandleLevelCopied, Priority.Critical);
                    OnLevelDeletedEvent.Register(HandleLevelDeleted, Priority.Critical);

                    OnBlockHandlersUpdatedEvent.Register(HandleBlockHandlersUpdated, Priority.Critical);
                    //OnMainLevelChangingEvent.Register(HandleMainLevelChanging, Priority.Critical); //TODO: Once MCGalaxy upgrades, update this event
                    #endregion Level register
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

                #region Economy
                #endregion Economy
                #region Entity

                void HandleTabListEntryAdded(Entity e, ref string tabName, ref string tabGroup, Player dst) {
                    LuaContainer tabName_c = new LuaContainer((object) tabName);
                    LuaContainer tabGroup_c = new LuaContainer((object) tabGroup);
                    this.Call("onTabListEntryAdded", e, tabName_c, tabGroup_c, dst);
                    tabName = (string) tabName_c.contained;
                    tabGroup = (string) tabGroup_c.contained;
                }

                void HandleTabListEntryRemoved(Entity e, Player dst) {
                    this.Call("onTabListEntryRemoved", e, dst);
                }

                void HandleEntitySpawned(Entity e, ref string name, ref string skin, ref string model, Player dst) {
                    LuaContainer name_c = new LuaContainer((object) name);
                    LuaContainer skin_c = new LuaContainer((object) skin);
                    LuaContainer model_c = new LuaContainer((object) model);
                    this.Call("onEntitySpawned", e, name_c, skin_c, model_c, dst);
                    name = (string) name_c.contained;
                    skin = (string) skin_c.contained;
                    model = (string) model_c.contained;
                }

                void HandleEntityDespawned(Entity e, Player dst) {
                    this.Call("onEntityDespawned", e, dst);
                }

                void HandleSendingModel(Entity e, ref string model, Player dst) {
                    LuaContainer model_c = new LuaContainer((object) model);
                    this.Call("onSendingModel", e, model_c, dst);
                    model = (string) model_c.contained;
                }

                void HandleGettingCanSeeEntity(Player p, ref bool canSee, Entity target) {
                    LuaContainer canSee_c = new LuaContainer((object) canSee);
                    this.Call("onGettingCanSeeEntity", p, canSee_c, target);
                    canSee = (bool) canSee_c.contained;
                }
                
                #endregion Entity
                #region Game
                
                void HandleStateChanged(IGame game) {
                    this.Call("onStateChanged", game);
                }

                void HandleMapsChanged(RoundsGame game) {
                    this.Call("onMapsChanged", game);
                }

                #endregion Game
                #region Group

                void HandleGroupLoaded(Group g) {
                    this.Call("groupLoaded", g);
                }

                void HandleGroupLoad() {
                    this.Call("groupLoad");
                }

                void HandleGroupSave() {
                    this.Call("groupSave");
                }

                void HandleChangingGroup(string player, Group curRank, Group newRank, ref bool cancel) {
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    this.Call("onChangingGroup", player, curRank, newRank, cancel_c);
                    cancel = (bool) cancel_c.contained;
                }

                #endregion Group
                #region Level

                void HandleLevelLoaded(Level lvl) {
                    this.Call("onLevelLoaded", lvl);
                }

                void HandleLevelLoad(string name, string path, ref bool cancel) {
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    this.Call("onLevelLoad", name, path, cancel_c);
                    cancel = (bool) cancel_c.contained;
                }

                void HandleLevelSave(Level lvl, ref bool cancel) {
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    this.Call("onLevelSave", lvl, cancel_c);
                    cancel = (bool) cancel_c.contained;
                }

                void HandleLevelUnload(Level lvl, ref bool cancel) {
                    LuaContainer cancel_c = new LuaContainer((object) cancel);
                    this.Call("onLevelUnload", lvl, cancel_c);
                    cancel = (bool) cancel_c.contained;
                }

                void HandleLevelAdded(Level lvl) {
                    this.Call("onLevelAdded", lvl);
                }

                void HandleLevelRemoved(Level lvl) {
                    this.Call("onLevelRemoved", lvl);
                }

                void HandlePhysicsStateChanged(Level lvl, PhysicsState state) {
                    this.Call("onPhysicsStateChanged", lvl, state);
                }

                void HandlePhysicsLevelChanged(Level lvl, int level) {
                    this.Call("onPhysicsLevelChanged", lvl, level);
                }

                void HandlePhysicsUpdate(ushort x, ushort y, ushort z, PhysicsArgs args, Level lvl)  {
                    this.Call("onPhysicsUpdate", x, y, z, args, lvl);
                }

                void HandleLevelRenamed(string srcMap, string dstMap) {
                    this.Call("onLevelRenamed", srcMap, dstMap);
                }

                void HandleLevelCopied(string srcMap, string dstMap) {
                    this.Call("onLevelCopied", srcMap, dstMap);
                }

                void HandleLevelDeleted(string map) {
                    this.Call("onLevelDeleted", map);
                }

                void HandleBlockHandlersUpdated(Level lvl, BlockID block) {
                    this.Call("onBlockHandlersUpdated", lvl, block);
                }

                void HandleMainLevelChanging(ref string map) {
                    LuaContainer map_c = new LuaContainer((object) map);
                    this.Call("onMainLevelChanging", map_c);
                    map = (string) map_c.contained;
                }

                #endregion Level
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

                    #region Entitiy unregister
                    OnTabListEntryAddedEvent.Unregister(HandleTabListEntryAdded);
                    OnTabListEntryRemovedEvent.Unregister(HandleTabListEntryRemoved);

                    OnEntitySpawnedEvent.Unregister(HandleEntitySpawned);
                    OnEntityDespawnedEvent.Unregister(HandleEntityDespawned);

                    OnSendingModelEvent.Unregister(HandleSendingModel);
                    OnGettingCanSeeEntityEvent.Unregister(HandleGettingCanSeeEntity);
                    #endregion Entity unregister
                    #region Games unregister
                    OnStateChangedEvent.Unregister(HandleStateChanged);
                    OnMapsChangedEvent.Unregister(HandleMapsChanged);
                    #endregion Games unregister
                    #region Group unregister
                    OnGroupLoadedEvent.Unregister(HandleGroupLoaded);
                    OnGroupLoadEvent.Unregister(HandleGroupLoad);
                    OnGroupSaveEvent.Unregister(HandleGroupSave);
                    OnChangingGroupEvent.Unregister(HandleChangingGroup);
                    #endregion Group unregister
                    #region Level unregister
                    OnLevelLoadedEvent.Unregister(HandleLevelLoaded);
                    OnLevelLoadEvent.Unregister(HandleLevelLoad);
                    
                    OnLevelSaveEvent.Unregister(HandleLevelSave);
                    
                    OnLevelUnloadEvent.Unregister(HandleLevelUnload);
                    
                    OnLevelAddedEvent.Unregister(HandleLevelAdded);
                    OnLevelRemovedEvent.Unregister(HandleLevelRemoved);
                    
                    OnPhysicsStateChangedEvent.Unregister(HandlePhysicsStateChanged);
                    OnPhysicsUpdateEvent.Unregister(HandlePhysicsUpdate);
                    
                    OnLevelRenamedEvent.Unregister(HandleLevelRenamed);
                    OnLevelCopiedEvent.Unregister(HandleLevelCopied);
                    OnLevelDeletedEvent.Unregister(HandleLevelDeleted);

                    OnBlockHandlersUpdatedEvent.Unregister(HandleBlockHandlersUpdated);
                    //OnMainLevelChangingEvent.Unregister(HandleMainLevelChanging); //TODO: Once MCGalaxy upgrades, update this event
                    #endregion Level unregister
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
