using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Rebuilds levels 3-7 as five-tier glass enclosures in the style of levels 1-2.
/// Each scene starts as a copy of Augmentation Lab, keeping its enclosure, rats, camera, UI,
/// lighting and tokens. The route sections are then replaced. Hazards, stations, checkpoints
/// and exits are cloned from the template, so they keep its shaders, materials and fire
/// particles. Each level introduces one new augment safely, then combines it with older ones:
///   3 Relay Archive    - Dash          4 Coolant Foundry  - Wall jump
///   5 Scanner Gallery  - Glide/updraft 6 Containment Core - Phase
///   7 Escape Spire     - Ground pound
/// The target scene files keep their own .meta (scene GUID). Re-running overwrites levels 3-7.
/// </summary>
public static class EnclosureLevelBuilder
{
    private const string TemplatePath = "Assets/Scenes/AugmentationLab.unity";
    // Deck tops of the five tiers, shared with the reference enclosures (5.2m apart).
    private const float T1 = 1f, T2 = 6.2f, T3 = 11.4f, T4 = 16.6f, T5 = 21.8f;
    private const float Frame = 27.12f; // Underside of the enclosure's upper frame.

    private static Transform root, section;
    private static List<Transform> oldSections, coins;
    private static int levelIndex, checkpointNumber, coinNumber;
    private static Material steel, cyan, navy, pale, gold, recess, water, phaseField, updraftField, cyanGlow;

    [MenuItem("Project RAT/Rebuild Enclosure Levels 3-7")]
    public static void BuildAll()
    {
        FixWheelTraps(); // The template's wheel is cloned into every level, so fix it first.
        PrepareMaterials();
        for (int i = 2; i < CampaignCatalog.Count; i++) Build(i);
        FireParticleAuthoring.Build();
        AssetDatabase.SaveAssets();
        Debug.Log("RAT_ENCLOSURES_BUILT");
    }

    private static readonly Action[] Layouts = { RelayArchive, CoolantFoundry, ScannerGallery, ContainmentCore, EscapeSpire };

    private static void Build(int index)
    {
        string path = CampaignCatalog.Scenes[index];
        // Save the template over the target through Unity, not a raw file copy: the asset worker can
        // briefly hold freshly written scenes open. The target's .meta (scene GUID) is kept.
        Scene template = EditorSceneManager.OpenScene(TemplatePath, OpenSceneMode.Single);
        if (!EditorSceneManager.SaveScene(template, path, true)) throw new Exception("Could not write " + path);
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        levelIndex = index; checkpointNumber = 0; coinNumber = 0;

        root = All<Transform>().First(t => t.name == "01 - Augmentation Lab");
        oldSections = root.Cast<Transform>().Where(t => t.name.StartsWith("0") && !t.name.StartsWith("00") && !t.name.StartsWith("06")).ToList();
        root.name = $"{index + 1:00} - {CampaignCatalog.Names[index]}";
        coins = All<Transform>().First(t => t.name == "Campaign tokens").Cast<Transform>()
            .OrderBy(t => int.Parse(t.name.Substring(6))).ToList();
        SetText(All<TextMeshPro>().First(t => t.text.StartsWith("R.A.T.")).gameObject, $"R.A.T.  /  ENCLOSURE {index + 1:00}");
        CampaignAuthoring.Set(Object.FindFirstObjectByType<CampaignSession>(), "levelIndex", index);
        AddAugmentVisuals();

        Section("00 - Release");
        Clone(Tpl("Release hatch", 2, 2.3f), 0, 0); Clone(Tpl("Hatch door", 2, 2.3f), 0, 0);
        Clone(Tpl("Hatch seam", 2, 2.3f), 0, 0); Clone(Tpl("Hatch status", 2, 3.7f), 0, 0); Clone(Tpl("RELEASE", 2, 3.1f), 0, 0);
        SetText(Clone(Tpl("01  >  AUGMENTATION TRIAL", 8, 4.5f), 0, 0), $"{index + 1:00}  >  {CampaignCatalog.Names[index].ToUpperInvariant()}");
        Section("99 - Extraction");
        foreach (string part in new[] { "Extraction hatch", "Hatch door", "Hatch seam", "Hatch status", "EXIT", "Exit trigger" })
            Clone(Tpl(part, 40, 23.2f), 0, 0);

        Layouts[index - 2]();

        foreach (Transform old in oldSections) Object.DestroyImmediate(old.gameObject);
        if (coinNumber != coins.Count) throw new Exception($"{path}: placed {coinNumber} of {coins.Count} tokens.");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"RAT_ENCLOSURE_BUILT {path} checkpoints={checkpointNumber}");
    }

    // ---------------------------------------------------------------- Level 3: Dash
    private static void RelayArchive()
    {
        Section("01 - Floor 1 / learn to dash");
        Deck("Release deck", 0.7f, 9f, T1);
        Orb(RatAugment.Dash, 6.5f, T1);
        Label("JUMP, THEN Q IN MID-AIR  >", 16f, 5.3f);
        Deck("Practice trench floor", 9f, 16f, 0.3f);
        Riser(16f, 22f, -0.25f, 2.6f); Deck("Raised landing", 16f, 22f, 2.6f);
        Checkpoint(17.2f, 2.6f);
        Coolant(22f, 31f, T1, false);
        Label("DASH OVER THE COOLANT  >", 26.5f, 5.0f);
        Deck("Coolant landing", 31f, 39.6f, T1);
        Checkpoint(32.5f, T1);
        Orb(RatAugment.DoubleJump, 36f, T1);
        Riser(39.6f, 43.3f, -0.25f, 4.3f); Deck("Double jump perch", 39.6f, 43.3f, 4.3f);

        Section("02 - Floor 2 / shield and dash");
        Deck("Laser gallery", 32.5f, 39.4f, T2);
        Checkpoint(38.4f, T2);
        Orb(RatAugment.Shield, 37f, T2);
        Laser(35f, T2, 3.6f);
        Orb(RatAugment.Dash, 33.5f, T2);
        Label("<  SHIELD, THEN DASH THE GAP", 28.5f, 10f);
        Deck("Vent gallery", 0.7f, 24.5f, T2);
        Checkpoint(23f, T2);
        Vent(17f, T2, 1.5f, 2.4f, 0f); Vent(12f, T2, 1.5f, 2.4f, 1.9f); // Clear of the dash landing (~x 21).
        Label("<  WAIT FOR THE VENTS", 16f, 9.9f);
        Orb(RatAugment.DoubleJump, 8f, T2);
        Riser(0.7f, 4f, T2, 9.5f); Deck("Left double jump perch", 0.7f, 4f, 9.5f);
        Label("DOUBLE JUMP  ^", 3f, 13f);

        Section("03 - Floor 3 / speed dash");
        Deck("Speed start", 5f, 10.5f, T3);
        Checkpoint(5.8f, T3);
        Orb(RatAugment.SpeedBoost, 7.3f, T3); Orb(RatAugment.Dash, 9.2f, T3);
        Label("SPEED + DASH  >", 17.5f, 15f);
        Deck("Relay deck", 24.5f, 39.5f, T3);
        Checkpoint(26f, T3);
        ElectricGate(34f, T3, 1.5f, 2.4f, 0f); // Clear of the speed-dash landing (~x 31).
        Elevator(41.3f, T3);

        Section("04 - Floor 4 / dash the wheel");
        Deck("Wheel deck", 31f, 39.4f, T4);
        Checkpoint(39f, T4);
        Orb(RatAugment.Dash, 37.7f, T4);
        Wheel(35.5f, T4, 60f);
        Label("<  DASH PAST THE WHEEL, THEN THE GAP", 28f, 19.4f);
        Deck("Jet deck", 6f, 22.5f, T4);
        Checkpoint(21f, T4);
        Vent(15f, T4, 1.2f, 1.8f, 0f); Vent(10.5f, T4, 1.2f, 1.8f, 1f); // Clear of the dash landing (~x 19).
        Orb(RatAugment.Jetpack, 8f, T4);
        Label("JETPACK  ^", 3.4f, 19.8f);

        Section("05 - Floor 5 / extraction");
        Deck("Extraction approach", 6.5f, 28f, T5);
        Checkpoint(8.5f, T5);
        Orb(RatAugment.Shield, 12f, T5);
        Laser(16f, T5, 3.6f, 1.5f, 2.4f, 0f);
        ElectricGate(22f, T5, 1.2f, 2f, 1f);
        Orb(RatAugment.Dash, 25.5f, T5);
        Coolant(28f, 36f, T5, true);
        Deck("Extraction deck", 36f, 43.3f, T5);

        Coins(4f, 1.7f, 12.5f, 1f, 19f, 3.3f, 26.5f, 4.6f, 34f, 1.7f,
              36f, 6.9f, 28.5f, 8.4f, 21f, 6.9f, 14.5f, 6.9f, 9f, 6.9f, 2.4f, 10.2f,
              8.2f, 12.1f, 17.5f, 14f, 28.5f, 12.1f, 36f, 12.1f,
              33f, 17.3f, 27f, 18.8f, 12.8f, 17.3f,
              19f, 22.5f, 32f, 24.3f);
    }

    // ---------------------------------------------------------------- Level 4: Wall jump
    private static void CoolantFoundry()
    {
        Section("01 - Floor 1 / pools, shuttle and first chimney");
        Deck("Release deck", 0.7f, 8f, T1);
        Orb(RatAugment.Dash, 5.5f, T1);
        Coolant(8f, 14f, T1, false);
        Deck("Pump deck", 14f, 24f, T1);
        Checkpoint(15.2f, T1);
        Orb(RatAugment.SlowTime, 22.5f, T1);
        Coolant(24f, 36f, T1, false);
        Shuttle(25.9f, T1, 8f, 7f);
        Label("RIDE ACROSS  >", 29f, 3f);
        Deck("Chimney floor", 36f, 42.6f, T1);
        Checkpoint(37f, T1);
        Orb(RatAugment.WallJump, 38.5f, T1);
        Chimney(39.6f, 42.6f, T1, T2, true);
        Label("HOLD TOWARDS A WALL, SPACE TO KICK  ^", 36.5f, 3.7f);

        Section("02 - Floor 2 / foundry pit");
        Deck("Pit edge", 32f, 40.2f, T2);
        Checkpoint(38.6f, T2);
        Orb(RatAugment.Dash, 36.5f, T2);
        Coolant(24f, 32f, T2, true);
        Label("<  DASH OVER THE PIT", 28f, 8.9f);
        Deck("Casting floor", 0.7f, 24f, T2);
        Checkpoint(22.5f, T2);
        ElectricGate(16f, T2, 1.4f, 2.2f, 0f); // Clear of the dash landing (~x 20).
        Vent(12.5f, T2, 1.2f, 1.6f, 0f); Vent(8.6f, T2, 1.2f, 1.6f, 0.9f);
        Orb(RatAugment.WallJump, 6f, T2);
        Chimney(0.7f, 3.2f, T2, T3, false);

        Section("03 - Floor 3 / coolant shuttle");
        Deck("Shuttle dock", 3.8f, 15f, T3);
        Checkpoint(5.5f, T3);
        Orb(RatAugment.SlowTime, 8f, T3);
        Coolant(15f, 31f, T3, true);
        Shuttle(16.8f, T3, 12f, 8f);
        Label("SLOW TIME HELPS  >", 23f, 15.4f);
        Deck("Return dock", 31f, 42.6f, T3);
        Checkpoint(32f, T3);
        ElectricGate(35f, T3, 1.2f, 1.8f, 0f);
        Orb(RatAugment.WallJump, 37.8f, T3);
        Chimney(39.6f, 42.6f, T3, T4, true);

        Section("04 - Floor 4 / wheel and gap");
        Deck("Wheel deck", 30f, 40.2f, T4);
        Checkpoint(38.8f, T4);
        Orb(RatAugment.SlowTime, 37.5f, T4);
        Wheel(35f, T4, 70f);
        Orb(RatAugment.Dash, 32f, T4);
        Label("<  DASH", 26f, 19.4f);
        Deck("Furnace deck", 0.7f, 22f, T4);
        Checkpoint(20.5f, T4);
        Vent(13.5f, T4, 1.2f, 1.6f, 0f); Vent(9.5f, T4, 1.2f, 1.6f, 0.8f); // Clear of the dash landing (~x 18).
        Orb(RatAugment.WallJump, 6.5f, T4);
        Chimney(0.7f, 3.2f, T4, T5, false);

        Section("05 - Floor 5 / extraction");
        Deck("Extraction approach", 3.8f, 20f, T5);
        Checkpoint(5.5f, T5);
        Orb(RatAugment.Dash, 8.5f, T5); Orb(RatAugment.Shield, 11f, T5);
        Laser(14f, T5, 3.6f, 1.3f, 2f, 0f);
        ElectricGate(17.5f, T5, 1.2f, 1.8f, 0.5f);
        Coolant(20f, 27f, T5, true);
        Deck("Extraction deck", 27f, 43.3f, T5);
        Vent(34.5f, T5, 1.2f, 1.6f, 0f); // Clear of the dash landing (~x 32).

        Coins(4f, 1.7f, 11f, 3.2f, 19f, 1.7f, 30f, 3.4f, 41.4f, 4.5f,
              36f, 6.9f, 28f, 8.6f, 20f, 6.9f, 14.2f, 6.9f, 2f, 9.6f,
              9.5f, 12.1f, 23f, 13.8f, 35.5f, 12.1f, 41.4f, 14.5f,
              31f, 17.3f, 26f, 18.8f, 11.5f, 17.3f,
              24f, 23.8f, 38f, 22.5f, 2f, 20f);
    }

    // ---------------------------------------------------------------- Level 5: Glide and updrafts
    private static void ScannerGallery()
    {
        Section("01 - Floor 1 / learn to glide");
        Deck("Release deck", 0.7f, 8f, T1);
        Orb(RatAugment.Glide, 5.5f, T1);
        Riser(8f, 9.5f, -0.25f, 2f); Deck("Launch step", 8f, 9.5f, 2f);
        Riser(9.5f, 11.5f, -0.25f, 3f); Deck("Launch tower", 9.5f, 11.5f, 3f);
        Label("JUMP, THEN HOLD SPACE  >", 15f, 5f);
        // 8.5m gap: a walking glide clears about 12m; a sprint jump without glide only about 7.3m.
        Deck("Practice trench floor", 11.5f, 20f, 0.3f);
        Riser(11.5f, 13f, -0.25f, 1.5f); Deck("Trench step", 11.5f, 13f, 1.5f);
        Riser(20f, 31f, -0.25f, 2.6f); Deck("Glide landing", 20f, 31f, 2.6f);
        Checkpoint(21.5f, 2.6f);
        Orb(RatAugment.Glide, 29.5f, 2.6f);
        Deck("Updraft floor", 31f, 43.3f, T1);
        Updraft(31.6f, 34.4f, T1, 8.2f);
        Label("HOLD SPACE IN THE UPDRAFT  ^", 37.5f, 3.8f);

        Section("02 - Floor 2 / scanner corridor");
        Deck("Scanner corridor", 0.7f, 31.2f, T2);
        Checkpoint(30f, T2);
        Orb(RatAugment.Shield, 28.5f, T2);
        Laser(25.5f, T2, 3.6f, 1.2f, 1.8f, 0f); Laser(22f, T2, 3.6f, 1.2f, 1.8f, 0.8f);
        ElectricGate(18.5f, T2, 1.4f, 2f, 0.3f);
        Orb(RatAugment.GravityPulse, 15f, T2);
        MagneticGate(12f, T2);
        Checkpoint(10f, T2);
        Orb(RatAugment.Glide, 7.5f, T2);
        Updraft(1.2f, 3.8f, T2, 13.4f);

        Section("03 - Floor 3 / glide the gallery");
        Deck("Gallery launch", 4.3f, 12f, T3);
        Checkpoint(6f, T3);
        Orb(RatAugment.Glide, 8f, T3); Orb(RatAugment.Dash, 10.5f, T3);
        Label("GLIDE  >  HOVER IN THE UPDRAFT  >  GLIDE ON", 21f, 14.7f);
        // 4m wide so even a rat gliding straight through at walking speed reaches the top.
        // The top stays low enough that a rising rat never touches floor 4's coolant pit overhead.
        Updraft(17f, 21f, 9f, 14.3f);
        Deck("Gallery landing", 25.5f, 42.6f, T3);
        Checkpoint(31.5f, T3);
        Orb(RatAugment.WallJump, 34f, T3);
        Chimney(39.6f, 42.6f, T3, T4, true);

        Section("04 - Floor 4 / updraft over coolant");
        Deck("Wheel deck", 28f, 40.2f, T4);
        Checkpoint(38.6f, T4);
        Wheel(34f, T4, 75f);
        Orb(RatAugment.Glide, 31f, T4); Orb(RatAugment.Dash, 29.5f, T4);
        Coolant(13f, 28f, T4, true);
        Updraft(18f, 22f, T4 - 1f, 19.6f); // 4m wide: see floor 3.
        Label("<  CATCH THE UPDRAFT", 20f, 21f);
        Deck("Far deck", 0.7f, 13f, T4);
        Checkpoint(10.5f, T4);
        Orb(RatAugment.Glide, 7.5f, T4);
        Updraft(1.2f, 3.8f, T4, 23.8f);

        Section("05 - Floor 5 / scanner gallery");
        Deck("Scanner gallery", 4.3f, 43.3f, T5);
        Orb(RatAugment.Shield, 7.5f, T5);
        Laser(11f, T5, 3.6f, 1f, 1.6f, 0f); Laser(14.5f, T5, 3.6f, 1f, 1.6f, 0.5f); Laser(18f, T5, 3.6f, 1f, 1.6f, 1f);
        Checkpoint(22f, T5);
        Orb(RatAugment.GravityPulse, 24f, T5);
        MagneticGate(27f, T5);
        ElectricGate(32f, T5, 1f, 1.4f, 0f);

        Coins(4f, 1.7f, 10.5f, 3.7f, 15.5f, 4.8f, 16f, 1f, 27.5f, 3.3f, 37f, 1.7f,
              24f, 6.9f, 16.5f, 6.9f, 9f, 6.9f, 2.5f, 10f,
              7f, 12.1f, 19f, 14.3f, 23.5f, 13.6f, 36f, 12.1f,
              36f, 17.3f, 20f, 19.2f, 5f, 17.3f,
              9.5f, 22.5f, 35.5f, 22.5f, 41.4f, 14.5f);
    }

    // ---------------------------------------------------------------- Level 6: Phase
    private static void ContainmentCore()
    {
        Section("01 - Floor 1 / containment cell");
        Deck("Release deck", 0.7f, 23.5f, T1);
        Orb(RatAugment.Phase, 6f, T1);
        PhaseField(10f, T1, T2 - 0.48f);
        Label("WALK THROUGH THE VIOLET FIELD  >", 17f, 5.3f);
        Checkpoint(12f, T1);
        Orb(RatAugment.Phase, 13.5f, T1);
        Vent(16.5f, T1, 2.6f, 0.6f, 0f);
        Laser(19f, T1, 3.6f);
        Vent(21f, T1, 2.6f, 0.6f, 1.3f);
        PhaseField(23f, T1, T2 - 0.48f);
        Coolant(23.5f, 29.5f, T1, false);
        Deck("Chimney floor", 29.5f, 42.6f, T1);
        Checkpoint(31f, T1);
        Orb(RatAugment.Phase, 35f, T1);
        Orb(RatAugment.WallJump, 41.4f, T1); // Inside the chimney, so a rat past the field can always climb.
        Chimney(39.6f, 42.6f, T1, T2, true);
        PhaseField(39.9f, T1, 3.2f);

        Section("02 - Floor 2 / field chain");
        Deck("Field gallery", 26f, 40.2f, T2);
        Checkpoint(38.6f, T2);
        Orb(RatAugment.Phase, 37f, T2);
        PhaseField(35f, T2, T3 - 0.48f);
        ElectricGate(33f, T2, 1f, 1.2f, 0f);
        Wheel(30f, T2, 90f);
        PhaseField(27.5f, T2, T3 - 0.48f);
        Coolant(20f, 26f, T2, true);
        Deck("Core gallery", 6f, 20f, T2);
        Checkpoint(18.5f, T2);
        Orb(RatAugment.SlowTime, 16.5f, T2);
        Wheel(13f, T2, 85f);
        Vent(9.5f, T2, 1f, 1.4f, 0f);
        Orb(RatAugment.Jetpack, 7f, T2);
        Label("JETPACK  ^", 3.4f, 9.8f);

        Section("03 - Floor 3 / sealed pit");
        Deck("Cell approach", 6.5f, 16f, T3);
        Checkpoint(8f, T3);
        Orb(RatAugment.Glide, 10f, T3); Orb(RatAugment.Phase, 12f, T3);
        PhaseField(14f, T3, T4 - 0.48f);
        Coolant(16f, 22f, T3, true);
        Label("PHASE STILL DROWNS: JUMP  >", 19f, 15.5f);
        Deck("Generator deck", 22f, 43.3f, T3);
        PhaseField(23.5f, T3, T4 - 0.48f);
        Checkpoint(25.5f, T3);
        Orb(RatAugment.Dash, 27.5f, T3);
        ElectricGate(30.5f, T3, 1f, 1.4f, 0f); ElectricGate(34f, T3, 1f, 1.4f, 0.6f);
        Orb(RatAugment.Glide, 37.5f, T3);
        Updraft(40f, 43f, T3, 18.4f);

        Section("04 - Floor 4 / gauntlet");
        Deck("Gauntlet deck", 31f, 39.6f, T4);
        Checkpoint(39.2f, T4);
        Orb(RatAugment.Phase, 38.3f, T4); Orb(RatAugment.Dash, 36.8f, T4);
        Wheel(34.5f, T4, 90f);
        Label("<  PHASE, THEN DASH", 26f, 19.4f);
        Deck("Core deck", 0.7f, 23f, T4);
        Checkpoint(20.5f, T4);
        Orb(RatAugment.Phase, 17.8f, T4); // The gauntlet starts clear of the dash landing (~x 19.5).
        Laser(15.8f, T4, 3.6f);
        Vent(13.3f, T4, 2.6f, 0.6f, 0f);
        Laser(10.8f, T4, 3.6f);
        PhaseField(8.8f, T4, T5 - 0.48f);
        Orb(RatAugment.WallJump, 6.3f, T4);
        Chimney(0.7f, 3.2f, T4, T5, false);

        Section("05 - Floor 5 / core breach");
        Deck("Breach deck", 3.8f, 28f, T5);
        Checkpoint(5.5f, T5);
        Orb(RatAugment.Phase, 7.5f, T5);
        PhaseField(9.5f, T5, Frame);
        Laser(11.5f, T5, 3.6f);
        Vent(13.5f, T5, 2.6f, 0.6f, 0f);
        Wheel(16f, T5, 100f);
        Laser(18.5f, T5, 3.6f);
        PhaseField(20.5f, T5, Frame);
        Orb(RatAugment.Shield, 22.5f, T5);
        Laser(24.5f, T5, 3.6f, 1f, 1.4f, 0f);
        Orb(RatAugment.Dash, 26.5f, T5);
        Coolant(28f, 35f, T5, true);
        Deck("Extraction deck", 35f, 43.3f, T5);

        Coins(4f, 1.7f, 11f, 1.7f, 17.8f, 1.7f, 26.5f, 3.2f, 33f, 1.7f, 41.4f, 4.5f,
              36f, 6.9f, 23f, 8.6f, 15f, 6.9f, 3.4f, 9.5f,
              9f, 12.1f, 19f, 13.6f, 29f, 12.1f, 41.5f, 15f,
              32.2f, 17.3f, 27f, 18.8f, 22f, 17.3f, 2f, 20f,
              31.5f, 24f, 38f, 22.5f);
    }

    // ---------------------------------------------------------------- Level 7: Ground pound
    private static void EscapeSpire()
    {
        Section("01 - Floor 1 / learn to pound");
        Deck("Release deck", 0.7f, 6f, T1);
        Orb(RatAugment.GroundPound, 4.8f, T1);
        Riser(6f, 7.5f, -0.25f, 2.2f); Deck("Service step", 6f, 7.5f, 2.2f);
        Riser(7.5f, 13f, -0.25f, 3.4f); Deck("Service walkway", 7.5f, 13f, 3.4f);
        Hatch(13f, 15f, 3.4f);
        Wall("Service bulkhead", 15f, 16f, 2f, T2 - 0.48f, false);
        Label("JUMP, THEN S TO SMASH THE HATCH", 16f, 5.3f);
        Deck("Service trench floor", 13f, 21f, 0.3f);
        Deck("Spire floor", 21f, 39.6f, T1);
        Checkpoint(22.5f, T1);
        Orb(RatAugment.SlowTime, 25f, T1);
        Wheel(34.5f, T1, 85f);
        Orb(RatAugment.DoubleJump, 37.3f, T1);
        Riser(39.6f, 43.3f, -0.25f, 4.3f); Deck("Double jump perch", 39.6f, 43.3f, 4.3f);

        Section("02 - Floor 2 / service channel");
        Deck("Upper landing", 32f, 39.4f, T2);
        Checkpoint(38.2f, T2);
        Orb(RatAugment.GroundPound, 36.8f, T2);
        ElectricGate(34.5f, T2, 1f, 1.3f, 0f);
        Hatch(30f, 32f, T2);
        Wall("Channel bulkhead", 28.5f, 29.5f, 4.9f, T3 - 0.48f, false);
        Deck("Service channel", 20f, 32f, 3.7f);
        Riser(21f, 22.5f, 3.7f, 4.7f); Deck("Channel step", 21f, 22.5f, 4.7f);
        Label("<  POUND, CRAWL UNDER, CLIMB OUT", 26f, 9.4f);
        Deck("Vent gallery", 0.7f, 20f, T2);
        Checkpoint(18.5f, T2);
        Orb(RatAugment.Shield, 17f, T2);
        Vent(14.5f, T2, 1f, 1.3f, 0f);
        Laser(12f, T2, 3.6f);
        Vent(9.5f, T2, 1f, 1.3f, 0.6f);
        Orb(RatAugment.WallJump, 6f, T2);
        Chimney(0.7f, 3.2f, T2, T3, false);

        Section("03 - Floor 3 / glide the spire");
        Deck("Scanner floor", 3.8f, 24f, T3);
        Checkpoint(5.5f, T3);
        Vent(9.5f, T3, 1f, 1.4f, 0f); Vent(13.5f, T3, 1f, 1.4f, 0.6f);
        Orb(RatAugment.Glide, 19f, T3); Orb(RatAugment.Dash, 20.8f, T3); Orb(RatAugment.WallJump, 22.6f, T3);
        Coolant(24f, 38.5f, T3, true);
        Updraft(28.5f, 32.5f, T3 - 1f, 14.8f); // 4m wide so a walking glide reaches the top.
        Label("GLIDE  >  UPDRAFT  >  CHIMNEY", 31f, 15.4f);
        Deck("Chimney floor", 38.5f, 42.6f, T3);
        Checkpoint(39f, T3);
        Chimney(39.6f, 42.6f, T3, T4, true);

        Section("04 - Floor 4 / spire core");
        Deck("Core deck", 21f, 40.2f, T4);
        Checkpoint(38.6f, T4);
        Orb(RatAugment.GroundPound, 37f, T4);
        ElectricGate(34f, T4, 1f, 1.2f, 0f);
        Orb(RatAugment.SlowTime, 31.5f, T4);
        Wheel(28.5f, T4, 95f);
        Hatch(19f, 21f, T4);
        Wall("Core bulkhead", 17.5f, 18.5f, 15.3f, T5 - 0.48f, false);
        Deck("Core channel", 6f, 21f, 14.1f);
        Riser(7f, 8.5f, 14.1f, 15.1f); Deck("Core channel step", 7f, 8.5f, 15.1f);
        Deck("Jet ledge", 0.7f, 6f, T4);
        Orb(RatAugment.Jetpack, 4.5f, T4);
        Label("JETPACK  ^", 2.3f, 20.2f);

        Section("05 - Floor 5 / the last door");
        Deck("Escape deck", 3.8f, 27f, T5);
        Checkpoint(5.5f, T5);
        Orb(RatAugment.Phase, 6.8f, T5);
        PhaseField(8.5f, T5, Frame);
        Laser(10f, T5, 3.6f);
        Wheel(12.5f, T5, 110f);
        Vent(15f, T5, 2.6f, 0.5f, 0f);
        PhaseField(17.3f, T5, Frame);
        Orb(RatAugment.Shield, 19f, T5);
        Laser(21.5f, T5, 3.6f, 1f, 1.2f, 0f);
        Orb(RatAugment.Dash, 24.5f, T5);
        Coolant(27f, 33f, T5, true);
        Deck("Vault deck", 33f, 43.3f, T5);
        Orb(RatAugment.DoubleJump, 33.8f, T5); Orb(RatAugment.GroundPound, 36.1f, T5);
        Wall("Vault wall", 37f, 37.6f, T5, 24.52f, false);
        Deck("Vault roof", 37f, 39f, 25f); Hatch(39f, 41f, 25f); Deck("Vault roof", 41f, 43.3f, 25f);
        Label("DOUBLE JUMP UP, POUND INTO THE VAULT", 30f, 26.2f);
        // The vault roof hides the exit sign, so lift it onto the roof above the hatch.
        ((RectTransform)root.Find("99 - Extraction/EXIT")).anchoredPosition = new Vector2(40f, 26.1f);

        Coins(5.8f, 1.7f, 10f, 4.1f, 18f, 1f, 24f, 1.7f, 31f, 1.7f, 41.4f, 5f,
              36f, 6.9f, 26f, 4.4f, 15.8f, 6.9f, 2f, 9.6f,
              8f, 12.1f, 17.5f, 12.1f, 30.5f, 14.5f, 41.4f, 14.5f,
              34f, 17.3f, 14f, 14.8f, 2.5f, 19.5f,
              19f, 22.5f, 30f, 23.9f, 42f, 22.5f);
    }

    // ---------------------------------------------------------------- Building blocks
    private static void Section(string name)
    {
        section = new GameObject(name).transform;
        section.SetParent(root, false);
    }

    private static GameObject Cube(string name, Vector3 centre, Vector3 size, Material material, bool solid, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent != null ? parent : section, true);
        go.transform.position = centre;
        go.transform.localScale = parent != null ? Div(size, parent.lossyScale) : size;
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    private static Vector3 Div(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

    /// <summary>Walkable deck with the reference levels' lit front edge, trim and fasteners.</summary>
    private static GameObject Deck(string name, float left, float right, float top)
    {
        float width = right - left, x = (left + right) / 2f;
        var deck = Cube(name, new Vector3(x, top - 0.24f, 0), new Vector3(width, 0.48f, 2.8f), steel, true);
        Cube("Walkable edge", new Vector3(x, top - 0.055f, -1.42f), new Vector3(width - 0.08f, 0.075f, 0.08f), cyan, false, deck.transform);
        Cube("Lower trim", new Vector3(x, top - 0.48f, -1.4f), new Vector3(width, 0.065f, 0.08f), navy, false, deck.transform);
        for (float f = left + 0.35f; f < right - 0.2f; f += 2.4f)
            Cube("Deck fastener", new Vector3(f, top - 0.29f, -1.45f), new Vector3(0.09f, 0.09f, 0.06f), pale, false, deck.transform);
        return deck;
    }

    private static void Riser(float left, float right, float bottom, float top) =>
        Cube("Riser", new Vector3((left + right) / 2f, (bottom + top - 0.48f) / 2f, 0), new Vector3(right - left, top - 0.48f - bottom, 2.8f), navy, true);

    private static void Wall(string name, float left, float right, float bottom, float top, bool climbGuide)
    {
        var wall = Cube(name, new Vector3((left + right) / 2f, (bottom + top) / 2f, 0), new Vector3(right - left, top - bottom, 2.8f), navy, true);
        if (climbGuide)
            Cube("Climb guide", new Vector3((left + right) / 2f, (bottom + top) / 2f, -1.43f), new Vector3(0.08f, top - bottom - 0.4f, 0.06f), cyan, false, wall.transform);
    }

    /// <summary>
    /// Wall-jump chimney between tiers. The inner wall stops 2.2m above the lower deck so the rat
    /// walks underneath; the upper deck sits on the inner wall's top.
    /// </summary>
    private static void Chimney(float left, float right, float lower, float upper, bool rightSide)
    {
        if (rightSide)
        {
            Wall("Chimney inner wall", left, left + 0.6f, lower + 2.2f, upper, true);
            Wall("Chimney outer wall", right, 43.3f, lower, upper + 2.4f, true);
        }
        else Wall("Chimney inner wall", right, right + 0.6f, lower + 2.2f, upper, true);
    }

    private static void Coolant(float left, float right, float top, bool pit)
    {
        float width = right - left, x = (left + right) / 2f;
        // Floor coolant sits just under deck level; an upper-tier pit hangs 1.5m below the decks.
        float trough = pit ? top - 1.5f : top - 0.8f, body = pit ? top - 1.12f : top - 0.42f;
        float surface = pit ? top - 0.93f : top - 0.23f, deep = pit ? top - 1.04f : top - 0.34f;
        Cube("Coolant trough", new Vector3(x, trough, 0), new Vector3(width, 0.38f, 3f), navy, false);
        Cube("Coolant body", new Vector3(x, body, 0), new Vector3(width, 0.35f, 2.8f), water, false);
        Cube("Coolant surface", new Vector3(x, surface, 0), new Vector3(width, 0.06f, 2.8f), cyan, false);
        var hazard = Clone(Tpl("Deep coolant", 12.5f, 0.66f), 0, 0);
        hazard.transform.position = new Vector3(x, deep, 0);
        hazard.GetComponent<BoxCollider>().size = new Vector3(width - 0.16f, 0.34f, 2.7f);
        for (float s = left + 0.6f; s < right - 0.3f; s += 0.6f)
        {
            var stripe = Cube("Coolant warning stripe", new Vector3(s, trough - 0.01f, -1.6f), new Vector3(0.3f, 0.14f, 0.05f), gold, false);
            stripe.transform.rotation = Quaternion.Euler(0, 0, 25.4f);
        }
    }

    private static void Orb(RatAugment kind, float x, float top)
    {
        float dx = x - 18f, dy = top - T1;
        var orb = Clone(Tpl("Augmentation - SpeedBoost", 18, 1.73f), dx, dy);
        orb.name = "Augmentation - " + kind;
        orb.GetComponent<RatPickup>().kind = kind;
        Clone(Tpl("Augmentation pedestal", 18, 1.04f), dx, dy);
        SetText(Clone(Tpl("SPEED BOOST", 18, 2.47f), dx, dy), RatPowerups.Names[(int)kind]);
    }

    private static void Checkpoint(float x, float top)
    {
        int n = ++checkpointNumber;
        float dx = x - 16f, dy = top - T1;
        var spawn = Clone(Tpl("Checkpoint 1 respawn", 16, 1.08f), dx, dy);
        spawn.name = $"Checkpoint {n} respawn";
        var trigger = Clone(Tpl("Checkpoint 1", 16, 2f), dx, dy);
        trigger.name = $"Checkpoint {n}";
        var checkpoint = trigger.GetComponent<Checkpoint>();
        CampaignAuthoring.Set(checkpoint, "checkpointNumber", n);
        CampaignAuthoring.Set(checkpoint, "respawnPoint", spawn.transform);
        Clone(Tpl("Checkpoint post", 16, 1.66f), dx, dy);
        Clone(Tpl("Checkpoint lamp", 16, 2.4f), dx, dy);
        SetText(Clone(Tpl("CP 01", 16, 2.83f), dx, dy), $"CP {n:00}");
    }

    /// <summary>Thermal vent: shader plume, damage box, nozzle on the right and fire particles.</summary>
    private static void Vent(float x, float top, float on, float off, float phase)
    {
        float dx = x - 16.5f, dy = top - T2;
        var vent = Clone(Tpl("Thermal vent", 16.5f, 6.85f), dx, dy);
        Clone(Tpl("Thermal nozzle", 17.75f, 6.7f), dx, dy);
        var cycle = Clone(Tpl("Thermal vent timing", 17.75f, 7.3f), dx, dy).GetComponent<CampaignHazardCycle>();
        var lamp = Clone(Tpl("Hazard status lamp", 17.75f, 7.3f), dx, dy);
        Cycle(cycle, vent, lamp, on, off, phase);
    }

    /// <summary>Vertical laser of any height. Without timings it is always on (a shield or phase crossing).</summary>
    private static void Laser(float x, float bottom, float height, float on = 0f, float off = 0f, float phase = 0f)
    {
        float dx = x - 23f;
        var laser = Clone(Tpl("Final scan laser", 23, 23.15f), dx, 0);
        laser.name = on > 0f ? "Scan laser" : "Shield laser";
        laser.transform.position = new Vector3(x, bottom + height / 2f, 0);
        laser.GetComponent<BoxCollider>().size = new Vector3(0.28f, height, 2.6f);
        Transform beam = laser.transform.Find("Laser beam");
        beam.localScale = new Vector3(beam.localScale.x, height, beam.localScale.z);
        Clone(Tpl("Laser lower emitter", 23, 21.88f), dx, 0).transform.position = new Vector3(x, bottom + 0.08f, 0);
        Clone(Tpl("Laser upper emitter", 23, 24.6f), dx, 0).transform.position = new Vector3(x, bottom + height + 0.1f, 0);
        if (on <= 0f) return;
        var cycle = Clone(Tpl("Final scan laser cycle", 23, 24.9f), dx, 0);
        cycle.transform.position = new Vector3(x, bottom + height + 0.4f, 0);
        var lamp = Clone(Tpl("Hazard status lamp", 23, 24.9f), dx, 0);
        lamp.transform.position = cycle.transform.position;
        Cycle(cycle.GetComponent<CampaignHazardCycle>(), laser, lamp, on, off, phase);
    }

    /// <summary>Pulsed 3.2m electric gate. The bottom can float for mid-air gates.</summary>
    private static void ElectricGate(float x, float bottom, float on, float off, float phase)
    {
        float dx = x - 19.8f, dy = bottom - T4;
        var gate = Clone(Tpl("Pulsed electric gate", 19.8f, 18.2f), dx, dy);
        Clone(Tpl("Electric frame lower", 19.8f, 16.75f), dx, dy);
        Clone(Tpl("Electric frame upper", 19.8f, 19.9f), dx, dy);
        var cycle = Clone(Tpl("Pulsed electric gate timing", 19.8f, 20.25f), dx, dy).GetComponent<CampaignHazardCycle>();
        var lamp = Clone(Tpl("Hazard status lamp", 19.8f, 20.25f), dx, dy);
        Cycle(cycle, gate, lamp, on, off, phase);
    }

    private static void Cycle(CampaignHazardCycle cycle, GameObject live, GameObject lamp, float on, float off, float phase)
    {
        cycle.live = live;
        cycle.indicator = lamp.GetComponent<Renderer>();
        cycle.onSeconds = on; cycle.offSeconds = off; cycle.phase = phase;
    }

    /// <summary>
    /// The reference wheel had two crossed arms on a hub 1.3m up, so a blade always swept through
    /// rat height and it could never be passed. A passable wheel keeps one arm (two blades) and a
    /// hub 2.1m up: the blades only reach a standing rat within about 43 degrees of vertical, which
    /// leaves a clear window every half turn. Idempotent; applied to the template (level 1).
    /// </summary>
    [MenuItem("Project RAT/Make Wheel Traps Passable")]
    public static void FixWheelTraps()
    {
        Scene scene = EditorSceneManager.OpenScene(TemplatePath, OpenSceneMode.Single);
        int fixedCount = 0;
        foreach (TrialWheel wheel in All<TrialWheel>())
        {
            var arms = wheel.transform.Cast<Transform>().Where(t => t.name == "Wheel sweep arm").ToList();
            if (arms.Count < 2) continue;
            foreach (Transform crossArm in arms.Where(t => Quaternion.Angle(t.localRotation, Quaternion.identity) > 1f))
                Object.DestroyImmediate(crossArm.gameObject);
            wheel.transform.position += Vector3.up * (WheelHubHeight - 1.3f);
            fixedCount++;
        }
        if (fixedCount == 0) return;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"RAT_WHEELS_FIXED {fixedCount} in {TemplatePath}");
    }

    private const float WheelHubHeight = 2.1f;

    private static void Wheel(float x, float top, float degreesPerSecond)
    {
        var wheel = Clone(Tpl("Sweep wheel", 27.5f, T4 + WheelHubHeight), 0, 0);
        wheel.transform.position = new Vector3(x, top + WheelHubHeight, wheel.transform.position.z);
        wheel.GetComponent<TrialWheel>().degreesPerSecond = degreesPerSecond;
    }

    private static void Elevator(float x, float top)
    {
        float dx = x - 41.5f, dy = top - T3;
        Clone(Tpl("Right transfer elevator", 41.5f, 11.16f), dx, dy);
        Clone(Tpl("Elevator rail", 41.5f, 14f), dx, dy);
        foreach (Transform notch in TplAll("Rail notch").Where(t => Mathf.Abs(t.position.x - 41.5f) < 0.1f && t.position.y > 11f && t.position.y < 17f))
            Clone(notch.gameObject, dx, dy);
        Label("RIDE UP  ^", x, top + 3.3f);
    }

    /// <summary>Suspended shuttle that glides travel metres to the right and back.</summary>
    private static void Shuttle(float x, float top, float travel, float period)
    {
        var deck = Cube("Suspended shuttle", new Vector3(x, top - 0.24f, 0), new Vector3(3.4f, 0.48f, 2.8f), steel, true);
        Cube("Walkable edge", new Vector3(x, top - 0.055f, -1.42f), new Vector3(3.3f, 0.075f, 0.08f), cyan, false, deck.transform);
        foreach (float side in new[] { -1.15f, 1.15f })
        {
            Cube("Suspension cable", new Vector3(x + side, top + 2.2f, 1.1f), new Vector3(0.05f, 4.4f, 0.05f), pale, false, deck.transform);
            Cube("Suspension mount", new Vector3(x + side, top + 4.3f, 1.1f), new Vector3(0.18f, 0.3f, 0.2f), gold, false, deck.transform);
        }
        var shuttle = deck.AddComponent<TrialMovingPlatform>();
        shuttle.travel = new Vector3(travel, 0, 0);
        shuttle.period = period;
    }

    private static void MagneticGate(float x, float top)
    {
        float dx = x - 20f, dy = top - T3;
        var gate = Clone(Tpl("Magnetic interlock", 20, 12.9f), dx, dy).GetComponent<CampaignPulseGate>();
        gate.barrier = Clone(Tpl("Magnetic barrier collider", 20, 13.78f), dx, dy);
        Clone(Tpl("Magnetic head housing", 20, 16.37f), dx, dy);
        Label("E  /  PUSH THE MAGNETIC GATE", x, top + 4.1f);
    }

    /// <summary>Violet containment field: solid unless the rat is phasing.</summary>
    private static void PhaseField(float x, float bottom, float top)
    {
        float height = top - bottom;
        var go = new GameObject("Containment field");
        go.transform.SetParent(section, false);
        go.transform.position = new Vector3(x, bottom + height / 2f, 0);
        var collider = go.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.5f, height, 2.8f);
        var field = GameObject.CreatePrimitive(PrimitiveType.Quad);
        field.name = "Field";
        Object.DestroyImmediate(field.GetComponent<Collider>());
        field.transform.SetParent(go.transform, false);
        field.transform.localPosition = new Vector3(0, 0, -0.75f);
        field.transform.localScale = new Vector3(0.9f, height, 1f);
        field.GetComponent<Renderer>().sharedMaterial = phaseField;
        Cube("Field emitter", new Vector3(x, bottom + 0.12f, 0), new Vector3(1f, 0.24f, 1.5f), navy, false);
        CampaignAuthoring.Set(go.AddComponent<PhaseBarrier>(), "field", field.GetComponent<Renderer>());
    }

    /// <summary>Rising air column with a floor fan. Only a gliding rat is lifted.</summary>
    private static void Updraft(float left, float right, float bottom, float top)
    {
        float width = right - left, height = top - bottom, x = (left + right) / 2f;
        var go = new GameObject("Updraft column");
        go.transform.SetParent(section, false);
        go.transform.position = new Vector3(x, bottom + height / 2f, 0);
        var column = go.AddComponent<BoxCollider>();
        column.isTrigger = true;
        column.size = new Vector3(width, height, 2.6f);
        go.AddComponent<Updraft>();
        var air = GameObject.CreatePrimitive(PrimitiveType.Quad);
        air.name = "Rising air";
        Object.DestroyImmediate(air.GetComponent<Collider>());
        air.transform.SetParent(go.transform, false);
        air.transform.localPosition = new Vector3(0, 0, -0.6f);
        air.transform.localScale = new Vector3(width, height, 1f);
        air.GetComponent<Renderer>().sharedMaterial = updraftField;
        Cube("Updraft fan", new Vector3(x, bottom + 0.06f, 0), new Vector3(width, 0.12f, 2.4f), navy, false);
        for (float s = left + 0.3f; s < right - 0.1f; s += 0.5f)
            Cube("Fan vane", new Vector3(s, bottom + 0.14f, -1.25f), new Vector3(0.22f, 0.05f, 0.05f), cyanGlow, false);
    }

    /// <summary>Cracked floor hatch: solid until a ground-pounding rat lands on it.</summary>
    private static void Hatch(float left, float right, float top)
    {
        float width = right - left, x = (left + right) / 2f;
        var go = new GameObject("Cracked hatch");
        go.transform.SetParent(section, false);
        go.transform.position = new Vector3(x, top - 0.24f, 0);
        go.AddComponent<BoxCollider>().size = new Vector3(width, 0.48f, 2.8f);
        var intact = new GameObject("Hatch plate").transform;
        intact.SetParent(go.transform, false);
        Cube("Plate", new Vector3(x, top - 0.24f, 0), new Vector3(width - 0.06f, 0.46f, 2.78f), steel, false, intact);
        Cube("Hazard edge", new Vector3(x, top - 0.055f, -1.42f), new Vector3(width - 0.08f, 0.075f, 0.08f), gold, false, intact);
        for (int i = 0; i < 3; i++)
        {
            var crack = Cube("Crack", new Vector3(x - width * 0.3f + i * width * 0.3f, top - 0.24f, -1.43f), new Vector3(0.05f, 0.42f, 0.04f), recess, false, intact);
            crack.transform.rotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 28f : -24f);
        }
        var broken = new GameObject("Hatch shards").transform;
        broken.SetParent(go.transform, false);
        foreach (float side in new[] { -1f, 1f })
        {
            var shard = Cube("Shard", new Vector3(x + side * (width / 2f - 0.15f), top - 0.6f, 0), new Vector3(0.3f, 0.7f, 2.6f), steel, false, broken);
            shard.transform.rotation = Quaternion.Euler(0, 0, side * 18f);
        }
        broken.gameObject.SetActive(false);
        var hatch = go.AddComponent<BreakableHatch>();
        CampaignAuthoring.Set(hatch, "intact", intact.gameObject);
        CampaignAuthoring.Set(hatch, "broken", broken.gameObject);
    }

    private static void Label(string text, float x, float y)
    {
        var label = Clone(Tpl("SPACE  /  JUMP", 6, 3.5f), 0, 0);
        label.name = text;
        ((RectTransform)label.transform).anchoredPosition = new Vector2(x, y);
        SetText(label, text);
    }

    private static void Coins(params float[] xy)
    {
        for (int i = 0; i + 1 < xy.Length; i += 2)
        {
            if (coinNumber >= coins.Count) throw new Exception("Too many token positions.");
            Transform coin = coins[coinNumber];
            coin.position = new Vector3(xy[i], xy[i + 1], 0);
            CampaignAuthoring.Set(coin.GetComponent<CoinPickup>(), "stableId", CampaignCatalog.Ids[levelIndex] + ":" + coinNumber);
            coinNumber++;
        }
    }

    // ---------------------------------------------------------------- Template access
    private static T[] All<T>() where T : Object => Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    private static IEnumerable<Transform> TplAll(string name) =>
        oldSections.SelectMany(s => s.GetComponentsInChildren<Transform>(true)).Where(t => t.name == name);

    /// <summary>The template object with this name nearest (x, y) in the copied Augmentation Lab sections.</summary>
    private static GameObject Tpl(string name, float x, float y)
    {
        Transform best = TplAll(name).OrderBy(t => Vector2.Distance(t.position, new Vector2(x, y))).FirstOrDefault();
        if (best == null) throw new Exception("Template object missing: " + name);
        return best.gameObject;
    }

    private static GameObject Clone(GameObject template, float dx, float dy)
    {
        var clone = Object.Instantiate(template, section, true);
        clone.name = template.name;
        // Text labels are RectTransforms whose position lives in anchoredPosition (sections sit at the origin).
        if (clone.transform is RectTransform rect)
            rect.anchoredPosition = ((RectTransform)template.transform).anchoredPosition + new Vector2(dx, dy);
        else clone.transform.position = template.transform.position + new Vector3(dx, dy, 0);
        return clone;
    }

    private static void SetText(GameObject go, string text) => go.GetComponent<TextMeshPro>().text = text;

    private static void AddAugmentVisuals()
    {
        var player = Object.FindFirstObjectByType<PlayerRatController>();
        var powers = player.GetComponent<RatPowerups>();
        powers.dashVisual = Visual(player.transform, "Dash streak", new Vector3(0, 0.45f, 0), new Vector3(1.6f, 0.14f, 0.14f), cyanGlow);
        powers.glideVisual = Visual(player.transform, "Glide canopy", new Vector3(0, 1.3f, 0), new Vector3(1.7f, 0.1f, 0.8f), cyanGlow);
        powers.phaseVisual = Visual(player.transform, "Phase shimmer", new Vector3(0, 0.5f, 0), Vector3.one * 1.25f, phaseField);
    }

    private static Transform Visual(Transform parent, string name, Vector3 local, Vector3 scale, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = local;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        go.SetActive(false);
        return go.transform;
    }

    // ---------------------------------------------------------------- Materials
    private static void PrepareMaterials()
    {
        steel = Load("Assets/Materials/Campaign/Deck steel.mat");
        cyan = Load("Assets/Materials/Campaign/Route cyan.mat");
        navy = Load("Assets/Materials/Campaign/Structure navy.mat");
        pale = Load("Assets/Materials/Campaign/Pale trim.mat");
        gold = Load("Assets/Materials/Campaign/Augment gold.mat");
        recess = Load("Assets/Materials/Campaign/Recess.mat");
        water = Load("Assets/Materials/Water/WaterBody.mat");
        cyanGlow = Load("Assets/Materials/Environment/Campaign cyan.mat");
        // Both new fields reuse the project's AnimatedEnergyHazard shader with new colours.
        phaseField = EnergyMaterial("Assets/Materials/Hazards/PhaseField.mat",
            new Color(0.16f, 0.03f, 0.3f, 0.75f), new Color(0.8f, 0.45f, 1f, 1f), 1.6f, 9f, 0.86f);
        updraftField = EnergyMaterial("Assets/Materials/Hazards/Updraft.mat",
            new Color(0.05f, 0.22f, 0.2f, 0.2f), new Color(0.7f, 1f, 0.92f, 1f), 3.2f, 6f, 0.45f);
    }

    private static Material Load(string path) =>
        AssetDatabase.LoadAssetAtPath<Material>(path) ?? throw new Exception("Missing material " + path);

    private static Material EnergyMaterial(string path, Color baseColour, Color energy, float speed, float scale, float alpha)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("ProjectRAT/GameSystems/AnimatedEnergyHazard"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", baseColour);
        material.SetColor("_EnergyColor", energy);
        material.SetFloat("_Speed", speed);
        material.SetFloat("_Scale", scale);
        material.SetFloat("_Alpha", alpha);
        material.SetFloat("_GateMode", 0f);
        material.SetFloat("_WaveHeight", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }
}
