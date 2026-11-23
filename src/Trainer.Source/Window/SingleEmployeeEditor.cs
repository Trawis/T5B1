namespace Trainer_v5.Window
{
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

		public void Reset()
		{
			Window = null;
		}

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

		protected abstract void ShowEmployee(Employee employee);
	}
}
