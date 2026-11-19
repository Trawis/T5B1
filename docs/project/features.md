# Features

Open Trainer Settings with the **Trainer** button or **F1** (F1 also brings an
open window to the front); **F2** closes it. The window has three tabs and
reopens on the last one used in the running game:

- **Actions**: one-shot buttons, grouped by Company, Employee, Product, Work
  and Environment. The *Product Name Here* box at the top of the Product
  section supplies the name for product actions.
- **Toggles**: settings that keep acting while on, grouped the same way.
- **Settings**: the Efficiency and Lead Efficiency drop-downs (Default,
  100%–8000%) and Discord.

Names typed into actions are matched exactly. Product actions search every
company; company actions refuse an ambiguous name. Max Followers, Max
Followers (Old) and Fix Bugs only look at your own work. A target is taken
when the button is pressed, and the change applies only if it still exists
when the dialog is confirmed.

**Confirmations.** Actions marked ⚠ ask a Yes/No question first that names the
scope; No, Esc or closing changes nothing.

## Actions

One-shot: each click applies once.

| Action | Input | Effect |
|---|---|---|
| Add Money | Amount | Adds money to your company |
| Adjust AI Funds | Company, amount | Adds to or (⚠) subtracts from an AI company's money; subtracting more than it has bankrupts it at once |
| Takeover Company | Company | Buys out the company |
| Subsidiary Company | Company | Makes the company your subsidiary |
| Bankrupt ⚠ | Company | Marks an AI company as going bankrupt (one-way); not your company or its subsidiaries |
| AI Bankrupt All ⚠ | – | Same for every AI company except yours and your subsidiaries |
| Clear all loans ⚠ | – | Removes all loans without repaying them |
| Transfer Offshore Funds | – | Moves the offshore balance into your company's money |
| Max Reputation ⚠ | – | Maxes business reputation in every category |
| Set Business Reputation | Stars 0–6 | Sets your business reputation to that many stars |
| Days per month | Amount | Changes days per month (needs a restart) |
| Unlock and Claim Rewards ⚠ | – | Completes and claims all task rewards |
| Max skill of employees | – | Maxes skills, specializations and lead specializations |
| Reset age of employees | – | Resets every employee to the youngest age |
| Make All Employees Happy Now ⚠ | – | Removes negative, lawsuit and quit thoughts (not restored), clears complaints, maxes job satisfaction and adds Love Work |
| Set Product Price / Stock / Active Users | Product, amount | Sets the value of the exactly named product (any company) |
| Max Followers | Product | Fills your unpaused in-development product's followers to the game's cap |
| Max Followers (Old) | Product | Adds a billion followers per click, no limit |
| Max Market Recognition | – | Maxes market recognition for all software types |
| Instant Marketing | – | Completes your active press releases |
| Sell products stock | – | Sells the stock of products with no active users at half price |
| Fix Bugs | Product | Fixes all bugs of your product in beta |
| Instant Research | – | Completes your active research |
| Instant Patent | – | Files your patent work through the game's patent step, paying its price; skips what is expired, owned, not patentable, in a multiplayer deal or unaffordable |
| Instant Porting | – | Completes your active porting work |
| Optimize Team Assignments ⚠ | – | Moves single-team design, marketing and porting work to your best-matching team; the question previews each change and each skipped item |
| Extend Deadline | – | Extends every contract deadline by one year |
| Unlock all furniture | – | Unlocks all furniture |
| Unlock all space | – | Unlocks all plots |
| Clean Office Now | – | Removes floor dirt once from the rooms your company uses |
| Discord | – | Shows the Discord invite (Settings tab) |

## Toggles

Saved with the save game and applied while on.

| Toggle | Effect |
|---|---|
| Disable Needs / Disable Stress | Employees have no needs / stress |
| Free Employees / Free Staff | You do not pay employees / staff |
| Full Satisfaction | Employees are fully satisfied; negative thoughts are cleared |
| Lock Age of Employees | Employees do not age |
| No Vacation | Employees do not go on vacation |
| No Sickness | Employees do not fall ill |
| No Education Cost | Education is free |
| Increase Walk Speed | Employees walk faster |
| Increase Bookshelf Skill | Bookshelves give more skill |
| More Inspiration | Inspiration has no cooldown |
| Auto Research Start | Starts research for available tech levels |
| Auto Design End | Promotes finished design documents |
| Auto Contract Progression | Promotes contract work after design and development; QA, bug fixing and release stay manual |
| Auto Research End | Finishes research and files patents |
| Auto Patent End | Completes finished patents |
| Auto Porting | Advances your active porting work |
| Disable Force Pause / Disable Force Freeze | The game does not force-pause / force-freeze |
| Digital Distribution Monopol | Shuts down rival distribution platforms |
| Free Print / Increase Print Speed | Printing is free / faster |
| Increase Courier Capacity | Couriers carry more boxes |
| More Hosting Deals | Adds hosting deals and rewards; the reward is paid at noon |
| Auto Accept Hosting Deals | Accepts valid deals waiting in the deal list; skips rejected or filtered offers |
| No Missed Support Tickets | Support tickets never age out |
| Free Marketing | Refunds daily budget-based marketing spend |
| Reduce Internet Cost | Halves ISP cost |
| No Server Cost | Servers are free |
| No Insurance Cost | Refunds the daily content insurance bill |
| No Loan Interest | Refunds the interest part of loan payments |
| No Offshore Heat | Keeps offshore heat at zero |
| No Founder Dividends | Refunds the founder share of dividends |
| Unlimited Subsidiaries | No limit on subsidiaries |
| Disable Fire Inspection | Removes fire inspectors and passes inspections |
| Disable Fires | Prevents and puts out fires |
| Disable Burglars | Removes burglars (on enabling and monthly) |
| Disable Furniture Stealing | Furniture cannot be stolen |
| Full Environment | Rooms get maximum furniture environment |
| Full Sun Light | Rooms are fully lit |
| Lock Temperature To 21 | Room temperature stays at 21 °C |
| No Maintenance | Furniture never needs maintenance |
| Noise Reduction | Furniture and employees make no noise |
| Rooms Never Dirty | Rooms stay clean and odourless |
| Free Water & Electricity | Water, electricity and gas are free |
| Force Lights | Building and street lights stay on |
| Show Room Ceilings | Rooms show a ceiling |

### Switching toggles off

Switching a toggle off stops it. Only these also undo their changes: No
Education Cost, Free Print, Full Environment, Full Sun Light, Increase Walk
Speed, Increase Courier Capacity, Increase Print Speed and Reduce Internet
Cost; Force Lights, Show Room Ceilings and Unlimited Subsidiaries follow the
setting directly. The others leave their changes:

| Toggle | What stays |
|---|---|
| Free Employees | Salaries and demands stay at 0 (saved) |
| Lock Age of Employees | Shifted birth dates (saved) |
| No Vacation | Postponed vacations; called-back employees stay |
| Full Satisfaction | Cleared negative thoughts |
| No Missed Support Tickets | Reset ticket ages |
| No Maintenance | Repairs (saved); raised chair comfort until reload |
| Increase Bookshelf Skill, Free Water & Electricity, Noise Reduction, Disable Furniture Stealing, Disable Fires | Furniture values until the save is reloaded |
| Auto toggles, Auto Accept Hosting Deals, More Hosting Deals | Work promoted, started, finished, patented or accepted, and deals added |
| Digital Distribution Monopol | Closed rival platforms |
| Disable Burglars, Disable Fire Inspection | Removed burglars and inspectors; inspection stays passed until the game writes it |
| Refund toggles | Money already refunded |
| Other toggles | Last values until the game writes them again |

## Employees window buttons

Buttons above the game's Employees window act on the employees selected in
its list. Single-employee editors need exactly one selected employee.

| Button | Effect |
|---|---|
| Trait | Toggles the employee's traits |
| Demand | Toggles a lead's design demands |
| LeadSpec | Sets lead specialization values per software type |
| Inspiration | Sets inspiration (0–2) |
| Founder ⚠ | Turns the Founder flag on or off (a founder is unpaid and cannot be fired) |
| Skills | Opens Set Skills (specialization stars −3 to 3) and Set Base Skills (0–1, also raises the skill ceiling) for every selected employee |
| Make Happy ⚠ | Make All Employees Happy Now for the selected employees |

## Products window buttons

Buttons above your own Products list act on the one software product selected
there (not add-ons, frameworks, archived or foreign products).

| Button | Effect |
|---|---|
| Set Price / Set Stock / Set Active Users | As the Actions buttons, for the selected product |
| Fix Product Bugs | Removes every remaining bug of the released product |
| Max Awareness | Sets awareness to its category maximum once; it then fades normally |

## Companies window buttons

Buttons above the Companies list act on the one AI company selected there (not
your company, a subsidiary or a company already going bankrupt).

| Button | Effect |
|---|---|
| Adjust AI Funds | As the Actions button |
| Force Bankrupt ⚠ | As Bankrupt |
| Take Over | As Takeover Company |
| Make Subsidiary | As Subsidiary Company |
| Set Reputation | Sets the company's business reputation stars |

## Options screen

The game's Options screen lists the trainer with two switches, both off by
default, stored in the mod's `DLLMods/<mod>Setting.txt` for every save:

- **Enable Experimental Features** shows the controls below. Turning it off
  switches the experimental toggles off.
- **Debug Logging** writes extra `Trainer [debug]:` lines to the dev console.

Retired settings (Reduce Box Price, Auto Max Market Share, More Creativity,
Reduce Expansion Cost and the old Experimental toggle) have no control; values
stored in older saves are ignored.

## Experimental

Not tested in the game. Shown in an Experimental section of the Actions and
Toggles tabs only while the option is on.

| Control | Type | Effect |
|---|---|---|
| No Forgetful Skill Loss | Toggle | Monthly, pushes the specialization-loss timer of Forgetful employees 12 months ahead; pushed timers are saved and lost stars are not restored |
| Auto Instant Patent | Toggle | Every minute, does what Instant Patent does, silently |
| Auto Instant Research | Toggle | Every minute, does what Instant Research does, silently |
| Auto Max Followers | Toggle | Every hour, fills followers of your unpaused beta products to the cap |
| Amazing Compatibility ⚠ | Action | Raises every personality pair below the game's maximum (2) to 2. **Permanent in this save and its autosaves; no undo; back up first.** Not in multiplayer |
| Hire Employees ⚠ | Action | Hires 1–10 medium-wage candidates of a chosen role through the game's own hiring steps, paying their upfront demands; skips candidates your reputation or money cannot cover. Not in multiplayer |
| Skip 1 Hour, Skip 1 Day ⚠, Skip To Hour ⚠ | Action | Advances the clock hour by hour through the game's own updates (salaries, bills, deadlines, autosaves). Staff in the office do no work during skipped hours. Not in multiplayer or while the game holds time |
