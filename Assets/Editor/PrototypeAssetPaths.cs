/// <summary>Shared asset locations for editor authoring, launch and verification tools.</summary>
public static class PrototypeAssetPaths
{
    public const string Scenes = "Assets/Scenes";
    public const string PrototypeScene = Scenes + "/RatEnclosure.unity";
    public const string StarterScene = Scenes + "/StartScene.unity";
    public const string Materials = "Assets/Materials";
    public const string EnvironmentMaterials = Materials + "/Environment";
    public const string CharacterMaterials = Materials + "/Characters";
    public const string GlassMaterial = EnvironmentMaterials + "/GlassEnclosure.mat";

    public static string LevelMaterial(string name)
    {
        string folder = name == "Rat ivory" || name == "Rat ears" ? CharacterMaterials : EnvironmentMaterials;
        return folder + "/" + name + ".mat";
    }
}
