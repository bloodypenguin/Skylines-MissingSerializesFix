
using ICities;
using LoadingExtension;

namespace MissingSerializersFix
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            PackageHelperDetour.Init();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            PackageHelperDetour.Revert();
        }
    }
}
