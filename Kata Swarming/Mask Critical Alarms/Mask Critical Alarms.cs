namespace MaskCriticalAlarms
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Enums;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			var connection = engine.GetUserConnection();

			var inputParameter = engine.GetScriptParam("ElementID")?.Value;
			var elementID = ElementID.FromString(inputParameter);

			if (elementID is null)
			{
				engine.ExitFail("No valid input ElementID");
				return;
			}

			var getActiveAlarms = new GetActiveAlarmsMessage()
			{
				DataMinerID = elementID.DataMinerID,
				ElementID = elementID.EID,
			};

			var response = connection.HandleMessage(getActiveAlarms).FirstOrDefault() as ActiveAlarmsResponseMessage;

			if(response is null)
			{
				engine.ExitFail("Failed to get active alarms");
				return;
			}

			var criticalAlarms = response.ActiveAlarms.Where(alarm => alarm.Severity == "Critical").ToArray();

			var maskRequests = new List<SetAlarmStateMessage>();
			foreach (var alarm in criticalAlarms)
			{
				var maskRequest = new SetAlarmStateMessage();
				maskRequest.DataMinerID = alarm.DataMinerID;
				maskRequest.AlarmId = alarm.AlarmID;
				maskRequest.DesiredStatus = AlarmUserStatus.Mask;
				maskRequests.Add(maskRequest);
			}

			connection.HandleMessages(maskRequests.ToArray());
		}
	}
}
