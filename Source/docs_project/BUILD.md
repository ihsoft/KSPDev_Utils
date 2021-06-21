# Building the documentation system

The help files must be made _before_ merging into the master branch. I.e. the code changes and the
updated docs must land into the `next` branch prior to the merge. They all must be merged into
`master` in one PR.

#### Prerequisites

- Download and install the latest build of [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB/releases).
- Download and install the [.NET Framework for developers](https://docs.microsoft.com/en-us/dotnet/framework/install/guide-for-developers)
  for the current game `.Net` version:
  - Note, that the older versions get eventually deprecated. If this is the case, choose the
    minumum version available.
  - The current compatible version is `ndp48-devpack-enu-4.5.1.exe`.

#### Steps

- Load project `Source/docs_project/KSPDev_Utils.shfbproj`.
- Update the output folder to the appropriate utils version.
- Run the build.
- Update `docs/README.md` file with the new version link.
- Commit all the changes.

# Things to NOT do

Don't even try to build the `KSP_API.shfbproj` project! If you do, it will create somewhat 100k of
files in the file system. And ALL of them won't make any sense for `KSPDev_Utils`. The `KSP_API`
project only exists to avoid the false unresolved referrences warnings in the main project (a
workaround for the SHFB issue). Never build it on its own!!!
