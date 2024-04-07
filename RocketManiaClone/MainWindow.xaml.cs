using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media.Animation;

namespace RocketManiaClone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static readonly int[] tileDistribution = { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 3 };
        public static readonly Pair<int, int>[] neighboursLTRB = { new Pair<int, int>(0, -1), new Pair<int, int>(-1, 0), new Pair<int, int>(0, 1), new Pair<int, int>(1, 0), };
        public static readonly Dictionary<string, Neighbour[]> dictOfNeighbours = new Dictionary<string, Neighbour[]>
        {
            { "ITile-0", App.Ar(Neighbour.Left, Neighbour.Right) },
            { "ITile-90", App.Ar(Neighbour.Top, Neighbour.Bottom) },
            { "ITile-180", App.Ar(Neighbour.Left, Neighbour.Right) },
            { "ITile-270", App.Ar(Neighbour.Top, Neighbour.Bottom) },
            { "LTile-0", App.Ar(Neighbour.Left, Neighbour.Top) },
            { "LTile-90", App.Ar(Neighbour.Top, Neighbour.Right) },
            { "LTile-180", App.Ar(Neighbour.Right, Neighbour.Bottom) },
            { "LTile-270", App.Ar(Neighbour.Bottom, Neighbour.Left) },
            { "TTile-0", App.Ar(Neighbour.Left, Neighbour.Top, Neighbour.Right) },
            { "TTile-90", App.Ar(Neighbour.Top, Neighbour.Right, Neighbour.Bottom) },
            { "TTile-180", App.Ar(Neighbour.Right, Neighbour.Bottom, Neighbour.Left) },
            { "TTile-270", App.Ar(Neighbour.Bottom, Neighbour.Left, Neighbour.Top) },
            { "XTile-0", App.Ar(Neighbour.Left, Neighbour.Top, Neighbour.Right, Neighbour.Bottom) },
            { "XTile-90", App.Ar(Neighbour.Left, Neighbour.Top, Neighbour.Right, Neighbour.Bottom) },
            { "XTile-180", App.Ar(Neighbour.Left, Neighbour.Top, Neighbour.Right, Neighbour.Bottom) },
            { "XTile-270", App.Ar(Neighbour.Left, Neighbour.Top, Neighbour.Right, Neighbour.Bottom) },
        };

        private readonly Border[] rockets;
        private readonly Border[] footerTiles;
        private readonly Border[,] tiles = new Border[10, 10];
        private readonly Dictionary<String, int> graphMatrixDict = new Dictionary<string, int>();
        private readonly List<int> rocketsToLaunch = new List<int>();
        private readonly List<int> firesToIgnite = new List<int>();
        private readonly bool[,] isGreen = new bool[10, 10];
        private int animatedInt;
        private int rocketsFadingInTilesFalling;
        private bool rocketsAnimationOn = false;

        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += AnimateRockets;

            rockets = new Border[] { rocket_0, rocket_1, rocket_2, rocket_3, rocket_4, rocket_5, rocket_6, rocket_7, rocket_8, rocket_9 };
            footerTiles = new Border[] { footer_0, footer_1, footer_2, footer_3, footer_4, footer_5, footer_6, footer_7, footer_8, footer_9 };

            Type thisType = this.GetType();
            for (int i = 0; i < 10; ++i)
                for (int j = 0; j < 10; ++j)
                    tiles[i, j] = (Border)thisType.GetField(string.Format("tile_{0}{1}", i, j), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
        }

        public Dictionary<string, int> GraphMatrixDict
        {
            get { return graphMatrixDict; }
        }

        private Border GetTile(int n)
        {
            int a = n % 10;
            return tiles[(n - a) / 10, a];
        }

        private Border UpperNeighbour(int i, int j)
        {
            if (i == 9)
                return footerTiles[j];
            else
                return tiles[i + 1, j];
        }

        private FrameworkElement CreateTile(string str)
        {
            switch (str)
            {
                case "ITile":
                    return new ITile();
                case "LTile":
                    return new LTile();
                case "TTile":
                    return new TTile();
                case "XTile":
                    return new XTile();
                default:
                    return null;
            }
        }

        private Pair<FrameworkElement, double> GenerateRandomTile()
        {
            int a, b;
            FrameworkElement element;
            a = App.randomGenerator.Next(16);
            a = tileDistribution[a];
            switch (a)
            {
                case 0:
                    element = new ITile();
                    break;
                case 1:
                    element = new LTile();
                    break;
                case 2:
                    element = new TTile();
                    break;
                case 3:
                    element = new XTile();
                    break;
                default:
                    element = null;
                    break;
            }
            b = App.randomGenerator.Next(4);
            return new Pair<FrameworkElement, double>(element, b * 90.0);
        }

        private void InitialiseTiles()
        {
            Pair<FrameworkElement, double> newTile;
            for (int i = 0; i < 10; ++i)
                for (int j = 1; j < 9; ++j)
                {
                    newTile = GenerateRandomTile();
                    tiles[i, j].Tag = newTile.First.GetType().Name;
                    ((Border)tiles[i, j].Child).Child = newTile.First;
                    ((Border)tiles[i, j].Child).Tag = newTile.Second;
                }
        }

        private void PostInitialiseTiles()
        {
            FillGraphMatrix();
            ColourTiles();
            if (rocketsToLaunch.Count > 0)
                LaunchRockets();
            else
                modalBorder.IsHitTestVisible = false;
        }

        private void FillGraphMatrix()
        {
            int a, b;
            graphMatrixDict.Clear();
            for (int i = 0; i < 10; ++i)
                for (int j = 1; j < 9; ++j)
                {
                    string key1 = string.Format("{0}-{1}", tiles[i, j].Tag, Convert.ToInt32((double)((Border)tiles[i, j].Child).Tag) % 360);
                    for (int k = 0; k < 4; ++k)
                    {
                        Pair<int, int> pair = neighboursLTRB[k];
                        a = i + pair.First;
                        b = j + pair.Second;
                        if (a < 0 || a > 9) continue;
                        if (b == 0 || b == 9)
                        {
                            if (dictOfNeighbours[key1].Contains((Neighbour)k))
                                graphMatrixDict[string.Format("{0}{1}-{2}{3}", i, j, a, b)] = graphMatrixDict[string.Format("{0}{1}-{2}{3}", a, b, i, j)] = 1;
                            continue;
                        }
                        string key2 = string.Format("{0}-{1}", tiles[a, b].Tag, Convert.ToInt32((double)((Border)tiles[a, b].Child).Tag) % 360);
                        if (dictOfNeighbours[key1].Contains((Neighbour)k) && dictOfNeighbours[key2].Contains((Neighbour)((k + 2) % 4)))
                            graphMatrixDict[string.Format("{0}{1}-{2}{3}", i, j, a, b)] = 1;
                    }
                }
        }

        private void UpdateGraphMatrix(int i, int j)
        {
            int a, b;
            string key1 = string.Format("{0}-{1}", tiles[i, j].Tag, Convert.ToInt32((double)((Border)tiles[i, j].Child).Tag) % 360);
            for (int k = 0; k < 4; ++k)
            {
                Pair<int, int> pair = neighboursLTRB[k];
                a = i + pair.First;
                b = j + pair.Second;
                if (a < 0 || a > 9) continue;
                if (b == 0 || b == 9)
                {
                    if (dictOfNeighbours[key1].Contains((Neighbour)k))
                    {
                        graphMatrixDict[string.Format("{0}{1}-{2}{3}", i, j, a, b)] = graphMatrixDict[string.Format("{0}{1}-{2}{3}", a, b, i, j)] = 1;
                    }
                    else
                    {
                        graphMatrixDict.Remove(string.Format("{0}{1}-{2}{3}", i, j, a, b));
                        graphMatrixDict.Remove(string.Format("{0}{1}-{2}{3}", a, b, i, j));
                    }
                    continue;
                }
                string key2 = string.Format("{0}-{1}", tiles[a, b].Tag, Convert.ToInt32((double)((Border)tiles[a, b].Child).Tag) % 360);
                if (dictOfNeighbours[key1].Contains((Neighbour)k) && dictOfNeighbours[key2].Contains((Neighbour)((k + 2) % 4)))
                {
                    graphMatrixDict[string.Format("{0}{1}-{2}{3}", i, j, a, b)] = graphMatrixDict[string.Format("{0}{1}-{2}{3}", a, b, i, j)] = 1;
                }
                else
                {
                    graphMatrixDict.Remove(string.Format("{0}{1}-{2}{3}", i, j, a, b));
                    graphMatrixDict.Remove(string.Format("{0}{1}-{2}{3}", a, b, i, j));
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs args)
        {
            boardPanel.Margin = new Thickness(0, 30, 72, 0);
            modalBorder.Child = null;
            InitialiseTiles();
            modalBorder.Background = Brushes.Transparent;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RocketManiaClone.flame.gif");
            System.Drawing.Image gifImage = System.Drawing.Image.FromStream(stream);
            pictureBox_9.Image = pictureBox_8.Image = pictureBox_7.Image = pictureBox_6.Image = pictureBox_5.Image = pictureBox_4.Image = pictureBox_3.Image = pictureBox_2.Image = pictureBox_1.Image = pictureBox_0.Image = gifImage;
            PostInitialiseTiles();
        }

        private void TileMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            Border border = (Border)sender;
            int d = Convert.ToInt32((double)((Border)border.Child).Tag) % 360;

            DoubleAnimation doubleAnimation = new DoubleAnimation { From = d + 0.0, To = d + 90.0, By = 1.0 };
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            doubleAnimation.Completed += (object s, EventArgs e) =>
            {
                string shortName = border.Name.Substring(border.Name.IndexOf('_') + 1);
                int i = shortName[0] - 48;
                int j = shortName[1] - 48;
                UpdateGraphMatrix(i, j);
                ColourTiles();
                if (rocketsToLaunch.Count > 0)
                    LaunchRockets();
                else
                    modalBorder.IsHitTestVisible = false;
            };
            modalBorder.IsHitTestVisible = true;
            ((Border)border.Child).BeginAnimation(Border.TagProperty, doubleAnimation);
        }

        private void ColourTiles()
        {
            for (int i = 0; i < 10; ++i)
                for (int j = 1; j < 9; ++j)
                    ((Border)tiles[i, j].Child).ClearValue(Border.BackgroundProperty);

            Border border;
            firesToIgnite.Clear();
            rocketsToLaunch.Clear();

            foreach (int n in Dijkstra.Rockets(this))
            {
                border = GetTile(n).Child as Border;
                if (border != null)
                    border.Background = Brushes.Red;
                else if (n % 10 == 9)
                    firesToIgnite.Add(n);
            }
            foreach (int n in Dijkstra.Fires(this))
            {
                border = GetTile(n).Child as Border;
                if (border != null)
                    border.Background = Brushes.Orange;
                else if (n % 10 == 0)
                    rocketsToLaunch.Add(n);
            }
        }

        private async Task PaintTilesGreen()
        {
            int[] neighboursOfThese;
            List<int> additionalGreenTiles = new List<int>();
            bool[] additionalGreenTilesPredicate = new bool[100];
            Border border;

            for (int i = 0; i < 10; ++i)
                for (int j = 0; j < 10; ++j)
                    isGreen[i, j] = false;

            foreach (int n in firesToIgnite)
                additionalGreenTiles.Add(n - 1);

            do
            {
                if (additionalGreenTiles.Count == 0) break;
                foreach (int n in additionalGreenTiles)
                {
                    border = GetTile(n).Child as Border;
                    if (border != null)
                    {
                        border.Background = Brushes.LightGreen;
                        int l = n % 10;
                        isGreen[(n - l) / 10, l] = true;
                    }
                }
                await Task.Delay(50);
                neighboursOfThese = additionalGreenTiles.ToArray();
                additionalGreenTiles.Clear();
                foreach (int n in neighboursOfThese)
                {
                    int j = n % 10;
                    int i = (n - j) / 10;
                    int a, b;
                    string key = string.Format("{0}-{1}", tiles[i, j].Tag, Convert.ToInt32((double)((Border)tiles[i, j].Child).Tag) % 360);
                    foreach (Neighbour neighbour in dictOfNeighbours[key])
                    {
                        Pair<int, int> pair = neighboursLTRB[(int)neighbour];
                        a = i + pair.First;
                        b = j + pair.Second;
                        int m = 10 * a + b;
                        if (a >= 0 && a <= 9 && b > 0 && b < 9 && !isGreen[a, b] && this.GraphMatrix(m, n) == 1)
                            additionalGreenTilesPredicate[m] = true;
                    }
                }
                for (int k = 0; k < 100; ++k)
                    if (additionalGreenTilesPredicate[k])
                    {
                        additionalGreenTiles.Add(k);
                        additionalGreenTilesPredicate[k] = false;
                    }
            } while (true);
        }

        private async void LaunchRockets()
        {
            animatedInt = 0;
            rocketsFadingInTilesFalling = 0;
            await PaintTilesGreen();
            foreach (int n in rocketsToLaunch)
            {
                rockets[n / 10].Style = (Style)System.Windows.Application.Current.Resources["FlyingRocketBorderStyle"];
                ((Image)rockets[n / 10].Child).Source = (BitmapImage)System.Windows.Application.Current.Resources["Rocket1"];
            }
            rocketsAnimationOn = true;
        }

        private void AnimateRockets(object sender, EventArgs args)
        {
            if (!rocketsAnimationOn)
                return;
            animatedInt += 10;
            foreach (int n in rocketsToLaunch)
            {
                rockets[n / 10].Tag = 566.0 - Math.Max(0, animatedInt - 400);
                if (animatedInt % 100 == 0)
                    ((Image)rockets[n / 10].Child).Source = (BitmapImage)System.Windows.Application.Current.Resources[string.Format("Rocket{0}", (animatedInt / 100) % 2 + 1)];
            }
            if (animatedInt > 1250)
            {
                rocketsAnimationOn = false;
                DoubleAnimation opacityAnimation = new DoubleAnimation { From = 0.0, To = 1.0, By = 0.1 };
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
                opacityAnimation.Completed += (object s, EventArgs e) =>
                {
                    foreach (int n in rocketsToLaunch)
                        ((Image)rockets[n / 10].Child).Style = (Style)System.Windows.Application.Current.Resources["RocketImageStyle"];
                    rootGrid.BeginAnimation(Grid.TagProperty, null);
                    ++rocketsFadingInTilesFalling;
                    if (rocketsFadingInTilesFalling == 2)
                        PostInitialiseTiles();
                };
                foreach (int n in rocketsToLaunch)
                {
                    rockets[n / 10].Style = (Style)System.Windows.Application.Current.Resources["RocketBorderStyle"];
                    ((Image)rockets[n / 10].Child).Style = (Style)System.Windows.Application.Current.Resources["RocketFadeInImageStyle"];
                    ((Image)rockets[n / 10].Child).ClearValue(Image.SourceProperty);
                    rockets[n / 10].Tag = 566.0;
                }

                rootGrid.BeginAnimation(Grid.TagProperty, opacityAnimation);
                TilesFalling(App.Ar(9, 9, 9, 9, 9, 9, 9, 9));
            }
        }

        private async void TilesFalling(int[] fallingTiles)
        {
            if (fallingTiles.Sum(n => n) == -8)
            {
                ++rocketsFadingInTilesFalling;
                if (rocketsFadingInTilesFalling == 2)
                    PostInitialiseTiles();
                return;
            }
            Border[] borders = new Border[8];
            for (int i = 0; i < fallingTiles.Length; ++i)
                while (fallingTiles[i] >= 0 && !isGreen[fallingTiles[i], i + 1])
                    --fallingTiles[i];
            for (int i = 0; i < fallingTiles.Length; ++i)
                if (fallingTiles[i] >= 0)
                {
                    isGreen[fallingTiles[i], i + 1] = false;
                    Pair<FrameworkElement, double> newTile = GenerateRandomTile();
                    footerTiles[i + 1].Tag = newTile.First.GetType().Name;
                    ((Border)footerTiles[i + 1].Child).Child = newTile.First;
                    ((Border)footerTiles[i + 1].Child).Tag = newTile.Second;
                    for (int j = fallingTiles[i] + 1; j < 10; ++j)
                        tiles[j, i + 1].Style = (Style)System.Windows.Application.Current.Resources["FallingTileStyle"];
                    footerTiles[i + 1].Style = (Style)System.Windows.Application.Current.Resources["FallingTileStyle"];
                    borders[i] = (Border)tiles[fallingTiles[i], i + 1].Child;
                    tiles[fallingTiles[i], i + 1].Child = null;
                    borders[i].ClearValue(Border.BackgroundProperty);
                }
            await Task.Delay(100);
            DoubleAnimation doubleAnimation = new DoubleAnimation { From = 0.0, To = -72.0, By = -1.0 };
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));
            doubleAnimation.Completed += (object s, EventArgs e) =>
            {
                for (int i = 0; i < fallingTiles.Length; ++i)
                    if (fallingTiles[i] >= 0)
                    {
                        tiles[fallingTiles[i], i + 1].Child = borders[i];
                        for (int j = fallingTiles[i]; j < 10; ++j)
                        {
                            tiles[j, i + 1].Tag = UpperNeighbour(j, i + 1).Tag;
                            ((Border)tiles[j, i + 1].Child).Child = CreateTile((string)UpperNeighbour(j, i + 1).Tag);
                            // This removes the animation, and returns the original value to Border.TagProperty.
                            ((Border)tiles[j, i + 1].Child).BeginAnimation(Border.TagProperty, null);
                            ((Border)tiles[j, i + 1].Child).Tag = ((Border)UpperNeighbour(j, i + 1).Child).Tag;
                            ((Border)tiles[j, i + 1].Child).Background = ((Border)UpperNeighbour(j, i + 1).Child).Background;
                            UpperNeighbour(j, i + 1).Style = (Style)System.Windows.Application.Current.Resources["TileStyle"];
                        }
                    }
                mainWindow.BeginAnimation(Window.TagProperty, null);
                for (int i = 0; i < fallingTiles.Length; ++i)
                    if (fallingTiles[i] >= 0)
                        --fallingTiles[i];
                TilesFalling(fallingTiles);
            };

            mainWindow.BeginAnimation(Window.TagProperty, doubleAnimation);
        }
    }
}
