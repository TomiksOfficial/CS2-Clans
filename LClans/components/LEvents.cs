using Clans.components;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

namespace Clans;

public partial class LClans
{
	[GameEventHandler]
	public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;

		if (!LStock.IsValidPlayer(player))
		{
			return HookResult.Continue;
		}

		LBaseInfo.LPlayers.Add(player!.SteamID, new LPlayer(player));

		Task.Run(async () =>
		{
			var result = await DB.InitPlayer(player.SteamID);

			// if (result == null)
			// {
			// 	Console.WriteLine("RESULT IS NULL");
			// }
			
			// LBaseInfo.LPlayers[player.SteamID]!.SetClan(result.Item2);
			LBaseInfo.LPlayers[player.SteamID]!.SetMemberInfo(result);

			// if (LBaseInfo.LPlayers[player.SteamID] == null)
			// {
			// 	Console.WriteLine("PLAYER IS NULL");
			// }
			//
			// if (LBaseInfo.LPlayers[player.SteamID].MemberInfo == null)
			// {
			// 	Console.WriteLine("MEMBER INFO IS NULL");
			// }
		});

		return HookResult.Continue;
	}
    
	[GameEventHandler]
	public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
	{
		if (LStock.IsValidPlayer(@event.Attacker) && @event.Attacker != @event.Userid &&
		    LBaseInfo.LPlayers[@event.Attacker!.SteamID] != null &&
		    LBaseInfo.LPlayers[@event.Attacker!.SteamID]?.MemberInfo.ClanID != -1 &&
		    LBaseInfo.Clans.TryGetValue(LBaseInfo.LPlayers[@event.Attacker!.SteamID]!.MemberInfo.ClanID, out LClan? clanAttacker))
			clanAttacker.GiveExp(LBaseInfo.Config.KillExp);

		if (LStock.IsValidPlayer(@event.Assister) && LBaseInfo.LPlayers[@event.Assister!.SteamID] != null &&
		    LBaseInfo.LPlayers[@event.Assister!.SteamID]?.MemberInfo.ClanID != -1 &&
		    LBaseInfo.Clans.TryGetValue(LBaseInfo.LPlayers[@event.Assister!.SteamID]!.MemberInfo.ClanID,
			    out LClan? clanAssister))
			clanAssister.GiveExp(LBaseInfo.Config.AssistExp);

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult EventRoundEnd(EventRoundEnd @event, GameEventInfo info)
	{
		
		foreach (var player in Utilities.GetPlayers())
		{
			if (!LStock.IsValidPlayer(player) || !LBaseInfo.LPlayers.ContainsKey(player.SteamID) ||
			    LBaseInfo.LPlayers[player.SteamID] == null ||
			    LBaseInfo.LPlayers[player.SteamID]?.MemberInfo.ClanID == -1 ||
			    !LBaseInfo.Clans.TryGetValue(LBaseInfo.LPlayers[player.SteamID]!.MemberInfo.ClanID, out LClan? clan))
				continue;
			
			if (LBaseInfo.LPlayers[player.SteamID].player.PlayerPawn.Value!.LifeState == (byte)LifeState_t.LIFE_ALIVE)
				clan.GiveExp(LBaseInfo.Config.AliveInRoundEnd);

			if (@event.Winner == (byte)player.Team)
				clan.GiveExp(LBaseInfo.Config.WinExp);
			else if (player.Team != CsTeam.None && player.Team != CsTeam.Spectator)
				clan.GiveExp(LBaseInfo.Config.LoseExp);
		}
		
		// Data Save | Write all other actions upper
		foreach (LClan? clan in LBaseInfo.Clans.Values)
		{
			if (clan == null)
			{
				continue;
			}
			
			Task.Run(async () =>
			{
				DB.SaveClan(clan);
			});
		}

		return HookResult.Continue;
	}
	
	public void OnPlayerDisconnectPost(int playerSlot)
	{
		CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
		
		if (!LStock.IsValidPlayer(player) || !LBaseInfo.LPlayers.ContainsKey(player!.SteamID))
			return;

		string name = player.PlayerName;
		
		Task.Run(async () =>
		{
			await DB.SavePlayer(LBaseInfo.LPlayers[player.SteamID]!, name);
			
			Server.NextFrame(() =>
			{
				if (!LStock.IsValidPlayer(player))
					return;
				LBaseInfo.APIEvent.IOnPlayerDisconnectFull(LBaseInfo.LPlayers[player.SteamID]);
			
				LBaseInfo.LPlayers.Remove(player.SteamID);
			});
		});
	}
	
	private static Dictionary<ulong, CounterStrikeSharp.API.Modules.Timers.Timer> respawnTimers = new Dictionary<ulong, CounterStrikeSharp.API.Modules.Timers.Timer>();
	
	[GameEventHandler]
	public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
	{
		var player = @event.Userid;
		
		if (!LStock.IsValidPlayer(player) || !LBaseInfo.LPlayers.ContainsKey(player!.SteamID) || LBaseInfo.LPlayers[player.SteamID] == null)
		{
			return HookResult.Continue;
		}
	
		if(respawnTimers.TryGetValue(player.SteamID, out CounterStrikeSharp.API.Modules.Timers.Timer? tm))
		{
			tm.Kill();
			respawnTimers.Remove(player.SteamID);
		}
	
		respawnTimers.Add(player.SteamID, new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, () =>
		{
			if (LStock.IsValidPlayer(player) && player.PlayerPawn.Value!.LifeState == (int)LifeState_t.LIFE_ALIVE)
			{
				Server.NextFrame(() =>
				{
					// LBaseInfo.LPlayers[player.SteamID]!.Modifiers.SetAllDefault();
					LBaseInfo.APIEvent.IOnPlayerSpawned(LBaseInfo.LPlayers[player.SteamID]!);
				});
			}
	
			respawnTimers.Remove(player.SteamID);
		}));
		
		return HookResult.Continue;
	}
}