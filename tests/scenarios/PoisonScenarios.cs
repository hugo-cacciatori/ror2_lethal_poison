using System.Collections;
using RoR2;
using Ror2Mods.GateProbe;

// In-game scenarios run by the boot gate in a real run (host, no local user). Spawned bodies have no AI.
namespace Ror2Mods.LethalPoison.Scenarios
{
    public static class PoisonScenarios
    {
        private static string Describe(DeathRecord death) =>
            death == null ? "not reported" : $"dot {death.DotIndex}, type {death.DamageType}, attacker {(death.Attacker ? death.Attacker.name : "none")}";

        [Scenario("poison kills a monster")]
        public static IEnumerator PoisonKillsMonster(ScenarioContext ctx)
        {
            var acrid = ctx.Spawn("CrocoMonsterMaster", TeamIndex.Monster);
            var beetle = ctx.Spawn("BeetleMaster", TeamIndex.Monster);
            ctx.InflictDot(beetle, acrid, DotController.DotIndex.Poison, 60f);
            yield return ctx.WaitUntil(() => !ctx.IsAlive(beetle), 30f);
            var death = ctx.DeathOf(beetle);
            ctx.Assert(death != null, "the poisoned monster is still alive after 30 s");
            ctx.Assert(death.DotIndex == DotController.DotIndex.Poison, $"the monster died from {death.DotIndex}, not from poison");
        }

        [Scenario("poison never kills the player team")]
        public static IEnumerator PoisonSparesPlayerTeam(ScenarioContext ctx)
        {
            var acrid = ctx.Spawn("CrocoMonsterMaster", TeamIndex.Monster);
            var ally = ctx.Spawn("BeetleMaster", TeamIndex.Player);
            yield return ctx.Wait(2f); // settle on the ground first
            ctx.Assert(ctx.IsAlive(ally), $"the ally died before being poisoned ({Describe(ctx.DeathOf(ally))})");
            ctx.InflictDot(ally, acrid, DotController.DotIndex.Poison, 60f);
            var lowest = float.MaxValue;
            yield return ctx.WaitUntil(() =>
            {
                if (ctx.IsAlive(ally)) lowest = System.Math.Min(lowest, ally.healthComponent.health);
                return !ctx.IsAlive(ally) || lowest <= 1.01f;
            }, 30f);
            yield return ctx.Wait(3f); // several more poison ticks at 1 HP
            var death = ctx.DeathOf(ally);
            ctx.Assert(death == null || death.DotIndex != DotController.DotIndex.Poison, $"poison killed the ally ({Describe(death)})");
            ctx.Assert(lowest <= 1.01f, $"poison never brought the ally down to 1 HP (lowest {lowest})");
            // At 1 HP any stray non-poison damage kills; that is outside this rule, so only a poison death fails.
        }
    }
}
