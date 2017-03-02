using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NDK.Framework;

namespace NDK.SofdViewer {

	public partial class MainForm : PluginForm {
		private DataGridViewImageColumn resultListAdStatus;
		private DataGridViewTextBoxColumn resultListFirstName;
		private DataGridViewTextBoxColumn resultListLastName;
		private DataGridViewTextBoxColumn resultListDisplayName;
		private DataGridViewTextBoxColumn resultListCprNumber;
		private DataGridViewTextBoxColumn resultListAdUserid;
		private DataGridViewTextBoxColumn resultListOpusUserid;
		private DataGridViewTextBoxColumn resultListPhone;
		private DataGridViewTextBoxColumn resultListMobile;
		private DataGridViewTextBoxColumn resultListEMail;

		private Button actionProperties;
		private Button actionAdEnableUser;
		private Button actionAdDisableUser;

		private System.Windows.Forms.Timer searchThreadTimer = null;
		private ManualResetEvent searchThreadShutdownEvent = null;
		private Thread searchThread = null;

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
			// Invoke base method.
			base.OnLoad(e);

			// Hide tabs.
			tabControl1.Appearance = TabAppearance.FlatButtons;
			tabControl1.ItemSize = new Size(0, 1);
			tabControl1.SizeMode = TabSizeMode.Fixed;

			// Initialize result list columns.
			this.resultList.AutoGenerateColumns = false;

			this.resultListAdStatus = new DataGridViewImageColumn();
			this.resultListAdStatus.Name = "resultListAdStatus";
			this.resultListAdStatus.HeaderText = "";
			this.resultListAdStatus.Image = this.GetResourceImage("Circle Blank.png");
			this.resultListAdStatus.ImageLayout = DataGridViewImageCellLayout.Zoom;
			this.resultListAdStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListAdStatus.MinimumWidth = 25;
			this.resultListAdStatus.Width = 25;
			this.resultListAdStatus.Visible = this.GetOptionValue("resultListAdStatus", true);
			this.resultList.Columns.Add(this.resultListAdStatus);

			this.resultListFirstName = new DataGridViewTextBoxColumn();
			this.resultListFirstName.DataPropertyName = "ForNavn";
			this.resultListFirstName.HeaderText = "First name";
			this.resultListFirstName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListFirstName.MinimumWidth = 100;
			this.resultListFirstName.Visible = this.GetOptionValue("resultListFirstName", true);
			this.resultList.Columns.Add(this.resultListFirstName);

			this.resultListLastName = new DataGridViewTextBoxColumn();
			this.resultListLastName.DataPropertyName = "EfterNavn";
			this.resultListLastName.HeaderText = "Last name";
			this.resultListLastName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListLastName.MinimumWidth = 100;
			this.resultListLastName.Visible = this.GetOptionValue("resultListLastName", true);
			this.resultList.Columns.Add(this.resultListLastName);

			this.resultListDisplayName = new DataGridViewTextBoxColumn();
			this.resultListDisplayName.DataPropertyName = "KaldeNavn";
			this.resultListDisplayName.HeaderText = "Display name";
			this.resultListDisplayName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListDisplayName.MinimumWidth = 150;
			this.resultListDisplayName.Visible = this.GetOptionValue("resultListDisplayName", true);
			this.resultList.Columns.Add(this.resultListDisplayName);

			this.resultListCprNumber = new DataGridViewTextBoxColumn();
			this.resultListCprNumber.DataPropertyName = "CprNummer";
			this.resultListCprNumber.HeaderText = "CPR";
			this.resultListCprNumber.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListCprNumber.MinimumWidth = 70;
			this.resultListCprNumber.Visible = this.GetOptionValue("resultListCprNumber", true);
			this.resultList.Columns.Add(this.resultListCprNumber);

			this.resultListAdUserid = new DataGridViewTextBoxColumn();
			this.resultListAdUserid.DataPropertyName = "AdBrugerNavn";
			this.resultListAdUserid.HeaderText = "AD";
			this.resultListAdUserid.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListAdUserid.MinimumWidth = 100;
			this.resultListAdUserid.Visible = this.GetOptionValue("resultListAdUserid", true);
			this.resultList.Columns.Add(this.resultListAdUserid);

			this.resultListOpusUserid = new DataGridViewTextBoxColumn();
			this.resultListOpusUserid.DataPropertyName = "OpusBrugerNavn";
			this.resultListOpusUserid.HeaderText = "Opus";
			this.resultListOpusUserid.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListOpusUserid.MinimumWidth = 100;
			this.resultListOpusUserid.Visible = this.GetOptionValue("resultListOpusUserid", true);
			this.resultList.Columns.Add(this.resultListOpusUserid);

			this.resultListPhone = new DataGridViewTextBoxColumn();
			this.resultListPhone.DataPropertyName = "TelefonNummer";
			this.resultListPhone.HeaderText = "Phone";
			this.resultListPhone.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListPhone.MinimumWidth = 100;
			this.resultListPhone.Visible = this.GetOptionValue("resultListPhone", true);
			this.resultList.Columns.Add(this.resultListPhone);

			this.resultListMobile = new DataGridViewTextBoxColumn();
			this.resultListMobile.DataPropertyName = "MobilNummer";
			this.resultListMobile.HeaderText = "Mobile";
			this.resultListMobile.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListMobile.MinimumWidth = 100;
			this.resultListMobile.Visible = this.GetOptionValue("resultListMobile", true);
			this.resultList.Columns.Add(this.resultListMobile);

			this.resultListEMail = new DataGridViewTextBoxColumn();
			this.resultListEMail.DataPropertyName = "Epost";
			this.resultListEMail.HeaderText = "E-mail";
			this.resultListEMail.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.resultListEMail.MinimumWidth = 100;
			this.resultListEMail.Visible = this.GetOptionValue("resultListEMail", true);
			this.resultList.Columns.Add(this.resultListEMail);

			// Initialize action properties.
			this.actionProperties = new Button();
			this.actionProperties.Text = "Show properties";
			//this.actionProperties.Image =
			this.actionProperties.Enabled = false;
			this.actionProperties.Visible = true;
			this.actionProperties.Click += this.ActionPropertiesClick;
			this.actionPanel.Controls.Add(this.actionProperties);

			this.actionAdEnableUser = new Button();
			this.actionAdEnableUser.Text = "Enable user AD account";
			//this.actionAdEnableUser.Image =
			this.actionAdEnableUser.Enabled = false;
			this.actionAdEnableUser.Visible = true;
			this.actionAdEnableUser.Click += this.ActionAdEnableUserClick;
			this.actionPanel.Controls.Add(this.actionAdEnableUser);

			this.actionAdDisableUser = new Button();
			this.actionAdDisableUser.Text = "Disable user AD account";
			//this.actionAdDisableUser.Image =
			this.actionAdDisableUser.Enabled = false;
			this.actionAdDisableUser.Visible = true;
			this.actionAdDisableUser.Click += this.ActionAdDisableUserClick;
			this.actionPanel.Controls.Add(this.actionAdDisableUser);

			// Initialize filters.
			this.filterText.Text = this.GetOptionValue("filterText", Environment.UserName);
			this.filterTextAutoWildcards.Checked = this.GetOptionValue("filterTextAutoWildcards", true);
			this.filterActive.CheckState = (CheckState)this.GetOptionValue("filterActive", (Int32)CheckState.Checked);
			this.filterAd.CheckState = (CheckState)this.GetOptionValue("filterAd", (Int32)CheckState.Checked);

			this.filterEmploymentFirstDateBegin.Value = this.GetOptionValue("filterEmploymentFirstDateBeginValue", DateTime.Now);
			this.filterEmploymentFirstDateBegin.Checked = this.GetOptionValue("filterEmploymentFirstDateBegin", false);
			this.filterEmploymentFirstDateEnd.Value = this.GetOptionValue("filterEmploymentFirstDateEndValue", DateTime.Now);
			this.filterEmploymentFirstDateEnd.Checked = this.GetOptionValue("filterEmploymentFirstDateEnd", false);

			this.filterEmploymentLastDateBegin.Value = this.GetOptionValue("filterEmploymentLastDateBeginValue", DateTime.Now);
			this.filterEmploymentLastDateBegin.Checked = this.GetOptionValue("filterEmploymentLastDateBegin", false);
			this.filterEmploymentLastDateEnd.Value = this.GetOptionValue("filterEmploymentLastDateEndValue", DateTime.Now);
			this.filterEmploymentLastDateEnd.Checked = this.GetOptionValue("filterEmploymentLastDateEnd", false);

			this.filterEmploymentOldestFirstDateBegin.Value = this.GetOptionValue("filterEmploymentOldestFirstDateBeginValue", DateTime.Now);
			this.filterEmploymentOldestFirstDateBegin.Checked = this.GetOptionValue("filterEmploymentOldestFirstDateBegin", false);
			this.filterEmploymentOldestFirstDateEnd.Value = this.GetOptionValue("filterEmploymentOldestFirstDateEndValue", DateTime.Now);
			this.filterEmploymentOldestFirstDateEnd.Checked = this.GetOptionValue("filterEmploymentOldestFirstDateEnd", false);

			this.filterEmploymentJubileeDateBegin.Value = this.GetOptionValue("filterEmploymentJubileeDateBeginValue", DateTime.Now);
			this.filterEmploymentJubileeDateBegin.Checked = this.GetOptionValue("filterEmploymentJubileeDateBegin", false);
			this.filterEmploymentJubileeDateEnd.Value = this.GetOptionValue("filterEmploymentJubileeDateEndValue", DateTime.Now);
			this.filterEmploymentJubileeDateEnd.Checked = this.GetOptionValue("filterEmploymentJubileeDateEnd", false);

			this.filterHistoryActiveFromDateBegin.Value = this.GetOptionValue("filterHistoryActiveFromDateBeginValue", DateTime.Now);
			this.filterHistoryActiveFromDateBegin.Checked = this.GetOptionValue("filterHistoryActiveFromDateBegin", false);
			this.filterHistoryActiveFromDateEnd.Value = this.GetOptionValue("filterHistoryActiveFromDateEndValue", DateTime.Now);
			this.filterHistoryActiveFromDateEnd.Checked = this.GetOptionValue("filterHistoryActiveFromDateEnd", false);

			this.filterHistoryActiveToDateBegin.Value = this.GetOptionValue("filterHistoryActiveToDateBeginValue", DateTime.Now);
			this.filterHistoryActiveToDateBegin.Checked = this.GetOptionValue("filterHistoryActiveToDateBegin", false);
			this.filterHistoryActiveToDateEnd.Value = this.GetOptionValue("filterHistoryActiveToDateEndValue", DateTime.Now);
			this.filterHistoryActiveToDateEnd.Checked = this.GetOptionValue("filterHistoryActiveToDateEnd", false);

			this.filterHistoryChangedDateBegin.Value = this.GetOptionValue("filterHistoryChangedDateBeginValue", DateTime.Now);
			this.filterHistoryChangedDateBegin.Checked = this.GetOptionValue("filterHistoryChangedDateBegin", false);
			this.filterHistoryChangedDateEnd.Value = this.GetOptionValue("filterHistoryChangedDateEndValue", DateTime.Now);
			this.filterHistoryChangedDateEnd.Checked = this.GetOptionValue("filterHistoryChangedDateEnd", false);

			// Search.
			this.SearchStartTimer(this, e);
		} // OnLoad

		protected override void OnClosing(CancelEventArgs e) {
			// Save result list column states.
			this.SetOptionValue("resultListFirstName", this.resultListFirstName.Visible);
			this.SetOptionValue("resultListLastName", this.resultListLastName.Visible);
			this.SetOptionValue("resultListDisplayName", this.resultListDisplayName.Visible);
			this.SetOptionValue("resultListCprNumber", this.resultListCprNumber.Visible);
			this.SetOptionValue("resultListAdUserid", this.resultListAdUserid.Visible);
			this.SetOptionValue("resultListOpusUserid", this.resultListOpusUserid.Visible);
			this.SetOptionValue("resultListPhone", this.resultListPhone.Visible);
			this.SetOptionValue("resultListMobile", this.resultListMobile.Visible);
			this.SetOptionValue("resultListEMail", this.resultListEMail.Visible);

			// Save filter states.
			this.SetOptionValues("filterText", this.filterText.Text);
			this.SetOptionValue("filterTextAutoWildcards", this.filterTextAutoWildcards.Checked);
			this.SetOptionValue("filterActive", (Int32)this.filterActive.CheckState);
			this.SetOptionValue("filterAd", (Int32)this.filterAd.CheckState);

			this.SetOptionValue("filterEmploymentFirstDateBeginValue", this.filterEmploymentFirstDateBegin.Value);
			this.SetOptionValue("filterEmploymentFirstDateBegin", this.filterEmploymentFirstDateBegin.Checked);
			this.SetOptionValue("filterEmploymentFirstDateEndValue", this.filterEmploymentFirstDateEnd.Value);
			this.SetOptionValue("filterEmploymentFirstDateEnd", this.filterEmploymentFirstDateEnd.Checked);

			this.SetOptionValue("filterEmploymentLastDateBeginValue", this.filterEmploymentLastDateBegin.Value);
			this.SetOptionValue("filterEmploymentLastDateBegin", this.filterEmploymentLastDateBegin.Checked);
			this.SetOptionValue("filterEmploymentLastDateEndValue", this.filterEmploymentLastDateEnd.Value);
			this.SetOptionValue("filterEmploymentLastDateEnd", this.filterEmploymentLastDateEnd.Checked);

			this.SetOptionValue("filterEmploymentOldestFirstDateBeginValue", this.filterEmploymentOldestFirstDateBegin.Value);
			this.SetOptionValue("filterEmploymentOldestFirstDateBegin", this.filterEmploymentOldestFirstDateBegin.Checked);
			this.SetOptionValue("filterEmploymentOldestFirstDateEndValue", this.filterEmploymentOldestFirstDateEnd.Value);
			this.SetOptionValue("filterEmploymentOldestFirstDateEnd", this.filterEmploymentOldestFirstDateEnd.Checked);

			this.SetOptionValue("filterEmploymentJubileeDateBeginValue", this.filterEmploymentJubileeDateBegin.Value);
			this.SetOptionValue("filterEmploymentJubileeDateBegin", this.filterEmploymentJubileeDateBegin.Checked);
			this.SetOptionValue("filterEmploymentJubileeDateEndValue", this.filterEmploymentJubileeDateEnd.Value);
			this.SetOptionValue("filterEmploymentJubileeDateEnd", this.filterEmploymentJubileeDateEnd.Checked);

			this.SetOptionValue("filterHistoryActiveFromDateBeginValue", this.filterHistoryActiveFromDateBegin.Value);
			this.SetOptionValue("filterHistoryActiveFromDateBegin", this.filterHistoryActiveFromDateBegin.Checked);
			this.SetOptionValue("filterHistoryActiveFromDateEndValue", this.filterHistoryActiveFromDateEnd.Value);
			this.SetOptionValue("filterHistoryActiveFromDateEnd", this.filterHistoryActiveFromDateEnd.Checked);

			this.SetOptionValue("filterHistoryActiveToDateBeginValue", this.filterHistoryActiveToDateBegin.Value);
			this.SetOptionValue("filterHistoryActiveToDateBegin", this.filterHistoryActiveToDateBegin.Checked);
			this.SetOptionValue("filterHistoryActiveToDateEndValue", this.filterHistoryActiveToDateEnd.Value);
			this.SetOptionValue("filterHistoryActiveToDateEnd", this.filterHistoryActiveToDateEnd.Checked);

			this.SetOptionValue("filterHistoryChangedDateBeginValue", this.filterHistoryChangedDateBegin.Value);
			this.SetOptionValue("filterHistoryChangedDateBegin", this.filterHistoryChangedDateBegin.Checked);
			this.SetOptionValue("filterHistoryChangedDateEndValue", this.filterHistoryChangedDateEnd.Value);
			this.SetOptionValue("filterHistoryChangedDateEnd", this.filterHistoryChangedDateEnd.Checked);

			// Invoke base method.
			e.Cancel = false;
			base.OnClosing(e);
		} // OnClosing
		#endregion

		#region Filter and search methods.
		private void SearchStartTimer(Object sender, EventArgs e) {
			// Initialize the timer.
			if (this.searchThreadTimer == null) {
				this.searchThreadTimer = new System.Windows.Forms.Timer();
				this.searchThreadTimer.Interval = 1000;
				this.searchThreadTimer.Tick += this.SearchStartThread;
			}

			// Start the timer.
			this.searchThreadTimer.Stop();
			this.searchThreadTimer.Start();
		} // SearchStartTimer

		private void SearchStartThread(Object sender, EventArgs e) {
			try {
				// Stop timer.
				if (this.searchThreadTimer != null) {
					this.searchThreadTimer.Stop();
				}

				// Stop existing thread.
				if (this.searchThread != null) {
					this.searchThreadShutdownEvent.Set();
					this.searchThread.Abort();
					this.searchThread = null;
				}

				// Start new thread.
				this.searchThreadShutdownEvent = new ManualResetEvent(false);
				this.searchThread = new Thread(this.SearchRunThread);
				this.searchThread.IsBackground = true;
				this.searchThread.Start();
			} catch {}
		} // SearchStartThread

		private void SearchRunThread() {
			try {
				// Status.
				this.resultListStatus.Text = String.Format("{0} employees found - Searching Sofd Directory...", this.resultList.RowCount);

				// Build employee filters.
				List<SqlWhereFilterBase> employeeFilters = new List<SqlWhereFilterBase>();

				// Add filter text.
				String filterText = this.filterText.Text.Trim();
				if ((filterText != String.Empty) && (this.filterTextAutoWildcards.Checked == true)) {
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
				switch (this.filterActive.CheckState) {
					case CheckState.Checked:
						employeeFilters.Add(new SofdEmployeeFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, true));
						break;
					case CheckState.Unchecked:
						employeeFilters.Add(new SofdEmployeeFilter_Aktiv(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, false));
						break;
				}

				// Add filter AD.
				switch (this.filterAd.CheckState) {
					case CheckState.Checked:
						employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Like, "*"));
						break;
					case CheckState.Unchecked:
						employeeFilters.Add(new SofdEmployeeFilter_AdBrugerNavn(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, null));
						break;
				}

				// Add filter dates.
				if (this.filterEmploymentFirstDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AnsaettelsesDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.filterEmploymentFirstDateBegin.Value));
				}
				if (this.filterEmploymentFirstDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AnsaettelsesDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.filterEmploymentFirstDateEnd.Value));
				}

				if (this.filterEmploymentLastDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AnsaettelsesOphoersDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.filterEmploymentLastDateBegin.Value));
				}
				if (this.filterEmploymentLastDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AnsaettelsesOphoersDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.filterEmploymentLastDateEnd.Value));
				}

				if (this.filterEmploymentOldestFirstDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_FoersteAnsaettelsesDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.filterEmploymentOldestFirstDateBegin.Value));
				}
				if (this.filterEmploymentOldestFirstDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_FoersteAnsaettelsesDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.filterEmploymentOldestFirstDateEnd.Value));
				}

				if (this.filterEmploymentJubileeDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_JubilaeumsAncinnitetsDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.filterEmploymentJubileeDateBegin.Value));
				}
				if (this.filterEmploymentJubileeDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_JubilaeumsAncinnitetsDato(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.filterEmploymentJubileeDateEnd.Value));
				}

				if (this.filterHistoryActiveFromDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AktivFra(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.filterHistoryActiveFromDateBegin.Value));
				}
				if (this.filterHistoryActiveFromDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AktivFra(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.filterHistoryActiveFromDateEnd.Value));
				}

				if (this.filterHistoryActiveToDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AktivTil(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.filterHistoryActiveToDateBegin.Value));
				}
				if (this.filterHistoryActiveToDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_AktivTil(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.filterHistoryActiveToDateEnd.Value));
				}

				if (this.filterHistoryChangedDateBegin.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_SidstAendret(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.GreaterThanOrEquals, this.filterHistoryChangedDateBegin.Value));
				}
				if (this.filterHistoryChangedDateEnd.Checked == true) {
					employeeFilters.Add(new SofdEmployeeFilter_SidstAendret(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.LessThanOrEquals, this.filterHistoryChangedDateEnd.Value));
				}

				// Get employees.
				List<SofdEmployee> employees = this.GetAllEmployees(employeeFilters.ToArray());

				// Populate the list with the employees.
				this.resultList.DataSource = null;
				this.resultList.DataSource = employees;

				// Status.
				this.resultListStatus.Text = String.Format("{0} employees found - Searching Active Directory...", this.resultList.RowCount);

				// Update the list with AD data.
				// Loop until the service is stopped.
				Int32 employeeIndex = 0;
				while ((this.searchThreadShutdownEvent.WaitOne(0) == false) && (employeeIndex < this.resultList.RowCount)) {
					try {
						// Get the current row.
						DataGridViewRow row = this.resultList.Rows[employeeIndex];

						// Get the SOFD employee.
						SofdEmployee employee = (SofdEmployee)row.DataBoundItem;

						// Get the AD user.
						Person user = this.GetUser(employee.AdBrugerNavn);

						// Update the AD status.
						DataGridViewImageCell resultListAdStatus = (DataGridViewImageCell)row.Cells["resultListAdStatus"];
						if (user == null) {
							resultListAdStatus.Value = this.GetResourceImage("Circle Grey.png");
						} else if (user.Enabled == false) {
							resultListAdStatus.Value = this.GetResourceImage("Circle Red.png");
						} else if (user.IsAccountLockedOut() == true) {
							resultListAdStatus.Value = this.GetResourceImage("Circle Yellow.png");
						} else {
							resultListAdStatus.Value = this.GetResourceImage("Circle Green.png");
						}

					} catch (ThreadAbortException exception) {
						throw;
					} catch (Exception exception) {
					} finally {
						// Iterate.
						employeeIndex++;
					}
				}
			} catch (ThreadAbortException exception) {
				// The thread was aborted.
			} catch (Exception exception) {
			} finally {
				// Status.
				this.resultListStatus.Text = String.Format("{0} employees found", this.resultList.RowCount);
			}
		} // SearchRunThread

		private void ResultListSelectionChanged(Object sender, EventArgs e) {
			try {
				// Get the selection count.
				Int32 selectionCount = this.resultList.SelectedRows.Count;

				// Get the SOFD employee.
				SofdEmployee employee = (selectionCount != 1) ? null : (SofdEmployee)this.resultList.SelectedRows[0].DataBoundItem;

				// Get the AD user.
				Person user = (employee == null) ? null : this.GetUser(employee.AdBrugerNavn);

				// Enable buttons.
				this.actionProperties.Enabled = selectionCount == 1;
				this.actionAdEnableUser.Enabled = ((user != null) && (user.Enabled == false));
				this.actionAdDisableUser.Enabled = ((user != null) && (user.Enabled == true));

			} catch { }
		} // ResultListSelectionChanged

		private void ResultListDataError(Object sender, DataGridViewDataErrorEventArgs e) {
			// Trapping errors.
			e.Cancel = true;
			e.ThrowException = false;
		} // ResultListDataError
		#endregion

		#region Action methods.
		private void ActionPanelControlAdded(Object sender, ControlEventArgs e) {
			try {
				// Set the width of the control.
				e.Control.Width = this.actionPanel.Width - this.actionPanel.Margin.Horizontal;
			} catch { }
		} // ActionPannelSizeChanged

		private void ActionPropertiesClick(Object sender, EventArgs e) {
			try {
				if (this.resultList.SelectedRows.Count == 1) {
					// Clear the properties.
					this.PropertyClear();

					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.resultList.SelectedRows[0].DataBoundItem;

					// Populate the properties.
					this.PropertyPopulate(employee);

					// Show the properties tab.
					this.tabControl1.SelectTab(1);
				}
			} catch (Exception exception) {
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionPropertiesClick

		private void ActionAdEnableUserClick(Object sender, EventArgs e) {
			try {
				if (this.resultList.SelectedRows.Count == 1) {
					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.resultList.SelectedRows[0].DataBoundItem;

					// Get the AD user.
					Person user = this.GetUser(employee.AdBrugerNavn);
					if (user == null) {
						throw new Exception("The user was not found in Active Directory.");
					}

					// Enable the user.
					user.Enabled = true;
					user.Save();

				}
			} catch (Exception exception) {
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionAdEnableUserClick

		private void ActionAdDisableUserClick(Object sender, EventArgs e) {
			try {
				if (this.resultList.SelectedRows.Count == 1) {
					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.resultList.SelectedRows[0].DataBoundItem;

					// Get the AD user.
					Person user = this.GetUser(employee.AdBrugerNavn);
					if (user == null) {
						throw new Exception("The user was not found in Active Directory.");
					}

					// Enable the user.
					user.Enabled = false;
					user.Save();

				}
			} catch (Exception exception) {
				this.LogError(exception);
				MessageBox.Show(exception.Message, "Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		} // ActionAdDisableUserClick
		#endregion

		#region Property methods.
		private void PropertyClear() {
			try {
				this.propertyCprNumber.Clear();
				this.propertyWorkerId.Clear();
				this.propertyEmployeeNumber.Clear();
				this.propertyEmployeeHistoryNumber.Clear();
				this.propertyAdUserName.Clear();
				this.propertyOpusName.Clear();
				this.propertyActiveImage.Image = null;
				this.propertyActive.Clear();
				this.propertyAdStatusImage.Image = null;
				this.propertyAdStatus.Clear();

				this.propertyFirstName.Clear();
				this.propertyLastName.Clear();
				this.propertyName.Clear();
				this.propertyDisplayName.Clear();
				this.propertyAdDisplayName.Clear();
				this.propertyPhone.Clear();
				this.propertyMobile.Clear();
				this.propertyHomeAddress.Clear();

				this.propertyEmploymentFirstDate.Clear();
				this.propertyEmploymentLastDate.Clear();
				this.propertyEmploymentOldestFirstDate.Clear();
				this.propertyEmploymentJubileeDate.Clear();
				this.propertyTitle.Clear();
				this.propertyOrganizationName.Clear();
				this.propertyLeaderName.Clear();
				this.propertyAdExpiresDate.Clear();

				this.propertyHistoryActiveFromDate.Clear();
				this.propertyHistoryActiveToDate.Clear();
				this.propertyHistoryChangedDate.Clear();
				this.propertyHistoryOpusChangedDate.Clear();
				this.propertyAdChangedDate.Clear();
				this.propertyAdLogonDate.Clear();
				this.propertyAdInfo.Clear();

				this.propertyHistoryList.DataSource = null;
			} catch {}
		} // PropertyClear

		private void PropertyPopulate(SofdEmployee employee) {
			try {
				if (employee != null) {
					// Get the AD user.
					Person user = this.GetUser(employee.AdBrugerNavn);

					// Populate the controls.
					this.propertyCprNumber.Text = employee.CprNummer;
					this.propertyWorkerId.Text = employee.MaNummer.ToString();
					this.propertyEmployeeNumber.Text = employee.MedarbejderId.ToString();
					this.propertyEmployeeHistoryNumber.Text = employee.MedarbejderHistorikId.ToString();
					this.propertyAdUserName.Text = employee.AdBrugerNavn;
					this.propertyOpusName.Text = employee.OpusBrugerNavn;
					if (employee.Aktiv == true) {
						this.propertyActiveImage.Image = this.GetResourceImage("Circle Green.png");
					} else {
						this.propertyActiveImage.Image = this.GetResourceImage("Circle Red.png");
					}
					this.propertyActive.Text = (employee.Aktiv == true) ? "Active" : "Not Active";

					if (user == null) {
						this.propertyAdStatusImage.Image = this.GetResourceImage("Circle Grey.png");
						this.propertyAdStatus.Text = "Not found in Active Directory";
					} else if (user.Enabled == false) {
						this.propertyAdStatusImage.Image = this.GetResourceImage("Circle Red.png");
						this.propertyAdStatus.Text = "This user is disabled";
					} else if (user.IsAccountLockedOut() == true) {
						this.propertyAdStatusImage.Image = this.GetResourceImage("Circle Yellow.png");
						this.propertyAdStatus.Text = String.Format("Lockedout since {0:yyyy-MM-dd HH:mm}", user.AccountLockoutTime.Value);
					} else {
						this.propertyAdStatusImage.Image = this.GetResourceImage("Circle Green.png");
						this.propertyAdStatus.Text = "Active";
					}

					this.propertyFirstName.Text = employee.ForNavn;
					this.propertyLastName.Text = employee.EfterNavn;
					this.propertyName.Text = employee.Navn;
					this.propertyDisplayName.Text = employee.KaldeNavn;
					if (user != null) {
						this.propertyAdDisplayName.Text = user.DisplayName;
					}
					this.propertyPhone.Text = employee.TelefonNummer;
					this.propertyMobile.Text = employee.MobilNummer;
					this.propertyHomeAddress.Text = employee.AdresseText;

					if (employee.AnsaettelsesDato.Year > 1900) {
						this.propertyEmploymentFirstDate.Text = employee.AnsaettelsesDato.ToString("yyyy-MM-dd");
					}
					if (employee.AnsaettelsesOphoersDato.Year > 1900) {
						this.propertyEmploymentLastDate.Text = employee.AnsaettelsesOphoersDato.ToString("yyyy-MM-dd");
					}
					if (employee.FoersteAnsaettelsesDato.Year > 1900) {
						this.propertyEmploymentOldestFirstDate.Text = employee.FoersteAnsaettelsesDato.ToString("yyyy-MM-dd");
					}
					if (employee.JubilaeumsAnciennitetsDato.Year > 1900) {
						this.propertyEmploymentJubileeDate.Text = employee.JubilaeumsAnciennitetsDato.ToString("yyyy-MM-dd");
					}

					this.propertyTitle.Text = employee.StillingsBetegnelse;
					this.propertyOrganizationName.Text = employee.OrganisationNavn;
					this.propertyLeaderName.Text = employee.NaermesteLederNavn;

					if ((user != null) && (user.AccountExpirationDate != null) && (user.AccountExpirationDate.Value.Year > 1900)) {
						this.propertyAdLogonDate.Text = user.AccountExpirationDate.Value.ToString("yyyy-MM-dd  HH:mm");
					}


					if (employee.AktivFra.Year > 1900) {
						this.propertyHistoryActiveFromDate.Text = employee.AktivFra.ToString("yyyy-MM-dd");
					}
					if (employee.AktivTil.Year > 1900) {
						this.propertyHistoryActiveToDate.Text = employee.AktivTil.ToString("yyyy-MM-dd");
					}
					if (employee.SidstAendret.Year > 1900) {
						this.propertyHistoryChangedDate.Text = employee.SidstAendret.ToString("yyyy-MM-dd");
					}
					if (employee.OpusSidsstAendret.Year > 1900) {
						this.propertyHistoryOpusChangedDate.Text = employee.OpusSidsstAendret.ToString("yyyy-MM-dd");
					}

					if (user != null) {
						this.propertyAdChangedDate.Text = "";
					}
					if ((user != null) && (user.LastLogon != null) && (user.LastLogon.Value.Year > 1900)) {
						this.propertyAdLogonDate.Text = user.LastLogon.Value.ToString("yyyy-MM-dd  HH:mm");
					}

					if (user != null) {
						this.propertyAdInfo.Text = user.Info;
					}

					// Get history.
					List<SofdEmployee> employees = this.GetAllEmployees(
						new SofdEmployeeFilter_MedarbejderId(SqlWhereFilterOperator.AND, SqlWhereFilterValueOperator.Equals, employee.MedarbejderId)
					);
					this.propertyHistoryList.AutoGenerateColumns = false;
					this.propertyHistoryList.DataSource = null;
					this.propertyHistoryList.DataSource = employees;
					foreach (DataGridViewRow row in this.propertyHistoryList.Rows) {
						row.Selected = (employee.Equals(row.DataBoundItem) == true);
					}
				}
			} catch (Exception exception) {
			}
		} // PropertyPopulate

		private void PropertyHistoryListClick(Object sender, EventArgs e) {
			try {
				if ((this.propertyHistoryList.Focused == true) && (this.propertyHistoryList.SelectedRows.Count == 1)) {
					// Unfocus the history list.
					this.propertyClose.Focus();

					// Get the SOFD employee.
					SofdEmployee employee = (SofdEmployee)this.propertyHistoryList.SelectedRows[0].DataBoundItem;
					//MessageBox.Show(employee.MedarbejderHistorikId.ToString());

					// Clear the properties.
					this.PropertyClear();

					// Populate the properties.
					this.PropertyPopulate(employee);

					// Show the properties tab.
					//this.tabControl1.SelectTab(1);
				}
			} catch { }
		} // PropertyHistoryListClick

		private void PropertyCloseClick(Object sender, EventArgs e) {
			// Show the search tab.
			this.tabControl1.SelectTab(0);
		} // PropertyCloseClick
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

} // NDK.SofdViewer