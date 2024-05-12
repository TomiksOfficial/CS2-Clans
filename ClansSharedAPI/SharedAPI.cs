namespace ClansSharedAPI;

public interface IAPISkills
{
	public bool CreateDefаultSkillStructure(string skillname, int maxlevel,
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