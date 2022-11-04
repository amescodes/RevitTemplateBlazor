using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;


class Build : NukeBuild
{
    [Solution]
    readonly Solution Solution;

    //[GitVersion]
    //readonly GitVersion GitVersion;

    //[Parameter("Configuration to build. Use the Revit version year (2021, 2022, 2023) as a parameter to only build for one version.")]
    //readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    bool IsReleaseBuild = false;

    string OutputDirectory => IsReleaseBuild ? RootDirectory / "release" : RootDirectory / "output";

    public Guid ClientId => new Guid("513061CC-801C-41B1-BD00-887E5493EA3C");
    public string AppName => "Revit Addin Template - Web";
    public string ExternalAppClassName => "RevitTemplateWeb.ExtApp"; //? potential to automate

    public string RevitVendorId => "AMES_CODES";
    public string RevitVendorDescription => "ames.codes";

    //string relativeDllLocation = "";
    public string AddinFileContents => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<RevitAddIns>
  <AddIn Type=""Application"">
    <FullClassName>{ExternalAppClassName}</FullClassName>
    <ClientId>{ClientId}</ClientId>
    <Name>{AppName}</Name>
    <Assembly>RELATIVE_DLL_LOCATION</Assembly>
    <VendorId>{RevitVendorId}</VendorId>
    <VendorDescription>{RevitVendorDescription}</VendorDescription>
  </AddIn>
</RevitAddIns>";

    string addinFileString => $"{Solution.Name}.addin";

    public static int Main() => Execute<Build>(x => x.CompileAll);


    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);

            string commonPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string pcAddinPath = Path.Combine(commonPath, $@"Autodesk\\Revit\Addins");

            // 2021
            string pcAddinPath2021 = Path.Combine(pcAddinPath, $@"2021\\{Solution.Name}");
            if (Directory.Exists(pcAddinPath2021))
            {
                Directory.Delete(pcAddinPath2021,true);

                string addinFile2021 = Path.Combine(pcAddinPath, $@"2021\\{Solution.Name}.addin");
                if (File.Exists(addinFile2021))
                {
                    File.Delete(addinFile2021);
                }
            }
            // 2022
            string pcAddinPath2022 = Path.Combine(pcAddinPath, $@"2022\\{Solution.Name}");
            if (Directory.Exists(pcAddinPath2022))
            {
                Directory.Delete(pcAddinPath2022,true);

                string addinFile2022 = Path.Combine(pcAddinPath, $@"2022\\{Solution.Name}.addin");
                if (File.Exists(addinFile2022))
                {
                    File.Delete(addinFile2022);
                }

            }
            // 2023
            string pcAddinPath2023 = Path.Combine(pcAddinPath, $@"2023\\{Solution.Name}");
            if (Directory.Exists(pcAddinPath2023))
            {
                Directory.Delete(pcAddinPath2023,true);

                string addinFile2023 = Path.Combine(pcAddinPath, $@"2023\\{Solution.Name}.addin");
                if (File.Exists(addinFile2023))
                {
                    File.Delete(addinFile2023);
                }

            }
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore();
        });

    //Target CreateAddinManifest => _ => _
    //    .Executes(() =>
    //    {

    //    });

    Target CompileAll => _ => _
        .DependsOn(Compile2021)
        .DependsOn(Compile2022)
        .DependsOn(Compile2023);

    Target Compile2021 => _ => _
        .DependsOn(CompileOther)
        .Executes(() =>
        {
            CompileByVersion("2021");
        });

    Target Compile2022 => _ => _
        .DependsOn(CompileOther)
        .Executes(() =>
        {
            CompileByVersion("2022");
        });

    Target Compile2023 => _ => _
        .DependsOn(CompileOther)
        .Executes(() =>
        {
            CompileByVersion("2023");
        });

    Target CompileOther => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            IEnumerable<Project> nonRevitProjects = GetNonRevitProjectsToBuild();
            string nonRevitDirPath = Path.Combine(OutputDirectory, "temp");
            DirectoryInfo nonRevitDir = Directory.CreateDirectory(nonRevitDirPath);
            foreach (Project nonRevitProject in nonRevitProjects)
            {
                try
                {
                    DotNetBuild(_ => _
                        .SetProjectFile(nonRevitProject)
                        .SetConfiguration(Configuration.Debug)
                        .SetOutputDirectory(nonRevitDir.FullName));
                }
                catch
                {
                    // keep try/catch until figuring out what project/why it's breaking
                }
            }
        });

    Target CopyToAddinDirectory => _ => _
        .Executes(() =>
        {
            string commonPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string pcAddinPath = Path.Combine(commonPath, $@"Autodesk\\Revit\Addins");

            // 2021
            string outputPath2021 = Path.Combine(OutputDirectory, "2021");
            if (Directory.Exists(outputPath2021))
            {
                string pcAddinPath2021 = Path.Combine(pcAddinPath, $@"2021");
                CopyFilesRecursively(outputPath2021, pcAddinPath2021);
            }
            // 2022
            string outputPath2022 = Path.Combine(OutputDirectory, "2022");
            if (Directory.Exists(outputPath2022))
            {
                string pcAddinPath2022 = Path.Combine(pcAddinPath, $@"2022");
                CopyFilesRecursively(outputPath2022, pcAddinPath2022);
            }
            // 2023
            string outputPath2023 = Path.Combine(OutputDirectory, "2023");
            if (Directory.Exists(outputPath2023))
            {
                string pcAddinPath2023 = Path.Combine(pcAddinPath, $@"2023");
                CopyFilesRecursively(outputPath2023, pcAddinPath2023);
            }
        });

    void CompileByVersion(string revitVersion)
    {
        string nonRevitDirPath = Path.Combine(OutputDirectory, "temp");
        if (!Directory.Exists(nonRevitDirPath))
        {
            throw new Exception("other dll directory doesn't exist");
        }

        string versionDirPath = BuildRevitProject(revitVersion);

        CopyFilesRecursively(nonRevitDirPath, versionDirPath);

        CreateAddinManifest(revitVersion);
    }

    /// <summary>
    /// Returns path of built DLL(s).
    /// </summary>
    /// <param name="revitVersion"></param>
    /// <returns></returns>
    string BuildRevitProject(string revitVersion)
    {
        Project revitProject = Solution.GetProject($@"{Solution.Name}{revitVersion}");
        string versionDirPath = Path.Combine(OutputDirectory, revitVersion);
        Directory.CreateDirectory(versionDirPath);

        string nestedDllDirPath = Path.Combine(versionDirPath, Solution.Name);
        DirectoryInfo nestedDllDir = Directory.CreateDirectory(nestedDllDirPath);

        Configuration config = Configuration.Debug;
        switch (revitVersion)
        {
            case "2021":
                {
                    config = Configuration.Revit2021;
                    break;
                }
            case "2022":
                {
                    config = Configuration.Revit2022;
                    break;
                }
            case "2023":
                {
                    config = Configuration.Revit2023;
                    break;
                }
            default:
                {
                    break;
                }
        }

        DotNetBuild(_ => _
            .SetProjectFile(revitProject)
            .SetConfiguration(config)
            .SetFramework(revitProject.GetProperty("Framework"))
            .SetOutputDirectory(nestedDllDir.FullName));

        return versionDirPath;
    }

    void CreateAddinManifest(string revitVersion)
    {
        string assemblyName = Solution.Name;
        //string assemblyName = revitProject.GetProperty("AssemblyName");
        string relativeDllLocation = IsReleaseBuild ? $"{assemblyName}.dll" : $"{assemblyName}/{assemblyName}.dll";
        string finalAddinContents = AddinFileContents.Replace("RELATIVE_DLL_LOCATION", relativeDllLocation);

        string versionDirPath = Path.Combine(OutputDirectory, revitVersion);
        File.WriteAllText(Path.Combine(versionDirPath, addinFileString), finalAddinContents);
    }

    IList<Project> GetNonRevitProjectsToBuild()
    {
        IList<Project> projectsToBuild = new List<Project>();
        foreach (Project project in Solution.AllProjects)
        {
            try
            {
                if (project.GetProperty("RevitProject") is string isRevitProject &&
                    isRevitProject.Equals("true"))
                {
                }
            }
            catch
            {
                // ignored - there's no TryGetProperty so we wrap
                // in try/catch for the projects that will def fail
                projectsToBuild.Add(project);
            }
        }

        return projectsToBuild;
    }

    static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}