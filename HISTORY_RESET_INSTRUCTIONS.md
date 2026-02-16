# Git History Reset Instructions

This document explains how to reset the git history to a single commit, removing all unwanted files from the history.

## Problem

The git history contains files that shouldn't be there. To clean this up, we need to reset the repository to a single commit with only the current desired state.

## Solution

Due to GitHub branch protection and automation limitations, this operation requires manual intervention. Follow these steps to reset the git history:

### Step 1: Backup the Current State (Optional but Recommended)

```bash
git branch backup-before-reset
```

### Step 2: Create an Orphan Branch with Clean History

```bash
# Create a new orphan branch (no parent commits)
git checkout --orphan new-main

# Add all current files
git add -A

# Create a single initial commit
git commit -m "Initial commit - clean repository history"
```

### Step 3: Replace the Main Branch

```bash
# Delete the old main branch locally
git branch -D main

# Rename the new branch to main
git branch -m main

# Force push to GitHub (requires force push permission)
git push -f origin main
```

### Alternative: Reset Current Branch

If you want to reset the current branch instead of main:

```bash
# Create orphan branch
git checkout --orphan temp-clean

# Add and commit all files
git add -A
git commit -m "Initial commit - clean repository history"

# Force the current branch to point to this commit
git branch -f <your-branch-name> temp-clean

# Switch back to your branch
git checkout <your-branch-name>

# Delete the temporary branch
git branch -d temp-clean

# Force push
git push -f origin <your-branch-name>
```

## What Gets Removed

This process will:
- Remove all commit history
- Remove any files that were previously tracked but have since been deleted
- Keep only the current state of files in the working directory

## What Gets Kept

- All current files in the repository
- The .gitignore rules (so previously ignored files remain ignored)

## Verification

After resetting, verify the history:

```bash
# Should show only one commit
git log --oneline

# Check that all desired files are present
git ls-files
```

## Notes

- This operation rewrites history and requires force push
- All collaborators will need to re-clone the repository or reset their local copies
- Any open pull requests based on the old history will need to be recreated
- This cannot be easily undone, so make sure you have backups

## For This PR Branch

To reset just this PR branch (`copilot/remove-git-history`), the repository owner should run:

```bash
git checkout copilot/remove-git-history
git checkout --orphan temp-clean
git add -A
git commit -m "Initial commit - clean repository history"
git branch -f copilot/remove-git-history temp-clean
git checkout copilot/remove-git-history
git branch -d temp-clean
git push -f origin copilot/remove-git-history
```
