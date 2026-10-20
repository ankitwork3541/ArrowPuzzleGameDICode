using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Story 005 (progress survives a restart): the save format and its migration. ProgressStore itself
// reads and writes under persistentDataPath, so these tests never touch it; they exercise the pure
// pieces, the JSON shape and the private Migrate step, in memory.
public class ProgressMigrationTests
{
    static ProgressData Migrate(ProgressData loaded)
    {
        MethodInfo migrate = typeof(ProgressStore).GetMethod("Migrate", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(migrate, "ProgressStore.Migrate was renamed; update this test");
        return (ProgressData)migrate.Invoke(null, new object[] { loaded });
    }

    [Test]
    public void SaveData_JsonRoundTrip_KeepsProgress()
    {
        ProgressData original = new ProgressData { version = ProgressStore.Version, currentLevelIndex = 41, highestLevelCompleted = 42 };

        ProgressData restored = JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(original));

        Assert.AreEqual(original.version, restored.version);
        Assert.AreEqual(original.currentLevelIndex, restored.currentLevelIndex);
        Assert.AreEqual(original.highestLevelCompleted, restored.highestLevelCompleted);
    }

    [Test]
    public void SaveData_NewSave_StartsAtLevelOneWithNothingBeaten()
    {
        ProgressData fresh = new ProgressData();

        Assert.AreEqual(0, fresh.currentLevelIndex);
        Assert.AreEqual(0, fresh.highestLevelCompleted);
    }

    [Test]
    public void Migrate_VersionOneSave_BackfillsHighestLevelFromTheCurrentIndex()
    {
        // Index 10 means level 11 is being played, so ten levels are already beaten.
        ProgressData v1 = new ProgressData { version = 1, currentLevelIndex = 10 };

        ProgressData migrated = Migrate(v1);

        Assert.AreEqual(10, migrated.highestLevelCompleted);
        Assert.AreEqual(10, migrated.currentLevelIndex, "progress must not be wiped");
        Assert.AreEqual(ProgressStore.Version, migrated.version);
    }

    [Test]
    public void Migrate_VersionOneSaveWithNegativeIndex_NeverBackfillsBelowZero()
    {
        ProgressData v1 = new ProgressData { version = 1, currentLevelIndex = -3 };

        Assert.AreEqual(0, Migrate(v1).highestLevelCompleted);
    }

    [Test]
    public void Migrate_CurrentVersionSave_IsLeftUntouched()
    {
        // currentLevelIndex wraps to 0 at the end of the campaign; the beaten-level count must not reset.
        ProgressData current = new ProgressData { version = ProgressStore.Version, currentLevelIndex = 0, highestLevelCompleted = 1500 };

        ProgressData migrated = Migrate(current);

        Assert.AreEqual(1500, migrated.highestLevelCompleted);
        Assert.AreEqual(0, migrated.currentLevelIndex);
    }

    [Test]
    public void Migrate_SaveFromANewerBuild_StartsFreshInsteadOfMisreadingIt()
    {
        LogAssert.Expect(LogType.Warning, new Regex("save is version"));
        ProgressData newer = new ProgressData { version = ProgressStore.Version + 1, currentLevelIndex = 99, highestLevelCompleted = 99 };

        ProgressData migrated = Migrate(newer);

        Assert.AreEqual(ProgressStore.Version, migrated.version);
        Assert.AreEqual(0, migrated.currentLevelIndex);
        Assert.AreEqual(0, migrated.highestLevelCompleted);
    }
}
