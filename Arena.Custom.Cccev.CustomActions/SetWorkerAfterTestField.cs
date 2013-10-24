/**********************************************************************
* Description:	Workflow Action to set the worker based on a particular
*               field value from the assignment.
*				
* Created By:	Nick Airdo @ Central Christian Church AZ (Cccev)
* Date Created:	11/20/2012 12:59:59 PM
*
* $Workfile: SetWorkerAfterTestField.cs $
* $Revision: 3 $ 
* $Header: /trunk/Arena.Custom.Cccev/Arena.Custom.Cccev.CustomActions/SetWorkerAfterTestField.cs   3   2012-11-29 15:12:54-07:00   nicka $
* 
* $Log: /trunk/Arena.Custom.Cccev/Arena.Custom.Cccev.CustomActions/SetWorkerAfterTestField.cs $
*  
*  Revision: 3   Date: 2012-11-29 22:12:54Z   User: nicka 
*  Rename field settings because Arena does not support categorization in the 
*  UI like they do in Module Settings. 
*  
*  Revision: 2   Date: 2012-11-29 21:58:59Z   User: nicka 
*  Add ability to notify requester and new worker when a new worker is 
*  assigned 
*  
*  Revision: 1   Date: 2012-11-20 23:32:20Z   User: nicka 
*  New WF action that can change the worker based on a value one of the 
*  assignments fields. 
**********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Arena.Assignments;
using Arena.Core;
using Arena.Core.Communications;
using Arena.Portal;
using Arena.Utility;

namespace Arena.Custom.Cccev.CustomActions
{
	[Description( "[CCCEV] Set Worker After Testing Field" )]
	[Serializable]
	public class SetWorkerAfterTestField : WorkFlowAction
	{
		[AssignmentFieldListSetting( "Field", "Field to evaluate", true ), Category( "Field Testing" )]
		public string FieldSetting
		{
			get
			{
				return base.Setting("Field", "", true);
			}
		}
		[TextSetting( "Field Test Value", "Value to test for", true ), Category( "Field Testing" )]
		public string TestValueSetting
		{
			get
			{
				return base.Setting("TestValue", "", true);
			}
		}
		[PersonSetting( "New Worker", "The new worker to set assignment to if selected field equals the Test Value.", true ), Category( "Field Testing" )]
		public string WorkerPersonSetting
		{
			get
			{
				return base.Setting( "WorkerPerson", "-1", true );
			}
		}

		[BooleanSetting( "Notify Requester", "Should requester be notified when a NEW worker is assigned?", false, true ), Category("Notification")]
		public string NotifyRequesterSetting
		{
			get
			{
				return base.Setting( "NotifyRequester", "true", false );
			}
		}

		[BooleanSetting( "Notify Worker", "Should NEW worker be notified when worker is assigned?", false, true ), Category( "Notification" )]
		public string NotifyWorkerSetting
		{
			get
			{
				return base.Setting( "NotifyWorker", "true", false );
			}
		}

		public override bool PerformAction(Assignment assignment, Person currentPerson)
		{
			bool result;
			try
			{
				CustomFieldValue customFieldValue = assignment.FieldValues.FindById(Convert.ToInt32(this.FieldSetting));
				if ( customFieldValue != null && customFieldValue.SelectedValue.Trim() == this.TestValueSetting.Trim() )
				{
					foreach (AssignmentTypeWorker worker in assignment.AssignmentType.Workers)
					{
						if ( worker.PersonID.ToString() == this.WorkerPersonSetting)
						{
							assignment.WorkerPersonId = worker.PersonID;
							assignment.Save( "SetWorkerAfterTestField", null );
							NotifyNewWorker( assignment, currentPerson );
							NotifyRequesterOfNewWorker( assignment, currentPerson ); 
							break;
						}
					}
				}
				result = true;
			}
			catch (Exception ex)
			{
				assignment.AddNote( "Exception", ex.Message, false, null, "SetWorkerAfterTestField" );
				result = false;
			}
			return result;
		}

		private void NotifyRequesterOfNewWorker( Assignment assignment, Person currentPerson )
		{
			if ( assignment.RequesterPersonId != -1 && bool.Parse( this.NotifyRequesterSetting ) )
			{
				AssignmentEntryRequesterEmail assignmentEntryRequesterEmail = new AssignmentEntryRequesterEmail();
				if ( currentPerson != null && currentPerson.Emails.FirstActive != string.Empty )
				{
					assignmentEntryRequesterEmail.Template.Sender = currentPerson.FullName;
					assignmentEntryRequesterEmail.Template.SenderEmail = currentPerson.Emails.FirstActive;
				}
				assignmentEntryRequesterEmail.Send( assignment );
			}
		}

		private void NotifyNewWorker( Assignment assignment, Person currentPerson )
		{
			if ( assignment.WorkerPersonId != -1 && bool.Parse( this.NotifyWorkerSetting ) )
			{
				AssignmentEntryWorkerEmail assignmentEntryWorkerEmail = new AssignmentEntryWorkerEmail();
				if ( currentPerson != null && currentPerson.Emails.FirstActive != string.Empty )
				{
					assignmentEntryWorkerEmail.Template.Sender = currentPerson.FullName;
					assignmentEntryWorkerEmail.Template.SenderEmail = currentPerson.Emails.FirstActive;
				}
				assignmentEntryWorkerEmail.Send( assignment );
			}
		}
	}
}
