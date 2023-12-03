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

        public Tile(Context context) : base(context)
        {
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