using Clans.API;
using CounterStrikeSharp.API.Core;

namespace Clans.components;

public static class LBaseInfo
{
	public static CFGMain Config { get; set; }

	public static Dictionary<ulong, LPlayer?> LPlayers = new();

	public static LClans Plugin { get; set; }
	
	public static APISkills APISkills { get; set; }
	
	public static APIEvent APIEvent { get; set; }
	
	public static APIClan APIClan { get; set; }

	public static Dictionary<ulong, long> AwaitToAccept { get; set; } = new();

	public static Dictionary<long, LClan> Clans { get; set; } =  new();
}