﻿namespace NDK.SofdViewer {
	partial class NilexExpiresBox {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.label1 = new System.Windows.Forms.Label();
			this.dataNumber = new System.Windows.Forms.MaskedTextBox();
			this.dataMessage = new System.Windows.Forms.TextBox();
			this.buttonOk = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.dataDate = new System.Windows.Forms.DateTimePicker();
			this.label2 = new System.Windows.Forms.Label();
			this.button1Week = new System.Windows.Forms.Button();
			this.button2Weeks = new System.Windows.Forms.Button();
			this.button1Month = new System.Windows.Forms.Button();
			this.button3Months = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Nilex Number:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// dataNumber
			// 
			this.dataNumber.Location = new System.Drawing.Point(138, 12);
			this.dataNumber.Mask = "9999999";
			this.dataNumber.Name = "dataNumber";
			this.dataNumber.Size = new System.Drawing.Size(60, 22);
			this.dataNumber.TabIndex = 1;
			this.dataNumber.Text = "0000000";
			// 
			// dataMessage
			// 
			this.dataMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dataMessage.Cursor = System.Windows.Forms.Cursors.Default;
			this.dataMessage.Location = new System.Drawing.Point(12, 77);
			this.dataMessage.Multiline = true;
			this.dataMessage.Name = "dataMessage";
			this.dataMessage.ReadOnly = true;
			this.dataMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.dataMessage.Size = new System.Drawing.Size(560, 233);
			this.dataMessage.TabIndex = 2;
			this.dataMessage.TabStop = false;
			// 
			// buttonOk
			// 
			this.buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Location = new System.Drawing.Point(185, 324);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(100, 25);
			this.buttonOk.TabIndex = 3;
			this.buttonOk.Text = "&Ok";
			this.buttonOk.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(305, 324);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(100, 25);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// dataDate
			// 
			this.dataDate.CustomFormat = "dd.MM.yyyy";
			this.dataDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dataDate.Location = new System.Drawing.Point(138, 40);
			this.dataDate.Name = "dataDate";
			this.dataDate.Size = new System.Drawing.Size(89, 22);
			this.dataDate.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 43);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "Expiry Date:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// button1Week
			// 
			this.button1Week.Location = new System.Drawing.Point(233, 39);
			this.button1Week.Name = "button1Week";
			this.button1Week.Size = new System.Drawing.Size(75, 23);
			this.button1Week.TabIndex = 7;
			this.button1Week.Text = "1 Week";
			this.button1Week.UseVisualStyleBackColor = true;
			this.button1Week.Click += new System.EventHandler(this.Button1WeekClick);
			// 
			// button2Weeks
			// 
			this.button2Weeks.Location = new System.Drawing.Point(314, 39);
			this.button2Weeks.Name = "button2Weeks";
			this.button2Weeks.Size = new System.Drawing.Size(75, 23);
			this.button2Weeks.TabIndex = 8;
			this.button2Weeks.Text = "2 Weeks";
			this.button2Weeks.UseVisualStyleBackColor = true;
			this.button2Weeks.Click += new System.EventHandler(this.Button2WeeksClick);
			// 
			// button1Month
			// 
			this.button1Month.Location = new System.Drawing.Point(395, 39);
			this.button1Month.Name = "button1Month";
			this.button1Month.Size = new System.Drawing.Size(75, 23);
			this.button1Month.TabIndex = 9;
			this.button1Month.Text = "1 Month";
			this.button1Month.UseVisualStyleBackColor = true;
			this.button1Month.Click += new System.EventHandler(this.Button1MonthClick);
			// 
			// button3Months
			// 
			this.button3Months.Location = new System.Drawing.Point(476, 40);
			this.button3Months.Name = "button3Months";
			this.button3Months.Size = new System.Drawing.Size(75, 23);
			this.button3Months.TabIndex = 10;
			this.button3Months.Text = "3 Months";
			this.button3Months.UseVisualStyleBackColor = true;
			this.button3Months.Click += new System.EventHandler(this.Button3MonthsClick);
			// 
			// NilexExpiresBox
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(584, 361);
			this.ControlBox = false;
			this.Controls.Add(this.button3Months);
			this.Controls.Add(this.button1Month);
			this.Controls.Add(this.button2Weeks);
			this.Controls.Add(this.button1Week);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.dataDate);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.dataMessage);
			this.Controls.Add(this.dataNumber);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NilexExpiresBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Nilex MessageBox";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.MaskedTextBox dataNumber;
		private System.Windows.Forms.TextBox dataMessage;
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.DateTimePicker dataDate;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1Week;
		private System.Windows.Forms.Button button2Weeks;
		private System.Windows.Forms.Button button1Month;
		private System.Windows.Forms.Button button3Months;
	}
}