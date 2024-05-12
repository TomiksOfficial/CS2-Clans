using CounterStrikeSharp.API.Core;

namespace Clans.components;

public static class LStock
{
	public static bool IsValidPlayer(CCSPlayerController? player)
	{
		return player != null && player.IsValid && player.PlayerPawn.IsValid && !player.IsBot;
	}
}