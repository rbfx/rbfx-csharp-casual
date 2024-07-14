using System;
using System.Collections.Generic;
using RbfxTemplate.GameStates;
using Urho3DNet;

namespace RbfxTemplate
{
    [ObjectFactory]
    [Preserve(AllMembers = true)]
    public sealed partial class GameState : RmlUIStateBase
    {
        private readonly SharedPtr<Scene> _scene;
        private readonly UrhoPluginApplication _app;
        private readonly Node _cameraNode;
        private readonly Camera _camera;
        private readonly CameraOperator _cameraOperator;
        private readonly Viewport _viewport;
        private readonly PrefabResource _tilePrefab;

        private readonly PickState _pickState;
        private readonly DragState _dragState;
        private readonly VictoryState _victoryState;
        private readonly Random _random = new Random();
        private readonly List<Tile> _currentTiles = new List<Tile>();

        /// <summary>
        /// State interaction manager. Track active interactions.
        /// </summary>
        private readonly StatePointerInteractionManager _stateManager;

        /// <summary>
        /// Input adapter to handle arrow keys.
        /// </summary>
        private readonly DirectionalPadAdapter _directionalPad;

        private readonly Tuple<string, string>[] _pairs =
        {
            Tuple.Create("emoji_u1f436", "emoji_u1f431"),
            Tuple.Create("emoji_u1f34e", "emoji_u1f34a"),
            Tuple.Create("emoji_u1f3b5", "emoji_u1f3b6"),
            Tuple.Create("emoji_u1f31e", "emoji_u1f31b"),
            Tuple.Create("emoji_u1f382", "emoji_u1f381"),
            Tuple.Create("emoji_u1f697", "emoji_u1f68c"),
            Tuple.Create("emoji_u1f339", "emoji_u1f33b"),
            Tuple.Create("emoji_u1f3c0", "emoji_u26bd"),
            Tuple.Create("emoji_u1f355", "emoji_u1f354"),
            Tuple.Create("emoji_u1f4da", "emoji_u1f58a"),
            Tuple.Create("emoji_u1f48d", "emoji_u1f490"),
            Tuple.Create("emoji_u1f383", "emoji_u1f47b"),
            Tuple.Create("emoji_u1f384", "emoji_u1f385"),
            Tuple.Create("emoji_u1f386", "emoji_u1f387"),
            Tuple.Create("emoji_u1f308", "emoji_u1f984"),
            Tuple.Create("emoji_u1f5fd", "emoji_u1f5fc"),
            Tuple.Create("emoji_u1f43c", "emoji_u1f428"),
            Tuple.Create("emoji_u1f37f", "emoji_u1f3a5"),
            Tuple.Create("emoji_u1f30e", "emoji_u1f680"),
            Tuple.Create("emoji_u1f60a", "emoji_u1f622")
        };

        private readonly PhysicsRaycastResult _raycastResult;
        private readonly Material _baseMaterial;
        private StateBase _state;

        private int _levelIndex;
        private int _hintsLeft = 2;
        private bool _victory;
        private readonly FiniteTimeAction _confettiAnimation;

        public GameState(UrhoPluginApplication app) : base(app, "UI/GameScreen.rml")
        {
            MouseMode = MouseMode.MmFree;
            IsMouseVisible = true;

            _raycastResult = new PhysicsRaycastResult();

            // Initialize state interaction manager.
            _stateManager = new StatePointerInteractionManager();
            _stateManager.LastKnownMousePosition = Input.MousePosition;

            _app = app;
            _scene = Context.CreateObject<Scene>();
            _scene.Ptr.LoadXML("Scenes/Scene.scene");

            _cameraNode = _scene.Ptr.GetChild("Main Camera");
            _viewport = Context.CreateObject<Viewport>();
            _camera = _cameraNode?.GetComponent<Camera>();
            _cameraOperator = _cameraNode?.GetComponent<CameraOperator>();
            _viewport.Camera = _camera;
            _viewport.Scene = _scene;
            SetViewport(0, _viewport);
            _scene.Ptr.IsUpdateEnabled = false;

            _baseMaterial = Context.ResourceCache.GetResource<Material>("Materials/TileMaterial.material");

            _tilePrefab = Context.ResourceCache.GetResource<PrefabResource>("Objects/Tile.prefab");

            var pointer =
                _scene.Ptr.InstantiatePrefab(
                    Context.ResourceCache.GetResource<PrefabResource>("Objects/Pointer.prefab"));

            _pickState = new PickState(this, pointer);
            _dragState = new DragState(this);
            _victoryState = new VictoryState(this);

            _directionalPad = new DirectionalPadAdapter(Context);

            _confettiAnimation = new ActionBuilder(Context).Enable().ShaderParameterFromTo(1.0f, "AnimationPhase", 0.0f, 1.0f).Disable().Build();

            NextLevel(null);

            Deactivate();
        }

        public override void OnDataModelInitialized(GameRmlUIComponent component)
        {
            component.BindDataModelProperty("Level", _ => _.Set("Level " + _levelIndex), _ => { });
            component.BindDataModelProperty("BaseLevel", _ => _.Set(1+((_levelIndex-1)/5)*5), _ => { });
            component.BindDataModelProperty("CurrentLevel", _ => _.Set(_levelIndex), _ => { });
            component.BindDataModelProperty("Victory", _ => _.Set(_victory), _ => { });
            component.BindDataModelEvent("Next", NextLevel);
            component.BindDataModelEvent("Settings", Settings);
        }

        private void Settings(VariantList obj)
        {
            _app.HandleBackKey();
        }

        public override void Activate(StringVariantMap bundle)
        {
            SubscribeToEvent(E.KeyUp, HandleEscKeyUp);

            SubscribeToEvent(E.MouseButtonDown, HandleMouseDown);
            SubscribeToEvent(E.MouseButtonUp, HandleMouseUp);
            SubscribeToEvent(E.MouseMove, HandleMouseMove);

            SubscribeToEvent(E.TouchBegin, HandleTouchBegin);
            SubscribeToEvent(E.TouchEnd, HandleTouchEnd);
            SubscribeToEvent(E.TouchMove, HandleTouchMove);

            _directionalPad.IsEnabled = true;

            _scene.Ptr.IsUpdateEnabled = true;


            base.Activate(bundle);
        }

        public override void Update(float timeStep)
        {
            _stateManager.State.Update(timeStep);
        }

        public override void Deactivate()
        {
            _scene.Ptr.IsUpdateEnabled = false;
            UnsubscribeFromEvent(E.KeyUp);

            UnsubscribeFromEvent(E.MouseButtonDown);
            UnsubscribeFromEvent(E.MouseButtonUp);
            UnsubscribeFromEvent(E.MouseMove);

            UnsubscribeFromEvent(E.TouchBegin);
            UnsubscribeFromEvent(E.TouchEnd);
            UnsubscribeFromEvent(E.TouchMove);

            base.Deactivate();
        }

        public void AddPairs(int numTiles)
        {
            _currentTiles.Clear();
            var leftTiles = new List<Node>();
            var rightTiles = new List<Node>();
            var visited = new HashSet<string>();
            while (leftTiles.Count < numTiles)
            {
                var pair = _pairs[_random.Next(_pairs.Length)];
                if (visited.Contains(pair.Item1) || visited.Contains(pair.Item2))
                    continue;

                visited.Add(pair.Item1);
                visited.Add(pair.Item2);

                var t1 = CreateTile(pair.Item1);
                var t2 = CreateTile(pair.Item2);

                if (_random.Next(2) == 0) (t1, t2) = (t2, t1);

                leftTiles.Add(t1.Node);
                rightTiles.Add(t2.Node);
                t1.ValidLink = t2;
                t2.ValidLink = t1;

                _currentTiles.Add(t1);
                _currentTiles.Add(t2);
            }

            Randomize(leftTiles);
            Randomize(rightTiles);

            var d = 1.5f;

            var pos = new Vector3(0, 0, (-leftTiles.Count * 0.5f + 0.5f) * d);
            for (var index = 0; index < leftTiles.Count; index++)
            {
                leftTiles[index].Position = pos + new Vector3(-d, 0, index * d);
                rightTiles[index].Position = pos + new Vector3(d, 0, index * d);
            }

            var size = new Vector3(5, 1, (leftTiles.Count + 1) * d);
            _cameraOperator.BoundingBox = new BoundingBox(size*-0.5f, size*0.5f);
        }

        public Tile CreateTile(string imageName)
        {
            var m = _baseMaterial.Clone();
            var tex = Context.ResourceCache.GetResource<Texture2D>("Images/Emoji/" + imageName + ".png");
            m.SetTexture("Albedo", tex);

            var tile = _scene.Ptr.InstantiatePrefab(_tilePrefab);
            var model = tile.GetComponent<StaticModel>(true);
            model.SetMaterial(m);
            return tile.GetComponent<Tile>(true);
        }

        public Ray GetScreenRay(IntVector2 inputMousePosition)
        {
            return _viewport.GetScreenRay(inputMousePosition.X, inputMousePosition.Y);
        }

        public Tile PickTile(IntVector2 inputMousePosition)
        {
            var ray = GetScreenRay(inputMousePosition);
            var physics = _scene.Ptr.GetComponent<PhysicsWorld>();
            physics.RaycastSingle(_raycastResult, ray, 100);
            if (_raycastResult.Body != null)
                return _raycastResult.Body.Node.GetComponent<Tile>() ??
                       _raycastResult.Body.Node.GetParentComponent<Tile>(true);
            return null;
        }

        public void DragTile(Tile tile, InteractionKey interactionKey)
        {
            _dragState.TrackTile(tile, interactionKey);
            _stateManager.State = _dragState;
        }

        public void StartPicking()
        {
            _stateManager.State = _pickState;

            var isComplete = true;
            var isCorrect = true;
            Tile hint = null;

            foreach (var tile in _currentTiles)
            {
                if (tile.LinkedTile == null) isComplete = false;
                if (tile.LinkedTile != tile.ValidLink)
                {
                    isCorrect = false;
                    if (hint == null)
                        hint = tile;
                }
            }

            if (isComplete)
            {
                if (isCorrect)
                {
                    Victory();
                }
                else
                {
                    var material = Context.ResourceCache.GetResource<Material>("Materials/Red.material");
                    foreach (var tile in _currentTiles)
                        if (tile.LinkedTile != tile.ValidLink)
                            tile.Link.GetComponent<AnimatedModel>().SetMaterial(material);

                    return;
                }
            }

            if (_hintsLeft > 0 && hint != null)
            {
                _pickState.ShowHint(hint);
                --_hintsLeft;
            }
        }

        public void NextLevel(VariantList args)
        {
            _victory = false;
            RmlUiComponent.UpdateProperties();

            ++_levelIndex;
            RmlUiComponent.UpdateProperties();
            foreach (var tile in _currentTiles) tile.Node.Remove();

            AddPairs(Math.Min(4, _levelIndex));

            StartPicking();
        }

        protected override void Dispose(bool disposing)
        {
            _scene?.Dispose();

            base.Dispose(disposing);
        }

        private void Victory()
        {
            var nodeList = new NodeList();
            _scene.Ptr.GetNodesWithTag(nodeList, "Confetti");
            foreach (var node in nodeList)
            {
                ActionManager.AddAction(_confettiAnimation, node);
            }
            _stateManager.State = _victoryState;
            _victory = true;
            RmlUiComponent.UpdateProperties();
        }

        private void Randomize(List<Node> rightTiles)
        {
            for (var index = 0; index < rightTiles.Count; index++)
            {
                var j = _random.Next(rightTiles.Count);
                (rightTiles[j], rightTiles[index]) = (rightTiles[index], rightTiles[j]);
            }
        }

        private void HandleTouchBegin(VariantMap args)
        {
            _stateManager.HandleTouchBegin(args);
        }

        private void HandleTouchEnd(VariantMap args)
        {
            _stateManager.HandleTouchEnd(args);
        }

        private void HandleTouchMove(VariantMap args)
        {
            _stateManager.HandleTouchMove(args);
        }

        private void HandleMouseMove(VariantMap args)
        {
            _stateManager.HandleMouseMove(args);
        }

        private void HandleMouseUp(VariantMap args)
        {
            _stateManager.HandleMouseUp(args);
        }

        private void HandleMouseDown(VariantMap args)
        {
            _stateManager.HandleMouseDown(args);
        }

        private void HandleDpadKeyDown(VariantMap args)
        {
            var scanCode =(Scancode)args[E.KeyUp.Scancode].Int;
            switch (scanCode)
            {
                case Scancode.ScancodeUp:
                    _stateManager.HandleDpad(HatPosition.HatUp);
                    break;
                case Scancode.ScancodeDown:
                    _stateManager.HandleDpad(HatPosition.HatDown);
                    break;
                case Scancode.ScancodeLeft:
                    _stateManager.HandleDpad(HatPosition.HatLeft);
                    break;
                case Scancode.ScancodeRight:
                    _stateManager.HandleDpad(HatPosition.HatRight);
                    break;
            }
        }

        private void HandleEscKeyUp(VariantMap args)
        {
            var key = (Key)args[E.KeyUp.Key].Int;
            switch (key)
            {
                case Key.KeyEscape:
                case Key.KeyBackspace:
                    _app.HandleBackKey();
                    return;
            }
        }
    }
}