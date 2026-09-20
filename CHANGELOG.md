# Changelog

All meaningful user-facing and developer-facing changes to the trainer are
documented in this file.

Newest entries first. Dates use `YYYY-MM-DD`. Versions use the semantic version
from `Helpers.Version`. All releases target *Software Inc.* Beta 1.

## Unreleased

- Fixed Auto Design End repeatedly attempting to promote finished design
  documents that still need a lead designer assigned, instead of skipping
  them until a lead designer is set.
- Fixed generated UI control names using the literal suffix `_T` instead of
  the actual control type name.
- Fixed the Trainer Settings window crashing on open when its close button
  is missing or duplicated; such cases are now logged instead.
- Fixed Auto Accept Hosting Deals aborting the rest of the per-frame trainer
  update loop when there were no server groups or deals to process.
- Fixed trainer UI button teardown on returning to the main menu throwing
  when the trainer button and skill-change button did not share the same
  initialization state.
- Fixed repeated scene loads (e.g. loading a different save while already in
  game) stacking duplicate time-event subscriptions, which caused Lock Age to
  advance employee birth dates more than once per in-game month passed.
- Fixed the LeadSpec window showing stale toggle state from the previously
  edited employee instead of refreshing to match the selected employee.
- Fixed Product stock accepting a negative number and silently wrapping it
  into a stock of billions of copies; Days per month, Product price, Product
  stock, Active users, and Add Money now reject malformed numeric input with
  an error instead of silently substituting a default value, and Days per
  month, Product price, and Active users reject out-of-range values.
- Fixed the employee detail window's trainer controls (Trait, Demand,
  Creativity, Inspiration, LeadSpec) not being reinstalled after a save
  reload or scene transition recreated the detail window.

## [5.2.7] - 2026-09-10

- Documentation restructured to the managed templates (README, CHANGELOG, and
  the architecture overview).
- Removed version-conditional compilation from the lead-specialization setters;
  they now use a single code path.
- Fixed Digital Distribution Monopol and Auto Accept Hosting Deals being
  compiled out of Release builds; they are now included in released binaries.

## [5.2.6] - 2026-05-25

- Max Market Share button: instantly sets all company products to 100% market share.
- Auto Max Market Share toggle: keeps all company products at 100% market share continuously.

## [5.2.5] - 2026-01-16

- Updated libraries.

## [5.2.4] - 2025-10-08

- Updated libraries.
- Removed Reduce Box Price, due to incompatibility.

## [5.2.3] - 2025-08-24

- Updated libraries.

## [5.2.2] - 2024-12-23

- Fixed Creativity modification in DetailWindowTrainer (muddxyii).

## [5.2.1] - 2024-11-27

- Updated libraries.
- Fixed broken features.

## [5.2.0] - 2024-11-08

- Updated libraries.
- Fixed broken features (muddxyii).

## [5.1.9] - 2024-10-13

- Updated libraries.
- Fixed Disable Stress toggle (savisitor15).

## [5.1.8] - 2024-05-25

- Confirmation for the Max Reputation feature.
- Experimental toggle.
- Updated libraries.
- UI logic refactor.

## [5.1.7] - 2023-10-23

- Experimental features column.
- New loans.
- Disable Employee smell (NoNeeds upgraded).
- Possible fix for NoSickness.
- Possible fix for FreeEmployees where an employee tried to negotiate salary even when it was zero.
- Removed deprecated features (non-fixable).

## [5.1.6] - 2023-10-22

- Auto Accept Hosting Deals feature.
- Display game version in the Trainer Settings window.
- Remove the distribution platform if the simulated company is bankrupt.
- Fixed Trainer Settings save on version increase.
- Fixed duplicated Hosting Deals when More Hosting Deals is activated.
- Fixed missing Hosting category when using the Max Reputation feature.

## [5.1.5] - 2023-10-21

- Fixed not saving new toggles.

## [5.1.4] - 2023-10-16

- 8000% efficiency option.
- Backward compatibility for Beta 1.6.
- Trainer window: toggles rearranged.
- Updated Discord invite link.
- Fixed Disable Fire Inspection feature.

## [5.1.3] - 2023-10-15

- Disable Fire Inspection feature.
- Disable Force Pause feature.
- Disable Force Freeze feature.
- Auto Research Start feature.
- Digital Distribution Monopol feature.
- Dynamic loading of software types for More Hosting Deals.

## [5.1.2] - 2023-10-14

- Unlock and Claim all Rewards feature.
- Updated libraries.

## [5.1.1] - 2023-09-11

- Added/updated localizations.
- Fixed Max Skill of employees feature.

## [5.1.0] - 2023-09-10

- Updated libraries.

## [5.0.9] - 2023-05-20

- Updated libraries.
- Fixed compiler issues.

## [5.0.8] - 2023-05-18

- Updated libraries.

## [5.0.7] - 2023-04-15

- Updated libraries.

## [5.0.6] - 2022-12-14

- Employee Demand change, Employee Lead Spec change, Employee Trait change, Console (credits: jiandy666).
- Inspiration Use (credits: progesor).
- Updated libraries.

## [5.0.5] - 2022-10-15

- Updated libraries.

## [5.0.4] - 2022-09-17

- Updated libraries.
- Typo fix.

## [5.0.3] - 2022-07-28

- Updated libraries.
- Removed Auto Distribution Deals option, due to incompatibility.

## [5.0.2] - 2022-06-11

- Updated libraries.
- Various fixes.

## [5.0.1] - 2022-04-02

- "Default" option for the efficiency dropdown(s).
- New localizations: Croatian (Trawis), German (chaikobar), Korean (dragontalk), Spanish (jesustb).
- Fixed Lock Age option.

## [5.0] - 2022-03-14

- Initial version.
