using ClansSharedAPI;

namespace Clans.components;

public class LClan : IClan
{
	public string ClanName { get; private set; }
	public int MaxMembers { get; private set; }
	public int SkillPoints { get; set; }
	public long ClanID { get; set; }
	
	public List<ulong> ListSteamIDOfMembers { get; private set; } // parse db
	public Dictionary<string, ISkill> Skills { get; } = new Dictionary<string, ISkill>();

	public ulong ClanLeader { get; private set; } = 0;
	
	public int Exp { get; private set; }
	public int ReqExp { get; private set; }
	public int Level { get; private set; }

	public LClan(string clanName, int maxMembers, int skillPoints, int exp, int level, string clanLeader, long id)
	{
		ClanName = clanName;
		MaxMembers = maxMembers;
		SkillPoints = skillPoints;
		Exp = exp;
		Level = level;
		ClanLeader = Convert.ToUInt64(clanLeader);
		ClanID = id;
		
		ReqExp = level * LBaseInfo.Config.NeedExpPerLevel;
	}
	
	public bool RegisterSkill(LClanSkill skill)
	{
		int MaxLevel = 0;

		if (!SkillsInfo.GetMaxLevel(skill.SkillName, ref MaxLevel) || Skills.ContainsKey(skill.SkillName)) return false;

		if (skill.SkillLevel < 0)
			skill.SkillLevel = 0;
		else if (skill.SkillLevel > MaxLevel) skill.SkillLevel = MaxLevel;

		return Skills.TryAdd(skill.SkillName, skill);
	}
	
	public bool LevelUp(string skillName, bool consumeSkillPoint = true)
	{
		if (!Skills.ContainsKey(skillName) || !SkillsInfo.skills.TryGetValue(skillName, out LSkillInfo? skill))
		{
			return false;
		}

		if (Skills[skillName].SkillLevel >= skill.MaxLevel)
		{
			return false;
		}

		Skills[skillName].SkillLevel += 1;
		
		if(consumeSkillPoint)
		{
			SkillPoints -= 1;
		}

		// foreach (var ply in LBaseInfo.LPlayers.Values.Where(p => p != null && p.MemberInfo.ClanID == ClanID))
		// {
		// 	if (ply!.Clan == null)
		// 	{
		// 		continue;
		// 	}
		//
		// 	ply.Clan.Skills[skillName].SkillLevel = Skills[skillName].SkillLevel;
		// }

		return true;
	}
	
	public bool Sell(string skillName, bool returnSkillPoint = true)
	{
		if (!Skills.ContainsKey(skillName) || Skills[skillName].SkillLevel <= 0)
		{
			return false;
		}
		
		Skills[skillName].SkillLevel -= 1;
		
		if(returnSkillPoint)
		{
			SkillPoints += 1;
		}
		
		// foreach (var ply in LBaseInfo.LPlayers.Values.Where(p => p != null && p.MemberInfo.ClanID == ClanID))
		// {
		// 	if (ply!.Clan == null)
		// 	{
		// 		continue;
		// 	}
		//
		// 	ply.Clan.Skills[skillName].SkillLevel = Skills[skillName].SkillLevel;
		// }

		return true;
	}
	
	public void GiveExp(int expCount)
	{
		if (Exp + expCount >= ReqExp)
		{
			Level += 1;
			Exp = Exp + expCount - ReqExp;
			ReqExp = Level * LBaseInfo.Config.NeedExpPerLevel;
			SkillPoints += 1;
			MaxMembers += LBaseInfo.Config.MaxClanMembersPerLevel;
		} else
		{
			Exp += expCount;
		}
	}
	
}