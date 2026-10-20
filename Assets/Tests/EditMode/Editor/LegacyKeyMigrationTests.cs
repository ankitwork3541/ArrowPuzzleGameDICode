using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

// The rename pass moved the save file and the PlayerPrefs keys to new names. A player whose data sits
// under an earlier name must keep it: the first earlier name found is copied across once, the old
// value stays where it was, and an existing new value is never overwritten.
public class LegacyKeyMigrationTests
{
    string dir;
    readonly string[] prefKeys =
    {
        "test.lanterndragons.current", "test.lanterndragons.legacyNew", "test.lanterndragons.legacyShipped"
    };

    [SetUp]
    public void SetUp()
    {
        dir = Path.Combine(Path.GetTempPath(), "lanterndragons-migration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        foreach (string k in prefKeys) PlayerPrefs.DeleteKey(k);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        foreach (string k in prefKeys) PlayerPrefs.DeleteKey(k);
    }

    void Write(string name, string body) => File.WriteAllText(Path.Combine(dir, name), body);
    string Read(string name) => File.ReadAllText(Path.Combine(dir, name));

    [Test]
    public void CopyLegacyFile_CurrentFileAlreadyExists_LeavesItUntouched()
    {
        Write(ProgressStore.FileName, "current");
        Write("arrowqube_progress.json", "older");

        string copied = ProgressStore.CopyLegacyFileIfMissing(dir);

        Assert.IsNull(copied);
        Assert.AreEqual("current", Read(ProgressStore.FileName));
    }

    [Test]
    public void CopyLegacyFile_OnlyTheMiddleGenerationNamedFileExists_IsCopiedAndTheOriginalKept()
    {
        Write("arrowqube_progress.json", "{\"highestLevelCompleted\":7}");

        string copied = ProgressStore.CopyLegacyFileIfMissing(dir);

        Assert.AreEqual("arrowqube_progress.json", copied);
        Assert.AreEqual("{\"highestLevelCompleted\":7}", Read(ProgressStore.FileName));
        Assert.IsTrue(File.Exists(Path.Combine(dir, "arrowqube_progress.json")), "copy, not move");
    }

    [Test]
    public void CopyLegacyFile_OnlyTheShippedNameExists_IsCopied()
    {
        Write("progress.json", "{\"highestLevelCompleted\":3}");

        string copied = ProgressStore.CopyLegacyFileIfMissing(dir);

        Assert.AreEqual("progress.json", copied);
        Assert.AreEqual("{\"highestLevelCompleted\":3}", Read(ProgressStore.FileName));
    }

    [Test]
    public void CopyLegacyFile_BothEarlierNamesExist_TheNewerGenerationWins()
    {
        Write("arrowqube_progress.json", "newer");
        Write("progress.json", "oldest");

        ProgressStore.CopyLegacyFileIfMissing(dir);

        Assert.AreEqual("newer", Read(ProgressStore.FileName));
    }

    [Test]
    public void CopyLegacyFile_NothingExists_DoesNothing()
    {
        Assert.IsNull(ProgressStore.CopyLegacyFileIfMissing(dir));
        Assert.IsFalse(File.Exists(Path.Combine(dir, ProgressStore.FileName)));
    }

    [Test]
    public void CopyInt_CurrentKeyExists_IsNotOverwritten()
    {
        PlayerPrefs.SetInt(prefKeys[0], 5);
        PlayerPrefs.SetInt(prefKeys[1], 9);

        string copied = KeyMigration.CopyIntIfMissing(prefKeys[0], prefKeys[1], prefKeys[2]);

        Assert.IsNull(copied);
        Assert.AreEqual(5, PlayerPrefs.GetInt(prefKeys[0]));
    }

    [Test]
    public void CopyInt_OnlyAnEarlierKeyExists_IsCopiedAndTheOldValueKept()
    {
        PlayerPrefs.SetInt(prefKeys[2], 4);

        string copied = KeyMigration.CopyIntIfMissing(prefKeys[0], prefKeys[1], prefKeys[2]);

        Assert.AreEqual(prefKeys[2], copied);
        Assert.AreEqual(4, PlayerPrefs.GetInt(prefKeys[0]));
        Assert.AreEqual(4, PlayerPrefs.GetInt(prefKeys[2]), "copy, not move");
    }

    [Test]
    public void CopyInt_TwoEarlierKeysExist_TheFirstListedWins()
    {
        PlayerPrefs.SetInt(prefKeys[1], 8);
        PlayerPrefs.SetInt(prefKeys[2], 2);

        KeyMigration.CopyIntIfMissing(prefKeys[0], prefKeys[1], prefKeys[2]);

        Assert.AreEqual(8, PlayerPrefs.GetInt(prefKeys[0]));
    }

    [Test]
    public void CopyInt_NoKeyExists_LeavesTheCurrentKeyAbsent()
    {
        Assert.IsNull(KeyMigration.CopyIntIfMissing(prefKeys[0], prefKeys[1], prefKeys[2]));
        Assert.IsFalse(PlayerPrefs.HasKey(prefKeys[0]));
    }
}
