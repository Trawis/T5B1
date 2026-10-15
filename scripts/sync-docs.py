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
VALID_SCAFFOLD_GROUPS = {"project", "optional", "github", "editorconfig"}
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
LEGACY_LABEL_PATTERN = re.compile(r"^labels:\s*(.+)$", re.MULTILINE)
LEGACY_LABEL_VALUES = {"bug", "enhancement"}
PRE_5_0_STATE_ERROR = (
    "This repository uses a pre-5.0 repo-seed state without explicit conventions.\n"
    "Rerun with --conventions <list>."
)


def format_unavailable_conventions_error(unavailable: frozenset[str]) -> str:
    names = ", ".join(sorted(unavailable))
    return (
        "The recorded convention selection contains conventions no longer available\n"
        f"in this repo-seed pack: {names}.\n\n"
        "Rerun with --conventions <current-list> to confirm the new selection."
    )


@dataclass(frozen=True)
class Asset:
    path: str
    asset_type: str
    profiles: tuple[str, ...]
    scaffold_group: str | None = None
    scaffold_target: str | None = None
    convention: str | None = None


@dataclass(frozen=True)
class PackManifest:
    schema_version: int
    pack_version: str
    state_file: str
    profiles: tuple[str, ...]
    package_files: tuple[str, ...]
    assets: tuple[Asset, ...]


@dataclass(frozen=True)
class SyncAction:
    action: str
    path: str
    detail: str


@dataclass(frozen=True)
class ManagedState:
    schema_version: int | None
    pack_version: str | None
    profile: str | None
    conventions: frozenset[str] | None
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
    if raw.get("schema_version") != 3:
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
        scaffold_group = raw_asset.get("scaffold_group")
        scaffold_target = raw_asset.get("scaffold_target")
        convention = raw_asset.get("convention")

        relative_path(path, f"{context}.path")
        reject_project_owned_tree_path(path, f"{context}.path")
        if path in paths:
            raise ValueError(f"Duplicate asset path: {path}")
        if asset_type not in VALID_TYPES:
            raise ValueError(f"{context}.type must be managed or template")
        if not set(asset_profiles).issubset(profile_set):
            raise ValueError(f"{context}.profiles contains an unknown profile")
        if convention is not None and (not isinstance(convention, str) or not convention):
            raise ValueError(f"{context}.convention must be a non-empty string")
        if convention is not None and asset_type != "managed":
            raise ValueError(f"{context}.convention only applies to managed assets")

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
                scaffold_group=scaffold_group if isinstance(scaffold_group, str) else None,
                scaffold_target=scaffold_target if isinstance(scaffold_target, str) else None,
                convention=convention if isinstance(convention, str) else None,
            )
        )

    collisions = paths.intersection(scaffold_targets)
    if collisions:
        raise ValueError(f"Scaffold targets collide with managed paths: {', '.join(sorted(collisions))}")
    if state_file in paths or state_file in scaffold_targets or state_file in package_files:
        raise ValueError("manifest.state_file cannot collide with pack or target assets")

    return PackManifest(
        schema_version=3,
        pack_version=pack_version,
        state_file=state_file,
        profiles=profiles,
        package_files=package_files,
        assets=tuple(assets),
    )


def known_conventions(manifest: PackManifest) -> frozenset[str]:
    return frozenset(asset.convention for asset in manifest.assets if asset.convention)


def convention_assets(manifest: PackManifest) -> dict[str, Asset]:
    return {asset.convention: asset for asset in manifest.assets if asset.convention}


def validate_conventions(conventions: frozenset[str], manifest: PackManifest) -> None:
    unknown = conventions - known_conventions(manifest)
    if unknown:
        raise ValueError(
            f"Unknown convention(s): {', '.join(sorted(unknown))}. "
            f"Known: {', '.join(sorted(known_conventions(manifest))) or 'none'}"
        )


def assets_for_profile(
    manifest: PackManifest, profile: str, conventions: frozenset[str] = frozenset()
) -> tuple[Asset, ...]:
    if profile not in manifest.profiles:
        raise ValueError(f"Unknown profile '{profile}'. Choices: {', '.join(manifest.profiles)}")
    validate_conventions(conventions, manifest)
    return tuple(
        asset
        for asset in manifest.assets
        if profile in asset.profiles
        and (asset.convention is None or asset.convention in conventions)
    )


def scaffold_name(asset: Asset) -> str:
    name = Path(asset.path).name
    for suffix in (".template.md", ".template.yml", ".template"):
        if name.endswith(suffix):
            return name[: -len(suffix)]
    return name


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


def content_hash(content: str) -> str:
    normalized = content.replace("\r\n", "\n").replace("\r", "\n")
    return hashlib.sha256(normalized.encode("utf-8")).hexdigest()


def managed_file_hash(path: Path) -> str:
    return content_hash(path.read_text(encoding="utf-8"))


def read_managed_state(target_root: Path, manifest: PackManifest) -> ManagedState:
    state_path = safe_child(target_root, manifest.state_file, "managed state path")
    if state_path.is_symlink():
        raise ValueError(f"Managed state cannot be a symbolic link: {manifest.state_file}")
    if not state_path.exists():
        return ManagedState(
            schema_version=None,
            pack_version=None,
            profile=None,
            conventions=None,
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
    if not isinstance(raw, dict) or raw.get("schema_version") not in (1, 2):
        raise ValueError("Managed state must use schema_version 1 or 2")
    schema_version = raw["schema_version"]
    if not isinstance(raw.get("pack_version"), str):
        raise ValueError("Managed state pack_version must be a string")
    version_key(raw["pack_version"], "managed state pack_version")
    if not isinstance(raw.get("profile"), str) or not raw["profile"]:
        raise ValueError("Managed state profile must be a non-empty string")

    # A path recorded here does not need to exist in the current manifest: a
    # future pack version may remove a managed asset entirely, and the state
    # file is sufficient historical ownership evidence on its own. Safety is
    # enforced by path validation below, not by manifest membership; a path
    # no longer in the manifest is handled conservatively by
    # prune_stale_assets (preserved and tombstoned, never auto-deleted).
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
            result[path] = hash_value
        return result

    managed_files = hash_map("managed_files")
    tombstones = hash_map("tombstones")
    if set(managed_files).intersection(tombstones):
        raise ValueError("Managed state paths cannot be both active and tombstoned")

    # Schema 1 is the one small pre-5.0 bridge: it never records conventions,
    # so `conventions` stays None to signal that an explicit --conventions is
    # required before this state can be synced. Schema 2 always records them.
    #
    # A persisted convention id is not required to exist in the current
    # manifest: state is historical ownership/configuration evidence, and a
    # future pack may legitimately remove a convention. Callers that need a
    # convention selection made only of currently known ids (synchronize(),
    # audit_target()) are responsible for checking that and failing clearly;
    # this parser only validates shape.
    conventions: frozenset[str] | None = None
    if schema_version == 2:
        raw_conventions = raw.get("conventions")
        if not isinstance(raw_conventions, list) or not all(
            isinstance(item, str) and item for item in raw_conventions
        ):
            raise ValueError("Managed state conventions must be a non-empty-string array")
        if len(raw_conventions) != len(set(raw_conventions)):
            raise ValueError("Managed state conventions contains duplicates")
        conventions = frozenset(raw_conventions)

    return ManagedState(
        schema_version=schema_version,
        pack_version=raw["pack_version"],
        profile=raw["profile"],
        conventions=conventions,
        managed_files=managed_files,
        tombstones=tombstones,
        exists=True,
    )


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
    target_root: Path,
    manifest: PackManifest,
    selected: tuple[Asset, ...],
    state: ManagedState,
    dry_run: bool,
) -> tuple[list[SyncAction], dict[str, str]]:
    selected_paths = {asset.path for asset in selected}
    known_paths = {asset.path for asset in manifest.assets}
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

    actions: list[SyncAction] = []
    tombstones: dict[str, str] = {}
    protected_paths = {
        asset.scaffold_target
        for asset in manifest.assets
        if asset.scaffold_target is not None
    }
    for path, expected_hash in sorted(candidates.items()):
        target = target_root / relative_path(path, "stale managed path")
        if not target.exists() and not target.is_symlink():
            continue
        if path in protected_paths:
            actions.append(SyncAction("preserve", path, "stale path is now project-owned"))
            continue
        if path not in known_paths:
            # No longer a managed asset in the current manifest at all. The
            # state file is the only historical evidence for it, so it is
            # never auto-deleted merely because its hash matches; it stays
            # tombstoned for manual review until the file itself is removed.
            actions.append(SyncAction("preserve", path, "historical managed path is no longer in the manifest"))
            tombstones[path] = expected_hash
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


def write_managed_state(
    source_root: Path,
    target_root: Path,
    manifest: PackManifest,
    profile: str,
    conventions: frozenset[str],
    selected: tuple[Asset, ...],
    tombstones: dict[str, str],
    dry_run: bool,
) -> SyncAction:
    state_path = safe_child(target_root, manifest.state_file, "managed state path")
    payload = {
        "schema_version": 2,
        "pack_version": manifest.pack_version,
        "profile": profile,
        "conventions": sorted(conventions),
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
    """Whether `content` carries valid, matching repo-seed scaffold provenance.

    Returns True when the markers are present and the content is unchanged,
    False when markers are present but do not match (customized or invalid),
    and None when there is no repo-seed provenance to evaluate at all.
    """
    source_matches = list(SCAFFOLD_SOURCE_PATTERN.finditer(content))
    hash_matches = list(SCAFFOLD_HASH_PATTERN.finditer(content))
    if not source_matches and not hash_matches:
        return None
    if (
        len(source_matches) != 1
        or len(hash_matches) != 1
        or source_matches[0].group("source") != asset.path
    ):
        return False
    without_hash = SCAFFOLD_HASH_PATTERN.sub("", content, count=1)
    return content_hash(without_hash) == hash_matches[0].group("hash")


def write_scaffold(target: Path, content: str, dry_run: bool) -> None:
    if dry_run:
        return
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(content, encoding="utf-8", newline="\n")


def upgrade_scaffold_asset(source_root: Path, target_root: Path, asset: Asset, dry_run: bool) -> SyncAction | None:
    """Upgrade an existing scaffold in place, if and only if it is safe.

    - Valid, unchanged repo-seed provenance: refresh it to the current template.
    - Provenance present but not matching (customized): preserve it, report why.
    - No provenance at all: return None; the caller treats it as project-owned
      and leaves it alone. There is no attempt to identify older, unmarked
      scaffolds from earlier repo-seed generations.
    """
    if asset.scaffold_target is None:
        raise ValueError(f"Template has no scaffold target: {asset.path}")

    target = safe_child(target_root, asset.scaffold_target, "scaffold target")
    if not target.is_file() or target.is_symlink() or target.suffix.lower() != ".md":
        return None

    content = target.read_text(encoding="utf-8")
    verified = verified_current_scaffold(content, asset)
    if verified is None:
        return None
    if verified is False:
        return SyncAction("preserve", asset.scaffold_target, "scaffold provenance does not match local content")

    rendered = render_scaffold(source_root, asset)
    if content == rendered:
        return SyncAction("skip", asset.scaffold_target, "scaffold already matches current template")
    write_scaffold(target, rendered, dry_run)
    detail = "would upgrade verified scaffold" if dry_run else "upgraded verified scaffold"
    return SyncAction("upgrade", asset.scaffold_target, detail)


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


def audit_target(
    source_root: Path,
    target_root: Path,
    profile: str | None,
    conventions: frozenset[str] | None = None,
) -> list[str]:
    """Report drift diagnostics for a target without writing any files.

    This is a local, file-based diagnostic: it never contacts GitHub and
    never requires network access.
    """
    source_root = source_root.expanduser().resolve()
    target_root = target_root.expanduser().resolve()
    manifest = load_manifest(source_root)
    state = read_managed_state(target_root, manifest)

    lines: list[str] = [
        f"pack version (source): {manifest.pack_version}",
        f"pack version (target): {state.pack_version or 'not recorded'}",
    ]
    if state.pack_version and state.pack_version != manifest.pack_version:
        lines.append("drift: target pack version is behind the source pack")

    active_profile = profile or state.profile
    if active_profile is None:
        lines.append("drift: no recorded or requested profile; pass --profile to audit one")
        return lines
    if active_profile not in manifest.profiles:
        lines.append(f"drift: '{active_profile}' is not a known profile")
        return lines
    lines.append(f"profile: {active_profile}")

    conventions_resolved = True
    unresolved_reason: str | None = None
    if conventions is not None:
        active_conventions = frozenset(conventions)
    elif state.conventions is not None:
        unavailable = state.conventions - known_conventions(manifest)
        if unavailable:
            for name in sorted(unavailable):
                lines.append(f"recorded convention unavailable: {name}")
            active_conventions = frozenset()
            conventions_resolved = False
            unresolved_reason = "recorded convention no longer available"
        else:
            active_conventions = state.conventions
    elif state.schema_version == 1:
        lines.append(
            "info: pre-5.0 state has no explicit convention selection; "
            "pass --conventions to audit convention state"
        )
        active_conventions = frozenset()
        conventions_resolved = False
        unresolved_reason = "pre-5.0 state"
    else:
        active_conventions = frozenset()
    lines.append(
        f"conventions: {', '.join(sorted(active_conventions)) or 'none selected'}"
        if conventions_resolved
        else f"conventions: unresolved ({unresolved_reason})"
    )

    selected = assets_for_profile(manifest, active_profile, active_conventions)
    for asset in selected:
        target = safe_child(target_root, asset.path, "asset target")
        source = safe_child(source_root / FILES_DIRECTORY, asset.path, "asset path")
        try:
            validate_managed_destination(target_root, asset)
        except ValueError as ex:
            lines.append(f"drift: {ex}")
            continue
        if not target.is_file():
            lines.append(f"missing: {asset.path} (expected managed file for profile '{active_profile}')")
        elif managed_file_hash(target) != managed_file_hash(source):
            lines.append(f"drift: {asset.path} differs from the current managed content")

    # .agents/project.md is project-owned and optional: a repository with no
    # meaningful project-specific instructions legitimately has none. This is
    # informational only, never a drift/failure finding.
    project_guidance = safe_child(target_root, ".agents/project.md", "project guidance path")
    if not project_guidance.is_file():
        lines.append("info: .agents/project.md not present")

    for asset in selected:
        if asset.asset_type != "template" or asset.scaffold_target is None:
            continue
        if not asset.scaffold_target.endswith(".md"):
            continue
        scaffold = safe_child(target_root, asset.scaffold_target, "scaffold target")
        if not scaffold.is_file():
            continue
        content = scaffold.read_text(encoding="utf-8")
        match = LEGACY_LABEL_PATTERN.search(content)
        if match and match.group(1).strip() in LEGACY_LABEL_VALUES:
            lines.append(
                f"legacy label: {asset.scaffold_target} uses '{match.group(1).strip()}' "
                "instead of a canonical type: label"
            )
        # Reuse the same verification the sync path uses to decide whether a
        # scaffold may be upgraded, so "outdated" is reported only when there
        # is reliable evidence of an unchanged repo-seed scaffold, never for
        # an ordinary customized project-owned document.
        upgrade_status = upgrade_scaffold_asset(source_root, target_root, asset, dry_run=True)
        if upgrade_status is not None and upgrade_status.action == "upgrade":
            lines.append(
                f"outdated scaffold: {asset.scaffold_target} is a verified unchanged repo-seed "
                "scaffold and can be safely upgraded"
            )
        elif (
            upgrade_status is not None
            and upgrade_status.action == "preserve"
            and "does not match local content" in upgrade_status.detail
        ):
            lines.append(
                f"info: {asset.scaffold_target} has repo-seed scaffold markers that no longer "
                "match its content; treated as a customized, project-owned document"
            )

    if conventions_resolved:
        for convention, asset in convention_assets(manifest).items():
            if convention in active_conventions:
                continue
            target = safe_child(target_root, asset.path, "asset target")
            if target.is_file():
                lines.append(
                    f"unused managed convention: {asset.path} "
                    f"(selected conventions: {', '.join(sorted(active_conventions)) or 'none'})"
                )

    # A tombstone means stale managed content was preserved and still needs
    # manual review; a state-owned path no longer in the manifest at all is
    # the same situation before a normal sync has moved it into tombstones.
    # Report both, without touching state (audit never writes anything).
    known_paths = {asset.path for asset in manifest.assets}
    reported_stale_paths: set[str] = set()
    for path in sorted(state.tombstones):
        if path in reported_stale_paths:
            continue
        target = safe_child(target_root, path, "tombstone target")
        if target.is_file():
            lines.append(f"tombstone: {path} (preserved stale managed content requires review)")
            reported_stale_paths.add(path)
    for path in sorted(state.managed_files):
        if path in reported_stale_paths or path in known_paths:
            continue
        target = safe_child(target_root, path, "historical managed target")
        if target.is_file():
            lines.append(f"tombstone: {path} (preserved stale managed content requires review)")
            reported_stale_paths.add(path)

    findings = [
        line
        for line in lines
        if line.startswith(
            (
                "drift:",
                "missing:",
                "legacy label:",
                "outdated scaffold:",
                "unused managed convention:",
                "recorded convention unavailable:",
                "tombstone:",
            )
        )
    ]
    lines.append("no drift detected" if not findings else f"{len(findings)} finding(s) reported above")
    return lines


def synchronize(
    source_root: Path,
    target_root: Path,
    profile: str,
    conventions: frozenset[str] | None = None,
    scaffold_project_files: bool = False,
    scaffold_github_templates: bool = False,
    scaffold_editorconfig: bool = False,
    scaffold_names: tuple[str, ...] = (),
    dry_run: bool = False,
) -> list[SyncAction]:
    source_root = source_root.expanduser().resolve()
    target_root = target_root.expanduser().resolve()
    if not target_root.is_dir():
        raise ValueError(f"Target repository does not exist or is not a directory: {target_root}")

    manifest = load_manifest(source_root)
    managed_state = read_managed_state(target_root, manifest)

    if conventions is not None:
        resolved_conventions = frozenset(conventions)
        validate_conventions(resolved_conventions, manifest)
    elif managed_state.conventions is not None:
        unavailable = managed_state.conventions - known_conventions(manifest)
        if unavailable:
            raise ValueError(format_unavailable_conventions_error(unavailable))
        resolved_conventions = managed_state.conventions
    elif managed_state.exists:
        # Schema 1: the one small pre-5.0 bridge. Require an explicit choice
        # rather than inferring one from the filesystem.
        raise ValueError(PRE_5_0_STATE_ERROR)
    else:
        resolved_conventions = frozenset()

    selected = assets_for_profile(manifest, profile, resolved_conventions)

    requested_groups: set[str] = set()
    if scaffold_project_files:
        requested_groups.add("project")
    if scaffold_github_templates:
        requested_groups.add("github")
    if scaffold_editorconfig:
        requested_groups.add("editorconfig")

    group_scaffold_assets = [
        asset
        for asset in selected
        if asset.asset_type == "template" and asset.scaffold_group in requested_groups
    ]
    optional_by_name = {
        scaffold_name(asset): asset
        for asset in selected
        if asset.asset_type == "template" and asset.scaffold_group == "optional"
    }
    named_scaffold_assets: list[Asset] = []
    for name in scaffold_names:
        asset = optional_by_name.get(name)
        if asset is None:
            available = ", ".join(sorted(optional_by_name)) or "none for this profile"
            raise ValueError(f"Unknown or unavailable scaffold '{name}' for profile '{profile}'. Available: {available}")
        named_scaffold_assets.append(asset)
    scaffold_assets = tuple(dict.fromkeys(group_scaffold_assets + named_scaffold_assets))

    for asset in selected:
        validate_managed_destination(target_root, asset)
    for asset in scaffold_assets:
        validate_scaffold_destination(target_root, asset)
    state_path = safe_child(target_root, manifest.state_file, "managed state path")
    validate_parent_directory(state_path, target_root, "Managed state")

    prune_actions, tombstones = prune_stale_assets(target_root, manifest, selected, managed_state, dry_run)
    actions: list[SyncAction] = list(prune_actions)
    actions.extend(copy_asset(source_root, target_root, asset, dry_run) for asset in selected)
    for asset in scaffold_assets:
        upgrade_action = upgrade_scaffold_asset(source_root, target_root, asset, dry_run)
        actions.append(
            upgrade_action if upgrade_action is not None else scaffold_asset(source_root, target_root, asset, dry_run)
        )
    actions.append(
        write_managed_state(
            source_root,
            target_root,
            manifest,
            profile,
            resolved_conventions,
            selected,
            tombstones,
            dry_run,
        )
    )
    return actions


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Synchronize a managed repo-seed profile and optionally scaffold project files."
    )
    parser.add_argument(
        "--source",
        help="Pack directory containing manifest.json and files/. Auto-detected when run from an extracted pack.",
    )
    parser.add_argument("--target", default=".", help="Target repository. Defaults to the current directory.")
    parser.add_argument(
        "--profile",
        help="Profile name (minimal, library, app, game). Required on first sync; later syncs reuse the recorded profile when omitted.",
    )
    parser.add_argument(
        "--conventions",
        help=(
            "Comma-separated language/tool convention ids to install, e.g. 'csharp,unity' "
            "(known: csharp, python, scripts, shell, unity). Later syncs reuse the recorded "
            "selection when omitted, except a pre-5.0 state, which requires this once."
        ),
    )
    parser.add_argument(
        "--scaffold-project-files",
        action="store_true",
        help="Create missing baseline project files (README, CHANGELOG) or upgrade verified unchanged Markdown.",
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
    parser.add_argument(
        "--scaffold",
        action="append",
        metavar="NAME",
        dest="scaffold_names",
        help=(
            "Create one on-demand project document scaffold by name: architecture, fsd, gdd, "
            "user-guide (only those available for the selected profile can be created). "
            "May be passed multiple times."
        ),
    )
    parser.add_argument("--dry-run", action="store_true", help="Validate and show operations without writing files.")
    parser.add_argument(
        "--version",
        action="store_true",
        help="Show the detected pack version and exit.",
    )
    parser.add_argument(
        "--audit",
        action="store_true",
        help="Report drift diagnostics for the target without writing any files.",
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
        conventions: frozenset[str] | None = None
        if args.conventions is not None:
            conventions = frozenset(item.strip() for item in args.conventions.split(",") if item.strip())
            validate_conventions(conventions, manifest)
        if args.audit:
            report = audit_target(source_root, target_root, args.profile, conventions)
            print(f"source         {source_root}")
            print(f"target         {target_root}")
            for line in report:
                print(line)
            return 0
        if args.profile:
            profile = args.profile
        else:
            state = read_managed_state(target_root, manifest)
            if not state.exists or state.profile not in manifest.profiles:
                raise ValueError(
                    "No reusable project profile is recorded; pass --profile with "
                    "minimal, library, app, or game"
                )
            profile = state.profile
        actions = synchronize(
            source_root=source_root,
            target_root=target_root,
            profile=profile,
            conventions=conventions,
            scaffold_project_files=args.scaffold_project_files,
            scaffold_github_templates=args.scaffold_github_templates,
            scaffold_editorconfig=args.scaffold_editorconfig,
            scaffold_names=tuple(args.scaffold_names or ()),
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
