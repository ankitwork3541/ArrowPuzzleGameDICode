# Neon Route Escape — Design System Reference

Source: user-provided design export (2026-08-10, "stitch_neon_route_ui_design"), covering the Neon Futuristic/Cyberpunk art direction locked in `Docs/Game-Design.md` §4. This is the source-of-truth palette/spec for all future theming work (Home panel, Gameplay HUD, arrow re-theming) — reconcile any ad-hoc colors picked earlier (e.g. Splash screen) against this list when revisiting them.

## Core Colors (hex)

| Token | Hex | Usage |
|---|---|---|
| Background | `#050816` | Base app background (Level 0) |
| Surface (panel) | `#101936` | Panels, at 70-80% opacity for glassmorphism |
| Surface (chip/pill) | `#080D20` | Small pill/chip backgrounds (e.g. difficulty badge) |
| Active element surface | `#162451` | Level 2 — active/selected elements, with 1px Neon Cyan border at 40% opacity |
| Nav/container surface | `#192122` | Bottom nav bar background, ~70% opacity |
| Neon Cyan (primary) | `#00E5FF` | Primary accent, CTA gradients, glows |
| Neon Cyan (dim variant) | `#00DAF3` | Secondary cyan (e.g. "LEVEL 01" label) |
| Neon Magenta (secondary) | `#FF2BD6` (also seen as `#FF2FD6`) | Secondary accent, CTA gradient end, glows |
| Locked/muted gray-blue | `#39415F` | Disabled/locked nav items |
| Text on dark | `#DCE4E5` / white | Body/heading text |

**Emissive glow rule**: any neon color gets a 4–12px "bloom" (outer glow) using the *same* hex at ~30% opacity, to simulate light emission. In Unity: TextMeshPro's built-in **Underlay** (Face/Outline/Underlay settings in the material inspector) achieves this without a custom shader — no need to hand-roll a glow shader.

## Typography (3-tier strategy)

| Tier | Font | Used for |
|---|---|---|
| Headings | **Sora** (weight 700–800) | Level numbers, titles, major headers |
| Body | **Hanken Grotesk** (weight 400) | Instructions, descriptions |
| Labels/data | **JetBrains Mono** (weight 600, uppercase, letter-spacing 0.1em) | Move counters, timers, small metadata — reinforces the "computer interface" feel |

Exact sizes (reference, px — convert to Unity font sizes as needed): display-lg 40px/800, display-lg-mobile 32px/800, headline-md 24px/700, body-lg 18px/400, body-sm 14px/400, label-caps 12px/600.

## Spacing & Shape

- 4px base unit; common steps: xs 4, sm 8, md 16, lg 24, xl 32, gutter 16, safe-margin 20.
- Corner radius: sm 4px, default 8px, md 12px, lg 16px, xl 24px, full = pill (9999px).
- Borders: 1–2px stroke; for a "premium" feel, a linear gradient stroke fading from a neon color to transparent (or a solid neon color as a simpler v1 approximation — see Progress Log for what was actually built).

## Brand Pillars (for future decisions)

- **Luminance Contrast**: neon light cutting through a deep dark void — keep backgrounds genuinely dark, let neon "pop."
- **Atmospheric Depth**: translucent stacked panels simulating a HUD (glassmorphism) — background blur is a CSS-only trick, not directly available in stock Unity UI; approximate with semi-transparent dark panels instead of chasing true blur.
- **Kinetic Energy**: thin borders + gradients implying flow/movement.
- **Arrows/Path Markers spec**: when a piece is "active," it should pulse opacity 60%→100% on a 0.5s ease-in-out loop — relevant once arrow/grid re-theming is tackled (see Game-Design.md §4, still pending).

## Component Notes

- **CTA Buttons** (e.g. PLAY): diagonal gradient `#00E5FF` → `#FF2BD6`, subtle white inner-glow at top, heavy outer-bloom in the primary color, label-caps text.
- **Chips/Pills** (e.g. difficulty badge): `#080D20` background + neon stroke matching the stat/category.
- **Lists**: rows separated by a 1px horizontal gradient line fading at the edges, not boxed containers.

## Full original files

Kept for reference, not copied into the repo: `code.html` (a full HTML/Tailwind mockup of the Home/Level-select screen) and `screen.png` (rendered screenshot) — both in the design export the user provided. The HTML was read directly to extract the specifics captured above; not reproducible 1:1 in Unity UI (e.g. `backdrop-filter: blur()` has no direct Unity UI equivalent) — see `CLAUDE.md` Progress Log for what was actually built vs. approximated.
