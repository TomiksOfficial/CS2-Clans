using System.Text.Json;

namespace Clans.components;

public class SkillsInfo
{
	public static readonly Dictionary<string, LSkillInfo> skills = new();
	public static readonly HashSet<string> activeSkills = new();

	public static bool GetMaxLevel(string skillName, ref int level)
	{
		if (!skills.TryGetValue(skillName, out var skill)) return false;

		level = skill.MaxLevel;

		return true;
	}

	public static bool GetSkillConfig(string skillName)
	{
		if (skills.ContainsKey(skillName)) return true;

		Dictionary<string, string>? process =
			JsonSerializer.Deserialize<Dictionary<string, string>>(
				File.ReadAllText(LBaseInfo.Plugin.ModuleDirectory + $"/skills/{skillName}.json"));

		if (process is null) return false;

		skills.Add(process["skillname"], new LSkillInfo(Convert.ToInt32(process["maxlevel"]), process["skillname"]));

		return true;
	}

	public static bool GetConfigsInfo()
	{
		var DF = new DirectoryInfo(LBaseInfo.Plugin.ModuleDirectory + "/skills/");

		Dictionary<string, string>? process;

		foreach (var fileInfo in DF.GetFiles())
		{
			process = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(fileInfo.FullName));

			if (process is null) continue;

			skills.Add(process["skillname"],
				new LSkillInfo(Convert.ToInt32(process["maxlevel"]), process["skillname"]));
		}

		return true;
	}

	public static void RegisterActiveSkill(string skillname)
	{
		activeSkills.Add(skillname);
	}
}

public class LSkillInfo
{
	private int _maxLevel = 0;
	private string _skillName = "";

	public int MaxLevel
	{
		get => _maxLevel;
		private set => _maxLevel = value;
	}

	public string SkillName
	{
		get => _skillName;
		private set => _skillName = value;
	}

	public LSkillInfo(int maxLevel, string skillName)
	{
		MaxLevel = maxLevel;
		SkillName = skillName;
	}
}