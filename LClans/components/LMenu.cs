using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;

namespace Clans.components;

public class LMenu
{
	public static void MainMenu(LPlayer? player)
	{
		if (player == null)
		{
			return;
		}
		
		var menu = LBaseInfo.Plugin.menuAPI.NewMenu(LBaseInfo.Plugin.Localizer["clans.mainmenu.title"]);

		if (player.MemberInfo.ClanID == -1)
		{
			menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.mainmenu.createclan"], (ply, option) =>
			{
				player.player.PrintToChat(LBaseInfo.Plugin.Localizer["clans.mainmenu.createclanmessage"]);
			});
		} else
		{
			if (!LBaseInfo.Clans.TryGetValue(player.MemberInfo.ClanID, out var clan))
			{
				return;
			}

			menu.AddMenuOption(clan.ClanName, (controller, option) =>
			{
				OpenClanMenu(player);
			});
		}

		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.mainmenu.topclans"], (controller, option) =>
		{
			ShowTopClans(player);
		});

		menu.ExitButton = true;
		menu.Open(player.player);
	}
	
	public static void OpenClanMenu(LPlayer? player)
	{
		if (player == null)
		{
			return;
		}
		
		var menu = LBaseInfo.Plugin.menuAPI.NewMenu(LBaseInfo.Plugin.Localizer["clans.clanmenu.title", LBaseInfo.Clans[player.MemberInfo.ClanID].ClanName]);

		menu.AddMenuOption(
			LBaseInfo.Plugin.Localizer["clans.clanmenu.access", player.MemberInfo.AccesToEditClan ? "+" : "-"],
			(controller, option) =>
			{
			}, true);

		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.clanmenu.skills"], (controller, option) =>
		{
			OpenSkillsListMenu(player);
		});
		
		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.clanmenu.members"], (controller, option) =>
		{
			if (!LBaseInfo.Clans.TryGetValue(player.MemberInfo.ClanID, out var clan))
			{
				return;
			}
			
			Task.Run(async () =>
			{
				var result = await DB.ShowMembersList(clan.ClanID);

				if (result == null)
				{
					return;
				}
				
				Server.NextFrame(() =>
				{
					OpenMembersList(player, result);
				});
			});
		});

		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.clanmenu.invite"], (controller, option) =>
		{
			OpenInviteMenu(player);
		}, !player.MemberInfo.AccesToEditClan);
		
		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.clanmenu.claninfo"], (controller, option) =>
		{
			OpenClanInfoMenu(player);
		});

		menu.ExitButton = true;
		menu.Open(player.player);
	}
	
	public static void OpenInviteMenu(LPlayer? player)
	{
		if (player is null || !player.player.IsValid || !LBaseInfo.Clans.TryGetValue(player.MemberInfo.ClanID, out var clan))
		{
			return;
		}

		var menu = LBaseInfo.Plugin.menuAPI.NewMenu(LBaseInfo.Plugin.Localizer["clans.invite.title"]);
		
		menu.ExitButton = true;

		foreach (var ply in LBaseInfo.LPlayers.Values.Where(ply =>
			         ply != null && LStock.IsValidPlayer(ply.player) && ply.MemberInfo.ClanID == -1))
		{
			if (ply == null)
			{
				continue;
			}

			menu.AddMenuOption(ply.player.PlayerName, (controller, option) =>
			{
				if (ply == null)
				{
					return;
				}
				
				ply.player.PrintToChat(LBaseInfo.Plugin.Localizer["clans.invite.invitemessage", clan.ClanName]);
				LBaseInfo.AwaitToAccept.TryAdd(ply.player.SteamID, clan.ClanID);

				LBaseInfo.Plugin.AddTimer(15.0f, () =>
				{
					if (ply == null)
					{
						return;
					}
					
					LBaseInfo.AwaitToAccept.Remove(ply.player.SteamID);
				}, TimerFlags.STOP_ON_MAPCHANGE);

				LBaseInfo.Plugin.menuAPI.CloseMenu(player.player);
			}, LBaseInfo.AwaitToAccept.ContainsKey(ply.player.SteamID));
		}

		menu.Open(player.player);
	}
	
	public static void ShowTopClans(LPlayer? player)
	{
		Task.Run(async () =>
		{
			var result = await DB.GetTopClans()!;
			
			Server.NextFrame(() =>
			{
				if (player is null || !player.player.IsValid)
				{
					return;
				}

				var menu = LBaseInfo.Plugin.menuAPI.NewMenu(LBaseInfo.Plugin.Localizer["clans.topclans.title"]);
		
				menu.ExitButton = true;

				if (result == null)
				{
					menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.topclans.noclans"],
						(controller, option) => { }, true);
				} else
				{
					foreach (var name in result)
					{
						menu.AddMenuOption(name, (controller, option) => { }, true);
					}
				}
			
				menu.Open(player.player);
			});
		});
	}
	
	public static void OpenSkillsListMenu(LPlayer? player)
	{
		if (player is null || !player.player.IsValid || !LBaseInfo.Clans.TryGetValue(player.MemberInfo.ClanID, out var clan))
		{
			return;
		}

		var menu = LBaseInfo.Plugin.menuAPI.NewMenu(LBaseInfo.Plugin.Localizer["clans.mainmenu.skillslist"]);
		
		menu.ExitButton = true;

		foreach (var skill in SkillsInfo.activeSkills)
		{
			menu.AddMenuOption(
				$"{LBaseInfo.Plugin.Localizer[$"clans.skills.{skill}"]} [{clan.Skills[skill].SkillLevel}]",
				(ply, option) =>
				{
					OpenSkillMenu(player, skill);
				}, !player.MemberInfo.AccesToEditClan);
		}

		menu.Open(player.player);
	}

	public static void OpenSkillMenu(LPlayer? player, string skillName)
	{
		if (player is null || !player.player.IsValid || !LBaseInfo.Clans.TryGetValue(player.MemberInfo.ClanID, out var clan))
		{
			return;
		}

		int maxLevel = -1;
		if (!SkillsInfo.GetMaxLevel(skillName, ref maxLevel))
		{
			return;
		}

		var menu =
			LBaseInfo.Plugin.menuAPI.NewMenu(
				$"{LBaseInfo.Plugin.Localizer["clans.skillmenu.title", LBaseInfo.Plugin.Localizer[$"clans.skills.{skillName}"]]}");

		menu.ExitButton = true;

		menu.AddMenuOption(
			LBaseInfo.Plugin.Localizer["clans.skillmenu.skilllevel", clan.Skills[skillName].SkillLevel],
			(ply, option) => { }, true);
		menu.AddMenuOption(
			LBaseInfo.Plugin.Localizer["clans.skillmenu.skilllevelup"], (ply, option) =>
			{
				if (clan.LevelUp(skillName))
				{
					LBaseInfo.APIEvent.IOnPlayerUpgradeSkill(player, skillName);
					OpenSkillMenu(player, skillName);
				}
			},
			!player.MemberInfo.AccesToEditClan || clan.Skills[skillName].SkillLevel >= maxLevel ||
			maxLevel < 0 || clan.SkillPoints <= 0);

		menu.AddMenuOption(
			LBaseInfo.Plugin.Localizer["clans.skillmenu.skillsell"], (ply, option) =>
			{
				if (clan.Skills[skillName].SkillLevel > 0 && clan.Sell(skillName))
				{
					LBaseInfo.APIEvent.IOnPlayerSellSkill(player, skillName);
					OpenSkillMenu(player, skillName);
				}
			},
			!player.MemberInfo.AccesToEditClan || clan.Skills[skillName].SkillLevel <= 0);
		
		menu.Open(player.player);
	}
	
	public static void OpenClanInfoMenu(LPlayer? player)
	{
		if (player is null || !player.player.IsValid || !LBaseInfo.Clans.TryGetValue(player.MemberInfo.ClanID, out var clan))
		{
			return;
		}

		var menu = LBaseInfo.Plugin.menuAPI.NewMenu(LBaseInfo.Plugin.Localizer["clans.infomenu.title"]);
		
		menu.ExitButton = true;

		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.infomenu.clanname", clan.ClanName],
			(controller, option) => { }, true);
		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.infomenu.skillpoints", clan.SkillPoints],
			(controller, option) => { }, true);
		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.infomenu.level", clan.Level],
			(controller, option) => { }, true);
		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.infomenu.exp", clan.Exp],
			(controller, option) => { }, true);
		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.infomenu.reqexp", clan.ReqExp],
			(controller, option) => { }, true);
		
		menu.Open(player.player);
	}
	
	public static void OpenMembersList(LPlayer? player, List<(ulong steamid, string name, string clanRole, bool clanEdit)> members)
	{
		if (player is null || !player.player.IsValid)
		{
			return;
		}

		var menu = LBaseInfo.Plugin.menuAPI.NewMenu(LBaseInfo.Plugin.Localizer["clans.membersmenu.title"]);
		
		menu.ExitButton = true;

		foreach (var member in members)
		{
			menu.AddMenuOption(member.name, (ply, option) =>
			{
				OpenMemberMenu(player, member);
			}, !player.MemberInfo.AccesToEditClan);
		}
		
		menu.Open(player.player);
	}
	
	public static void OpenMemberMenu(LPlayer? player, (ulong steamid, string name, string clanRole, bool clanEdit) member)
	{
		if (player is null || !player.player.IsValid || !LBaseInfo.Clans.TryGetValue(player.MemberInfo.ClanID, out var clan))
		{
			return;
		}

		var menu = LBaseInfo.Plugin.menuAPI.NewMenu(LBaseInfo.Plugin.Localizer["clans.membersmenu.member", member.name]);
		
		menu.ExitButton = true;

		string clanEdit = LBaseInfo.LPlayers.TryGetValue(member.steamid, out var memb) && memb != null
			? (memb.MemberInfo.AccesToEditClan ? "+" : "-")
			: (member.clanEdit ? "+" : "-");

		menu.AddMenuOption(
			LBaseInfo.Plugin.Localizer["clans.membersmenu.role",
				LBaseInfo.LPlayers.ContainsKey(member.steamid) && LBaseInfo.LPlayers[member.steamid]?.player != null
					? LBaseInfo.LPlayers[member.steamid]!.MemberInfo.ClanRole
					: member.clanRole], (controller, option) => {}, true); // добавить функционал

		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.membersmenu.accesstoedit", clanEdit],
			(controller, option) =>
			{
				if (memb == null || memb.MemberInfo.ClanID != player.MemberInfo.ClanID)
				{
					return;
				}
				
				memb.MemberInfo.AccesToEditClan = !memb.MemberInfo.AccesToEditClan;
				
				OpenMemberMenu(player, member);
			}, !player.MemberInfo.AccesToEditClan);

		menu.AddMenuOption(LBaseInfo.Plugin.Localizer["clans.membersmenu.eject"], (ply, option) =>
		{
			DB.EjectPlayerFromClan(member.steamid, clan.ClanID);
		}, !player.MemberInfo.AccesToEditClan);
		
		menu.Open(player.player);
	}
}