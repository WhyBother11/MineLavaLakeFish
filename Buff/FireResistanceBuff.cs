using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MineLavaLakeFish.HarmonyUtil;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace MineLavaLakeFish.Buff;

public static class FireResistanceBuff
{
    private static readonly string BuffName = $"{ModProperties.UniqueID}CP_FireResistance";

    private static ModLog Log;

    public static void ApplyPatches(Harmony harmony, ModLog log)
    {
        Log = log;
        log.Trace($"Applying prefix patch {nameof(Farmer_takeDamage_Prefix)}");
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)),
            prefix: new HarmonyMethod(typeof(FireResistanceBuff), nameof(Farmer_takeDamage_Prefix))
        );
        
        log.Trace($"Applying prefix patch {nameof(BasicProjectile_behaviorOnCollisionWithPlayer_Prefix)}");
        harmony.Patch(
            original: AccessTools.Method(typeof(BasicProjectile),
                nameof(BasicProjectile.behaviorOnCollisionWithPlayer)),
            prefix: new HarmonyMethod(typeof(FireResistanceBuff),
                nameof(BasicProjectile_behaviorOnCollisionWithPlayer_Prefix))
        );
        
        log.Trace($"Applying postfix patch {nameof(Bat_onDealContactDamage_Postfix)}");
        harmony.Patch(
            original: AccessTools.Method(typeof(Bat), nameof(Bat.onDealContactDamage)),
            postfix: new HarmonyMethod(typeof(FireResistanceBuff), nameof(Bat_onDealContactDamage_Postfix))
        );

        log.Trace($"Applying transpile patch {nameof(DinoMonster_BreathProjectile_Update_Transpiler)}");
        harmony.Patch(
            original: AccessTools.Method(typeof(DinoMonster.BreathProjectile), nameof(DinoMonster.BreathProjectile.Update)),
            transpiler: new HarmonyMethod(typeof(FireResistanceBuff), nameof(DinoMonster_BreathProjectile_Update_Transpiler))
        );
    }

    /// <summary>
    /// Prefix to adjust fire damage from actual monster attacks.
    /// </summary>
    /// <param name="__instance">Farmer instance</param>
    /// <param name="damage">See <see cref="Farmer.takeDamage"/></param>
    /// <param name="damager">See <see cref="Farmer.takeDamage"/></param>
    public static void Farmer_takeDamage_Prefix(
        Farmer __instance,
        ref int damage,
        Monster damager)
    {
        try
        {
            if (__instance.HasBuff() && damager.IsMagmaSprite())
            {
                damage /= 2;
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to execute prefix patch {nameof(Farmer_takeDamage_Prefix)} for Farmer::takeDamage.", e);
        }
    }

    /// <summary>
    /// Prefix to adjust fire damage from projectiles fired by monsters.
    /// </summary>
    /// <param name="__instance">BasicProjectile instance</param>
    /// <param name="location">See <see cref="BasicProjectile.behaviorOnCollisionWithPlayer"/></param>
    /// <param name="player">See <see cref="BasicProjectile.behaviorOnCollisionWithPlayer"/></param>
    public static void BasicProjectile_behaviorOnCollisionWithPlayer_Prefix(
        BasicProjectile __instance,
        GameLocation location,
        Farmer player)
    {
        try
        {
            if (!player.HasBuff()) return;
            var firer = __instance.theOneWhoFiredMe.Get(location);
            if (firer is Monster monster && monster is LavaLurk or SquidKid)
            {
                __instance.damageToFarmer.Set(__instance.damageToFarmer.Value / 2);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to execute prefix patch {nameof(BasicProjectile_behaviorOnCollisionWithPlayer_Prefix)} for BasicProjectile::behaviorOnCollisionWithPlayer.", e);
        }
    }

    /// <summary>
    /// Postfix to remove Burnt debuff
    /// </summary>
    /// <param name="__instance">Bat instance</param>
    /// <param name="who">See <see cref="Bat.onDealContactDamage"/></param>
    public static void Bat_onDealContactDamage_Postfix(
        Bat __instance,
        Farmer who)
    {
        try
        {
            if (__instance.IsMagmaSprite() && who.HasBuff())
            {
                who.buffs.Remove(StardewValley.Buff.goblinsCurse); // remove Burnt debuff
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to execute postfix patch {nameof(Bat_onDealContactDamage_Postfix)} for Bat::onDealContactDamage", e);
        }
    }

    /// <summary>
    /// Check if monster is MagmaSprite or MagmaSparker.
    /// </summary>
    /// <param name="monster">Monster instance</param>
    /// <returns>Monster is dealing fire damage</returns>
    public static bool IsMagmaSprite(this Monster monster)
    {
        if (monster is Bat bat)
        {
            return bat.magmaSprite.Value;
        }

        return false;
    }

    /// <summary>
    /// Patches DinoMonster.BreathProjectile::Update to reduce breath projectile damage to player.
    /// </summary>
    /// <param name="instructions">Original IL instructions</param>
    /// <returns>Modified IL instructions</returns>
    public static IEnumerable<CodeInstruction> DinoMonster_BreathProjectile_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            var matcher = new CodeMatcher(instructions);
            // find instruction for breath projectile damage value
            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)25)
            );
            if (matcher.IsInvalid)
            {
                // minor because hardcoding is not cool and if some other patch changes this method so be it
                throw new ModHarmonyException("Cannot find base damage to farmer, it was probably already replaced by another patch.\nPatch to DinoMonster.BreathProjectile will be ignored.", HarmonyExceptionSeverity.MINOR);
            }

            matcher.RemoveInstruction(); // remove hardcoded damage value
            var getPlayer = typeof(Game1).GetProperty(nameof(Game1.player), BindingFlags.Public | BindingFlags.Static).GetGetMethod();
            // insert check for buff and halved damage
            matcher.Insert(
                new CodeInstruction(
                    OpCodes.Call,
                    getPlayer),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(typeof(FireResistanceBuff), nameof(GetProjectileDamage)))
            );
            return matcher.InstructionEnumeration();
        }
        catch (Exception e)
        {
            if (e is ModHarmonyException harmonyException)
            {
                if (harmonyException.Severity == HarmonyExceptionSeverity.SEVERE)
                    Log.Error($"{harmonyException.Message}.");
                else Log.Warn($"{harmonyException.Message}.");
            } else Log.Error($"Failed to inject DinoMonster fire breathing damage reduction.\nException: {e}");
            return instructions;
        }
    }

    public static int GetProjectileDamage(Farmer who)
    {
        if (who.HasBuff()) return 25 / 2;
        else return 25;
    }

    private static bool HasBuff(this Farmer who)
    {
        return who.hasBuff(BuffName);
    }
}