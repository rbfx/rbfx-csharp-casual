using Urho3DNet;

namespace RbfxTemplate
{
    [ObjectFactory(Category = "Component/Game")]
    [Preserve(AllMembers = true)]
    public sealed partial class Tile : Component
    {
        [SerializeField(Mode = AttributeMode.AmDefault | AttributeMode.AmNodeid, Name = "Link")]
        private int _linkId;

        [SerializeField(Mode = AttributeMode.AmDefault | AttributeMode.AmNodeid, Name = "LinkSource")]
        private int _linkSource;

        [SerializeField(Mode = AttributeMode.AmDefault | AttributeMode.AmNodeid, Name = "LinkTarget")]
        private int _linkTarget;

        public Tile(Context context) : base(context)
        {
        }

        public Node Link { get; set; }

        public Node LinkSource { get; set; }

        public Node LinkTarget { get; set; }

        public Tile ValidLink { get; set; }

        public Tile LinkedTile { get; set; }

        public override void ApplyAttributes()
        {
            Link = Scene.GetNode((uint)_linkId);
            LinkSource = Scene.GetNode((uint)_linkSource);
            LinkTarget = Scene.GetNode((uint)_linkTarget);
            base.ApplyAttributes();
        }

        public void LinkTo(Tile tile)
        {
            Link.IsEnabled = false;

            if (tile == LinkedTile)
                return;

            {
                var linkedTile = LinkedTile;
                LinkedTile = null;
                if (linkedTile != null) linkedTile.LinkTo(null);
            }

            if (tile != null)
            {
                var material = Context.ResourceCache.GetResource<Material>("Materials/White.material");
                tile.Link.GetComponent<AnimatedModel>().SetMaterial(material);
                LinkedTile = tile;
                Link.IsEnabled = true;
                LinkedTile.LinkTo(this);

                var targetPos = tile.Link.WorldPosition;
                LinkTarget.WorldPosition = targetPos;
                LinkSource.LookAt(targetPos);
                LinkTarget.LookAt(Node.WorldPosition);
            }
        }
    }
}