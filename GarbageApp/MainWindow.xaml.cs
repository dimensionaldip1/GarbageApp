using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using Microsoft.Msagl.Drawing;
using System.Diagnostics;
using System.IO;

namespace GarbageApp;


public partial class MainWindow : Window
{
    private int Vertices;
    private int[,] MatrixInput;
    private int[,] FLdist;
    private int?[,] FLnext;
    private int[,] DDist;
    private int?[,] Dprev;
    private int strt;
    private int finsh;
    private TextBox strtBox;
    private TextBox finshBox;
   
    private string MethodName = "Blank";
    private List<int> Path;



    private TextBox[,] MatrixBoxes;


    public MainWindow()
    {
        InitializeComponent();
    }

    private static readonly Regex _regex = new Regex("^[0-9]+$");

    private void TextBoxPreview(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !_regex.IsMatch(e.Text);
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        string input = vertText.Text;
        if (int.TryParse(input, out int result) && result < 101 && result > 0)
        {
            Vertices = result;
            MatrixBoxes = new TextBox[Vertices, Vertices];



            MessageBox.Show($"Отримано: {Vertices}");
            Button inputBtn = new Button
            {

                Height = 50,
                Width = 150,
                Content = "Build a graph",

            };
            inputBtn.Click += GraphBuilder;
            MainGrid.Children.Add(inputBtn);
            Grid.SetColumn(inputBtn, 2);
            Grid.SetRow(inputBtn, 1);
            vertText.Clear();
            StackPanel stackpanel = new StackPanel
            {
                Background = Brushes.Azure
            };
            Grid.SetRow(stackpanel, 2);
            Grid.SetRow(stackpanel, 2);
            Grid.SetRowSpan(stackpanel, 4);

            MainGrid.Children.Add(stackpanel);
            MatrixInput = new int[Vertices, Vertices];
            Grid smallgrid = new Grid
            {
                Name = "SmallGrid",
                Margin = new Thickness(10)
            };
            stackpanel.Children.Add(smallgrid);

            for (int i = 0; i < Vertices; i++)
            {
                smallgrid.RowDefinitions.Add(new RowDefinition());
                smallgrid.ColumnDefinitions.Add(new ColumnDefinition());

            }

            for (int i = 0; i < Vertices; i++)
            {
                for (int j = 0; j < Vertices; j++)
                {

                    TextBox tb = new TextBox
                    {
                        Text = "0",
                        Height = 20,
                        Width = 30

                    };
                    tb.PreviewTextInput += TextBoxPreview;
                    smallgrid.Children.Add(tb);
                    Grid.SetRow(tb, i);
                    Grid.SetColumn(tb, j);

                    MatrixBoxes[i, j] = tb;



                }
            }




        }
        else
        {
            MessageBox.Show("Некоректний ввід.Максимальна кількість вершин -- 10, а мінімальна- 1");
            vertText.Clear();

        }


    }
    private void GraphBuilder(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < Vertices; i++)
        {
            for (int j = 0; j < Vertices; j++)
            {
                string value = MatrixBoxes[i, j].Text;
                int.TryParse(value, out MatrixInput[i, j]);
            }
        }
        Grid smolgrid = new Grid 
        {
            
        };
        smolgrid.RowDefinitions.Add(new RowDefinition());
        MainGrid.Children.Add(smolgrid);
        Grid.SetRow(smolgrid, 0);
        Grid.SetRowSpan(smolgrid, 1);
        Grid.SetColumn(smolgrid, 1);
        smolgrid.ColumnDefinitions.Add(new ColumnDefinition());
        smolgrid.ColumnDefinitions.Add(new ColumnDefinition());
        smolgrid.RowDefinitions.Add(new RowDefinition());
        smolgrid.RowDefinitions.Add(new RowDefinition());
        Button DanBtn = new Button
        {
            Height = 50,
            Width = 100,
            Content = "Dantzig search",
        };
        Button FloydBtn = new Button
        {
            Height = 50,
            Width = 100,
            Content = "Floyd-Warshall search",
            
        };
        strtBox = new TextBox
        {
            Text = "strt",
            Height = 20,
            Width = 30
        };
        finshBox = new TextBox
        {
            Text = "finsh",
            Height = 20,
            Width = 30,
           
        };
        finshBox.PreviewTextInput += TextBoxPreview;
        strtBox.PreviewTextInput += TextBoxPreview;
        smolgrid.Children.Add(finshBox);
        smolgrid.Children.Add(strtBox);
        Grid.SetRow(strtBox, 0);
        Grid.SetColumn(strtBox, 0);
        Grid.SetRow(finshBox, 0);
        Grid.SetColumn(finshBox, 1);
        FloydBtn.Click += OnFloydClick;
        DanBtn.Click += OnDanClick;

        smolgrid.Children.Add(DanBtn);
        smolgrid.Children.Add(FloydBtn);
        Grid.SetRow(DanBtn, 2);
        Grid.SetRow(FloydBtn, 2);
        Grid.SetColumn(DanBtn, 0);
        Grid.SetColumn(FloydBtn, 1);
        var graph = new Microsoft.Msagl.Drawing.Graph();
        for (int i = 0; i < Vertices; i++)
        {
            for (int j = 0; j < Vertices; j++)
            {
                if (MatrixInput[i, j] > 0)
                {
                    var edge = graph.AddEdge(i.ToString(), MatrixInput[i, j].ToString(), j.ToString());
                    foreach (var node in graph.Nodes)
                    {
                        node.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Circle;
                        node.Attr.XRadius = 15;
                        node.Attr.YRadius = 15;
                    }

                    edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                    edge.Attr.LineWidth = 1;
                }
            }
        }
        GraphViewerControl.Graph = graph;
    }
    private void RunFloydWarshall( out string method)
    {
        string Method = "Floyd-Warshall";
        method = Method;
        Stopwatch stopw = Stopwatch.StartNew();
        int operations = 0;
        int n = Vertices;
        const int INF = int.MaxValue / 2;
        FLdist = new int[n, n];
        FLnext = new int?[n, n];
        operations += 2;

        for (int i = 0; i < n; i++) {
            operations++;
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                    FLdist[i, j] = 0;
                else if (MatrixInput[i, j] > 0)
                    FLdist[i, j] = MatrixInput[i, j];
                else
                    FLdist[i, j] = INF;

                FLnext[i, j] = MatrixInput[i, j] > 0 ? j : (int?)null;
                operations++;
            }
        }
            
        for (int k = 0; k < n; k++)
        {
            operations++;
            for (int i = 0; i < n; i++)
            {
                operations++;
                for (int j = 0; j < n; j++) {
                    if (FLdist[i, k] + FLdist[k, j] < FLdist[i, j])
                    {
                        FLdist[i, j] = FLdist[i, k] + FLdist[k, j];
                        FLnext[i, j] = FLnext[i, k];
                        operations++;
                    }
                }
                    
            }
               
        }
        stopw.Stop();
        
        MessageBox.Show($"Час виконання {stopw.ElapsedMilliseconds} мс\n Кількість операцій {operations} ");
           
    }

    private void RunDantzig( out string method)
    {
        string Method = "Dantzig";
        method = Method;
        Stopwatch stopw = Stopwatch.StartNew();
        int operations=0;
        int n = Vertices;
        const int INF = int.MaxValue / 2;
        DDist = new int[n, n];
        Dprev = new int?[n, n];
        operations += n * n * 2;
        for (int s = 0; s < n; s++)
        {
            var distS = new int[n];
            var prevS = new int?[n];
            var used = new bool[n];
            operations++;
            for (int i = 0; i < n; i++) 
            {
                distS[i] = INF; 
                prevS[i] = null; 
                used[i] = false;
                operations++;
            }
            distS[s] = 0;
            for (int iter = 0; iter < n; iter++)
            {
                int u = -1;
                operations++;
                for (int i = 0; i < n; i++)
                    if (!used[i] && (u == -1 || distS[i] < distS[u])) u = i;

                if (u == -1 || distS[u] == INF) break;
                used[u] = true;
                for (int v = 0; v < n; v++)
                    if (MatrixInput[u, v] > 0 && distS[u] + MatrixInput[u, v] < distS[v])
                    {
                        distS[v] = distS[u] + MatrixInput[u, v];
                        prevS[v] = u;
                        operations++;
                    }
            }
            for (int t = 0; t < n; t++)
            {
                DDist[s, t] = distS[t]; 
                Dprev[s, t] = prevS[t];
                operations++;
            }
          
        }
        stopw.Stop();
        
        MessageBox.Show($"Час виконання {stopw.ElapsedMilliseconds} мс\nКількість операцій {operations}");
    }
    private List<int> GetFloydPath(int u, int v)
    {
        var path = new List<int>();
        if (FLnext[u, v] == null) return path;
        int current = u;
        path.Add(current);
        while (current != v)
        {
            current = FLnext[current, v].Value;
            path.Add(current);
        }
        Path = path;
        return path;
    }

    
    private void HighlightFloydPath(Graph graph, int start, int end)
    {
        var path = GetFloydPath(start, end);
        for (int i = 0; i < path.Count - 1; i++)
        {
            var src = path[i].ToString();
            var dst = path[i + 1].ToString();
            
            var edge = graph.Edges.FirstOrDefault(e => (e.Source == src && e.Target == dst) || (e.Source == dst && e.Target == src));
            if (edge != null)
            {
                edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                edge.Attr.LineWidth = 2;
            }
            var node = graph.FindNode(src);
            if (node != null) node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightCoral;
        }
        var last = graph.FindNode(path.Last().ToString());
        if (last != null) last.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightCoral;
    }

    
    private List<int> GetDantzigPath(int start, int end)
    {
        var path = new List<int>();
        if (Dprev[start, end] == null && start != end) return path;
        int? current = end;
        while (current != null)
        {
            path.Insert(0, current.Value);
            if (current == start) break;
            current = Dprev[start, current.Value];
        }
        Path = path;
        return path;
    }

    
    private void HighlightDantzigPath(Graph graph, int start, int end)
    {
        var path = GetDantzigPath(start, end);
        for (int i = 0; i < path.Count - 1; i++)
        {
            var src = path[i].ToString();
            var dst = path[i + 1].ToString();
            var edge = graph.Edges.FirstOrDefault(e => (e.Source == src && e.Target == dst) || (e.Source == dst && e.Target == src));
            if (edge != null)
            {
                edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                edge.Attr.LineWidth = 2;
            }
            var node = graph.FindNode(src);
            if (node != null) node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightCoral;
        }
        var last = graph.FindNode(path.Last().ToString());
        if (last != null) last.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightCoral;
    }

    
    private void ShowFloydPath(int start, int end)
    {
        RunFloydWarshall(out MethodName);
        var path = GetFloydPath(start, end);

        
        if (path.Count == 0)
        {
            MessageBox.Show("Найкоротший шлях між цими вершинами не знайдено.");
            return;
        }

        
        var graph = GraphViewerControl.Graph;
        HighlightFloydPath(graph, start, end);
        GraphViewerControl.Graph = graph;
    }

    private void ShowDantzigPath(int start, int end)
    {
        RunDantzig(out MethodName);
        var path = GetDantzigPath(start, end);
        
        if (path.Count == 0)
        {
            MessageBox.Show("Найкоротший шлях між цими вершинами не знайдено.");
            return;
        }
        var graph = GraphViewerControl.Graph;
        HighlightDantzigPath(graph, start, end);
        GraphViewerControl.Graph = graph;
    }

    private void OnFloydClick(object sender, RoutedEventArgs e)
    {
        bool findableS = false;
        bool findableF = false;
        if (!int.TryParse(strtBox.Text, out strt) || strt > Vertices)
        {
            MessageBox.Show("Некоректний ввід");
            strtBox.Text = "strt";
        }
        else
        {
            findableS = true;
        }
        if (!int.TryParse(finshBox.Text, out finsh) || finsh > Vertices)
        {
            MessageBox.Show("Некоректний ввід");
            finshBox.Text = "finsh";
        }
        else
        {
            findableF = true;
        }
        if(!findableS==false || !findableF == false)
        {
            ShowFloydPath(strt, finsh);
        }

        Button saveBtn = new Button
        {
            Height = 50,
            Width = 100,
            Content = "Save solution",
        };
        saveBtn.Click += OnSaveClick;
        MainGrid.Children.Add(saveBtn);
        Grid.SetRow(saveBtn, 1);
        Grid.SetColumn(saveBtn, 1);





    }
    private void OnDanClick(object sender, RoutedEventArgs e)
    {
        bool findableS = false;
        bool findableF = false;
        if (!int.TryParse(strtBox.Text, out strt) || strt > Vertices)
        {
            MessageBox.Show("Некоректний ввід");
            strtBox.Text = "strt";
        }
        else
        {
            findableS = true;
        }
        if (!int.TryParse(finshBox.Text, out finsh) || finsh > Vertices)
        {
            MessageBox.Show($"Некоректний ввід (Загальна кількість вершин:{Vertices}");
            finshBox.Text = "finsh";
        }
        else
        {
            findableF = true;
        }
        if (!findableS == false || !findableF == false)
        {
            ShowDantzigPath(strt, finsh);
        }

        Button saveBtn = new Button
        {
            Height = 50,
            Width = 100,
            Content = "Save solution",
        };
        saveBtn.Click += OnSaveClick;
        MainGrid.Children.Add(saveBtn);
        Grid.SetRow(saveBtn, 1);
        Grid.SetColumn(saveBtn, 1);

    }
    private void SaveText(int[,] Matrix, string method)
    {
        string filePath = "matrix.txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            int rows = Matrix.GetLength(0);
            int cols = Matrix.GetLength(1);
            writer.WriteLine("Матриця");
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    writer.Write(Matrix[i, j]);
                    if (j < cols - 1)
                        writer.Write(" ");
                }
                writer.WriteLine();
            }
            writer.WriteLine($"Метод:{MethodName}");
            writer.WriteLine($"Найкоротший шлях:від {strt} до {finsh}");
            if (Path.Count > 0)
            {
                writer.WriteLine(string.Join(" >>> ", Path));
                
            }
            else
            {
                writer.WriteLine("Шляху не знайдено " + strt + " до" + finsh);
            }



        }
    }

   

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SaveText(MatrixInput, MethodName);
    }

}

