namespace EnableSwarming
{
	using System;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Swarming;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Label = Skyline.DataMiner.Utils.InteractiveAutomationScript.Label;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private InteractiveController _app;
		private IEngine _engine;

		/// <summary>
		/// The Script entry point.
		/// IEngine.ShowUI();.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				_engine = engine;
				_app = new InteractiveController(engine);

				engine.SetFlag(RunTimeFlags.NoKeyCaching);
				engine.Timeout = TimeSpan.FromHours(10);

				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (ScriptForceAbortException)
			{
				throw;
			}
			catch (ScriptTimeoutException)
			{
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				throw;
			}
			catch (Exception ex)
			{
				engine.Log($"Run|Something went wrong: {ex}");
				ShowExceptionDialog(engine, ex);
			}
		}

		private void RunSafe(IEngine engine)
		{
			var response = CheckPrerequisites(engine, false);
			var prerequisiteDialog = new PrerequisiteDialog(engine, response);
			_app.Run(prerequisiteDialog);
		}

		private static SwarmingPrerequisitesCheckResponse CheckPrerequisites(IEngine engine, bool analyzeAlarmIds)
		{
			var req = new SwarmingPrerequisitesCheckRequest()
			{
				AnalyzeAlarmIDUsage = analyzeAlarmIds,

				// do alarmid analysis on local agent only (not really necessary, just gives a better summary)
				DataMinerID = analyzeAlarmIds ? engine.GetUserConnection().ServerDetails.AgentID : -1,
			};

			var resp = engine.SendSLNetMessages(new[] { req });

			if (resp == null || resp.Length == 0)
				throw new Exception($"SwarmingPrerequisitesCheckResponse is null or empty");

			var dmaResponses = resp.OfType<SwarmingPrerequisitesCheckResponse>().ToArray();
			if (dmaResponses.Length != 1)
				throw new Exception($"{nameof(dmaResponses)} does not contain exactly 1 response");

			return dmaResponses.First();
		}

		private void ShowExceptionDialog(IEngine engine, Exception exception)
		{
			ExceptionDialog exceptionDialog = new ExceptionDialog(engine, exception);
			exceptionDialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong.");
			if (_app.IsRunning)
				_app.ShowDialog(exceptionDialog);
			else
				_app.Run(exceptionDialog);
		}

		private class PrerequisiteDialog : Dialog
		{
			private IEngine _engine;
			private int _widgetRowIdx = 0;
			private Button _alarmIdButton;

			public PrerequisiteDialog(IEngine engine, SwarmingPrerequisitesCheckResponse resp) : base(engine)
			{
				_engine = engine;

				// Set title
				Title = "Swarming Prerequisites";
				MinWidth = 600;

				// Init widgets
				var sb = new StringBuilder();
				sb.AppendLine("Swarming Prerequisites:");
				sb.AppendLine();
				sb.AppendLine("More info at aka.dataminer.services/enable-swarming");
				sb.AppendLine();
				sb.AppendLine("1) Static requirements:");

				AddLabelWidget(sb.ToString());

				AddPrerequisiteWidget("No failover", resp.SupportedDMS);
				AddPrerequisiteWidget("Shared database", resp.SupportedDatabase);
				AddPrerequisiteWidget("No central database", resp.CentralDatabaseNotConfigured);
				AddPrerequisiteWidget("No legacy reports and dashboards", resp.LegacyReportsAndDashboardsDisabled);
				AddPrerequisiteWidget("No incompatible enhanced services", resp.NoIncompatibleEnhancedServicesOnDMS);

				if (!resp.SatisfiesPrerequisites)
				{
					sb = new StringBuilder();
					sb.AppendLine();
					sb.AppendLine("Result summary:");
					sb.AppendLine();
					sb.AppendLine(resp.Summary);

					AddLabelWidget(sb.ToString());

					var secretButton = new Button(string.Empty);
					secretButton.Width = 1;
					secretButton.Height = 1;
					AddWidget(secretButton, _widgetRowIdx++, 0);

					return; // dont do alarmid usage without meeting static requirements
				}

				sb = new StringBuilder();
				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("2) AlarmID usage");
				sb.AppendLine();
				sb.AppendLine("For the next part we will analyze the AlarmID usage");
				sb.AppendLine("in scripts (automation/GQI) and protocol QActions.");
				sb.AppendLine("Note that dependencies (e.g. nugets) are not checked.");
				sb.AppendLine("This can take a while, up to several minutes.");
				sb.AppendLine("Keep this window open and wait for the results to appear.");
				sb.AppendLine();
				AddLabelWidget(sb.ToString());

				_alarmIdButton = new Button("Analyze");
				_alarmIdButton.Pressed += AlarmIdButton_Pressed;
				AddWidget(_alarmIdButton, _widgetRowIdx++, 0);
			}

			private void AlarmIdButton_Pressed(object sender, EventArgs e)
			{
				_alarmIdButton.IsEnabled = false;

				var response = Script.CheckPrerequisites(_engine, true);

				AddLabelWidget(string.Empty);

				AddPrerequisiteWidget("No obsolete alarm id usage in protocol QActions", response.NoObsoleteAlarmIdUsageInProtocolQActions);
				AddPrerequisiteWidget("No obsolete alarm id usage in scripts", response.NoObsoleteAlarmIdUsageInScripts);

				if (!response.NoObsoleteAlarmIdUsageInProtocolQActions || !response.NoObsoleteAlarmIdUsageInScripts)
				{
					var sb = new StringBuilder();
					sb.AppendLine();
					sb.AppendLine();
					sb.AppendLine("Alarm ID usage summary: ");
					sb.AppendLine();

					sb.AppendLine(response.Summary);
					AddLabelWidget(sb.ToString());
				}

				if (response.SatisfiesPrerequisites)
				{
					var sb = new StringBuilder();
					sb.AppendLine();
					sb.AppendLine("All prerequisites are met.");
					sb.AppendLine("You can now enable Swarming.");
					sb.AppendLine("This involves another prerequisite check so this will take as long as the check above.");
					sb.AppendLine("Keep in mind that enabling swarming involves a full cluster wide DataMiner restart.");
					sb.AppendLine();

					AddLabelWidget(sb.ToString());

					var enableButton = new Button("Restart DMS and enable Swarming");
					enableButton.Pressed += EnableButton_Pressed;
					AddWidget(enableButton, _widgetRowIdx++, 0);
				}
			}

			private void EnableButton_Pressed(object sender, EventArgs e)
			{
				_engine.SendSLNetMessage(new EnableSwarmingRequest());
			}

			private void AddPrerequisiteWidget(string label, bool value)
			{
				AddWidget(new Label(label + ": "), _widgetRowIdx, 0);
				AddWidget(new Label(value ? "Ok" : "Not Ok"), _widgetRowIdx++, 1);
			}

			private void AddLabelWidget(string label)
			{
				AddWidget(new Label(label), _widgetRowIdx++, 0, 1, 2);
			}
		}
	}
}
