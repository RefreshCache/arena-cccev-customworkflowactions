/**********************************************************************
* Description:	Custom action to execute a stored procedure.
* Created By:   Nick Airdo @ Central Christian Church (Cccev)
* Date Created:	10/22/2013
*
* $Workfile: $
* $Revision: $ 
* $Header: $
* 
* $Log: $
* 
**********************************************************************/
using System;
using System.ComponentModel;

using Arena.Core;
using Arena.Assignments;
using Arena.Portal;
using Arena.DataLayer.Organization;
using System.Collections;

namespace Arena.Custom.Cccev.CustomActions
{
	[Serializable]
	[Description( "[CCCEV] Execute Stored Procedure" )]
	public class ExecuteStoredProcedure : WorkFlowAction
	{
		[NumericSetting( "OrganizationID", "Organization ID this action is operating under. Default is 1.", false )]
		public string OrganizationIDSetting { get { return Setting( "OrganizationID", "1", false ); } }

		[TextSetting( "Stored Procedure Name", "Name of a stored procedure", true ), Category( "Stored Procedure Name" )]
		public string StoredProcedureNameSetting { get { return base.Setting( "StoredProcedureName", "", true ); } }

		public override bool PerformAction( Assignment assignment, Person currentPerson )
		{
			try
			{
				if ( StoredProcedureNameSetting != null && ! string.IsNullOrEmpty( StoredProcedureNameSetting.Trim() ) )
				{
					// someday, maybe we'll want to pass some parameters/values via two "Settings"...
					// but doing it that way would require some assumptions about the values.
					ArrayList arrayList = new ArrayList();

					new OrganizationData().ExecuteNonQuery( StoredProcedureNameSetting.Trim(), arrayList );
				}

				return true;
			}
			catch ( System.Exception ex )
			{
				assignment.AddNote( "Exception", ex.Message, false, null, "ExecuteStoredProcedure" );
				return false;
			}
		}
	}
}
