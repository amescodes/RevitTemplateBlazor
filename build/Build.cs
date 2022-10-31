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

    [Parameter("Configuration to build. Use the Revit version year (2021, 2022, 2023) as a parameter to only build for one version.")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    bool IsReleaseBuild => Configuration.Equals(Configuration.Release);

    string OutputDirectory => IsReleaseBuild ? RootDirectory / "release" : RootDirectory / "output";

    public Guid ClientId => new Guid("513061CC-801C-41B1-BD00-887E5493EA3C");
    public string AppName => "Revit Addin Template - Web";
    public string ExternalAppClassName => "RevitTemplateWeb.ExtApp"; //? potential to automate

    public string RevitVendorId => "AMES_CODES";
    public string RevitVendorDescription => "ames.codes";

    string relativeDllLocation = "";
    public string AddinFileContents => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<RevitAddIns>
  <AddIn Type=""Application"">
    <FullClassName>{ExternalAppClassName}</FullClassName>
    <ClientId>{ClientId}</ClientId>
    <Name>{AppName}</Name>
    <Assembly>{relativeDllLocation}</Assembly>
    <VendorId>{RevitVendorId}</VendorId>
    <VendorDescription>{RevitVendorDescription}</VendorDescription>
  </AddIn>
</RevitAddIns>";

    public static int Main() => Execute<Build>(x => x.Compile);


    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore();
        });

    Target CreateAddinManifest => _ => _
        .Executes(() =>
        {
            IList<Project> projectsToBuild = GetRevitProjectsToBuild();

            foreach (Project project in projectsToBuild)
            {
                string assemblyName = project.GetProperty("AssemblyName");
                relativeDllLocation = IsReleaseBuild ? $"{assemblyName}.dll" : $"{assemblyName}/{assemblyName}.dll";

                string revitVersion = project.GetProperty("RevitVersion");
                string versionDirPath = Path.Combine(OutputDirectory, revitVersion);
                DirectoryInfo versionDir = Directory.CreateDirectory(versionDirPath);

                File.WriteAllText(Path.Combine(versionDir.FullName, $"{assemblyName}.addin"), AddinFileContents);
            }
        });

    Target Compile => _ => _
        .DependsOn(CreateAddinManifest)
        .Executes(() =>
        {
            string uiOutputDir = Path.Combine(OutputDirectory, "UI");
            Project uiProject = Solution.GetProject($@"{Solution.Name}.UI");
            DotNetBuild(_ => _
                    .SetProjectFile(uiProject)
                    .SetConfiguration(Configuration)
                    .SetOutputDirectory(uiOutputDir));
            
            IList<Project> projectsToBuild = GetRevitProjectsToBuild();
            foreach (Project project in projectsToBuild)
            {
                string revitVersion = project.GetProperty("RevitVersion");
                string versionDirPath = Path.Combine(OutputDirectory, revitVersion);
                DirectoryInfo versionDir = Directory.CreateDirectory(versionDirPath);

                DotNetBuild(_ => _
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration)
                    .SetFramework(project.GetProperty("Framework"))
                    .SetOutputDirectory(versionDir.FullName));
                
                if (Directory.Exists(uiOutputDir))
                {
                    CopyFilesRecursively(uiOutputDir,Path.Combine(versionDirPath,"UI"));
                }
            }
        });

    IList<Project> GetRevitProjectsToBuild()
    {
        IList<Project> projectsToBuild = new List<Project>();

        if (Configuration.Equals(Configuration.Revit2021))
        {
            projectsToBuild.Add(Solution.GetProject($@"{Solution.Name}2021"));
        }
        else if (Configuration.Equals(Configuration.Revit2022))
        {
            projectsToBuild.Add(Solution.GetProject($@"{Solution.Name}2022"));
        }
        else if (Configuration.Equals(Configuration.Revit2023))
        {
            projectsToBuild.Add(Solution.GetProject($@"{Solution.Name}2023"));
        }
        else
        {
            foreach (Project project in Solution.AllProjects.Where(p=>!p.Name.Equals("_build") &&
                                                                      !p.Name.EndsWith("Shared") &&
                                                                      !p.Name.EndsWith("UI")))
            {
                projectsToBuild.Add(project);
            }
        }

        return projectsToBuild;
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}