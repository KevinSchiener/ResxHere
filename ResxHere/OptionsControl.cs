using System;
using System.Windows.Forms;
using System.Globalization;
using System.Linq;

namespace ResxHere
{
    public partial class OptionsControl : UserControl
    {
        public OptionsControl()
        {
            InitializeComponent();

            CultureComboBox.DisplayMember = nameof(CultureInfo.DisplayName);
            ListBox.DisplayMember = nameof(CultureInfo.DisplayName);

            PopulateComboBox();
        }

        internal CultureOptions optionPage;

        public void Initialize()
        {
            foreach (var name in optionPage.Cultures)
            {
                ListBox.Items.Add(CultureInfo.GetCultureInfo(name));
            }
        }

        private void PopulateComboBox()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures).OrderBy(c => c.DisplayName, StringComparer.CurrentCultureIgnoreCase);
            foreach (var culture in cultures)
            {
                CultureComboBox.Items.Add(culture);
            }
            CultureComboBox.SelectedIndex = 0;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (CultureComboBox.SelectedItem == null) return;

            if (!ListBox.Items.Contains(CultureComboBox.SelectedItem))
            {
                ListBox.Items.Add(CultureComboBox.SelectedItem);
                Save();
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (ListBox.SelectedItem == null) return;
            ListBox.Items.Remove(ListBox.SelectedItem);
            Save();
        }

        private void Save()
        {
            optionPage.Cultures = ListBox.Items.OfType<CultureInfo>().Select(c => c.Name).ToList();
        }
    }
}
