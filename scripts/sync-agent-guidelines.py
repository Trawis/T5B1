#!/usr/bin/env python3
"""
Sync agent guideline files from repo-seed into another repository.

This script is intended to run locally from a clean target repository.
It prepares a strict Git Flow feature branch, copies managed guideline files,
and stops before commit, push, or PR creation.

Examples:
    python /path/to/repo-seed/scripts/sync-agent-guidelines.py --target . --dry-run
    python /path/to/repo-seed/scripts/sync-agent-guidelines.py --source /path/to/repo-seed --target .
"""

from __future__ import annotations

import argparse
import hashlib
import json
import shutil
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path

SCRIPT_NAME = "sync-agent-guidelines"
SCRIPT_VERSION = "1.4.0"
PACK_VERSION = "1.24.0"
MANIFEST_FILE = ".agent-guidelines-manifest.json"
CONFLICT_DIR = ".agent-guidelines-conflicts"

CORE_FILES = [
    "AGENTS.md",
    "CLAUDE.md",
    ".agent-guidelines-version",
    ".editorconfig",
    ".github/pull_request_template.md",
    "docs/coding-conventions-csharp.md",
    "docs/coding-conventions-scripts.md",
    "docs/coding-conventions-python.md",
    "docs/coding-conventions-shell.md",
    "docs/fsd-template.md",
    "docs/tsd-template.md",
    "docs/gdd-template.md",
    "scripts/sync-agent-guidelines.py",
]

PROJECT_DOC_TEMPLATES = [
    "README.md",
    "CHANGELOG.md",
    "FEATURES.md",
]


@dataclass(frozen=True)
class SyncAction:
    relative_path: str
    action: str
    reason: str


def run_git(target_root: Path, *args: str, check: bool = True) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        ["git", *args],
        cwd=target_root,
        check=check,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )


def is_git_repo(target_root: Path) -> bool:
    result = run_git(target_root, "rev-parse", "--is-inside-work-tree", check=False)
    return result.returncode == 0 and result.stdout.strip() == "true"


def git_status_porcelain(target_root: Path) -> str:
    return run_git(target_root, "status", "--porcelain", check=True).stdout.strip()


def fetch_remote_state(target_root: Path) -> str | None:
    result = run_git(target_root, "fetch", "--all", "--prune", check=False)
    if result.returncode == 0:
        return None
    message = result.stderr.strip() or result.stdout.strip() or "git fetch failed"
    return message


def branch_exists(target_root: Path, branch_name: str) -> bool:
    result = run_git(target_root, "show-ref", "--verify", f"refs/heads/{branch_name}", check=False)
    return result.returncode == 0


def remote_branch_exists(target_root: Path, branch_name: str) -> bool:
    result = run_git(target_root, "show-ref", "--verify", f"refs/remotes/origin/{branch_name}", check=False)
    return result.returncode == 0


def current_branch(target_root: Path) -> str:
    result = run_git(target_root, "branch", "--show-current", check=True)
    return result.stdout.strip()


def switch_branch(target_root: Path, branch_name: str) -> None:
    run_git(target_root, "switch", branch_name, check=True)


def create_branch(target_root: Path, branch_name: str, start_point: str) -> None:
    run_git(target_root, "switch", "-c", branch_name, start_point, check=True)


def ensure_base_branch(target_root: Path, base_branch: str) -> list[str]:
    messages: list[str] = []

    if branch_exists(target_root, base_branch):
        switch_branch(target_root, base_branch)
        messages.append(f"checked out existing base branch {base_branch}")
        return messages

    if remote_branch_exists(target_root, base_branch):
        create_branch(target_root, base_branch, f"origin/{base_branch}")
        messages.append(f"created local {base_branch} from origin/{base_branch}")
        return messages

    if base_branch == "develop":
        if branch_exists(target_root, "main"):
            create_branch(target_root, "develop", "main")
            messages.append("created develop from main")
            return messages

        if remote_branch_exists(target_root, "main"):
            create_branch(target_root, "main", "origin/main")
            create_branch(target_root, "develop", "main")
            messages.append("created main from origin/main and develop from main")
            return messages

    fallback = "HEAD"
    create_branch(target_root, base_branch, fallback)
    messages.append(f"created {base_branch} from current HEAD because no main/origin branch was found")
    return messages


def prepare_branch(target_root: Path, branch_name: str, base_branch: str, dry_run: bool, no_branch: bool, skip_fetch: bool) -> list[str]:
    if no_branch:
        return ["branch creation skipped because --no-branch was used"]

    if not is_git_repo(target_root):
        raise RuntimeError("Target is not a Git repository. Initialize Git or use --no-branch explicitly.")

    status = git_status_porcelain(target_root)
    if status:
        raise RuntimeError(
            "Target working tree has uncommitted changes. Commit/stash them before syncing.\n"
            f"Git status:\n{status}"
        )

    if dry_run:
        if skip_fetch:
            return [f"dry-run: would create or reuse branch {branch_name} from {base_branch}"]
        return [f"dry-run: would fetch/prune remotes, then create or reuse branch {branch_name} from {base_branch}"]

    messages: list[str] = []
    if not skip_fetch:
        fetch_error = fetch_remote_state(target_root)
        if fetch_error:
            messages.append(f"remote fetch/prune skipped or failed: {fetch_error}")
        else:
            messages.append("fetched/pruned remotes before branch preparation")

    messages.extend(ensure_base_branch(target_root, base_branch))

    if branch_exists(target_root, branch_name):
        switch_branch(target_root, branch_name)
        messages.append(f"checked out existing task branch {branch_name}")
    else:
        create_branch(target_root, branch_name, base_branch)
        messages.append(f"created task branch {branch_name} from {base_branch}")

    return messages


def default_source_root() -> Path:
    script_path = Path(__file__).resolve()
    if script_path.parent.name == "scripts":
        return script_path.parent.parent
    return Path.cwd()


def resolve_root(value: str | None, fallback: Path) -> Path:
    if value:
        return Path(value).expanduser().resolve()
    return fallback.expanduser().resolve()


def validate_relative_path(relative_path: str) -> Path:
    path = Path(relative_path)
    if path.is_absolute() or ".." in path.parts:
        raise ValueError(f"Unsafe relative path: {relative_path}")
    return path


def file_hash(path: Path) -> str:
    hasher = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            hasher.update(chunk)
    return hasher.hexdigest()


def read_manifest(target_root: Path) -> dict[str, str]:
    manifest_path = target_root / MANIFEST_FILE
    if not manifest_path.exists():
        return {}

    try:
        data = json.loads(manifest_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError:
        return {}

    files = data.get("files", {})
    if not isinstance(files, dict):
        return {}

    return {str(key): str(value) for key, value in files.items()}


def write_manifest(target_root: Path, hashes: dict[str, str], dry_run: bool) -> SyncAction:
    manifest_path = target_root / MANIFEST_FILE
    payload = {
        "pack_version": PACK_VERSION,
        "sync_script": f"{SCRIPT_NAME}.py",
        "sync_script_version": SCRIPT_VERSION,
        "files": dict(sorted(hashes.items())),
    }

    content = json.dumps(payload, indent=2, sort_keys=True) + "\n"

    if manifest_path.exists() and manifest_path.read_text(encoding="utf-8") == content:
        return SyncAction(MANIFEST_FILE, "unchanged", "manifest already current")

    if dry_run:
        return SyncAction(MANIFEST_FILE, "dry-run", "would update managed file hash manifest")

    manifest_path.write_text(content, encoding="utf-8")
    return SyncAction(MANIFEST_FILE, "copied", "managed file hash manifest written")


def conflict_filename(relative_path: str) -> str:
    safe_path = relative_path.replace("/", "__").replace("\\", "__")
    return f"{SCRIPT_NAME}_{SCRIPT_VERSION}_incoming_{safe_path}"


def write_conflict_file(source: Path, target_root: Path, relative_path: str, dry_run: bool) -> SyncAction:
    conflict_path = target_root / CONFLICT_DIR / conflict_filename(relative_path)

    if dry_run:
        return SyncAction(relative_path, "conflict", f"would keep local file and write incoming version to {conflict_path.relative_to(target_root)}")

    conflict_path.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, conflict_path)
    return SyncAction(relative_path, "conflict", f"kept local file; incoming version written to {conflict_path.relative_to(target_root)}")


def sync_managed_file(
    source_root: Path,
    target_root: Path,
    relative_path: str,
    previous_hashes: dict[str, str],
    new_hashes: dict[str, str],
    dry_run: bool,
) -> SyncAction:
    rel = validate_relative_path(relative_path)
    source = source_root / rel
    target = target_root / rel

    if not source.exists():
        return SyncAction(relative_path, "missing-source", "source file does not exist")

    source_hash = file_hash(source)

    if not target.exists():
        if not dry_run:
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, target)
        new_hashes[relative_path] = source_hash
        return SyncAction(relative_path, "dry-run" if dry_run else "copied", "would create" if dry_run else "created")

    target_hash = file_hash(target)

    if target_hash == source_hash:
        new_hashes[relative_path] = source_hash
        return SyncAction(relative_path, "unchanged", "target already matches source")

    previous_hash = previous_hashes.get(relative_path)
    if previous_hash is None:
        return write_conflict_file(source, target_root, relative_path, dry_run)

    if target_hash != previous_hash:
        return write_conflict_file(source, target_root, relative_path, dry_run)

    if not dry_run:
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, target)
    new_hashes[relative_path] = source_hash
    return SyncAction(relative_path, "dry-run" if dry_run else "copied", "would update" if dry_run else "updated")


def sync_project_doc_template(source_root: Path, target_root: Path, relative_path: str, force: bool, dry_run: bool) -> SyncAction:
    rel = validate_relative_path(relative_path)
    source = source_root / rel
    target = target_root / rel

    if not source.exists():
        return SyncAction(relative_path, "missing-source", "source file does not exist")

    if target.exists() and not force:
        return SyncAction(relative_path, "skipped", "project document already exists; use --force-project-docs to overwrite")

    if target.exists() and file_hash(source) == file_hash(target):
        return SyncAction(relative_path, "unchanged", "target already matches source")

    if dry_run:
        reason = "would overwrite project doc" if target.exists() else "would create project doc"
        return SyncAction(relative_path, "dry-run", reason)

    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, target)
    return SyncAction(relative_path, "copied", "project document copied")


def version_branch_name(pack_version: str) -> str:
    return f"feature/sync-agent-guidelines-{pack_version.replace('.', '-')}"


def main() -> int:
    parser = argparse.ArgumentParser(description="Sync agent guideline files from repo-seed into another repository.")
    parser.add_argument("--source", help="Path to repo-seed / guideline source root. Defaults to the parent of this script's scripts directory.")
    parser.add_argument("--target", default=".", help="Target repository root. Defaults to the current directory.")
    parser.add_argument("--dry-run", action="store_true", help="Show what would change without switching branches or copying files.")
    parser.add_argument("--branch-name", default=version_branch_name(PACK_VERSION), help="Task branch to create or reuse. Defaults to feature/sync-agent-guidelines-<version>.")
    parser.add_argument("--base-branch", default="develop", help="Base branch for the sync branch. Defaults to develop.")
    parser.add_argument("--no-branch", action="store_true", help="Do not create/switch branches. Use only when the caller has already prepared the correct branch.")
    parser.add_argument("--skip-fetch", action="store_true", help="Do not run git fetch --all --prune before creating/reusing the sync branch.")
    parser.add_argument("--include-project-docs", action="store_true", help="Also copy README.md, CHANGELOG.md, and FEATURES.md only when missing.")
    parser.add_argument("--force-project-docs", action="store_true", help="Overwrite README.md, CHANGELOG.md, and FEATURES.md when --include-project-docs is used.")
    parser.add_argument("--skip-editorconfig", action="store_true", help="Do not sync .editorconfig.")
    parser.add_argument("--exclude", action="append", default=[], help="Relative file path to exclude from syncing. May be passed multiple times.")
    args = parser.parse_args()

    source_root = resolve_root(args.source, default_source_root())
    target_root = resolve_root(args.target, Path.cwd())

    if source_root == target_root:
        print("Source and target are the same directory; nothing to sync.", file=sys.stderr)
        return 2

    if not source_root.exists():
        print(f"Source root does not exist: {source_root}", file=sys.stderr)
        return 2

    if not target_root.exists():
        print(f"Target root does not exist: {target_root}", file=sys.stderr)
        return 2

    try:
        branch_messages = prepare_branch(target_root, args.branch_name, args.base_branch, args.dry_run, args.no_branch, args.skip_fetch)
    except (RuntimeError, subprocess.CalledProcessError) as ex:
        print(f"Branch preparation failed: {ex}", file=sys.stderr)
        return 2

    for message in branch_messages:
        print(f"branch        {message}")

    excluded = set(args.exclude)
    if args.skip_editorconfig:
        excluded.add(".editorconfig")

    files = [path for path in CORE_FILES if path not in excluded]
    previous_hashes = read_manifest(target_root)
    new_hashes = dict(previous_hashes)

    actions: list[SyncAction] = []
    for relative_path in files:
        actions.append(sync_managed_file(source_root, target_root, relative_path, previous_hashes, new_hashes, args.dry_run))

    if args.include_project_docs:
        for relative_path in PROJECT_DOC_TEMPLATES:
            if relative_path not in excluded:
                actions.append(sync_project_doc_template(source_root, target_root, relative_path, args.force_project_docs, args.dry_run))

    actions.append(write_manifest(target_root, new_hashes, args.dry_run))

    changed = 0
    conflicts = 0
    for item in actions:
        print(f"{item.action:14} {item.relative_path} - {item.reason}")
        if item.action in {"copied", "dry-run"}:
            changed += 1
        if item.action == "conflict":
            conflicts += 1

    if args.dry_run:
        print(f"\nDry run complete. Pending changes: {changed}. Conflicts: {conflicts}.")
    else:
        print(f"\nSync complete. Files changed or created: {changed}. Conflicts: {conflicts}.")
        print(f"Current/intended branch: {args.branch_name}")
        print("Next steps: review the diff, resolve conflicts if any, run relevant checks, commit, push, and open a PR to develop.")

    if conflicts:
        print("Conflict files were written under .agent-guidelines-conflicts/ for manual review." if not args.dry_run else "Conflicts would be written under .agent-guidelines-conflicts/.")
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
