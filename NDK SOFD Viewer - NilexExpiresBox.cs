using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NDK.SofdViewer {
	public partial class NilexExpiresBox : Form {

		public NilexExpiresBox() {
			InitializeComponent();
			this.dataDate.MinDate = DateTime.Now.Date.AddDays(1);
			this.dataDate.MaxDate = DateTime.Now.AddYears(99);
		} // NilexExpiresBox

		public Int32 Number {
			get {
				try {
					return Int32.Parse(this.dataNumber.Text);
				} catch {
					return 0;
				}
			}
			set {
				try {
					this.dataNumber.Text = Number.ToString();
				} catch { }
			}
		} // Number

		public DateTime Date {
			get {
				return this.dataDate.Value.Date;
			}
			set {
				try {
					this.dataDate.Value = value;
				} catch { }
			}
		} // Date

		public String Message {
			get {
				return this.dataMessage.Text;
			}
			set {
				if (value != null) {
					this.dataMessage.Text = value;
				} else {
					this.dataMessage.Text = String.Empty;
				}
			}
		} // Message

		private void Button1WeekClick(object sender, EventArgs e) {
			this.dataDate.Value = DateTime.Now.Date.AddDays(7);
			this.dataDate.Focus();
		} // Button1WeekClick

		private void Button2WeeksClick(object sender, EventArgs e) {
			this.dataDate.Value = DateTime.Now.Date.AddDays(14);
			this.dataDate.Focus();
		} // Button2WeeksClick

		private void Button1MonthClick(object sender, EventArgs e) {
			this.dataDate.Value = DateTime.Now.Date.AddMonths(1);
			this.dataDate.Focus();
		} // Button1MonthClick

		private void Button3MonthsClick(object sender, EventArgs e) {
			this.dataDate.Value = DateTime.Now.Date.AddMonths(3);
			this.dataDate.Focus();
		} // Button3MonthsClick

	} // NilexExpiresBox

} // NDK.SofdViewer