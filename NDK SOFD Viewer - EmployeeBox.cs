using System;
using System.Windows.Forms;
using NDK.Framework;

namespace NDK.SofdViewer {
	public partial class EmployeeBox : Form {
		private SofdEmployee sofdEmployee = null;

		public EmployeeBox() {
			InitializeComponent();
		} // EmployeeBox

		public SofdEmployee SofdEmployee {
			get {
				return this.sofdEmployee;
			}
			set {
				this.sofdEmployee = value;
				this.employeePropertyEmployeeHistoryNumber.Text = this.sofdEmployee.MedarbejderHistorikId.ToString();
				this.employeePropertyEmployeeNumber.Text = this.sofdEmployee.MedarbejderId.ToString();
				this.employeePropertyWorkerId.Text = this.sofdEmployee.MaNummer.ToString();
				this.employeePropertyCprNumber.Text = this.sofdEmployee.CprNummer.ToString();
				this.employeePropertyName.Text = this.sofdEmployee.Navn;

				this.employeePropertyMiFareId.Text = this.sofdEmployee.MiFareId;
				this.employeePropertyPhone.Text = this.sofdEmployee.TelefonNummer.GetNotNull().FormatStringPhone();
				this.employeePropertyMobile1.Text = this.sofdEmployee.MobilNummer.GetNotNull().FormatStringPhone();
				this.employeePropertyMobile2.Text = this.sofdEmployee.MobilNummer2.GetNotNull().FormatStringPhone();
				this.employeePropertyEmail.Text = this.sofdEmployee.Epost.GetNotNull();
				this.employeePropertyIntern.Checked = this.sofdEmployee.Intern;
				this.employeePropertyExtern.Checked = this.sofdEmployee.Ekstern;
			}
		} // SofdEmployee

		public String MiFareId {
			get {
				return this.employeePropertyMiFareId.Text;
			}
			set {
				try {
					this.employeePropertyMiFareId.Text = value;
				} catch { }
			}
		} // MiFareId

		public String Phone {
			get {
				return this.employeePropertyPhone.Text.Trim().FormatStringPhone();
			}
			set {
				this.employeePropertyPhone.Text = value.GetNotNull().FormatStringPhone();
			}
		} // Phone

		public String Mobile1 {
			get {
				return this.employeePropertyMobile1.Text.Trim().FormatStringPhone();
			}
			set {
				this.employeePropertyMobile1.Text = value.GetNotNull().FormatStringPhone();
			}
		} // Mobile1

		public String Mobile2 {
			get {
				return this.employeePropertyMobile2.Text.Trim().FormatStringPhone();
			}
			set {
				this.employeePropertyMobile2.Text = value.GetNotNull().FormatStringPhone();
			}
		} // Mobile2

		public String Email {
			get {
				return this.employeePropertyEmail.Text.Trim();
			}
			set {
				this.employeePropertyEmail.Text = value.GetNotNull();
			}
		} // Email

		public Boolean Intern {
			get {
				return this.employeePropertyIntern.Checked;
			}
			set {
				this.employeePropertyIntern.Checked = value;
			}
		} // Intern

		public Boolean Extern {
			get {
				return this.employeePropertyExtern.Checked;
			}
			set {
				this.employeePropertyExtern.Checked = value;
			}
		} // Extern

	} // EmployeeBox

} // NDK.SofdViewer