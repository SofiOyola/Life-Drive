using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    // Editable, layered 48x64 pixel portrait. Presentation concept, not a content ID.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AvatarPersonalizableUI : MaskableGraphic
    {
        public bool mujer;
        public Color piel=new Color32(193,133,91,255),ojos=new Color32(68,85,55,255),cabello=new Color32(49,35,30,255),traje=new Color32(104,119,57,255);
        VertexHelper mesh;
        static readonly Color Tinta=new Color32(32,40,30,255), Claro=new Color32(242,231,191,255);
        public void Actualizar(){SetVerticesDirty();}
        void Pixel(float x,float y,float w,float h,Color c)
        {
            var r=rectTransform.rect;float u=Mathf.Min(r.width/48,r.height/64);
            float ox=r.center.x-24*u,oy=r.center.y+32*u;int i=mesh.currentVertCount;
            mesh.AddVert(new Vector3(ox+x*u,oy-y*u),c,Vector2.zero);mesh.AddVert(new Vector3(ox+(x+w)*u,oy-y*u),c,Vector2.zero);
            mesh.AddVert(new Vector3(ox+(x+w)*u,oy-(y+h)*u),c,Vector2.zero);mesh.AddVert(new Vector3(ox+x*u,oy-(y+h)*u),c,Vector2.zero);
            mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();mesh=vh;
            if(mujer){Pixel(11,9,27,32,Tinta);Pixel(13,8,23,34,cabello);Pixel(35,20,5,18,cabello);}
            Pixel(9,40,30,22,Tinta);Pixel(5,45,38,17,Tinta);Pixel(7,46,34,16,traje*.78f);Pixel(11,42,26,20,traje);
            Pixel(13,42,3,20,traje*1.15f);Pixel(34,45,3,17,traje*.72f);Pixel(8,60,32,2,Tinta);
            Pixel(19,32,11,11,piel*.78f);Pixel(20,32,9,9,piel);
            Pixel(15,40,5,5,Claro);Pixel(29,40,5,5,Claro);Pixel(19,44,3,4,Claro);Pixel(27,44,3,4,Claro);
            Pixel(23,45,3,17,Tinta);Pixel(24,47,1,13,Claro);Pixel(29,50,5,5,traje*.7f);Pixel(29,50,5,1,Claro);
            Pixel(14,12,21,22,Tinta);Pixel(12,21,25,7,piel*.85f);Pixel(16,13,17,22,piel);Pixel(17,14,5,15,piel*1.12f);
            Pixel(18,35,13,2,Tinta);Pixel(15,10,20,9,cabello);Pixel(18,7,14,4,cabello);Pixel(13,14,4,9,cabello);Pixel(32,14,3,9,cabello);
            Pixel(17,11,13,2,cabello*1.35f);Pixel(17,18,5,2,cabello);Pixel(27,18,5,2,cabello);
            Pixel(17,22,5,3,Claro);Pixel(27,22,5,3,Claro);Pixel(19,22,3,3,ojos);Pixel(27,22,3,3,ojos);
            Pixel(20,22,1,2,Tinta);Pixel(27,22,1,2,Tinta);Pixel(24,23,2,6,piel*.75f);Pixel(24,29,3,1,piel*.65f);
            Pixel(21,32,7,1,Tinta);Pixel(22,33,5,1,piel*1.12f);
            if(mujer){Pixel(15,16,3,6,cabello);Pixel(30,13,4,7,cabello);Pixel(15,28,1,2,Claro);}
            else{Pixel(17,33,2,2,piel*.72f);Pixel(30,32,2,3,piel*.72f);}
        }
    }
}
