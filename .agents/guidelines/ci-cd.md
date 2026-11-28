# CI/CD Guidance

**Document role**: Managed coding-agent guidance
**Sync destination**: `.agents/guidelines/ci-cd.md`

Use this guidance for workflows, releases, deployments, or infrastructure automation.

## Boundaries

- Change CI/CD only when the task explicitly requires it.
- Preserve the provider, triggers, permissions, gates, environments, and secret references.
- Make the narrowest relevant workflow or job change.
- Do not add publishing, deployment, infrastructure, or broad write permissions implicitly.
- Never print secrets or guess secret names.
- Do not weaken required checks or approvals.

Start with checkout, dependency restore and the checks relevant to the project.
Build or publish only when needed. Add deployment only when its target, trigger,
credentials, rollback and validation are documented.

## Builds and Releases

- Build from the reviewed source commit using declared runtime and tool versions,
  dependency lockfiles where supported, and pinned external inputs. Prefer full
  commit SHAs for third-party GitHub Actions when immutability matters.
- Keep release versions, tags and published artifacts immutable. Publish a new
  version for corrections instead of replacing existing release contents.
- Record the source commit, version, build inputs and artifact SHA-256. Verify
  archive contents and checksums before publishing; publish the checksum with
  the artifact. Use signed provenance or attestations when the project requires
  them; a checksum from the same publisher is not independent authentication.
- Prefer publishing the artifact that passed validation rather than rebuilding
  it with different inputs. Application-specific acceptance, installation,
  upgrade and deployment checks remain the consuming project's responsibility.
- Use existing suitable utilities, such as the optional archive verifier in
  [project-tools](https://github.com/Trawis/project-tools), where available.
  Do not introduce a mandatory shared pipeline or runtime dependency.

## Runners and Permissions

- Default to GitHub-hosted runners for GitHub Actions. Use self-hosted runners
  only by explicit project opt-in for trusted workloads; isolate them from
  untrusted pull requests, credentials and production infrastructure. Runner
  labels select eligible runners, not automatic failover.
- Keep validation read-only wherever possible. Grant publishing and deployment
  permissions only to the jobs that need them, on trusted refs and with the
  configured approval gates. Do not execute untrusted PR code with release
  credentials or a privileged `pull_request_target` context.
- Scope secrets to the jobs and environments that require them. Prefer
  short-lived credentials where supported. Never publish secrets in artifacts
  or logs.

## Validation

- Check workflow syntax when possible.
- Run equivalent local commands when practical.
- Recheck triggers, paths, permissions, environments, and branch targets.
- State which behavior still needs hosted verification.

Update project documentation when a change affects build commands, releases, deployment, secrets, environments, or approval steps.
