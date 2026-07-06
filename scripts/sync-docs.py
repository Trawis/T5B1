#!/usr/bin/env python3
"""Copy a repo-seed documentation profile into a target repository."""

from __future__ import annotations

import argparse
import json
import re
import shutil
import sys
from dataclasses import dataclass
from pathlib import Path, PurePosixPath

SCRIPT_VERSION = "3.2.3"
MANIFEST_FILE = "manifest.json"
FILES_DIRECTORY = "files"
TEMPLATE_METADATA_START = "repo-seed-template:start"
TEMPLATE_METADATA_END = "repo-seed-template:end"
VALID_TYPES = {"managed", "template"}
VALID_SCAFFOLD_GROUPS = {"project", "github", "editorconfig"}
SEMVER_PATTERN = re.compile(r"^\d+\.\d+\.\d+$")


@dataclass(frozen=True)
class Asset:
    path: str
    asset_type: str
    profiles: tuple[str, ...]
    scaffold_group: str | None = None
    scaffold_target: str | None = None


@dataclass(frozen=True)
class PackManifest:
    schema_version: int
    pack_version: str
    default_profile: str
    profiles: tuple[str, ...]
    package_files: tuple[str, ...]
    assets: tuple[Asset, ...]


@dataclass(frozen=True)
class SyncAction:
    action: str
    path: str
    detail: str


def relative_path(value: str, context: str) -> Path:
    if not value or "\\" in value:
        raise ValueError(f"{context} must be a non-empty POSIX relative path")

    pure_path = PurePosixPath(value)
    if pure_path.is_absolute() or pure_path == PurePosixPath(".") or ".." in pure_path.parts:
        raise ValueError(f"Unsafe path in {context}: {value}")
    if ":" in pure_path.parts[0]:
        raise ValueError(f"Unsafe path in {context}: {value}")
    if pure_path.as_posix() != value:
        raise ValueError(f"{context} must use a canonical POSIX path: {value}")
    return Path(*pure_path.parts)


def safe_child(root: Path, value: str, context: str) -> Path:
    root_resolved = root.resolve()
    child = root / relative_path(value, context)
    resolved = child.resolve()
    try:
        resolved.relative_to(root_resolved)
    except ValueError as ex:
        raise ValueError(f"{context} resolves outside its root: {value}") from ex
    return child


def require_string(data: dict[str, object], key: str, context: str) -> str:
    value = data.get(key)
    if not isinstance(value, str) or not value:
        raise ValueError(f"{context}.{key} must be a non-empty string")
    return value


def require_string_list(data: dict[str, object], key: str, context: str) -> tuple[str, ...]:
    value = data.get(key)
    if not isinstance(value, list) or not value or not all(isinstance(item, str) and item for item in value):
        raise ValueError(f"{context}.{key} must be a non-empty string array")
    if len(value) != len(set(value)):
        raise ValueError(f"{context}.{key} contains duplicates")
    return tuple(value)


def optional_string_list(data: dict[str, object], key: str, context: str) -> tuple[str, ...]:
    value = data.get(key, [])
    if not isinstance(value, list) or not all(isinstance(item, str) and item for item in value):
        raise ValueError(f"{context}.{key} must be a string array")
    if len(value) != len(set(value)):
        raise ValueError(f"{context}.{key} contains duplicates")
    return tuple(value)


def template_body(source: Path) -> str:
    lines = source.read_text(encoding="utf-8").splitlines(keepends=True)
    starts = [index for index, line in enumerate(lines) if TEMPLATE_METADATA_START in line]
    ends = [index for index, line in enumerate(lines) if TEMPLATE_METADATA_END in line]
    if len(starts) != 1 or len(ends) != 1 or starts[0] >= ends[0]:
        raise ValueError(f"Template metadata markers are missing or invalid: {source}")

    body = "".join(lines[: starts[0]] + lines[ends[0] + 1 :]).strip()
    if not body:
        raise ValueError(f"Template body is empty: {source}")
    return f"{body}\n"


def load_manifest(source_root: Path, validate_sources: bool = True) -> PackManifest:
    manifest_path = source_root / MANIFEST_FILE
    files_root = source_root / FILES_DIRECTORY
    if manifest_path.is_symlink():
        raise ValueError("Pack manifest cannot be a symbolic link")
    if validate_sources and files_root.is_symlink():
        raise ValueError("Pack files directory cannot be a symbolic link")
    try:
        raw = json.loads(manifest_path.read_text(encoding="utf-8"))
    except FileNotFoundError as ex:
        raise ValueError(f"Pack manifest does not exist: {manifest_path}") from ex
    except json.JSONDecodeError as ex:
        raise ValueError(f"Pack manifest is not valid JSON: {ex}") from ex

    if not isinstance(raw, dict):
        raise ValueError("Pack manifest root must be an object")
    if raw.get("schema_version") != 1:
        raise ValueError(f"Unsupported manifest schema_version: {raw.get('schema_version')}")

    pack_version = require_string(raw, "pack_version", "manifest")
    if not SEMVER_PATTERN.fullmatch(pack_version):
        raise ValueError("manifest.pack_version must use MAJOR.MINOR.PATCH")

    profiles = require_string_list(raw, "profiles", "manifest")
    default_profile = require_string(raw, "default_profile", "manifest")
    if default_profile not in profiles:
        raise ValueError("manifest.default_profile must be listed in manifest.profiles")

    package_files = optional_string_list(raw, "package_files", "manifest")
    for index, package_file in enumerate(package_files):
        context = f"manifest.package_files[{index}]"
        package_path = relative_path(package_file, context)
        if package_file == MANIFEST_FILE or package_path.parts[0] == FILES_DIRECTORY:
            raise ValueError(f"{context} must not replace manifest.json or use files/")
        source = safe_child(source_root, package_file, context)
        if validate_sources:
            if source.is_symlink():
                raise ValueError(f"Package file cannot be a symbolic link: {package_file}")
            if not source.is_file():
                raise ValueError(f"Package file does not exist: {package_file}")

    raw_assets = raw.get("assets")
    if not isinstance(raw_assets, list) or not raw_assets:
        raise ValueError("manifest.assets must be a non-empty array")

    assets: list[Asset] = []
    paths: set[str] = set()
    scaffold_targets: set[str] = set()
    profile_set = set(profiles)
    for index, raw_asset in enumerate(raw_assets):
        context = f"manifest.assets[{index}]"
        if not isinstance(raw_asset, dict):
            raise ValueError(f"{context} must be an object")

        path = require_string(raw_asset, "path", context)
        asset_type = require_string(raw_asset, "type", context)
        asset_profiles = require_string_list(raw_asset, "profiles", context)
        scaffold_group = raw_asset.get("scaffold_group")
        scaffold_target = raw_asset.get("scaffold_target")

        relative_path(path, f"{context}.path")
        if path in paths:
            raise ValueError(f"Duplicate asset path: {path}")
        if asset_type not in VALID_TYPES:
            raise ValueError(f"{context}.type must be managed or template")
        if not set(asset_profiles).issubset(profile_set):
            raise ValueError(f"{context}.profiles contains an unknown profile")

        if asset_type == "template":
            if scaffold_group not in VALID_SCAFFOLD_GROUPS:
                raise ValueError(
                    f"{context}.scaffold_group must be one of: {', '.join(sorted(VALID_SCAFFOLD_GROUPS))}"
                )
            if not isinstance(scaffold_target, str) or not scaffold_target:
                raise ValueError(f"{context}.scaffold_target must be a non-empty string")
            relative_path(scaffold_target, f"{context}.scaffold_target")
            if scaffold_target in scaffold_targets:
                raise ValueError(f"Duplicate scaffold target: {scaffold_target}")
            if not path.startswith("docs/templates/"):
                raise ValueError(f"{context}.path must be under docs/templates")
            scaffold_targets.add(scaffold_target)
        elif scaffold_group is not None or scaffold_target is not None:
            raise ValueError(f"{context} managed assets cannot define scaffold fields")

        source = safe_child(files_root, path, f"{context}.path")
        if validate_sources:
            if source.is_symlink():
                raise ValueError(f"Asset source cannot be a symbolic link: {path}")
            if not source.is_file():
                raise ValueError(f"Asset source does not exist: {path}")
            if asset_type == "template":
                template_body(source)

        paths.add(path)
        assets.append(
            Asset(
                path=path,
                asset_type=asset_type,
                profiles=asset_profiles,
                scaffold_group=scaffold_group if isinstance(scaffold_group, str) else None,
                scaffold_target=scaffold_target if isinstance(scaffold_target, str) else None,
            )
        )

    collisions = paths.intersection(scaffold_targets)
    if collisions:
        raise ValueError(f"Scaffold targets collide with managed paths: {', '.join(sorted(collisions))}")

    return PackManifest(
        schema_version=1,
        pack_version=pack_version,
        default_profile=default_profile,
        profiles=profiles,
        package_files=package_files,
        assets=tuple(assets),
    )


def assets_for_profile(manifest: PackManifest, profile: str) -> tuple[Asset, ...]:
    if profile not in manifest.profiles:
        raise ValueError(f"Unknown profile '{profile}'. Choices: {', '.join(manifest.profiles)}")
    return tuple(asset for asset in manifest.assets if profile in asset.profiles)


def discover_source_root() -> Path | None:
    script_candidate = Path(__file__).resolve().parents[2]
    candidates = (script_candidate, Path.cwd().resolve(), Path.cwd().resolve() / "pack")
    for candidate in candidates:
        if (candidate / MANIFEST_FILE).is_file() and (candidate / FILES_DIRECTORY).is_dir():
            return candidate
    return None


def validate_parent_directory(target: Path, target_root: Path, context: str) -> None:
    parent = target.parent
    root = target_root.resolve()
    while not parent.exists() and parent != root:
        parent = parent.parent
    if not parent.is_dir():
        raise ValueError(f"{context} parent is not a directory: {parent}")


def validate_managed_destination(target_root: Path, asset: Asset) -> None:
    target = safe_child(target_root, asset.path, "asset target")
    if target.is_symlink():
        raise ValueError(f"Managed target cannot be a symbolic link: {asset.path}")
    if target.exists() and not target.is_file():
        raise ValueError(f"Managed target is not a file: {asset.path}")
    validate_parent_directory(target, target_root, f"Managed target '{asset.path}'")


def validate_scaffold_destination(target_root: Path, asset: Asset) -> None:
    if asset.scaffold_target is None:
        raise ValueError(f"Template has no scaffold target: {asset.path}")
    target = safe_child(target_root, asset.scaffold_target, "scaffold target")
    if target.exists() or target.is_symlink():
        return
    validate_parent_directory(target, target_root, f"Scaffold target '{asset.scaffold_target}'")


def copy_asset(source_root: Path, target_root: Path, asset: Asset, dry_run: bool) -> SyncAction:
    source = safe_child(source_root / FILES_DIRECTORY, asset.path, "asset path")
    target = safe_child(target_root, asset.path, "asset target")
    if dry_run:
        return SyncAction("copy", asset.path, "would overwrite managed copy")

    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, target)
    return SyncAction("copy", asset.path, "overwrote managed copy")


def add_source_marker(body: str, template_path: str) -> str:
    marker = f"<!-- Scaffolded from: {template_path} -->"
    stripped = body.strip()

    if stripped.startswith("---\n"):
        closing = stripped.find("\n---", 4)
        if closing >= 0:
            closing += len("\n---")
            return f"{stripped[:closing]}\n\n{marker}\n\n{stripped[closing:].lstrip()}\n"

    first_line, separator, remainder = stripped.partition("\n")
    if separator and first_line.startswith("#"):
        return f"{first_line}\n\n{marker}\n\n{remainder.lstrip()}\n"
    return f"{marker}\n\n{stripped}\n"


def scaffold_asset(source_root: Path, target_root: Path, asset: Asset, dry_run: bool) -> SyncAction:
    if asset.scaffold_target is None:
        raise ValueError(f"Template has no scaffold target: {asset.path}")

    target = safe_child(target_root, asset.scaffold_target, "scaffold target")
    if target.exists() or target.is_symlink():
        return SyncAction("skip", asset.scaffold_target, "project-owned destination already exists")

    source = safe_child(source_root / FILES_DIRECTORY, asset.path, "template path")
    body = template_body(source)
    if Path(asset.scaffold_target).suffix.lower() == ".md":
        body = add_source_marker(body, asset.path)

    if dry_run:
        return SyncAction("scaffold", asset.scaffold_target, f"would create from {asset.path}")

    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(body, encoding="utf-8", newline="\n")
    return SyncAction("scaffold", asset.scaffold_target, f"created from {asset.path}")


def synchronize(
    source_root: Path,
    target_root: Path,
    profile: str,
    scaffold_project_files: bool = False,
    scaffold_github_templates: bool = False,
    scaffold_editorconfig: bool = False,
    dry_run: bool = False,
) -> list[SyncAction]:
    source_root = source_root.expanduser().resolve()
    target_root = target_root.expanduser().resolve()
    if not target_root.is_dir():
        raise ValueError(f"Target repository does not exist or is not a directory: {target_root}")

    manifest = load_manifest(source_root)
    selected = assets_for_profile(manifest, profile)

    requested_groups: set[str] = set()
    if scaffold_project_files:
        requested_groups.add("project")
    if scaffold_github_templates:
        requested_groups.add("github")
    if scaffold_editorconfig:
        requested_groups.add("editorconfig")

    scaffold_assets = tuple(
        asset
        for asset in selected
        if asset.asset_type == "template" and asset.scaffold_group in requested_groups
    )

    for asset in selected:
        validate_managed_destination(target_root, asset)
    for asset in scaffold_assets:
        validate_scaffold_destination(target_root, asset)

    actions = [copy_asset(source_root, target_root, asset, dry_run) for asset in selected]
    actions.extend(scaffold_asset(source_root, target_root, asset, dry_run) for asset in scaffold_assets)
    return actions


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Overwrite managed documentation-pack files and optionally scaffold missing project files."
    )
    parser.add_argument(
        "--source",
        help="Pack directory containing manifest.json and files/. Auto-detected when run from an extracted pack.",
    )
    parser.add_argument("--target", default=".", help="Target repository. Defaults to the current directory.")
    parser.add_argument("--profile", help="Profile name. Defaults to the manifest default.")
    parser.add_argument(
        "--scaffold-project-files",
        action="store_true",
        help="Create missing project-owned documents and .gitignore.",
    )
    parser.add_argument(
        "--scaffold-github-templates",
        action="store_true",
        help="Create missing bug, feature, and chooser files.",
    )
    parser.add_argument(
        "--scaffold-editorconfig",
        action="store_true",
        help="Create .editorconfig only when it is missing.",
    )
    parser.add_argument("--dry-run", action="store_true", help="Validate and show operations without writing files.")
    parser.add_argument("--version", action="version", version=f"%(prog)s {SCRIPT_VERSION}")
    return parser


def main(argv: list[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    try:
        if args.source:
            source_root = Path(args.source).expanduser().resolve()
        else:
            source_root = discover_source_root()
            if source_root is None:
                raise ValueError("Pack source was not found; pass --source with the directory containing manifest.json")

        target_root = Path(args.target).expanduser().resolve()
        if not target_root.is_dir():
            raise ValueError(f"Target repository does not exist or is not a directory: {target_root}")

        manifest = load_manifest(source_root)
        profile = args.profile or manifest.default_profile
        actions = synchronize(
            source_root=source_root,
            target_root=target_root,
            profile=profile,
            scaffold_project_files=args.scaffold_project_files,
            scaffold_github_templates=args.scaffold_github_templates,
            scaffold_editorconfig=args.scaffold_editorconfig,
            dry_run=args.dry_run,
        )
    except (OSError, ValueError) as ex:
        print(f"error: {ex}", file=sys.stderr)
        return 2

    print(f"source         {source_root}")
    print(f"target         {target_root}")
    print(f"profile        {profile}")
    for action in actions:
        print(f"{action.action:<14} {action.path} ({action.detail})")

    copies = sum(action.action == "copy" for action in actions)
    scaffolds = sum(action.action == "scaffold" for action in actions)
    skips = sum(action.action == "skip" for action in actions)
    mode = "Dry run" if args.dry_run else "Sync"
    print(f"{mode} complete. Managed copies: {copies}. Scaffolds: {scaffolds}. Preserved: {skips}.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
