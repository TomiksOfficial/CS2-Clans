using CounterStrikeSharp.API.Core;

namespace Clans.components;

public class LPlayer/* :ILPlayer*/
{
	public CCSPlayerController player { get; private set; }
	public ulong steamid { get; }
	public bool Loaded { get; set; } = false;
	// public LClan? Clan { get; set; } = null;
	public LMemberInfo MemberInfo { get; private set; }

	public LPlayer(CCSPlayerController player)
	{
		this.player = player;
		steamid = player.SteamID;
	}

	public void SetPlayer(CCSPlayerController _player)
	{
		player = _player;
	}

	// public void SetClan(LClan? lclan)
	// {
	// 	Clan = lclan ?? null;
	// }
	
	public void SetMemberInfo(LMemberInfo memberInfo)
	{
		MemberInfo = memberInfo;
	}
}