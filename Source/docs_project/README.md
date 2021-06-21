# Building documentation for a new version

KSPDev Utils is a redistributable library, which means there are many different versions in the
field at the given moment of time. To not have conflicts with the older versions, each version of
Utils gest its own folder on the documentation site. So the first step before building docs is
changing the output directory.

The base output directory is:
[`<repo root>/docs`](https://github.com/ihsoft/KSPDev/tree/master/docs).
Each version has own subfolder there, named after the version, e.g. `v1.0` holds documentation for
version `1.0`.

For every new version of the documentation, the
[`README.md`](../../docs/README.md) on the documentation site must be extended. The links to the
newer versions come at the top.

For more details on building the docs, refer [BUILD.md](BUILD.md).

# When updating documenation

Every major or minor version of the Utils must have the updated docs. It's OK to skip updates when
a patch is released (the third number in the version).
