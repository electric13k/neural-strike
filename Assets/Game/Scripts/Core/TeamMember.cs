using UnityEngine;

// ============================================================
//  TEAM MEMBER  — Neural Strike
//  Attach to any entity (player, bot) to give it a team.
//  FlagZone, damage systems, and spawners all read this.
// ============================================================

public class TeamMember : MonoBehaviour
{
    [Tooltip("Alpha | Bravo | Tango | Charlie  (or colour name for Duos)")]
    public string teamName = "Alpha";

    public bool IsEnemy(TeamMember other)
    {
        if (other == null) return false;
        return other.teamName != teamName;
    }
}
