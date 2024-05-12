using Clans.API;
using CounterStrikeSharp.API.Core;

namespace Clans.components;

public static class LBaseInfo
{
	public static CFGMain Config { get; set; }

	public static Dictionary<ulong, LPlayer?> LPlayers = new();

	public static BasePlugin Plugin { get; set; }
	
	public static APISkills APISkills { get; set; }

	public static Dictionary<ulong, long> AwaitToAccept { get; set; } = new();

	public static Dictionary<long, LClan> Clans { get; set; } = null;
}