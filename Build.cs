using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.Tools.DotNet;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;


class Build : NukeBuild
{
    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    Project Revit2021Project => Solution.RevitTemplateWeb2021;
    Project Revit2022Project => Solution.RevitTemplateWeb2022;
    Project Revit2023Project => Solution.RevitTemplateWeb2023;

    public Guid ClientId => new Guid("513061CC-801C-41B1-BD00-887E5493EA3C");
    public string RevitVendorId => "RVT_TEMPLATE_WEB";
    public string RevitVendorDescription => "ames.codes";
    public string AppName => "Revit Addin Template - Web";
    public string ExternalAppClassName => "RevitTemplateWeb.ExtApp";

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

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    string OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {

        });

    Target CreateAddinManifest => _ => _
        .Executes(() =>
        {

            {
                string assemblyName = Revit2021Project.GetProperty("AssemblyName");
                string revitVersion = Revit2021Project.GetProperty("RevitVersion");

                relativeDllLocation = IsLocalBuild ? @$"{assemblyName}/{assemblyName}.dll" : $@"{assemblyName}.dll";

                File.WriteAllText(Path.Combine(OutputDirectory, $@"{assemblyName}.dll"), AddinFileContents);
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(CreateAddinManifest)
        .Executes(() =>
        {

        });
}