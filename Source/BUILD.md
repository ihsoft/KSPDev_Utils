### Documentation

The help files must be made _before_ merging into the master branch. I.e. the code changes and the
updated docs must land into the `next` branch prior to the merge. They all must be merged into
`master` in one PR.

Be patient on the docs building. It may take considerable time for the project to build. Even more
patience will be needed on the commit.

If you have an anti-virus installed in your system, consider disabling it temporarily - the
builder will emit somewhat of 100k of files. Checking them all for viruses doesn't make sense,
but the overall performance may drop significantly.

Prerequisites:

- Download and install the latest build of [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB/releases).
- Download and install the [.NET Framework for developers](https://docs.microsoft.com/en-us/dotnet/framework/install/guide-for-developers)
  for the current game `.Net` version:
  - Note, that the older versions get eventually deprecated. If this is the case, choose the
    minumum version available.
  - You can also check the `Binaries` folder for the appropriate packages.
  - It's a good idea to commit the working versions of the developer packages into the `Binaries`
    folder. Drop the older version(s) if you commit a newer one.

Steps:

- Load project `Source/docs_project/KSPDev_Utils.shfbproj`.
- Update the output folder to the appropriate utils version.
- Run the build.
- Update `docs/README.md` file with the new version link.
- Commit all the changes.
  - It could be a very long process! If a new version is being made, prepare
    to wait for about 30 minutes.
