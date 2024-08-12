using Prototype.Main.Controls;
using Prototype.Main.GraphSourceParts;
using Prototype.Main.VirtualCanvas;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Prototype.Main.ViewModels
{
    internal class StatisticsViewModel : BaseNavigateViewModel, ISearchTextOwner, ICanvasProxyOwner
    {    
        private const double INITIAL_SEARCH_MAX_WIDTH = 70;

        private ICanvasProxy? _canvasProxy;
        private readonly IEventAggregator _eventAggregator;
        private readonly IUIHelper _uIHelper;
        private readonly Caliburn.Micro.BindableCollection<ProjectViewModel> _projects = new Caliburn.Micro.BindableCollection<ProjectViewModel>();
        private GraphInfos? _graph = null;

        public DelegateCommand<object> CheckedCommand { get; }
        public DelegateCommand<object> UncheckedCommand { get; }

        public StatisticsViewModel(IEventAggregator eventAggregator, IUIHelper uIHelper)
        {
            _uIHelper = uIHelper;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<BuildStatisticEvent>().Subscribe(OnBuildStatistic);
            CheckedCommand = new DelegateCommand<object>(OnChecked);
            UncheckedCommand = new DelegateCommand<object>(OnUnchecked);
        }

        public ICanvasProxy? CanvasProxy
        {
            get => _canvasProxy;
            set
            {
                _canvasProxy = value;
                DrawCanvasGraphs();
            }
        }

        private void GenerateShapes(Action<GraphShape> action)
        {
            int rows = (int)Math.Sqrt(Projects.Count) + 1;
            int pos = 0;
            double offset = 0;
            Random r = new Random(Environment.TickCount);
            for (int column = 0; pos < Projects.Count; column++)
            {
                double maxOffset = 0;
                double height = 0;
                for (int row = 0; row < rows; row++)
                {
                    var parent = Projects.ElementAt(pos);
                    var shape = new GraphShape(offset, height, parent, _uIHelper)
                    {
                        IsVisible = true,
                        ZIndex = 1,
                        Fill = new SolidColorBrush(Colors.Transparent),
                        Stroke = GetRandomColor(r),
                        StrokeThickness = 1,
                        Type = ShapeType.CustomDrawableObject,
                        StarPoints = r.Next(4, 10),
                    };
                    maxOffset = Math.Max(shape.Bounds.Width, maxOffset);
                    height += shape.Bounds.Height;
                    action(shape);
                    pos++;
                    if (pos == Projects.Count)
                        return;
                }
                offset += 15 + maxOffset;
            }
        }

        private static Brush GetRandomColor(Random r)
        {
            return new SolidColorBrush(Color.FromArgb((byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255)));
        }

        private DemoSpatialIndex BuildSpatialIndex()
        {
            double maxX = 10000;
            double maxY = 10000;
            var spatialIndex = new DemoSpatialIndex();
            spatialIndex.Extent = new Rect(0, 0, maxX, maxY);
            var maxWidth = INITIAL_SEARCH_MAX_WIDTH;
            GenerateShapes(shape =>
            {
                spatialIndex.Insert(shape);
                maxWidth = Math.Max(maxWidth, shape.CalculateHeader().Width);
            });
            SearchMaxWidth = maxWidth;
            return spatialIndex;
        }

        private void DrawCanvasGraphs()
        {
            _canvasProxy!.Fill(BuildSpatialIndex());
            _canvasProxy!.MoveScrollTo(new Point(0, 0), true);
        }

        // Create lines
        public void OnChecked(object obj)
        {
            Debug.Assert(obj is ProjectViewModel);
            if (_canvasProxy == null)
            {
                Debug.Assert(false);
                return;
            }

            var item = _canvasProxy.Generate<GraphShape>().FirstOrDefault(it => GraphShape.IsMatched(it, obj));
            if (item == null)
            {
                Debug.Assert(false);
                return;
            }
            var items = _canvasProxy.Generate<GraphShape>().ToList();
            foreach (var parent in item.Project.FlatList.OfType<DependedProjectViewModel>())
            {
                var foundParent = items.Where(it => it != item).FirstOrDefault
               (shape => shape.Project.Description == parent.Description);
                if (foundParent == null)
                    continue;

                var shape = GraphArrowLineWithInfo.BuildGraphLine(item, foundParent, parent);
                if (shape != null)
                {
                    shape.IsVisible = true;
                    shape.Fill = new SolidColorBrush(Colors.Red);
                    shape.Stroke = new SolidColorBrush(Colors.Red);
                    shape.StrokeThickness = 1;
                    shape.StarPoints = foundParent.StarPoints;
                    _canvasProxy.Add(shape);
                }
            }
        }

        // Remove lines
        public void OnUnchecked(object obj)
        {            
            Debug.Assert(obj is ProjectViewModel);
            if (_canvasProxy == null) 
            {
                Debug.Assert(false);
                return;
            }

            var removingLines = _canvasProxy.Generate<GraphArrowLineWithInfo>().Where(it =>  it.IsBeginFrom(obj)).ToList();
            foreach (var removeIem in removingLines) 
            {
                removeIem.Detach();
                _canvasProxy.Remove(removeIem);
            }
        }

        public Caliburn.Micro.BindableCollection<ProjectViewModel> Projects => _projects;

        private void OnBuildStatistic(GraphInfos graph)
        {
            var isInitializedCalculator = _canvasProxy != null;
            _graph = graph;
            SearchText = "";
            BuildProjects(SearchText);
            if (isInitializedCalculator)
                DrawCanvasGraphs();
        }

        private void DetachGraphLines()
        {
            var graphLines = _canvasProxy?.Generate<GraphArrowLineWithInfo>().ToList();
            if (graphLines != null)
                foreach (var removeIem in graphLines)
                {
                    removeIem.Detach();
                }
        }

        private void BuildProjects(string search)
        {
            DetachGraphLines();
            Projects.IsNotifying = false;
            Projects.Clear();
            var directory = PathUtility.NormalizePath(_graph!.TargetDirecory);
            foreach (var project in _graph.ProjectsDependencies)
            {
                var parent = new ProjectViewModel(project.Key.Remove(0, directory.Length));
                var sameParentItemsCount = 0;
                var anotherParentsItemsCount = 0;
                var isParentMatched = parent.IsMatched(search);
                var parentChildren = new List<INodeViewModel>();
                foreach (var item in project.Value)
                {
                    var child = new DependedProjectViewModel(item.Key.Remove(0, directory.Length));
                    var isParentChildMatched = isParentMatched || child.IsMatched(search, false);
                    var children = item.Value.Select(it => new ClassViewModel(it.Name, it.Frequency));
                    child.AddRange(children, isParentMatched ? "" : search);
                    if (isParentChildMatched || child.IsMatched(search, true))
                    {
                        parentChildren.Add(child);
                        int onChildAnotherCount = child.Children.OfType<ClassViewModel>().
                            Sum(it => it.Info);

                        child.IsExpanded = true;
                        if (item.Key == project.Key)
                        {
                            sameParentItemsCount += onChildAnotherCount;
                        }
                        else
                        {
                            anotherParentsItemsCount += onChildAnotherCount;
                        }
                        child.Info = $"{onChildAnotherCount}";
                    }
                }
                parent.Info = string.Format("{0} [{1}]", anotherParentsItemsCount, sameParentItemsCount);
                if (anotherParentsItemsCount != 0 || sameParentItemsCount != 0)
                {
                    parent.CanShowLines = anotherParentsItemsCount > 0;
                    parent.RefreshFlatList(parentChildren);
                    Projects.Add(parent);                   
                }
            }

            RaisePropertyChanged(nameof(ShowNoProjectsMessage));
            RaisePropertyChanged(nameof(ShowProjects));
            Projects.IsNotifying = true;
            Projects.Refresh();
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                var isInitializedCalculator = _canvasProxy != null;
                _searchText = value;
                RaisePropertyChanged(nameof(SearchText));
                BuildProjects(value);
                if (isInitializedCalculator)
                    DrawCanvasGraphs();
            }
        }

        public bool ShowNoProjectsMessage => !Projects.Any();

        public bool ShowProjects => Projects.Any();

        private double _searchMaxWidth = INITIAL_SEARCH_MAX_WIDTH;
        public double SearchMaxWidth
        {
            get => _searchMaxWidth;
            set
            {
                _searchMaxWidth = value;
                RaisePropertyChanged(nameof(SearchMaxWidth));
            }
        }

    }
}
