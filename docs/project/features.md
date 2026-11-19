# Features

**F1** or the **Trainer** button opens Trainer Settings. **F2** closes it.
It has three tabs:

- **Actions**: one-shot buttons. Product actions use the *Product Name Here*
  box.
- **Toggles**: saved with the game; act while on.
- **Settings**: Efficiency and Lead Efficiency (Default, 100%–8000%) and
  Discord.

Typed names must match exactly. Ambiguous company names are refused. Targets
are checked again before a delayed change. ⚠ marks a Yes/No confirmation.

## Actions

| Action | Input | Effect |
|---|---|---|
| Add Money | Amount | Adds money |
| Adjust AI Funds | Company, amount | Changes an AI company's money. ⚠ when negative; going below zero bankrupts it |
| Takeover Company | Company | Buys the company |
| Subsidiary Company | Company | Makes it your subsidiary |
| Bankrupt ⚠ | Company | Bankrupts an AI company, not your subsidiary. One-way |
| AI Bankrupt All ⚠ | – | Bankrupts every AI company except your subsidiaries. One-way |
| Clear all loans ⚠ | – | Removes loans without paying |
| Transfer Offshore Funds | – | Moves offshore money to your company |
| Max Reputation ⚠ | – | Maxes business reputation |
| Set Business Reputation | 0–6 stars | Sets business reputation |
| Days per month | Amount | Sets days per month (restart needed) |
| Unlock and Claim Rewards ⚠ | – | Completes and claims all rewards |
| Max skill of employees | – | Maxes skills and specializations |
| Reset age of employees | – | Makes every employee youngest age |
| Make All Employees Happy Now ⚠ | – | Clears negative thoughts and complaints, maxes satisfaction |
| Set Product Price / Stock / Active Users | Product, amount | Sets the value for that product (any company) |
| Max Followers | Product | Fills your unpaused in-development product's followers to the game's cap |
| Max Followers (Old) | Product | Adds a billion followers, no cap |
| Max Market Recognition | – | Maxes market recognition |
| Instant Marketing | – | Finishes active press releases |
| Sell products stock | – | Sells stock of products without users at half price |
| Fix Bugs | Product | Fixes all bugs of your product in beta |
| Instant Research | – | Finishes active research |
| Instant Patent | – | Files patents and pays for them; skips unaffordable or invalid ones |
| Instant Porting | – | Finishes active porting |
| Optimize Team Assignments ⚠ | – | Moves single-team work to the best team; previews changes |
| Extend Deadline | – | Adds one year to contract deadlines |
| Unlock all furniture | – | Unlocks furniture |
| Unlock all space | – | Unlocks all plots |
| Clean Office Now | – | Removes floor dirt in your rooms once |

## Toggles

| Toggle | Effect |
|---|---|
| Disable Needs / Disable Stress | No needs / no stress |
| Free Employees / Free Staff | No pay for employees / staff |
| Full Satisfaction | Full satisfaction; negative thoughts cleared |
| Lock Age of Employees | No ageing |
| No Vacation | No vacations |
| No Sickness | No illness |
| No Education Cost | Free education |
| Increase Walk Speed | Faster walking |
| Increase Bookshelf Skill | Bookshelves give more skill |
| More Inspiration | No inspiration cooldown |
| Auto Research Start | Starts available research |
| Auto Design End | Promotes finished designs |
| Auto Contract Progression | Promotes contract work after design and development; QA and release stay manual |
| Auto Research End | Finishes research and files patents |
| Auto Patent End | Completes finished patents |
| Auto Porting | Advances porting |
| Disable Force Pause / Force Freeze | No forced pause / freeze |
| Digital Distribution Monopol | Closes rival platforms |
| Free Print / Increase Print Speed | Free / faster printing |
| Increase Courier Capacity | Couriers carry more |
| More Hosting Deals | More hosting deals; reward paid at noon |
| Auto Accept Hosting Deals | Accepts valid waiting deals |
| No Missed Support Tickets | Tickets never expire |
| Free Marketing | Refunds daily marketing spend |
| Reduce Internet Cost | Halves ISP cost |
| No Server Cost | Free servers |
| No Insurance Cost | Refunds insurance |
| No Loan Interest | Refunds loan interest |
| No Offshore Heat | No offshore heat |
| No Founder Dividends | Refunds founder dividends |
| Unlimited Subsidiaries | No subsidiary limit |
| Disable Fire Inspection | Passes fire inspections |
| Disable Fires | No fires |
| Disable Burglars | Removes burglars |
| Disable Furniture Stealing | Nothing can be stolen |
| Full Environment | Maximum room environment |
| Full Sun Light | Rooms fully lit |
| Lock Temperature To 21 | Rooms stay at 21 °C |
| No Maintenance | No furniture maintenance |
| Noise Reduction | No noise |
| Rooms Never Dirty | Rooms stay clean |
| Free Water & Electricity | Free utilities |
| Force Lights | Lights stay on |
| Show Room Ceilings | Rooms show ceilings |

### Switching toggles off

These undo their changes: No Education Cost, Free Print, Full Environment,
Full Sun Light, Increase Walk Speed, Increase Courier Capacity, Increase Print
Speed, Reduce Internet Cost, Force Lights, Show Room Ceilings and Unlimited
Subsidiaries.

All others stop but leave their changes:

| Toggle | What stays |
|---|---|
| Free Employees | Zero salaries (saved) |
| Lock Age of Employees | Shifted birth dates (saved) |
| No Vacation | Postponed vacations |
| Full Satisfaction | Cleared thoughts |
| No Missed Support Tickets | Reset ticket ages |
| No Maintenance | Repairs (saved) |
| Bookshelf Skill, Free Water & Electricity, Noise Reduction, Furniture Stealing, Disable Fires | Furniture values until reload |
| Auto toggles, hosting toggles | Finished, accepted or added work and deals |
| Digital Distribution Monopol | Closed platforms |
| Disable Burglars, Fire Inspection | Removed visitors; passed inspection |
| Refund toggles | Refunded money |
| Others | Last values until the game changes them |

## Toolbar buttons

They act on rows selected in the native window.

**Employees** (exactly one selected, except Skills and Make Happy):

| Button | Effect |
|---|---|
| Trait | Toggles traits |
| Demand | Toggles a lead's demands |
| LeadSpec | Sets lead specialization values |
| Inspiration | Sets inspiration (0–2) |
| Founder ⚠ | Toggles Founder (unpaid, cannot be fired) |
| Skills | Sets stars (−3 to 3) or base skill (0–1) for all selected |
| Make Happy ⚠ | Make All Employees Happy Now for the selected |

**Products** (one of your software products):

| Button | Effect |
|---|---|
| Set Price / Stock / Active Users | Sets the value |
| Fix Product Bugs | Removes all bugs |
| Max Awareness | Sets awareness to maximum once |

**Companies** (one AI company, not yours or a subsidiary):

| Button | Effect |
|---|---|
| Adjust AI Funds | As the action |
| Force Bankrupt ⚠ | As Bankrupt |
| Take Over | As Takeover Company |
| Make Subsidiary | As Subsidiary Company |
| Set Reputation | Sets its reputation stars |

## Options screen

Both switches are off by default. They are stored for all saves, not in a
save.

- **Enable Experimental Features** shows the controls below. Turning it off
  switches the experimental toggles off.
- **Debug Logging** writes trace lines to the dev console.

Retired settings have no control. Old saved values are ignored.

## Experimental

Not tested in the game.

| Control | Effect |
|---|---|
| No Forgetful Skill Loss (toggle) | Forgetful employees keep their specializations |
| Auto Instant Patent (toggle) | Instant Patent every minute |
| Auto Instant Research (toggle) | Instant Research every minute |
| Auto Max Followers (toggle) | Max Followers for beta products every hour |
| Amazing Compatibility ⚠ | Maxes personality compatibility. **Permanent; no undo.** |
| Hire Employees ⚠ | Hires 1–10 candidates of one role; pays their demands |
| Skip 1 Hour, Skip 1 Day ⚠, Skip To Hour ⚠ | Advances the clock; staff do not work during the skip |

Amazing Compatibility, Hire Employees and the skips are not available in
multiplayer.
