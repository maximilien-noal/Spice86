# Rebase Plan: Sync Fork with Upstream Master

This document provides a step-by-step plan to rebase the `maximilien-noal/Spice86` fork on the upstream `OpenRakis/Spice86:master` while preserving all local modifications.

## Current State
- **Fork**: `maximilien-noal/Spice86:master`
- **Upstream**: `OpenRakis/Spice86:master`
- **Status**: 27 commits ahead, 20 commits behind

## Prerequisites

1. Ensure you have a local clone of your fork
2. Backup your work before proceeding (optional but recommended)

## Step-by-Step Rebase Commands

### Step 1: Clone and Setup (if not already done)

```bash
# Clone your fork (if needed)
git clone https://github.com/maximilien-noal/Spice86.git
cd Spice86

# Add upstream remote
git remote add upstream https://github.com/OpenRakis/Spice86.git

# Verify remotes
git remote -v
# Should show:
# origin    https://github.com/maximilien-noal/Spice86 (fetch)
# origin    https://github.com/maximilien-noal/Spice86 (push)
# upstream  https://github.com/OpenRakis/Spice86 (fetch)
# upstream  https://github.com/OpenRakis/Spice86 (push)
```

### Step 2: Fetch Latest from Both Remotes

```bash
# Fetch all branches from both remotes
git fetch origin
git fetch upstream

# View the state of branches
git log --oneline --graph origin/master upstream/master -20
```

### Step 3: Create a Backup Branch (Recommended)

```bash
# Checkout your master branch
git checkout master

# Create a backup of your current master
git branch backup-before-rebase

# Verify backup exists
git branch -a | grep backup
```

### Step 4: Identify Your Fork's Unique Commits

```bash
# List commits that are in your fork but not in upstream
git log --oneline upstream/master..origin/master

# This shows the 27 commits you've added to your fork
# Save this list for reference
git log --oneline upstream/master..origin/master > fork-commits.txt
```

### Step 5: Rebase Your Master on Upstream Master

```bash
# Ensure you're on your master branch
git checkout master

# Rebase your master onto upstream/master
# This replays your 27 commits on top of the latest upstream
git rebase upstream/master
```

### Step 6: Resolve Conflicts (If Any)

During the rebase, you may encounter conflicts. For each conflict:

```bash
# Check which files have conflicts
git status

# Open and resolve conflicts in each file
# Look for conflict markers: <<<<<<<, =======, >>>>>>>
# Edit files to keep the appropriate changes

# After resolving conflicts in a file, stage it
git add <resolved-file>

# Continue the rebase
git rebase --continue

# If you need to skip a commit (rare)
# git rebase --skip

# If you need to abort and start over
# git rebase --abort
```

### Step 7: Verify the Rebase

```bash
# Check that your commits are now on top of upstream
git log --oneline -30

# Verify no commits were lost - your fork commits should appear
git log --oneline | head -27

# Compare with your backup to ensure all changes preserved
git diff backup-before-rebase
# Should show the changes from upstream, not lost changes from your fork
```

### Step 8: Force Push to Update Your Fork

```bash
# Force push is required because history has been rewritten
git push --force-with-lease origin master

# --force-with-lease is safer than --force as it prevents
# overwriting work if someone else pushed to your fork
```

### Step 9: Clean Up

```bash
# Delete the backup branch (optional, after confirming everything is correct)
git branch -d backup-before-rebase

# Delete the commits list file
rm fork-commits.txt
```

## Alternative: Interactive Rebase (Advanced)

If you want more control over which commits to keep, squash, or edit:

```bash
# Interactive rebase
git rebase -i upstream/master

# In the editor that opens:
# - 'pick' = keep the commit as-is
# - 'squash' = combine with previous commit
# - 'edit' = pause to amend the commit
# - 'drop' = remove the commit
```

## Handling Specific Scenarios

### Scenario A: Conflict in a Single File

```bash
# During rebase, if conflict occurs in file.cs:
git status  # Shows conflicted files

# Edit the file to resolve conflicts
# Remove conflict markers and keep correct code
code path/to/file.cs

git add path/to/file.cs
git rebase --continue
```

### Scenario B: Multiple Conflicts

```bash
# Resolve each file one by one
git diff --name-only --diff-filter=U  # List unmerged files

# For each file:
git checkout --theirs path/to/file  # Use upstream version
# OR
git checkout --ours path/to/file    # Use your version
# OR manually edit to merge both

git add .
git rebase --continue
```

### Scenario C: Need to Abort and Restart

```bash
# If something goes wrong
git rebase --abort

# Verify you're back to original state
git log --oneline -10
```

## Post-Rebase Verification Checklist

- [ ] All 27 fork commits are present in history
- [ ] Master is up to date with upstream (20 commits synced)
- [ ] Build succeeds: `dotnet build`
- [ ] Tests pass: `dotnet test`
- [ ] No unintended file changes

## Notes

1. **Force Push Warning**: Force pushing rewrites history. Coordinate with any collaborators before doing this.

2. **CI/CD**: After force pushing, CI/CD pipelines will run on the new history.

3. **Open PRs**: Any open pull requests may need to be rebased after this operation.

4. **Backup**: Always keep the backup branch until you've verified everything is correct.

## Summary of Key Commands

```bash
# Quick reference (run from your local clone)
git remote add upstream https://github.com/OpenRakis/Spice86.git
git fetch upstream
git checkout master
git branch backup-before-rebase
git rebase upstream/master
# resolve conflicts if any
git push --force-with-lease origin master
```
