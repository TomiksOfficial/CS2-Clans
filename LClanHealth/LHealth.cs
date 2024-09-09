using ClansSharedAPI;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;

namespace LHealth;

public class LHealth : BasePlugin
{
	public override string ModuleName => "Clans | Health";
	public override string ModuleVersion => "0.1.0";
	public override string ModuleDescription => "LClans Health Skill";
	public override string ModuleAuthor => "Tomiks(vk.com/tomiksofficial)";

	private static string SkillName => "health";

	private static PluginCapability<IEvents> APIEvents { get; } = new PluginCapability<IEvents>("clans:events");
	private static PlayerCapability<ILPlayer> APIPlayer { get; } = new PlayerCapability<ILPlayer>("clans:player");
	private static PluginCapability<IAPISkills> APISkills { get; } = new("clans:skills");
	private static PluginCapability<IAPIClan> APIClan { get; } = new("clans:clan");
	
	private Dictionary<string, string>? SkillConfig { get; set; }
	
	private IEvents? EventManager { get; set; }
	private IAPIClan? ClanManager { get; set; }
	
	public static bool IsValidPlayer(CCSPlayerController? player)
	{
		return player is not null && player.IsValid && player.PlayerPawn.IsValid && !player.IsBot;
	}
	
	public override void OnAllPluginsLoaded(bool hotReload)
	{
		IAPISkills SkillManager = APISkills.Get()!;
		
		EventManager = APIEvents.Get()!;
		ClanManager = APIClan.Get()!;

		EventManager.OnPlayerSpawned += OnPlayerSpawn;

		SkillManager.CreateDefaultSkillStructure(SkillName, 20,
			new Dictionary<string, string>
			{
				{"health", "50"}
			});

		SkillConfig = SkillManager.GetSkillConfig(SkillName);

		if (SkillConfig is null)
		{
			return;
		}
		
		SkillManager.RegisterActiveSkill(SkillName);
	}

	public override void Unload(bool hotReload)
	{
		if (EventManager is not null)
		{
			EventManager.OnPlayerSpawned -= OnPlayerSpawn;
		}
	}

	public void OnPlayerSpawn(ILPlayer? player)
	{
		if (SkillConfig is null)
			return;

		if (player is null || player.MemberInfo.ClanID == -1)
		{
			return;
		}

		IClan? clan = ClanManager!.GetClanById(player.MemberInfo.ClanID);

		if (clan == null)
		{
			return;
		}
		
		player.player.PlayerPawn.Value!.Health += Convert.ToInt32(SkillConfig["health"]) * clan.Skills[SkillName].SkillLevel;
		Utilities.SetStateChanged(player.player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
	}
}