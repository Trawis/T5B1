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

Discover the model progressively, using only what is needed. Start with local
or repository information:

- repository instructions, including applicable `AGENTS.md` files and
  `.agents/project.md`;
- the current branch and working tree when relevant;
- documented contribution rules such as `CONTRIBUTING.md`;
- existing local branch structure.

Consult remote or hosted information only when needed to resolve ambiguity.
Fetching or pruning remotes, inspecting protected branches, and reviewing
recent pull requests are not required when the model and target are already
clear.

If the base branch, PR target, current branch family, or branch-name convention
is unclear, stop and resolve it before branch or PR work. Unknown base branch,
wrong branch family, wrong PR target, and missing branch-name convention are
blocking. Do not create missing long-lived branches implicitly. Do not infer
GitHub Flow only because the hosted default branch is `main` or `master` when
the repository clearly uses `develop` or `dev`.

## Branch Models

- GitHub Flow: normal feature work starts from `main` or `master`; normal PRs
  target `main` or `master`.
- GitFlow: normal feature work starts from `develop` or `dev`; normal PRs
  target `develop` or `dev`; `main` is production; release and hotfix branches
  follow repository-specific release rules; normal feature PRs must not target
  `main`.
- Unknown model: after inspection, branch from the hosted default branch with
  `feature/<short-kebab-description>` and report the fallback.

## Branches, Commits, and Pull Requests

- Use the discovered branching model and naming conventions.
- Keep commits focused and imperative.
- Keep PR titles and descriptions concise and use the repository template.
- Never force-push a long-lived branch, bypass protection, auto-merge, or approve your own PR.
- Do not include assistant, model, or tool names in repository history or release text.

Squash short-lived feature branches when desired. When a repository uses long-lived integration and production branches such as `develop` and `main`, preserve merge commits across that boundary for releases, hotfixes, and merge-backs so both branches retain shared ancestry.

Before opening a PR, confirm the source branch and target branch still match the
discovered model. A wrong branch family or wrong PR target is blocking.

If hosted tooling is unavailable, report the intended source branch, target, title, and validation without claiming a PR exists.
