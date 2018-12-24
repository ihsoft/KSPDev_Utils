# Public domain license.
# Author: igor.zavoychinskiy@gmail.com
# A very simple script to pack KSPDev Utils into a ZIP.

import os.path
import re
import shutil
import subprocess
import sys

PROJECT_ROOT = '../'
RELEASE_FOLDER = PROJECT_ROOT + 'Release/'
BIN_FOLDER = PROJECT_ROOT + 'Source/Bin/Release/'
BINARY_PATH = BIN_FOLDER + 'KSPDev_Utils.dll'
ASSEMBLY_INFO_FILE = PROJECT_ROOT + 'Source/Properties/AssemblyInfo.cs'


# Checks if path doesn't try to address a file above the root.
#
# @param test_path The path to check.
# @param action The name of the action that needs the check. It will be reported in case of
#     negative result.
# @return Absolute path for {@code test_path}.
def CheckChroot(test_path, chroot=None, action=None):
  abs_test_path = os.path.abspath(test_path)
  abs_chroot = os.path.abspath(os.path.join(chroot or PROJECT_ROOT))
  rel_path = os.path.relpath(abs_test_path, abs_chroot)
  if rel_path.startswith('..'):
    print 'ERROR: Action %s is not permitted above the root: %s (root=%s)' % (
        action, abs_test_path, abs_chroot)
    raise RuntimeError('Path is not secure!')
  return abs_test_path


# Creates all elements in the path if they don't exist. Ensures the folders are created within
# {PROJECT_ROOT}.
def OsSafeMakedirs(folder):
  abs_path = CheckChroot(folder, action='MAKE PATH')
  if not os.path.isdir(abs_path):
    print '=> create folder:', abs_path
    os.makedirs(abs_path)


# Copies a file or folder. Ensures that source is defined within {PROJECT_ROOT} and target is in
# {RELEASE}.
def OsSafeCopyToRelease(src_filename, dest_folder, source_must_exist=True, dest_filename=None):
  abs_src = CheckChroot(src_filename, action='COPY-FROM')
  abs_dest_folder = CheckChroot(dest_folder, chroot=RELEASE_FOLDER, action='COPY-TO')
  abs_dest = CheckChroot(
      abs_dest_folder + '/' + (dest_filename or ''), chroot=RELEASE_FOLDER, action='COPY-TO')
  if os.path.isfile(abs_src):
    OsSafeMakedirs(abs_dest_folder)
    if dest_filename:
      print '=> copy file:', abs_src, '(rename: %s)' % os.path.basename(abs_dest)
    else:
      print '=> copy file:', abs_src
    shutil.copy(abs_src, abs_dest)
  elif os.path.isdir(abs_src):
    print '=> copy folder:', abs_src
    shutil.copytree(abs_src, os.path.join(abs_dest_folder, os.path.basename(abs_src)))
  else:
    if source_must_exist:
      print 'ERROR: Source path not found"', abs_src
      exit(-1)
    print "=> skipping:", abs_src


# Deletes a file or folder. Ensures that path is defined within {RELEASE}.
def OsSafeDeleteFromDest(path):
  abs_path = CheckChroot(path, chroot=RELEASE_FOLDER, action='DELETE')
  if os.path.isfile(abs_path):
    print '=> drop file:', abs_path
    os.unlink(abs_path)
  else:
    print '=> drop folder:', abs_path
    shutil.rmtree(abs_path, True)


# Extracts the version number of the release from the source file.
def ExtractVersion():
  file_path = CheckChroot(ASSEMBLY_INFO_FILE, action='GET VESION')
  print 'Extract release version...'
  print '=> AssemblyInfo:', file_path
  with open(file_path) as f:
    content = f.readlines()
  for line in content:
    if line.lstrip().startswith('//'):
      continue
    # Expect: [assembly: AssemblyVersion("X.Y.Z")]
    matches = re.match(
        r'\[assembly: AssemblyVersion.*\("(\d+)\.(\d+)\.(\*|\d+)(.(\*|\d+))?"\)\]', line)
    if matches:
      print '=> found version: v%s.%s' % (matches.group(1), matches.group(2))
      return '%s.%s' % (matches.group(1), matches.group(2))
  print 'ERROR: Cannot extract version from: %s' % file_path
  exit(-1)


# Creates a package for re-destribution.
def MakePackage(filename, overwrite_existing):
  print 'Making %s package...' % os.path.basename(filename)
  abs_archive_name = CheckChroot(PROJECT_ROOT + filename, action='PACKAGE')
  if os.path.exists(abs_archive_name): 
    if not overwrite_existing:
      print 'ERROR: Package for this version already exists: %s' % abs_archive_name
      exit(-1)
    print '=> package already exists. DELETING.'
    os.remove(abs_archive_name)
  abs_release_folder = CheckChroot(RELEASE_FOLDER, action='RELEASE')
  shutil.make_archive(abs_archive_name, 'zip', abs_release_folder)
  print '=> stored in:', abs_archive_name


# Makes the binary.
def CompileBinary():
  print 'Compiling sources in PROD mode...'
  binary_path = CheckChroot(BINARY_PATH, action='BINARY')
  code = subprocess.call('make_binary.cmd')
  if code != 0 or not os.path.exists(binary_path):
    print 'ERROR: Compilation failed. Cannot find target DLL:', binary_path
    exit(code)


def main(argv):
  CompileBinary()

  print 'Cleanup release folder...'
  OsSafeDeleteFromDest(RELEASE_FOLDER)

  version = ExtractVersion()

  print "Make release structure..."
  OsSafeCopyToRelease(BINARY_PATH, RELEASE_FOLDER,
                      dest_filename='KSPDev_Utils.%s.dll' % version)
  OsSafeCopyToRelease(BIN_FOLDER + 'KSPDev_Utils.xml', RELEASE_FOLDER,
                      dest_filename='KSPDev_Utils.%s.xml' % version)
  OsSafeCopyToRelease('../LICENSE.md', RELEASE_FOLDER, dest_filename='KSPDev_Utils_LICENSE.md')
  OsSafeCopyToRelease('../README.md', RELEASE_FOLDER, dest_filename='KSPDev_Utils_README.md')

  MakePackage('KSPDevUtils_v%s' % version, False)

  print '\n!!! Manually drop the bogus folder "." from the target archive !!!'


main(sys.argv)
