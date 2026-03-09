using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AutoCodeGen
{
    public partial class ucFeatures : UserControl
    {
        private bool _TablesChecked = false;
        private bool _FeaturesChecked = false;

        public IGenerator Plugin { get; private set; }

        public ucFeatures(IGenerator plugin, IEnumerable<string> tableNames)
        {
            ArgumentNullException.ThrowIfNull(nameof(plugin));
            ArgumentNullException.ThrowIfNull(nameof(tableNames));

            InitializeComponent();

            Plugin = plugin;

            lblDescription.Text = plugin.Description;

            foreach (var table in tableNames)
                cblTables.Items.Add(table, false);

            foreach (var feature in plugin.FeatureNames)
                cblFeatures.Items.Add(feature, false);
        }

        private void btnToggleTables_Click(object sender, EventArgs e)
        {
            _TablesChecked = !_TablesChecked;

            for (int i = 0; i < cblTables.Items.Count; i++)
                cblTables.SetItemChecked(i, _TablesChecked);
        }

        private void btnToggleFeatures_Click(object sender, EventArgs e)
        {
            _FeaturesChecked = !_FeaturesChecked;

            for (int i = 0; i < cblFeatures.Items.Count; i++)
                cblFeatures.SetItemChecked(i, _FeaturesChecked);
        }

        public List<string> GetCheckedTables() => cblTables.CheckedItems.Cast<string>().ToList();
        public List<string> GetCheckedFeatures() => cblFeatures.CheckedItems.Cast<string>().ToList();
    }
}
