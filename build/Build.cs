﻿// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/ide-extensions/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using Nuke.Common.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.NuGet;
using Nuke.Core;
using Nuke.Core.Utilities;
using Nuke.Core.Utilities.Collections;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Core.Tooling.NuGetPackageResolver;
using static Nuke.Core.IO.FileSystemTasks;
using static Nuke.Core.IO.PathConstruction;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;


class Build : NukeBuild
{
    // Console application entry. Also defines the default target.
    public static int Main() => Execute<Build>(x => x.Changelog);

    [Parameter] readonly string Source = "https://resharper-plugins.jetbrains.com/api/v2/package";
    [Parameter] readonly string ApiKey;

    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    string ProjectFile => SourceDirectory / "ReSharper.Nuke" / "ReSharper.Nuke.csproj";

    Target Clean => _ => _
        .Executes(() =>
        {
            DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            MSBuild(s => DefaultMSBuildRestore);
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            MSBuild(s => DefaultMSBuildCompile);
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
        });

    string ChangelogFile => RootDirectory / "CHANGELOG.md";

    Target Changelog => _ => _
        .OnlyWhen(() => InvokedTargets.Contains(nameof(Changelog)))
        .Executes(() =>
        {
            FinalizeChangelog(ChangelogFile, GitVersion.SemVer, GitRepository);

            Git($"add {ChangelogFile}");
            Git($"commit -m \"Finalize {Path.GetFileName(ChangelogFile)} for {GitVersion.SemVer}.\"");
            Git($"tag -f {GitVersion.SemVer}");
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            GlobFiles(SourceDirectory, "*.nuspec")
                .ForEach(x => NuGetPack(s => DefaultNuGetPack
                    .SetTargetPath(x)
                    .SetBasePath(Path.GetDirectoryName(x))
                    .SetProperty("wave", GetWaveVersion(ProjectFile) + ".0")
                    .SetProperty("currentyear", DateTime.Now.Year.ToString())
                    .EnableNoPackageAnalysis()));
        });

    Target Push => _ => _
        .DependsOn(Pack, Test, Changelog)
        .Requires(() => ApiKey)
        .Requires(() => Configuration.EqualsOrdinalIgnoreCase("Release"))
        .Executes(() =>
        {
            GlobFiles(OutputDirectory, "*.nupkg")
                .ForEach(x => NuGetPush(s => s
                    .SetTargetPath(x)
                    .SetSource(Source)
                    .SetApiKey(ApiKey)));
        });

    static string GetWaveVersion(string packagesConfigFile)
    {
        var fullWaveVersion = GetLocalInstalledPackages(packagesConfigFile, includeDependencies: true)
            .SingleOrDefault(x => x.Id == "Wave").NotNull("fullWaveVersion != null").Version.ToString();
        return fullWaveVersion.Substring(startIndex: 0, length: fullWaveVersion.IndexOf(value: '.'));
    }
}