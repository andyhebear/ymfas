namespace Ymfas {
    partial class frmGameLobby {
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
			this.components = new System.ComponentModel.Container();
			this.rtxtChatWindow = new System.Windows.Forms.RichTextBox();
			this.txtChatInput = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lstPlayers = new System.Windows.Forms.ListBox();
			this.chkReady = new System.Windows.Forms.CheckBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.cmbGameMode = new System.Windows.Forms.ComboBox();
			this.cmbTeam = new System.Windows.Forms.ComboBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// rtxtChatWindow
			// 
			this.rtxtChatWindow.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rtxtChatWindow.Location = new System.Drawing.Point(6, 19);
			this.rtxtChatWindow.Name = "rtxtChatWindow";
			this.rtxtChatWindow.ReadOnly = true;
			this.rtxtChatWindow.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.rtxtChatWindow.Size = new System.Drawing.Size(400, 285);
			this.rtxtChatWindow.TabIndex = 0;
			this.rtxtChatWindow.Text = "";
			// 
			// txtChatInput
			// 
			this.txtChatInput.Location = new System.Drawing.Point(6, 310);
			this.txtChatInput.Multiline = true;
			this.txtChatInput.Name = "txtChatInput";
			this.txtChatInput.Size = new System.Drawing.Size(400, 20);
			this.txtChatInput.TabIndex = 1;
			this.txtChatInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtChatInput_KeyDown);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.rtxtChatWindow);
			this.groupBox1.Controls.Add(this.txtChatInput);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(412, 336);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Chat";
			// 
			// lstPlayers
			// 
			this.lstPlayers.FormattingEnabled = true;
			this.lstPlayers.Location = new System.Drawing.Point(430, 31);
			this.lstPlayers.Name = "lstPlayers";
			this.lstPlayers.Size = new System.Drawing.Size(182, 225);
			this.lstPlayers.TabIndex = 3;
			// 
			// chkReady
			// 
			this.chkReady.AutoSize = true;
			this.chkReady.Location = new System.Drawing.Point(430, 324);
			this.chkReady.Name = "chkReady";
			this.chkReady.Size = new System.Drawing.Size(57, 17);
			this.chkReady.TabIndex = 4;
			this.chkReady.Text = "Ready";
			this.chkReady.UseVisualStyleBackColor = true;
			this.chkReady.CheckedChanged += new System.EventHandler(this.chkReady_CheckedChanged);
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(493, 316);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(118, 26);
			this.btnStart.TabIndex = 5;
			this.btnStart.Text = "&Start Game";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Visible = false;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// cmbGameMode
			// 
			this.cmbGameMode.Enabled = false;
			this.cmbGameMode.FormattingEnabled = true;
			this.cmbGameMode.Items.AddRange(new object[] {
            "Deathmatch",
            "Team Deathmatch",
            "Capture The Flag",
            "Convoy Defense",
            "King Of The Asteroid"});
			this.cmbGameMode.Location = new System.Drawing.Point(430, 289);
			this.cmbGameMode.Name = "cmbGameMode";
			this.cmbGameMode.Size = new System.Drawing.Size(181, 21);
			this.cmbGameMode.TabIndex = 6;
			this.cmbGameMode.SelectedIndexChanged += new System.EventHandler(this.cmbGameMode_SelectedIndexChanged);
			// 
			// cmbTeam
			// 
			this.cmbTeam.Enabled = false;
			this.cmbTeam.Items.AddRange(new object[] {
            "No Team",
            "Team 1",
            "Team 2"});
			this.cmbTeam.Location = new System.Drawing.Point(432, 262);
			this.cmbTeam.Name = "cmbTeam";
			this.cmbTeam.Size = new System.Drawing.Size(180, 21);
			this.cmbTeam.TabIndex = 7;
			this.cmbTeam.SelectedIndexChanged += new System.EventHandler(this.cmbTeam_SelectedIndexChanged);
			// 
			// frmGameLobby
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(632, 356);
			this.Controls.Add(this.cmbTeam);
			this.Controls.Add(this.cmbGameMode);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.chkReady);
			this.Controls.Add(this.lstPlayers);
			this.Controls.Add(this.groupBox1);
			this.MaximumSize = new System.Drawing.Size(640, 390);
			this.MinimumSize = new System.Drawing.Size(640, 390);
			this.Name = "frmGameLobby";
			this.Text = "frmGameLobby";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmGameLobby_FormClosing);
			this.Load += new System.EventHandler(this.frmGameLobby_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtxtChatWindow;
        private System.Windows.Forms.TextBox txtChatInput;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lstPlayers;
        private System.Windows.Forms.CheckBox chkReady;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ComboBox cmbGameMode;
        private System.Windows.Forms.ComboBox cmbTeam;
    }
}