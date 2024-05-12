namespace Clans.components;

public class LMemberInfo
{
	public string ClanRole { get; set; } = "";
	public bool AccesToEditClan { get; set; } = false;
	public long ClanID { get; set; } = -1;

	public LMemberInfo(string clanRole, bool clanEdit, long clanid)
	{
		ClanRole = clanRole;
		AccesToEditClan = clanEdit;
		ClanID = clanid;
	}
}