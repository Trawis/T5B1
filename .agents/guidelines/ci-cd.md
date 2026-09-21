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

For a new small project, start with checkout, dependency restore, build, tests, and configured lint or format checks. Add publishing or deployment only when its target, trigger, credentials, rollback, and validation are documented.

## Validation

- Check workflow syntax when possible.
- Run equivalent local commands when practical.
- Recheck triggers, paths, permissions, environments, and branch targets.
- State which behavior still requires hosted verification.

Update project documentation when a change affects build commands, releases, deployment, secrets, environments, or approval steps.
