using System.IO;
using System;
using Reqnroll;

[Binding]
public class Hooks
{
    private static readonly string AllureResultsPath = Path.Combine(Directory.GetCurrentDirectory(), "allure-results");

    [BeforeTestRun(Order = 0)]
    public static void CleanAllureResultsFolder()
    {
        try
        {
            if (Directory.Exists(AllureResultsPath))
            {
                Console.WriteLine($"[Allure Cleanup] Deleting old Allure results at: {AllureResultsPath}");
                Directory.Delete(AllureResultsPath, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Allure Cleanup] Failed to clean Allure results folder: {ex.Message}");
        }
    }
}
