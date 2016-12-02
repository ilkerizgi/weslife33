#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BuildBuddy.Other_Views;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;

#endregion

namespace BuildBuddy
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            SetColor();

            foreach (var champ in Data.Data.Champions)
            {
                ChampionComboBox.Items.Add(champ);
            }
            ItemAutoCompleteBox.ItemsSource = GetItemNames();

            InsertAfter.Click += InsertAfter_Click;
            InsertBefore.Click += InsertBefore_Click;
            Remove.Click += Remove_Click;
            Clear.Click += Clear_Click;

            KeyUp += MainWindow_KeyUp;
            SaveButton.Click += SaveButton_Click;

            SettingsButton.Click += SettingsButton_Click;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.Items.Count <= 0)
            {
                await this.ShowMessageAsync("Error", "You must have at least one item!");
                return;
            }
            if (ChampionComboBox.SelectedItem == null)
            {
                await this.ShowMessageAsync("Error", "You must select a champion!");
                return;
            }
            List<string> tmp = new List<string>();
            tmp.AddRange(ItemsListBox.Items.OfType<string>());

            await SaveFromList(tmp.ToArray());
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex == -1)
            {
                return;
            }

            ItemsListBox.Items.Clear();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex == -1)
            {
                return;
            }

            ItemsListBox.Items.RemoveAt(ItemsListBox.SelectedIndex);
        }

        private void InsertBefore_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex == -1)
            {
                return;
            }

            if (!string.IsNullOrEmpty(ItemAutoCompleteBox.Text))
            {
                string text = ItemAutoCompleteBox.Text;

                ItemsListBox.Items.Insert(ItemsListBox.SelectedIndex, ItemAutoCompleteBox.Text);
            }
        }

        private void InsertAfter_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex == -1)
            {
                return;
            }

            if (!string.IsNullOrEmpty(ItemAutoCompleteBox.Text))
            {
                ItemsListBox.Items.Insert(ItemsListBox.SelectedIndex + 1, ItemAutoCompleteBox.Text);
            }
        }

        private async void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(ItemAutoCompleteBox.Text))
                {
                    string text = ItemAutoCompleteBox.Text;

                    if (!GetItemNames().Contains(text))
                    {
                        await this.ShowMessageAsync("Error", "That is not a valid item :pepe:");
                        return;
                    }
                    ItemsListBox.Items.Add(text);
                }
            }
        }

        private static string[] GetItemNames()
        {
            string json = Encoding.Default.GetString(Properties.Resources.ItemData);

            dynamic parsed = JObject.Parse(json);
            return new string[]
                   {
                       parsed.data.id1001.name,
                       parsed.data.id1004.name,
                       parsed.data.id1006.name,
                       parsed.data.id1011.name,
                       parsed.data.id1018.name,
                       parsed.data.id1026.name,
                       parsed.data.id1027.name,
                       parsed.data.id1028.name,
                       parsed.data.id1029.name,
                       parsed.data.id1031.name,
                       parsed.data.id1033.name,
                       parsed.data.id1036.name,
                       parsed.data.id1037.name,
                       parsed.data.id1038.name,
                       parsed.data.id1039.name,
                       parsed.data.id1041.name,
                       parsed.data.id1042.name,
                       parsed.data.id1043.name,
                       parsed.data.id1051.name,
                       parsed.data.id1052.name,
                       parsed.data.id1053.name,
                       parsed.data.id1054.name,
                       parsed.data.id1055.name,
                       parsed.data.id1056.name,
                       parsed.data.id1057.name,
                       parsed.data.id1058.name,
                       parsed.data.id1082.name,
                       parsed.data.id1083.name,
                       parsed.data.id1400.name,
                       parsed.data.id1401.name,
                       parsed.data.id1402.name,
                       parsed.data.id1408.name,
                       parsed.data.id1409.name,
                       parsed.data.id1410.name,
                       parsed.data.id1412.name,
                       parsed.data.id1413.name,
                       parsed.data.id1414.name,
                       parsed.data.id1416.name,
                       parsed.data.id1418.name,
                       parsed.data.id1419.name,
                       parsed.data.id2003.name,
                       parsed.data.id2009.name,
                       parsed.data.id2010.name,
                       parsed.data.id2015.name,
                       parsed.data.id2031.name,
                       parsed.data.id2032.name,
                       parsed.data.id2033.name,
                       parsed.data.id2043.name,
                       parsed.data.id2045.name,
                       parsed.data.id2047.name,
                       parsed.data.id2049.name,
                       parsed.data.id2050.name,
                       parsed.data.id2051.name,
                       parsed.data.id2052.name,
                       parsed.data.id2053.name,
                       parsed.data.id2054.name,
                       parsed.data.id2138.name,
                       parsed.data.id2139.name,
                       parsed.data.id2140.name,
                       parsed.data.id2301.name,
                       parsed.data.id2302.name,
                       parsed.data.id2303.name,
                       parsed.data.id3001.name,
                       parsed.data.id3003.name,
                       parsed.data.id3004.name,
                       parsed.data.id3006.name,
                       parsed.data.id3007.name,
                       parsed.data.id3008.name,
                       parsed.data.id3009.name,
                       parsed.data.id3010.name,
                       parsed.data.id3020.name,
                       parsed.data.id3022.name,
                       parsed.data.id3024.name,
                       parsed.data.id3025.name,
                       parsed.data.id3026.name,
                       parsed.data.id3027.name,
                       parsed.data.id3028.name,
                       parsed.data.id3029.name,
                       parsed.data.id3030.name,
                       parsed.data.id3031.name,
                       parsed.data.id3033.name,
                       parsed.data.id3034.name,
                       parsed.data.id3035.name,
                       parsed.data.id3036.name,
                       parsed.data.id3040.name,
                       parsed.data.id3041.name,
                       parsed.data.id3042.name,
                       parsed.data.id3043.name,
                       parsed.data.id3044.name,
                       parsed.data.id3046.name,
                       parsed.data.id3047.name,
                       parsed.data.id3048.name,
                       parsed.data.id3050.name,
                       parsed.data.id3052.name,
                       parsed.data.id3053.name,
                       parsed.data.id3056.name,
                       parsed.data.id3057.name,
                       parsed.data.id3060.name,
                       parsed.data.id3065.name,
                       parsed.data.id3067.name,
                       parsed.data.id3068.name,
                       parsed.data.id3069.name,
                       parsed.data.id3070.name,
                       parsed.data.id3071.name,
                       parsed.data.id3072.name,
                       parsed.data.id3073.name,
                       parsed.data.id3074.name,
                       parsed.data.id3075.name,
                       parsed.data.id3077.name,
                       parsed.data.id3078.name,
                       parsed.data.id3082.name,
                       parsed.data.id3083.name,
                       parsed.data.id3084.name,
                       parsed.data.id3085.name,
                       parsed.data.id3086.name,
                       parsed.data.id3087.name,
                       parsed.data.id3089.name,
                       parsed.data.id3090.name,
                       parsed.data.id3091.name,
                       parsed.data.id3092.name,
                       parsed.data.id3094.name,
                       parsed.data.id3096.name,
                       parsed.data.id3097.name,
                       parsed.data.id3098.name,
                       parsed.data.id3100.name,
                       parsed.data.id3101.name,
                       parsed.data.id3102.name,
                       parsed.data.id3104.name,
                       parsed.data.id3105.name,
                       parsed.data.id3108.name,
                       parsed.data.id3110.name,
                       parsed.data.id3111.name,
                       parsed.data.id3112.name,
                       parsed.data.id3113.name,
                       parsed.data.id3114.name,
                       parsed.data.id3115.name,
                       parsed.data.id3116.name,
                       parsed.data.id3117.name,
                       parsed.data.id3122.name,
                       parsed.data.id3123.name,
                       parsed.data.id3124.name,
                       parsed.data.id3133.name,
                       parsed.data.id3134.name,
                       parsed.data.id3135.name,
                       parsed.data.id3136.name,
                       parsed.data.id3137.name,
                       parsed.data.id3139.name,
                       parsed.data.id3140.name,
                       parsed.data.id3142.name,
                       parsed.data.id3143.name,
                       parsed.data.id3144.name,
                       parsed.data.id3145.name,
                       parsed.data.id3146.name,
                       parsed.data.id3147.name,
                       parsed.data.id3151.name,
                       parsed.data.id3152.name,
                       parsed.data.id3153.name,
                       parsed.data.id3155.name,
                       parsed.data.id3156.name,
                       parsed.data.id3157.name,
                       parsed.data.id3158.name,
                       parsed.data.id3165.name,
                       parsed.data.id3170.name,
                       parsed.data.id3174.name,
                       parsed.data.id3181.name,
                       parsed.data.id3184.name,
                       parsed.data.id3185.name,
                       parsed.data.id3187.name,
                       parsed.data.id3190.name,
                       parsed.data.id3191.name,
                       parsed.data.id3196.name,
                       parsed.data.id3197.name,
                       parsed.data.id3198.name,
                       parsed.data.id3200.name,
                       parsed.data.id3211.name,
                       parsed.data.id3222.name,
                       parsed.data.id3285.name,
                       parsed.data.id3301.name,
                       parsed.data.id3302.name,
                       parsed.data.id3303.name,
                       parsed.data.id3340.name,
                       parsed.data.id3341.name,
                       parsed.data.id3345.name,
                       parsed.data.id3348.name,
                       parsed.data.id3361.name,
                       parsed.data.id3362.name,
                       parsed.data.id3363.name,
                       parsed.data.id3364.name,
                       parsed.data.id3401.name,
                       parsed.data.id3460.name,
                       parsed.data.id3461.name,
                       parsed.data.id3462.name,
                       parsed.data.id3504.name,
                       parsed.data.id3508.name,
                       parsed.data.id3512.name,
                       parsed.data.id3599.name,
                       parsed.data.id3671.name,
                       parsed.data.id3672.name,
                       parsed.data.id3673.name,
                       parsed.data.id3675.name,
                       parsed.data.id3706.name,
                       parsed.data.id3711.name,
                       parsed.data.id3715.name,
                       parsed.data.id3742.name,
                       parsed.data.id3748.name,
                       parsed.data.id3751.name,
                       parsed.data.id3800.name,
                       parsed.data.id3801.name,
                       parsed.data.id3802.name,
                       parsed.data.id3812.name,
                       parsed.data.id3901.name,
                       parsed.data.id3902.name,
                       parsed.data.id3903.name
                   };
        }

        private async Task SaveFromList(string[] items)
        {
            try
            {
                string location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                  "\\EloBuddy\\AramBuddy\\Builds";

                JObject json = new JObject(new JProperty("data", items));

                Directory.CreateDirectory(location);
                if (File.Exists(location + "\\" + ChampionComboBox + ".json"))
                {
                    await this.ShowMessageAsync("Error", "A Build with that Champion already exists!");
                    return;
                }
                File.WriteAllText(location + "\\" + ChampionComboBox.Text.Trim() + ".json", json.ToString());

                await
                    this.ShowMessageAsync("Success",
                        "A Build with the name '" + ChampionComboBox.Text.Trim() + ".json" +
                        "' was created in the build path!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("error occured: " + ex);
                Environment.Exit(-1);
            }
        }

        private void SetColor()
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + "\\settings.cfg"))
            {
                return;
            }

            string data =
                File.ReadAllLines(Directory.GetCurrentDirectory() + "\\settings.cfg")
                    .FirstOrDefault()
                    .Replace("Color=", "");

            Accent color = ThemeManager.GetAccent(data.Replace("Color=", ""));

            if ((color != null) && (color != ThemeManager.DetectAppStyle(Application.Current).Item2))
            {
                ThemeManager.ChangeAppStyle(Application.Current, color,
                    ThemeManager.DetectAppStyle(Application.Current).Item1);
            }
        }
    }
}