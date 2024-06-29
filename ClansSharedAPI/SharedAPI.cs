using CounterStrikeSharp.API.Core;

namespace ClansSharedAPI;

public interface IAPISkills
{
	public bool CreateDefaultSkillStructure(string skillname, int maxlevel,
		Dictionary<string, string> parameters);

	public bool IsSkillConfigExists(string skillname);

	public void RegisterActiveSkill(string skillname);

	public Dictionary<string, string>? GetSkillConfig(string skillname);
}

public interface ISkill
{
	public int SkillLevel { get; set; }
	public string SkillName { get; }
}

public interface ILPlayer
{
	CCSPlayerController player { get; }
	public ulong steamid { get; }
	public bool Loaded { get; set; }
	public IMemberInfo MemberInfo { get; }
}

public interface IMemberInfo
{
	public string ClanRole { get; set; }
	public bool AccesToEditClan { get; set; }
	public long ClanID { get; set; }
}

public interface IEvents
{
	event Action<ILPlayer> OnPlayerLoaded;
	event Action<ILPlayer> OnPlayerDisconnectFull;
	event Action<ILPlayer> OnPlayerSpawned;
	event Action<ILPlayer, string> OnPlayerUpgradeSkill;
	event Action<ILPlayer, string> OnPlayerSellSkill;
}

public interface IClan
{
	public Dictionary<string, ISkill> Skills { get; }
	public bool LevelUp(string skillName, bool consumeSkillPoint = true);
	
	public bool Sell(string skillName, bool returnSkillPoint = true);

	public void GiveExp(int expCount);
}

public interface IAPIClan
{
	public IClan? GetClanById(long clanID);
}