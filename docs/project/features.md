# Features

Current user-visible features. Open the trainer with the **Trainer** button
(or F1; F2 closes it). The window has three tabs and reopens on the tab last
used in the running game: **Actions** (the buttons below, in Company, Employee,
Product, Work and Environment sections), **Toggles** (the toggles below, in
Employee, Environment, Work, Company and Product sections; the page scrolls
when the screen is short) and **Settings** (the two efficiency dropdowns and
Discord). Switching tabs runs nothing and keeps typed and selected values. On
the Actions tab the *Product Name Here* input box, first in the Product section,
supplies the product name for the product actions. Set Product Price, Stock and Active
Users match the name exactly across every company (player and AI) and report a
conflict instead of guessing when several products share it; Max Followers and
Fix Bugs only look at your own company's work items.

## Buttons

Applied once when clicked. All but Discord are on the Actions tab; Discord is on
the Settings tab.

|  Feature               | Parameters             | Description                                                 |
|:----------------------:|:----------------------:|:-----------------------------------------------------------:|
| Add Money              | Amount                 | Adds money to your company (Deals category)                 |
| Add AI Funds           | Company Name, Amount   | Adds money to the specified AI company (Deals category)     |
| Max Followers          | Product Name           | Fills the followers of your active (not paused) in-development product to its current cap |
| Takeover Company       | Company Name           | Buys out the specified company                              |
| Subsidiary Company     | Company Name           | Specified company becomes your company's subsidiary         |
| Bankrupt               | Company Name           | Marks the specified AI company as going bankrupt after a Yes/No confirmation naming the company; one-way, and your own company and its subsidiaries are not accepted |
| AI Bankrupt All        | None                   | After a Yes/No confirmation showing how many AI companies are affected, marks every simulated AI company as going bankrupt except your own company and companies you own (subsidiaries); with none left it only says so |
| Fix Bugs               | Product Name           | Fixes all bugs of your product with that name that is in beta |
| Instant Research       | None                   | Immediately completes all of your currently active, unfinished research |
| Instant Porting        | None                   | Immediately completes your active porting work, one target platform at a time |
| Instant Marketing      | None                   | Immediately completes your active press release campaigns (other campaign types run continuously and have no completion to force) |
| Transfer Offshore Funds | None                  | Moves your entire offshore account balance into your company's main balance |
| Optimize Team Assignments | None                 | Reassigns single-team design documents, marketing campaigns, and porting work to whichever of your existing teams best matches the relevant role; leaves multi-team, lead-committed, and AI-managed work untouched |
| Set Product Price      | Product Name, Amount   | Sets the price of the exact-matching product (player or AI company) |
| Set Product Stock      | Product Name, Amount   | Sets the physical stock of the exact-matching product (player or AI company) |
| Set Active Users       | Product Name, Amount   | Sets the active users of the exact-matching product (player or AI company) |
| Sell products stock    | None                   | Stock of products with no active users is sold at half price|
| Max Reputation         | None (confirmation)    | Maxes business reputation across all categories after a Yes/No confirmation |
| Max market recognition | None                   | Maxes market recognition for all software types/categories  |
| Max Skill of employees | None                   | Maxes skills, specializations, and lead specializations     |
| Reset age of employees | None                   | Resets every employee to the youngest age                   |
| Days per month         | Amount                 | Changes days per month (requires restart)                   |
| Clear all loans        | None                   | Removes all loans without repaying them, after a Yes/No confirmation showing the loan count; with none it only says so |
| Extend Deadline        | None                   | Extends every contract deadline by one year                 |
| Unlock all furniture   | None                   | Unlocks all furniture                                       |
| Unlock all space       | None                   | Unlocks all plots of land                                   |
| Unlock and Claim Rewards | None                 | Completes and claims all task rewards, after a Yes/No confirmation showing how many are still open; with none it only says so |
| Discord                | None                   | Opens the Discord invite message                            |

## Dropdowns

On the Settings tab.

- **Efficiency**: Default, 100%-8000%; sets effectiveness for non-lead
  employees.
- **Lead Efficiency**: Default, 100%-8000%; sets effectiveness for lead
  employees.

## Toggles

Active for as long as they are enabled. Toggles are saved with the save game.

|  Feature                    | Description                                                   |
|:---------------------------:|:------------------------------------------------------------:|
| Disable Needs               | Employees have no needs                                      |
| Disable Stress              | Employees have no stress                                     |
| Free Employees              | You do not pay your employees                                |
| Free Staff                  | You do not pay your staff                                    |
| Full Satisfaction           | Employees are fully satisfied and negative thoughts clear    |
| Lock Age of Employees       | Employees do not age                                         |
| No Vacation                 | Employees do not go on vacation                              |
| No Sickness                 | Employees do not fall ill                                    |
| Auto Research Start         | Automatically starts research for available tech levels      |
| Digital Distribution Monopol| Shuts down rival digital distribution platforms              |
| Disable Fire Inspection     | Removes fire inspectors and passes inspections               |
| Full Environment            | Rooms have maximum furniture environment                     |
| Full Sun Light              | Rooms are fully lit                                          |
| Lock Temperature To 21      | Room temperatures are locked to 21 C                         |
| No Maintenance              | Furniture never needs maintenance                            |
| Noise Reduction             | Furniture and employees produce no noise                     |
| Rooms Never Dirty           | Rooms are always clean and odorless                          |
| No Education Cost           | Education is free                                            |
| Disable Fires               | Prevents and extinguishes fires                              |
| Disable Burglars            | Removes burglars that are present and any that appear (on enabling and every month) |
| Auto Design End             | Automatically promotes finished design documents             |
| Auto Contract Progression   | Automatically promotes contract work once its design/development is finished; QA, bug-fixing, and release stay manual |
| Auto Research End           | Automatically finishes research and files patents            |
| Auto Patent End             | Automatically completes finished patents                     |
| Increase Walk Speed         | Employees walk faster                                        |
| Disable Furniture Stealing  | Furniture cannot be stolen                                   |
| Free Print                  | Printing has no cost                                         |
| Free Water & Electricity    | Water, electricity, and gas are free                         |
| Increase Bookshelf Skill    | Bookshelves provide more skill                               |
| Increase Courier Capacity   | Couriers can carry more boxes                                |
| Increase Print Speed        | Printers print faster                                       |
| More Hosting Deals          | Generates extra hosting deals and rewards                    |
| Reduce Internet Cost        | Halves ISP cost                                             |
| No Server Cost              | Servers are free                                            |
| Disable Force Pause         | Prevents the game from force-pausing                         |
| Disable Force Freeze        | Prevents the game from force-freezing                        |
| Auto Accept Hosting Deals   | Accepts valid server deals that are waiting in the deal list; offers you rejected or filtered out are skipped, and nothing is accepted if the list cannot be identified |
| Auto Porting                | Automatically advances your active porting work toward completion |
| More Inspiration            | Inspiration can be used without cooldown                     |
| Force Lights                | Building and street lights stay on regardless of time of day |
| Show Room Ceilings          | Rooms render a ceiling mesh instead of being open from above |
| Unlimited Subsidiaries      | Removes the limit on how many companies you can take over as subsidiaries |
| No Missed Support Tickets   | Your support tickets never age out and count as missed        |
| No Insurance Cost           | Refunds your daily content insurance bill; coverage and rates are unaffected |
| No Loan Interest            | Refunds the interest portion of your monthly loan payments; principal repayment continues normally |
| Free Marketing               | Refunds your daily budget-based marketing spend; press releases and follower campaigns are unaffected |
| No Offshore Heat             | Clears offshore account heat every frame, preventing it from reaching the audit threshold |
| No Founder Dividends         | Refunds the founder-share portion of your monthly dividend payout; other shareholders are unaffected |

## Experimental options and retired settings

The game's Options screen lists the trainer under its mods and offers *Enable
Experimental Features*, off by default. The choice is stored by the game in the
mod's own `DLLMods/<mod>Setting.txt`, applies to every save game and is not
saved in one. No experimental feature exists yet, so turning it on changes
nothing today. The Trainer Settings window has no Experimental toggle and no
Test button.

The same Options screen offers *Debug Logging*, off by default and independent
of the experimental switch. When on, extra `Trainer [debug]:` trace lines (toggle
changes, retries, game ready and per-game resets) are written to the dev console
only; errors and warnings are always logged. It is stored in the same
`DLLMods/<mod>Setting.txt` and applies to every save game.

Retired settings (Reduce Box Price, Auto Max Market Share, More Creativity,
Reduce Expansion Cost and the former Experimental toggle) have no control and
no behavior; a value stored for them in an older save is ignored.

## Switching toggles off

Switching a toggle off stops its effect from being applied again. Only these
toggles also undo what they changed: No Education Cost, Free Print, Full
Environment, Full Sun Light, Increase Walk Speed, Increase Courier Capacity,
Increase Print Speed and Reduce Internet Cost. Force Lights, Show Room
Ceilings and Unlimited Subsidiaries follow the setting directly. Every other
toggle leaves its changes as described below. "Not captured" means the original
value is not recorded, so the trainer cannot put it back.

|  Toggle                      | After switching off                                           |
|:----------------------------:|:-------------------------------------------------------------|
| Increase Bookshelf Skill     | Bookshelves keep the boosted aura value; the original per-bookshelf value is not captured. The value is not part of the furniture's save data, so loading the save drops it |
| No Maintenance               | Chairs raised to high comfort keep it (previous comfort not captured; not saved, so a reload drops it); furniture that was repaired stays repaired (quality is saved) |
| Free Water & Electricity     | Furniture water and wattage stay at 0 (previous values not captured; not saved, so a reload drops them); the water, electricity and gas bills are written by the game again |
| Noise Reduction              | Furniture base noisiness stays at 0 (not captured; not saved, so a reload drops it); the noise values the game recalculates and employee noisiness are written by the game again |
| Disable Furniture Stealing   | Furniture stays unstealable (not saved, so a reload drops it) |
| Disable Fires                | The fire-starting chance of furniture stays at 0 (not saved, so a reload drops it); fires already put out stay out |
| Free Employees               | Salaries and salary demands stay at 0 and employees no longer negotiate salary; the previous values are not captured and are saved with the employee |
| Lock Age of Employees        | Employee birth dates stay shifted; ages are not restored |
| No Vacation                  | Next vacation dates stay pushed back and employees already called back from vacation stay back; the game sets new vacation dates through its own scheduling, when not determined |
| Full Satisfaction            | Cleared negative thoughts stay cleared; job satisfaction is written by the game again |
| No Missed Support Tickets    | Ticket ages stay reset |
| Auto Design End, Auto Contract Progression, Auto Research Start, Auto Research End, Auto Patent End, Auto Porting, Auto Accept Hosting Deals | Work already promoted, started, finished, patented or accepted stays so |
| More Hosting Deals           | Deals and rewards already added stay |
| Digital Distribution Monopol | Rival platforms already closed or removed stay closed; nothing in the game's code reopens an AI company's platform |
| Disable Burglars, Disable Fire Inspection | Removed burglars and inspectors stay removed; fire inspection stays marked as passed until the game next writes that flag |
| No Insurance Cost, No Loan Interest, Free Marketing, No Founder Dividends | Money already refunded stays refunded |
| Disable Needs, Disable Stress, Lock Temperature To 21, Rooms Never Dirty, No Sickness, More Inspiration, No Offshore Heat, No Server Cost, Free Staff, Disable Force Pause, Disable Force Freeze | The last applied values stay until the game writes them again; how soon that happens is not determined. Sickness, dirt and similar states already cleared stay cleared |

## Employee editing

The extra buttons added to the game's employee detail window edit the employee
it shows. **Skills** opens the skill editor for that employee; its **All selected**
option applies a write to every selected employee instead.

|  Feature       | Description                                                        |
|:--------------:|:-----------------------------------------------------------------:|
| Set Skills     | Changes specialization stars by -3 to 3 (not 0; the game keeps 0 to 3 stars) for selected roles/specs |
| Set Base Skills| Sets base role skill (0 to 1.0); also raises the employee skill ceiling to 1.0 |
| Trait          | Toggles an employee's traits                                       |
| Demand         | Toggles a lead's design demands                                    |
| LeadSpec       | Sets lead specialization values per software type                  |
| Inspiration    | Sets an employee's inspiration (0 to 2.0)                          |
| Founder        | Turns an employee's Founder flag on or off (a founder is unpaid and cannot be fired) after a Yes/No confirmation naming the employee and both states |
