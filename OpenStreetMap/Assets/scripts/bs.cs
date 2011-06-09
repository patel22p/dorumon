using UnityEngine;
using System.Collections;

public class bs : MonoBehaviour {

    //public static void Combine(GameObject g)
    //{
    //    var generateTriangleStrips = true;
    //    Component[] filters = g.GetComponentsInChildren(typeof(MeshFilter));
    //    Matrix4x4 myTransform = g.transform.worldToLocalMatrix;
    //    Hashtable materialToMesh = new Hashtable();

    //    for (int i = 0; i < filters.Length; i++)
    //    {
    //        MeshFilter filter = (MeshFilter)filters[i];
    //        Renderer curRenderer = filters[i].renderer;
    //        MeshCombineUtility.MeshInstance instance = new MeshCombineUtility.MeshInstance();
    //        instance.mesh = filter.sharedMesh;
    //        if (curRenderer != null && curRenderer.enabled && instance.mesh != null)
    //        {
    //            instance.transform = myTransform * filter.transform.localToWorldMatrix;

    //            Material[] materials = curRenderer.sharedMaterials;
    //            for (int m = 0; m < materials.Length; m++)
    //            {
    //                instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);

    //                ArrayList objects = (ArrayList)materialToMesh[materials[m]];
    //                if (objects != null)
    //                {
    //                    objects.Add(instance);
    //                }
    //                else
    //                {
    //                    objects = new ArrayList();
    //                    objects.Add(instance);
    //                    materialToMesh.Add(materials[m], objects);
    //                }
    //            }

    //            curRenderer.enabled = false;
    //        }
    //    }

    //    foreach (DictionaryEntry de in materialToMesh)
    //    {
    //        ArrayList elements = (ArrayList)de.Value;
    //        MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(MeshCombineUtility.MeshInstance));

    //        // We have a maximum of one material, so just attach the mesh to our own game object
    //        if (materialToMesh.Count == 1)
    //        {
    //            // Make sure we have a mesh filter & renderer
    //            if (g.GetComponent(typeof(MeshFilter)) == null)
    //                g.gameObject.AddComponent(typeof(MeshFilter));
    //            if (!g.GetComponent("MeshRenderer"))
    //                g.gameObject.AddComponent("MeshRenderer");

    //            MeshFilter filter = (MeshFilter)g.GetComponent(typeof(MeshFilter));
    //            filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
    //            g.renderer.material = (Material)de.Key;
    //            g.renderer.enabled = true;
    //        }
    //        // We have multiple materials to take care of, build one mesh / gameobject for each material
    //        // and parent it to this object
    //        else
    //        {
    //            GameObject go = new GameObject("Combined mesh");
    //            go.transform.parent = g.transform;
    //            go.transform.localScale = Vector3.one;
    //            go.transform.localRotation = Quaternion.identity;
    //            go.transform.localPosition = Vector3.zero;
    //            go.AddComponent(typeof(MeshFilter));
    //            go.AddComponent("MeshRenderer");
    //            go.renderer.material = (Material)de.Key;
    //            MeshFilter filter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
    //            filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
    //        }
    //    }
    //}
}
