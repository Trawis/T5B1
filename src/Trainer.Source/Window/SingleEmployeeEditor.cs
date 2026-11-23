namespace Trainer_v5.Window
{
	// The mechanics the single-employee editors share: one window, the employee it edits, and the guard that keeps the
	// game's change events (raised when Refresh sets a toggle) from being taken as edits. Not generic: the game's compiler
	// emits invalid IL for fields of generic classes.
	internal abstract class SingleEmployeeEditor
	{
		protected GUIWindow Window;
		protected EmployeeTarget Target;
		protected bool IsRefreshing;

		public void Show()
		{
			if (EmployeeTarget.RequireOne() == null)
				return;

			if (Window == null)
				Window = CreateWindow();
			else
				Window.Toggle();
			Refresh();
		}

		public void Refresh()
		{
			TargetSource source;
			if (!EmployeeTarget.TryResolveFor(Window, out Target, out source))
				return;

			Window.SetTitle(Title(EmployeeTarget.Describe(Target, source)));

			IsRefreshing = true;
			try
			{
				ShowEmployee(Target == null ? null : Target.Employee);
			}
			finally
			{
				IsRefreshing = false;
			}
		}

		// The controls are bound to one game's lists, so a new game builds the window again.
		public void Reset()
		{
			Window = null;
		}

		// The employee an edit may change, or null: during a refresh, or after telling the player to refresh.
		protected Employee EditableEmployee()
		{
			if (IsRefreshing)
				return null;

			if (Target == null || !Target.IsStillValid())
			{
				Notification.ShowError(SharedMessages.EmployeeChangedPressRefresh());
				return null;
			}

			return Target.Employee;
		}

		protected abstract string Title(string employee);

		protected abstract GUIWindow CreateWindow();

		// Sets the controls to the employee's values; null when there is no employee to show.
		protected abstract void ShowEmployee(Employee employee);
	}
}
