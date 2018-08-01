﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Drawing;

namespace AionLogAnalyzer
{
    public class HostedCheckbox : ToolStripControlHost
    {
        public event EventHandler OnClicked;

        public HostedCheckbox()
            : base(new CheckBox())
        {
        }

        public CheckBox CheckBoxControl
        {
            get
            {
                return Control as CheckBox;
            }
        }

        public bool Checked
        {
            get
            {
                return CheckBoxControl.Checked;
            }
            set
            {
                CheckBoxControl.Checked = value;
            }
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);

            CheckBox checkBox = (CheckBox)control;

            checkBox.Click += new EventHandler(OnClick);
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);

            CheckBox checkBox = (CheckBox)control;

            checkBox.Click -= new EventHandler(OnClick);
        }

        private void OnClick(object sender, EventArgs e)
        {
            if (OnClicked != null)
            {
                OnClicked(this, e);
            }
        }
    }


}