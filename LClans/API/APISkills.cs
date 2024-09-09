using System.Text.Json;
using Clans.components;
using ClansSharedAPI;

namespace Clans.API;

public class APISkills : IAPISkills
{
	public bool CreateDefaultSkillStructure(string skillname, int maxlevel,
		Dictionary<string, string> skill)
	{
		var cfg = GetSkillConfig(skillname);

		skill.TryAdd("skillname", skillname);
		skill.TryAdd("maxlevel", maxlevel.ToString());

		if (cfg is not null && (skill.Count != cfg.Count || !skill.All(x => cfg.ContainsKey(x.Key))))
		{
			foreach (var kvp in skill.Where(x => !cfg.ContainsKey(x.Key))) cfg.TryAdd(kvp.Key, kvp.Value);

			foreach (var kvp in cfg.Where(x => !skill.ContainsKey(x.Key))) cfg.Remove(kvp.Key);
		} else if (cfg is null)
		{
			cfg = skill;
		} else
		{
			return true;
		}

		try
		{
			var jso = new JsonSerializerOptions();
			jso.WriteIndented = true;

			File.WriteAllText(LBaseInfo.Plugin.ModuleDirectory + $"/skills/{skillname}.json",
				JsonSerializer.Serialize(cfg, jso));

			SkillsInfo.GetSkillConfig(skillname);

			return true;
		}
		catch (Exception e)
		{
			return false;
		}
		
		////
		
		// try
		// {
		// 	skill.TryAdd("skillname", skillname);
		// 	skill.TryAdd("maxlevel", maxlevel.ToString());
		//
		// 	var jso = new JsonSerializerOptions();
		// 	jso.WriteIndented = true;
		// 	
		// 	File.WriteAllText(LBaseInfo.Plugin.ModuleDirectory + $"/skills/{skillname}.json", JsonSerializer.Serialize(skill, jso));
		//
		// 	SkillsInfo.GetSkillConfig(skillname);
		//
		// 	return true;
		// }
		// catch (Exception e)
		// {
		// 	return false;
		// }
	}

	public bool IsSkillConfigExists(string skillname)
	{
		return File.Exists(LBaseInfo.Plugin.ModuleDirectory + $"/skills/{skillname}.json");
	}

	public void RegisterActiveSkill(string skillname)
	{
		SkillsInfo.RegisterActiveSkill(skillname);
	}

	public Dictionary<string, string>? GetSkillConfig(string skillname)
	{
		if (!IsSkillConfigExists(skillname))
		{
			return null;
		}
		
		return JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(LBaseInfo.Plugin.ModuleDirectory + $"/skills/{skillname}.json"));
	}
}