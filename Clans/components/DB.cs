using CounterStrikeSharp.API;
using Npgsql;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Clans.components;

public class DB
{
	private static NpgsqlConnection GetConnection()
	{
		var connection = new NpgsqlConnectionStringBuilder
		{
			Host = LBaseInfo.Config.Host,
			Database = LBaseInfo.Config.Database,
			Password = LBaseInfo.Config.Password,
			Username = LBaseInfo.Config.UserName,
			Port = LBaseInfo.Config.Port,
			SslMode = SslMode.Prefer
		}.ConnectionString;
		return new NpgsqlConnection(connection);
	}

	public static async Task InitTable()
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();

		await conn.QueryAsync(@"CREATE TABLE IF NOT EXISTS lclans_players(
                        id				BIGSERIAL NOT NULL PRIMARY KEY,
                        steamid			VARCHAR(64) NOT NULL,
						name			VARCHAR(128) NOT NULL,
						clanid			INT NOT NULL default -1,
						clanRole		VARCHAR(128) NOT NULL default '',
						clanEdit		BOOLEAN NOT NULL default FALSE,
						UNIQUE (steamid));");

		await conn.QueryAsync(@"CREATE TABLE IF NOT EXISTS lclans_clans(
                        id				BIGSERIAL NOT NULL PRIMARY KEY,
                        clanLeader		VARCHAR(64) NOT NULL,
						clanName		VARCHAR(64) NOT NULL,
						skillPoints		INT NOT NULL default 0,
						level			INT NOT NULL default 1,
						exp				INT NOT NULL default 0,
						maxMembers		INT NOT NULL default 1,
						UNIQUE (clanLeader));");
		
		await conn.QueryAsync(@"CREATE TABLE IF NOT EXISTS lclans_skills(
                        id				BIGSERIAL NOT NULL PRIMARY KEY,
                        clanid			VARCHAR(64) NOT NULL,
						skillName		VARCHAR(64) NOT NULL,
						level			INT NOT NULL default 0,
						UNIQUE (clanid, skillName));");

		await InitClans();
	}
	
	public static async Task InitClans()
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();
	
		Dictionary<long, LClan> clans;
		
		try
		{
			clans = (await conn.QueryAsync<LClan>(
				@$"SELECT clanName, maxMembers, skillPoints, exp, level, clanLeader, id FROM lclans_clans")).ToDictionary(c => c.ClanID, c => c);
		}
		catch
		{
			return;
		}

		HashSet<LClanSkill> HSkills;
		IEnumerable<string> notRegisteredSkills;

		foreach (var c in clans)
		{
			HSkills =
				(await conn.QueryAsync<LClanSkill>(
					$"SELECT level AS skillLevel, skillName FROM lclans_skills WHERE clanid = '{c.Key}';"))
				.ToHashSet();
	
			notRegisteredSkills =
				SkillsInfo.activeSkills.Where(s1 => HSkills.All(s2 => !s1.ToLower().Equals(s2.SkillName.ToLower())));
	
			foreach (var skill in notRegisteredSkills)
			{
				c.Value.RegisterSkill(new LClanSkill(0, skill));
			}

			foreach (var skill in HSkills)
			{
				c.Value.RegisterSkill(skill);
			}
		}

		LBaseInfo.Clans = clans;
	}

	public static async Task<LMemberInfo> InitPlayer(ulong steamid)
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();

		// LClan? clan = null;
		LMemberInfo memberInfo;

		try
		{
			memberInfo = (await conn.QueryAsync<LMemberInfo>(@$"SELECT clanRole, clanEdit, clanid FROM lclans_players WHERE steamid = '{steamid}'")).First();
		}
		catch
		{
			return new LMemberInfo("", false, -1);
		}
		
		// try
		// {
		// 	clan = (await conn.QueryAsync<LClan>(
		// 		@$"SELECT clanName, maxMembers, skillPoints, exp, level, clanLeader, id FROM lclans_clans WHERE id = '{memberInfo.ClanID}'")).First();
		// }
		// catch
		// {
		// 	clan = null;
		// }
		//
		// HashSet<LClanSkill> HSkills =
		// 	(await conn.QueryAsync<LClanSkill>(
		// 		$"SELECT level AS skillLevel, skillName FROM lclans_skills WHERE clanid = '{memberInfo.ClanID}';"))
		// 	.ToHashSet();
		//
		// var notRegisteredSkills =
		// 	SkillsInfo.activeSkills.Where(s1 => HSkills.All(s2 => !s1.ToLower().Equals(s2.SkillName.ToLower())));
		//
		// foreach (var skill in notRegisteredSkills)
		// {
		// 	HSkills.Add(new LClanSkill(0, skill));
		// }
		
		
		
		return memberInfo;
	}
	
	public static async Task SavePlayer(LPlayer player, string playerName)
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();

		var query =
			$@"INSERT INTO lclans_players(steamid, name, clanid, clanRole, clanEdit) 
			VALUES('{player.steamid}', '{playerName}', {player.MemberInfo.ClanID}, {player.MemberInfo.ClanRole}, {player.MemberInfo.AccesToEditClan}) 
			ON CONFLICT (steamid) DO UPDATE SET name = '{playerName}', clanid = {player.MemberInfo.ClanID}, clanRole = {player.MemberInfo.ClanRole},
			clanEdit = {player.MemberInfo.AccesToEditClan};";

		await conn.QueryAsync(query);

		// Перенос на момент прокачки умения real-time
		// if (player.Clan == null)
		// {
		// 	return;
		// }
		//
		// foreach (LClanSkill skill in player.Clan.ClanSkills.Values)
		// {
		// 	query =
		// 		$"INSERT INTO lclans_skills(clanid, skillName, level) VALUES('{player.MemberInfo.ClanID}', '{skill.SkillName}', {skill.SkillLevel}) ON CONFLICT (clanid, skillname) DO UPDATE SET level = {skill.SkillLevel};";
		// 	
		// 	await conn.QueryAsync(query);
		// }
	}

	// Возможность существования кланов с одинаковыми названиями; Bad or No?
	public static async Task CreateClan(LPlayer player, string clanName)
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();

		var query = $@"SELECT id FROM lclans_clans WHERE clanName = '{clanName}';";

		try
		{
			(await conn.QueryAsync<long>(query)).First();
		}
		catch
		{
			query = $@"INSERT INTO lclans_clans(clanLeader, clanName, maxMembers)
						VALUES('{player.steamid}', '{clanName}', {LBaseInfo.Config.MaxClanMembersPerLevel}) RETURNING id;";
			try
			{
				var clanid = (await conn.QueryAsync<long>(query)).First();

				// player.Clan = new LClan(clanName, LBaseInfo.Config.MaxClanMembersPerLevel, 0, 0, 1, player.steamid, clanid);

				LBaseInfo.Clans.TryAdd(clanid, new LClan(clanName, LBaseInfo.Config.MaxClanMembersPerLevel, 0, 0, 1, player.steamid, clanid));
				
				player.MemberInfo.ClanRole = "Leader";
				player.MemberInfo.AccesToEditClan = true;
				player.MemberInfo.ClanID = clanid;
				
				Server.NextFrame(() =>
				{
					player.player.PrintToChat("Clan was created");
				});
			}
			catch
			{
				Server.NextFrame(() =>
				{
					LBaseInfo.Plugin.Logger.LogError("[CLANS] Failed to create clan; clanid is not returned");
				});
			}
		}
	}

	public static async Task DeleteClan(long ClanID)
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();
		
		var query = $@"DELETE FROM lclans_clans WHERE clanid = {ClanID};";
		await conn.QueryAsync(query);

		query = $@"UPDATE lclans_players SET clanid = -1, clanRole = '', clanEdit = FALSE WHERE clan = {ClanID};";
		await conn.QueryAsync(query);

		LBaseInfo.Clans.Remove(ClanID);

		foreach (LPlayer? player in LBaseInfo.LPlayers.Values)
		{
			if (player == null || player.MemberInfo.ClanID != ClanID)
			{
				continue;
			}
			
			player.MemberInfo.ClanID = -1;
			player.MemberInfo.ClanRole = "";
			player.MemberInfo.AccesToEditClan = false;
			
			Server.NextFrame(() =>
			{
				player.player.PrintToChat(LBaseInfo.Plugin.Localizer["clans.clan.disband"]);
			});
		}
	}
	
	public static async Task<List<(ulong steamid, string name, string clanRole, bool accessToEdit)>?> ShowMembersList(long ClanID)
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();
		
		var query = $@"SELECT steamid, name, clanRole, clanEdit FROM lclans_players WHERE clanid = {ClanID};";

		try
		{
			var data = (await conn.QueryAsync<(ulong, string, string, bool)>(query)).ToList();
			return data;
		}
		catch
		{
			return null;
		}
	}
	
	public static async Task EjectPlayerFromClan(ulong steamid, long ClanID)
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();
		
		var query = $@"UPDATE lclans_players SET clanid = -1, clanRole = '', clanEdit = FALSE WHERE clan = {ClanID} AND steamid = '{steamid}';";
		await conn.QueryAsync(query);

		if (!LBaseInfo.LPlayers.TryGetValue(steamid, out var player) && player != null &&
		    player.MemberInfo.ClanID == ClanID)
		{
			player.MemberInfo.ClanID = -1;
			player.MemberInfo.ClanRole = "";
			player.MemberInfo.AccesToEditClan = false;

			Server.NextFrame(() => { player.player.PrintToChat(LBaseInfo.Plugin.Localizer["clans.clan.disband"]); });
		}
	}
}