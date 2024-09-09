using Clans.components;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace Clans;

public partial class LClans
{
	[ConsoleCommand("css_clans")]
	public void CLANSCommand(CCSPlayerController player, CommandInfo info)
	{
		// Console.WriteLine("test clans comm 1");
		if (!LStock.IsValidPlayer(player))
		{
			return;
		}
		
		// Console.WriteLine("test clans comm 2");
		
		LMenu.MainMenu(LBaseInfo.LPlayers[player.SteamID]);
	}
	
	[ConsoleCommand("css_clanaccept")]
	public void CLANSAcceptCommand(CCSPlayerController player, CommandInfo info)
	{
		if (LBaseInfo.AwaitToAccept.TryGetValue(player.SteamID, out var ClanID))
		{
			LBaseInfo.LPlayers[player.SteamID]!.MemberInfo.AccesToEditClan = false;
			LBaseInfo.LPlayers[player.SteamID]!.MemberInfo.ClanID = ClanID;
			LBaseInfo.LPlayers[player.SteamID]!.MemberInfo.ClanRole = "Member";
			
			LBaseInfo.AwaitToAccept.Remove(player.SteamID);

			DB.SavePlayer(LBaseInfo.LPlayers[player.SteamID]!, player.PlayerName);
			
			// player.PrintToChat();
		}
	}
	
	[ConsoleCommand("css_createclan")]
	public void CLANSCreateCommand(CCSPlayerController player, CommandInfo info)
	{
		if (!LStock.IsValidPlayer(player) || info.ArgCount != 2 || LBaseInfo.LPlayers[player.SteamID] == null)
		{
			return;
		}

		DB.CreateClan(LBaseInfo.LPlayers[player.SteamID]!, info.GetArg(1));
	}
}