using BepInEx;
using Ror2Mods.Combat;

namespace Ror2Mods.LethalPoison
{
    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private void OnEnable() => DotLethalityRule.Install(DotLethality.KillsOutsidePlayerTeam(DotKind.Poison));

        private void OnDisable() => DotLethalityRule.Uninstall();
    }
}
