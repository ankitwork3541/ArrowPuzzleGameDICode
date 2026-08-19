using System.Collections.Generic;
using UnityEngine;

// The neon body colours an arrow can wear. The four the Neon Route design system ships with,
// widened to seven so a big board reads as a spread rather than four repeating stripes.
//
// Two rules govern what is allowed in here:
//
//  1. Hues stay inside roughly 157°-340° — spring green round through cyan, blue, purple and
//     magenta to pink. The red and yellow side of the wheel is left deliberately empty because
//     those two are state colours: red is a blocked bounce, yellow is the hint pulse. An arrow
//     sitting there in its resting colour would read as feedback that never happened.
//  2. Everything stays bright and saturated. Glow comes from URP Bloom with its threshold pulled
//     below 1, so a dim colour simply never crosses it and lands on the board looking flat.
//
// Deliberately plain LDR colours, not HDR: LineRenderer pushes startColor/endColor into 8-bit
// vertex colours, which clamp at 1.0, so any intensity above that would simply be thrown away.
public static class ArrowPalette
{
    public static readonly Color[] Colors =
    {
        new Color(0.000f, 1.000f, 0.616f),   // Neon Green    #00FF9D   157°
        new Color(0.000f, 0.898f, 1.000f),   // Neon Cyan     #00E5FF   187°
        new Color(0.161f, 0.475f, 1.000f),   // Electric Blue #2979FF   219°
        new Color(0.545f, 0.361f, 1.000f),   // Neon Purple   #8B5CFF   258°
        new Color(0.761f, 0.302f, 1.000f),   // Neon Violet   #C24DFF   285°
        new Color(1.000f, 0.169f, 0.839f),   // Neon Magenta  #FF2BD6   314°
        new Color(1.000f, 0.302f, 0.553f),   // Hot Pink      #FF4D8D   340°
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
