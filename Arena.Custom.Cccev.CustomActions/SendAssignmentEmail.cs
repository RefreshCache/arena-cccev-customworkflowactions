/**********************************************************************
* Description:	Adds the ability to use Assignment merge codes in the
*				RecipientSetting field.  Based nearly 100% on Arena's
*				original SendAssignmentEmail WorkFlowAction.
*				
* Created By:	Nick Airdo @ Central Christian Church of the East Valley
*				Derek Mangrum @ Central Christian Church of the East Valley 
* Date Created:	9/1/2011 12:59:59 PM
*
* $Workfile: SendAssignmentEmail.cs $
* $Revision: 2 $ 
* $Header: /trunk/Arena.Custom.Cccev/Arena.Custom.Cccev.CustomActions/SendAssignmentEmail.cs   2   2012-09-17 14:16:15-07:00   nicka $
* 
* $Log: /trunk/Arena.Custom.Cccev/Arena.Custom.Cccev.CustomActions/SendAssignmentEmail.cs $
*  
*  Revision: 2   Date: 2012-09-17 21:16:15Z   User: nicka 
*  Corrected namespace for custom version of Arena's SendAssignmentEmail 
*  action 
*  
*  Revision: 1   Date: 2011-09-01 21:01:10Z   User: nicka 
*  initial version 
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
	[Description( "[CCCEV] Send Email" )]
	[Serializable]
	public class SendAssignmentEmail : WorkFlowAction
	{
		[TextSetting( "From Name", "Name of sender", true )]
		public string FromNameSetting
		{
			get
			{
				return base.Setting( "FromName", "", true );
			}
		}
		[TextSetting( "From Address", "Email address of sender", true )]
		public string FromAddressSetting
		{
			get
			{
				return base.Setting( "FromAddress", "", true );
			}
		}
		[TextSetting( "Recipient Addresses", "Semi-colon delimited list of email addresses to send to (Note: you can use any of the assignment merge codes)", false )]
		public string RecipientSetting
		{
			get
			{
				return base.Setting( "Recipient", "", false );
			}
		}
		[AssignmentFieldListSetting( "Recipient Custom Field", "Custom Field that contains an email address or person to send to", false )]
		public string RecipientFieldSetting
		{
			get
			{
				return base.Setting( "RecipientField", "", false );
			}
		}
		[TextSetting( "CC", "Semi-colon delimited list of email addresses to carbon copy", false )]
		public string CCSetting
		{
			get
			{
				return base.Setting( "CC", "", false );
			}
		}
		[TextSetting( "BCC", "Semi-colon delimited list of email addresses to blind carbon copy", false )]
		public string BCCSetting
		{
			get
			{
				return base.Setting( "BCC", "", false );
			}
		}
		[TextSetting( "Subject", "Email Subject (Note: you can use any of the assignment merge codes)", true )]
		public string SubjectSetting
		{
			get
			{
				return base.Setting( "Subject", "", true );
			}
		}
		[TextSetting( "Body", "Email Body (Note: you can use any of the assignment merge codes)", true )]
		public string BodySetting
		{
			get
			{
				return base.Setting( "Body", "", true );
			}
		}
		public override bool PerformAction( Assignment assignment, Person currentPerson )
		{
			bool result;
			try
			{
				List<string> list = new List<string>();
				CommunicationType.ParseMergeFieldNames( list, this.SubjectSetting );
				CommunicationType.ParseMergeFieldNames( list, this.BodySetting );
				string text = this.BodySetting;
				string text2 = this.SubjectSetting;
				string text3 = this.RecipientSetting;
				foreach ( KeyValuePair<string, string> current in assignment.GetMergedValues( list, true ) )
				{
					text = text.Replace( current.Key, current.Value );
					text2 = text2.Replace( current.Key, current.Value );
					text3 = text3.Replace( current.Key, current.Value );
				}
				
				if ( this.RecipientFieldSetting != string.Empty )
				{
					CustomFieldValue customFieldValue = assignment.FieldValues.FindById( Convert.ToInt32( this.RecipientFieldSetting ) );
					if ( customFieldValue != null )
					{
						string text4 = customFieldValue.SelectedValue;
						if ( customFieldValue.FieldTypeAssemblyName.ToLower().Contains( "personfield" ) )
						{
							try
							{
								Person person = new Person( Convert.ToInt32( customFieldValue.SelectedValue ) );
								text4 = person.Emails.FirstActive;
							}
							catch
							{
							}
						}
						if ( text4.Trim() != string.Empty )
						{
							text3 = ( ( text3 == string.Empty ) ? text4 : ( text3 + ";" + text4 ) );
						}
					}
				}
				result = ArenaSendMail.SendMail( this.FromAddressSetting, this.FromNameSetting, text3, this.FromAddressSetting, this.CCSetting, this.BCCSetting, text2, text, string.Empty );
			}
			catch ( Exception ex )
			{
				assignment.AddNote( "Exception", ex.Message, false, null, "SendAssignmentEmail" );
				result = false;
			}
			return result;
		}
	}
}
