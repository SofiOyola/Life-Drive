using UnityEngine;
using UnityEngine.UI;
namespace LifeDrive.Mapas.InterfazVisual
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class VolantePixelUI : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float unit=Mathf.Min(r.width,r.height)/32;
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {
                float dx=x-15.5f,dy=y-15.5f,d=Mathf.Sqrt(dx*dx+dy*dy);
                bool rim=d>=10&&d<=15,hub=d<=4,spoke=(Mathf.Abs(dy)<2&&Mathf.Abs(dx)<12)||(Mathf.Abs(dx)<2&&dy<0&&dy>-12);
                if(!rim&&!hub&&!spoke)continue;
                Color c=new Color32(30,32,30,255);
                if(rim&&d>13)c=new Color32(66,69,62,255);
                if(rim&&Mathf.Abs(dx)<2&&dy>9)c=new Color32(179,65,49,255);
                int i=vh.currentVertCount;float xx=r.center.x+(x-16)*unit,yy=r.center.y+(y-16)*unit;
                vh.AddVert(new Vector3(xx,yy),c,Vector2.zero);vh.AddVert(new Vector3(xx,yy+unit),c,Vector2.zero);vh.AddVert(new Vector3(xx+unit,yy+unit),c,Vector2.zero);vh.AddVert(new Vector3(xx+unit,yy),c,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
            }
        }
    }
}
