# Git and Pull Request Guidance

**Document role**: Managed coding-agent guidance
**Sync destination**: `.agents/guidelines/git.md`

Follow repository-specific instructions and hosted settings first. This guidance
applies to agents, automation tools, and contributors using this pack.

## Branch Model Preflight

Before creating or switching branches, committing or pushing where branch
choice matters, or opening or targeting a pull request, resolve the
repository's branch model and branch-name convention. Editing files alone does
not require branch-model or hosted-state investigation.

Start with local information, and use only what you need:

- repository instructions, including applicable `AGENTS.md` files and
  `.agents/project.md`;
- the current branch and working tree when relevant;
- documented contribution rules such as `CONTRIBUTING.md`;
- existing local branch structure.

Consult remote or hosted information only when needed to resolve ambiguity.
Fetching or pruning remotes, inspecting protected branches, and reviewing
recent pull requests are not required when the model and target are clear.

If the base branch, PR target, current branch family, or branch-name convention
is unclear, stop and resolve it before branch or PR work. Unknown base branch,
wrong branch family, wrong PR target, and missing branch-name convention are
blocking. Do not create missing long-lived branches implicitly. Do not infer
GitHub Flow only because the hosted default branch is `main` or `master` when
the repository clearly uses `develop` or `dev`.

## Branch Models

- GitHub Flow: feature work starts from `main` or `master`. Normal PRs target
  `main` or `master`.
- GitFlow: feature work starts from `develop` or `dev`. Normal PRs target
  `develop` or `dev`. `main` is production. Release and hotfix branches follow
  repository-specific release rules; normal feature PRs must not target `main`.
- Unknown model: after inspection, branch from the hosted default branch with
  `feature/<short-kebab-description>` and report the fallback.

## Branches, Commits, and Pull Requests

- Use the discovered branching model and naming conventions.
- Keep commits focused: one logical change each.
- Keep PR titles and descriptions concise and use the repository template.
- Never force-push a long-lived branch, bypass protection, auto-merge, or approve your own PR.
- Do not include assistant, model, or tool names in repository history or release text.

## Merging

Merge only when the task asks for it. Pick the method by branch type:

| Merge | Method |
|---|---|
| `feature/*` into `develop` (GitFlow) or `main` (GitHub Flow) | Squash |
| `release/*` or `hotfix/*` into `main` | Merge commit |
| `main` back into `develop` after a release or hotfix | Merge commit |

- Never squash or rebase across the `develop` and `main` boundary. A merge
  commit keeps shared ancestry, so both branches stay in sync.
- Squash messages use the PR title and a short body. Follow the commit rules
  above.
- Delete merged feature, release, and hotfix branches. Never delete `develop`,
  `dev`, `main`, or `master`.
- Resolve conflicts by merging the base branch into your branch. Do not rebase
  or force-push a shared branch.
- If the repository documents a different merge method, follow it.

## Commit Messages

- Write an imperative subject of at most 72 characters, with no trailing period.
- Add a blank line and a short body when the reason is not obvious.
- Reference issues in the body or PR, such as `Closes #123`.
- Follow the repository's existing prefix style. Do not add one it does not use.

## Releases

Create release branches, tags, and version bumps only when the task asks.

- GitFlow release: branch `release/<version>` from `develop`. Allow only
  fixes, the version bump, and the changelog. Open a PR to `main` and merge
  with a merge commit. Tag the merge commit `v<version>`. Merge `main` back
  into `develop` with a merge commit, then delete the release branch.
- GitFlow hotfix: branch `hotfix/<short-kebab-description>` from `main`. Open a
  PR to `main` and merge with a merge commit. Tag a patch version. Merge `main`
  back into `develop`, or into the open `release/*` branch if one exists.
- GitHub Flow: release from `main` by tagging it. Add a release branch only if
  the repository already uses them.
- Use semantic versions when the project has a versioned public interface.
  Keep the version, changelog, and tag consistent.

Before opening a PR, confirm the source and target branches still match the
discovered model. A wrong branch family or wrong PR target is blocking.

If hosted tooling is unavailable, report the intended source branch, target, title, and validation without claiming a PR exists.
