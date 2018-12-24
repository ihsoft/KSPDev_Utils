The scripts in this folder are used to make and publish the Utils binary.

In order to release a new version, do the following:

1. Ensure the right version is set in `AssemblyInfo.cs`. It will be used to mark the release.
2. Check if `CHANGELOG.md` has the latest version described at the top of the file. This info will be published for the release notes.
3. Have all the sources updated and commited.
4. Run `MakePackage.py`. It will compile the sources and create a redistribution ZIP.
   - Due to Python 2.7 bug, the ZIP archive will have an empty folder, named `.`. Drop it manually before publishing the release.
5. Run `publish_github.cmd` to create a GitHub release draft.
6. Got to GitHub and publish the draft!
