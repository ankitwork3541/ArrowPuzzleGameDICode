using System.Collections.Generic;
using UnityEngine;

// The lantern-festival colours a dragon can wear: lacquer red, gold, jade, orange, rose, lime and
// violet, all deep enough to read on the cream paper board (#FFF8EC).
//
// Two rules govern what is allowed in here:
//
//  1. Nothing close to the two feedback colours. A blocked bump flashes near-black ink and the
//     hint pulses a strong blue (see ArrowController.bumpColor / hintColor), so no dragon may be
//     near-black, and none may sit in the blue range (roughly 195-235 degrees; jade stays at 150,
//     violet at 255).
//  2. Everything stays dark and saturated enough for about 3:1 contrast against cream. That is the
//     reason gold and lime are noticeably deeper than they were on the navy board: a bright gold
//     or lime on cream all but vanishes. The dragons also wear an outline and a soft shadow
//     (ArrowController), since there is no Bloom on a light board.
//
// Deliberately plain LDR colours, not HDR: LineRenderer pushes startColor/endColor into 8-bit
// vertex colours, which clamp at 1.0, so any intensity above that would simply be thrown away.
public static class ArrowPalette
{
    public static readonly Color[] Colors =
    {
        new Color(0.839f, 0.157f, 0.157f),   // Lacquer Red   #D62828     0°
        new Color(0.749f, 0.514f, 0.000f),   // Festival Gold #BF8300    41°
        new Color(0.059f, 0.616f, 0.345f),   // Jade          #0F9D58   150°
        new Color(0.910f, 0.349f, 0.047f),   // Ember Orange  #E8590C    21°
        new Color(0.878f, 0.271f, 0.482f),   // Peony Rose    #E0457B   337°
        new Color(0.369f, 0.580f, 0.000f),   // Lime Lantern  #5E9400    79°
        new Color(0.482f, 0.302f, 1.000f),   // Night Violet  #7B4DFF   255°
    };

    // Hands colours out to one level's arrows. Deals from a shuffled deck rather than rolling a
    // die per arrow: independent rolls collide far more often than they feel like they should —
    // five arrows drawn from seven colours repeat about 85% of the time — and on a small board
    // that reads as a bug rather than as randomness. A deck guarantees every colour is spent
    // before any is spent twice, so a level with seven or fewer arrows has no repeat at all.
    public class Dealer
    {
        private readonly System.Random rng;
        private readonly List<int> deck = new List<int>();
        private int lastDealt = -1;

        // Seed with the level index so a level's colours survive a restart unchanged.
        public Dealer(int seed)
        {
            rng = new System.Random(seed);
        }

        public Color Next()
        {
            if (deck.Count == 0) Refill();

            int top = deck.Count - 1;
            lastDealt = deck[top];
            deck.RemoveAt(top);
            return Colors[lastDealt];
        }

        void Refill()
        {
            for (int i = 0; i < Colors.Length; i++) deck.Add(i);

            // Fisher-Yates. Dealing happens off the end of the list, so the shuffle only has to
            // be uniform — which end is the "top" of the deck does not matter.
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }

            // A fresh deck can open on the same colour the previous one closed on, which is the
            // one back-to-back repeat the whole deck exists to prevent. Bury it.
            int top = deck.Count - 1;
            if (deck.Count > 1 && deck[top] == lastDealt)
            {
                int j = rng.Next(deck.Count - 1);
                (deck[top], deck[j]) = (deck[j], deck[top]);
            }
        }
    }
}
