using UnityEngine;

[System.Serializable]
public class BuildInfo
{
    public string CompanyName;
    public string ApplicationName;

    public int MajorVersion;
    public int MinorVersion;
    public int PatchVersion;
    public int BuildVersion;

    public string BuildNotes;

    public string BuildDateTime;

    public bool Release;

    public static BuildInfo DefaultValues = new BuildInfo
    {
        CompanyName = "PenseCre",
        ApplicationName = "Nosso Projeto",
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        BuildVersion = 0,
        BuildNotes = "-",
        BuildDateTime = "",
        Release = false
    };
}