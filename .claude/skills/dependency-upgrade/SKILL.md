---
name: dependency-upgrade
description: Upgrades project dependencies, frameworks, runtimes, or tooling with focused compatibility analysis and validation. Use when changing dependency versions, adopting a newer framework/runtime version, resolving an upgrade migration, or removing compatibility made obsolete by a new minimum version.
---

# Dependency Upgrade

Follow the applicable `AGENTS.md` files and `.agents/project.md` when it exists.
Load any relevant language conventions the repository provides. If the upgrade
changes build, release, or hosted automation, also read
`.agents/guidelines/ci-cd.md`.

## Workflow

1. Identify the current version, requested or appropriate target version,
   package manager, lockfile, runtime/toolchain constraints, and whether the
   dependency is direct or transitive.
2. Inspect the repository for APIs, configuration, workarounds, version checks,
   and compatibility branches tied to the old version.
3. Consult authoritative release notes, migration guides, or current
   documentation when available. Do not rely on model memory for current
   package versions or recent breaking changes.
4. Upgrade only the requested dependency set. Do not bundle unrelated
   dependency updates merely because newer versions exist.
5. Update manifests and lockfiles through the repository's normal package
   manager or tooling.
6. Adapt source, configuration, tests, and build scripts for documented breaking
   changes.
7. Remove obsolete workarounds or compatibility code only when the repository's
   supported-version policy no longer requires them.
8. Review resulting transitive, peer, runtime, and toolchain changes for
   accidental broad upgrades.
9. Run the relevant focused tests, build, lint/type/format checks, and practical
   startup or smoke validation.

## Guardrails

- Do not guess a latest version when external/current verification matters.
- Do not hand-edit generated lockfile sections when the package manager can
  update them correctly.
- Do not retain both old and new code paths without a supported compatibility
  requirement.
- Do not weaken tests to accommodate changed behavior without verifying that the
  upstream contract changed.

## Completion

Report versions changed, migration work performed, compatibility code removed
or retained, and validation performed.
