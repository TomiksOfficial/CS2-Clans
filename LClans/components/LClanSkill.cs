using ClansSharedAPI;

namespace Clans.components;

public class LClanSkill : ISkill
{
	private int _skillLevel = 0;
	
	public int SkillLevel
	{
		get => _skillLevel;
		set
		{
			int maxLevel = -1;
			if (!SkillsInfo.GetMaxLevel(SkillName, ref maxLevel))
			{
				return;
			}

			if (value > maxLevel)
			{
				return;
			}

			_skillLevel = value;
		}
	}
	
	public string SkillName { get; }

	public LClanSkill(int skillLevel, string skillName)
	{
		SkillName = skillName;
		SkillLevel = skillLevel;
	}
}