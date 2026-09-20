# Library Compatibility

**Status**: Active
**Last Updated**: 2026-09-20

This document records the deterministic identity of the vendored Software Inc
/ Unity reference assemblies under `Trainer_v5/Trainer.Libraries/` and the
procedure for updating them.

## Vendored reference set

`Trainer_v5.csproj` currently references exactly these six files from
`Trainer_v5/Trainer.Libraries/` (`Private=False`; the game supplies them at
runtime, they are not shipped with the mod):

- `Assembly-CSharp.dll`
- `Assembly-CSharp-firstpass.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.TextRenderingModule.dll`
- `UnityEngine.UI.dll`

## Software Inc / Unity version identity

**No exact Software Inc build number can be established from the vendored
files themselves.** Their embedded PE version resources
(`FileVersion`/`ProductVersion`) are either `0.0.0.0` or otherwise
uninformative placeholders, and none of the assemblies contain a
recognizable Unity engine version string (e.g. `20xx.x.xxfx`). This
limitation is intentional to document rather than paper over: do not treat
any specific build number as confirmed unless a future update to this
document cites concrete evidence for it.

The best-evidenced compatibility association available is indirect, from
source code rather than the binaries' own metadata:

- `Assembly-CSharp.dll` contains a `SoftwareProduct.MarketShare` member
  (confirmed present via a raw string scan of the assembly), which is the
  API the trainer's `MaxMarketShare`/`AutoMaxMarketShare` features require
  and gate behind the `SWINCBETA1_7`/`1_8`/`1_9`/`1_10` preprocessor symbols
  (see `docs/project/architecture.md`).
- The build-configuration matrix established in #119 targets Beta `1.7` as
  the actively maintained Software Inc Beta 1 sub-version (`Debug`/`Release`
  both define `SWINCBETA1_7`).

Taken together, this vendored assembly snapshot is reasonably associated
with **Software Inc Beta 1.7** (or a build compatible with it), but this is
an inference from what the code requires and targets, not a confirmed exact
game build number. If a more precise identity becomes available (e.g. from a
future game update's changelog or a build number surfaced in-game), update
this section with that evidence.

- **Runtime/CLR**: all six files are 32-bit (`x86`/`AnyCPU`-style)
  Mono/.NET PE assemblies, consistent with `Trainer_v5.csproj` targeting
  `net46`.

## Deterministic identity (manifest)

Exact identity is tracked in
[`Trainer_v5/Trainer.Libraries/library-manifest.json`](../../Trainer_v5/Trainer.Libraries/library-manifest.json)
as a `{ name, sizeBytes, sha256 }` entry per file:

| File | SHA-256 |
|---|---|
| `Assembly-CSharp.dll` | `5e2ee420b8689baf89d7b7b8397c712deac856c7cb8a092047e960c524807f4a` |
| `Assembly-CSharp-firstpass.dll` | `a066f82eab18ecf63d9ee134285d4fc22c250cd943327a639a81da284ccef340` |
| `UnityEngine.dll` | `4a436f325c99de49d030bdc7d9f12d019f716da384c55266360f4fac113a40d5` |
| `UnityEngine.CoreModule.dll` | `3670754bf8aaacc634b50cbe565ea38712a244b643a1e2e214a019b737024dca` |
| `UnityEngine.TextRenderingModule.dll` | `cb9fa16373a2031d1141801dcab5fb59c6c757b2a6eeef7d6c290f5464f9e4bf` |
| `UnityEngine.UI.dll` | `57270e258e604ad27dc12e9e94eeeeb64573552170b9e7ef37c66bd82ab7b8c0` |

SHA-256 was chosen as a simple, collision-resistant, widely-available
identity check that needs no additional dependency beyond a standard hash
implementation (`Get-FileHash` in PowerShell, `sha256sum` on Linux/macOS).

## Validation

[`scripts/verify-library-compatibility.ps1`](../../scripts/verify-library-compatibility.ps1)
validates every file in the manifest against what is actually committed:

```powershell
pwsh ./scripts/verify-library-compatibility.ps1
```

It fails (non-zero exit code) and prints a clear message when:

- an expected reference DLL listed in the manifest is missing from
  `Trainer_v5/Trainer.Libraries/`; or
- a committed reference DLL's size or SHA-256 hash no longer matches the
  manifest.

This is intentionally a small, repository-native PowerShell script (the
repo's CI already runs on `windows-latest` with PowerShell available) rather
than a new external tool or heavy dependency, so it can run locally or be
added to a workflow with no additional setup.

## Update procedure

When the vendored assemblies genuinely need to change (e.g. the game
updates and the mod is verified against the new version), follow this
procedure and update this document as part of the same change:

1. **Obtain the replacement files.** Copy the six files listed above from
   the installed Software Inc game's managed-assembly folder (the same
   files the game itself loads at runtime) into
   `Trainer_v5/Trainer.Libraries/`, overwriting the existing files.
2. **Confirm expected filenames.** Verify the replacement set still matches
   the six filenames listed above exactly (no renames, no additions or
   removals) unless `Trainer_v5.csproj`'s `<Reference>`/`<Content>` entries
   are updated to match — filename drift is exactly what the validation
   script is meant to catch.
3. **Recalculate identities.** Run:
   ```powershell
   pwsh ./scripts/verify-library-compatibility.ps1 -UpdateManifest
   ```
   This recomputes each file's size and SHA-256 hash and rewrites
   `library-manifest.json`. Review the diff: every changed hash should
   correspond to a file you intentionally replaced.
4. **Build the supported configurations.** Build `Debug`, `Release`,
   `SWINCBETA1_7`, `SWINCBETA`, and `SWINCRELEASE` (see
   `docs/project/architecture.md` for the configuration matrix) against the
   new assemblies to confirm the trainer's game API surface still compiles.
5. **Update compatibility documentation.** Revise the "Software Inc / Unity
   version identity" section above with whatever evidence justifies the new
   association (do not guess an exact build number without evidence, per
   the note above), and update `docs/project/architecture.md` /
   `CHANGELOG.md` as appropriate for the change.

Hashes are *expected* to change whenever this procedure is followed
intentionally; the validation script's job is only to catch an
**unreviewed** or accidental change to a vendored assembly, not to prevent
legitimate library refreshes.
