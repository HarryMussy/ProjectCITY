
namespace CitySkylines0._5alphabeta
{
    partial class LoadSaveForm
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
            SuspendLayout();
            // 
            // LoadSaveForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "LoadSaveForm";
            Text = "LoadSaveForm";
            Load += LoadSaveForm_Load;
            ResumeLayout(false);
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "City Save (*.citysave)|*.citysave";
            dialog.DefaultExt = "citysave";
            dialog.AddExtension = true;
            dialog.Title = "Save Game";
        }

        private void LoadSaveForm_Load(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}