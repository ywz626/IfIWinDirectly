using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Win.script.cards;

namespace Win.script.patches;

[HarmonyPatch(typeof(Player), "PopulateStartingDeck")]
public static class StartingDeckPatch
{
    static void Postfix(Player __instance)
    {
        // Silent 角色的 ID 是 SILENT（Slugify 后全大写）
        if (__instance.Character.Id.Entry != "SILENT")
            return;

        // StrikeSilent 的 ID 是 STRIKE_SILENT
        var strike = __instance.Deck.Cards.FirstOrDefault(c => c.Id.Entry == "STRIKE_SILENT");
        if (strike != null)
        {
            __instance.Deck.RemoveInternal(strike, true);
            Log.Info($"[WinMod] Removed {strike.Id.Entry} from starting deck");
        }
        else
        {
            Log.Warn("[WinMod] Could not find STRIKE_SILENT in starting deck");
        }

        var card = ModelDb.Card<IfIWinDirectly>().ToMutable();
        card.FloorAddedToDeck = 1;
        __instance.Deck.AddInternal(card, -1, true);
        Log.Info($"[WinMod] Added {card.Id.Entry} to starting deck");
    }
}