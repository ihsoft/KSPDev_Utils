### Documentation

The help files must be made _before_ merging into the master branch. I.e. the code changes and the
updated docs must land into the `next` branch. They all must merge in one PR.

Be patient on the docs building. It may take considerable time for the project to build.

If you have an anti-virus installed in your system, consider disabling it temporarily - the
builder will emit somewhat of 100k of files. Checking them for viruses doesn't make sense,
but the overall performance can drop significantly.

Prerequisites:

- Download and install the latest build of [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB/releases).
- Download and install the [.NET Framework for developers](https://docs.microsoft.com/en-us/dotnet/framework/install/guide-for-developers)
  for the current game .Net target.
  - Note, that the older versions get eventually deprecated. If this is the case, choose the
    minumum version available.
  - You can also check the `Binaries` folder for the appropriate packages.
  - It's a good idea to commit the working versions of the developer packages into the `Binaries`
    folder.

Steps:

- Download the latest build and install.
- Install the [.NET Framework for developers](https://docs.microsoft.com/en-us/dotnet/framework/install/guide-for-developers).
  Choose the right version!
  - Note, that the older versions get eventually deprecated. If this is the case, choose the
    minumum version available.
  - You can also check the `Binaries` folder for the appropriate packages.
  - It's a good idea to commit the working versions of the developer packages into the `Binaries`
    folder.
- Load project `Source/docs_project/KSPDev_Utils.shfbproj`.
- Update the output folder to the appropriate utils version.
- Run the build.
- Update `docs/README.md` file with the new version link.
- Commit all the changes.
