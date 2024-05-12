using Clans.components;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

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
			
			// LBaseInfo.LPlayers[player.SteamID]!.SetClan(result.Item2);
			LBaseInfo.LPlayers[player.SteamID]!.SetMemberInfo(result);
		});

		return HookResult.Continue;
	}
	
	public void OnPlayerDisconnectPost(int playerSlot)
	{
		CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
		
		if (!LStock.IsValidPlayer(player) || !LBaseInfo.LPlayers.ContainsKey(player!.SteamID))
			return;

		Task.Run(async () =>
		{
			await DB.SavePlayer(LBaseInfo.LPlayers[player.SteamID]!, player.PlayerName);
			
			Server.NextFrame(() =>
			{
				// if (!LStock.IsValidPlayer(player))
				// 	return;
				// LBaseInfo.LEvents.IOnPlayerDisconnectFull(LBaseInfo.LPlayers[player.SteamID]);
			
				LBaseInfo.LPlayers.Remove(player.SteamID);
			});
		});
	}
}