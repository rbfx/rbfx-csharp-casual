using Urho3DNet;

namespace RbfxTemplate
{
    [ObjectFactory(Category = "Component/Game")]
    [Preserve(AllMembers = true)]
    public sealed class Tile : Component
    {
        [Urho3DNet.SerializeField(Mode = AttributeMode.AmDefault | AttributeMode.AmNodeid, Name = "Link")]
        private int _linkId;

        [Urho3DNet.SerializeField(Mode = AttributeMode.AmDefault | AttributeMode.AmNodeid, Name = "LinkSource")]
        private int _linkSource;

        [Urho3DNet.SerializeField(Mode = AttributeMode.AmDefault | AttributeMode.AmNodeid, Name = "LinkTarget")]
        private int _linkTarget;

        public Node Link { get; set; }

        public Node LinkSource { get; set; }

        public Node LinkTarget { get; set; }

        public Tile ValidLink { get; set; }

        public Tile LinkedTile { get; set; }

        public Tile(Context context) : base(context)
        {
        }

        public void LinkTo(Tile tile)
        {
            Link.IsEnabled = false;

            if (tile == LinkedTile)
                return;

            {
                var linkedTile = LinkedTile;
                LinkedTile = null;
                if (linkedTile != null)
                {
                    linkedTile.LinkTo(null);
                }
            }

            if (tile != null)
            {
                LinkedTile = tile;
                Link.IsEnabled = true;
                LinkedTile.LinkTo(this);

                var targetPos = tile.Link.WorldPosition;
                LinkTarget.WorldPosition = targetPos;
                LinkSource.LookAt(targetPos);
                LinkTarget.LookAt(Node.WorldPosition);
            }
        }

        public override void ApplyAttributes()
        {
            Link = Scene.GetNode((uint)_linkId);
            LinkSource = Scene.GetNode((uint)_linkSource);
            LinkTarget = Scene.GetNode((uint)_linkTarget);
            base.ApplyAttributes();
        }
    }
}