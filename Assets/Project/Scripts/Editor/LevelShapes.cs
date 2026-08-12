// ASCII silhouettes for the shape levels. Top row first, so each literal reads the way the shape
// looks on screen; '.' is empty, '#' is filled. LevelCarver.FromAscii flips the rows, because grid
// y grows upward.
//
// Two rules these must obey, both verified by LevelCarver.CountRegions before generating:
//   - 4-connected. A detached island of 1 cell can never hold an arrow (they need 2) and would
//     strand. Detached LARGER blocks are fine and deliberate — one reference level is a shape with
//     a separate block under it.
//   - No 1-cell-wide dead-end spurs sticking out, for the same reason.
//
// A 1-cell-wide corridor is fine: an arrow is a path, so it can run down a single-cell channel.
// Authored small and scaled up by LevelCarver.Upscale, so one silhouette serves any difficulty.
public static class LevelShapes
{
    public static readonly string[] Heart =
    {
        "..####....####..",
        ".######..######.",
        "################",
        "################",
        "################",
        ".##############.",
        "..############..",
        "...##########...",
        "....########....",
        ".....######.....",
        "......####......",
        ".......##......."
    };

    public static readonly string[] Bulb =
    {
        "....######....",
        "..##########..",
        ".############.",
        "##############",
        "##############",
        "##############",
        "##############",
        ".############.",
        "..##########..",
        "...########...",
        "....######....",
        "...########...",
        "....######....",
        "...########...",
        "....######....",
        ".....####....."
    };

    public static readonly string[] Teardrop =
    {
        ".......##.......",
        ".......##.......",
        "......####......",
        "......####......",
        ".....######.....",
        "....########....",
        "...##########...",
        "..############..",
        ".##############.",
        "################",
        "################",
        "################",
        ".##############.",
        "..############..",
        "...##########...",
        ".....######....."
    };

    public static readonly string[] Cat =
    {
        ".##........##.",
        ".####....####.",
        ".############.",
        "##############",
        "##############",
        "##############",
        "##############",
        ".############.",
        "...########...",
        "...########...",
        "...########...",
        "..##########..",
        ".############.",
        "##############"
    };

    public static readonly string[] Gun =
    {
        "##############",
        "##############",
        "##.......#####",
        "##.......#####",
        "##....########",
        "##....###.....",
        ".##....###....",
        ".##....###....",
        "..##....###...",
        "..##....###...",
        "...##....###..",
        "...#########..",
        "....########.."
    };
}
