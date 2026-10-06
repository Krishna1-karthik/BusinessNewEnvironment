# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade all projects in the solution to net10.0
**Scope**: Projects currently targeting .NET 8 (solution contains a small-to-medium set of projects). Final plan will rely on assessment.md for exact project list and package inventory.

## Tasks

### 01-update-projects-to-net10: Update project TargetFrameworks to net10.0

Update each project file's TargetFramework (or TargetFrameworks) to `net10.0`. Where projects currently multi-target, add `net10.0` alongside existing targets if needed and prefer single-targeting `net10.0` when safe.

Affected items: All projects reported by assessment.md as targeting net8.0

Done when: All project files reference `net10.0` and solution builds (or builds with known package-related errors documented).

---

### 02-update-nuget-packages: Upgrade NuGet packages to net10-compatible versions

For each project, review NuGet packages flagged by assessment.md as incompatible or recommended for upgrade. Update package references to the latest compatible stable versions that support net10.0. Favor non-breaking minor/patch upgrades when available; if a major upgrade is required, document breaking changes and add remediation steps to the task.md.

Done when: All upgraded packages restore successfully and the solution builds or remaining errors are only due to known code-breaking API changes documented in task progress.

---

### 03-compile-and-fix: Build solution and resolve compilation issues

Build the solution, identify compile-time errors introduced by TFMs or package updates, and apply minimal code fixes (API replacements, namespace changes, nullability fixes). Document each class of change in the task's progress-details.md.

Done when: Solution builds without errors and without new warnings introduced by the changes (we treat warnings as errors per workflow rules).

---

### 04-run-tests: Run unit and integration tests

Execute test projects. Fix failing tests that are legitimately broken by framework or package behavior changes. For flaky tests, document the flakiness and add a note in progress-details.md.

Done when: All tests pass or failing tests are documented with remediation tracked as follow-up tasks.

---

### 05-finalize-and-commit: Final verification and commit changes

Perform a full verification build, run tests, update scenario artifacts (progress-details.md), and commit changes per commit strategy (After Each Task). Push branch if user prefers.

Done when: All artifacts updated, commits created, and tasks.md shows all tasks completed.
