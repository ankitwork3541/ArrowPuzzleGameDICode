using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Story 005 (progress survives a restart): the save format and its migration. SaveSystem itself
// reads and writes under persistentDataPath, so these tests never touch it; they exercise the pure
// pieces, the JSON shape and the private Migrate step, in memory.
public class SaveMigrationTests
{
    static SaveData Migrate(SaveData loaded)
    {
        MethodInfo migrate = typeof(SaveSystem).GetMethod("Migrate", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(migrate, "SaveSystem.Migrate was renamed; update this test");
        return (SaveData)migrate.Invoke(null, new object[] { loaded });
    }

    [Test]
    public void SaveData_JsonRoundTrip_KeepsProgress()
    {
        SaveData original = new SaveData { version = SaveSystem.Version, currentLevelIndex = 41, highestLevelCompleted = 42 };

        SaveData restored = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(original));

        Assert.AreEqual(original.version, restored.version);
        Assert.AreEqual(original.currentLevelIndex, restored.currentLevelIndex);
        Assert.AreEqual(original.highestLevelCompleted, restored.highestLevelCompleted);
    }

    [Test]
    public void SaveData_NewSave_StartsAtLevelOneWithNothingBeaten()
    {
        SaveData fresh = new SaveData();

        Assert.AreEqual(0, fresh.currentLevelIndex);
        Assert.AreEqual(0, fresh.highestLevelCompleted);
    }

    [Test]
    public void Migrate_VersionOneSave_BackfillsHighestLevelFromTheCurrentIndex()
    {
        // Index 10 means level 11 is being played, so ten levels are already beaten.
        SaveData v1 = new SaveData { version = 1, currentLevelIndex = 10 };

        SaveData migrated = Migrate(v1);

        Assert.AreEqual(10, migrated.highestLevelCompleted);
        Assert.AreEqual(10, migrated.currentLevelIndex, "progress must not be wiped");
        Assert.AreEqual(SaveSystem.Version, migrated.version);
    }

    [Test]
    public void Migrate_VersionOneSaveWithNegativeIndex_NeverBackfillsBelowZero()
    {
        SaveData v1 = new SaveData { version = 1, currentLevelIndex = -3 };

        Assert.AreEqual(0, Migrate(v1).highestLevelCompleted);
    }

    [Test]
    public void Migrate_CurrentVersionSave_IsLeftUntouched()
    {
        // currentLevelIndex wraps to 0 at the end of the campaign; the milestone must not re-lock.
        SaveData current = new SaveData { version = SaveSystem.Version, currentLevelIndex = 0, highestLevelCompleted = 1500 };

        SaveData migrated = Migrate(current);

        Assert.AreEqual(1500, migrated.highestLevelCompleted);
        Assert.AreEqual(0, migrated.currentLevelIndex);
    }

    [Test]
    public void Migrate_SaveFromANewerBuild_StartsFreshInsteadOfMisreadingIt()
    {
        LogAssert.Expect(LogType.Warning, new Regex("save is version"));
        SaveData newer = new SaveData { version = SaveSystem.Version + 1, currentLevelIndex = 99, highestLevelCompleted = 99 };

        SaveData migrated = Migrate(newer);

        Assert.AreEqual(SaveSystem.Version, migrated.version);
        Assert.AreEqual(0, migrated.currentLevelIndex);
        Assert.AreEqual(0, migrated.highestLevelCompleted);
    }
}
