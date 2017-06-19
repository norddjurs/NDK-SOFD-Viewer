using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NDK.Framework;

namespace NDK.SofdViewer {

	public partial class MainForm : BaseForm {
		private System.Windows.Forms.Timer employeeSearchThreadTimer = null;
		private ManualResetEvent employeeSearchThreadShutdownEvent = null;
		private Thread employeeSearchThread = null;

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
				this.actionActiveDirectory.Image = this.GetResourceImage("Book.png");
				this.actionActiveDirectoryEnableUser.Image = this.GetResourceImage("User Green.png");
				this.actionActiveDirectoryDisableUser.Image = this.GetResourceImage("User Red.png");
				this.actionActiveDirectoryExpireUser.Image = this.GetResourceImage("User Orange.png");
				this.actionActiveDirectoryResetUser.Image = this.GetResourceImage("User Yellow.png");
				this.actionActiveDirectoryShowUser.Image = this.GetResourceImage("Book.png");
				this.actionSofdDirectoryEditEmployee.Image = this.GetResourceImage("User Green.png");

				// Initialize employee.
				this.EmployeeInitialize();

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
				// Finalize employee.
				this.EmployeeFinalize();
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

		#region Action methods.
		private void ActionUpdate(Object sender, EventArgs e) {
			try {
				// Get the employee selection count.
				Int32 employeeListSelectionCount = this.employeeList.SelectedRows.Count;

				// Get the SOFD employee.
				SofdEmployee employee = (employeeListSelectionCount != 1) ? null : (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;

				// Get the AD user.
				AdUser user = (employee == null) ? null : this.GetUser(employee.AdBrugerNavn);

				// Enable buttons.
				this.actionEmployeeList.Enabled = (this.MainFormPages.SelectedTab == this.employeePropertyPage);
				this.actionEmployeeProperties.Enabled = ((this.MainFormPages.SelectedTab == this.employeeListPage) && (employeeListSelectionCount == 1));
				this.actionEmployeeCopyProperties.Enabled = (this.MainFormPages.SelectedTab == this.employeeListPage);
				this.actionActiveDirectoryEnableUser.Enabled = ((user != null) && ((user.Enabled == false) || (user.IsAccountLockedOut() == true)));
				this.actionActiveDirectoryDisableUser.Enabled = ((user != null) && (user.Enabled == true));
				this.actionActiveDirectoryExpireUser.Enabled = ((user != null) && (user.Enabled == true));
				this.actionActiveDirectoryResetUser.Enabled = (user != null);
				this.actionActiveDirectoryShowUser.Enabled = (user != null);
				this.actionSofdDirectoryEditEmployee.Enabled = ((employee != null) && (employee.Aktiv == true));
			} catch (Exception exception) {
				// Log.
				this.LogError(exception);
			}
		} // ActionUpdate

		private void ActionEmployeeShowListClick(Object sender, EventArgs e) {
			try {
				// Show the employee search tab.
				this.MainFormPages.SelectTab(this.employeeListPage);
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Employee Show List", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionEmployeeShowListClick

		private void ActionEmployeeShowPropertiesClick(Object sender, EventArgs e) {
			try {
				if (this.employeeList.SelectedRows.Count == 1) {
					// Clear the employee properties.
					this.EmployeePropertyClear();

					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;

					// Populate the employee properties.
					this.EmployeePropertyPopulate(employee);

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
				// Copy selected employees, if one or more employees are selected.
				// Otherwise copy all employees.
				List<SofdEmployee> employees = new List<SofdEmployee>();
				if (this.employeeList.SelectedRows.Count > 0) {
					foreach (DataGridViewRow row in this.employeeList.SelectedRows) {
						employees.Add((SofdEmployee)row.DataBoundItem);
					}
				} else if (this.employeeList.Rows.Count > 0) {
					foreach (DataGridViewRow row in this.employeeList.Rows) {
						employees.Add((SofdEmployee)row.DataBoundItem);
					}
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
		#endregion

		#region Action methods (Active Directory).
		private void ActionEnableActiveDirectoryUserClick(Object sender, EventArgs e) {
			try {
				if (this.employeeList.SelectedRows.Count == 1) {
					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;

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
		} // ActionEnableActiveDirectoryUserClick

		private void ActionDisableActiveDirectoryUserClick(Object sender, EventArgs e) {
			try {
				if (this.employeeList.SelectedRows.Count == 1) {
					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;

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
		} // ActionDisableActiveDirectoryUserClick

		private void ActionExpireActiveDirectoryUserClick(Object sender, EventArgs e) {
			try {
				if (this.employeeList.SelectedRows.Count == 1) {
					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;

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
		} // ActionExpireActiveDirectoryUserClick

		private void ActionResetActiveDirectoryUserClick(Object sender, EventArgs e) {
			try {
				if (this.employeeList.SelectedRows.Count == 1) {
					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;

					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);
					if (user == null) {
						throw new Exception("The user was not found in Active Directory.");
					}

					// Generate random password.
					Random random = new Random();
					List<String> passwordWords = new List<String>();
					passwordWords.Add("London");
					passwordWords.Add("Oslo");
					passwordWords.Add("Vivild");
					passwordWords.Add("Madrid");
					passwordWords.Add("Orebro");

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
					dialog.Password = passwordWords[random.Next(0, passwordWords.Count)];
					while (dialog.Password.Length < 10) {
						dialog.Password += random.Next(0, 9);
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
		} // ActionResetActiveDirectoryUserClick

		private void ActionShowActiveDirectoryUserClick(Object sender, EventArgs e) {
			try {
				if (this.employeeList.SelectedRows.Count == 1) {
					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;

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
		} // ActionShowActiveDirectoryUserClick
		#endregion

		#region Action methods (SOFD Directory).
		private void ActionEditSofdEmployeeClick(Object sender, EventArgs e) {
			try {
				if (this.employeeList.SelectedRows.Count == 1) {
					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.employeeList.SelectedRows[0].DataBoundItem;

					if (employee.Aktiv == true) {
						// Show dialog.
						EmployeeBox dialog = new EmployeeBox();
						dialog.SofdEmployee = employee;
						if (dialog.ShowDialog(this) == DialogResult.OK) {
							// Save.
							employee.MiFareId = dialog.MiFareId;
							employee.Save();

							// Update the employee properties page.
							if (this.MainFormPages.SelectedTab == this.employeePropertyPage) {
								this.EmployeePropertyPopulate(employee);
							}
						}
					}
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Action Edit Employee", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionEditSofdEmployeeClick
		#endregion



		#region Employee initialize and finalize methods.
		private void EmployeeInitialize() {
			// Initialize result list columns.
			this.employeeList.AutoGenerateColumns = false;

			this.employeeListRefresh.BackgroundImage = this.GetResourceImage("Refresh.png");
			this.employeeListSearchFromClipboard.BackgroundImage = this.GetResourceImage("Paste.png");
			this.employeeListAdStatus.Image = this.GetResourceImage("Circle Blank.png");

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

		private void EmployeeSearchRunThreadFromFilterText(String filterTexts) {
			try {
				// Status.
				this.employeeListStatus.Text = String.Format("{0} employees found - Searching Sofd Directory...", this.employeeList.RowCount);

				// Build employee filters.
				List<SqlWhereFilterBase> employeeFilters = new List<SqlWhereFilterBase>();

				// Add filter text.
				String filterText = filterTexts; //this.employeeFilterText.Text.Trim();
				if ((filterText != String.Empty) && (this.employeeFilterTextAutoWildcards.Checked == true)) {
					if (filterText.StartsWith("*") == false) {
						filterText = "*" + filterText;
					}
					if (filterText.EndsWith("*") == false) {
						filterText = filterText + "*";
					}
				}
				if (filterText != String.Empty) {
					employeeFilters.Add(new SqlWhereFilterBeginGroup());
					employeeFilters.Add(new SofdEmployeeFilter_Navn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_ForNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_EfterNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_KaldeNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_TelefonNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_MobilNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_MobilNummer2(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_Epost(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SofdEmployeeFilter_CprNummer(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
					employeeFilters.Add(new SqlWhereFilterEndGroup());
				}

				// Add filter Active.
				switch (this.employeeFilterActive.CheckState) {
					case CheckState.Checked:
						employeeFilters.Add(new SofdEmployeeFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, true));
						break;
					case CheckState.Unchecked:
						employeeFilters.Add(new SofdEmployeeFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, false));
						break;
				}

				// Add filter AD.
				switch (this.employeeFilterAd.CheckState) {
					case CheckState.Checked:
						employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Like, "*"));
						break;
					case CheckState.Unchecked:
						employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, null));
						break;
				}

				// Add filter dates.
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
					employeeFilters.Add(new SqlWhereFilterBeginGroup());
					employeeFilters.Add(new SofdEmployeeFilter_Epost(SqlWhereFilterOperator.OR, SqlWhereFilterValueOperator.Like, filterText));
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

				// Employment.
				this.employeePropertyEmploymentFirstDate.Clear();
				this.employeePropertyEmploymentLastDate.Clear();
				this.employeePropertyEmploymentOldestFirstDate.Clear();
				this.employeePropertyEmploymentJubileeDate.Clear();
				this.employeePropertyTitle.Clear();
				this.employeePropertyOrganizationName.Clear();
				this.employeePropertyLeaderName.Clear();

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
					// Get the AD user.
					AdUser user = this.GetUser(employee.AdBrugerNavn);

					// Populate the controls.
					// Identifiers.
					if (employee.Aktiv == true) {
						this.employeePropertyActiveImage.Image = this.GetResourceImage("Circle Green.png");
					} else {
						this.employeePropertyActiveImage.Image = this.GetResourceImage("Circle Red.png");
					}
					this.employeePropertyActive.Text = (employee.Aktiv == true) ? "Active" : "Not Active";
					this.employeePropertyEmployeeHistoryNumber.Text = employee.MedarbejderHistorikId.ToString();
					this.employeePropertyEmployeeNumber.Text = employee.MedarbejderId.ToString();
					this.employeePropertyAdUserName.Text = employee.AdBrugerNavn;
					this.employeePropertyOpusName.Text = employee.OpusBrugerNavn;
					this.employeePropertyCprNumber.Text = employee.CprNummer;
					this.employeePropertyMiFareId.Text = employee.MiFareId;
					this.employeePropertyWorkerId.Text = employee.MaNummer.ToString();

					// Name and addresses.
					this.employeePropertyFirstName.Text = employee.ForNavn;
					this.employeePropertyLastName.Text = employee.EfterNavn;
					this.employeePropertyName.Text = employee.Navn;
					this.employeePropertyDisplayName.Text = employee.KaldeNavn;
					this.employeePropertyPhone.Text = employee.TelefonNummer;
					this.employeePropertyMobile.Text = employee.MobilNummer;
					this.employeePropertyHomeAddress.Text = employee.AdresseText;

					// Employment.
					if (employee.AnsaettelsesDato.Year > 1900) {
						this.employeePropertyEmploymentFirstDate.Text = employee.AnsaettelsesDato.ToString("dd.MM.yyyy");
					}
					if (employee.AnsaettelsesOphoersDato.Year > 1900) {
						this.employeePropertyEmploymentLastDate.Text = employee.AnsaettelsesOphoersDato.ToString("dd.MM.yyyy");
					}
					if (employee.FoersteAnsaettelsesDato.Year > 1900) {
						this.employeePropertyEmploymentOldestFirstDate.Text = employee.FoersteAnsaettelsesDato.ToString("dd.MM.yyyy");
					}
					if (employee.JubilaeumsAnciennitetsDato.Year > 1900) {
						this.employeePropertyEmploymentJubileeDate.Text = employee.JubilaeumsAnciennitetsDato.ToString("dd.MM.yyyy");
					}
					this.employeePropertyTitle.Text = employee.StillingsBetegnelse;
					this.employeePropertyOrganizationName.Text = employee.OrganisationNavn;
					this.employeePropertyLeaderName.Text = employee.NaermesteLederNavn;

					// Data history.
					if (employee.AktivFra.Year > 1900) {
						this.employeePropertyHistoryActiveFromDate.Text = employee.AktivFra.ToString("dd.MM.yyyy");
					}
					if (employee.AktivTil.Year > 1900) {
						this.employeePropertyHistoryActiveToDate.Text = employee.AktivTil.ToString("dd.MM.yyyy");
					}
					if (employee.SidstAendret.Year > 1900) {
						this.employeePropertyHistoryChangedDate.Text = employee.SidstAendret.ToString("dd.MM.yyyy");
					}
					if (employee.OpusSidsstAendret.Year > 1900) {
						this.employeePropertyHistoryOpusChangedDate.Text = employee.OpusSidsstAendret.ToString("dd.MM.yyyy");
					}
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
							this.employeePropertyAdStatus.Text = String.Format("Expired since {0:dd.MM.yyyy HH:mm}", user.AccountExpirationDate.Value);
						} else if (user.IsAccountLockedOut() == true) {
							this.employeePropertyAdStatusImage.Image = this.GetResourceImage("Circle Yellow.png");
							this.employeePropertyAdStatus.Text = String.Format("Lockedout since {0:dd.MM.yyyy HH:mm}", user.AccountLockoutTime.Value);
						} else {
							this.employeePropertyAdStatusImage.Image = this.GetResourceImage("Circle Green.png");
							this.employeePropertyAdStatus.Text = "Active";
						}
						this.employeePropertyAdDisplayName.Text = user.DisplayName;
						if ((user.AccountExpirationDate != null) && (user.AccountExpirationDate.Value.Year > 1900)) {
							this.employeePropertyAdExpiresDate.Text = user.AccountExpirationDate.Value.ToString("dd.MM.yyyy  HH:mm");
						}
						//this.employeePropertyAdChangedDate.Text = "";
						if ((user.Modified != null) && (user.Modified.Value.Year > 1900)) {
							this.employeePropertyAdChangedDate.Text = user.Modified.Value.ToString("dd.MM.yyyy  HH:mm");
						}
						if ((user.LastLogon != null) && (user.LastLogon.Value.Year > 1900)) {
							this.employeePropertyAdLogonDate.Text = user.LastLogon.Value.ToString("dd.MM.yyyy  HH:mm");
						}
						this.employeePropertyAdInfo.Text = user.Info;
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

					// Clear the employee properties.
					this.EmployeePropertyClear();

					// Populate the employee properties.
					this.EmployeePropertyPopulate(employee);
				}
			} catch (Exception exception) {
				// Log and show the error.
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Employee Property History Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // EmployeePropertyHistoryListClick
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

} // NDK.SofdViewer