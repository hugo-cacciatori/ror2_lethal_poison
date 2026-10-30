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
            ctx.InflictDot(ally, acrid, DotController.DotIndex.Poison, 60f);
            yield return ctx.WaitUntil(() => !ctx.IsAlive(ally) || ally.healthComponent.health <= 1.01f, 30f);
            yield return ctx.Wait(3f);
            ctx.Assert(ctx.IsAlive(ally), ally
                ? $"the poisoned ally died (death: {Describe(ctx.DeathOf(ally))}, hp {ally.healthComponent.health}, y {ally.corePosition.y:F0})"
                : $"the poisoned ally's body is gone (death: {Describe(ctx.DeathOf(ally))})");
            ctx.Assert(ally.healthComponent.health <= 1.01f, $"poison did not bring the ally down to 1 HP (hp {ally.healthComponent.health})");
            ctx.Assert(ally.HasBuff(RoR2Content.Buffs.Poisoned), "the ally is no longer poisoned, so the 1 HP floor was not exercised");
        }
    }
}
