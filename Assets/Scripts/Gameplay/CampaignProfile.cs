using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Stable level identities: saves never load arbitrary scene names or build indices.</summary>
public static class CampaignCatalog
{
    public static readonly string[] Ids = { "enclosure", "transfer", "relay", "coolant", "scanner", "containment", "escape" };
    public static readonly string[] Names = { "Augmentation Lab", "Reactor Divide", "Relay Archive", "Coolant Foundry", "Scanner Gallery", "Containment Core", "Escape Spire" };
    public static readonly string[] Scenes = { "Assets/Scenes/AugmentationLab.unity", "Assets/Scenes/ReactorDivide.unity", "Assets/Scenes/RelayArchive.unity", "Assets/Scenes/CoolantFoundry.unity", "Assets/Scenes/ScannerGallery.unity", "Assets/Scenes/ContainmentCore.unity", "Assets/Scenes/EscapeSpire.unity" };
    public static readonly string[] OutfitIds = { "classic", "copper_patch", "lab_scarf", "technician" };
    public static readonly string[] OutfitNames = { "Classic", "Copper Patch", "Lab Scarf", "Technician" };
    public static readonly int[] Prices = { 0, 10, 20, 30 };
    public static int Count => Ids.Length;
}

[Serializable]
public class RatSettings
{
    public float master = .8f, music = .5f, effects = .8f;
    public bool mute, reducedMotion, fullscreen = true, vSync = true;
    public int quality = 1;
}

[Serializable]
public class RatLevelRecord
{
    public string id;
    public int attempts, wins, failures, abandoned, deaths, bestRats;
    public double seconds, bestSeconds;
}

[Serializable]
public class RatAttempt
{
    public string id, level, outcome = "active";
    public int deaths, coins, survivors;
    public List<string> pickedCoins = new List<string>();
    public double seconds;
}

[Serializable]
public class RatProfileData
{
    public int version = 1, unlocked = 1, selected, spent, campaignClears;
    public bool campaignComplete, hasCampaign;
    public int earned;
    public List<int> entryRats = new List<int> { 3, 3, 3, 3, 3, 3, 3 };
    public string equipped = "classic";
    public List<string> owned = new List<string> { "classic" };
    public List<string> coins = new List<string>();
    public List<RatLevelRecord> levels = new List<RatLevelRecord>();
    public RatSettings settings = new RatSettings();
    public RatAttempt active;
}

/// <summary>One versioned writer for progression, economy, settings and attempt accounting.</summary>
public static class CampaignProfile
{
    private static string SaveKey = "ProjectRAT.Profile.v1";
    private static RatProfileData data;
    public static string Notice { get; private set; }
    public static event Action Changed;
    public static RatProfileData Data { get { EnsureLoaded(); return data; } }
    public static int Wallet => Data.earned - Data.spent;
    public static bool Playing => Data.active != null && Data.active.outcome == "active";
    public static int Equipped => Mathf.Max(0, Array.IndexOf(CampaignCatalog.OutfitIds, Data.equipped));

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        data = null; Notice = null; Changed = null;
#if UNITY_EDITOR
        SaveKey = UnityEditor.SessionState.GetString("RAT.Validation.Profile", "ProjectRAT.Profile.v1");
#endif
    }
#if UNITY_EDITOR
    public static void UseValidationProfile(string key)
    {
        UnityEditor.SessionState.SetString("RAT.Validation.Profile", key);
        SaveKey = key; data = null; Notice = null; Changed = null;
    }
    public static void EndValidationProfile()
    {
        if (SaveKey.StartsWith("ProjectRAT.Validation.", StringComparison.Ordinal))
        {
            PlayerPrefs.DeleteKey(SaveKey); PlayerPrefs.DeleteKey(SaveKey + ".previous"); PlayerPrefs.DeleteKey(SaveKey + ".recovery"); PlayerPrefs.Save();
        }
        UnityEditor.SessionState.EraseString("RAT.Validation.Profile");
        SaveKey = "ProjectRAT.Profile.v1"; data = null; Notice = null;
    }
#endif

    private static bool Finite(double n) => !double.IsNaN(n) && !double.IsInfinity(n) && n >= 0;
    private static void EnsureLoaded()
    {
        if (data != null) return;
        string raw = "";
        try
        {
            raw = PlayerPrefs.GetString(SaveKey, "");
            data = string.IsNullOrEmpty(raw) ? new RatProfileData() : JsonUtility.FromJson<RatProfileData>(raw);
            if (data == null || data.version != 1 || data.coins == null || data.owned == null || data.levels == null || data.settings == null)
                throw new InvalidOperationException("Unsupported profile.");
            Validate();
        }
        catch (Exception)
        {
            data = new RatProfileData();
            Notice = "Saved data could not be read. A fresh profile is in use.";
            try
            {
                if (!string.IsNullOrEmpty(raw)) PlayerPrefs.SetString(SaveKey + ".recovery", raw);
                string previous = PlayerPrefs.GetString(SaveKey + ".previous", "");
                if (!string.IsNullOrEmpty(previous))
                {
                    var recovered = JsonUtility.FromJson<RatProfileData>(previous);
                    if (recovered != null && recovered.version == 1 && recovered.coins != null && recovered.owned != null && recovered.levels != null && recovered.settings != null)
                    { data = recovered; Validate(); Notice = "Saved data was recovered from the previous snapshot."; }
                }
            }
            catch (Exception) { Notice = "Saved data could not be recovered. A fresh profile is in use."; data = new RatProfileData(); }
        }
        foreach (string id in CampaignCatalog.Ids) Record(id);
        if (Playing) Finish("abandoned", 0);
    }

    private static void Validate()
    {
        data.unlocked = Mathf.Clamp(data.unlocked, 1, CampaignCatalog.Count);
        data.selected = Mathf.Clamp(data.selected, 0, data.unlocked - 1);
        data.campaignClears = Mathf.Max(0, data.campaignClears);
        data.coins = new List<string>(new HashSet<string>(data.coins));
        data.coins.RemoveAll(id => !ValidCoin(id));
        data.owned = new List<string>(new HashSet<string>(data.owned));
        data.owned.RemoveAll(id => Array.IndexOf(CampaignCatalog.OutfitIds, id) < 0);
        if (!data.owned.Contains("classic")) data.owned.Add("classic");
        if (!data.owned.Contains(data.equipped)) data.equipped = "classic";
        // Existing profiles earned one coin per unique ID. Preserve their exact balance.
        data.earned = Mathf.Max(data.coins.Count, data.earned);
        data.spent = Mathf.Clamp(data.spent, 0, data.earned);
        if (data.entryRats == null) data.entryRats = new List<int>();
        while (data.entryRats.Count < CampaignCatalog.Count) data.entryRats.Add(3);
        for (int i = 0; i < data.entryRats.Count; i++) data.entryRats[i] = Mathf.Clamp(data.entryRats[i], 1, 3);
        var seen = new HashSet<string>();
        data.levels.RemoveAll(r => r == null || Array.IndexOf(CampaignCatalog.Ids, r.id) < 0 || !seen.Add(r.id));
        foreach (var r in data.levels)
        {
            r.wins = Mathf.Max(0, r.wins); r.failures = Mathf.Max(0, r.failures); r.abandoned = Mathf.Max(0, r.abandoned);
            r.deaths = Mathf.Max(0, r.deaths); r.bestRats = Mathf.Clamp(r.bestRats, 0, 3);
            r.seconds = Finite(r.seconds) ? r.seconds : 0; r.bestSeconds = Finite(r.bestSeconds) ? r.bestSeconds : 0;
        }
        if (data.active != null && (Array.IndexOf(CampaignCatalog.Ids, data.active.level) < 0 || string.IsNullOrEmpty(data.active.id) || data.active.outcome != "active")) data.active = null;
        if (data.active != null)
        {
            if(data.active.pickedCoins == null) data.active.pickedCoins = new List<string>();
            data.active.seconds = Finite(data.active.seconds) ? data.active.seconds : 0;
            data.active.deaths = Mathf.Clamp(data.active.deaths, 0, 3); data.active.coins = Mathf.Clamp(data.active.coins, 0, 20);
        }
        foreach (var r in data.levels) r.attempts = r.wins + r.failures + r.abandoned + (data.active != null && data.active.level == r.id ? 1 : 0);
        RatSettings s = data.settings;
        s.master = float.IsNaN(s.master) ? .8f : Mathf.Clamp01(s.master);
        s.music = float.IsNaN(s.music) ? .5f : Mathf.Clamp01(s.music);
        s.effects = float.IsNaN(s.effects) ? .8f : Mathf.Clamp01(s.effects);
        s.quality = Mathf.Clamp(s.quality, 0, 2);
    }

    private static bool ValidCoin(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        string[] parts = value.Split(':');
        return parts.Length == 2 && Array.IndexOf(CampaignCatalog.Ids, parts[0]) >= 0 && int.TryParse(parts[1], out int n) && n >= 0 && n < 20;
    }

    public static RatLevelRecord Record(string id)
    {
        EnsureLoaded();
        RatLevelRecord record = data.levels.Find(r => r.id == id);
        if (record == null) { record = new RatLevelRecord { id = id }; data.levels.Add(record); }
        return record;
    }
    public static int Collected(int level) => Data.coins.FindAll(id => id.StartsWith(CampaignCatalog.Ids[level] + ":", StringComparison.Ordinal)).Count;
    public static bool HasCoin(string id) => Data.coins.Contains(id);
    public static bool PickedThisAttempt(string id) => Playing && Data.active.pickedCoins.Contains(id);
    public static int StartingRats(int level) => Data.entryRats[Mathf.Clamp(level, 0, CampaignCatalog.Count - 1)];
    public static void Save()
    {
        EnsureLoaded();
        try
        {
            string previous = PlayerPrefs.GetString(SaveKey, "");
            if (!string.IsNullOrEmpty(previous)) PlayerPrefs.SetString(SaveKey + ".previous", previous);
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data)); PlayerPrefs.Save();
        }
        catch (Exception) { Notice = "Progress will not be saved: local storage is unavailable."; }
        Changed?.Invoke();
    }
    public static void Begin(int level)
    {
        EnsureLoaded();
        if (Playing) Finish("abandoned", 0);
        data.selected = level; data.hasCampaign = true;
        data.active = new RatAttempt { id = Guid.NewGuid().ToString("N"), level = CampaignCatalog.Ids[level] };
        Record(data.active.level).attempts++;
        Save();
    }
    public static void AddTime(float seconds)
    {
        if (!Playing || seconds <= 0) return;
        data.active.seconds += seconds; Record(data.active.level).seconds += seconds;
    }
    public static void Death()
    {
        if (!Playing) return;
        data.active.deaths++; Record(data.active.level).deaths++; Save();
    }
    public static bool Collect(string id)
    {
        if (!Playing || !ValidCoin(id) || !id.StartsWith(data.active.level + ":", StringComparison.Ordinal) || data.active.pickedCoins.Contains(id)) return false;
        data.active.pickedCoins.Add(id);
        if (!data.coins.Contains(id)) data.coins.Add(id);
        data.earned++; data.active.coins++; Save(); return true;
    }
    public static RatAttempt Finish(string outcome, int survivors)
    {
        if (!Playing) return null;
        RatAttempt result = data.active;
        result.outcome = outcome; result.survivors = Mathf.Clamp(survivors, 0, 3);
        RatLevelRecord record = Record(result.level);
        if (outcome == "won")
        {
            record.wins++;
            if (record.wins == 1 || result.seconds < record.bestSeconds) record.bestSeconds = result.seconds;
            record.bestRats = Mathf.Max(record.bestRats, result.survivors);
            int index = Array.IndexOf(CampaignCatalog.Ids, result.level);
            data.unlocked = Mathf.Max(data.unlocked, Mathf.Min(CampaignCatalog.Count, index + 2));
            data.selected = Mathf.Min(index + 1, CampaignCatalog.Count - 1);
            if(index + 1 < CampaignCatalog.Count) data.entryRats[index + 1] = Mathf.Clamp(result.survivors, 1, 3);
            if (index == CampaignCatalog.Count - 1 && !data.campaignComplete) { data.campaignComplete = true; data.campaignClears++; }
        }
        else if (outcome == "lost") record.failures++;
        else record.abandoned++;
        data.active = null; Save(); return result;
    }
    public static bool Buy(int outfit)
    {
        if (outfit < 1 || outfit >= CampaignCatalog.OutfitIds.Length || Data.owned.Contains(CampaignCatalog.OutfitIds[outfit]) || Wallet < CampaignCatalog.Prices[outfit]) return false;
        data.spent += CampaignCatalog.Prices[outfit]; data.owned.Add(CampaignCatalog.OutfitIds[outfit]); Save(); return true;
    }
    public static void Equip(int outfit)
    {
        if (outfit < 0 || outfit >= CampaignCatalog.OutfitIds.Length || !Data.owned.Contains(CampaignCatalog.OutfitIds[outfit])) return;
        data.equipped = CampaignCatalog.OutfitIds[outfit]; Save();
    }
    public static void NewCampaign()
    {
        if (Playing) Finish("abandoned", 0);
        Data.unlocked = 1; data.selected = 0; data.campaignComplete = false; data.hasCampaign = true;
        for(int i = 0; i < data.entryRats.Count; i++) data.entryRats[i] = 3;
        Save();
    }
    public static void Erase()
    {
        // Erasure must not leave a recoverable copy of the previous personal profile.
        try { PlayerPrefs.DeleteKey(SaveKey); PlayerPrefs.DeleteKey(SaveKey + ".previous"); PlayerPrefs.DeleteKey(SaveKey + ".recovery"); }
        catch (Exception) { Notice = "Progress will not be saved: local storage is unavailable."; }
        data = new RatProfileData();
        foreach (string id in CampaignCatalog.Ids) Record(id);
        Save();
    }
    public static string TimeText(double seconds) => TimeSpan.FromSeconds(Math.Max(0, seconds)).ToString(seconds >= 3600 ? @"h\:mm\:ss" : @"mm\:ss");
}
