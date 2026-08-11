namespace MIS.Infrastructure.Files;

public sealed class HrFileStorageOptions
{
    public const string SectionName = "HrFiles";

    public string RootPath { get; set; } = "App_Data/HrFiles";
}
