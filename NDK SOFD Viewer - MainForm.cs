using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NDK.Framework;

namespace NDK.SofdViewer {

	public partial class MainForm : BaseForm {
		private System.Windows.Forms.Timer employeeSearchThreadTimer = null;
		private ManualResetEvent employeeSearchThreadShutdownEvent = null;
		private Thread employeeSearchThread = null;
		private SofdEmployee employeeLastShown = null;

		private System.Windows.Forms.Timer organizationSearchThreadTimer = null;
		private ManualResetEvent organizationSearchThreadShutdownEvent = null;
		private Thread organizationSearchThread = null;
		private SofdOrganization organizationLastShown = null;

		#region Constructors.
		public MainForm() {
			InitializeComponent();
		} // MainForm
		#endregion

		#region Override and event handling methods.
		/// <summary>
		/// Initialize the application and form.
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			try {
				// Invoke base method.
				base.OnLoad(e);

				// Add build date to title.
				//Assembly.GetExecutingAssembly().
				FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
				base.Text += " (" + fileInfo.LastWriteTime.ToString("dd.MM.yy") + ")";

				// Hide tabs.
				this.MainFormPages.Appearance = TabAppearance.FlatButtons;
				this.MainFormPages.ItemSize = new Size(0, 1);
				this.MainFormPages.SizeMode = TabSizeMode.Fixed;

				// Initialize action buttons.
				this.actionEmployeeList.Image = this.GetResourceImage("User Search.png");
				this.actionEmployeeProperties.Image = this.GetResourceImage("User Id.png");
				this.actionEmployeeCopyProperties.Image = this.GetResourceImage("Copy.png");
				this.actionOrganizationList.Image = this.GetResourceImage("Org Search.png");
				this.actionOrganizationProperties.Image = this.GetResourceImage("Org Id.png");
				this.actionActiveDirectory.Image = this.GetResourceImage("Book.png");
				this.actionActiveDirectoryEnableUser.Image = this.GetResourceImage("User Green.png");
				this.actionActiveDirectoryDisableUser.Image = this.GetResourceImage("User Red.png");
				this.actionActiveDirectoryExpireUser.Image = this.GetResourceImage("User Orange.png");
				this.actionActiveDirectoryResetUser.Image = this.GetResourceImage("User Yellow.png");
				this.actionActiveDirectoryShowUser.Image = this.GetResourceImage("Book.png");
				this.actionSofdDirectory.Image = this.GetResourceImage("Book.png");
				this.actionSofdDirectoryEditEmployee.Image = this.GetResourceImage("User Green.png");

				// Initialize employee and organization.
				this.EmployeeInitialize();
				this.OrganizationInitialize();

				// Show the employee search tab.
				this.MainFormPages.SelectTab(this.employeeListPage);

				// Search.
				this.EmployeeSearchStartThreadFromFilterText(this, e);
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Application Loading", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // OnLoad

		protected override void OnClosing(CancelEventArgs e) {
			try {
				// Finalize employee and organization.
				this.EmployeeFinalize();
				this.OrganizationFinalize();
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Application Closing", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				// Invoke base method.
				e.Cancel = false;
				base.OnClosing(e);
			}
		} // OnClosing
		#endregion

		#region Active employee/organization methods.
		private SofdEmployee GetActiveEmployee() {
			if ((this.MainFormPages.SelectedTab == this.employeeListPage) && (this.employeeList.SelectedRows.Count == 1)) {
				return (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;
			}
			if ((this.MainFormPages.SelectedTab == this.employeePropertyPage) && (this.employeeLastShown != null)) {
				return this.employeeLastShown;
			}

			return null;
		} // GetActiveEmployee

		private SofdEmployee GetActiveLeader(Boolean promptWhenActiveEmployeeIsLeader = false) {
			if ((this.MainFormPages.SelectedTab == this.employeeListPage) ||
				(this.MainFormPages.SelectedTab == this.employeePropertyPage)) {
				if (this.GetActiveEmployee() != null) {
					if ((promptWhenActiveEmployeeIsLeader == true) &&
						(this.GetActiveEmployee().Leder == true) &&
						(MessageBox.Show(
							String.Format("The active employee is a leader. Select YES to show the employees who have {0} as leader, or NO to show users who have the same leader as {0}.", this.GetActiveEmployee().Navn),
							"Active employee is leader",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question
						) == DialogResult.Yes)) {
						return this.GetActiveEmployee();
					} else {
						return this.GetActiveEmployee().GetNearestLeader();
					}
				}
			}
			if ((this.MainFormPages.SelectedTab == this.organizationListPage) ||
				(this.MainFormPages.SelectedTab == this.organizationPropertyPage)) {
				if (this.GetActiveOrganization() != null) {
					return this.GetActiveOrganization().GetLeader();
				}
			}

			return null;
		} // GetActiveLeader

		private SofdOrganization GetActiveOrganization() {
			if ((this.MainFormPages.SelectedTab == this.organizationListPage) && (this.organizationList.SelectedRows.Count == 1)) {
				return (SofdOrganization)this.organizationList.SelectedRows[0].DataBoundItem;
			}
			if ((this.MainFormPages.SelectedTab == this.organizationPropertyPage) && (this.organizationLastShown != null)) {
				return this.organizationLastShown;
			}
			if (this.GetActiveEmployee() != null) {
				return this.GetActiveEmployee().GetOrganisation();
			}

			return null;
		} // GetActiveOrganization

		private void SetActiveEmployee(SofdEmployee employee) {
			this.employeeLastShown = employee;
		} // SetActiveEmployee

		private void SetActiveOrganization(SofdOrganization organization) {
			this.organizationLastShown = organization;
		} // SetActiveOrganization
		#endregion

		#region Action methods.
		private void ActionUpdate(Object sender, EventArgs e) {
			try {
				// Get the employee and organization list selection count.
				Int32 employeeListSelectionCount = this.employeeList.SelectedRows.Count;
				Int32 organizationListSelectionCount = this.organizationList.SelectedRows.Count;

				// Get the active employee and organization.
				SofdEmployee employee = this.GetActiveEmployee();
				SofdOrganization organization = this.GetActiveOrganization();
				SofdEmployee leader = this.GetActiveLeader();

				// Get the Active Directory user associated with the active employee.
				AdUser user = null;
				if (employee != null) {
					user = this.GetUser(employee.AdBrugerNavn);
				}

				// Enable buttons.
				this.actionEmployeeList.Enabled = (this.MainFormPages.SelectedTab != this.employeeListPage);
				this.actionEmployeeProperties.Enabled = ((this.MainFormPages.SelectedTab == this.employeeListPage) && (employeeListSelectionCount == 1));
				this.actionEmployeeCopyProperties.Enabled = ((this.MainFormPages.SelectedTab == this.employeeListPage) || (this.MainFormPages.SelectedTab == this.employeePropertyPage));
				this.actionOrganizationList.Enabled = (this.MainFormPages.SelectedTab != this.organizationListPage);
				this.actionOrganizationProperties.Enabled = ((this.MainFormPages.SelectedTab == this.organizationListPage) && (organizationListSelectionCount == 1));

				this.actionActiveDirectoryEnableUser.Enabled = ((user != null) && ((user.Enabled == false) || (user.IsAccountLockedOut() == true)));
				this.actionActiveDirectoryDisableUser.Enabled = ((user != null) && (user.Enabled == true));
				this.actionActiveDirectoryExpireUser.Enabled = ((user != null) && (user.Enabled == true));
				this.actionActiveDirectoryResetUser.Enabled = (user != null);
				this.actionActiveDirectoryShowUser.Enabled = (user != null);
				this.actionSofdDirectoryEditEmployee.Enabled = ((employee != null) && (employee.Aktiv == true));
				this.actionSofdDirectoryShowLeader.Enabled = (leader != null);
				this.actionSofdDirectoryShowOrganization.Enabled = (organization != null);
				this.actionSofdDirectoryListEmployees.Enabled = (leader != null);
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // ActionUpdate

		private void ActionEmployeeShowListClick(Object sender, EventArgs e) {
			try {
				// Show the employee search tab.
				this.MainFormPages.SelectTab(this.employeeListPage);
				this.SetActiveEmployee(null);
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Employee Show List", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionEmployeeShowListClick

		private void ActionEmployeeShowPropertiesClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveEmployee() != null) {
					// Clear the employee properties.
					this.EmployeePropertyClear();

					// Populate the employee properties.
					this.EmployeePropertyPopulate(this.GetActiveEmployee());

					// Show the enployee properties tab.
					this.MainFormPages.SelectTab(this.employeePropertyPage);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Employee Show Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionEmployeeShowPropertiesClick

		private void ActionEmployeeCopyPropertiesClick(Object sender, EventArgs e) {
			try {
				List<SofdEmployee> employees = new List<SofdEmployee>();

				// Get employees when the list page is shown.
				if (this.MainFormPages.SelectedTab == this.employeeListPage) {
					// Get selected employees, if one or more employees are selected.
					// Otherwise copy all employees.
					if (this.employeeList.SelectedRows.Count > 0) {
						foreach (DataGridViewRow row in this.employeeList.SelectedRows) {
							employees.Add((SofdEmployee)row.DataBoundItem);
						}
					} else if (this.employeeList.Rows.Count > 0) {
						foreach (DataGridViewRow row in this.employeeList.Rows) {
							employees.Add((SofdEmployee)row.DataBoundItem);
						}
					}
				}

				// Get the employee when the properties page is shown.
				if ((this.MainFormPages.SelectedTab == this.employeePropertyPage) && (this.GetActiveEmployee() != null)) {
					employees.Add(this.GetActiveEmployee());
				}

				// Throw exception when no employees exist.
				if (employees.Count == 0) {
					throw new Exception("No employees selected or shown.");
				}

				// Get text from each employee.
				Boolean textHeader = true;
				StringBuilder text = new StringBuilder();
				foreach (SofdEmployee employee in employees) {
					text.AppendLine(employee.ToString(textHeader, true));
					textHeader = false;
				}

				// Copy text to clipboard.
				Clipboard.SetText(text.ToString());
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Employee Copy Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionEmployeeCopyPropertiesClick

		private void ActionOrganizationShowListClick(Object sender, EventArgs e) {
			try {
				// Show the organization search tab.
				this.MainFormPages.SelectTab(this.organizationListPage);
				this.SetActiveOrganization(null);
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Organization Show List", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionOrganizationShowListClick

		private void ActionOrganizationShowPropertiesClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveOrganization() != null) {
					// Clear the organization properties.
					this.OrganizationPropertyClear();

					// Populate the organization properties.
					this.OrganizationPropertyPopulate(this.GetActiveOrganization());

					// Show the enployee properties tab.
					this.MainFormPages.SelectTab(this.organizationPropertyPage);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Organization Show Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionOrganizationShowPropertiesClick
		#endregion

		#region Action methods (Active Directory).
		private void ActionActiveDirectoryEnableUserClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveEmployee() != null) {
					// Get the SOFD employee.
					SofdEmployee employee = this.GetActiveEmployee();

					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);
					if (user == null) {
						throw new Exception("The user was not found in Active Directory.");
					}

					// Build dialog text.
					StringBuilder dialogText = new StringBuilder();
					dialogText.AppendLine("Do you want to enable and unlock the selected user in the Active Directory ?");
					dialogText.AppendLine();
					dialogText.AppendFormat("{0}\t{1}", user.SamAccountName, user.DisplayName);

					// Show dialog.
					NilexMessageBox dialog = new NilexMessageBox();
					dialog.Text = "Action Enable Active Directory User";
					dialog.Message = dialogText.ToString();
					if (dialog.ShowDialog(this) == DialogResult.OK) {
						// Unlock the user.
						user.UnlockAccount();

						// Enable the user.
						user.Enabled = true;
						user.InsertInfo("Account Enabled and unlocked.");
						user.Save();

						// Send message.
					}

					// Update the selected row.
					this.EmployeeListUpdateRow(this.employeeList.SelectedRows[0].Index);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Enable Active Directory User", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionActiveDirectoryEnableUserClick

		private void ActionActiveDirectoryDisableUserClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveEmployee() != null) {
					// Get the SOFD employee.
					SofdEmployee employee = this.GetActiveEmployee();

					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);
					if (user == null) {
						throw new Exception("The user was not found in Active Directory.");
					}

					// Build dialog text.
					StringBuilder dialogText = new StringBuilder();
					dialogText.AppendLine("Do you want to disable the selected user in the Active Directory ?");
					dialogText.AppendLine();
					dialogText.AppendFormat("{0}\t{1}", user.SamAccountName, user.DisplayName);

					// Show dialog.
					NilexMessageBox dialog = new NilexMessageBox();
					dialog.Text = "Action Disable Active Directory User";
					dialog.Message = dialogText.ToString();
					if (dialog.ShowDialog(this) == DialogResult.OK) {
						// Disable the user.
						user.Enabled = false;
						user.InsertInfo("Account Disabled.");
						user.Save();

						// Send message.
					}

					// Update the selected row.
					this.EmployeeListUpdateRow(this.employeeList.SelectedRows[0].Index);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Disable Active Directory User", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionActiveDirectoryDisableUserClick

		private void ActionActiveDirectoryExpireUserClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveEmployee() != null) {
					// Get the SOFD employee.
					SofdEmployee employee = this.GetActiveEmployee();

					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);
					if (user == null) {
						throw new Exception("The user was not found in Active Directory.");
					}

					// Build dialog text.
					StringBuilder dialogText = new StringBuilder();
					dialogText.AppendLine("Do you want to expire the selected user in the Active Directory ?");
					dialogText.AppendLine();
					dialogText.AppendFormat("{0}\t{1}", user.SamAccountName, user.DisplayName);

					// Show dialog.
					NilexExpiresBox dialog = new NilexExpiresBox();
					dialog.Text = "Action Expire Active Directory User";
					dialog.Message = dialogText.ToString();
					dialog.Date = DateTime.Now.AddDays(10);
					if (dialog.ShowDialog(this) == DialogResult.OK) {
						// Expire the user.
						user.AccountExpirationDate = dialog.Date;
						user.InsertInfo(String.Format("Account Expires {0:dd.MM.yy}.", dialog.Date));
						user.Save();

						// Send message.
					}

					// Update the selected row.
					this.EmployeeListUpdateRow(this.employeeList.SelectedRows[0].Index);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Expire Active Directory User", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionActiveDirectoryExpireUserClick

		private void ActionActiveDirectoryResetUserClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveEmployee() != null) {
					// Get the SOFD employee.
					SofdEmployee employee = this.GetActiveEmployee();

					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);
					if (user == null) {
						throw new Exception("The user was not found in Active Directory.");
					}

					// Generate random password.
					Random random = new Random();
					Int32 passwordLength = this.GetLocalValue("SuggestedPasswordLength", 8);
					if (passwordLength < 1) {
						passwordLength = 8;
					}
					List<String> passwordWords = this.GetLocalValues("SuggestedPasswords");
					for (Int32 index = 0; index < passwordWords.Count; ) {
						passwordWords[index] = passwordWords[index].Trim();
						if ((passwordWords[index].Length > passwordLength) || (passwordWords[index].Length == 0)) {
							passwordWords.RemoveAt(index);
						} else {
							while (passwordWords[index].Length < passwordLength) {
								passwordWords[index] += random.Next(0, 9);
							}
							index++;
						}
					}

					// Build dialog text.
					StringBuilder dialogText = new StringBuilder();
					dialogText.AppendLine("Do you want to reset the selected user in the Active Directory ?");
					dialogText.AppendLine();
					dialogText.AppendFormat("{0}\t{1}", user.SamAccountName, user.DisplayName);
					dialogText.AppendLine();
					dialogText.AppendLine();
					dialogText.AppendLine("The user account is enabled, if disabled.");
					dialogText.AppendLine("The user account is unlocked, if locked.");
					dialogText.AppendLine("The password is reset, if one is speficied.");
					dialogText.AppendLine("The user must change the password at next logon, if speficied.");

					// Show dialog.
					NilexResetBox dialog = new NilexResetBox();
					dialog.Text = "Action Reset Active Directory User";
					dialog.Message = dialogText.ToString();
					if (passwordWords.Count > 0) {
						dialog.Password = passwordWords[random.Next(0, passwordWords.Count)];   // Select random password, from the suggested passwords.
					}
					if (dialog.ShowDialog(this) == DialogResult.OK) {
						// Unlock the user.
						user.UnlockAccount();

						// Reset the user password.
						if (dialog.Password.Length > 0) {
							user.SetPassword(dialog.Password);
						}

						// Expire the user password (the user must change at next logon).
						if (dialog.ExpirePassword == true) {
							user.ExpirePasswordNow();
						} else {
							user.RefreshExpiredPassword();
						}

						// Enable the user.
						user.Enabled = true;
						user.InsertInfo("Account Reset");
						user.Save();

						// Send message.
					}

					// Update the selected row.
					this.EmployeeListUpdateRow(this.employeeList.SelectedRows[0].Index);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Reset Active Directory User", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionActiveDirectoryResetUserClick

		private void ActionActiveDirectoryShowUserClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveEmployee() != null) {
					// Get the SOFD employee.
					SofdEmployee employee = this.GetActiveEmployee();

					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);
					if (user == null) {
						throw new Exception("The user was not found in Active Directory.");
					}

					// Show the AD user in MMC.
					// Get the RDN. This is the DN without the domain part.
					String rdn = user.DistinguishedName;
					if (rdn.ToLower().IndexOf(",dc=") > 0) {
						rdn = rdn.Substring(0, rdn.ToLower().IndexOf(",dc="));
					}

					// Open the data in Notepad.
					Process.Start("dsa.msc", String.Format("/rdn=\"{0}\"", rdn)).WaitForInputIdle(15 * 1000);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Show Active Directory User", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionActiveDirectoryShowUserClick
		#endregion

		#region Action methods (SOFD Directory).
		private void ActionSofdDirectoryEditEmployee(Object sender, EventArgs e) {
			try {
				if (this.GetActiveEmployee() != null) {
					// Get the SOFD employee.
					SofdEmployee employee = this.GetActiveEmployee();

					if (employee.Aktiv == true) {
						// Show dialog.
						EmployeeBox dialog = new EmployeeBox();
						dialog.SofdEmployee = employee;
						if (dialog.ShowDialog(this) == DialogResult.OK) {
							// Save.
							employee.MiFareId = dialog.MiFareId;
							employee.TelefonNummer = (dialog.Phone.IsNullOrWhiteSpace() == false) ? dialog.Phone : null;
							employee.MobilNummer = (dialog.Mobile1.IsNullOrWhiteSpace() == false) ? dialog.Mobile1 : null;
							employee.MobilNummer2 = (dialog.Mobile2.IsNullOrWhiteSpace() == false) ? dialog.Mobile2 : null;
							employee.Epost = (dialog.Email.IsNullOrWhiteSpace() == false) ? dialog.Email.ToUpper() : null;
							employee.Intern = dialog.Intern;
							employee.Ekstern = dialog.Extern;
							employee.Save(true);

							// Update the employee properties page.
							if (this.MainFormPages.SelectedTab == this.employeePropertyPage) {
								this.EmployeePropertyPopulate(employee);
							}

							// Update the employee list.
							this.employeeList.Refresh();
						}
					}
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Edit Employee", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionSofdDirectoryEditEmployee

		private void ActionSofdDirectoryShowActiveLeaderClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveLeader() != null) {
					// Clear the employee properties.
					this.EmployeePropertyClear();

					// Populate the employee properties.
					this.EmployeePropertyPopulate(this.GetActiveLeader());

					// Show the enployee properties tab.
					this.MainFormPages.SelectTab(this.employeePropertyPage);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Employee Show Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionSofdDirectoryShowActiveLeaderClick

		private void ActionSofdDirectoryShowActiveOrganizationClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveOrganization() != null) {
					// Clear the organization properties.
					this.OrganizationPropertyClear();

					// Populate the organization properties.
					this.OrganizationPropertyPopulate(this.GetActiveOrganization());

					// Show the organization properties tab.
					this.MainFormPages.SelectTab(this.organizationPropertyPage);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Organization Show Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionSofdDirectoryShowActiveOrganizationClick

		private void ActionSofdDirectoryListEmployeesClick(Object sender, EventArgs e) {
			try {
				if (this.GetActiveLeader() != null) {
					// Search.
					this.employeeFilterText.Text = "Leader=" + this.GetActiveLeader(true).MaNummer.ToString();
					this.EmployeeSearchStartThreadFromFilterText(sender, e);

					// Show the enployee list tab.
					this.MainFormPages.SelectTab(this.employeeListPage);

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Employee List Employees", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionSofdDirectoryListEmployeesClick
		#endregion



		#region Employee initialize and finalize methods.
		private void EmployeeInitialize() {
			// Initialize result list columns.
			this.employeeList.AutoGenerateColumns = false;

			this.employeeListRefresh.BackgroundImage = this.GetResourceImage("Refresh.png");
			this.employeeListSearchFromClipboard.BackgroundImage = this.GetResourceImage("Paste.png");
			this.employeeListActiveStatus.Image = this.GetResourceImage("Circle Blank.png");
			this.employeeListAdStatus.Image = this.GetResourceImage("Circle Blank.png");

			this.employeeListActiveStatus.Visible = this.GetOptionValue("EmployeeListActiveStatus", true);
			this.employeeListAdStatus.Visible = this.GetOptionValue("EmployeeListAdStatus", true);
			this.employeeListFirstName.Visible = this.GetOptionValue("EmployeeListFirstName", true);
			this.employeeListLastName.Visible = this.GetOptionValue("EmployeeListLastName", true);
			this.employeeListDisplayName.Visible = this.GetOptionValue("EmployeeListDisplayName", true);
			this.employeeListCprNumber.Visible = this.GetOptionValue("EmployeeListCprNumber", true);
			this.employeeListAdUserName.Visible = this.GetOptionValue("EmployeeListAdUserName", true);
			this.employeeListOpusUserName.Visible = this.GetOptionValue("EmployeeListOpusUserName", true);
			this.employeeListPhone.Visible = this.GetOptionValue("EmployeeListPhone", true);
			this.employeeListMobile.Visible = this.GetOptionValue("EmployeeListMobile", true);
			this.employeeListMail.Visible = this.GetOptionValue("EmployeeListMail", true);
			this.employeeListOrganizationName.Visible = this.GetOptionValue("EmployeeListOrganizationName", true);

			// Initialize filters.
			this.employeeFilterText.Text = this.GetOptionValue("EmployeeFilterText", Environment.UserName);
			this.employeeFilterTextAutoWildcards.Checked = this.GetOptionValue("EmployeeFilterTextAutoWildcards", true);
			this.employeeFilterActive.CheckState = (CheckState)this.GetOptionValue("EmployeeFilterActive", (Int32)CheckState.Checked);
			this.employeeFilterAd.CheckState = (CheckState)this.GetOptionValue("EmployeeFilterAd", (Int32)CheckState.Checked);

			this.employeeFilterEmploymentFirstDateBegin.Value = this.GetOptionValue("EmployeeFilterEmploymentFirstDateBeginValue", DateTime.Now);
			this.employeeFilterEmploymentFirstDateBegin.Checked = this.GetOptionValue("EmployeeFilterEmploymentFirstDateBegin", false);
			this.employeeFilterEmploymentFirstDateEnd.Value = this.GetOptionValue("EmployeeFilterEmploymentFirstDateEndValue", DateTime.Now);
			this.employeeFilterEmploymentFirstDateEnd.Checked = this.GetOptionValue("EmployeeFilterEmploymentFirstDateEnd", false);

			this.employeeFilterEmploymentLastDateBegin.Value = this.GetOptionValue("EmployeeFilterEmploymentLastDateBeginValue", DateTime.Now);
			this.employeeFilterEmploymentLastDateBegin.Checked = this.GetOptionValue("EmployeeFilterEmploymentLastDateBegin", false);
			this.employeeFilterEmploymentLastDateEnd.Value = this.GetOptionValue("EmployeeFilterEmploymentLastDateEndValue", DateTime.Now);
			this.employeeFilterEmploymentLastDateEnd.Checked = this.GetOptionValue("EmployeeFilterEmploymentLastDateEnd", false);

			this.employeeFilterEmploymentOldestFirstDateBegin.Value = this.GetOptionValue("EmployeeFilterEmploymentOldestFirstDateBeginValue", DateTime.Now);
			this.employeeFilterEmploymentOldestFirstDateBegin.Checked = this.GetOptionValue("EmployeeFilterEmploymentOldestFirstDateBegin", false);
			this.employeeFilterEmploymentOldestFirstDateEnd.Value = this.GetOptionValue("EmployeeFilterEmploymentOldestFirstDateEndValue", DateTime.Now);
			this.employeeFilterEmploymentOldestFirstDateEnd.Checked = this.GetOptionValue("EmployeeFilterEmploymentOldestFirstDateEnd", false);

			this.employeeFilterEmploymentJubileeDateBegin.Value = this.GetOptionValue("EmployeeFilterEmploymentJubileeDateBeginValue", DateTime.Now);
			this.employeeFilterEmploymentJubileeDateBegin.Checked = this.GetOptionValue("EmployeeFilterEmploymentJubileeDateBegin", false);
			this.employeeFilterEmploymentJubileeDateEnd.Value = this.GetOptionValue("EmployeeFilterEmploymentJubileeDateEndValue", DateTime.Now);
			this.employeeFilterEmploymentJubileeDateEnd.Checked = this.GetOptionValue("EmployeeFilterEmploymentJubileeDateEnd", false);

			this.employeeFilterHistoryActiveFromDateBegin.Value = this.GetOptionValue("EmployeeFilterHistoryActiveFromDateBeginValue", DateTime.Now);
			this.employeeFilterHistoryActiveFromDateBegin.Checked = this.GetOptionValue("EmployeeFilterHistoryActiveFromDateBegin", false);
			this.employeeFilterHistoryActiveFromDateEnd.Value = this.GetOptionValue("EmployeeFilterHistoryActiveFromDateEndValue", DateTime.Now);
			this.employeeFilterHistoryActiveFromDateEnd.Checked = this.GetOptionValue("EmployeeFilterHistoryActiveFromDateEnd", false);

			this.employeeFilterHistoryActiveToDateBegin.Value = this.GetOptionValue("EmployeeFilterHistoryActiveToDateBeginValue", DateTime.Now);
			this.employeeFilterHistoryActiveToDateBegin.Checked = this.GetOptionValue("EmployeeFilterHistoryActiveToDateBegin", false);
			this.employeeFilterHistoryActiveToDateEnd.Value = this.GetOptionValue("EmployeeFilterHistoryActiveToDateEndValue", DateTime.Now);
			this.employeeFilterHistoryActiveToDateEnd.Checked = this.GetOptionValue("EmployeeFilterHistoryActiveToDateEnd", false);

			this.employeeFilterChangedDateBegin.Value = this.GetOptionValue("EmployeeFilterChangedDateBeginValue", DateTime.Now);
			this.employeeFilterChangedDateBegin.Checked = this.GetOptionValue("EmployeeFilterChangedDateBegin", false);
			this.employeeFilterChangedDateEnd.Value = this.GetOptionValue("EmployeeFilterChangedDateEndValue", DateTime.Now);
			this.employeeFilterChangedDateEnd.Checked = this.GetOptionValue("EmployeeFilterChangedDateEnd", false);
		} // EmployeeInitialize

		private void EmployeeFinalize() {
			// Save result list column states.
			this.SetOptionValue("EmployeeListActiveStatus", this.employeeListActiveStatus.Visible);
			this.SetOptionValue("EmployeeListAdStatus", this.employeeListAdStatus.Visible);
			this.SetOptionValue("EmployeeListFirstName", this.employeeListFirstName.Visible);
			this.SetOptionValue("EmployeeListLastName", this.employeeListLastName.Visible);
			this.SetOptionValue("EmployeeListDisplayName", this.employeeListDisplayName.Visible);
			this.SetOptionValue("EmployeeListCprNumber", this.employeeListCprNumber.Visible);
			this.SetOptionValue("EmployeeListAdUserName", this.employeeListAdUserName.Visible);
			this.SetOptionValue("EmployeeListOpusUserName", this.employeeListOpusUserName.Visible);
			this.SetOptionValue("EmployeeListPhone", this.employeeListPhone.Visible);
			this.SetOptionValue("EmployeeListMobile", this.employeeListMobile.Visible);
			this.SetOptionValue("EmployeeListMail", this.employeeListMail.Visible);

			// Save filter states.
			this.SetOptionValues("EmployeeFilterText", this.employeeFilterText.Text);
			this.SetOptionValue("EmployeeFilterTextAutoWildcards", this.employeeFilterTextAutoWildcards.Checked);
			this.SetOptionValue("EmployeeFilterActive", (Int32)this.employeeFilterActive.CheckState);
			this.SetOptionValue("EmployeeFilterAd", (Int32)this.employeeFilterAd.CheckState);

			this.SetOptionValue("EmployeeFilterEmploymentFirstDateBeginValue", this.employeeFilterEmploymentFirstDateBegin.Value);
			this.SetOptionValue("EmployeeFilterEmploymentFirstDateBegin", this.employeeFilterEmploymentFirstDateBegin.Checked);
			this.SetOptionValue("EmployeeFilterEmploymentFirstDateEndValue", this.employeeFilterEmploymentFirstDateEnd.Value);
			this.SetOptionValue("EmployeeFilterEmploymentFirstDateEnd", this.employeeFilterEmploymentFirstDateEnd.Checked);

			this.SetOptionValue("EmployeeFilterEmploymentLastDateBeginValue", this.employeeFilterEmploymentLastDateBegin.Value);
			this.SetOptionValue("EmployeeFilterEmploymentLastDateBegin", this.employeeFilterEmploymentLastDateBegin.Checked);
			this.SetOptionValue("EmployeeFilterEmploymentLastDateEndValue", this.employeeFilterEmploymentLastDateEnd.Value);
			this.SetOptionValue("EmployeeFilterEmploymentLastDateEnd", this.employeeFilterEmploymentLastDateEnd.Checked);

			this.SetOptionValue("EmployeeFilterEmploymentOldestFirstDateBeginValue", this.employeeFilterEmploymentOldestFirstDateBegin.Value);
			this.SetOptionValue("EmployeeFilterEmploymentOldestFirstDateBegin", this.employeeFilterEmploymentOldestFirstDateBegin.Checked);
			this.SetOptionValue("EmployeeFilterEmploymentOldestFirstDateEndValue", this.employeeFilterEmploymentOldestFirstDateEnd.Value);
			this.SetOptionValue("EmployeeFilterEmploymentOldestFirstDateEnd", this.employeeFilterEmploymentOldestFirstDateEnd.Checked);

			this.SetOptionValue("EmployeeFilterEmploymentJubileeDateBeginValue", this.employeeFilterEmploymentJubileeDateBegin.Value);
			this.SetOptionValue("EmployeeFilterEmploymentJubileeDateBegin", this.employeeFilterEmploymentJubileeDateBegin.Checked);
			this.SetOptionValue("EmployeeFilterEmploymentJubileeDateEndValue", this.employeeFilterEmploymentJubileeDateEnd.Value);
			this.SetOptionValue("EmployeeFilterEmploymentJubileeDateEnd", this.employeeFilterEmploymentJubileeDateEnd.Checked);

			this.SetOptionValue("EmployeeFilterHistoryActiveFromDateBeginValue", this.employeeFilterHistoryActiveFromDateBegin.Value);
			this.SetOptionValue("EmployeeFilterHistoryActiveFromDateBegin", this.employeeFilterHistoryActiveFromDateBegin.Checked);
			this.SetOptionValue("EmployeeFilterHistoryActiveFromDateEndValue", this.employeeFilterHistoryActiveFromDateEnd.Value);
			this.SetOptionValue("EmployeeFilterHistoryActiveFromDateEnd", this.employeeFilterHistoryActiveFromDateEnd.Checked);

			this.SetOptionValue("EmployeeFilterHistoryActiveToDateBeginValue", this.employeeFilterHistoryActiveToDateBegin.Value);
			this.SetOptionValue("EmployeeFilterHistoryActiveToDateBegin", this.employeeFilterHistoryActiveToDateBegin.Checked);
			this.SetOptionValue("EmployeeFilterHistoryActiveToDateEndValue", this.employeeFilterHistoryActiveToDateEnd.Value);
			this.SetOptionValue("EmployeeFilterHistoryActiveToDateEnd", this.employeeFilterHistoryActiveToDateEnd.Checked);

			this.SetOptionValue("EmployeeFilterChangedDateBeginValue", this.employeeFilterChangedDateBegin.Value);
			this.SetOptionValue("EmployeeFilterChangedDateBegin", this.employeeFilterChangedDateBegin.Checked);
			this.SetOptionValue("EmployeeFilterChangedDateEndValue", this.employeeFilterChangedDateEnd.Value);
			this.SetOptionValue("EmployeeFilterChangedDateEnd", this.employeeFilterChangedDateEnd.Checked);
		} // EmployeeFinalize
		#endregion

		#region Employee filter and search methods.
		private void EmployeeSearchStartTimer(Object sender, EventArgs e) {
			try {
				// Initialize the timer.
				if (this.employeeSearchThreadTimer == null) {
					this.employeeSearchThreadTimer = new System.Windows.Forms.Timer();
					this.employeeSearchThreadTimer.Interval = 1000;
					this.employeeSearchThreadTimer.Tick += this.EmployeeSearchStartThreadFromFilterText;
				}

				// Start the timer.
				this.employeeSearchThreadTimer.Stop();
				this.employeeSearchThreadTimer.Start();
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // EmployeeSearchStartTimer

		private void EmployeeSearchStartThreadFromFilterText(Object sender, EventArgs e) {
			try {
				// Stop timer.
				if (this.employeeSearchThreadTimer != null) {
					this.employeeSearchThreadTimer.Stop();
				}

				// Stop existing thread.
				if (this.employeeSearchThread != null) {
					this.employeeSearchThreadShutdownEvent.Set();
					this.employeeSearchThread.Abort();
					this.employeeSearchThread = null;
				}

				// Start new thread.
				this.employeeSearchThreadShutdownEvent = new ManualResetEvent(false);
				//this.employeeSearchThread = new Thread(this.EmployeeSearchRunThread);
				this.employeeSearchThread = new Thread(() => EmployeeSearchRunThreadFromFilterText(this.employeeFilterText.Text));
				this.employeeSearchThread.IsBackground = true;
				this.employeeSearchThread.Start();
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // EmployeeSearchStartThreadFromFilterText

		private void EmployeeSearchStartThreadFromClipboard(Object sender, EventArgs e) {
			try {
				if ((Clipboard.ContainsText(TextDataFormat.Text) == true) || (Clipboard.ContainsText(TextDataFormat.UnicodeText) == true)) {
					// Stop timer.
					if (this.employeeSearchThreadTimer != null) {
						this.employeeSearchThreadTimer.Stop();
					}

					// Stop existing thread.
					if (this.employeeSearchThread != null) {
						this.employeeSearchThreadShutdownEvent.Set();
						this.employeeSearchThread.Abort();
						this.employeeSearchThread = null;
					}

					// Get the text.
					String[] clipboardFilterText = Clipboard.GetText().Split(new String[] { ";", ",", "\t", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

					// Start new thread.
					this.employeeSearchThreadShutdownEvent = new ManualResetEvent(false);
					//this.employeeSearchThread = new Thread(this.EmployeeSearchRunThread);
					this.employeeSearchThread = new Thread(() => EmployeeSearchRunThreadFromClipboard(clipboardFilterText));
					this.employeeSearchThread.IsBackground = true;
					this.employeeSearchThread.Start();
				}
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // EmployeeSearchStartThreadFromClipboard

		private void EmployeeSearchRunThreadFromFilterText(String filterText) {
			try {
				// Status.
				this.employeeListStatus.Text = String.Format("{0} employees found - Searching Sofd Directory...", this.employeeList.RowCount);

				String filterName = String.Empty;
				Int32 filterNumber = 0;
				List<SqlWhereFilterBase> employeeFilters = new List<SqlWhereFilterBase>();

				if (filterText.IsNullOrWhiteSpace() == false) {
					// If the filter contains a equal character, split the filter text into a filter name and filter text.
					if (filterText.Contains("=") == true) {
						filterName = filterText.Substring(0, filterText.IndexOf("="));
						filterText = filterText.Substring(filterText.IndexOf("=") + 1);
					}

					// Get the filter text converted to a integer.
					filterNumber = filterText.ToInt32();

					// Add wildcards to the filter text.
					if ((filterText != String.Empty) && (this.employeeFilterTextAutoWildcards.Checked == true)) {
						if (filterText.StartsWith("*") == false) {
							filterText = "*" + filterText;
						}
						if (filterText.EndsWith("*") == false) {
							filterText = filterText + "*";
						}
					}

					// Build employee filters.
					if (filterName.ToLower() == "leader") {
						employeeFilters.Add(new SqlWhereFilterBeginGroup());
						employeeFilters.Add(new SofdEmployeeFilter_NaermesteLederAdBrugerNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_NaermesteLederCprNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_NaermesteLederMaNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
						employeeFilters.Add(new SofdEmployeeFilter_NaermesteLederNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SqlWhereFilterEndGroup());
					} else if (filterName.ToLower() == "organization") {
						employeeFilters.Add(new SqlWhereFilterBeginGroup());
						employeeFilters.Add(new SofdEmployeeFilter_OrganisationId(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
						employeeFilters.Add(new SofdEmployeeFilter_OrganisationNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SqlWhereFilterEndGroup());
					} else {
						employeeFilters.Add(new SqlWhereFilterBeginGroup());
						employeeFilters.Add(new SofdEmployeeFilter_Navn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_ForNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_EfterNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_KaldeNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_TelefonNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_MobilNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_MobilNummer2(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_Epost(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_MaNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
						employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SofdEmployeeFilter_CprNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
						employeeFilters.Add(new SqlWhereFilterEndGroup());
					}
				}

				// Add filter: Active.
				switch (this.employeeFilterActive.CheckState) {
					case CheckState.Checked:
						employeeFilters.Add(new SofdEmployeeFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, true));
						break;
					case CheckState.Unchecked:
						employeeFilters.Add(new SofdEmployeeFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, false));
						break;
				}

				// Add filter: AD.
				switch (this.employeeFilterAd.CheckState) {
					case CheckState.Checked:
						employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Like, "*"));
						break;
					case CheckState.Unchecked:
						employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, null));
						break;
				}

				// Add filter: Dates.
				if (this.employeeFilterEmploymentFirstDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AnsaettelsesDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.employeeFilterEmploymentFirstDateBegin.Value));
				}
				if (this.employeeFilterEmploymentFirstDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AnsaettelsesDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.employeeFilterEmploymentFirstDateEnd.Value));
				}

				if (this.employeeFilterEmploymentLastDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AnsaettelsesOphoersDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.employeeFilterEmploymentLastDateBegin.Value));
				}
				if (this.employeeFilterEmploymentLastDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AnsaettelsesOphoersDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.employeeFilterEmploymentLastDateEnd.Value));
				}

				if (this.employeeFilterEmploymentOldestFirstDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_FoersteAnsaettelsesDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.employeeFilterEmploymentOldestFirstDateBegin.Value));
				}
				if (this.employeeFilterEmploymentOldestFirstDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_FoersteAnsaettelsesDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.employeeFilterEmploymentOldestFirstDateEnd.Value));
				}

				if (this.employeeFilterEmploymentJubileeDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_JubilaeumsAncinnitetsDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.employeeFilterEmploymentJubileeDateBegin.Value));
				}
				if (this.employeeFilterEmploymentJubileeDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_JubilaeumsAncinnitetsDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.employeeFilterEmploymentJubileeDateEnd.Value));
				}

				if (this.employeeFilterHistoryActiveFromDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AktivFra(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.employeeFilterHistoryActiveFromDateBegin.Value));
				}
				if (this.employeeFilterHistoryActiveFromDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AktivFra(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.employeeFilterHistoryActiveFromDateEnd.Value));
				}

				if (this.employeeFilterHistoryActiveToDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AktivTil(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.employeeFilterHistoryActiveToDateBegin.Value));
				}
				if (this.employeeFilterHistoryActiveToDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AktivTil(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.employeeFilterHistoryActiveToDateEnd.Value));
				}

				if (this.employeeFilterChangedDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_SidstAendret(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.employeeFilterChangedDateBegin.Value));
				}
				if (this.employeeFilterChangedDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_SidstAendret(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.employeeFilterChangedDateEnd.Value));
				}

				// Get employees.
				List<SofdEmployee> employees = this.GetAllEmployees(employeeFilters.ToArray());

				// Populate the list with the employees.
				this.employeeList.DataSource = null;
				this.employeeList.DataSource = employees;
				this.employeeList.PerformLayout();

				// Status.
				this.employeeListStatus.Text = String.Format("{0} employees found - Searching Active Directory...", this.employeeList.RowCount);

				// Update the list with AD data.
				// Loop until the service is stopped.
				Int32 employeeIndex = 0;
				while ((this.employeeSearchThreadShutdownEvent.WaitOne(0) == false) && (employeeIndex < this.employeeList.RowCount)) {
					// Update the row.
					this.EmployeeListUpdateRow(employeeIndex);

					// Iterate.
					employeeIndex++;
				}
			} catch (ThreadAbortException exception) {
				// The thread was aborted.
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			} finally {
				// Status.
				this.employeeListStatus.Text = String.Format("{0} employees found", this.employeeList.RowCount);
			}
		} // EmployeeSearchRunThreadFromFilterText

		private void EmployeeSearchRunThreadFromClipboard(String[] filterTexts) {
			try {
				// Status.
				this.employeeListStatus.Text = String.Format("{0} employees found - Searching Sofd Directory...", this.employeeList.RowCount);

				//
				StringBuilder result = new StringBuilder();
				List<SofdEmployee> employees = new List<SofdEmployee>();
				foreach (String filterText in filterTexts) {
					// Build employee filters.
					List<SqlWhereFilterBase> employeeFilters = new List<SqlWhereFilterBase>();

					// Add filter text.
					Int32 filterNumber = 0;
					Int32.TryParse(filterText, out filterNumber);
					employeeFilters.Add(new SqlWhereFilterBeginGroup());
					employeeFilters.Add(new SofdEmployeeFilter_Epost(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_MaNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
					employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_CprNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SqlWhereFilterEndGroup());
					employeeFilters.Add(new SofdEmployeeFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, true));

					// Get employees.
					List<SofdEmployee> employeesFound = this.GetAllEmployees(employeeFilters.ToArray());

					// Add the found employee.
					if (employeesFound.Count == 1) {
						employees.Add(employeesFound[0]);
					} else if (employeesFound.Count > 1) {
						employees.AddRange(employeesFound);
						result.AppendFormat("{0}\t{1} found.", filterText, employeesFound.Count);
						result.AppendLine();
					} else {
						result.AppendFormat("{0}\tNot found.", filterText);
						result.AppendLine();
					}
				}

				// Populate the list with the employees.
				this.employeeList.DataSource = null;
				this.employeeList.DataSource = employees;
				this.employeeList.PerformLayout();

				// Copy result to the clipboard.
				if (filterTexts.Length != employees.Count) {
					Invoke((Action)(() => {
						Clipboard.SetText(result.ToString());
					}));
				}

				// Status.
				this.employeeListStatus.Text = String.Format("{0} employees found - Searching Active Directory...", this.employeeList.RowCount);

				// Update the list with AD data.
				// Loop until the service is stopped.
				Int32 employeeIndex = 0;
				while ((this.employeeSearchThreadShutdownEvent.WaitOne(0) == false) && (employeeIndex < this.employeeList.RowCount)) {
					// Update the row.
					this.EmployeeListUpdateRow(employeeIndex);

					// Iterate.
					employeeIndex++;
				}
			} catch (ThreadAbortException exception) {
				// The thread was aborted.
			} catch (Exception exception) {
				MessageBox.Show(exception.Message);
				// Log.
				this.LogError(exception);
			} finally {
				// Status.
				this.employeeListStatus.Text = String.Format("{0} employees found", this.employeeList.RowCount);
			}
		} // EmployeeSearchRunThreadFromClipboard

		private void EmployeeListDataError(Object sender, DataGridViewDataErrorEventArgs e) {
			// Trapping errors.
			e.Cancel = true;
			e.ThrowException = false;
		} // EmployeeListDataError

		private void EmployeeListUpdateRow(Int32 rowIndex) {
			try {
				if ((rowIndex >= 0) && (rowIndex < this.employeeList.RowCount)) {
					// Get the SOFD employee.
					DataGridViewRow row = this.employeeList.Rows[rowIndex];
					SofdEmployee employee = (SofdEmployee)row.DataBoundItem;

					// Update the active status.
					DataGridViewImageCell employeeListActiveStatus = (DataGridViewImageCell)row.Cells["employeeListActiveStatus"];
					if (employee.Aktiv == true) {
						employeeListActiveStatus.Value = this.GetResourceImage("User Green.png");
					} else {
						employeeListActiveStatus.Value = this.GetResourceImage("User Red.png");
					}

					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);

					// Update the AD status.
					DataGridViewImageCell employeeListAdStatus = (DataGridViewImageCell)row.Cells["employeeListAdStatus"];
					if (user == null) {
						employeeListAdStatus.Value = this.GetResourceImage("Circle Grey.png");
					} else if (user.Enabled == false) {
						employeeListAdStatus.Value = this.GetResourceImage("Circle Red.png");
					} else if (user.IsAccountExpired() == true) {
						employeeListAdStatus.Value = this.GetResourceImage("Circle Orange.png");
					} else if (user.IsAccountLockedOut() == true) {
						employeeListAdStatus.Value = this.GetResourceImage("Circle Yellow.png");
					} else {
						employeeListAdStatus.Value = this.GetResourceImage("Circle Green.png");
					}
				}
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // EmployeeListUpdateRow
		#endregion

		#region Employee Property methods.
		private void EmployeePropertyClear() {
			try {
				// Clear the controls.
				// Identifiers.
				this.employeePropertyActiveImage.Image = null;
				this.employeePropertyActive.Clear();
				this.employeePropertyEmployeeHistoryNumber.Clear();
				this.employeePropertyEmployeeNumber.Clear();
				this.employeePropertyAdUserName.Clear();
				this.employeePropertyOpusName.Clear();
				this.employeePropertyCprNumber.Clear();
				this.employeePropertyMiFareId.Clear();
				this.employeePropertyWorkerId.Clear();

				// Name and addresses.
				this.employeePropertyFirstName.Clear();
				this.employeePropertyLastName.Clear();
				this.employeePropertyName.Clear();
				this.employeePropertyDisplayName.Clear();
				this.employeePropertyPhone.Clear();
				this.employeePropertyMobile.Clear();
				this.employeePropertyMail.Clear();
				this.employeePropertyHomeAddress.Clear();
				this.employeePropertyWorkAddress.Clear();

				// Employment.
				this.employeePropertyEmploymentFirstDate.Clear();
				this.employeePropertyEmploymentLastDate.Clear();
				this.employeePropertyEmploymentOldestFirstDate.Clear();
				this.employeePropertyEmploymentJubileeDate.Clear();
				this.employeePropertyTitle.Clear();
				this.employeePropertyOrganizationName.Clear();
				this.employeePropertyLeaderName.Clear();
				this.employeePropertyInternExtern.Clear();

				// Data history.
				this.employeePropertyHistoryActiveFromDate.Clear();
				this.employeePropertyHistoryActiveToDate.Clear();
				this.employeePropertyHistoryChangedDate.Clear();
				this.employeePropertyHistoryOpusChangedDate.Clear();
				this.employeePropertyHistoryList.DataSource = null;

				// Active Directory.
				this.employeePropertyAdStatusImage.Image = null;
				this.employeePropertyAdStatus.Clear();
				this.employeePropertyAdDisplayName.Clear();
				this.employeePropertyAdChangedDate.Clear();
				this.employeePropertyAdExpiresDate.Clear();
				this.employeePropertyAdLogonDate.Clear();
				this.employeePropertyAdInfo.Clear();
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Employee Property Clear", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // EmployeePropertyClear

		private void EmployeePropertyPopulate(SofdEmployee employee) {
			try {
				if (employee != null) {
					this.SetActiveEmployee(employee);

					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);

					// Populate the controls.
					// Identifiers.
					if (employee.Aktiv == true) {
						this.employeePropertyActiveImage.Image = this.GetResourceImage("User Green.png");
					} else {
						this.employeePropertyActiveImage.Image = this.GetResourceImage("User Red.png");
					}
					this.employeePropertyActive.Text = (employee.Aktiv == true) ? "Active" : "Not Active";
					this.employeePropertyEmployeeHistoryNumber.Text = employee.MedarbejderHistorikId.ToString();
					this.employeePropertyEmployeeNumber.Text = employee.MedarbejderId.ToString();
					this.employeePropertyAdUserName.Text = employee.AdBrugerNavn.GetNotNull();
					this.employeePropertyOpusName.Text = employee.OpusBrugerNavn.GetNotNull();
					this.employeePropertyCprNumber.Text = employee.CprNummer.FormatStringCpr();
					this.employeePropertyMiFareId.Text = employee.MiFareId.GetNotNull();
					this.employeePropertyWorkerId.Text = employee.MaNummer.ToString();

					// Name and addresses.
					this.employeePropertyFirstName.Text = employee.ForNavn.GetNotNull();
					this.employeePropertyLastName.Text = employee.EfterNavn.GetNotNull();
					this.employeePropertyName.Text = employee.Navn.GetNotNull();
					this.employeePropertyDisplayName.Text = employee.KaldeNavn.GetNotNull();
					this.employeePropertyPhone.Text = employee.TelefonNummer.FormatStringPhone();
					this.employeePropertyMobile.Text = employee.MobilNummer.FormatStringPhone();
					this.employeePropertyMail.Text = employee.Epost.GetNotNull().ToUpper();
					this.employeePropertyHomeAddress.Text = employee.AdresseText;
					this.employeePropertyWorkAddress.Text = (employee.GetOrganisation() != null) ? employee.GetOrganisation().AdresseText : String.Empty;

					// Employment.
					this.employeePropertyEmploymentFirstDate.Text = employee.AnsaettelsesDato.FormatStringDate();
					this.employeePropertyEmploymentLastDate.Text = employee.AnsaettelsesOphoersDato.FormatStringDate();
					this.employeePropertyEmploymentOldestFirstDate.Text = employee.FoersteAnsaettelsesDato.FormatStringDate();
					this.employeePropertyEmploymentJubileeDate.Text = employee.JubilaeumsAnciennitetsDato.FormatStringDate();
					this.employeePropertyTitle.Text = employee.StillingsBetegnelse.GetNotNull();
					this.employeePropertyOrganizationName.Text = employee.OrganisationNavn.GetNotNull() + " ("+ employee.OrganisationId + ")";
					this.employeePropertyLeaderName.Text = employee.NaermesteLederNavn.GetNotNull();
					if ((employee.Intern == false) && (employee.Ekstern == false)) {
						this.employeePropertyInternExtern.Text = "No";
					} else if ((employee.Intern == true) && (employee.Ekstern == false)) {
						this.employeePropertyInternExtern.Text = "Intern";
					} else if ((employee.Intern == false) && (employee.Ekstern == true)) {
						this.employeePropertyInternExtern.Text = "Extern";
					} else {
						this.employeePropertyInternExtern.Text = "Intern and Extern";
					}

					// Data history.
					this.employeePropertyHistoryActiveFromDate.Text = employee.AktivFra.FormatStringDate();
					this.employeePropertyHistoryActiveToDate.Text = employee.AktivTil.FormatStringDate();
					this.employeePropertyHistoryChangedDate.Text = employee.SidstAendret.FormatStringDate();
					this.employeePropertyHistoryOpusChangedDate.Text = employee.OpusSidsstAendret.FormatStringDate();
					this.employeePropertyHistoryList.AutoGenerateColumns = false;
					this.employeePropertyHistoryList.DataSource = null;
					this.employeePropertyHistoryList.DataSource = this.GetAllEmployees(new SofdEmployeeFilter_MedarbejderId(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, employee.MedarbejderId));
					foreach (DataGridViewRow row in this.employeePropertyHistoryList.Rows) {
						row.Selected = (employee.Equals(row.DataBoundItem) == true);
					}

					// Active Directory.
					if (user == null) {
						this.employeePropertyAdStatusImage.Image = this.GetResourceImage("Circle Grey.png");
						this.employeePropertyAdStatus.Text = "Not found in Active Directory";
					} else {
						if (user.Enabled == false) {
							this.employeePropertyAdStatusImage.Image = this.GetResourceImage("Circle Red.png");
							this.employeePropertyAdStatus.Text = "This user is disabled";
						} else if (user.IsAccountExpired() == true) {
							this.employeePropertyAdStatusImage.Image = this.GetResourceImage("Circle Orange.png");
							this.employeePropertyAdStatus.Text = String.Format("Expired since {0}", user.AccountExpirationDate.Value.FormatStringDateTime());
						} else if (user.IsAccountLockedOut() == true) {
							this.employeePropertyAdStatusImage.Image = this.GetResourceImage("Circle Yellow.png");
							this.employeePropertyAdStatus.Text = String.Format("Lockedout since {0}", user.AccountLockoutTime.Value.FormatStringDateTime());
						} else {
							this.employeePropertyAdStatusImage.Image = this.GetResourceImage("Circle Green.png");
							this.employeePropertyAdStatus.Text = "Active";
						}
						this.employeePropertyAdDisplayName.Text = user.DisplayName.GetNotNull();
						if (user.AccountExpirationDate != null) {
							this.employeePropertyAdExpiresDate.Text = user.AccountExpirationDate.Value.FormatStringDateTime();
						}
						//this.employeePropertyAdChangedDate.Text = "";
						if (user.Modified != null) {
							this.employeePropertyAdChangedDate.Text = user.Modified.Value.FormatStringDateTime();
						}
						if (user.LastLogon != null) {
							this.employeePropertyAdLogonDate.Text = user.LastLogon.Value.FormatStringDateTime();
						}
						this.employeePropertyAdInfo.Text = user.Info.GetNotNull();
					}
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Employee Property Populate", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // EmployeePropertyPopulate

		private void EmployeePropertyHistoryListClick(Object sender, EventArgs e) {
			try {
				if ((this.employeePropertyHistoryList.Focused == true) && (this.employeePropertyHistoryList.SelectedRows.Count == 1)) {
					// Unfocus the history list.
					this.employeePropertyClose.Focus();

					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.employeePropertyHistoryList.SelectedRows[0].DataBoundItem;
					if (employee != null) {
						// Clear the employee properties.
						this.EmployeePropertyClear();

						// Populate the employee properties.
						this.EmployeePropertyPopulate(employee);
					}

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Employee Property History Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // EmployeePropertyHistoryListClick
		#endregion



		#region Organization initialize and finalize methods.
		private void OrganizationInitialize() {
			// Initialize result list columns.
			this.organizationList.AutoGenerateColumns = false;

			this.organizationListRefresh.BackgroundImage = this.GetResourceImage("Refresh.png");
			this.organizationListSearchFromClipboard.BackgroundImage = this.GetResourceImage("Paste.png");
			this.organizationListActiveStatus.Image = this.GetResourceImage("Circle Blank.png");

			this.organizationListActiveStatus.Visible = this.GetOptionValue("OrganizationListActiveStatus", true);
			this.organizationListShortName.Visible = this.GetOptionValue("OrganizationListShortName", true);
			this.organizationListName.Visible = this.GetOptionValue("OrganizationListName", true);
			this.organizationListPhone.Visible = this.GetOptionValue("OrganizationListPhone", true);
			this.organizationListSeNumber.Visible = this.GetOptionValue("OrganizationListSeNumber", true);
			this.organizationListEanNumber.Visible = this.GetOptionValue("OrganizationListEanNumber", true);
			this.organizationListOmkSted.Visible = this.GetOptionValue("OrganizationListOmkSted", true);
			this.organizationListLeaderName.Visible = this.GetOptionValue("OrganizationListLeaderName", true);

			// Initialize filters.
			this.organizationFilterText.Text = this.GetOptionValue("OrganizationFilterText", "IT");
			this.organizationFilterTextAutoWildcards.Checked = this.GetOptionValue("OrganizationFilterTextAutoWildcards", true);
			this.organizationFilterActive.CheckState = (CheckState)this.GetOptionValue("OrganizationFilterActive", (Int32)CheckState.Checked);

			this.organizationFilterHistoryActiveFromDateBegin.Value = this.GetOptionValue("OrganizationFilterHistoryActiveFromDateBeginValue", DateTime.Now);
			this.organizationFilterHistoryActiveFromDateBegin.Checked = this.GetOptionValue("OrganizationFilterHistoryActiveFromDateBegin", false);
			this.organizationFilterHistoryActiveFromDateEnd.Value = this.GetOptionValue("OrganizationFilterHistoryActiveFromDateEndValue", DateTime.Now);
			this.organizationFilterHistoryActiveFromDateEnd.Checked = this.GetOptionValue("OrganizationFilterHistoryActiveFromDateEnd", false);

			this.organizationFilterHistoryActiveToDateBegin.Value = this.GetOptionValue("OrganizationFilterHistoryActiveToDateBeginValue", DateTime.Now);
			this.organizationFilterHistoryActiveToDateBegin.Checked = this.GetOptionValue("OrganizationFilterHistoryActiveToDateBegin", false);
			this.organizationFilterHistoryActiveToDateEnd.Value = this.GetOptionValue("OrganizationFilterHistoryActiveToDateEndValue", DateTime.Now);
			this.organizationFilterHistoryActiveToDateEnd.Checked = this.GetOptionValue("OrganizationFilterHistoryActiveToDateEnd", false);

			this.organizationFilterChangedDateBegin.Value = this.GetOptionValue("OrganizationFilterChangedDateBeginValue", DateTime.Now);
			this.organizationFilterChangedDateBegin.Checked = this.GetOptionValue("OrganizationFilterChangedDateBegin", false);
			this.organizationFilterChangedDateEnd.Value = this.GetOptionValue("OrganizationFilterChangedDateEndValue", DateTime.Now);
			this.organizationFilterChangedDateEnd.Checked = this.GetOptionValue("OrganizationFilterChangedDateEnd", false);
		} // OrganizationInitialize

		private void OrganizationFinalize() {
			// Save result list column states.
			this.SetOptionValue("OrganizationListActiveStatus", this.organizationListActiveStatus.Visible);
			this.SetOptionValue("OrganizationListShortName", this.organizationListShortName.Visible);
			this.SetOptionValue("OrganizationListName", this.organizationListName.Visible);
			this.SetOptionValue("OrganizationListPhone", this.organizationListPhone.Visible);
			this.SetOptionValue("OrganizationListSeNumber", this.organizationListSeNumber.Visible);
			this.SetOptionValue("OrganizationListEanNumber", this.organizationListEanNumber.Visible);
			this.SetOptionValue("OrganizationListOmkSted", this.organizationListOmkSted.Visible);
			this.SetOptionValue("OrganizationListLeaderName", this.organizationListLeaderName.Visible);

			// Save filter states.
			this.SetOptionValues("OrganizationFilterText", this.organizationFilterText.Text);
			this.SetOptionValue("OrganizationFilterTextAutoWildcards", this.organizationFilterTextAutoWildcards.Checked);
			this.SetOptionValue("OrganizationFilterActive", (Int32)this.organizationFilterActive.CheckState);

			this.SetOptionValue("OrganizationFilterHistoryActiveFromDateBeginValue", this.organizationFilterHistoryActiveFromDateBegin.Value);
			this.SetOptionValue("OrganizationFilterHistoryActiveFromDateBegin", this.organizationFilterHistoryActiveFromDateBegin.Checked);
			this.SetOptionValue("OrganizationFilterHistoryActiveFromDateEndValue", this.organizationFilterHistoryActiveFromDateEnd.Value);
			this.SetOptionValue("OrganizationFilterHistoryActiveFromDateEnd", this.organizationFilterHistoryActiveFromDateEnd.Checked);

			this.SetOptionValue("OrganizationFilterHistoryActiveToDateBeginValue", this.organizationFilterHistoryActiveToDateBegin.Value);
			this.SetOptionValue("OrganizationFilterHistoryActiveToDateBegin", this.organizationFilterHistoryActiveToDateBegin.Checked);
			this.SetOptionValue("OrganizationFilterHistoryActiveToDateEndValue", this.organizationFilterHistoryActiveToDateEnd.Value);
			this.SetOptionValue("OrganizationFilterHistoryActiveToDateEnd", this.organizationFilterHistoryActiveToDateEnd.Checked);

			this.SetOptionValue("OrganizationFilterChangedDateBeginValue", this.organizationFilterChangedDateBegin.Value);
			this.SetOptionValue("OrganizationFilterChangedDateBegin", this.organizationFilterChangedDateBegin.Checked);
			this.SetOptionValue("OrganizationFilterChangedDateEndValue", this.organizationFilterChangedDateEnd.Value);
			this.SetOptionValue("OrganizationFilterChangedDateEnd", this.organizationFilterChangedDateEnd.Checked);
		} // OrganizationFinalize
		#endregion

		#region Organization filter and search methods.
		private void OrganizationSearchStartTimer(Object sender, EventArgs e) {
			try {
				// Initialize the timer.
				if (this.organizationSearchThreadTimer == null) {
					this.organizationSearchThreadTimer = new System.Windows.Forms.Timer();
					this.organizationSearchThreadTimer.Interval = 1000;
					this.organizationSearchThreadTimer.Tick += this.OrganizationSearchStartThreadFromFilterText;
				}

				// Start the timer.
				this.organizationSearchThreadTimer.Stop();
				this.organizationSearchThreadTimer.Start();
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // OrganizationSearchStartTimer

		private void OrganizationSearchStartThreadFromFilterText(Object sender, EventArgs e) {
			try {
				// Stop timer.
				if (this.organizationSearchThreadTimer != null) {
					this.organizationSearchThreadTimer.Stop();
				}

				// Stop existing thread.
				if (this.organizationSearchThread != null) {
					this.organizationSearchThreadShutdownEvent.Set();
					this.organizationSearchThread.Abort();
					this.organizationSearchThread = null;
				}

				// Start new thread.
				this.organizationSearchThreadShutdownEvent = new ManualResetEvent(false);
				this.organizationSearchThread = new Thread(() => OrganizationSearchRunThreadFromFilterText(this.organizationFilterText.Text));
				this.organizationSearchThread.IsBackground = true;
				this.organizationSearchThread.Start();
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // OrganizationSearchStartThreadFromFilterText

		private void OrganizationSearchStartThreadFromClipboard(Object sender, EventArgs e) {
			try {
				if ((Clipboard.ContainsText(TextDataFormat.Text) == true) || (Clipboard.ContainsText(TextDataFormat.UnicodeText) == true)) {
					// Stop timer.
					if (this.organizationSearchThreadTimer != null) {
						this.organizationSearchThreadTimer.Stop();
					}

					// Stop existing thread.
					if (this.organizationSearchThread != null) {
						this.organizationSearchThreadShutdownEvent.Set();
						this.organizationSearchThread.Abort();
						this.organizationSearchThread = null;
					}

					// Get the text.
					String[] clipboardFilterText = Clipboard.GetText().Split(new String[] { ";", ",", "\t", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

					// Start new thread.
					this.organizationSearchThreadShutdownEvent = new ManualResetEvent(false);
					this.organizationSearchThread = new Thread(() => OrganizationSearchRunThreadFromClipboard(clipboardFilterText));
					this.organizationSearchThread.IsBackground = true;
					this.organizationSearchThread.Start();
				}
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // OrganizationSearchStartThreadFromClipboard

		private void OrganizationSearchRunThreadFromFilterText(String filterTexts) {
			try {
				// Status.
				this.organizationListStatus.Text = String.Format("{0} organizations found - Searching Sofd Directory...", this.organizationList.RowCount);

				// Build organization filters.
				List<SqlWhereFilterBase> organizationFilters = new List<SqlWhereFilterBase>();

				// Add filter text.
				Int32 filterNumber = 0;
				Int32.TryParse(filterTexts, out filterNumber);
				String filterText = filterTexts;
				if ((filterText != String.Empty) && (this.organizationFilterTextAutoWildcards.Checked == true)) {
					if (filterText.StartsWith("*") == false) {
						filterText = "*" + filterText;
					}
					if (filterText.EndsWith("*") == false) {
						filterText = filterText + "*";
					}
				}
				if (filterText != String.Empty) {
					organizationFilters.Add(new SqlWhereFilterBeginGroup());
					organizationFilters.Add(new SofdOrganizationFilter_Navn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					organizationFilters.Add(new SofdOrganizationFilter_KortNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					organizationFilters.Add(new SofdOrganizationFilter_TelefonNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					organizationFilters.Add(new SofdOrganizationFilter_SeNumber(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
					organizationFilters.Add(new SofdOrganizationFilter_EanNumber(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
					organizationFilters.Add(new SofdOrganizationFilter_OmkostningsSted(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
					organizationFilters.Add(new SqlWhereFilterEndGroup());
				}

				// Add filter Active.
				switch (this.organizationFilterActive.CheckState) {
					case CheckState.Checked:
						organizationFilters.Add(new SofdOrganizationFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, true));
						break;
					case CheckState.Unchecked:
						organizationFilters.Add(new SofdOrganizationFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, false));
						break;
				}

				// Add filter dates.
				if (this.organizationFilterHistoryActiveFromDateBegin.Checked == true) {
					organizationFilters.Add(new SofdOrganizationFilter_AktivFra(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.organizationFilterHistoryActiveFromDateBegin.Value));
				}
				if (this.organizationFilterHistoryActiveFromDateEnd.Checked == true) {
					organizationFilters.Add(new SofdOrganizationFilter_AktivFra(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.organizationFilterHistoryActiveFromDateEnd.Value));
				}

				if (this.organizationFilterHistoryActiveToDateBegin.Checked == true) {
					organizationFilters.Add(new SofdOrganizationFilter_AktivTil(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.organizationFilterHistoryActiveToDateBegin.Value));
				}
				if (this.organizationFilterHistoryActiveToDateEnd.Checked == true) {
					organizationFilters.Add(new SofdOrganizationFilter_AktivTil(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.organizationFilterHistoryActiveToDateEnd.Value));
				}

				if (this.organizationFilterChangedDateBegin.Checked == true) {
					organizationFilters.Add(new SofdOrganizationFilter_SidstAendret(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.organizationFilterChangedDateBegin.Value));
				}
				if (this.organizationFilterChangedDateEnd.Checked == true) {
					organizationFilters.Add(new SofdOrganizationFilter_SidstAendret(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.organizationFilterChangedDateEnd.Value));
				}

				// Get organizations.
				List<SofdOrganization> organizations = this.GetAllOrganizations(organizationFilters.ToArray());

				// Populate the list with the organizations.
				this.organizationList.DataSource = null;
				this.organizationList.DataSource = organizations;
				this.organizationList.PerformLayout();

				// Status.
				this.organizationListStatus.Text = String.Format("{0} organizations found - Searching Active Directory...", this.organizationList.RowCount);

				// Update the list with AD data.
				// Loop until the service is stopped.
				Int32 organizationIndex = 0;
				while ((this.organizationSearchThreadShutdownEvent.WaitOne(0) == false) && (organizationIndex < this.organizationList.RowCount)) {
					// Update the row.
					this.OrganizationListUpdateRow(organizationIndex);

					// Iterate.
					organizationIndex++;
				}
			} catch (ThreadAbortException exception) {
				// The thread was aborted.
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			} finally {
				// Status.
				this.organizationListStatus.Text = String.Format("{0} organizations found", this.organizationList.RowCount);
			}
		} // OrganizationSearchRunThreadFromFilterText

		private void OrganizationSearchRunThreadFromClipboard(String[] filterTexts) {
			try {
				// Status.
				this.organizationListStatus.Text = String.Format("{0} organizations found - Searching Sofd Directory...", this.organizationList.RowCount);

				//
				StringBuilder result = new StringBuilder();
				List<SofdOrganization> organizations = new List<SofdOrganization>();
				foreach (String filterText in filterTexts) {
					// Build organization filters.
					List<SqlWhereFilterBase> organizationFilters = new List<SqlWhereFilterBase>();

					// Add filter text.
					Int32 filterNumber = 0;
					Int32.TryParse(filterText, out filterNumber);
					organizationFilters.Add(new SqlWhereFilterBeginGroup());
					organizationFilters.Add(new SofdOrganizationFilter_Navn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					organizationFilters.Add(new SofdOrganizationFilter_KortNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					organizationFilters.Add(new SofdOrganizationFilter_TelefonNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					organizationFilters.Add(new SofdOrganizationFilter_SeNumber(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
					organizationFilters.Add(new SofdOrganizationFilter_EanNumber(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Equals, filterNumber));
					organizationFilters.Add(new SqlWhereFilterEndGroup());
					organizationFilters.Add(new SofdOrganizationFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, true));

					// Get organizations.
					List<SofdOrganization> organizationsFound = this.GetAllOrganizations(organizationFilters.ToArray());

					// Add the found organization.
					if (organizationsFound.Count == 1) {
						organizations.Add(organizationsFound[0]);
					} else if (organizationsFound.Count > 1) {
						organizations.AddRange(organizationsFound);
						result.AppendFormat("{0}\t{1} found.", filterText, organizationsFound.Count);
						result.AppendLine();
					} else {
						result.AppendFormat("{0}\tNot found.", filterText);
						result.AppendLine();
					}
				}

				// Populate the list with the organizations.
				this.organizationList.DataSource = null;
				this.organizationList.DataSource = organizations;
				this.organizationList.PerformLayout();

				// Copy result to the clipboard.
				if (filterTexts.Length != organizations.Count) {
					Invoke((Action)(() => {
						Clipboard.SetText(result.ToString());
					}));
				}

				// Status.
				this.organizationListStatus.Text = String.Format("{0} organizations found - Searching Active Directory...", this.organizationList.RowCount);

				// Update the list with AD data.
				// Loop until the service is stopped.
				Int32 organizationIndex = 0;
				while ((this.organizationSearchThreadShutdownEvent.WaitOne(0) == false) && (organizationIndex < this.organizationList.RowCount)) {
					// Update the row.
					this.OrganizationListUpdateRow(organizationIndex);

					// Iterate.
					organizationIndex++;
				}
			} catch (ThreadAbortException exception) {
				// The thread was aborted.
			} catch (Exception exception) {
				MessageBox.Show(exception.Message);
				// Log.
				this.LogError(exception);
			} finally {
				// Status.
				this.organizationListStatus.Text = String.Format("{0} organizations found", this.organizationList.RowCount);
			}
		} // OrganizationSearchRunThreadFromClipboard

		private void OrganizationListDataError(Object sender, DataGridViewDataErrorEventArgs e) {
			// Trapping errors.
			e.Cancel = true;
			e.ThrowException = false;
		} // OrganizationListDataError

		private void OrganizationListUpdateRow(Int32 rowIndex) {
			try {
				if ((rowIndex >= 0) && (rowIndex < this.organizationList.RowCount)) {
					// Get the SOFD organization.
					DataGridViewRow row = this.organizationList.Rows[rowIndex];
					SofdOrganization organization = (SofdOrganization)row.DataBoundItem;

					// Update the active status.
					DataGridViewImageCell organizationListAdStatus = (DataGridViewImageCell)row.Cells["organizationListActiveStatus"];
					if (organization.Aktiv == true) {
						organizationListAdStatus.Value = this.GetResourceImage("Org Green.png");
					} else {
						organizationListAdStatus.Value = this.GetResourceImage("Org Red.png");
					}
				}
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // OrganizationListUpdateRow
		#endregion

		#region Organization Property methods.
		private void OrganizationPropertyClear() {
			try {
				// Clear the controls.
				// Identifiers.
				this.organizationPropertyActiveImage.Image = null;
				this.organizationPropertyActive.Clear();
				this.organizationPropertyOrganizationHistoryNumber.Clear();
				this.organizationPropertyOrganizationNumber.Clear();
				this.organizationPropertyEanNumber.Clear();
				this.organizationPropertyCvrNumber.Clear();
				this.organizationPropertySeNumber.Clear();
				this.organizationPropertyPNumber.Clear();

				// Name and addresses.
				this.organizationPropertyShortName.Clear();
				this.organizationPropertyName.Clear();
				this.organizationPropertyPhone.Clear();
				this.organizationPropertyAddress.Clear();

				// Organization.
				this.organizationPropertyLeaderName.Clear();
				this.organizationPropertyLeaderInherited.Clear();
				this.organizationPropertyOmkSted.Clear();
				this.organizationPropertyLosOrgId.Clear();

				// Data history.
				this.organizationPropertyHistoryActiveFromDate.Clear();
				this.organizationPropertyHistoryActiveToDate.Clear();
				this.organizationPropertyHistoryChangedDate.Clear();
				this.organizationPropertyHistoryLosChangedDate.Clear();
				this.organizationPropertyHistoryList.DataSource = null;
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Organization Property Clear", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // OrganizationPropertyClear

		private void OrganizationPropertyPopulate(SofdOrganization organization) {
			try {
				if (organization != null) {
					this.SetActiveOrganization(organization);

					// Populate the controls.
					// Identifiers.
					if (organization.Aktiv == true) {
						this.organizationPropertyActiveImage.Image = this.GetResourceImage("Org Green.png");
					} else {
						this.organizationPropertyActiveImage.Image = this.GetResourceImage("Org Red.png");
					}
					this.organizationPropertyActive.Text = (organization.Aktiv == true) ? "Active" : "Not Active";
					this.organizationPropertyOrganizationHistoryNumber.Text = organization.OrganisationHistorikId.ToString();
					this.organizationPropertyOrganizationNumber.Text = organization.OrganisationId.ToString();
					this.organizationPropertyEanNumber.Text = organization.EanNummer.ToString();
					this.organizationPropertyCvrNumber.Text = organization.CvrNummer.ToString();
					this.organizationPropertySeNumber.Text = organization.SeNummer.ToString();
					this.organizationPropertyPNumber.Text = organization.PNummer.ToString();

					// Name and addresses.
					this.organizationPropertyShortName.Text = organization.KortNavn.GetNotNull();
					this.organizationPropertyName.Text = organization.Navn.GetNotNull();
					this.organizationPropertyPhone.Text = organization.TelefonNummer.FormatStringPhone();
					this.organizationPropertyAddress.Text = organization.AdresseText;

					// Organization.
					this.organizationPropertyLeaderName.Text = organization.LederNavn.GetNotNull();
					this.organizationPropertyLeaderInherited.Text = (organization.LederNedarvet == true) ? "Inherited" : "Not inherited";
					this.organizationPropertyOmkSted.Text = organization.Omkostningssted.ToString();
					this.organizationPropertyLosOrgId.Text = organization.LosOrganisationId.ToString();

					// Data history.
					this.organizationPropertyHistoryActiveFromDate.Text = organization.AktivFra.FormatStringDate();
					this.organizationPropertyHistoryActiveToDate.Text = organization.AktivTil.FormatStringDate();
					this.organizationPropertyHistoryChangedDate.Text = organization.SidstAendret.FormatStringDate();
					this.organizationPropertyHistoryLosChangedDate.Text = organization.LosSidstAendret.FormatStringDate();
					this.organizationPropertyHistoryList.AutoGenerateColumns = false;
					this.organizationPropertyHistoryList.DataSource = null;
					this.organizationPropertyHistoryList.DataSource = this.GetAllOrganizations(new SofdOrganizationFilter_OrganisationId(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, organization.OrganisationId));
					foreach (DataGridViewRow row in this.organizationPropertyHistoryList.Rows) {
						row.Selected = (organization.Equals(row.DataBoundItem) == true);
					}
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Organization Property Populate", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // OrganizationPropertyPopulate

		private void OrganizationPropertyHistoryListClick(Object sender, EventArgs e) {
			try {
				if ((this.organizationPropertyHistoryList.Focused == true) && (this.organizationPropertyHistoryList.SelectedRows.Count == 1)) {
					// Unfocus the history list.
					this.organizationPropertyClose.Focus();

					// Get the SOFD organization.
					SofdOrganization organization = (SofdOrganization)this.organizationPropertyHistoryList.SelectedRows[0].DataBoundItem;
					if (organization != null) {
						// Clear the organization properties.
						this.OrganizationPropertyClear();

						// Populate the organization properties.
						this.OrganizationPropertyPopulate(organization);

						// Show the organization properties tab.
						this.MainFormPages.SelectTab(this.organizationPropertyPage);
					}

					// Update.
					this.ActionUpdate(sender, e);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Organization Property History Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // OrganizationPropertyHistoryListClick
		#endregion



		#region Abstract and Virtual methods.
		/// <summary>
		/// Gets the unique form guid used when referencing resources.
		/// When implementing a form, this method should return the same unique guid every time. 
		/// </summary>
		/// <returns></returns>
		public override Guid GetGuid() {
			return new Guid("{C7DA4530-5233-48F3-AB21-CA4F87350F6D}");
		} // GetGuid

		/// <summary>
		/// Gets the the form name.
		/// When implementing a form, this method should return a proper display name.
		/// </summary>
		/// <returns></returns>
		public override String GetName() {
			return "NDK SOFD Viewer";
		} // GetName
		#endregion

		#region Static main methods.
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		} // Main
		#endregion

	} // MainForm

	#region SafeDataGridView class
	public class SafeDataGridView : DataGridView {

		public SafeDataGridView() : base() {
			// Always show vertical scrollbar.
			this.VerticalScrollBar.Visible = true;
			this.VerticalScrollBar.VisibleChanged += new EventHandler(VerticalScrollBar_VisibleChanged);
			this.VerticalScrollBar.SetBounds(this.VerticalScrollBar.Location.X, this.VerticalScrollBar.Location.Y, this.VerticalScrollBar.Width, this.Height);
		} // SafeDataGridView

		void VerticalScrollBar_VisibleChanged(Object sender, EventArgs e) {
			this.VerticalScrollBar.Visible = true;
		}

		// Fix red X.
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			try {
				base.OnPaint(e);
			} catch (Exception) {
				this.Invalidate();
			}
		} // OnPaint

	} // SafeDataGridView
	#endregion

} // NDK.SofdViewer