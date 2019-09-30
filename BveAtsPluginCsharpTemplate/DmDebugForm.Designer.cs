namespace AtsPlugin
{
    partial class DmDebugForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TargetRpm = new System.Windows.Forms.Label();
            this.ActualRpm = new System.Windows.Forms.Label();
            this.GovernerRatio = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.FuelInjectionCurrent = new System.Windows.Forms.Label();
            this.EngineRpm = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.CurrentDrivingState = new System.Windows.Forms.Label();
            this.CcsSyncingState = new System.Windows.Forms.Label();
            this.BackwardClutch = new System.Windows.Forms.Label();
            this.ForwardClutch = new System.Windows.Forms.Label();
            this.Gear2Clutch = new System.Windows.Forms.Label();
            this.Gear1Clutch = new System.Windows.Forms.Label();
            this.MissionClutch = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TargetRpm);
            this.groupBox1.Controls.Add(this.ActualRpm);
            this.groupBox1.Controls.Add(this.GovernerRatio);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(151, 196);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Governer";
            // 
            // TargetRpm
            // 
            this.TargetRpm.AutoSize = true;
            this.TargetRpm.Location = new System.Drawing.Point(25, 158);
            this.TargetRpm.Name = "TargetRpm";
            this.TargetRpm.Size = new System.Drawing.Size(35, 12);
            this.TargetRpm.TabIndex = 5;
            this.TargetRpm.Text = "label2";
            // 
            // ActualRpm
            // 
            this.ActualRpm.AutoSize = true;
            this.ActualRpm.Location = new System.Drawing.Point(25, 86);
            this.ActualRpm.Name = "ActualRpm";
            this.ActualRpm.Size = new System.Drawing.Size(35, 12);
            this.ActualRpm.TabIndex = 4;
            this.ActualRpm.Text = "label1";
            // 
            // GovernerRatio
            // 
            this.GovernerRatio.AutoSize = true;
            this.GovernerRatio.Location = new System.Drawing.Point(25, 28);
            this.GovernerRatio.Name = "GovernerRatio";
            this.GovernerRatio.Size = new System.Drawing.Size(35, 12);
            this.GovernerRatio.TabIndex = 3;
            this.GovernerRatio.Text = "label1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.FuelInjectionCurrent);
            this.groupBox2.Controls.Add(this.EngineRpm);
            this.groupBox2.Location = new System.Drawing.Point(169, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 147);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Engine";
            // 
            // FuelInjectionCurrent
            // 
            this.FuelInjectionCurrent.AutoSize = true;
            this.FuelInjectionCurrent.Location = new System.Drawing.Point(25, 74);
            this.FuelInjectionCurrent.Name = "FuelInjectionCurrent";
            this.FuelInjectionCurrent.Size = new System.Drawing.Size(35, 12);
            this.FuelInjectionCurrent.TabIndex = 5;
            this.FuelInjectionCurrent.Text = "label1";
            // 
            // EngineRpm
            // 
            this.EngineRpm.AutoSize = true;
            this.EngineRpm.Location = new System.Drawing.Point(24, 28);
            this.EngineRpm.Name = "EngineRpm";
            this.EngineRpm.Size = new System.Drawing.Size(35, 12);
            this.EngineRpm.TabIndex = 0;
            this.EngineRpm.Text = "label1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.CurrentDrivingState);
            this.groupBox3.Controls.Add(this.CcsSyncingState);
            this.groupBox3.Controls.Add(this.BackwardClutch);
            this.groupBox3.Controls.Add(this.ForwardClutch);
            this.groupBox3.Controls.Add(this.Gear2Clutch);
            this.groupBox3.Controls.Add(this.Gear1Clutch);
            this.groupBox3.Controls.Add(this.MissionClutch);
            this.groupBox3.Location = new System.Drawing.Point(12, 214);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(357, 236);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Transmission";
            // 
            // CurrentDrivingState
            // 
            this.CurrentDrivingState.AutoSize = true;
            this.CurrentDrivingState.Location = new System.Drawing.Point(82, 154);
            this.CurrentDrivingState.Name = "CurrentDrivingState";
            this.CurrentDrivingState.Size = new System.Drawing.Size(35, 12);
            this.CurrentDrivingState.TabIndex = 10;
            this.CurrentDrivingState.Text = "label1";
            // 
            // CcsSyncingState
            // 
            this.CcsSyncingState.AutoSize = true;
            this.CcsSyncingState.Location = new System.Drawing.Point(82, 191);
            this.CcsSyncingState.Name = "CcsSyncingState";
            this.CcsSyncingState.Size = new System.Drawing.Size(35, 12);
            this.CcsSyncingState.TabIndex = 9;
            this.CcsSyncingState.Text = "label1";
            // 
            // BackwardClutch
            // 
            this.BackwardClutch.AutoSize = true;
            this.BackwardClutch.Location = new System.Drawing.Point(139, 65);
            this.BackwardClutch.Name = "BackwardClutch";
            this.BackwardClutch.Size = new System.Drawing.Size(35, 12);
            this.BackwardClutch.TabIndex = 8;
            this.BackwardClutch.Text = "label1";
            // 
            // ForwardClutch
            // 
            this.ForwardClutch.AutoSize = true;
            this.ForwardClutch.Location = new System.Drawing.Point(139, 27);
            this.ForwardClutch.Name = "ForwardClutch";
            this.ForwardClutch.Size = new System.Drawing.Size(35, 12);
            this.ForwardClutch.TabIndex = 7;
            this.ForwardClutch.Text = "label4";
            // 
            // Gear2Clutch
            // 
            this.Gear2Clutch.AutoSize = true;
            this.Gear2Clutch.Location = new System.Drawing.Point(25, 108);
            this.Gear2Clutch.Name = "Gear2Clutch";
            this.Gear2Clutch.Size = new System.Drawing.Size(35, 12);
            this.Gear2Clutch.TabIndex = 6;
            this.Gear2Clutch.Text = "label3";
            // 
            // Gear1Clutch
            // 
            this.Gear1Clutch.AutoSize = true;
            this.Gear1Clutch.Location = new System.Drawing.Point(25, 65);
            this.Gear1Clutch.Name = "Gear1Clutch";
            this.Gear1Clutch.Size = new System.Drawing.Size(35, 12);
            this.Gear1Clutch.TabIndex = 5;
            this.Gear1Clutch.Text = "label2";
            // 
            // MissionClutch
            // 
            this.MissionClutch.AutoSize = true;
            this.MissionClutch.Location = new System.Drawing.Point(25, 27);
            this.MissionClutch.Name = "MissionClutch";
            this.MissionClutch.Size = new System.Drawing.Size(35, 12);
            this.MissionClutch.TabIndex = 4;
            this.MissionClutch.Text = "label1";
            // 
            // DmDebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 462);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "DmDebugForm";
            this.Text = "DmDebugForm";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.Label TargetRpm;
        public System.Windows.Forms.Label ActualRpm;
        public System.Windows.Forms.Label GovernerRatio;
        public System.Windows.Forms.Label EngineRpm;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.Label FuelInjectionCurrent;
        public System.Windows.Forms.Label ForwardClutch;
        public System.Windows.Forms.Label Gear2Clutch;
        public System.Windows.Forms.Label Gear1Clutch;
        public System.Windows.Forms.Label MissionClutch;
        public System.Windows.Forms.Label BackwardClutch;
        public System.Windows.Forms.Label CurrentDrivingState;
        public System.Windows.Forms.Label CcsSyncingState;
    }
}