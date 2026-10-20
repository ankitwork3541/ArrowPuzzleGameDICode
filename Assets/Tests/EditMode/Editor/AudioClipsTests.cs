using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The shipped sound set against production/audio/lantern-dragons-sfx/sfx-spec.md: every clip exists,
// has the right channel count and length, and none peaks above the spec's ceiling. Nobody can listen
// to a unit test, so this only guards the measurable half of the spec.
public class AudioClipsTests
{
    private const string Folder = "Assets/Project/Audio/";
    private const string ScenePath = "Assets/Project/Scenes/LanternDragons.unity";

    // Spec: never above -3 dBFS peak (a hair of slack for 16-bit rounding).
    private const float PeakLimit = 0.7085f;

    // name, minSeconds, maxSeconds, channels
    private static readonly object[][] Clips =
    {
        new object[] { "Lantern_DragonTap", 0.15f, 0.35f, 1 },
        new object[] { "Lantern_DragonTap_B", 0.15f, 0.35f, 1 },
        new object[] { "Lantern_DragonTap_C", 0.15f, 0.35f, 1 },
        new object[] { "Lantern_DragonBump", 0.25f, 0.50f, 1 },
        new object[] { "Lantern_DragonBump_B", 0.25f, 0.50f, 1 },
        new object[] { "Lantern_UIClick", 0.04f, 0.12f, 1 },
        new object[] { "Lantern_LockShake", 0.20f, 0.41f, 1 },
        new object[] { "Lantern_LockShake_B", 0.20f, 0.41f, 1 },
        new object[] { "Lantern_Unlock", 0.40f, 0.70f, 1 },
        new object[] { "Lantern_DragonExit", 0.30f, 0.60f, 1 },
        new object[] { "Lantern_HeartLost", 0.20f, 0.36f, 1 },
        new object[] { "Lantern_HintPulse", 0.40f, 0.70f, 1 },
        new object[] { "Lantern_LevelEntrance", 0.40f, 0.80f, 1 },
        new object[] { "Lantern_MapNodeTap", 0.08f, 0.20f, 1 },
        new object[] { "Lantern_SheetOpen", 0.15f, 0.30f, 1 },
        new object[] { "Lantern_SheetClose", 0.15f, 0.30f, 1 },
        new object[] { "Lantern_ToggleOn", 0.05f, 0.12f, 1 },
        new object[] { "Lantern_ToggleOff", 0.05f, 0.12f, 1 },
        new object[] { "Lantern_Win", 2.00f, 3.50f, 2 },
        new object[] { "Lantern_Lose", 1.20f, 2.00f, 2 },
    };

    private static AudioClip Load(string name)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + name + ".wav");
        Assert.IsNotNull(clip, name + " is missing from " + Folder);
        return clip;
    }

    // Peak is read from the source WAV on disk, not from the imported clip: a Compressed In Memory
    // clip cannot be read with GetData in the Editor, and the delivered file is what the spec limits.
    private static float PeakOf(AudioClip clip)
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(clip));
        byte[] bytes = File.ReadAllBytes(path);

        int pos = 12;   // after "RIFF....WAVE"
        while (pos + 8 <= bytes.Length)
        {
            string id = System.Text.Encoding.ASCII.GetString(bytes, pos, 4);
            int size = System.BitConverter.ToInt32(bytes, pos + 4);
            if (id == "data")
            {
                float peak = 0f;
                for (int i = pos + 8; i + 1 < pos + 8 + size && i + 1 < bytes.Length; i += 2)
                {
                    float a = Mathf.Abs(System.BitConverter.ToInt16(bytes, i) / 32768f);
                    if (a > peak) peak = a;
                }

                return peak;
            }

            pos += 8 + size;
        }

        Assert.Fail("no data chunk in " + path);
        return 0f;
    }

    [Test]
    public void EveryClip_Exists_HasSpecLengthAndChannels()
    {
        foreach (object[] c in Clips)
        {
            AudioClip clip = Load((string)c[0]);

            Assert.GreaterOrEqual(clip.length, (float)c[1], (string)c[0] + " is too short");
            Assert.LessOrEqual(clip.length, (float)c[2], (string)c[0] + " is too long");
            Assert.AreEqual((int)c[3], clip.channels, (string)c[0] + " channel count");
        }
    }

    [Test]
    public void EveryClip_PeakStaysUnderTheSpecCeiling()
    {
        foreach (object[] c in Clips)
        {
            string name = (string)c[0];

            Assert.LessOrEqual(PeakOf(Load(name)), PeakLimit, name + " peaks above the ceiling");
        }
    }

    [Test]
    public void EveryClip_IsNotSilent()
    {
        foreach (object[] c in Clips)
        {
            Assert.Greater(PeakOf(Load((string)c[0])), 0.05f, (string)c[0] + " is (almost) silent");
        }
    }

    [Test]
    public void LoudnessLadder_UiQuietest_JinglesLoudest()
    {
        float ui = PeakOf(Load("Lantern_UIClick"));
        float tap = PeakOf(Load("Lantern_DragonTap"));
        float bump = PeakOf(Load("Lantern_DragonBump"));
        float lose = PeakOf(Load("Lantern_Lose"));
        float win = PeakOf(Load("Lantern_Win"));

        Assert.Less(ui, tap);
        Assert.Less(tap, bump);
        Assert.Less(bump, lose);
        Assert.Less(lose, win);
    }

    [Test]
    public void Jingles_AreCompressedInMemory_AndOneShotsDecompressOnLoad()
    {
        foreach (object[] c in Clips)
        {
            string name = (string)c[0];
            var importer = (AudioImporter)AssetImporter.GetAtPath(Folder + name + ".wav");
            Assert.IsNotNull(importer, name);

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if ((int)c[3] == 2)
            {
                Assert.AreEqual(AudioClipLoadType.CompressedInMemory, settings.loadType, name);
                Assert.AreEqual(AudioCompressionFormat.Vorbis, settings.compressionFormat, name);
            }
            else
            {
                Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, settings.loadType, name);
                Assert.AreEqual(AudioCompressionFormat.ADPCM, settings.compressionFormat, name);
                Assert.IsTrue(importer.forceToMono, name);
            }
        }
    }

    // The scene's SoundDirector must point at the real clips (checked in the YAML so the open scene is
    // never touched or dirtied by a test).
    [TestCase("dragonTapClip", "Lantern_DragonTap")]
    [TestCase("dragonBumpClip", "Lantern_DragonBump")]
    [TestCase("uiClickClip", "Lantern_UIClick")]
    [TestCase("levelCompleteClip", "Lantern_Win")]
    [TestCase("levelFailedClip", "Lantern_Lose")]
    [TestCase("lockShakeClip", "Lantern_LockShake")]
    [TestCase("unlockClip", "Lantern_Unlock")]
    [TestCase("dragonExitClip", "Lantern_DragonExit")]
    [TestCase("heartLostClip", "Lantern_HeartLost")]
    [TestCase("hintPulseClip", "Lantern_HintPulse")]
    [TestCase("levelEntranceClip", "Lantern_LevelEntrance")]
    [TestCase("mapNodeTapClip", "Lantern_MapNodeTap")]
    [TestCase("sheetOpenClip", "Lantern_SheetOpen")]
    [TestCase("sheetCloseClip", "Lantern_SheetClose")]
    [TestCase("toggleOnClip", "Lantern_ToggleOn")]
    [TestCase("toggleOffClip", "Lantern_ToggleOff")]
    public void SoundDirectorField_PointsAtTheExpectedClip(string field, string expectedName)
    {
        string yaml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ScenePath));
        Match m = Regex.Match(yaml, "^\\s*" + field + ": \\{fileID: \\d+, guid: ([0-9a-f]{32})", RegexOptions.Multiline);

        Assert.IsTrue(m.Success, field + " is not assigned in the scene");
        string path = AssetDatabase.GUIDToAssetPath(m.Groups[1].Value);
        Assert.AreEqual(Folder + expectedName + ".wav", path, field);
    }

    [TestCase("dragonTapAlt", 2)]
    [TestCase("dragonBumpAlt", 1)]
    [TestCase("lockShakeAlt", 1)]
    public void SoundDirectorAlternates_AreAssigned(string field, int expectedCount)
    {
        string yaml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ScenePath));
        Match m = Regex.Match(yaml, "^\\s*" + field + ":\\r?\\n((?:\\s*- \\{fileID: \\d+, guid: [0-9a-f]{32}, type: 3\\}\\r?\\n)+)", RegexOptions.Multiline);

        Assert.IsTrue(m.Success, field + " list is not assigned in the scene");
        Assert.AreEqual(expectedCount, Regex.Matches(m.Groups[1].Value, "guid: ").Count, field);
    }
}
