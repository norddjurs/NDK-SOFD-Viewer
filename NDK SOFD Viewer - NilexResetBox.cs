using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NDK.SofdViewer {
	public partial class NilexResetBox : Form {

		public NilexResetBox() {
			InitializeComponent();
		} // NilexResetBox

		private void DataPasswordKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) {
			// Only allow A-Z and 0-9 characters.
			// x08 = Backspace
			if (Regex.IsMatch(e.KeyChar.ToString(), @"^[a-zA-Z0-9\x08]*$") == false) {
				// Stop the character from being entered into the control since it is illegal.
				e.Handled = true;
			}
		} // DataPasswordKeyPress

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

		public String Password {
			get {
				return this.dataPassword.Text;
			}
			set {
				if (value != null) {
					this.dataPassword.Text = value.Trim();
				} else {
					this.dataPassword.Text = String.Empty;
				}
			}
		} // Password

		public Boolean ExpirePassword {
			get {
				return this.dataExpirePassword.Checked;
			}
			set {
				this.dataExpirePassword.Checked = value;
			}
		} // ExpirePassword

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

	} // NilexResetBox

} // NDK.SofdViewer