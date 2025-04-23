namespace ReferenceConversion.Presentation
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Btn_GetFolder = new Button();
            Tb_ShowPath = new TextBox();
            Lb_ShowAllCsproj = new ListBox();
            groupBox1 = new GroupBox();
            Lb_Allowlist = new ListBox();
            label3 = new Label();
            Cbx_Project_Allowlist = new ComboBox();
            groupBox2 = new GroupBox();
            Cbx_solution_file = new ComboBox();
            label2 = new Label();
            Btn_ToReference = new Button();
            Btn_ToProjectReference = new Button();
            label1 = new Label();
            Tb_Ref_Path = new TextBox();
            Lb_Convert_Status = new Label();
            Btn_ToggleLog = new Button();
            Lb_Log_Status = new Label();
            Pnl_Log = new Panel();
            Lb_Log_Path = new Label();
            Lb_Log = new ListBox();
            Btn_Clear_Log = new Button();
            label4 = new Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            Pnl_Log.SuspendLayout();
            SuspendLayout();
            // 
            // Btn_GetFolder
            // 
            Btn_GetFolder.Location = new Point(444, 196);
            Btn_GetFolder.Name = "Btn_GetFolder";
            Btn_GetFolder.Size = new Size(82, 29);
            Btn_GetFolder.TabIndex = 0;
            Btn_GetFolder.Text = "選取";
            Btn_GetFolder.UseVisualStyleBackColor = true;
            Btn_GetFolder.Click += Btn_GetFolder_Click;
            // 
            // Tb_ShowPath
            // 
            Tb_ShowPath.Location = new Point(90, 198);
            Tb_ShowPath.Name = "Tb_ShowPath";
            Tb_ShowPath.Size = new Size(345, 23);
            Tb_ShowPath.TabIndex = 1;
            // 
            // Lb_ShowAllCsproj
            // 
            Lb_ShowAllCsproj.FormattingEnabled = true;
            Lb_ShowAllCsproj.HorizontalScrollbar = true;
            Lb_ShowAllCsproj.ItemHeight = 15;
            Lb_ShowAllCsproj.Location = new Point(6, 24);
            Lb_ShowAllCsproj.Name = "Lb_ShowAllCsproj";
            Lb_ShowAllCsproj.Size = new Size(504, 154);
            Lb_ShowAllCsproj.TabIndex = 2;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(Lb_Allowlist);
            groupBox1.Location = new Point(8, 40);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(5);
            groupBox1.Size = new Size(518, 152);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "要替換的參考列表";
            // 
            // Lb_Allowlist
            // 
            Lb_Allowlist.FormattingEnabled = true;
            Lb_Allowlist.HorizontalScrollbar = true;
            Lb_Allowlist.ItemHeight = 15;
            Lb_Allowlist.Location = new Point(0, 20);
            Lb_Allowlist.Name = "Lb_Allowlist";
            Lb_Allowlist.Size = new Size(502, 124);
            Lb_Allowlist.TabIndex = 8;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(8, 16);
            label3.Name = "label3";
            label3.Size = new Size(67, 15);
            label3.TabIndex = 5;
            label3.Text = "專案名稱：";
            // 
            // Cbx_Project_Allowlist
            // 
            Cbx_Project_Allowlist.FormattingEnabled = true;
            Cbx_Project_Allowlist.Location = new Point(90, 13);
            Cbx_Project_Allowlist.Name = "Cbx_Project_Allowlist";
            Cbx_Project_Allowlist.Size = new Size(294, 23);
            Cbx_Project_Allowlist.TabIndex = 7;
            Cbx_Project_Allowlist.Text = "請選取專案名稱";
            Cbx_Project_Allowlist.SelectedIndexChanged += Cbx_Project_Allowlist_SelectedIndexChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(Lb_ShowAllCsproj);
            groupBox2.Location = new Point(8, 252);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(5);
            groupBox2.Size = new Size(518, 187);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "專案內所有 csproj 檔";
            // 
            // Cbx_solution_file
            // 
            Cbx_solution_file.FormattingEnabled = true;
            Cbx_solution_file.Location = new Point(90, 227);
            Cbx_solution_file.Name = "Cbx_solution_file";
            Cbx_solution_file.Size = new Size(294, 23);
            Cbx_solution_file.TabIndex = 4;
            Cbx_solution_file.Text = "請選取 Solution File";
            Cbx_solution_file.SelectedIndexChanged += Cbx_solution_file_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 230);
            label2.Name = "label2";
            label2.Size = new Size(76, 15);
            label2.TabIndex = 3;
            label2.Text = "專案 sln 檔：";
            // 
            // Btn_ToReference
            // 
            Btn_ToReference.Location = new Point(8, 490);
            Btn_ToReference.Name = "Btn_ToReference";
            Btn_ToReference.Size = new Size(120, 40);
            Btn_ToReference.TabIndex = 8;
            Btn_ToReference.Text = "轉成 檔案參考";
            Btn_ToReference.UseVisualStyleBackColor = true;
            Btn_ToReference.Click += Btn_ToReference_Click;
            // 
            // Btn_ToProjectReference
            // 
            Btn_ToProjectReference.Location = new Point(135, 490);
            Btn_ToProjectReference.Name = "Btn_ToProjectReference";
            Btn_ToProjectReference.Size = new Size(120, 40);
            Btn_ToProjectReference.TabIndex = 9;
            Btn_ToProjectReference.Text = "轉成 專案參考";
            Btn_ToProjectReference.UseVisualStyleBackColor = true;
            Btn_ToProjectReference.Click += Btn_ToProjectReference_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 203);
            label1.Name = "label1";
            label1.Size = new Size(79, 15);
            label1.TabIndex = 10;
            label1.Text = "專案資料夾：";
            // 
            // Tb_Ref_Path
            // 
            Tb_Ref_Path.BackColor = SystemColors.HighlightText;
            Tb_Ref_Path.Location = new Point(135, 448);
            Tb_Ref_Path.Name = "Tb_Ref_Path";
            Tb_Ref_Path.PlaceholderText = "DLL 預設路徑";
            Tb_Ref_Path.ReadOnly = true;
            Tb_Ref_Path.Size = new Size(391, 23);
            Tb_Ref_Path.TabIndex = 12;
            // 
            // Lb_Convert_Status
            // 
            Lb_Convert_Status.AutoSize = true;
            Lb_Convert_Status.Location = new Point(8, 536);
            Lb_Convert_Status.Name = "Lb_Convert_Status";
            Lb_Convert_Status.Size = new Size(55, 15);
            Lb_Convert_Status.TabIndex = 14;
            Lb_Convert_Status.Text = "轉換狀態";
            // 
            // Btn_ToggleLog
            // 
            Btn_ToggleLog.Location = new Point(309, 494);
            Btn_ToggleLog.Name = "Btn_ToggleLog";
            Btn_ToggleLog.Size = new Size(75, 23);
            Btn_ToggleLog.TabIndex = 15;
            Btn_ToggleLog.Text = "Log 狀態";
            Btn_ToggleLog.UseVisualStyleBackColor = true;
            Btn_ToggleLog.Click += Btn_ToggleLog_Click;
            // 
            // Lb_Log_Status
            // 
            Lb_Log_Status.AutoSize = true;
            Lb_Log_Status.Location = new Point(390, 498);
            Lb_Log_Status.Name = "Lb_Log_Status";
            Lb_Log_Status.Size = new Size(59, 15);
            Lb_Log_Status.TabIndex = 16;
            Lb_Log_Status.Text = "Log: 關閉";
            // 
            // Pnl_Log
            // 
            Pnl_Log.Controls.Add(Lb_Log_Path);
            Pnl_Log.Controls.Add(Lb_Log);
            Pnl_Log.Location = new Point(551, 8);
            Pnl_Log.Name = "Pnl_Log";
            Pnl_Log.Size = new Size(805, 559);
            Pnl_Log.TabIndex = 17;
            // 
            // Lb_Log_Path
            // 
            Lb_Log_Path.AutoSize = true;
            Lb_Log_Path.Location = new Point(3, 8);
            Lb_Log_Path.Name = "Lb_Log_Path";
            Lb_Log_Path.Size = new Size(80, 15);
            Lb_Log_Path.TabIndex = 20;
            Lb_Log_Path.Text = "Log 存放路徑";
            // 
            // Lb_Log
            // 
            Lb_Log.FormattingEnabled = true;
            Lb_Log.HorizontalScrollbar = true;
            Lb_Log.ItemHeight = 15;
            Lb_Log.Location = new Point(3, 27);
            Lb_Log.Name = "Lb_Log";
            Lb_Log.ScrollAlwaysVisible = true;
            Lb_Log.Size = new Size(799, 529);
            Lb_Log.TabIndex = 0;
            // 
            // Btn_Clear_Log
            // 
            Btn_Clear_Log.Location = new Point(309, 523);
            Btn_Clear_Log.Name = "Btn_Clear_Log";
            Btn_Clear_Log.Size = new Size(75, 23);
            Btn_Clear_Log.TabIndex = 18;
            Btn_Clear_Log.Text = "清除 Log";
            Btn_Clear_Log.UseVisualStyleBackColor = true;
            Btn_Clear_Log.Click += Btn_Clear_Log_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(8, 451);
            label4.Name = "label4";
            label4.Size = new Size(129, 15);
            label4.TabIndex = 19;
            label4.Text = "專案參考（dll）路徑：";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1364, 571);
            Controls.Add(Btn_Clear_Log);
            Controls.Add(Tb_Ref_Path);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(Cbx_solution_file);
            Controls.Add(label3);
            Controls.Add(Cbx_Project_Allowlist);
            Controls.Add(Pnl_Log);
            Controls.Add(Lb_Log_Status);
            Controls.Add(Btn_ToggleLog);
            Controls.Add(Lb_Convert_Status);
            Controls.Add(Btn_GetFolder);
            Controls.Add(Tb_ShowPath);
            Controls.Add(label1);
            Controls.Add(Btn_ToProjectReference);
            Controls.Add(Btn_ToReference);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            MaximumSize = new Size(1380, 610);
            MinimumSize = new Size(540, 610);
            Name = "Form1";
            Padding = new Padding(5);
            Text = "Reference Convert";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            Pnl_Log.ResumeLayout(false);
            Pnl_Log.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Btn_GetFolder;
        private TextBox Tb_ShowPath;
        private ListBox Lb_ShowAllCsproj;
        private GroupBox groupBox1;
        private ListBox Lb_Allowlist;
        private ComboBox Cbx_Project_Allowlist;
        private GroupBox groupBox2;
        private Button Btn_ToReference;
        private Button Btn_ToProjectReference;
        private Label label1;
        private ComboBox Cbx_solution_file;
        private Label label2;
        private TextBox Tb_Ref_Path;
        private Label Lb_Convert_Status;
        private Button Btn_ToggleLog;
        private Label Lb_Log_Status;
        private Label label3;
        private Panel Pnl_Log;
        private ListBox Lb_Log;
        private Button Btn_Clear_Log;
        private Label label4;
        private Label Lb_Log_Path;
    }
}
