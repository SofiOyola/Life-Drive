using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    // Vector UI geometry: no generated PNGs or dependencies on game assets.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FormaRedondeadaUI : MaskableGraphic
    {
        public float radio = 20;
        public bool circular;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r = rectTransform.rect;
            float rad = circular ? Mathf.Min(r.width,r.height)/2 : Mathf.Min(radio, Mathf.Min(r.width,r.height)/2);
            vh.AddVert(r.center,color,Vector2.zero);
            const int steps=16;
            for(int corner=0;corner<4;corner++)
            {
                var center=new Vector2(corner==0||corner==3?r.xMax-rad:r.xMin+rad,corner<2?r.yMax-rad:r.yMin+rad);
                for(int s=0;s<=steps;s++)
                {
                    float a=(corner*90f+s*90f/steps)*Mathf.Deg2Rad;
                    vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,color,Vector2.zero);
                }
            }
            int count=4*(steps+1);
            for(int i=1;i<=count;i++) vh.AddTriangle(0,i,i==count?1:i+1);
        }
        public override bool Raycast(Vector2 sp,Camera cam)
        {
            if(!base.Raycast(sp,cam))return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,sp,cam,out var p);
            var r=rectTransform.rect; float rad=circular?Mathf.Min(r.width,r.height)/2:Mathf.Min(radio,Mathf.Min(r.width,r.height)/2);
            var q=new Vector2(Mathf.Clamp(p.x,r.xMin+rad,r.xMax-rad),Mathf.Clamp(p.y,r.yMin+rad,r.yMax-rad));
            return (p-q).sqrMagnitude<=rad*rad;
        }
    }
}
