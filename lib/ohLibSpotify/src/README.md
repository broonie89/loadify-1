# src

The src directory is the root for the source code used to build the project.

## packages.config

This is a nuget packages file. It contains packages that should be fetched even if
no individual project depends on them. This is most commonly used for tools that
are used by automated builds and tests, such as NUnit.Runners.

This file is not directly consumed during a build. It is used by the
"go update-nuget" command, which searches for all packages.config files in the
/src hierarchy, and consolidates them into one file in /projectdata.

## SharedSettings.targets

This is an msbuild file that is imported by each project. It contains common
settings and targets used in multiple projects.

## Solution files

Visual Studio solution files go here.

## Subdirectories

There's one subdirectory for each msbuild project.
