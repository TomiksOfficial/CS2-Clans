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
						clanid			BIGINT NOT NULL default -1,
						clanRole		VARCHAR(128) NOT NULL default '',
						clanEdit		INT NOT NULL default 0,
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
                        clanid			BIGINT NOT NULL,
						skillName		VARCHAR(64) NOT NULL,
						level			INT NOT NULL default 0,
						UNIQUE (clanid, skillName));");

		// await InitClans();
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
		
		// Console.WriteLine("INIT PLAYER TEST 1");
		
		try
		{
			memberInfo = (await conn.QueryAsync<LMemberInfo>(@$"SELECT clanRole, clanEdit, clanid FROM lclans_players WHERE steamid = '{steamid}';")).First();
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
			// Console.WriteLine("INIT PLAYER TEST 1.1");
			return new LMemberInfo("", 0, -1);
		}
		
		// Console.WriteLine("INIT PLAYER TEST 2");

		if (memberInfo.ClanID != -1 && !LBaseInfo.Clans.ContainsKey(memberInfo.ClanID))
		{
			LClan? clan;
			
			// Console.WriteLine("INIT PLAYER TEST 3");
			try
			{
				clan = (await conn.QueryAsync<LClan>(
					@$"SELECT clanName, maxMembers, skillPoints, exp, level, clanLeader, id FROM lclans_clans WHERE id = {memberInfo.ClanID}")).First();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				return memberInfo;
			}

			HashSet<LClanSkill> HSkills = new HashSet<LClanSkill>();

			try
			{
				HSkills =
					(await conn.QueryAsync<LClanSkill>(
						$"SELECT level AS skillLevel, skillName FROM lclans_skills WHERE clanid = {memberInfo.ClanID};"))
					.ToHashSet();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				HSkills = new HashSet<LClanSkill>();
			}
			
			// Console.WriteLine("INIT PLAYER TEST 3.1");

			HashSet<string> notRegisteredSkills;
			
			// Console.WriteLine("INIT PLAYER TEST 3.11");
			
			try
			{
				// Console.WriteLine("INIT PLAYER TEST 3.12");
				notRegisteredSkills =
					SkillsInfo.activeSkills.Where(s1 => HSkills.All(s2 => !s1.ToLower().Equals(s2.SkillName.ToLower()))).ToHashSet();
				// Console.WriteLine("INIT PLAYER TEST 3.13");
			}
			catch (Exception e)
			{
				// Console.WriteLine("INIT PLAYER TEST 3.14");
				// Console.WriteLine(e);
				// Console.WriteLine("INIT PLAYER TEST 3.15");
				notRegisteredSkills = SkillsInfo.activeSkills;
				// Console.WriteLine("INIT PLAYER TEST 3.16");
			}
		
			// Console.WriteLine("INIT PLAYER TEST 3.17: " + notRegisteredSkills.Count);
			
			foreach (var skill in notRegisteredSkills)
			{
				// Console.WriteLine("INIT PLAYER TEST CLAN SKILL ADDED 1");
				HSkills.Add(new LClanSkill(0, skill));
				// Console.WriteLine("INIT PLAYER TEST CLAN SKILL ADDED 2");
			}
			
			// Console.WriteLine("INIT PLAYER TEST 3.2");
			
			foreach (var skill in HSkills)
			{
				// Console.WriteLine("Registering Skill");
				if (clan.RegisterSkill(skill))
				{
					// Console.WriteLine("GOOD ADD SKILL");
				}
			}
			
			// Console.WriteLine("INIT PLAYER TEST 3.21");

			if (!LBaseInfo.Clans.TryAdd(clan.ClanID, clan))
			{
				// Console.WriteLine("FAILED TO ADD CLAN");
			}
			
			// Console.WriteLine("INIT PLAYER TEST 3.3");
		}
		
		// Console.WriteLine("INIT PLAYER TEST 4");

		// if (memberInfo == null)
		// {
		// 	Console.WriteLine("INIT PLAYER TEST 5");
		// }
		
		return memberInfo;
	}
	
	public static async Task SavePlayer(LPlayer player, string playerName)
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();

		var query =
			$@"INSERT INTO lclans_players(steamid, name, clanid, clanRole, clanEdit) 
			VALUES('{player.steamid}', '{playerName}', {player.MemberInfo.ClanID}, '{player.MemberInfo.ClanRole}', {Convert.ToInt16(player.MemberInfo.AccesToEditClan)}) 
			ON CONFLICT (steamid) DO UPDATE SET name = '{playerName}', clanid = {player.MemberInfo.ClanID}, clanRole = '{player.MemberInfo.ClanRole}',
			clanEdit = {Convert.ToInt16(player.MemberInfo.AccesToEditClan)};";

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

				LBaseInfo.Clans.TryAdd(clanid, new LClan(clanName, LBaseInfo.Config.MaxClanMembersPerLevel, 0, 0, 1, Convert.ToString(player.steamid), clanid));

				foreach(string skill in SkillsInfo.activeSkills)
				{
					LBaseInfo.Clans[clanid].RegisterSkill(new LClanSkill(0, skill));
				}
				
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
		
		Server.NextFrame(() =>
		{
			if (LBaseInfo.LPlayers.TryGetValue(steamid, out var player) && player != null &&
			    player.MemberInfo.ClanID == ClanID)
			{
				player.MemberInfo.ClanID = -1;
				player.MemberInfo.ClanRole = "";
				player.MemberInfo.AccesToEditClan = false;

				player.player.PrintToChat(LBaseInfo.Plugin.Localizer["clans.clan.disband"]);
			}
		});
	}

	public static async Task SaveClan(LClan? clan)
	{
		if (clan == null)
		{
			return;
		}
		
		await using var conn = GetConnection();
		await conn.OpenAsync();

		var query =
			$@"UPDATE lclans_clans SET clanLeader = '{clan.ClanLeader}', clanName = '{clan.ClanName}', skillPoints = {clan.SkillPoints}, 
			level = {clan.Level}, exp = {clan.Exp}, maxMembers = {clan.MaxMembers} WHERE id = {clan.ClanID};";

		await conn.QueryAsync(query);
		
		foreach (LClanSkill skill in clan.Skills.Values)
		{
			query =
				$"INSERT INTO lclans_skills(clanid, skillName, level) VALUES({clan.ClanID}, '{skill.SkillName}', {skill.SkillLevel}) ON CONFLICT (clanid, skillname) DO UPDATE SET level = {skill.SkillLevel};";
			
			await conn.QueryAsync(query);
		}
	}
	
	public static async Task<List<string>?> GetTopClans()
	{
		await using var conn = GetConnection();
		await conn.OpenAsync();
		
		var query = $@"SELECT clanName from lclans_clans ORDER BY level DESC LIMIT 10;";
		var result = (await conn.QueryAsync<string>(query)).ToList();

		return result;
	}
}