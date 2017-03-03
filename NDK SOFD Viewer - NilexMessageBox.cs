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
	public partial class NilexMessageBox : Form {

		public NilexMessageBox() {
			InitializeComponent();
		} // NilexMessageBox

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

	} // NilexMessageBox

} // NDK.SofdViewer