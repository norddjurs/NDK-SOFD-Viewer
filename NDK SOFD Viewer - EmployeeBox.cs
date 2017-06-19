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

	} // EmployeeBox

} // NDK.SofdViewer