namespace ReferenceConversion
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
            Tb_SaveFolder = new TextBox();
            Btn_Convert = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // Btn_GetFolder
            // 
            Btn_GetFolder.Location = new Point(44, 27);
            Btn_GetFolder.Name = "Btn_GetFolder";
            Btn_GetFolder.Size = new Size(85, 29);
            Btn_GetFolder.TabIndex = 0;
            Btn_GetFolder.Text = "選取資料夾";
            Btn_GetFolder.UseVisualStyleBackColor = true;
            Btn_GetFolder.Click += Btn_GetFolder_Click;
            // 
            // Tb_ShowPath
            // 
            Tb_ShowPath.Location = new Point(135, 31);
            Tb_ShowPath.Name = "Tb_ShowPath";
            Tb_ShowPath.Size = new Size(384, 23);
            Tb_ShowPath.TabIndex = 1;
            // 
            // Lb_ShowAllCsproj
            // 
            Lb_ShowAllCsproj.FormattingEnabled = true;
            Lb_ShowAllCsproj.HorizontalScrollbar = true;
            Lb_ShowAllCsproj.ItemHeight = 15;
            Lb_ShowAllCsproj.Location = new Point(44, 80);
            Lb_ShowAllCsproj.Name = "Lb_ShowAllCsproj";
            Lb_ShowAllCsproj.Size = new Size(475, 184);
            Lb_ShowAllCsproj.TabIndex = 2;
            // 
            // Tb_SaveFolder
            // 
            Tb_SaveFolder.Location = new Point(213, 270);
            Tb_SaveFolder.Name = "Tb_SaveFolder";
            Tb_SaveFolder.Size = new Size(306, 23);
            Tb_SaveFolder.TabIndex = 3;
            // 
            // Btn_Convert
            // 
            Btn_Convert.Location = new Point(241, 334);
            Btn_Convert.Name = "Btn_Convert";
            Btn_Convert.Size = new Size(75, 23);
            Btn_Convert.TabIndex = 4;
            Btn_Convert.Text = "Convert";
            Btn_Convert.UseVisualStyleBackColor = true;
            Btn_Convert.Click += Btn_Convert_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(44, 273);
            label1.Name = "label1";
            label1.Size = new Size(169, 15);
            label1.TabIndex = 5;
            label1.Text = "Project Reference 指定資料夾";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(561, 369);
            Controls.Add(label1);
            Controls.Add(Btn_Convert);
            Controls.Add(Tb_SaveFolder);
            Controls.Add(Lb_ShowAllCsproj);
            Controls.Add(Tb_ShowPath);
            Controls.Add(Btn_GetFolder);
            Name = "Form1";
            Text = "Reference Convert";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Btn_GetFolder;
        private TextBox Tb_ShowPath;
        private ListBox Lb_ShowAllCsproj;
        private TextBox Tb_SaveFolder;
        private Button Btn_Convert;
        private Label label1;
    }
}
