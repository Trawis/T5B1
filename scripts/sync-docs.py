#!/usr/bin/env python3
"""Copy a repo-seed documentation profile into a target repository."""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import shutil
import sys
from dataclasses import dataclass
from pathlib import Path, PurePosixPath

MANIFEST_FILE = "manifest.json"
FILES_DIRECTORY = "files"
DEFAULT_STATE_FILE = ".repo-seed-state.json"
TEMPLATE_METADATA_START = "repo-seed-template:start"
TEMPLATE_METADATA_END = "repo-seed-template:end"
VALID_TYPES = {"managed", "template"}
VALID_SCAFFOLD_GROUPS = {"project", "github", "editorconfig"}
PROJECT_OWNED_TREES = (".github/workflows",)
SEMVER_PATTERN = re.compile(r"^\d+\.\d+\.\d+$")
SHA256_PATTERN = re.compile(r"^[0-9a-f]{64}$")
SCAFFOLD_SOURCE_PATTERN = re.compile(
    r"^<!-- Scaffolded from: (?P<source>[^\r\n]+) -->$",
    re.MULTILINE,
)
SCAFFOLD_HASH_PATTERN = re.compile(
    r"^<!-- Scaffolded content SHA-256: (?P<hash>[0-9a-f]{64}) -->\r?\n?",
    re.MULTILINE,
)
LEGACY_PROVENANCE_PATTERN = re.compile(
    r'^<!-- repo-seed-template id="(?P<id>[^"]+)" sha256="(?P<hash>[0-9a-f]{64})" -->$',
    re.MULTILINE,
)


@dataclass(frozen=True)
class Asset:
    path: str
    asset_type: str
    profiles: tuple[str, ...]
    previous_hashes: tuple[str, ...] = ()
    scaffold_group: str | None = None
    scaffold_target: str | None = None


@dataclass(frozen=True)
class RetiredPathSet:
    through_version: str
    paths: tuple[str, ...]


@dataclass(frozen=True)
class RetiredAsset:
    path: str
    content_hashes: tuple[str, ...]


@dataclass(frozen=True)
class ScaffoldUpgrade:
    from_versions: tuple[str, ...]
    legacy_target: str
    template: str
    content_hashes: tuple[str, ...]


@dataclass(frozen=True)
class MigrationConfig:
    legacy_manifest: str
    legacy_version: str
    legacy_conflicts: str
    protected_paths: tuple[str, ...]
    retired_assets: tuple[RetiredAsset, ...]
    retired_path_sets: tuple[RetiredPathSet, ...]
    scaffold_upgrades: tuple[ScaffoldUpgrade, ...]


@dataclass(frozen=True)
class PackManifest:
    schema_version: int
    pack_version: str
    state_file: str
    profiles: tuple[str, ...]
    package_files: tuple[str, ...]
    migration: MigrationConfig | None
    assets: tuple[Asset, ...]


@dataclass(frozen=True)
class SyncAction:
    action: str
    path: str
    detail: str


@dataclass(frozen=True)
class LegacyState:
    pack_version: str | None
    hashes: dict[str, str]
    manifest_exists: bool


@dataclass(frozen=True)
class ManagedState:
    pack_version: str | None
    profile: str | None
    managed_files: dict[str, str]
    tombstones: dict[str, str]
    exists: bool


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


def is_project_owned_tree_path(value: str) -> bool:
    return any(
        value == tree or value.startswith(f"{tree}/")
        for tree in PROJECT_OWNED_TREES
    )


def reject_project_owned_tree_path(value: str, context: str) -> None:
    if is_project_owned_tree_path(value):
        raise ValueError(f"{context} cannot use project-owned tree: {value}")


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


def version_key(value: str, context: str) -> tuple[int, int, int]:
    if not SEMVER_PATTERN.fullmatch(value):
        raise ValueError(f"{context} must use MAJOR.MINOR.PATCH")
    major, minor, patch = value.split(".")
    return int(major), int(minor), int(patch)


def load_migration(value: object, assets: tuple[Asset, ...]) -> MigrationConfig | None:
    if value is None:
        return None
    if not isinstance(value, dict):
        raise ValueError("manifest.migration must be an object")

    legacy_manifest = require_string(value, "legacy_manifest", "manifest.migration")
    legacy_version = require_string(value, "legacy_version", "manifest.migration")
    legacy_conflicts = require_string(value, "legacy_conflicts", "manifest.migration")
    state_paths = {legacy_manifest, legacy_version, legacy_conflicts}
    if len(state_paths) != 3:
        raise ValueError("Legacy manifest, version, and conflict paths must be distinct")
    protected_paths = require_string_list(value, "protected_paths", "manifest.migration")
    relative_path(legacy_manifest, "manifest.migration.legacy_manifest")
    relative_path(legacy_version, "manifest.migration.legacy_version")
    relative_path(legacy_conflicts, "manifest.migration.legacy_conflicts")
    for index, path in enumerate(protected_paths):
        relative_path(path, f"manifest.migration.protected_paths[{index}]")

    raw_retired_assets = value.get("retired_assets")
    if not isinstance(raw_retired_assets, list):
        raise ValueError("manifest.migration.retired_assets must be an array")
    retired_assets: list[RetiredAsset] = []
    retired_asset_paths: set[str] = set()
    for index, raw_asset in enumerate(raw_retired_assets):
        context = f"manifest.migration.retired_assets[{index}]"
        if not isinstance(raw_asset, dict):
            raise ValueError(f"{context} must be an object")
        path = require_string(raw_asset, "path", context)
        content_hashes = require_string_list(raw_asset, "content_hashes", context)
        relative_path(path, f"{context}.path")
        reject_project_owned_tree_path(path, f"{context}.path")
        if path in retired_asset_paths:
            raise ValueError(f"Duplicate retired asset: {path}")
        if not all(SHA256_PATTERN.fullmatch(hash_value) for hash_value in content_hashes):
            raise ValueError(f"{context}.content_hashes must contain SHA-256 values")
        retired_asset_paths.add(path)
        retired_assets.append(RetiredAsset(path=path, content_hashes=content_hashes))

    raw_sets = value.get("retired_path_sets")
    if not isinstance(raw_sets, list) or not raw_sets:
        raise ValueError("manifest.migration.retired_path_sets must be a non-empty array")
    retired_sets: list[RetiredPathSet] = []
    retired_paths: set[str] = set()
    for index, raw_set in enumerate(raw_sets):
        context = f"manifest.migration.retired_path_sets[{index}]"
        if not isinstance(raw_set, dict):
            raise ValueError(f"{context} must be an object")
        through_version = require_string(raw_set, "through_version", context)
        version_key(through_version, f"{context}.through_version")
        paths = require_string_list(raw_set, "paths", context)
        for path_index, path in enumerate(paths):
            relative_path(path, f"{context}.paths[{path_index}]")
            reject_project_owned_tree_path(path, f"{context}.paths[{path_index}]")
            if path in retired_paths:
                raise ValueError(f"Duplicate retired path: {path}")
            retired_paths.add(path)
        retired_sets.append(RetiredPathSet(through_version=through_version, paths=paths))

    if retired_paths.intersection(protected_paths):
        raise ValueError("Protected paths cannot also be retired")
    current_paths = {asset.path for asset in assets}
    scaffold_targets = {
        asset.scaffold_target
        for asset in assets
        if asset.scaffold_target is not None
    }
    if current_paths.intersection(protected_paths):
        raise ValueError("Current managed paths cannot also be protected")
    if retired_paths.intersection(current_paths) or retired_asset_paths.intersection(current_paths):
        raise ValueError("Retired paths cannot collide with current managed paths")
    if (
        retired_asset_paths.intersection(protected_paths)
        or retired_asset_paths.intersection(retired_paths)
        or retired_asset_paths.intersection(scaffold_targets)
        or retired_paths.intersection(scaffold_targets)
    ):
        raise ValueError(
            "Retired paths cannot collide with protected paths or scaffold targets"
        )
    if legacy_manifest in retired_paths or legacy_conflicts in retired_paths:
        raise ValueError("Legacy manifest and conflict paths cannot also be retired")
    if (
        state_paths.intersection(current_paths)
        or state_paths.intersection(protected_paths)
        or state_paths.intersection(retired_asset_paths)
    ):
        raise ValueError("Legacy state paths cannot collide with current or protected paths")

    raw_upgrades = value.get("scaffold_upgrades")
    if not isinstance(raw_upgrades, list):
        raise ValueError("manifest.migration.scaffold_upgrades must be an array")
    asset_by_path = {asset.path: asset for asset in assets}
    upgrades: list[ScaffoldUpgrade] = []
    legacy_targets: set[str] = set()
    for index, raw_upgrade in enumerate(raw_upgrades):
        context = f"manifest.migration.scaffold_upgrades[{index}]"
        if not isinstance(raw_upgrade, dict):
            raise ValueError(f"{context} must be an object")
        from_versions = require_string_list(raw_upgrade, "from_versions", context)
        legacy_target = require_string(raw_upgrade, "legacy_target", context)
        template = require_string(raw_upgrade, "template", context)
        content_hashes = require_string_list(raw_upgrade, "content_hashes", context)
        for version_index, version in enumerate(from_versions):
            version_key(version, f"{context}.from_versions[{version_index}]")
        relative_path(legacy_target, f"{context}.legacy_target")
        relative_path(template, f"{context}.template")
        reject_project_owned_tree_path(legacy_target, f"{context}.legacy_target")
        if legacy_target in legacy_targets:
            raise ValueError(f"Duplicate scaffold legacy target: {legacy_target}")
        if template not in asset_by_path or asset_by_path[template].asset_type != "template":
            raise ValueError(f"{context}.template must reference a template asset")
        if (
            legacy_target in current_paths
            or legacy_target in protected_paths
            or legacy_target in retired_asset_paths
        ):
            raise ValueError(f"{context}.legacy_target cannot be managed or protected")
        if not all(SHA256_PATTERN.fullmatch(hash_value) for hash_value in content_hashes):
            raise ValueError(f"{context}.content_hashes must contain SHA-256 values")
        legacy_targets.add(legacy_target)
        upgrades.append(
            ScaffoldUpgrade(
                from_versions=from_versions,
                legacy_target=legacy_target,
                template=template,
                content_hashes=content_hashes,
            )
        )

    return MigrationConfig(
        legacy_manifest=legacy_manifest,
        legacy_version=legacy_version,
        legacy_conflicts=legacy_conflicts,
        protected_paths=protected_paths,
        retired_assets=tuple(retired_assets),
        retired_path_sets=tuple(retired_sets),
        scaffold_upgrades=tuple(upgrades),
    )


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
    if raw.get("schema_version") != 2:
        raise ValueError(f"Unsupported manifest schema_version: {raw.get('schema_version')}")

    pack_version = require_string(raw, "pack_version", "manifest")
    if not SEMVER_PATTERN.fullmatch(pack_version):
        raise ValueError("manifest.pack_version must use MAJOR.MINOR.PATCH")
    state_file = require_string(raw, "state_file", "manifest")
    relative_path(state_file, "manifest.state_file")

    profiles = require_string_list(raw, "profiles", "manifest")

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
        previous_hashes = optional_string_list(raw_asset, "previous_hashes", context)
        scaffold_group = raw_asset.get("scaffold_group")
        scaffold_target = raw_asset.get("scaffold_target")

        relative_path(path, f"{context}.path")
        reject_project_owned_tree_path(path, f"{context}.path")
        if path in paths:
            raise ValueError(f"Duplicate asset path: {path}")
        if asset_type not in VALID_TYPES:
            raise ValueError(f"{context}.type must be managed or template")
        if not set(asset_profiles).issubset(profile_set):
            raise ValueError(f"{context}.profiles contains an unknown profile")
        if not all(SHA256_PATTERN.fullmatch(hash_value) for hash_value in previous_hashes):
            raise ValueError(f"{context}.previous_hashes must contain SHA-256 values")

        if asset_type == "template":
            if (scaffold_group is None) != (scaffold_target is None):
                raise ValueError(f"{context} must define both scaffold fields or neither")
            if scaffold_group is not None and scaffold_group not in VALID_SCAFFOLD_GROUPS:
                raise ValueError(
                    f"{context}.scaffold_group must be one of: {', '.join(sorted(VALID_SCAFFOLD_GROUPS))}"
                )
            if not path.startswith("docs/templates/"):
                raise ValueError(f"{context}.path must be under docs/templates")
            if scaffold_group is not None:
                if not isinstance(scaffold_target, str) or not scaffold_target:
                    raise ValueError(f"{context}.scaffold_target must be a non-empty string")
                relative_path(scaffold_target, f"{context}.scaffold_target")
                reject_project_owned_tree_path(
                    scaffold_target,
                    f"{context}.scaffold_target",
                )
                if scaffold_target in scaffold_targets:
                    raise ValueError(f"Duplicate scaffold target: {scaffold_target}")
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
                previous_hashes=previous_hashes,
                scaffold_group=scaffold_group if isinstance(scaffold_group, str) else None,
                scaffold_target=scaffold_target if isinstance(scaffold_target, str) else None,
            )
        )

    collisions = paths.intersection(scaffold_targets)
    if collisions:
        raise ValueError(f"Scaffold targets collide with managed paths: {', '.join(sorted(collisions))}")
    migration = load_migration(raw.get("migration"), tuple(assets))
    if state_file in paths or state_file in scaffold_targets or state_file in package_files:
        raise ValueError("manifest.state_file cannot collide with pack or target assets")
    if migration and state_file in {
        migration.legacy_manifest,
        migration.legacy_version,
        migration.legacy_conflicts,
        *migration.protected_paths,
        *(asset.path for asset in migration.retired_assets),
        *(
            path
            for retired_set in migration.retired_path_sets
            for path in retired_set.paths
        ),
    }:
        raise ValueError("manifest.state_file cannot collide with migration paths")

    return PackManifest(
        schema_version=2,
        pack_version=pack_version,
        state_file=state_file,
        profiles=profiles,
        package_files=package_files,
        migration=migration,
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


def read_state_version(target_root: Path) -> str | None:
    state_path = safe_child(target_root, DEFAULT_STATE_FILE, "managed state path")
    if state_path.is_symlink():
        raise ValueError(f"Managed state cannot be a symbolic link: {DEFAULT_STATE_FILE}")
    if not state_path.exists():
        return None
    if not state_path.is_file():
        raise ValueError(f"Managed state is not a file: {DEFAULT_STATE_FILE}")
    try:
        raw = json.loads(state_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as ex:
        raise ValueError(f"Managed state is not valid JSON: {ex}") from ex
    version = raw.get("pack_version")
    if version is None:
        return None
    if not isinstance(version, str) or not SEMVER_PATTERN.fullmatch(version):
        raise ValueError("Managed state pack_version must use MAJOR.MINOR.PATCH")
    return version


def resolve_cli_version(source: str | None, target: str | None) -> str:
    if source:
        return load_manifest(Path(source).expanduser().resolve()).pack_version
    source_root = discover_source_root()
    if source_root is not None:
        return load_manifest(source_root).pack_version
    target_root = Path(target).expanduser().resolve() if target else Path.cwd().resolve()
    return read_state_version(target_root) or "unknown"


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


def file_hash(path: Path) -> str:
    hasher = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            hasher.update(chunk)
    return hasher.hexdigest()


def content_hash(content: str) -> str:
    normalized = content.replace("\r\n", "\n").replace("\r", "\n")
    return hashlib.sha256(normalized.encode("utf-8")).hexdigest()


def managed_file_hash(path: Path) -> str:
    return content_hash(path.read_text(encoding="utf-8"))


def read_legacy_state(target_root: Path, migration: MigrationConfig | None) -> LegacyState:
    if migration is None:
        return LegacyState(pack_version=None, hashes={}, manifest_exists=False)

    version: str | None = None
    version_path = safe_child(target_root, migration.legacy_version, "legacy version path")
    if version_path.is_symlink():
        raise ValueError(f"Legacy version path cannot be a symbolic link: {migration.legacy_version}")
    if version_path.exists():
        if not version_path.is_file():
            raise ValueError(f"Legacy version path is not a file: {migration.legacy_version}")
        version = version_path.read_text(encoding="utf-8").strip()
        version_key(version, "legacy pack version")

    manifest_path = safe_child(target_root, migration.legacy_manifest, "legacy manifest path")
    if manifest_path.is_symlink():
        raise ValueError(f"Legacy manifest cannot be a symbolic link: {migration.legacy_manifest}")
    if not manifest_path.exists():
        return LegacyState(pack_version=version, hashes={}, manifest_exists=False)
    if not manifest_path.is_file():
        raise ValueError(f"Legacy manifest is not a file: {migration.legacy_manifest}")

    try:
        raw = json.loads(manifest_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as ex:
        raise ValueError(f"Legacy manifest is not valid JSON: {ex}") from ex
    if not isinstance(raw, dict) or not isinstance(raw.get("files"), dict):
        raise ValueError("Legacy manifest must contain a files object")

    manifest_version = raw.get("pack_version")
    if manifest_version is not None:
        if not isinstance(manifest_version, str):
            raise ValueError("Legacy manifest pack_version must be a string")
        version_key(manifest_version, "legacy manifest pack_version")
        if version is not None and version != manifest_version:
            raise ValueError("Legacy version file and manifest disagree")
        version = manifest_version

    hashes: dict[str, str] = {}
    for path, hash_value in raw["files"].items():
        if not isinstance(path, str) or not isinstance(hash_value, str):
            raise ValueError("Legacy manifest files must map paths to SHA-256 strings")
        relative_path(path, "legacy manifest file path")
        if not SHA256_PATTERN.fullmatch(hash_value):
            raise ValueError(f"Legacy manifest hash is invalid for: {path}")
        hashes[path] = hash_value

    return LegacyState(pack_version=version, hashes=hashes, manifest_exists=True)


def read_managed_state(target_root: Path, manifest: PackManifest) -> ManagedState:
    state_path = safe_child(target_root, manifest.state_file, "managed state path")
    if state_path.is_symlink():
        raise ValueError(f"Managed state cannot be a symbolic link: {manifest.state_file}")
    if not state_path.exists():
        return ManagedState(
            pack_version=None,
            profile=None,
            managed_files={},
            tombstones={},
            exists=False,
        )
    if not state_path.is_file():
        raise ValueError(f"Managed state is not a file: {manifest.state_file}")

    try:
        raw = json.loads(state_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as ex:
        raise ValueError(f"Managed state is not valid JSON: {ex}") from ex
    if not isinstance(raw, dict) or raw.get("schema_version") != 1:
        raise ValueError("Managed state must use schema_version 1")
    if not isinstance(raw.get("pack_version"), str):
        raise ValueError("Managed state pack_version must be a string")
    version_key(raw["pack_version"], "managed state pack_version")
    if not isinstance(raw.get("profile"), str) or not raw["profile"]:
        raise ValueError("Managed state profile must be a non-empty string")

    allowed_paths = {asset.path for asset in manifest.assets}
    protected_paths: set[str] = set()
    retired_hashes: dict[str, set[str]] = {}
    if manifest.migration:
        protected_paths.update(manifest.migration.protected_paths)
        retired_hashes.update(
            {
                asset.path: set(asset.content_hashes)
                for asset in manifest.migration.retired_assets
            }
        )
    allowed_paths.update(protected_paths)
    allowed_paths.update(retired_hashes)

    def hash_map(key: str) -> dict[str, str]:
        value = raw.get(key)
        if not isinstance(value, dict):
            raise ValueError(f"Managed state {key} must be an object")
        result: dict[str, str] = {}
        for path, hash_value in value.items():
            if not isinstance(path, str) or not isinstance(hash_value, str):
                raise ValueError(f"Managed state {key} must map paths to SHA-256 strings")
            relative_path(path, f"managed state {key} path")
            if path == manifest.state_file:
                raise ValueError("Managed state cannot own itself")
            if not SHA256_PATTERN.fullmatch(hash_value):
                raise ValueError(f"Managed state hash is invalid for: {path}")
            if path not in allowed_paths:
                raise ValueError(f"Managed state contains an unknown pack-owned path: {path}")
            if path in retired_hashes and hash_value not in retired_hashes[path]:
                raise ValueError(f"Managed state hash is not recognized for retired asset: {path}")
            result[path] = hash_value
        return result

    managed_files = hash_map("managed_files")
    tombstones = hash_map("tombstones")
    if set(managed_files).intersection(tombstones):
        raise ValueError("Managed state paths cannot be both active and tombstoned")
    return ManagedState(
        pack_version=raw["pack_version"],
        profile=raw["profile"],
        managed_files=managed_files,
        tombstones=tombstones,
        exists=True,
    )


def legacy_target(target_root: Path, path: str) -> Path:
    return target_root / relative_path(path, "legacy target")


def target_resolves_within_root(target_root: Path, target: Path) -> bool:
    try:
        target.resolve().relative_to(target_root.resolve())
    except ValueError:
        return False
    return True


def copy_asset(source_root: Path, target_root: Path, asset: Asset, dry_run: bool) -> SyncAction:
    source = safe_child(source_root / FILES_DIRECTORY, asset.path, "asset path")
    target = safe_child(target_root, asset.path, "asset target")
    existed = target.is_file()
    if existed and managed_file_hash(source) == managed_file_hash(target):
        return SyncAction("unchanged", asset.path, "managed copy already matches")
    if dry_run:
        detail = "would overwrite managed copy" if existed else "would create managed copy"
        return SyncAction("copy", asset.path, detail)

    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, target)
    detail = "overwrote managed copy" if existed else "created managed copy"
    return SyncAction("copy", asset.path, detail)


def prune_stale_assets(
    source_root: Path,
    target_root: Path,
    manifest: PackManifest,
    selected: tuple[Asset, ...],
    state: ManagedState,
    legacy_hashes: dict[str, str],
    dry_run: bool,
) -> tuple[list[SyncAction], dict[str, str]]:
    selected_paths = {asset.path for asset in selected}
    candidates = {
        path: hash_value
        for path, hash_value in state.tombstones.items()
        if path not in selected_paths
    }
    candidates.update(
        (path, hash_value)
        for path, hash_value in state.managed_files.items()
        if path not in selected_paths
    )
    if not state.exists:
        for asset in manifest.assets:
            if asset.path not in selected_paths:
                source = safe_child(source_root / FILES_DIRECTORY, asset.path, "asset path")
                expected_hash = managed_file_hash(source)
                target = target_root / relative_path(asset.path, "bootstrap managed path")
                if target.is_file() and not target.is_symlink():
                    target_hash = managed_file_hash(target)
                    legacy_match = (
                        asset.path in legacy_hashes
                        and file_hash(target) == legacy_hashes[asset.path]
                    )
                    if target_hash in asset.previous_hashes or legacy_match:
                        expected_hash = target_hash
                candidates.setdefault(asset.path, expected_hash)

    actions: list[SyncAction] = []
    tombstones: dict[str, str] = {}
    protected_paths = {
        asset.scaffold_target
        for asset in manifest.assets
        if asset.scaffold_target is not None
    }
    if manifest.migration:
        protected_paths.update(manifest.migration.protected_paths)
    for path, expected_hash in sorted(candidates.items()):
        target = target_root / relative_path(path, "stale managed path")
        if not target.exists() and not target.is_symlink():
            continue
        if path in protected_paths:
            actions.append(SyncAction("preserve", path, "stale path is now project-owned"))
            continue
        if target.is_symlink() or not target_resolves_within_root(target_root, target):
            actions.append(SyncAction("preserve", path, "stale managed path is a symbolic link"))
            tombstones[path] = expected_hash
        elif not target.is_file():
            actions.append(SyncAction("preserve", path, "stale managed path is not a regular file"))
            tombstones[path] = expected_hash
        elif managed_file_hash(target) != expected_hash:
            actions.append(SyncAction("preserve", path, "stale managed file has local changes"))
            tombstones[path] = expected_hash
        elif dry_run:
            actions.append(SyncAction("remove", path, "would remove unchanged stale managed file"))
        else:
            target.unlink()
            actions.append(SyncAction("remove", path, "removed unchanged stale managed file"))
    return actions, tombstones


def report_project_owned_paths(
    target_root: Path,
    manifest: PackManifest,
    state: ManagedState,
    already_reported: set[str],
) -> list[SyncAction]:
    if manifest.migration is None or state.pack_version == manifest.pack_version:
        return []
    actions: list[SyncAction] = []
    for path in manifest.migration.protected_paths:
        target = target_root / relative_path(path, "project-owned path")
        if path not in already_reported and (target.exists() or target.is_symlink()):
            actions.append(SyncAction("preserve", path, "project-owned path preserved"))
    return actions


def write_managed_state(
    source_root: Path,
    target_root: Path,
    manifest: PackManifest,
    profile: str,
    selected: tuple[Asset, ...],
    tombstones: dict[str, str],
    dry_run: bool,
) -> SyncAction:
    state_path = safe_child(target_root, manifest.state_file, "managed state path")
    payload = {
        "schema_version": 1,
        "pack_version": manifest.pack_version,
        "profile": profile,
        "managed_files": {
            asset.path: managed_file_hash(
                safe_child(source_root / FILES_DIRECTORY, asset.path, "asset path")
            )
            for asset in selected
        },
        "tombstones": dict(sorted(tombstones.items())),
    }
    content = json.dumps(payload, indent=2, sort_keys=True) + "\n"
    existed = state_path.is_file()
    if existed and state_path.read_text(encoding="utf-8") == content:
        return SyncAction("unchanged", manifest.state_file, "managed ownership state already matches")
    if dry_run:
        detail = "would update managed ownership state" if existed else "would create managed ownership state"
        return SyncAction("state", manifest.state_file, detail)
    state_path.parent.mkdir(parents=True, exist_ok=True)
    state_path.write_text(content, encoding="utf-8", newline="\n")
    detail = "updated managed ownership state" if existed else "created managed ownership state"
    return SyncAction("state", manifest.state_file, detail)


def retire_legacy_paths(
    target_root: Path,
    migration: MigrationConfig | None,
    state: LegacyState,
    dry_run: bool,
) -> list[SyncAction]:
    if migration is None or (state.pack_version is None and not state.manifest_exists):
        return []

    retired: set[str] = set()
    for retired_set in migration.retired_path_sets:
        if state.pack_version is None or version_key(
            state.pack_version, "legacy pack version"
        ) <= version_key(retired_set.through_version, "retired path version"):
            retired.update(retired_set.paths)

    actions: list[SyncAction] = []
    unsupported_version = state.pack_version is not None and not retired
    blocked = unsupported_version
    for path in sorted(retired):
        target = legacy_target(target_root, path)
        if not target.exists() and not target.is_symlink():
            continue
        expected_hash = state.hashes.get(path)
        # The legacy version file is repo-seed-owned even when the old manifest
        # omitted its own hash. Recognize it by its recorded version content so
        # migration can complete instead of preserving it (and the manifest)
        # forever.
        version_file_match = (
            expected_hash is None
            and path == migration.legacy_version
            and state.pack_version is not None
            and target.is_file()
            and not target.is_symlink()
            and target.read_text(encoding="utf-8").strip() == state.pack_version
        )
        if target.is_symlink() or not target_resolves_within_root(target_root, target):
            actions.append(SyncAction("preserve", path, "retired path is a symbolic link"))
            blocked = True
        elif not target.is_file():
            actions.append(SyncAction("preserve", path, "retired path is not a regular file"))
            blocked = True
        elif expected_hash is None and not version_file_match:
            actions.append(SyncAction("preserve", path, "retired path is not recorded in the legacy manifest"))
            blocked = True
        elif expected_hash is not None and file_hash(target) != expected_hash:
            actions.append(SyncAction("preserve", path, "retired path has local changes"))
            blocked = True
        elif dry_run:
            actions.append(SyncAction("remove", path, "would remove unchanged retired managed file"))
        else:
            target.unlink()
            actions.append(SyncAction("remove", path, "removed unchanged retired managed file"))

    if state.manifest_exists:
        manifest_path = legacy_target(target_root, migration.legacy_manifest)
        if blocked:
            detail = (
                f"legacy manifest retained because version {state.pack_version} is newer than supported migrations"
                if unsupported_version
                else "legacy manifest retained while retired files need review"
            )
            actions.append(
                SyncAction(
                    "preserve",
                    migration.legacy_manifest,
                    detail,
                )
            )
        elif dry_run:
            actions.append(SyncAction("remove", migration.legacy_manifest, "would remove migrated legacy manifest"))
        else:
            manifest_path.unlink()
            actions.append(SyncAction("remove", migration.legacy_manifest, "removed migrated legacy manifest"))

    for path in migration.protected_paths:
        target = legacy_target(target_root, path)
        if (target.exists() or target.is_symlink()) and (state.manifest_exists or state.pack_version):
            actions.append(SyncAction("preserve", path, "legacy-managed path is now project-owned"))

    conflicts = legacy_target(target_root, migration.legacy_conflicts)
    if (conflicts.exists() or conflicts.is_symlink()) and (state.manifest_exists or state.pack_version):
        actions.append(SyncAction("preserve", migration.legacy_conflicts, "legacy conflict output requires review"))

    return actions


def add_source_path_marker(body: str, template_path: str) -> str:
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


def add_source_marker(body: str, template_path: str) -> str:
    source_marker = f"<!-- Scaffolded from: {template_path} -->"
    marked = add_source_path_marker(body, template_path)
    hash_marker = f"<!-- Scaffolded content SHA-256: {content_hash(marked)} -->"
    return marked.replace(source_marker, f"{source_marker}\n{hash_marker}", 1)


def render_scaffold(source_root: Path, asset: Asset) -> str:
    source = safe_child(source_root / FILES_DIRECTORY, asset.path, "template path")
    body = template_body(source)
    if asset.scaffold_target and Path(asset.scaffold_target).suffix.lower() == ".md":
        return add_source_marker(body, asset.path)
    return body


def verified_current_scaffold(content: str, asset: Asset) -> bool | None:
    source_matches = list(SCAFFOLD_SOURCE_PATTERN.finditer(content))
    hash_matches = list(SCAFFOLD_HASH_PATTERN.finditer(content))
    if not source_matches and not hash_matches:
        return None
    if len(source_matches) == 1 and not hash_matches:
        return None
    if (
        len(source_matches) != 1
        or len(hash_matches) != 1
        or source_matches[0].group("source") != asset.path
    ):
        return False
    without_hash = SCAFFOLD_HASH_PATTERN.sub("", content, count=1)
    return content_hash(without_hash) == hash_matches[0].group("hash")


def legacy_template_id(asset: Asset) -> str | None:
    if asset.path.endswith("/.github/bug-report.template.md"):
        return "github-bug-template"
    if asset.path.endswith("/.github/feature-request.template.md"):
        return "github-feature-template"
    if ".template" not in Path(asset.path).name:
        return None
    return f"{Path(asset.path).name.split('.template', 1)[0]}-template"


def verified_legacy_scaffold(content: str, asset: Asset) -> bool | None:
    matches = list(LEGACY_PROVENANCE_PATTERN.finditer(content))
    if not matches:
        return None
    if len(matches) != 1 or matches[0].group("id") != legacy_template_id(asset):
        return False
    without_marker = LEGACY_PROVENANCE_PATTERN.sub("", content, count=1).rstrip()
    return content_hash(without_marker) == matches[0].group("hash")


def write_scaffold(target: Path, content: str, dry_run: bool) -> None:
    if dry_run:
        return
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(content, encoding="utf-8", newline="\n")


def upgrade_scaffold_asset(
    source_root: Path,
    target_root: Path,
    asset: Asset,
    migration: MigrationConfig | None,
    state: LegacyState,
    dry_run: bool,
) -> SyncAction | None:
    if asset.scaffold_target is None:
        raise ValueError(f"Template has no scaffold target: {asset.path}")
    if migration and asset.scaffold_target in migration.protected_paths:
        return None

    target = safe_child(target_root, asset.scaffold_target, "scaffold target")
    rendered = render_scaffold(source_root, asset)
    if target.is_file() and not target.is_symlink() and target.suffix.lower() == ".md":
        content = target.read_text(encoding="utf-8")
        current_verified = verified_current_scaffold(content, asset)
        legacy_verified = verified_legacy_scaffold(content, asset)
        old_current_render = add_source_path_marker(
            template_body(safe_child(source_root / FILES_DIRECTORY, asset.path, "template path")),
            asset.path,
        )
        source_only_marker = (
            current_verified is None
            and SCAFFOLD_SOURCE_PATTERN.search(content) is not None
        )

        if current_verified is False or legacy_verified is False:
            return SyncAction("preserve", asset.scaffold_target, "scaffold provenance does not match local content")
        if current_verified is True or legacy_verified is True or (
            source_only_marker and content == old_current_render
        ):
            if content == rendered:
                return SyncAction("skip", asset.scaffold_target, "scaffold already matches current template")
            write_scaffold(target, rendered, dry_run)
            detail = "would upgrade verified scaffold" if dry_run else "upgraded verified scaffold"
            return SyncAction("upgrade", asset.scaffold_target, detail)
        if source_only_marker:
            return SyncAction("preserve", asset.scaffold_target, "scaffold has local changes")

    if migration is None or state.pack_version is None:
        return None
    for upgrade in migration.scaffold_upgrades:
        if upgrade.template != asset.path or state.pack_version not in upgrade.from_versions:
            continue
        legacy_path = legacy_target(target_root, upgrade.legacy_target)
        if not legacy_path.is_file() or legacy_path.is_symlink():
            continue
        if content_hash(legacy_path.read_text(encoding="utf-8")) not in upgrade.content_hashes:
            continue
        if upgrade.legacy_target != asset.scaffold_target and (target.exists() or target.is_symlink()):
            return SyncAction(
                "preserve",
                upgrade.legacy_target,
                f"verified legacy scaffold retained because {asset.scaffold_target} exists",
            )

        write_scaffold(target, rendered, dry_run)
        if upgrade.legacy_target != asset.scaffold_target and not dry_run:
            legacy_path.unlink()
        detail = (
            f"would migrate verified scaffold to {asset.scaffold_target}"
            if dry_run
            else f"migrated verified scaffold to {asset.scaffold_target}"
        )
        return SyncAction("upgrade", upgrade.legacy_target, detail)
    return None


def scaffold_asset(source_root: Path, target_root: Path, asset: Asset, dry_run: bool) -> SyncAction:
    if asset.scaffold_target is None:
        raise ValueError(f"Template has no scaffold target: {asset.path}")

    target = safe_child(target_root, asset.scaffold_target, "scaffold target")
    if target.exists() or target.is_symlink():
        return SyncAction("skip", asset.scaffold_target, "project-owned destination already exists")

    body = render_scaffold(source_root, asset)

    if dry_run:
        return SyncAction("scaffold", asset.scaffold_target, f"would create from {asset.path}")

    write_scaffold(target, body, dry_run=False)
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
    if profile == "full" and scaffold_project_files:
        raise ValueError(
            "The full profile is a reference catalog and cannot scaffold project files; "
            "choose minimal, library, app, or game"
        )
    legacy_state = read_legacy_state(target_root, manifest.migration)
    managed_state = read_managed_state(target_root, manifest)

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
    state_path = safe_child(target_root, manifest.state_file, "managed state path")
    validate_parent_directory(state_path, target_root, "Managed state")

    actions = retire_legacy_paths(target_root, manifest.migration, legacy_state, dry_run)
    actions.extend(
        report_project_owned_paths(
            target_root,
            manifest,
            managed_state,
            {action.path for action in actions},
        )
    )
    prune_actions, tombstones = prune_stale_assets(
        source_root,
        target_root,
        manifest,
        selected,
        managed_state,
        legacy_state.hashes,
        dry_run,
    )
    actions.extend(prune_actions)
    actions.extend(copy_asset(source_root, target_root, asset, dry_run) for asset in selected)
    for asset in scaffold_assets:
        migration_action = upgrade_scaffold_asset(
            source_root,
            target_root,
            asset,
            manifest.migration,
            legacy_state,
            dry_run,
        )
        actions.append(
            migration_action
            if migration_action is not None
            else scaffold_asset(source_root, target_root, asset, dry_run)
        )
    actions.append(
        write_managed_state(
            source_root,
            target_root,
            manifest,
            profile,
            selected,
            tombstones,
            dry_run,
        )
    )
    return actions


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Migrate legacy files, synchronize a managed profile, and optionally scaffold project files."
    )
    parser.add_argument(
        "--source",
        help="Pack directory containing manifest.json and files/. Auto-detected when run from an extracted pack.",
    )
    parser.add_argument("--target", default=".", help="Target repository. Defaults to the current directory.")
    parser.add_argument(
        "--profile",
        help="Profile name. Required on first sync; later syncs reuse the recorded profile when omitted.",
    )
    parser.add_argument(
        "--scaffold-project-files",
        action="store_true",
        help="Create missing project files or upgrade verified unchanged Markdown.",
    )
    parser.add_argument(
        "--scaffold-github-templates",
        action="store_true",
        help="Create missing GitHub files or upgrade verified unchanged Markdown.",
    )
    parser.add_argument(
        "--scaffold-editorconfig",
        action="store_true",
        help="Create .editorconfig only when it is missing.",
    )
    parser.add_argument("--dry-run", action="store_true", help="Validate and show operations without writing files.")
    parser.add_argument(
        "--version",
        action="store_true",
        help="Show the detected pack version and exit.",
    )
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)
    try:
        if args.version:
            print(f"{parser.prog} {resolve_cli_version(args.source, args.target)}")
            return 0
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
        if args.profile:
            profile = args.profile
        else:
            state = read_managed_state(target_root, manifest)
            if (
                not state.exists
                or state.profile not in manifest.profiles
                or state.profile == "full"
            ):
                raise ValueError(
                    "No reusable project profile is recorded; pass --profile with "
                    "minimal, library, app, or game"
                )
            profile = state.profile
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
    removals = sum(action.action == "remove" for action in actions)
    upgrades = sum(action.action == "upgrade" for action in actions)
    scaffolds = sum(action.action == "scaffold" for action in actions)
    state_updates = sum(action.action == "state" for action in actions)
    unchanged = sum(action.action == "unchanged" for action in actions)
    preserved = sum(action.action in {"preserve", "skip"} for action in actions)
    mode = "Dry run" if args.dry_run else "Sync"
    print(
        f"{mode} complete. Removals: {removals}. Upgrades: {upgrades}. "
        f"Managed copies: {copies}. Scaffolds: {scaffolds}. "
        f"State updates: {state_updates}. Unchanged: {unchanged}. Preserved: {preserved}."
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
