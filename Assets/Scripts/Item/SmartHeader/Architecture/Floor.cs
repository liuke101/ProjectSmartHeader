using INab.WorldScanFX;
using UnityEngine;

namespace Item.SmartHeader.Architecture
{
    /// <summary>
    /// 楼板
    /// </summary>
    [RequireComponent(typeof(ScanFXHighlight), typeof(MeshCollider), typeof(Rigidbody))]
    public class Floor : MonoBehaviour
    {
        private Rigidbody rbody;
        
        private ScanFXHighlight scanFXHighlight;
        private CustomUIHighlight customUIHighlight;
        
        
        private void Awake()
        {
            rbody = GetComponent<Rigidbody>();
            if (rbody)
            {
                rbody.isKinematic = true;
            }
            
            scanFXHighlight = GetComponent<ScanFXHighlight>();
            if (scanFXHighlight)
            {
                scanFXHighlight.renderers.Add(GetComponent<Renderer>());
            }
        
            customUIHighlight = GetComponent<CustomUIHighlight>();
        }
    }
}