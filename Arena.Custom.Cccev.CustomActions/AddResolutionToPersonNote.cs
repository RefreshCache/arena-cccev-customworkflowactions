/**********************************************************************
* Description:	Custom action to copy resolution text to a person's
*				note.
* Created By:   Nick Airdo @ Central Christian Church (Cccev)
* Date Created:	02/18/2011
*
* $Workfile: AddResolutionToPersonNote.cs $
* $Revision: 1 $ 
* $Header: /trunk/Arena.Custom.Cccev/Arena.Custom.Cccev.CustomActions/AddResolutionToPersonNote.cs   1   2011-02-27 21:55:17-07:00   nicka $
* 
* $Log: /trunk/Arena.Custom.Cccev/Arena.Custom.Cccev.CustomActions/AddResolutionToPersonNote.cs $
*  
*  Revision: 1   Date: 2011-02-28 04:55:17Z   User: nicka 
* 
**********************************************************************/
using System;
using System.ComponentModel;

using Arena.Core;
using Arena.Assignments;
using Arena.Portal;

namespace Arena.Custom.Cccev.CustomActions
{
	[Serializable]
	[Description( "[CCCEV] Add Assignment Resolution to a Person's Note" )]
	public class AddResolutionToPersonNote : WorkFlowAction
	{
		[NumericSetting( "OrganizationID", "Organization ID this action is operating under. Default is 1.", false )]
		public string OrganizationIDSetting { get { return Setting( "OrganizationID", "1", false ); } }

		/*[AssignmentFieldListSetting( "Person Field", "Set Worker to value of Person Field", true, "Arena.Portal.UI.FieldTypes.PersonField" )]*/
		[AssignmentFieldListSetting( "Person Field", "The person field to which you want the resolutions added as note.", true)]
		public string FieldSetting { get { return Setting( "Field", "", true ); } }

		//[RoleSetting( "View Note Role", "The role(s) that will be given view access to the person note.", false, "1", ListSelectionMode.Multiple )]
		//public string ViewNoteRoleSetting { get { return Setting( "ViewNoteRole", "1", false ); } }

		[ListFromSqlSetting( "Security Template", "The security template to apply to the note to control who can view/edit, etc.", false, "", "select template_id, name from secu_template order by name" )]
		public string SecurityTemplateSetting { get { return base.Setting( "SecurityTemplate", "", false ); } }

		public override bool PerformAction( Assignment assignment, Person currentPerson )
		{
			try
			{
				// Find the custom field, then load the person.
				CustomFieldValue customField = assignment.FieldValues.FindById( int.Parse(FieldSetting) );
				if ( customField != null && customField.SelectedValue.Trim() != string.Empty )
				{
					int personID = int.Parse( customField.SelectedValue.Trim() );
					
					PersonHistory history = new PersonHistory();
					history.PersonID = personID;
					history.HistoryType = new Lookup( SystemLookup.PersonHistoryType_User );
					history.Text = assignment.ResolutionText;
					history.SystemHistory = false;
					history.Save( int.Parse(OrganizationIDSetting), assignment.Worker.FullName );

					Person person = new Person( personID );
					AssignmentHistory assignmentHistory = new AssignmentHistory();
					assignmentHistory.AssignmentId = assignment.AssignmentId;
					assignmentHistory.Action = "Resolution copied to " + person.FullName + " as a note";
					assignmentHistory.Save( "AddResolutionToPersonNote" );

					// This will let the worker view the person note:
					history.ApplyPersonSecurity( assignment.WorkerPersonId, assignment.Worker.FullName );
					
					// Now, we'll apply the security template if one was selected.
					if ( SecurityTemplateSetting != string.Empty )
					{
						history.ApplyTemplateSecurity( int.Parse(SecurityTemplateSetting) );
					}
				}

				return true;
			}
			catch ( System.Exception ex )
			{
				assignment.AddNote( "Exception", ex.Message, false, null, "AssignRequesterAsWorker" );
				return false;
			}
		}
	}
}
